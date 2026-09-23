# Process 2 — Changing a spec

One folder per feature: `specs/NNN-slug/`, `NNN` sequential in implementation order, `slug` in ASCII without accents. Two features = two folders.

| File | Holds |
|---|---|
| `spec.md` | What: observable behaviour, with its own approval box |
| `plan.md` | Steps, with its own approval box ([process 3](3-plan.md)) |
| `design.md` | How, only when the spec needs it: tables, modules, external interfaces |

`spec.md` contents, in this order:

1. `# NNN — <MÓD> · <título>`, where `<MÓD>` is the spec's short module code (`CFG`), unique across specs.
2. `- [ ] Spec approved   <- only the user marks this`
3. `## Objetivo` — one sentence.
4. `## Alcance` — what it covers and, if anything, what it leaves **fuera de alcance**, with the reason.
5. `## Requisitos` — `RF-<MÓD>-<n>`, each a checkable case: condition → observable result, including rejections and boundaries. Every row carries its priority and its T/A/I/D/U class.
6. `## Requisitos no funcionales` (`RNF-<n>`, with class) and `## Restricciones` (`R<n>`, with origin), when the spec has any.
7. `## Docs de referencia` — the sections of `docs/*.md` it rests on.

Rules:

- Behaviour only: file names, signatures and libraries go to `design.md`. Exception: a base spec (`001`) may hold the stack's technical constraints (`architecture.md` §3) as checkable requirements.
- Consistent with `docs/*.md`; where they disagree, the doc wins. A spec that needs a doc change waits for [process 1](1-docs.md).
- **Obligatorio** is V1; **Deseable** may be left out without invalidating the delivery.
- A figure not yet calibrated is named as such and given no value; it is listed in `architecture.md` *Decisiones abiertas*.
- Requirements go in the module's logical order. A new one is inserted where it belongs and renumbers the ones after it; the renumbering reaches its `plan.md` in the same commit. A retired one leaves its gap.
- Dependent specs reference each other by number; each requirement lives in exactly one spec.
- Written → leave its approval box unmarked and **stop**.
- Changing one: edit the requirements, unmark the approval boxes of both `spec.md` and `plan.md`, re-run [process 3](3-plan.md) and [process 4](4-code.md) for what changed. A deleted requirement means a deleted test.
