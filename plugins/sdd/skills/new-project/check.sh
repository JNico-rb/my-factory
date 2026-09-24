#!/usr/bin/env bash
# Checks a scaffolded project and lists each failure; exits 1 if there is any:
#   - no GAP comment left, in any file;
#   - AGENTS.md at most 200 lines (Anthropic's advice for instruction files: longer ones cost context
#     and lower adherence);
#   - every code dir in .claude/sdd.json exists;
#   - every relative link in the markdown resolves.
# With --links-only, only the last check runs: that is how this repo checks its own docs.
set -euo pipefail

links_only=0
if [[ ${1:-} == --links-only ]]; then links_only=1; shift; fi
TARGET="${1:?usage: check.sh [--links-only] <project-dir>}"
cd "$TARGET"
fail=0

if (( ! links_only )); then
  if grep -rn --exclude-dir=.git --exclude-dir=node_modules 'GAP:' . >&2; then
    echo "^ gaps still open" >&2
    fail=1
  fi

  if [[ -f AGENTS.md ]] && (( $(wc -l < AGENTS.md) > 200 )); then
    echo "AGENTS.md has $(wc -l < AGENTS.md) lines; keep it at 200 or fewer, move detail to docs/ or a skill" >&2
    fail=1
  fi

  if [[ -f .claude/sdd.json ]]; then
    while IFS= read -r dir; do
      [[ -d $dir ]] || { echo "code dir in .claude/sdd.json does not exist: $dir" >&2; fail=1; }
    done < <(tr -d '\r' < .claude/sdd.json | sed -n '/"codeDirs"/,/]/p' | grep -o '"[^"]*"' | grep -v '"codeDirs"' | tr -d '"')
  fi
fi

while IFS= read -r md; do
  dir="$(dirname "$md")"
  # Links outside code: fenced blocks, indented blocks and inline `spans`.
  while IFS= read -r link; do
    path="${link%%#*}"
    [[ -z $path || $path == *://* || $path == mailto:* ]] && continue
    if [[ ! -e "$dir/$path" ]]; then
      echo "broken link in $md: $link" >&2
      fail=1
    fi
  done < <(awk '/^```/ { code = !code; next } !code && !/^(    |\t)/' "$md" \
             | sed 's/`[^`]*`//g' | grep -o '\]([^)]*)' | sed 's/^](//; s/)$//')
done < <(find . -name '*.md' -not -path './.git/*' -not -path '*/node_modules/*')

exit "$fail"
