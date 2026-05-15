# .claude/skills/

KineTutor3D Claude Code 대화형 개발 스킬 에셋.

## 구조
- `kinetutor-guide/` — 도메인 지식 스킬 (core, kinematics, templates, ui, test, ops)
  - `core/` — math-module-add
  - `kinematics/` — dh-algorithm-add
  - `templates/` — robot-template-add
  - `ui/` — tutor-step-add, scene-scaffold, student-friendly-ux, ui-design-system, scene-ui-visibility, viewbuilder-extract
  - `test/` — editmode-test-add
  - `ops/` — pre-commit-validate, asmdef-setup, unity-official-docs, debug-success-capture
- `meta/` — 문서 오케스트레이션 스킬 (sprint-docs-sync)
  - `meta/` — 범용 운영 스킬 (`task-intake-router`, `change-impact-map`, `evidence-review`, `session-handoff`)

## 빠른 선택
- 새 요청 분류: `meta/task-intake-router`
- 영향 범위 축소: `meta/change-impact-map`
- 완료 전 근거 점검: `meta/evidence-review`
- 다음 세션 인계: `meta/session-handoff`
- FR5 도메인 구현/디버그: 기존 `kinetutor-guide/*`

## 스킬 포맷
모든 스킬은 `SKILL.md` (YAML front matter + 7개 섹션):
- Front matter: `name:`, `description:` (트리거 키워드 포함)
- 섹션: Trigger / Input Context / Read First / Do / Do Not / Validation / Output Template

## 교차 스킬 호출
```
robot-template-add → dh-algorithm-add + editmode-test-add
tutor-step-add → robot-template-add
student-friendly-ux → tutor-step-add + scene-scaffold
pre-commit-validate → editmode-test-add (검증만)
scene-scaffold → tutor-step-plan.md 참조 + scene-ui-visibility
scene-ui-visibility → ui-design-system
asmdef-setup → architecture-diagrams.md 참조
asmdef-setup → unity-official-docs 참조
pre-commit-validate → unity-official-docs 참조
debug-success-capture → pre-commit-validate + student-friendly-ux
viewbuilder-extract → ui-design-system + scene-ui-visibility
```

## 자동화 관계
- 스킬 = 변경 시점에서 드리프트 방지 (예방적)
- 자동화 = 야간 드리프트 감지 (탐지적)
- 상호 보완 구조
