# Useful links

## Validators
Languages and tools for building validators and logic provers:

- [TLA+](https://foundation.tlapl.us): specification and model checking · AWS, Microsoft, Oracle, Intel and distributed databases · ↑ moderate growth. — *2026-09-23*
- [Lean 4](https://lean-lang.org): proof assistant and programming language · mathematics, AWS, DeepMind and AI startups like Harmonic · ↑↑ fastest growing. — *2026-09-23*
- [Rocq (formerly Coq)](https://rocq-prover.org): proof assistant · CompCert, academia and cryptography · → stable, losing relative weight to Lean. — *2026-09-23*
- [Isabelle/HOL](https://isabelle.in.tum.de): proof assistant · seL4 (verified microkernel) and academia · → stable. — *2026-09-23*
- [Dafny](https://dafny.org): verifiable language, styled like C# or Python · AWS and AI benchmarks · ↑ growing. — *2026-09-23*
- [SPARK (Ada)](https://adacore.com/about-spark): verifiable subset of Ada · aerospace, defence and rail · → stable, regulated niche. — *2026-09-23*
- [B-Method](https://atelierb.eu) / [Event-B](https://event-b.org): specification by refinement · metro and rail (Alstom, Siemens) · → / ↓ legacy. — *2026-09-23*
- [P](https://p-org.github.io/P): state machine modelling · AWS (S3, DynamoDB…) · ↑ growing. — *2026-09-23*
- [Alloy](https://alloytools.org): lightweight specification · academia and data model design · → stable. — *2026-09-23*
- [F*](https://fstar-lang.org): dependently typed language · cryptography (HACL*, shipped in Firefox, Linux and Windows) · → niche. — *2026-09-23*
- [Verus](https://github.com/verus-lang/verus): Rust code verification · Rust systems and Microsoft Research · ↑↑ growing fast, from a small base. — *2026-09-23*
- [Kani](https://github.com/model-checking/kani): model checker for Rust · AWS and the Rust standard library · ↑ growing. — *2026-09-23*
- [Quint](https://quint-lang.org) · [Quint on GitHub](https://github.com/informalsystems/quint): TLA specification with modern syntax · blockchain and consensus protocols · ↑↑ growing fast, from a small base. — *2026-09-23*
- [Agda](https://agda.readthedocs.io) / [Idris 2](https://idris-lang.org): dependent types · type theory research · → academic. — *2026-09-23*
- Z, [VDM](https://overturetool.org), [PVS](https://pvs.csl.sri.com): classic specification · NASA (PVS) and legacy industrial systems · ↓ declining. Z has no single official website; it is an ISO standard. — *2026-09-23*

## Skills

- [mattpocock/skills · implement](https://github.com/mattpocock/skills/blob/main/skills/engineering/implement/SKILL.md): implements a spec or tickets with `/tdd`, reviews with `/code-review` and commits. MIT. **Already installed** with `mattpocock-skills` (dependency of `sdd`): `/mattpocock-skills:implement`. Only I invoke it (`disable-model-invocation: true`). — *2026-09-23*
- [obra/superpowers · test-driven-development](https://github.com/obra/superpowers/blob/main/skills/test-driven-development/SKILL.md): strict TDD ("no production code without a failing test first"). MIT. **Do not install**: clashes with `mattpocock-skills:tdd` and requires the whole `superpowers` plugin. Ideas to salvage for `workflow/4-code.md`: run the whole suite and report every red even if it isn't mine; table of excuses for skipping TDD. — *2026-09-23*
- [DietrichGebert/ponytail](https://github.com/DietrichGebert/ponytail/blob/main/skills/ponytail/SKILL.md): "lazy senior dev" mode: YAGNI, stdlib and native features before custom code, lite/full/ultra levels. MIT. Plugin with its own marketplace; if I want it, at user level, not inside `sdd`: `/plugin marketplace add DietrichGebert/ponytail` + `/plugin install ponytail@ponytail`. Heads-up: it activates on every code response. — *2026-09-23*

## Webs

- [Best SDD tools](https://codemyspec.com/blog/best-spec-driven-development-tools)
