# Class Notes

Consolidated notes from weeks 1 and 2, plus the course slides on context failure modes and loop anatomy.

## Contents

1. [Context engineering](#1-context-engineering)
2. [Protocols and observability](#2-protocols-and-observability)
3. [Agentic loops](#3-agentic-loops)
4. [Agents and architecture](#4-agents-and-architecture)
5. [Starting a new project](#5-starting-a-new-project)
6. [Verification](#6-verification)
7. [Prompting and working practices](#7-prompting-and-working-practices)
8. [Reusable prompts](#8-reusable-prompts)
9. [Story Maker: design notes and open questions](#9-story-maker-design-notes-and-open-questions)
10. [To-do](#10-to-do)

---

## 1. Context engineering

### 1.1 Four basic context-management techniques

| Technique | What it means |
|---|---|
| **Write** | Persist information outside the context window (notes, scratchpads, memory files) so it can be reloaded later. |
| **Select** | Pull only the relevant pieces into the window (retrieval, targeted file reads). |
| **Compress (summarize)** | Replace long content with a shorter summary that keeps what matters. |
| **Isolate** | Split work across separate contexts (sub-agents, sandboxes) so one task doesn't pollute another. |

### 1.2 Six failure modes to spot

| # | Failure mode | Symptom |
|---|---|---|
| 1 | **Context stuffing** | Pasting whole files "just in case" — the one critical line drowns. |
| 2 | **Invisible prerequisites** | Humans share tribal knowledge; the model only sees tokens. |
| 3 | **Stale scratchpads** | Old plans left in context contradict the new goal. |
| 4 | **Altitude thrash** | Swinging between rigid scripts and vague prompts after each failure. |
| 5 | **Prompt-only debugging** | Rewriting adjectives while the window still lacks the assertion. |
| 6 | **Silent success metrics** | Celebrating fluent answers without checking the evidence was present. |

### 1.3 The accordion strategy

Alternate between opening and closing the context:

- **Open** — understand the global context first.
- **Close** — then run narrow, specific analyses.

---

## 2. Protocols and observability

### 2.1 Topics covered

- **MCP (Model Context Protocol)** — the standard for connecting models/agents to external tools and data.
- **MCP Apps (MCP UI)** — MCP servers that also return interactive UI components, not just data.
- **A2A (Agent-to-Agent)** — protocol for agents to discover and talk to each other.
- **Langfuse** — LLM observability platform: traces, prompts, datasets, scores and evaluators.
  - What it does.
  - How to install/integrate it.
  - How to connect it via MCP.
  - How to run an initial diagnosis of traces through the Langfuse MCP.

### 2.2 Quality evaluator in Langfuse

Ideal state: an evaluator on **every output of every agent**, scoring whether each response is good or bad.

---

## 3. Agentic loops

### 3.1 What it is

An automated improvement loop for an agent or skill (with evaluators and a place where self-learning happens). It iterates until it either:

- reaches the goal, or
- stops improving (e.g. 5 iterations with no change).

Always review the loop's design before running it.

### 3.2 Anatomy of a loop — five fields

| Field | Question | Details |
|---|---|---|
| **Trigger** | What starts it | Entry conditions: what is needed and when. |
| **Goal** | Done means what | What we want to achieve and why. |
| **Verify** | How it grades itself | The method used to **measure** improvement. |
| **Stop** | When it must halt | Two conditions: (1) the target metric is reached, or (2) N iterations have run. |
| **Memory** | What it learns | Which improvement vectors are needed, what is allowed, what is not, and where it is stored. |

> Write these five fields down and you have a **loop specification** — an artifact you can review, version, and hand to an agent.

---

## 4. Agents and architecture

- **Error compensation in multi-agent systems** — error propagation and "less is more" (fewer agents is better) apply only when agents pursue **different** goals. When all agents share the **same** goal, they don't apply.
- **Hexagonal architecture** — SOLID principles applied at application (or module) level instead of class level. It applies the dependency inversion principle at the application boundary, so the domain does not depend on infrastructure.
- **LSP (Language Server Protocol)** — gives agents code intelligence (go-to-definition, references, diagnostics).
- **Sandboxes** — give each agent a fresh sandbox.
- **Tooling** — have as many plugins, skills, etc. as possible available when working.

---

## 5. Starting a new project

Before starting, be clear on:

- **Business logic** — what problem are we solving? What is the project's goal?
- **Technical architecture** — which technologies will we use? How will the project be structured?

### 5.1 Repository conventions

- `docs/` — general project context.
- `specs/` — specific to the task at hand.
- `AGENTS.md` must define the process for:
  - changing documentation (`docs/` and `specs/`);
  - changing frontend and backend runtime code;
  - changing specifications.
- Ask for **TDD** and a **`CHANGELOG.md`**.
- Record changes with a `CHANGELOG.md` or ADRs (Architecture Decision Records: short documents recording a decision and its rationale). A revision/change history is optional and up to the engineer — it keeps a record of decisions taken.

---

## 6. Verification

Two things must be verified:

- **A.** Good code generation.
- **B.** Good execution of the agentic solution (e.g. the generated novel).

To do: analyze, write, refine and improve `verification.md`.

### 6.1 Code-level verification (is the code correct?)

- **Type checking** — automated checking that values are used consistently with what operations expect (e.g. never passing a string where a number is required).
- **Static analysis / SAST** — scanning source code without running it, matching known-bad patterns (security vulnerabilities, code smells, anti-patterns).
- **Symbolic execution** — running code with placeholder ("symbolic") inputs to derive, via an SMT solver, the exact conditions and concrete counterexamples that would break it.
- **Formal verification / theorem proving** — mathematically proving code satisfies a specification for all possible inputs, not just tested or explored ones.
- **Unit / integration testing** — checking behavior against chosen example inputs and expected outputs.
- **Property-based testing** — specifying a general property that must hold for any input, then generating many inputs automatically to search for a violation.
- **Mutation testing** — deliberately introducing small bugs to check whether the test suite actually catches them.
- **Contract testing** — verifying that the interface (request/response shape) between two services stays consistent, independent of either side's internals.

### 6.2 Process-level verification (is the agent behaving reliably?)

- **Runtime observability / tracing** — instrumenting an agent so its actual trajectory (tool calls, tokens, latency, errors) is visible and queryable after the fact.
- **Evals** — structured tests of a model/agent's behavior against a dataset and scoring method (golden dataset, LLM-as-judge, task completion, adversarial, live/online).
- **Sandboxed execution** — running agent code in an isolated environment (container, microVM) so a bad action fails safely instead of reaching production.
- **Guardrails** — policies or filters that constrain which actions/outputs an agent may produce, before it acts.
- **Human-in-the-loop review** — a person approves, rejects or edits high-consequence agent actions; the decision is fed back as a training signal.
- **Multi-agent verification** — critic/verifier (a second model checks the first), self-consistency (majority vote across repeated runs), debate (two models argue, a judge decides), reflection (self-critique and revise), ensembles (different models combined).
- **CI/CD integration** — routing agent-generated changes through the same pipeline, tests and review as human-authored code, plus provenance tagging.
- **Progressive rollout** — shipping a change behind a feature flag to a small percentage of traffic, monitored before full release.
- **Red-teaming / adversarial testing** — deliberately probing for failures under an adversarial threat model (prompt injection, tool-misuse chains, goal drift, data exfiltration), not just ordinary errors.
- **Model checking** — exhaustively exploring an agent's reachable states/transitions to verify invariants (e.g. "never delete before backup"); the multi-agent-workflow analogue of symbolic execution.

### 6.3 Classification framework (from the Trust Spec)

| Code | Method | Verified by |
|---|---|---|
| **T** | Test | Running the system against concrete inputs. |
| **A** | Analysis | Static reasoning: types, SAST, symbolic execution or formal proof. |
| **I** | Inspection | A human or a critic model reading and judging it. |
| **D** | Demonstration | Observing correct operation in a realistic scenario (staging, sandbox). |
| **U** | Unverifiable / Accepted risk | No method applies or it isn't worth the cost; named explicitly rather than left as a silent assumption. |

---

## 7. Prompting and working practices

- **Ask neutrally.** Don't ask the AI for its opinion or take a stance in the prompt — models tend to agree with you. Ask for general data, then decide yourself.
- **Iterate to zero gap.** "Run this cycle as many times as needed until the gap is zero."
- **Pyramid communication** — lead with the conclusion, then supporting points, then details.
- **With clients:** when asked questions or explaining things, state the obvious — even paraphrase and say what something is, however obvious it seems.
- **What gets valued:** tools and validators. Rough split noted in class: 20% technical work, 80% other work (unsure: the second half of this note was incomplete).

---

## 8. Reusable prompts

**Domain ontology**

```prompt
I want to build an ontology to understand the domain of an AI solution that will generate science-fiction novels about what the world will look like after the AI revolution. I want to understand what I need to manage in terms of context and quality, its anatomy, etc.
```

```prompt
Create two documents: a definitions document, and a Mermaid document (or several) with the ontology tree.
```

**Verification document**

```prompt
Please create a verification.md (to verify that our code and agent output are correct) covering all the types I want to consider below.
[paste the lists from sections 6.1, 6.2 and 6.3]
```

**Spec as SRS**

```prompt
@specs/spec1.md Update the content as a single-document SRS, using the @docs/ folder as basic context, to produce a first version of the backend.
```

**Langfuse trace report**

```prompt
Give me a full report of Claude's latest trace using the Langfuse MCP, and write it up as a Markdown report.
```

---

## 9. Story Maker: design notes and open questions

### 9.1 Scaling context across chapters

- **Question:** with 100 chapters, what does the writer read at chapter N? This decision determines whether the system scales.
- **Option — semantic search (RAG):** a vector index over previous chapters with relevance-based retrieval. Powerful, but adds infrastructure (embeddings, index, retrieval) for a problem a word cap already solves. Candidate for a second version.
- **Idea:** a dynamic hybrid RAG without reranking, using the full context; the query is defined by the agent flow.

### 9.2 Context budget (100k tokens)

- If, when assembling the context for a late scene, the accumulated canon doesn't fit in 100k tokens, what happens? Fewer CanonCards are retrieved and the summary is compressed further until it fits. That is **silent runtime degradation** — exactly what §11 rules out as a mechanism.
- Review the 100k-token maximum and how it is defined.

### 9.3 Other open questions

- Would it be better to use more agents instead of so much code?
- What are the consequences if the system fails?

---

## 10. To-do

- [ ] Define in `AGENTS.md` the change processes for docs, specs and code (see 5.1).
- [ ] Add TDD and `CHANGELOG.md` / ADRs to the project.
- [ ] Analyze, refine and improve `verification.md` (see 6).
- [ ] Set up a Langfuse quality evaluator on all agent outputs (see 2.2).
- [ ] Review the 100k-token context budget and the degradation behavior (see 9.2).
