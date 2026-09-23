#!/usr/bin/env bash
# Checks a scaffolded project: no GAP comment left, and every relative link
# in its markdown resolves. Lists each failure and exits 1 if there is any.
set -euo pipefail

TARGET="${1:?usage: check.sh <project-dir>}"
cd "$TARGET"
fail=0

if grep -rn --include='*.md' 'GAP:' . >&2; then
  echo "^ gaps still open" >&2
  fail=1
fi

while IFS= read -r md; do
  dir="$(dirname "$md")"
  # Links outside fenced and indented code blocks.
  while IFS= read -r link; do
    path="${link%%#*}"
    [[ -z $path || $path == *://* || $path == mailto:* ]] && continue
    if [[ ! -e "$dir/$path" ]]; then
      echo "broken link in $md: $link" >&2
      fail=1
    fi
  done < <(awk '/^```/ { code = !code; next } !code && !/^(    |\t)/' "$md" \
             | grep -o '\]([^)]*)' | sed 's/^](//; s/)$//')
done < <(find . -name '*.md' -not -path './.git/*' -not -path '*/node_modules/*')

exit "$fail"
