# .claude/skills/

KineTutor3D Claude Code 대화형 개발 스킬 에셋.

## 구조
- `kinetutor-guide/` — 도메인 지식 스킬 (core, kinematics, templates, ui, test, ops)
  - `core/` — math-module-add
  - `kinematics/` — dh-algorithm-add
  - `templates/` — robot-template-add
  - `ui/` — tutor-step-add, scene-scaffold
  - `test/` — editmode-test-add
  - `ops/` — pre-commit-validate, asmdef-setup
- `meta/` — 문서 오케스트레이션 스킬 (sprint-docs-sync)

## 스킬 포맷
모든 스킬은 `SKILL.md` (YAML front matter + 7개 섹션):
- Front matter: `name:`, `description:` (트리거 키워드 포함)
- 섹션: Trigger / Input Context / Read First / Do / Do Not / Validation / Output Template

## 교차 스킬 호출
```
robot-template-add → dh-algorithm-add + editmode-test-add
tutor-step-add → robot-template-add
pre-commit-validate → editmode-test-add (검증만)
scene-scaffold → tutor-step-plan.md 참조
asmdef-setup → architecture-diagrams.md 참조
```

## 자동화 관계
- 스킬 = 변경 시점에서 드리프트 방지 (예방적)
- 자동화 = 야간 드리프트 감지 (탐지적)
- 상호 보완 구조
