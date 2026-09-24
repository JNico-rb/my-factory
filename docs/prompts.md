## Investigar

<prompt>
Necesito empezar a optimizar el uso de tokens en el proyecto. Buscame pulg-ins, herramientas... lo que sea que me pueda ayudar.
</prompt>

<prompt>
Investiga las mejores estrategias, proceso de desarrollo de código y herramientas (plugins, skills, etc) para optimizar al máximo el uso de tokens en repositorios con millones de líneas de código.
</prompt>

## Verification

<prompt>
- Quiero construir una ontología para entender el dominio de una solución de IA que generará novelas de ciencia ficción sobre como será el mundo tras la revolución de la IA. Quiero entender qué necesito gestionar a nivel de contexto y de calidad, su anatomía, etc.
</prompt>

<prompt>
- Créame dos documentos: el documento de definiciones y el documento de mermaid o mermaids con el árbol de las ontologías.
</prompt>

<prompt>
Please createa a verification.md (to verify that our code and agent output is correct) below all the types I want to consider.

* Type checking — automated checking that values are used consistently with what operations expect of them (e.g., never passing a string where a number is required).
* Static analysis / SAST — scanning source code without running it, to match against known-bad patterns (security vulnerabilities, code smells, anti-patterns).
* Symbolic execution — running code with placeholder ("symbolic") inputs to derive, via an SMT solver, the exact conditions and concrete counterexamples that would break it.
* Formal verification / theorem proving — mathematically proving code satisfies a specification for all possible inputs, not just tested or explored ones.
* Unit / integration testing — checking behavior against specific, chosen example inputs and expected outputs.
* Property-based testing — specifying a general property that must hold for any input, then generating many inputs automatically to search for a violation.
* Mutation testing — deliberately introducing small bugs into code to check whether the existing test suite actually catches them.
* Contract testing — verifying that the interface (request/response shape) between two services stays consistent, independent of either side's internals.

Process-level verification (is the agent behaving reliably?)

* Runtime observability / tracing — instrumenting an agent so its actual trajectory (tool calls, tokens, latency, errors) is visible and queryable after the fact.
* Evals — structured tests of a model/agent's behavior against a dataset and scoring method (golden-dataset, LLM-as-judge, task-completion, adversarial, live/online).
* Sandboxed execution — running agent code in an isolated environment (container, microVM) so a bad action fails safely rather than reaching production.
* Guardrails — policies or filters that constrain what actions/outputs an agent is allowed to produce, before it acts.
* Human-in-the-loop review — a person approves, rejects, or edits high-consequence agent actions, with the decision fed back as a training signal.
* Multi-agent verification — critic/verifier (a second model checks the first), self-consistency (majority vote across repeated runs), debate (two models argue, a judge decides), reflection (self-critique and revise), ensembles (different models combined).
* CI/CD integration — routing agent-generated changes through the same pipeline, tests, and review as human-authored code, plus provenance tagging.
* Progressive rollout — shipping a change behind a feature flag to a small percentage of traffic, monitored before full release.
* Red-teaming / adversarial testing — deliberately probing for failures under an adversarial threat model (prompt injection, tool misuse chains, goal drift, data exfiltration), not just ordinary error.
* Model checking — exhaustively exploring an agent's reachable states/transitions to verify invariants (e.g., "never delete before backup"), the multi-agent-workflow analogue of symbolic execution.

Classification framework (from the Trust Spec)

* T — Test — verified by running the system against concrete inputs.
* A — Analysis — verified by static reasoning: types, SAST, symbolic execution, or formal proof.
* I — Inspection — verified by a human or a critic model reading and judging it.
* D — Demonstration — verified by observing correct operation in a realistic scenario (staging, sandbox).
* U — Unverifiable / Accepted Risk — no method applies, or isn't worth the cost; named explicitly rather than left as a silent assumption.
</prompt>