# sdd

Spec-driven development for Claude Code: a project template whose layers run docs → spec → plan → tests → code, and the skills that work it.

| Skill | Invoked by | What it does |
|---|---|---|
| `new-project` | you: `/sdd:new-project` | Copies the template into an empty directory, then grills you to fill its gaps |
| `grill-me` | you or another skill | Interviews you in rounds of multiple-choice questions until every decision is settled |
| `wayfinder` | you: `/sdd:wayfinder` | Charts an effort too big for one session as a map of decision tickets in `.scratch/`, one ticket per session |
| `verification` | you or the agent | Writes `docs/verification.md`: what is verified, with which technique and which T/A/I/D/U class |

## Depends on

`mattpocock-skills`, from the official marketplace: `wayfinder` calls its `research`, `prototype` and `domain-modeling`, and the template's `AGENTS.md` loads its `writing-for-agents`. It is declared in `plugin.json` `dependencies`, so installing `sdd` installs it too; the marketplace allows that cross-marketplace pull in `allowCrossMarketplaceDependenciesOn`. Unversioned: it tracks upstream's latest.

## Provenance

| Skill | Origin | Licence |
|---|---|---|
| `grill-me` | adapted from `mattpocock/skills`, `skills/productivity/grilling`: every round goes through `AskUserQuestion` | MIT, [LICENSE](skills/grill-me/LICENSE) |
| `wayfinder` | adapted from `mattpocock/skills` at `959a8e9`, `skills/engineering/wayfinder`: calls `sdd:grill-me`, keeps the tracker in local `.scratch/`, adds *Repo overrides* pointing at the template's `workflow/` | MIT, [LICENSE](skills/wayfinder/LICENSE) |
| `verification`, `new-project` | own | — |

Re-copying an adapted skill from its origin erases its adaptation: re-apply it.

## Changing the template

The template lives in `skills/new-project/template/`, stored inert: `dot-` prefixes and `.tmpl` suffixes, which `scaffold.sh` turns back into `.claude/`, `AGENTS.md` and the rest. Test a change on a scratch directory:

```bash
cd skills/new-project
bash scaffold.sh /tmp/probe && bash check.sh /tmp/probe   # check lists every gap: expected on a fresh copy
```
