#!/usr/bin/env bash
set -euo pipefail

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"

required_files=(
  "AGENTS.md"
  "CLAUDE.md"
  "HARNESS-MANIFEST.yaml"
  "harness/REGISTRY.md"
  "harness/code-health-audit.md"
  "harness/change-class-doc-sync.md"
  "harness/session-retro.md"
  "harness/socratic-review.md"
  ".claude/README.md"
  ".claude/commands/CLAUDE.md"
  ".claude/commands/intake.md"
  ".claude/commands/impact-map.md"
  ".claude/commands/evidence-review.md"
  ".claude/commands/handoff.md"
  ".claude/commands/self-review.md"
  ".claude/skills/CLAUDE.md"
  ".claude/skills/README.md"
  ".claude/skills/meta/task-intake-router/SKILL.md"
  ".claude/skills/meta/change-impact-map/SKILL.md"
  ".claude/skills/meta/evidence-review/SKILL.md"
  ".claude/skills/meta/session-handoff/SKILL.md"
  "scripts/check-skills.sh"
  "scripts/check-harness.sh"
)

missing=0

for rel in "${required_files[@]}"; do
  if [[ ! -f "$ROOT/$rel" ]]; then
    echo "MISSING: $rel"
    missing=1
  fi
done

if ! grep -q 'task-intake-router' "$ROOT/harness/REGISTRY.md"; then
  echo "REGISTRY MISSING META SKILL: task-intake-router"
  missing=1
fi

if ! grep -q 'change-class-doc-sync' "$ROOT/HARNESS-MANIFEST.yaml"; then
  echo "MANIFEST MISSING HARNESS: change-class-doc-sync"
  missing=1
fi

if ! bash "$ROOT/scripts/check-skills.sh" >/dev/null; then
  echo "INVALID: skill structure"
  missing=1
fi

if [[ $missing -ne 0 ]]; then
  echo
  echo "Harness check failed."
  exit 1
fi

echo "Harness check passed."
