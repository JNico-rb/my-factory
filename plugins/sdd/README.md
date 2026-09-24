# sdd

Spec-driven development for Claude Code: a project template whose layers run docs → spec → plan → tests → code, the skills that work it, the agents that implement and close each plan, and the hooks that enforce its gates.

## Skills

| Skill | Invoked by | What it does |
|---|---|---|
| `new-project` | you: `/sdd:new-project` | Copies the template into an empty directory, then grills you to fill its gaps |
| `grill-me` | you or another skill | Interviews you in rounds of multiple-choice questions until every decision is settled |
| `wayfinder` | you: `/sdd:wayfinder` | Charts an effort too big for one session as a map of decision tickets in `.scratch/`, one ticket per session |
| `verification` | you or the agent | Writes `docs/verification.md`: what is verified, with which technique and which T/A/I/D/U class |

## Agents

| Agent | Delegated when | Writes |
|---|---|---|
| `implementer` | a plan's approval box is marked and its steps are open | tests and code of that plan's steps, one commit per green step |
| `verifier` | an implementation says it is done | only the plan's *Closing* boxes, on PASS; a FAIL lists `file:line · expected · found` |
| `security-auditor` | before a delivery, or after a change to an input path | only `docs/security-report.md` |

The implementer never closes its own work: that separation is the point. FAIL goes back to the implementer at most twice, then to the user (template `AGENTS.md`, *Guards and roles*).

## Hooks

Both run as `PreToolUse` on `Edit|Write|MultiEdit`, in every project where the plugin is enabled. Exit 2 blocks the call and tells the model why; they need only Node.

| Hook | Blocks | Passes |
|---|---|---|
| `guard-secrets` | new text shaped like a real credential (Anthropic, OpenAI, OpenRouter, Langfuse, GitHub, Slack, Google, AWS, Stripe, private keys, HTTP Basic); the message names the kind, never the value | placeholders such as `YOUR_KEY_HERE` |
| `guard-plan` | a write under a code dir of `.claude/sdd.json` while no `specs/*/plan.md` is approved with an open step | docs, specs, a code dir's own `AGENTS.md`, and any project without `.claude/sdd.json` |

Tests: `node --test plugins/sdd/hooks/*.test.mjs`. They run each hook as Claude Code does, JSON on stdin and the verdict in the exit code.

## Depends on

`mattpocock-skills`, from the official marketplace: `wayfinder` calls its `research`, `prototype` and `domain-modeling`, and the template's `AGENTS.md` loads its `writing-for-agents`. It is declared in `plugin.json` `dependencies`, so installing `sdd` installs it too; the marketplace allows that cross-marketplace pull in `allowCrossMarketplaceDependenciesOn`. Unversioned: it tracks upstream's latest.

## Provenance

| Piece | Origin | Licence |
|---|---|---|
| `grill-me` | adapted from `mattpocock/skills`, `skills/productivity/grilling`: every round goes through `AskUserQuestion` | MIT, [LICENSE](skills/grill-me/LICENSE) |
| `wayfinder` | adapted from `mattpocock/skills` at `959a8e9`, `skills/engineering/wayfinder`: calls `sdd:grill-me`, keeps the tracker in local `.scratch/`, adds *Repo overrides* pointing at the template's `workflow/` | MIT, [LICENSE](skills/wayfinder/LICENSE) |
| `verification`, `new-project`, agents, hooks | own; agents and hooks generalised from the story-maker project's harness | — |

Re-copying an adapted skill from its origin erases its adaptation: re-apply it.

## Changing the template

The template lives in `skills/new-project/template/`, stored inert: `dot-` prefixes and `.tmpl` suffixes, which `scaffold.sh` turns back into `.claude/`, `.github/`, `.env.example`, `AGENTS.md` and the rest. `scripts/verify.sh` at the repo root scaffolds it, checks that its gaps are reported, fills them and checks it clean; run it after any change.
