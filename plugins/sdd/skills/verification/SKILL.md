---
name: verification
description: Generates or updates `verification.md`, the project's verification plan — what is verified, with which technique and with which T/A/I/D/U classification. Use it when the user asks for a "verification plan", "verification.md", "how do we verify this", "how do we know the code or the agent's output is correct", or when closing a spec and deciding the testing strategy.
---

# Verification plan

You produce **one document**: `docs/verification.md` (or one next to the spec,
if the user asks for it for a specific feature). You do not
implement the tests; you decide and document **how each thing is
verified and who does it**.

The full taxonomy of techniques and the classification framework are in
[references/taxonomia.md](references/taxonomia.md). **Read it before
writing anything.**

## Protocol

1. Read `docs/architecture.md` and the spec of the feature in progress,
   `specs/NNN-slug/spec.md` (specs live at the repo root, not
   inside `docs/`). If none of that exists, ask the user
   what is being built before inventing.
2. Inventory **what needs to be verified**, in two separate blocks:
   - **Code** — modules, interfaces between services, data
     invariants.
   - **Agents** — each agent in `.claude/agents/`, its tools,
     its output and the points where it can do harm.
3. For each element, pick **one technique** from the taxonomy and
   **exactly one letter** from the T/A/I/D/U framework. If two techniques
   apply, pick the cheapest one that gives the required guarantee and mention
   the other as optional reinforcement.
4. Every element that **cannot** be verified is classified `U` and is
   written in the table like the rest. An accepted risk is
   named; it is not omitted.
5. Write `docs/verification.md` in the format below.
6. End with one line in chat: `verification.md -> <n> elements, <m> in U`.

## Output format

```markdown
# verification.md

## Classification framework (T/A/I/D/U)
<The table of the five classes. If the document already has it, keep it as is.>

## Scope
<What this plan covers and what is explicitly left out.>

## Code verification

| # | What is verified | Technique | Class | Tool / where it lives |
|---|---|---|---|---|
| V1 | `parse_outline()` never receives invalid types | Type checking | A | mypy in CI |
| V2 | FR-REC-3: default limit = 20 | Unit test | T | `tests/test_recent.py` |

## Process verification (agents)

| # | What is verified | Technique | Class | Tool / where it lives |
|---|---|---|---|---|
| P1 | The implementer does not write outside `src/` | Guardrails | A | permissions in `settings.json` |
| P2 | Quality of the generated output | Eval LLM-as-judge | I | `evals/coherence.yaml` |

## Accepted risks (U)

| # | What is not verified | Why | Partial mitigation |
|---|---|---|---|
| U1 | Real production cost at 10k users | No equivalent environment | Progressive rollout at 5% |

## Quality gates
<What has to be green to approve a change: the commands from the
Stack in `AGENTS.md`, coverage of every class T requirement, human
review at the `I` points.>
```

## Hard rules

- ❌ Never leave an element without a class. If you don't know, it is `U` with a
  written reason.
- ❌ Never propose formal verification or symbolic execution "because
  it sounds rigorous". Justify the cost or leave it out.
- ❌ Never write tests or configure tools from this skill.
  Only the plan.
- ✅ Every row points to a concrete file or command, existing or to be
  created. No bare "unit tests".
- ✅ If the project already has `verification.md`, update it preserving
  the existing `V<n>` / `P<n>` / `U<n>` numbering.
