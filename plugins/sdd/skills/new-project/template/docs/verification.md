# verification.md

How we check that **the code is correct** and that **the agents behave reliably**. The vocabulary is in `definitions.md`; the design decisions, in `architecture.md`. Written and maintained by the `sdd:verification` skill.

---

## Classification framework (T/A/I/D/U)

Every requirement, invariant or component gets **one letter**. The letter says *how* confidence is earned, not *how much*.

| Class | Name | Verified by… |
|---|---|---|
| **T** | Test | Running the system with concrete inputs |
| **A** | Analysis | Reasoning statically: types, static analysis, symbolic execution, formal proof |
| **I** | Inspection | Reading and judging: a person or a critic model |
| **D** | Demonstration | Observing correct operation in a realistic scenario |
| **U** | Unverifiable | No applicable method, or not worth its cost |

- **U is a decision, not an oversight.** It is written under *Accepted risks (U)* with name and reason.
- **The letter is not a promotion.** A is not "better" than T: pick the cheapest method that gives the required guarantee.

## Scope

Written with the `sdd:verification` skill when writing the first spec.

## Code verification

| # | What is verified | Technique | Class | Tool / where it lives |
|---|---|---|---|---|

## Process verification (agents)

| # | What is verified | Technique | Class | Tool / where it lives |
|---|---|---|---|---|

## Accepted risks (U)

| # | What is not verified | Why | Partial mitigation |
|---|---|---|---|

## Quality gates

<!-- GAP: the commands that have to be green to approve a change (the lint, type and test ones from the Stack in AGENTS.md). -->
