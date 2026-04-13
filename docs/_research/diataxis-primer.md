# Diataxis Primer for Pennington Documentation Subagents

Pennington's docs site is organized as four top-level folders under `Content/` — `tutorials/`, `how-to/`, `reference/`, `explanation/` — which map one-to-one onto Diataxis quadrants. A page that drifts across quadrants weakens both the page it belongs on and the one it leaks into. Before drafting, decide the quadrant, then keep the page pure. If an outline starts to cross boundaries, split it and cross-link.

## The four quadrants

**Tutorials** are learning-oriented. The user arrives wanting to *learn* — often a beginner, often without a concrete task — and success is the user finishing the lesson with a working artifact and the feeling "I can do this." The *author* chooses the topic and holds the user's hand on a single guaranteed-to-work path. Tone is patient, encouraging, first-person-plural ("we'll now add..."). Nothing must fail; nothing must branch. The learner's attention is the only resource you have, and you spend it on muscle memory, not decisions.

**How-to guides** are task-oriented. The user arrives with a *specific real-world goal already in mind* ("enable Roslyn highlighting for a blog site") and needs a direct recipe. The *user* chose the topic; they searched for it. Success is that the user's problem is solved. Tone is terse, imperative, competent-peer. Assumes working knowledge. A how-to is a recipe, not a lesson — it does not explain, it does not teach concepts, it does not start from zero. It lists the steps to reach a realistic end state and verifies it.

**Reference** is information-oriented. The user arrives wanting to *look something up* — an option name, a method signature, a config key, a route. Success is finding the fact and leaving. The *structure of the product* dictates the structure of the reference; one page per coherent unit (one options class, one keyspace, one interface family). Tone is neutral, descriptive, consistent, terse. No narrative, no rationale, no "when to use," no tutorials embedded inside. Reference pages are shaped like dictionaries and tables; prose between entries is a smell.

**Explanation** is understanding-oriented. The user arrives wanting to *understand why* — why the architecture is this way, what the tradeoffs were, how this concept relates to that one. The *author* chooses the topic. Success is the reader closing the tab with a clearer mental model. Tone is discursive, reflective, sometimes opinionated; "consider," "because," "in contrast to." Explanation does not list API members (that is reference) and does not prescribe steps (that is how-to). It connects ideas.

## Classification rubric — run before you outline

Ask these in order; the first "yes" wins:

1. Does the user arrive with a specific concrete goal already in mind? -> **how-to**
2. Does the user arrive wanting to learn from scratch, with no prior goal? -> **tutorial**
3. Does the user arrive needing to look up a fact or signature? -> **reference**
4. Does the user arrive wanting to understand *why* something is the way it is? -> **explanation**

Secondary disambiguators when the first pass feels ambiguous:

- Who picks the topic — author or user? Author-picked => tutorial or explanation. User-picked => how-to or reference.
- Does the page need to work end-to-end, step by step, with a feel-good payoff? => tutorial.
- Can the page be read in any order / is it lookup-shaped? => reference.
- Is the page mostly prose about ideas, with no runnable commands? => explanation.

If two quadrants still feel plausible, the page is probably two pages. Split.

## Per-quadrant self-checks

Run these against your *outline* before writing prose. Each question is phrased so that "No" means the outline is wrong and you must fix it.

### Tutorial self-check
- Are the steps numbered and strictly linear, with no "if your situation is X, go here" branches?
- Have I removed every "why we do this" tangent and pushed rationale to an explanation page?
- Is there a single guaranteed endpoint where the learner can say "I did it"?
- Is the whole thing completable in 30–60 minutes by a beginner?
- Does every step succeed deterministically — no "this may fail, try..." escape hatches?
- Does the tutorial produce a concrete, visible artifact the user can see working?
- Am I the teacher choosing the topic (not the user searching for a task)?

### How-to self-check
- Is my outline 7 steps or fewer, each a single imperative action?
- Does the page start from a realistic prerequisite state (installed, configured), not from zero?
- Have I resisted teaching concepts — no "first, let's understand..." preamble?
- Is there a terse **Verify** section at the end (one command or one observable outcome)?
- Is the goal in the title phrased as a task ("Add X", "Enable Y", "Publish Z")?
- Does the page solve exactly one problem and stop?
- Have I linked out to reference/explanation for background rather than inlining it?

### Reference self-check
- Is every entry lookup-shaped — one symbol, one table row, one option, one keyspace?
- Have I removed all "when to use" rationale, recommendations, and narrative?
- Have I avoided numbered procedural steps (those belong in how-to)?
- Is the structure dictated by the product's structure (not my sense of what flows nicely)?
- Is the tone uniformly neutral and terse across all entries?
- Can the reader land on any heading and get value without reading above it?
- Is there at most one coherent unit on this page (one options class, one interface family)?

### Explanation self-check
- Does the page answer a *why* or *how-does-this-fit* question, not a *how-do-I* question?
- Have I resisted listing API members? (If I'm enumerating a type's surface, that's reference.)
- Have I resisted step-by-step instructions? (If the reader is meant to follow along, that's how-to/tutorial.)
- Is the word count between 500 and 1,500?
- Is there exactly one concept under discussion, with related ones merely referenced?
- Does the page connect to the reader's existing mental model (contrasts, analogies, history)?
- Would a reader who *already knows how* still learn something from reading this?

## Top anti-patterns per quadrant

**Tutorials**
- Branching flowcharts: "if on Windows, do X; if on Linux, do Y" — pick one and commit.
- Explaining the framework in between steps instead of just having the learner type.
- "Exercises for the reader" or "try changing this" — tutorials are guided, not open-ended.
- Ending without a visible artifact — no reward, no completion signal.
- Assuming any prior knowledge beyond the stated starting point.

**How-to guides**
- Opening with "First, let's understand how the pipeline works..." — that's explanation.
- Starting from `dotnet new` when the realistic user already has a project.
- Padding with background, history, or design rationale — cut it or link out.
- Missing a verification step — the user can't tell if they succeeded.
- Covering multiple loosely related tasks on one page ("Configure search and also add a sitemap").

**Reference**
- Prose paragraphs between tables explaining "why you'd want to use this."
- Numbered procedures embedded in an options table.
- Tutorial-style walkthroughs under a `## Example` that run for a page.
- Inconsistent entry shape — some entries have "Remarks," others don't, ordering is ad hoc.
- Covering two unrelated types on one page for narrative convenience.

**Explanation**
- A tutorial in disguise — numbered steps dressed up as "let's explore."
- A reference dump — a table of every option with one sentence each.
- No thesis: the page wanders across topics without answering a specific *why*.
- Prescribing what the reader *should* do — slips into how-to.
- Going past ~1,500 words — split into two focused explanations.

## Granularity rules

- **One user need, one sitting.** Every page serves a single need a single user has at a single moment.
- **Tutorial duration**: 30–60 minutes, beginner pace. Longer means you're teaching too much; shorter means it's probably a how-to.
- **How-to**: one goal, one page, ideally under 7 steps. Multiple goals means multiple pages.
- **Reference**: one coherent unit per page — one options class, one keyspace, one interface group, one CLI command family. Not "all options everywhere."
- **Explanation**: 500–1,500 words, single concept. If two concepts need contrasting, the contrast itself is the single concept.
- **When a page wants to grow past these limits, split it.** Cross-link instead of nesting. A how-to that needs a concept should link to an explanation page, not grow one inside.
- **Cross-quadrant linking is the glue.** Tutorials link forward to how-to and reference. How-to links to reference for signatures and to explanation for rationale. Reference links to explanation for conceptual framing. Explanation links to all three.

## Quick-reference decision table

| Quadrant    | User arrives with                       | Page answers                        | Success looks like                                       |
|-------------|-----------------------------------------|-------------------------------------|----------------------------------------------------------|
| Tutorial    | Desire to learn, no specific goal       | "Follow along and you'll build X."  | Learner finishes with working artifact and confidence.   |
| How-to      | A concrete real-world task in mind      | "Here are the steps to do X."       | User's task is done; they close the tab.                 |
| Reference   | A specific fact to look up              | "X is defined as ...; options are..." | User finds the fact in under a minute and leaves.      |
| Explanation | A *why* or *how-does-this-fit* question | "X exists because...; it relates to Y by..." | Reader leaves with a clearer mental model.      |
