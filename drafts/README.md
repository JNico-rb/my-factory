# drafts — not shipped

Work not ready for a plugin. Nothing here is listed in `.claude-plugin/marketplace.json`, so no install ever copies it.

## agents/

`leader`, `spec_author`, `implementer`, `code_reviewer`, `reviewer`. They follow an earlier workflow the `sdd` template does not create: `feature_list.json`, `progress/`, `init.sh`, Kiro-style `specs/<name>/{requirements,design,tasks}.md`, `docs/conventions.md`, `CHECKPOINTS.md`. And `leader` takes "approved" in chat as approval, where the template's `AGENTS.md` lets only the user mark an approval box.

To ship them, adapt them to the template's layers first (`specs/NNN-slug/{spec,plan}.md`, `FR-<MOD>-<n>`, the approval boxes), then move them to `plugins/sdd/agents/`.
