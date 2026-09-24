// Runs guard-plan.mjs as Claude Code does, against a throwaway project built per test.
import { test } from "node:test";
import assert from "node:assert/strict";
import { spawnSync } from "node:child_process";
import { mkdirSync, mkdtempSync, writeFileSync } from "node:fs";
import { tmpdir } from "node:os";
import { join } from "node:path";
import { fileURLToPath } from "node:url";

const HOOK = fileURLToPath(new URL("./guard-plan.mjs", import.meta.url));

const plan = ({ approved, steps }) =>
  [
    "# 001 — CFG · Plan",
    "",
    `- [${approved ? "x" : " "}] Plan approved   <- only the user marks this`,
    "",
    "### Steps",
    ...steps.map((done, i) => `- [${done ? "x" : " "}] step ${i + 1} (FR-CFG-${i + 1})`),
    "",
    "### Closing",
    "- [ ] Full suite green, type checks clean",
  ].join("\n");

function project({ codeDirs = ["src", "tests"], plans = {} } = {}) {
  const root = mkdtempSync(join(tmpdir(), "guard-plan-"));
  if (codeDirs) {
    mkdirSync(join(root, ".claude"));
    writeFileSync(join(root, ".claude", "sdd.json"), JSON.stringify({ codeDirs }));
  }
  for (const [slug, p] of Object.entries(plans)) {
    mkdirSync(join(root, "specs", slug), { recursive: true });
    writeFileSync(join(root, "specs", slug, "plan.md"), plan(p));
  }
  return root;
}

const run = (root, file_path) =>
  spawnSync(process.execPath, [HOOK], {
    input: JSON.stringify({ tool_name: "Write", cwd: root, tool_input: { file_path, content: "x" } }),
    env: { ...process.env, CLAUDE_PROJECT_DIR: root },
    encoding: "utf8",
  });

test("code with no plan at all is blocked, and the reason points to the approval gate", () => {
  const root = project();
  const r = run(root, join(root, "src", "app.py"));
  assert.equal(r.status, 2);
  assert.match(r.stderr, /src\/app\.py/);
  assert.match(r.stderr, /only the user marks it/);
});

test("code is blocked while the only plan is written but not approved", () => {
  const root = project({ plans: { "001-config": { approved: false, steps: [false] } } });
  assert.equal(run(root, "tests/test_app.py").status, 2);
});

test("code passes while an approved plan has an open step", () => {
  const root = project({ plans: { "001-config": { approved: true, steps: [true, false] } } });
  assert.equal(run(root, join(root, "src", "app.py")).status, 0);
  assert.equal(run(root, "tests/test_app.py").status, 0);
});

test("code is blocked once every approved plan has all its steps done, naming those plans", () => {
  const root = project({ plans: { "001-config": { approved: true, steps: [true, true] } } });
  const r = run(root, "src/extra.py");
  assert.equal(r.status, 2);
  assert.match(r.stderr, /001-config/);
});

test("docs, specs and a code dir's own AGENTS.md pass without a plan", () => {
  const root = project();
  for (const f of ["docs/architecture.md", "specs/001-config/spec.md", "src/AGENTS.md", "README.md"]) {
    assert.equal(run(root, f).status, 0, f);
  }
});

test("a project without .claude/sdd.json, or with no code dirs yet, guards nothing", () => {
  assert.equal(run(project({ codeDirs: null }), "src/app.py").status, 0);
  assert.equal(run(project({ codeDirs: [] }), "src/app.py").status, 0);
});

test("a directory that only shares a code dir's prefix is not guarded", () => {
  assert.equal(run(project(), "srcs/notes.txt").status, 0);
});

test("files outside the project are not its business", () => {
  const root = project();
  assert.equal(run(root, join(tmpdir(), "elsewhere", "src", "a.py")).status, 0);
});
