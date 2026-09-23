#!/usr/bin/env bash
# Copies template/ into a target directory. Refuses to overwrite: when any
# destination file already exists it lists them all and exits 1 before copying.
#
# Template paths are stored inert, so this repo never loads them as its own
# config: a path segment starting with "dot-" becomes ".", and a trailing
# ".tmpl" is dropped (dot-claude/ -> .claude/, AGENTS.md.tmpl -> AGENTS.md).
set -euo pipefail

TEMPLATE="$(cd "$(dirname "${BASH_SOURCE[0]}")/template" && pwd)"
TARGET="${1:?usage: scaffold.sh <target-dir>}"
mkdir -p "$TARGET"
TARGET="$(cd "$TARGET" && pwd)"

dest_of() {
  local out="" seg
  local -a segs
  IFS='/' read -r -a segs <<< "$1"
  for seg in "${segs[@]}"; do
    [[ $seg == dot-* ]] && seg=".${seg#dot-}"
    out="${out:+$out/}$seg"
  done
  printf '%s\n' "${out%.tmpl}"
}

mapfile -t files < <(cd "$TEMPLATE" && find . -type f | sed 's|^\./||' | sort)

conflicts=()
for f in "${files[@]}"; do
  d="$(dest_of "$f")"
  if [[ -e "$TARGET/$d" ]]; then conflicts+=("$d"); fi
done
if (( ${#conflicts[@]} )); then
  printf 'refusing to overwrite; already in %s:\n' "$TARGET" >&2
  printf '  %s\n' "${conflicts[@]}" >&2
  exit 1
fi

for f in "${files[@]}"; do
  d="$(dest_of "$f")"
  mkdir -p "$TARGET/$(dirname "$d")"
  cp "$TEMPLATE/$f" "$TARGET/$d"
  printf 'created %s\n' "$d"
done
