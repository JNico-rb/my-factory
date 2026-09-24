# <!-- GAP: project name -->

<!-- GAP: one paragraph: what the project does and for whom. -->

## Repo layout

```
./
├── AGENTS.md          # universal rules
├── README.md
├── .env.example       # the variables .env must define, with placeholders
├── docs/              # source of truth: domain and design
├── specs/             # one folder per feature, NNN-slug/: spec.md, plan.md, design.md
├── workflow/          # how each layer changes: docs, specs, plans, code
├── .github/workflows/ # CI: the quality gates and a secrets scan of the whole history
└── .claude/           # project settings: denied reads, harness plugin, code dirs guarded by guard-plan
```

<!-- GAP: the top-level code directories (e.g. backend/, frontend/), each added to the tree above with a one-line comment. -->
