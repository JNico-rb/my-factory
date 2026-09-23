# Verification taxonomy

Two families of techniques and a classification framework. Pick **one
technique** and **one letter** per verified element.

## 1. Product verification (is the code correct?)

- **Type checking** — automatic check that values are used
  consistently with what operations expect of them (e.g.
  never passing a string where a number is required).
- **Static analysis / SAST** — scanning the source code without running it,
  looking for matches against patterns known to be bad
  (security vulnerabilities, code smells, antipatterns).
- **Symbolic execution** — running the code with symbolic inputs
  (placeholders) to derive, through an SMT solver, the exact conditions
  and the concrete counterexamples that would break it.
- **Formal verification / theorem proving** — proving mathematically
  that the code satisfies a specification for *all* possible
  inputs, not only the ones tested or explored.
- **Unit / integration testing** — checking behaviour against
  concrete example inputs and expected outputs.
- **Property-based testing** — specifying a general property that must
  hold for any input, and generating many inputs
  automatically looking for a violation.
- **Mutation testing** — deliberately introducing small bugs to
  check whether the existing test suite actually catches them.
- **Contract testing** — verifying that the interface (request/response
  shape) between two services stays consistent,
  regardless of the internals of each side.

## 2. Process verification (does the agent behave reliably?)

- **Runtime observability / tracing** — instrumenting an agent so that its
  actual trajectory (tool calls, tokens, latency, errors)
  is visible and queryable afterwards.
- **Evals** — structured tests of a model's or agent's behaviour
  against a dataset and a scoring method: golden-dataset,
  LLM-as-judge, task-completion, adversarial, live/online.
- **Sandboxed execution** — running the agent's code in an isolated
  environment (container, microVM) so that a bad action fails without
  consequences instead of reaching production.
- **Guardrails** — policies or filters that restrict which actions or
  outputs an agent can produce, *before* it acts.
- **Human-in-the-loop review** — a person approves, rejects or edits
  high-consequence actions, and that decision is fed back as a
  training signal.
- **Multi-agent verification** — critic/verifier (a second model reviews
  the first), self-consistency (majority vote across repeated
  runs), debate (two models argue and a judge decides), reflection
  (self-critique and revision), ensembles (combining different models).
- **CI/CD integration** — putting agent-generated changes through the
  same pipeline, tests and review as human-written code, plus
  provenance labelling.
- **Progressive rollout** — deploying a change behind a feature flag to a
  small percentage of traffic, monitored before the full
  rollout.
- **Red-teaming / adversarial testing** — deliberately probing for
  failures under an adversarial threat model (prompt injection,
  tool-misuse chains, goal drift, data exfiltration),
  not just ordinary error.
- **Model checking** — exhaustively exploring the reachable states and transitions
  of an agent to verify invariants (e.g. "never
  delete before backing up"); the analogue of symbolic execution for
  multi-agent flows.

## 3. Classification framework (Trust Spec)

- **T — Test** — verified by running the system against concrete
  inputs.
- **A — Analysis** — verified by static reasoning: types, SAST,
  symbolic execution or formal proof.
- **I — Inspection** — verified by a human or a critic model that
  reads it and judges it.
- **D — Demonstration** — verified by observing correct operation
  in a realistic scenario (staging, sandbox).
- **U — Unverifiable / Accepted Risk** — no method applies, or the cost
  is not worth it; it is named explicitly instead of being left as a
  silent assumption.

## Technique → class mapping (default)

| Technique | Class |
|---|---|
| Type checking, SAST, symbolic execution, formal verification, model checking, guardrails | A |
| Unit/integration, property-based, mutation, contract testing, golden-dataset and task-completion evals | T |
| Human-in-the-loop, LLM-as-judge, critic/verifier, reflection, code review | I |
| Sandboxed execution, progressive rollout, observability/tracing, red-teaming in staging | D |

This table is the starting point, not a rule. An adversarial eval
run in staging is `D`; the same eval in CI is `T`. Justify the
letter when you depart from the table.
