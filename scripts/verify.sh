#!/usr/bin/env bash
# Verifies the factory end to end; CI runs the same script (.github/workflows/verify.yml).
#   1. every JSON config parses;
#   2. the marketplace and the plugin pass `claude plugin validate --strict`;
#   3. the hooks' tests pass;
#   4. the template scaffolds, refuses to overwrite, reports its gaps, and checks clean once filled;
#   5. every relative link in this repo's markdown resolves.
# Each check prints one line; the script exits 1 at the end if any failed.
set -uo pipefail

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
SKILL="$ROOT/plugins/sdd/skills/new-project"
failed=0
pass() { printf 'ok    %s\n' "$1"; }
fail() { printf 'FAIL  %s\n' "$1"; failed=1; }
check() { local name="$1"; shift; if "$@" > "$LOG" 2>&1; then pass "$name"; else fail "$name"; sed 's/^/      /' "$LOG"; fi; }

LOG="$(mktemp)"
TMP="$(mktemp -d)"
trap 'rm -rf "$TMP" "$LOG"' EXIT

# 1. JSON
json_ok() {
  local f
  for f in "$ROOT/.claude-plugin/marketplace.json" "$ROOT/plugins/sdd/.claude-plugin/plugin.json" \
           "$ROOT/plugins/sdd/hooks/hooks.json" "$SKILL/template/dot-claude/settings.json" \
           "$SKILL/template/dot-claude/sdd.json"; do
    node -e 'JSON.parse(require("fs").readFileSync(process.argv[1], "utf8"))' "$f" || { echo "invalid JSON: $f"; return 1; }
  done
}
check "JSON configs parse" json_ok

# 2. Plugin validation
if command -v claude > /dev/null; then
  check "marketplace validates (--strict)" claude plugin validate "$ROOT" --strict
  check "plugin sdd validates (--strict)" claude plugin validate "$ROOT/plugins/sdd" --strict
else
  fail "claude CLI not found: install it (npm install -g @anthropic-ai/claude-code) to validate the plugin"
fi

# 3. Hooks
check "hook tests pass" node --test "$ROOT"/plugins/sdd/hooks/*.test.mjs

# 4. Template, end to end on a throwaway copy
check "template scaffolds into an empty dir" bash "$SKILL/scaffold.sh" "$TMP/p"
refuses() { ! bash "$SKILL/scaffold.sh" "$TMP/p"; }
check "scaffold refuses to overwrite" refuses
# Expects check.sh to exit 1 with a given line in its output.
rejects_with() {
  local out
  if out="$(bash "$SKILL/check.sh" "$TMP/p" 2>&1)"; then echo "check.sh exited 0"; return 1; fi
  grep -qF "$1" <<< "$out" || { echo "expected \"$1\" in:"; echo "$out"; return 1; }
}
reports_gaps() { rejects_with 'gaps still open'; }
check "check.sh reports the gaps of a fresh copy" reports_gaps
fill_and_check() {
  # Stand in for the grill: every line holding a gap becomes plain text.
  grep -rl --exclude-dir=.git 'GAP:' "$TMP/p" | while IFS= read -r f; do sed -i '/GAP:/c\filled' "$f"; done
  bash "$SKILL/check.sh" "$TMP/p"
}
check "check.sh passes once every gap is filled" fill_and_check
too_long() { seq 201 > "$TMP/p/AGENTS.md"; rejects_with 'AGENTS.md has 201 lines'; }
check "check.sh rejects an AGENTS.md over 200 lines" too_long

# 5. Links
check "relative links resolve" bash "$SKILL/check.sh" --links-only "$ROOT"

(( failed )) && { echo "verify: FAILED"; exit 1; }
echo "verify: all green"
