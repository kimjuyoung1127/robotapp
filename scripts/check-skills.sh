#!/usr/bin/env bash
set -euo pipefail

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
SKILLS_DIR="$ROOT/.claude/skills"

missing=0
warned=0

check_required_heading() {
  local file="$1"
  local heading="$2"
  if ! grep -q "^## ${heading}$" "$file"; then
    echo "MISSING SECTION: ${file#$ROOT/} -> ${heading}"
    missing=1
  fi
}

has_any_heading() {
  local file="$1"
  shift
  local heading
  for heading in "$@"; do
    if grep -Eq "$heading" "$file"; then
      return 0
    fi
  done
  return 1
}

while IFS= read -r skill_file; do
  line_count="$(wc -l < "$skill_file" | tr -d ' ')"

  if [[ "$skill_file" == *"/meta/"* ]]; then
    if ! grep -q '^---$' "$skill_file"; then
      echo "MISSING FRONTMATTER: ${skill_file#$ROOT/}"
      missing=1
    fi

    if ! grep -q '^name:' "$skill_file"; then
      echo "MISSING NAME: ${skill_file#$ROOT/}"
      missing=1
    fi

    if ! grep -q '^description:' "$skill_file"; then
      echo "MISSING DESCRIPTION: ${skill_file#$ROOT/}"
      missing=1
    fi

    check_required_heading "$skill_file" "Trigger"
    check_required_heading "$skill_file" "Input Context"
    check_required_heading "$skill_file" "Read First"
    check_required_heading "$skill_file" "Do"
    check_required_heading "$skill_file" "Do Not"
    check_required_heading "$skill_file" "Validation"
    check_required_heading "$skill_file" "Output Template"
  else
    if ! grep -q '^# ' "$skill_file" && ! grep -q '^---$' "$skill_file"; then
      echo "MISSING TITLE OR FRONTMATTER: ${skill_file#$ROOT/}"
      missing=1
    fi

    if ! has_any_heading "$skill_file" '^## Trigger$' '^## Trigger Keywords$' '^## Overview$'; then
      echo "MISSING ENTRY SECTION: ${skill_file#$ROOT/}"
      missing=1
    fi

    if ! has_any_heading "$skill_file" '^## Read First$' '^## Token Sources \(Assets/Scripts/UI/\)$'; then
      echo "MISSING READ-FIRST-LIKE SECTION: ${skill_file#$ROOT/}"
      missing=1
    fi

    if ! has_any_heading "$skill_file" '^## Do$' '^## Do \(' '^## Update Workflow$' '^## MUST Rules$' '^## Guardrails$'; then
      echo "MISSING ACTION SECTION: ${skill_file#$ROOT/}"
      missing=1
    fi

    if ! has_any_heading "$skill_file" '^## Validation$' '^## Validation Order$' '^## Validation Checklist$' '^## Acceptance Checks$'; then
      echo "MISSING VALIDATION SECTION: ${skill_file#$ROOT/}"
      missing=1
    fi
  fi

  if (( line_count > 260 )); then
    echo "WARN LONG SKILL: ${skill_file#$ROOT/} ($line_count)"
    warned=1
  fi
done < <(find "$SKILLS_DIR" -name 'SKILL.md' | sort)

if [[ $missing -ne 0 ]]; then
  echo
  echo "Skill check failed."
  exit 1
fi

if [[ $warned -ne 0 ]]; then
  echo "Skill check passed with warnings."
  exit 0
fi

echo "Skill check passed."
