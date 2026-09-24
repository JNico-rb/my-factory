// PreToolUse hook (Edit|Write|MultiEdit): blocks writing new text that carries something shaped like
// a real credential. Exit 2 blocks the tool call and hands stderr back to the model; the message names
// the kind of credential and the target file, never the value. Placeholders stay allowed: every
// pattern needs a long run of key characters after its prefix, which YOUR_KEY_HERE never has.
import { readFileSync } from "node:fs";

// A prefix "starts a word": at the beginning of the text or after a non [A-Za-z0-9_] character.
const key = (prefix) => new RegExp(`(?<![A-Za-z0-9_])${prefix}[A-Za-z0-9_-]{20}`);

const PATTERNS = [
  ["Anthropic API key", key("sk-ant-")],
  ["OpenRouter API key", key("sk-or-v1-")],
  ["OpenAI project key", key("sk-proj-")],
  ["Langfuse secret key", key("sk-lf-")],
  ["Langfuse public key", key("pk-lf-")],
  ["Stripe live key", key("sk_live_")],
  ["GitHub classic token", key("ghp_")],
  ["GitHub OAuth token", key("gho_")],
  ["GitHub fine-grained token", key("github_pat_")],
  ["Slack token", key("xox[abprs]-")],
  ["Google API key", /(?<![A-Za-z0-9_])AIza[A-Za-z0-9_-]{35}/],
  ["AWS access key id", /(?<![A-Za-z0-9])AKIA[0-9A-Z]{16}(?![0-9A-Z])/],
  ["private key block", /-----BEGIN (?:RSA |EC |DSA |OPENSSH |PGP )?PRIVATE KEY-----/],
  ["HTTP Basic credential", /Basic [A-Za-z0-9+/=]{40}/],
];

const args = JSON.parse(readFileSync(0, "utf8")).tool_input ?? {};
// The texts the call would write: Write's content, Edit's new_string, MultiEdit's edits.
const texts = [args.content, args.new_string, ...(args.edits ?? []).map((e) => e.new_string)]
  .filter((t) => typeof t === "string");

for (const [kind, pattern] of PATTERNS) {
  if (texts.some((t) => pattern.test(t))) {
    process.stderr.write(
      `guard-secrets: blocked. The new text for ${args.file_path ?? "the file"} carries something shaped like ` +
        `a ${kind}. Secrets live only in .env, which the user writes by hand; in code, docs and tests use a ` +
        "placeholder such as YOUR_KEY_HERE.\n",
    );
    process.exit(2);
  }
}
