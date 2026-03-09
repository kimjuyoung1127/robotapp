# 스킬-문서 매트릭스

스킬, 코드 위치, 필수 문서, 수용 기준의 매핑.

| skill | target_module | primary_code_paths | required_docs | dependent_skills | acceptance_checks |
|-------|--------------|-------------------|---------------|-----------------|------------------|
| math-module-add | Math | Assets/Scripts/Math/*.cs | docs/ref/dh-reference.md, docs/ref/test-reference-values.md | editmode-test-add | 컴파일 + EditMode 테스트 통과 |
| dh-algorithm-add | Kinematics | Assets/Scripts/Kinematics/*.cs, Assets/Tests/EditMode/DHStandardTests.cs, Assets/Tests/EditMode/FKTests.cs | docs/ref/dh-reference.md, docs/ref/coordinate-mapping.md, docs/ref/test-reference-values.md | editmode-test-add | 수치 검증 허용 오차 만족 + EditMode 테스트 통과 |
| robot-template-add | Templates | Assets/Scripts/Templates/*.cs, Assets/Tests/EditMode/Template2DOF_RRTests.cs | docs/ref/test-reference-values.md | dh-algorithm-add, editmode-test-add | Template2DOF_RR 로드/FK 동작 확인 + 템플릿 테스트 통과 |
| tutor-step-add | UI | Assets/Scripts/UI/StepTutorPanel.cs, Assets/Scripts/UI/DHTableEditor.cs, Assets/Scripts/UI/TemplateSelector.cs, Assets/Scripts/UI/MatrixDisplay.cs, Assets/Scripts/UI/UiRuntimeStyle.cs | docs/ref/architecture-diagrams.md, docs/ref/USER-FLOW.md | robot-template-add | `theta` read-only + `d/a/alpha` 편집 반영 + `A1/A2/T02` 실시간 갱신 + 4영역 UI surface 유지 |
| scene-scaffold | Visualization | Assets/Scripts/Visualization/*.cs, Assets/Scenes/Main.unity, Assets/Tests/PlayMode/VisualizationSmokeTests.cs | docs/ref/coordinate-mapping.md, docs/ref/architecture-diagrams.md | editmode-test-add | canonical frame(`frame_0/frame_1/Frame_EE`) ownership 유지 + donor mesh source 정책 유지 + FK와 3D 위치 일치 + 학습용 카메라 구도 유지 |
| student-friendly-ux | UI/UX | Assets/Scripts/UI/*.cs, Assets/Scripts/UI/Data/*.cs | docs/ref/tutor-step-plan.md, docs/ref/USER-FLOW.md | tutor-step-add, scene-scaffold | Step 매트릭스/게이트/온보딩 동작 일치 |
| editmode-test-add | Tests | Assets/Tests/EditMode/*.cs | docs/ref/test-reference-values.md | - | 테스트 전수 통과 |
| pre-commit-validate | Ops | .github/workflows/unity-tests.yml | CLAUDE.md, docs/ref/unity-official-evidence-phase01.md | editmode-test-add | 로컬 테스트 + CI 워크플로우 기준 검증 |
| debug-success-capture | Ops/QA | Assets/Scenes/Main.unity, Assets/Tests/PlayMode/*.cs, Assets/Tests/PlayMode/*.asmdef | docs/status/PROJECT-STATUS.md, docs/status/PHASE-EXECUTION-BOARD.md | pre-commit-validate, student-friendly-ux | 원인-조치-검증-재발방지 4항목 기록 + 회귀 검증 경로 보존 |
| asmdef-setup | Ops | Assets/**/*.asmdef | docs/ref/unity-official-evidence-phase01.md | unity-official-docs | asmdef 규칙/의존성 DAG 확인 |
| unity-official-docs | Ops | .claude/skills/kinetutor-guide/ops/unity-official-docs/* | .claude/skills/kinetutor-guide/ops/unity-official-docs/references/index.md, docs/ref/unity-official-evidence-phase01.md | asmdef-setup, pre-commit-validate | 결론-근거-적용규칙-버전메모 형식 충족 |
| sprint-docs-sync | Meta | docs/status/*.md | 모든 상태 문서 | - | 보드/매트릭스/상태 문서 일치 |

## 전역 규칙
1. 공식 출처는 `docs.unity3d.com`만 허용한다.
2. 공식 링크 없는 asmdef/test/compile/serialization 규칙 추가를 금지한다.
3. 문서 상태와 실제 프로젝트 상태가 다르면 실제 상태를 우선한다.
