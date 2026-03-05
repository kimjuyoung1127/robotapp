# 스킬-문서 매트릭스

스킬, 코드 터치포인트, 필요 참조 문서 간 매핑.

| skill | target_module | primary_code_paths | required_docs | dependent_skills | acceptance_checks |
|-------|--------------|-------------------|---------------|-----------------|------------------|
| math-module-add | Math | Assets/Scripts/Math/*.cs | docs/ref/dh-reference.md | editmode-test-add | 컴파일 + EditMode 테스트 통과 |
| dh-algorithm-add | Kinematics | Assets/Scripts/Kinematics/*.cs | docs/ref/dh-reference.md, docs/ref/coordinate-mapping.md | editmode-test-add | 수치 정확도 허용 범위 내 |
| robot-template-add | Templates | Assets/Scripts/Templates/*.cs | docs/ref/test-reference-values.md | dh-algorithm-add, editmode-test-add | 템플릿 로드, FK 업데이트 안정 |
| tutor-step-add | UI | Assets/Scripts/UI/StepTutorPanel.cs | docs/ref/architecture-diagrams.md | robot-template-add | 스텝 텍스트 + 행렬 표시 업데이트 |
| editmode-test-add | Tests | Assets/Tests/EditMode/*.cs | docs/ref/test-reference-values.md | - | 테스트 통과, 대상 메서드 커버리지 |
| pre-commit-validate | Ops | - (교차 관심사) | CLAUDE.md | editmode-test-add | 모든 검증 통과 |
| unity-official-docs | Ops | .claude/skills/kinetutor-guide/ops/unity-official-docs/* | .claude/skills/kinetutor-guide/ops/unity-official-docs/references/index.md | asmdef-setup, pre-commit-validate | 결론-근거-적용규칙-버전메모 형식 충족 |
| sprint-docs-sync | Meta | docs/status/*.md | 모든 상태 문서 | - | 제로 드리프트 |

## 전역 규칙
- 모듈 Source of Truth: `Assets/Scripts/` 폴더 구조
- 보드의 module 집합 = 매트릭스의 target_module 집합 유지
- required_docs가 누락된 경우 manual_required로 보고
- 공식 출처 정책: `docs.unity3d.com` 외 출처 인용 금지
