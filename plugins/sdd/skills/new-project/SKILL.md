---
name: new-project
description: Start a new project from the SDD template, then grill the user to fill its gaps.
disable-model-invocation: true
allowed-tools:
  - Bash(bash "${CLAUDE_PLUGIN_ROOT}/skills/new-project/scaffold.sh" *)
  - Bash(bash "${CLAUDE_PLUGIN_ROOT}/skills/new-project/check.sh" *)
  - Bash(grep -rn *)
  - Bash(git init)
  - Bash(git init *)
---

# New project

Scaffold a spec-driven project, then close its **gaps** with the user. A gap is a `<!-- GAP: ... -->` comment in a template file: a question about this project that the template cannot answer. The copy is a script, deterministic and refusing to overwrite; closing the gaps is a grill, where every decision is the user's.

The template already decides the process: layer order, approval gates, language rule, where decisions live. The grill covers the gaps and only the gaps.

## 1. Target

The target is the path in `$ARGUMENTS`, else the current working directory, as an absolute path. When it already holds files, show the user what is there and ask whether to scaffold beside them.

Done when the target is absolute and the user has accepted any existing content.

## 2. Copy

Run both scripts of this skill with the command exactly as written here, through the **Bash tool** when you have it: `allowed-tools` pre-approves those exact strings. With only PowerShell available, run the same command there; the user approves it once.

```bash
bash "${CLAUDE_PLUGIN_ROOT}/skills/new-project/scaffold.sh" "<target>"
```

Exit 1 means template files already exist in the target: the script lists them and copies nothing. Show the list to the user and stop. When the target is not yet a git repository: `git init "<target>"`.

Done when the script exits 0 and `<target>/.git` exists.

## 3. Grill

Collect the gaps with `grep -rn "GAP:" "<target>"`. Each gap is a node of the design tree: call the Skill tool with `sdd:grill-me` and work it. Their dependencies run roughly in this order:

1. What the project is: purpose, users, what is out of scope.
2. Stack and code layout: languages, frameworks, persistence, the lint, type and test commands, top-level code directories.
3. Domain: the first glossary terms, and whether the domain holds knowledge a developer would lack.
4. Shape: main components, and how a typical use case flows through them.
5. Quality gates: the commands that must be green to approve a change.

Look facts up before recommending: installed tool versions, OS, git remote. A gap the user cannot answer yet stays open (step 4): the start of a project is allowed to be foggy.

Done when grill-me's closing confirmation is accepted.

## 4. Fill

Write each answer where its gap sits, replacing the comment, in the file's language:

- **Answered** → the content itself.
- **Still open** → an entry under `docs/architecture.md`, *Decisiones abiertas*, one `###` each: the question, the options, what it blocks.
- **Chosen over a real alternative** (a database, a framework) → a row in *Decisiones cerradas*, with its reason in the section that owns it.
- **No domain knowledge to record** → delete `docs/domain-knowledge.md` and its line in `AGENTS.md`.
- **A code directory** → create it with an `AGENTS.md` holding one line: `Read [../AGENTS.md](../AGENTS.md) first.`

## 5. Check

Run it as in step 2:

```bash
bash "${CLAUDE_PLUGIN_ROOT}/skills/new-project/check.sh" "<target>"
```

It lists every gap left and every relative markdown link that does not resolve. Done when it exits 0: fix what it lists and re-run until then.

## 6. Hand over

The skill ends at the scaffold: specs, plans and code each pass their own gate in the new `AGENTS.md`. Tell the user the next step is the first spec, `specs/001-<slug>/`, through `workflow/2-specs.md` and its own grill round, and that an effort too big for one session goes to `/sdd:wayfinder`. Remind them to commit.
