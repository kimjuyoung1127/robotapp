# KineTutor3D 프로젝트 인덱스

KineTutor3D 작업 시작 시 가장 먼저 읽는 진입 문서입니다.
이 문서만 읽어도 현재 단계, 규칙, 다음 행동을 빠르게 파악할 수 있게 유지합니다.

## 저장소 경계
- Write Repo: `C:\Users\ezen601\Desktop\Jason\robotapp2`

## 시작 순서 (필수)
1. `CLAUDE.md` (이 파일)
2. `KineTutor3D_Execution_Plan.md`
3. `docs/status/PROJECT-STATUS.md`
4. `docs/status/PHASE-EXECUTION-BOARD.md`
5. `docs/status/SKILL-DOC-MATRIX.md`

## 현재 상태 (2026-03-09)
- Phase 0: Done
- Phase 1: Done
- Phase 2: Done
- Phase 3 (Template 2DOF + App/UI): Done
- Phase 4 (Visualization core): InProgress
- Phase 6 (CI/CD): InProgress

최근 확정 사항:
- Phase 3 확장 완료: `TemplateSelector`, `DHTableEditor`, `MatrixDisplay` 실동작 연결
- Phase 3 QA 마감: `Main.unity` 활성, Build index `0`, 프로젝트 코드 에러 `0`(`MCP` 로그 제외)
- Phase 4 확장: `frame_0`/`frame_1`을 canonical frame object로 통합, `Frame_EE` 유지
- Phase 4 확장: `ScaraRobot.prefab`을 hidden donor source로 두고 `BaseVisual`/`Link0Visual`/`Link1Visual`/`EndEffectorVisualMesh`에 mesh-only 복제
- 학습 화면 MVP 정리: `TopBar`/`LeftPanel`/`RightPanel`/`BottomBar` 4영역으로 정리하고 런타임 디버그성 흰 패널/텍스트를 공통 스타일 surface로 대체
- Phase 4 디버그: Built-in에서 URP(`com.unity.render-pipelines.universal@17.0.4`)로 전환하고 `GraphicsSettings`/`QualitySettings`를 `URP-Default.asset`에 고정
- Camera 정리: `Main Camera`를 Solid Color + 2DOF 학습 구도로 조정하고 donor mesh local offset/scale 보정 경로를 `RobotRenderer`에 고정
- Unity Test Runner 결과: EditMode `45/45`, PlayMode `17/17`
- CI 초안 추가: `.github/workflows/unity-tests.yml`

## 실행 규칙 (MUST)
1. 기존 코드/타입/유틸 우선 재사용, 중복 구현 금지
2. `Assets/Scripts/` 폴더 구조를 모듈 Source of Truth로 사용
3. Math/Types/Kinematics는 pure C# `double` 유지, `UnityEngine` 참조 금지
4. NaN/Infinity 입력은 FK 계산 전에 차단
5. `theta`는 Slider 단일 소스, DHTable에서는 read-only
6. 문서와 코드 상태가 다르면 코드/테스트 실제 상태를 우선
7. 명시 요청 없이는 임의 Git 파괴 명령 금지

## Skill 인덱스 (.claude/skills)
| # | Skill | Trigger 키워드 | 경로 |
|---|---|---|---|
| 1 | math-module-add | math, vector, matrix | `kinetutor-guide/core/math-module-add/` |
| 2 | dh-algorithm-add | DH, FK, kinematics | `kinetutor-guide/kinematics/dh-algorithm-add/` |
| 3 | robot-template-add | template, 2DOF/SCARA | `kinetutor-guide/templates/robot-template-add/` |
| 4 | tutor-step-add | step tutor, learning step | `kinetutor-guide/ui/tutor-step-add/` |
| 5 | editmode-test-add | editmode test | `kinetutor-guide/test/editmode-test-add/` |
| 6 | pre-commit-validate | pre-commit, validate | `kinetutor-guide/ops/pre-commit-validate/` |
| 7 | sprint-docs-sync | docs sync | `meta/sprint-docs-sync/` |
| 8 | asmdef-setup | asmdef, assembly definition | `kinetutor-guide/ops/asmdef-setup/` |
| 9 | scene-scaffold | Main.unity, scene scaffold | `kinetutor-guide/ui/scene-scaffold/` |
| 10 | unity-official-docs | Unity 공식문서 근거 | `kinetutor-guide/ops/unity-official-docs/` |

## Skill 의존 규칙
- `robot-template-add` -> `dh-algorithm-add` + `editmode-test-add`
- `tutor-step-add` -> `robot-template-add`
- `asmdef-setup` -> `unity-official-docs`
- `pre-commit-validate` -> `editmode-test-add` + `unity-official-docs`

## Source of Truth 문서
- 실행 계획: `KineTutor3D_Execution_Plan.md`
- 운영 상태: `docs/status/PROJECT-STATUS.md`
- 실행 보드: `docs/status/PHASE-EXECUTION-BOARD.md`
- 스킬 매트릭스: `docs/status/SKILL-DOC-MATRIX.md`
- 아키텍처: `docs/ref/architecture-diagrams.md`
- 사용자 흐름: `docs/ref/USER-FLOW.md`
- 튜터 스텝: `docs/ref/tutor-step-plan.md`

## 테스트 표준
- Local 우선 순서:
1. EditMode 전체
2. PlayMode 스모크
- 현재 기준:
1. EditMode: 45 passed
2. PlayMode: 17 passed
- CI 워크플로우:
1. `.github/workflows/unity-tests.yml`
2. runner: `self-hosted`, `windows`
3. `UNITY_EXE` 환경변수 필요

## 즉시 다음 작업
1. Phase 4 Visualization 계속: donor mesh 정렬/스케일 세부값 마감, 실제 Game View 수동 QA 마감
2. PR에서 `unity-tests` 워크플로우 실주행 1회 확인
3. `Assembly-CSharp.csproj` 로컬 빌드 불일치 원인 문서화
