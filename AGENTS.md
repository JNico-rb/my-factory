# AGENTS.md — Instructions

This repo is a Claude Code plugin marketplace: the `sdd` plugin in `plugins/sdd/` is what installs ship; everything else supports it. Layout: [README.md](README.md).

## Rules

- **Project-agnostic.** Nothing in `plugins/` names a specific project; what a project decides stays a `GAP:` in the template, closed by `/sdd:new-project`'s grill.
- **Every shipped piece has its check.** A new hook comes with its `*.test.mjs`; a template change keeps `scripts/verify.sh` green; a new check goes into that script.
- **Done** = `bash scripts/verify.sh` prints `verify: all green`. Report its output, not a summary of it.
- **Changing a plugin** → bump `version` in its `.claude-plugin/plugin.json` in the same commit: installs only update when it changes.
- **Language.** New files in English. Files in `docs/` keep the language they were written in.
- Editing a skill, an agent, `AGENTS.md` or anything in `.claude/` → load the `mattpocock-skills:writing-for-agents` skill first.
- **Personal data stays out of the repo.** `notes*`, `presentation/` and `project-constraints.md` are gitignored because they carry third parties' names and addresses; quote them by topic, never by person.
- Ideas not yet shipped go to [ROADMAP.md](ROADMAP.md), with their source.

## Replies to me

**When reporting information to me be extremely concise and sacrifice grammar for the sake of concision.**

- Terse. Fragments OK. No preamble, no recap. (I read fast; filler hides the answer.)
- Explain any necessary technical term in one sentence.
- If I'm wrong, say so directly. Don't agree to please me.
- State uncertainty explicitly ("unsure: X") instead of guessing.

## Before non-trivial changes (>1 file or ambiguous request)

- List assumptions in 1–3 bullets.
- If a decision is genuinely mine and changes the result, ask ONE question. Otherwise pick the sensible default and say which.
- If a simpler approach exists, propose it before implementing.
