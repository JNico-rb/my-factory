# docs — the knowledge behind the factory

What the `sdd` plugin is built on: course notes, research and references. No install ships this folder; the plugin distils what it needs into its skills, hooks and template. Each file keeps the language it was written in.

| Doc | Read it when | Lang |
|---|---|---|
| [class-notes.md](class-notes.md) | You need the concepts: the four context techniques and six failure modes, loop anatomy (trigger · goal · verify · stop · memory), the verification taxonomy and its T/A/I/D/U classes | EN |
| [links.md](links.md) | You pick a tool: formal validators (TLA+, Lean 4, Dafny, P, Alloy, Kani…) with who uses them and their trend, candidate skills and why each was taken or left | EN |
| [token-optimization-llm-projects.md](token-optimization-llm-projects.md) | A **product** spends too many tokens: caching, context editing, compaction, batch, model routing, with measured impact | ES |
| [token-optimization-large-repos.md](token-optimization-large-repos.md) | A **coding agent** spends too many tokens on a big repo: CLAUDE.md size, LSP, subagents, hooks that filter output, and which "token savers" failed independent measurement | ES |
| [prompts.md](prompts.md) | You start a domain ontology, a `verification.md` or a token-optimisation study: the prompts used in class, ready to paste | ES |

## Where each idea landed

| Idea | Source | Now lives in |
|---|---|---|
| Verification taxonomy and T/A/I/D/U | [class-notes.md §6](class-notes.md#6-verification) | `sdd:verification` skill, template `docs/verification.md` |
| `docs/` for context, `specs/` for the task, change processes in `AGENTS.md` | [class-notes.md §5.1](class-notes.md#51-repository-conventions) | template `AGENTS.md` and `workflow/` |
| Guardrails before the agent acts | [class-notes.md §6.2](class-notes.md#62-process-level-verification-is-the-agent-behaving-reliably) | `guard-secrets` and `guard-plan` hooks |
| Critic/verifier: a second agent checks the first | [class-notes.md §6.2](class-notes.md#62-process-level-verification-is-the-agent-behaving-reliably) | `sdd:verifier` agent |
| Red-teaming: prompt injection, exfiltration, leaked secrets | [class-notes.md §6.2](class-notes.md#62-process-level-verification-is-the-agent-behaving-reliably) | `sdd:security-auditor` agent |
| Instruction files under 200 lines | [token-optimization-large-repos.md §3](token-optimization-large-repos.md) | `check.sh` fails on a longer `AGENTS.md` |
| Loop specification, validator registry, formal starters | [class-notes.md §3](class-notes.md#3-agentic-loops), [links.md](links.md) | not yet: [ROADMAP.md](../ROADMAP.md) |
