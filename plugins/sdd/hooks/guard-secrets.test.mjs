// Runs guard-secrets.mjs as Claude Code does: tool call JSON on stdin, verdict in the exit code.
// Fake keys are assembled at runtime so this file never holds a credential-shaped literal.
import { test } from "node:test";
import assert from "node:assert/strict";
import { spawnSync } from "node:child_process";
import { fileURLToPath } from "node:url";

const HOOK = fileURLToPath(new URL("./guard-secrets.mjs", import.meta.url));
const run = (tool_input) =>
  spawnSync(process.execPath, [HOOK], { input: JSON.stringify({ tool_name: "Write", tool_input }), encoding: "utf8" });
const fake = (prefix, n = 32) => prefix + "a1B2".repeat(n / 4);

test("a Write carrying an Anthropic-shaped key is blocked, naming the kind but not the value", () => {
  const secret = fake("sk-ant-");
  const r = run({ file_path: "src/config.py", content: `KEY = "${secret}"` });
  assert.equal(r.status, 2);
  assert.match(r.stderr, /Anthropic API key/);
  assert.match(r.stderr, /src\/config\.py/);
  assert.ok(!r.stderr.includes(secret));
});

test("an Edit whose new_string carries a GitHub token is blocked", () => {
  const r = run({ file_path: "README.md", old_string: "x", new_string: `token: ${fake("ghp_", 36)}` });
  assert.equal(r.status, 2);
  assert.match(r.stderr, /GitHub classic token/);
});

test("a MultiEdit is blocked when any one of its edits carries a key", () => {
  const edits = [{ old_string: "a", new_string: "harmless" }, { old_string: "b", new_string: fake("sk-lf-") }];
  assert.equal(run({ file_path: "app.ts", edits }).status, 2);
});

test("an AWS access key id and a private key block are blocked", () => {
  assert.equal(run({ file_path: "a", content: "AKIA" + "ABCDEFGHIJKLMNOP" }).status, 2);
  assert.equal(run({ file_path: "b", content: "-----BEGIN " + "OPENSSH PRIVATE KEY-----" }).status, 2);
});

test("placeholders and prefixes inside longer words pass", () => {
  const texts = [
    "ANTHROPIC_API_KEY=YOUR_KEY_HERE",
    "keys look like sk-ant-...",
    `task-ant-${"a".repeat(30)}`,
    "Basic auth is described in RFC 7617",
  ];
  for (const content of texts) assert.equal(run({ file_path: ".env.example", content }).status, 0, content);
});

test("a call with nothing to write passes", () => {
  assert.equal(run({ file_path: "x.md" }).status, 0);
});
