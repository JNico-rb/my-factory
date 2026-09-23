# My Factory

Harness engineering resources: a Claude Code plugin marketplace with my reusable skills and agents, and a template to start spec-driven projects.

## Use it

Once per machine, from any Claude Code session:

```
/plugin marketplace add JNico-rb/my-factory
/plugin install sdd@my-factory
```

Then, in an empty directory, `/sdd:new-project`: it copies the template and grills you to fill its gaps. What each skill does: [plugins/sdd/README.md](plugins/sdd/README.md).

Projects created that way declare this marketplace in their `.claude/settings.json`, so whoever clones one is offered the plugins when they trust the folder.

## Layout

```
my-factory/
├── .claude-plugin/marketplace.json   # the marketplace: which plugins exist, where each lives
├── plugins/
│   └── sdd/                          # a plugin: only this folder reaches an install
└── drafts/                           # not shipped yet, each with its reason
```

## Changing a plugin

1. Edit it under `plugins/<name>/`.
2. Try it without publishing: `claude --plugin-dir <path-to>/plugins/<name>` from a scratch directory.
3. Bump `version` in its `.claude-plugin/plugin.json`: installs only update when it changes.
4. `claude plugin validate . --strict` and `claude plugin validate plugins/<name> --strict` pass.
5. Commit and push. Installs follow on the marketplace's auto-update (enable it in `/plugin`), or at once with `claude plugin update sdd@my-factory` and `/reload-plugins`.
