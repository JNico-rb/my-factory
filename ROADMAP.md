# Roadmap

What the factory could add next, kept **project-agnostic**: each item is a piece every harness-first project would reuse, with the project-specific part left as a gap. Ordered by value over cost. Each names where it comes from, so the reason survives the idea.

Rule for promoting an item: it ships once a real project has used it, and it ships with its own check in `scripts/verify.sh`.

## Next — high value, low cost

### 1. Validator registry
A table in the template's `docs/verification.md`: **name · kind · execution point · where its result goes**. Kind is programmatic, semantic (LLM-as-judge, human), formal over the product's data (e.g. Lean), or formal over the harness (e.g. TLA+). Execution point is a hook, a role (editor, critic), or a gate before publishing. A validator with no execution point is a wish, not a validator.
*Why:* the course's central claim: agents are non-deterministic, validators make the outcome trustworthy, and each one must be named and placed. *Where:* `template/docs/verification.md`, and a step in the `verification` skill.

### 2. Iteration log and red-team log
Two tables, cause → change → measured effect: one row per eval, counterexample or audit finding that changed code or prompts; one row per adversarial case, with the validator that caught it (or didn't) and the fix. A skill `/sdd:log-decision "<trigger>: <change and why>"` appends a row and commits.
*Why:* process documentation is graded on the reasoning, not the result; a log of decisions with cause and effect is that reasoning, and a diary is not. *Where:* `template/docs/verification.md`, `skills/log-decision/`.

### 3. Loop specification
A skill that writes the five fields of an improvement loop before it runs: **trigger · goal · verify · stop · memory**, with stop as *metric reached* or *N iterations*.
*Why:* an eval-tuning loop without a written stop condition is the fastest way to spend tokens for nothing ([class notes §3](docs/class-notes.md#3-agentic-loops)). *Where:* `skills/loop-spec/`.

### 4. Observability and browser MCP starters
A `.mcp.json` in the template with the Langfuse MCP (auth from an environment variable) and the Playwright MCP, so an agent can read its own traces and open the UI it built to check it visually. A skill that turns the latest trace into a Markdown report.
*Why:* tracing and visual inspection are demonstrations (class D) the agent can run by itself. *Where:* `template/dot-mcp.json`, `skills/trace-report/`.

## Later — high value, more work

### 5. Formal starters
- **TLA+** — a generic harness state machine (configure → plan → write → validate → retry ≤ N → publish, resume from checkpoint), with three safety invariants (nothing unvalidated is published, a resume neither duplicates nor loses a unit, retries never exceed the limit), one liveness property, a small `.cfg` and a `verify.sh` for TLC in CI.
- **Lean 4** — a starter library of event-order and exclusion invariants, fed by a generated facts file.

*Why:* model checking catches the harness bugs no test enumerates; the story-maker project shows both working in CI. The [validators landscape](docs/links.md#validators) lists the alternatives (P, Quint, Alloy, Dafny). *Where:* `template/tla/`, `template/lean/`, each behind a grill question: most projects need neither.

### 6. Evals scaffold
`evals/` with a case format, at least one adversarial case (injection in free text) and one designed to break an invariant; a results table *case × validator*; a before/after section for each tuning iteration; an LLM-as-judge rubric and a human review with the same rubric, to compare the two.
*Why:* without evals that produce numbers, an agent's quality is a belief; with them, every prompt change is an experiment. *Where:* `template/evals/`.

### 7. Committed memory mirror
`.claude/memory/` with a README: the live memory stays in the user profile; this copy is sanitised (no names, session ids or user paths) and synced by hand at the end of a session.
*Why:* makes the agent's working memory reviewable and portable without leaking personal data. *Where:* `template/dot-claude/memory/`.

### 8. Anti-false-positive gates for the verifier
Vendor `review-verification-protocol` (Apache-2.0): every finding quotes the file and line read in the same turn, "unused" needs a workspace-wide search, severity is calibrated.
*Why:* a verifier that invents findings erodes trust as fast as one that misses them. *Where:* `skills/review-verification-protocol/`, loaded by `agents/verifier.md`.

## Maybe — only when a project needs it

### 9. Parallel lanes
One git worktree per lane, an integrator session that owns `docs/` and merges `--no-ff` only after the verifier's PASS and a green full suite, `/orchestrate`, `/lane <X>`, `/status`.
*Why:* it paid off on story-maker with 30 specs; below about ten specs its overhead outweighs the parallelism. *Where:* `commands/`, plus a *Parallel lanes* section in the template's `AGENTS.md`.

### 10. Token hygiene hooks
A `PreToolUse` hook that trims test and build output to the failures; a `Stop` hook that proposes `AGENTS.md` updates; an `Explore` subagent pinned to a cheap model; a `ccusage` baseline before any optimisation.
*Why:* the measured wins are context discipline, not compressors ([large-repo study](docs/token-optimization-large-repos.md)). Every one enters with a paired A/B on real tasks. *Where:* `hooks/`, `agents/`.

### 11. Budget guard
A hard cap on tokens per run or concurrent context, enforced in code, failing loudly instead of degrading silently.
*Why:* silent runtime degradation is the failure mode that hides until the demo. *Where:* a template requirement in the base spec, with its check.

## Known limits of what ships today

- `guard-plan` is repo-wide: any open approved plan unlocks every code dir. Per-spec module ownership would narrow it.
- `guard-secrets` matches known shapes; an unknown provider's key passes. The CI gitleaks job is the second net.
- `gitleaks/gitleaks-action` needs a free licence key for repositories owned by a GitHub organisation; personal accounts need none.
