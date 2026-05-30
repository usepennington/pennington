#!/bin/bash
# SessionStart hook: install the .NET 11 preview SDK pinned by global.json and
# warm the NuGet cache so builds/tests work in Claude Code on the web sessions.
#
# The container filesystem (including $HOME) is cached after this hook completes,
# so a successful run is paid for once and reused by future cached sessions.
set -euo pipefail

# Only run in the remote (Claude Code on the web) environment. Locally, developers
# manage their own SDK install.
if [ "${CLAUDE_CODE_REMOTE:-}" != "true" ]; then
  exit 0
fi

PROJECT_DIR="${CLAUDE_PROJECT_DIR:-$(pwd)}"
DOTNET_DIR="$HOME/.dotnet"
INSTALL_SCRIPT="$(mktemp /tmp/dotnet-install.XXXXXX.sh)"

log() { echo "[session-start] $*"; }

# Persist the SDK on PATH for every session (runs on startup/resume/clear/compact).
if [ -n "${CLAUDE_ENV_FILE:-}" ]; then
  {
    echo "export DOTNET_ROOT=\"$DOTNET_DIR\""
    echo "export PATH=\"$DOTNET_DIR:\$PATH\""
    echo "export DOTNET_CLI_TELEMETRY_OPTOUT=1"
    echo "export DOTNET_NOLOGO=1"
  } >> "$CLAUDE_ENV_FILE"
fi
export DOTNET_ROOT="$DOTNET_DIR"
export PATH="$DOTNET_DIR:$PATH"

# Idempotent: skip the download if the pinned SDK is already in the cached container.
if [ -x "$DOTNET_DIR/dotnet" ] && \
   "$DOTNET_DIR/dotnet" --list-sdks 2>/dev/null | grep -q "11.0.100-preview"; then
  log ".NET 11 preview SDK already present:"
  "$DOTNET_DIR/dotnet" --list-sdks
else
  log "Installing the .NET SDK pinned in global.json ..."
  # dot.net / builds.dotnet.microsoft.com redirect through hosts that may not be on
  # the network allowlist; raw.githubusercontent.com is reliably reachable for the script itself.
  curl -fsSL https://raw.githubusercontent.com/dotnet/install-scripts/main/src/dotnet-install.sh \
    -o "$INSTALL_SCRIPT"
  chmod +x "$INSTALL_SCRIPT"

  # --jsonfile keeps the installed SDK exactly in sync with global.json.
  if ! "$INSTALL_SCRIPT" --jsonfile "$PROJECT_DIR/global.json" --install-dir "$DOTNET_DIR" --no-path; then
    log "ERROR: SDK download failed."
    log "The .NET SDK binaries are served from builds.dotnet.microsoft.com (primary)"
    log "and ci.dot.net (fallback). If this run failed with a network/allowlist error,"
    log "add those two hosts to this environment's Custom allowed domains and start a new session."
    rm -f "$INSTALL_SCRIPT"
    exit 1
  fi
  rm -f "$INSTALL_SCRIPT"
  log "Installed:"
  "$DOTNET_DIR/dotnet" --list-sdks
fi

# Warm the NuGet cache (nuget.org is allowlisted) so cached builds/tests are fast.
log "Restoring NuGet packages ..."
"$DOTNET_DIR/dotnet" restore "$PROJECT_DIR/Pennington.slnx"

log "Done."
