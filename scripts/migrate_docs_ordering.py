"""Migrate docs/Pennington.Docs/Content to folder-local 1,2,3 ordering.

For each folder under the configured Content root (excluding the blog/ area
which has its own content service):

- Compute each child's "effective order" using the original NavigationBuilder
  rule: a .md file's front-matter `order:` (or int.MaxValue if missing), a
  subfolder's min(children.effective_order) propagated recursively. Sort by
  (effective_order, title) — the original comparator.
- Reassign new orders 1, 2, 3, ... to each child in that sorted order.
- For .md files: rewrite the front-matter `order:` value (no other fields
  change).
- For subfolders: write `order: N` into a sibling `_meta.yml`. Preserve any
  existing keys (notably `title:` and the `llms:` block).

Then verify: compute the post-migration tree using the new NavigationBuilder
rule (sidecar order wins; fall back to min(children) where no sidecar is
present; index.md page is the folder node) and assert that the depth-first
flatten of the rendered tree is identical, leaf-for-leaf, to the pre-migration
flatten. The script exits non-zero if any area's ordering shifts.

Usage:
    python scripts/migrate_docs_ordering.py [--dry-run]
"""

from __future__ import annotations

import argparse
import os
import re
import sys
from dataclasses import dataclass, field
from pathlib import Path
from typing import Optional

CONTENT_ROOT = Path("B:/Penn/docs/Pennington.Docs/Content")
SKIP_TOP_DIRS = {"blog", "fonts"}  # blog has its own content service; fonts is assets


# ---------------------------------------------------------------------------
# Front-matter parsing
# ---------------------------------------------------------------------------

FRONT_MATTER_RE = re.compile(r"^---\r?\n(.*?)\r?\n---\r?\n", re.DOTALL)


def read_front_matter(path: Path) -> tuple[Optional[str], str]:
    """Return (front_matter_block, body). front_matter_block is None when missing."""
    text = path.read_text(encoding="utf-8")
    m = FRONT_MATTER_RE.match(text)
    if not m:
        return None, text
    return m.group(1), text[m.end():]


def parse_yaml_scalar(line: str) -> Optional[tuple[str, str]]:
    """Parse `key: value` at the top level of a YAML doc (no nesting awareness)."""
    m = re.match(r"^([A-Za-z_][\w-]*)\s*:\s*(.*)$", line)
    if not m:
        return None
    return m.group(1), m.group(2).strip()


def yaml_get_order(block: Optional[str]) -> Optional[int]:
    if block is None:
        return None
    for raw in block.splitlines():
        if raw.startswith(" "):  # nested key, skip
            continue
        parsed = parse_yaml_scalar(raw)
        if parsed and parsed[0] == "order":
            try:
                return int(parsed[1])
            except ValueError:
                return None
    return None


def yaml_get_title(block: Optional[str]) -> Optional[str]:
    if block is None:
        return None
    for raw in block.splitlines():
        if raw.startswith(" "):
            continue
        parsed = parse_yaml_scalar(raw)
        if parsed and parsed[0] == "title":
            value = parsed[1]
            # Strip simple quotes if present.
            if (value.startswith('"') and value.endswith('"')) or (
                value.startswith("'") and value.endswith("'")
            ):
                value = value[1:-1]
            return value
    return None


def rewrite_order_in_block(block: str, new_order: int) -> str:
    """Replace the `order:` line in a front-matter block, or append one."""
    lines = block.splitlines()
    new_lines = []
    found = False
    for raw in lines:
        if raw.startswith(" "):
            new_lines.append(raw)
            continue
        parsed = parse_yaml_scalar(raw)
        if parsed and parsed[0] == "order":
            new_lines.append(f"order: {new_order}")
            found = True
        else:
            new_lines.append(raw)
    if not found:
        new_lines.append(f"order: {new_order}")
    return "\n".join(new_lines)


def write_front_matter(path: Path, block: str, body: str) -> None:
    """Write a markdown file with the given front-matter block (no `---` delims) and body."""
    text = f"---\n{block}\n---\n{body}"
    # Preserve original line endings: detect from existing file.
    original = path.read_bytes()
    if b"\r\n" in original:
        text = text.replace("\n", "\r\n")
    path.write_bytes(text.encode("utf-8"))


# ---------------------------------------------------------------------------
# Content model
# ---------------------------------------------------------------------------

@dataclass
class FileNode:
    path: Path
    canonical: str  # /a/b/c (no trailing slash)
    title: str
    current_order: int  # int.MaxValue when unset
    new_order: int = 0

    @property
    def is_index(self) -> bool:
        return self.path.name.lower() == "index.md"

    def sort_key(self) -> tuple[int, str]:
        return (self.current_order, self.title.casefold())


@dataclass
class FolderNode:
    path: Path
    name: str  # folder slug
    canonical: str  # /a/b/c/ (with trailing slash)
    files: list[FileNode] = field(default_factory=list)
    subfolders: list["FolderNode"] = field(default_factory=list)
    # Sidecar state: existing _meta.yml contents (None if no file present).
    sidecar_path: Optional[Path] = None
    sidecar_existing_text: Optional[str] = None
    sidecar_existing_order: Optional[int] = None
    sidecar_existing_title: Optional[str] = None
    new_order: int = 0

    def all_files_recursive(self) -> list[FileNode]:
        out = list(self.files)
        for sf in self.subfolders:
            out.extend(sf.all_files_recursive())
        return out

    def effective_current_order(self) -> int:
        """Mirror BuildLevel today: min(child.current_order) including subfolders' effective orders."""
        # When the folder has an index.md, its own order is the folder's order today.
        index = next((f for f in self.files if f.is_index), None)
        if index is not None:
            return index.current_order
        candidates = [f.current_order for f in self.files] + [
            sf.effective_current_order() for sf in self.subfolders
        ]
        return min(candidates) if candidates else 2**31 - 1

    def title_for_sort(self) -> str:
        index = next((f for f in self.files if f.is_index), None)
        if index is not None:
            return index.title.casefold()
        # FormatSectionTitle equivalent (lower-cased for sort): kebab -> title case.
        return " ".join(part.capitalize() for part in self.name.split("-")).casefold()

    def sort_key(self) -> tuple[int, str]:
        return (self.effective_current_order(), self.title_for_sort())


# ---------------------------------------------------------------------------
# Discovery
# ---------------------------------------------------------------------------

INT_MAX = 2**31 - 1


def discover_folder(path: Path, base: Path, base_url: str) -> FolderNode:
    rel = path.relative_to(base).as_posix()
    canonical = f"{base_url}{rel}/" if rel != "." else base_url
    folder = FolderNode(
        path=path,
        name=path.name if rel != "." else "",
        canonical=canonical,
    )

    sidecar = path / "_meta.yml"
    if sidecar.exists():
        folder.sidecar_path = sidecar
        folder.sidecar_existing_text = sidecar.read_text(encoding="utf-8")
        folder.sidecar_existing_order = yaml_get_order(folder.sidecar_existing_text)
        folder.sidecar_existing_title = yaml_get_title(folder.sidecar_existing_text)

    for entry in sorted(path.iterdir()):
        if entry.name in SKIP_TOP_DIRS and path == base:
            continue
        if entry.is_dir():
            folder.subfolders.append(discover_folder(entry, base, base_url))
        elif entry.is_file() and entry.suffix == ".md" and not entry.name.endswith(".llms.md"):
            fm, _ = read_front_matter(entry)
            order = yaml_get_order(fm)
            title = yaml_get_title(fm) or entry.stem
            rel_file = entry.relative_to(base).with_suffix("").as_posix()
            canonical_file = f"{base_url}{rel_file}"
            # An index.md represents the folder; its canonical is the folder URL.
            if entry.name.lower() == "index.md":
                canonical_file = canonical
            folder.files.append(FileNode(
                path=entry,
                canonical=canonical_file,
                title=title,
                current_order=order if order is not None else INT_MAX,
            ))

    return folder


# ---------------------------------------------------------------------------
# Assigning new orders
# ---------------------------------------------------------------------------

def assign_new_orders(folder: FolderNode) -> None:
    """Sort children by current effective order, assign 1..N as new_order."""
    # Combine files and subfolders into one sequence at this level.
    items: list[tuple[tuple[int, str], object]] = []
    for f in folder.files:
        items.append((f.sort_key(), f))
    for sf in folder.subfolders:
        items.append((sf.sort_key(), sf))
    items.sort(key=lambda x: x[0])

    for i, (_, item) in enumerate(items, start=1):
        if isinstance(item, FileNode):
            item.new_order = i
        else:
            item.new_order = i

    for sf in folder.subfolders:
        assign_new_orders(sf)


# ---------------------------------------------------------------------------
# Simulated NavigationBuilder ordering (depth-first leaves)
# ---------------------------------------------------------------------------

def flatten_before(folder: FolderNode) -> list[str]:
    """Depth-first list of canonical paths, using CURRENT orders."""
    # Mirror NavigationBuilder.BuildLevel for the "before" state.
    # Children at this level: files (excluding index.md if it's the folder's overview)
    # and subfolders. Subfolders sort by emergent min(children).
    items: list[tuple[tuple[int, str], object]] = []
    index_file = next((f for f in folder.files if f.is_index), None)
    for f in folder.files:
        if f is index_file:
            continue  # index represents the folder itself
        items.append((f.sort_key(), f))
    for sf in folder.subfolders:
        items.append((sf.sort_key(), sf))
    items.sort(key=lambda x: x[0])

    result: list[str] = []
    if index_file is not None:
        result.append(index_file.canonical)
    for _, item in items:
        if isinstance(item, FileNode):
            result.append(item.canonical)
        else:
            result.extend(flatten_before(item))
    return result


def flatten_after(folder: FolderNode) -> list[str]:
    """Depth-first list of canonical paths, using POST-migration orders.

    - Files use their new_order.
    - Subfolders use their sidecar order (new_order assigned by this script).
    - Folders without sidecar (root folder of the area pass) fall back to
      min(children). We always write a sidecar for non-root folders, so this
      only matters at the area root.
    """
    items: list[tuple[tuple[int, str], object]] = []
    index_file = next((f for f in folder.files if f.is_index), None)
    for f in folder.files:
        if f is index_file:
            continue
        items.append(((f.new_order, f.title.casefold()), f))
    for sf in folder.subfolders:
        # Subfolder's title-for-sort: if its index.md sets a title that's what
        # the new tree uses for tie-breaking (no sidecar title overrides).
        items.append(((sf.new_order, sf.title_for_sort()), sf))
    items.sort(key=lambda x: x[0])

    result: list[str] = []
    if index_file is not None:
        result.append(index_file.canonical)
    for _, item in items:
        if isinstance(item, FileNode):
            result.append(item.canonical)
        else:
            result.extend(flatten_after(item))
    return result


# ---------------------------------------------------------------------------
# Writing
# ---------------------------------------------------------------------------

def write_file_order(file: FileNode) -> None:
    fm, body = read_front_matter(file.path)
    if fm is None:
        # Shouldn't happen — every .md in docs has front matter — but bail safely.
        return
    new_block = rewrite_order_in_block(fm, file.new_order)
    if new_block.strip() == fm.strip():
        return
    write_front_matter(file.path, new_block, body)


def write_sidecar(folder: FolderNode) -> None:
    """Write the folder's _meta.yml with order, preserving title/llms when present."""
    path = folder.path / "_meta.yml"
    existing = folder.sidecar_existing_text or ""

    if not existing.strip():
        # Fresh sidecar — order only.
        path.write_text(f"order: {folder.new_order}\n", encoding="utf-8")
        return

    # Update or append order in the existing text.
    lines = existing.splitlines()
    new_lines: list[str] = []
    found = False
    for raw in lines:
        # Don't touch nested-block lines (they belong to llms: etc.).
        if raw.startswith(" ") or raw.startswith("\t"):
            new_lines.append(raw)
            continue
        parsed = parse_yaml_scalar(raw)
        if parsed and parsed[0] == "order":
            new_lines.append(f"order: {folder.new_order}")
            found = True
        else:
            new_lines.append(raw)
    if not found:
        # Insert order right after title if it exists; otherwise prepend.
        insert_idx = 0
        for i, raw in enumerate(new_lines):
            parsed = parse_yaml_scalar(raw) if not raw.startswith(" ") else None
            if parsed and parsed[0] == "title":
                insert_idx = i + 1
                break
        new_lines.insert(insert_idx, f"order: {folder.new_order}")

    out = "\n".join(new_lines).rstrip() + "\n"
    path.write_text(out, encoding="utf-8")


# ---------------------------------------------------------------------------
# Driver
# ---------------------------------------------------------------------------

def gather_areas() -> list[FolderNode]:
    """Each top-level subfolder of Content/ (except blog/, fonts/) is its own area."""
    out: list[FolderNode] = []
    for entry in sorted(CONTENT_ROOT.iterdir()):
        if not entry.is_dir() or entry.name in SKIP_TOP_DIRS:
            continue
        # area base_url is "/{area}/", canonical paths are full URLs from site root.
        out.append(discover_folder(entry, CONTENT_ROOT, "/"))
    return out


def write_everything(area: FolderNode) -> None:
    """Walk an area, write file orders and subfolder sidecars."""
    for f in area.files:
        write_file_order(f)
    for sf in area.subfolders:
        write_sidecar(sf)
        write_everything(sf)


def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("--dry-run", action="store_true",
                        help="Compute and verify but do not write any files")
    args = parser.parse_args()

    if not CONTENT_ROOT.exists():
        print(f"Content root not found: {CONTENT_ROOT}", file=sys.stderr)
        return 1

    areas = gather_areas()

    # 1. Snapshot the pre-migration depth-first flatten per area.
    before = {area.name: flatten_before(area) for area in areas}

    # 2. Assign new orders.
    for area in areas:
        assign_new_orders(area)

    # 3. Snapshot the post-migration depth-first flatten per area.
    after = {area.name: flatten_after(area) for area in areas}

    # 4. Verify: every area's ordering must be identical.
    ok = True
    for area in areas:
        b = before[area.name]
        a = after[area.name]
        if b != a:
            ok = False
            print(f"!! Ordering changed in area '{area.name}':", file=sys.stderr)
            for i, (bi, ai) in enumerate(zip(b, a)):
                if bi != ai:
                    print(f"  [{i}] before={bi}  after={ai}", file=sys.stderr)
            if len(b) != len(a):
                print(f"  (length changed: {len(b)} -> {len(a)})", file=sys.stderr)

    if not ok:
        print("Verification failed — refusing to write.", file=sys.stderr)
        return 1

    print(f"Verified {len(areas)} areas: ordering unchanged.")
    for area in areas:
        leaf_count = len([x for x in area.all_files_recursive()])
        sub_count = sum(1 for _ in walk_folders(area))
        print(f"  /{area.name}/  {leaf_count:>3} files  {sub_count:>2} subfolders")

    if args.dry_run:
        print("Dry run — no files written.")
        return 0

    # 5. Write.
    for area in areas:
        # The area root itself doesn't need a _meta.yml — areas are independent
        # sidebars driven by ContentArea config. Only its descendants do.
        write_everything(area)

    print("Migration complete.")
    return 0


def walk_folders(folder: FolderNode):
    for sf in folder.subfolders:
        yield sf
        yield from walk_folders(sf)


if __name__ == "__main__":
    sys.exit(main())
