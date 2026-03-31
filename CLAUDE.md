# KineTutor3D Claude Index

KineTutor3D에서 Claude 계열 에이전트가 가장 먼저 읽는 루트 허브 문서입니다.
핵심 규칙은 짧게 유지하고, 실제 작업은 하위 `CLAUDE.md`와 정식 레퍼런스로 내려가도록 구성합니다.

## 저장소 경계
- Write Repo: `.` (저장소 루트)

## 시작 순서
1. `AGENTS.md` 또는 `CLAUDE.md`
2. `docs/ref/architecture-mermaid.md`
3. `docs/ref/project-flow-code-review.md`
4. `docs/ref/csharp-master-harness.md` (C# 생성/수정 시 상위 운영 규칙)
5. `docs/ref/code-patterns.md` (구현 디테일과 패턴)
6. `docs/status/PRODUCT-DOC-BOARD.md`
7. `docs/ref/PRD.md`
8. `docs/ref/WIREFRAME.md`
9. `docs/ref/PRODUCT-ROADMAP.md`
10. `docs/ref/phase5-implementation-plan.md` (Phase 5 구현/검수 시)

## 현재 구조 요약
- 현재 씬 흐름은 `Boot -> Onboarding -> RobotLibrary -> {MathReadiness, Sandbox, RobotControl}` 입니다.
- `RobotLibrary`가 메인 진입점입니다.
- `AppController`는 Guided Lesson/MathReadiness의 퍼블릭 앱 facade입니다.
- `SandboxSceneCoordinator`는 Sandbox 전용 독립 코디네이터입니다.
- `RobotControlSceneCoordinator`는 멀티로봇 RobotControl facade입니다.
- `RobotRenderer`는 시각화 facade입니다.
- `Home`과 `Main`은 현재 구조 기준으로는 역사적 이름입니다. 최신 판단에는 사용하지 않습니다.

## 핵심 규칙
1. 기존 코드, 타입, 유틸리티를 우선 재사용합니다.
2. `Math`, `Types`, `Kinematics`, `Templates`는 pure C# `double` 기반으로 유지합니다.
3. C# 수정 전에는 `docs/ref/csharp-master-harness.md`와 `docs/ref/code-patterns.md`를 읽고 운영 규칙과 구현 패턴을 맞춥니다.
4. Unity 작업의 기본 도구는 `unityctl`입니다. `unityctl`에 없는 작업만 MCP로 폴백합니다.
5. 문서와 코드가 다르면 현재 코드와 테스트 결과를 우선합니다.
6. 하위 폴더 규칙이 바뀌면 가장 가까운 `AGENTS.md` 또는 `CLAUDE.md`를 같이 갱신합니다.

## Unityctl Quickstart
- 고정 경로:
  `C:\Users\ezen601\Desktop\Jason\unityctl\src\Unityctl.Cli\bin\Debug\net10.0\unityctl.exe`
- 프로젝트 경로:
  `C:\Users\ezen601\Desktop\Jason\robotapp2`

추천 세션 변수:

```powershell
$unityctl = 'C:\Users\ezen601\Desktop\Jason\unityctl\src\Unityctl.Cli\bin\Debug\net10.0\unityctl.exe'
$project = 'C:\Users\ezen601\Desktop\Jason\robotapp2'
```

추천 첫 루프:

```powershell
& $unityctl status --project $project --wait --json
& $unityctl check --project $project --type compile --json
& $unityctl console get-entries --project $project --limit 50 --json
```

자주 쓰는 작업 루프:
- 컴파일 확인: `check --type compile`
- EditMode 테스트: `test --mode edit`
- PlayMode 테스트: `test --mode play`
- 씬/오브젝트 확인: `scene open`, `scene hierarchy`, `scene snapshot`
- Play 검증: `play start`, `console get-entries`, `play stop`
- 런타임 조사: `exec`
- UGUI 조사/조작: `ui find`, `ui get`, `ui toggle`, `ui input`

## 작업별 링크 허브

### 앱 런타임 / 씬 흐름
- 기본 규칙: `Assets/Scripts/App/AGENTS.md`
- 로컬 요약: `Assets/Scripts/App/CLAUDE.md`
- 현재 전체 플로우: `docs/ref/project-flow-code-review.md`

### RobotControl / 멀티로봇 / 실기 연동
- 공용 RobotControl 런타임: `Assets/Scripts/App/Fairino/CLAUDE.md`
- FR5 실기 참고: `docs/ref/product/robots/fairino-fr5-integration-reference.md`
- UR5e: `Assets/Scripts/App/UniversalRobots/CLAUDE.md`
- Doosan: `Assets/Scripts/App/Doosan/CLAUDE.md`
- Meca500: `Assets/Scripts/App/Mecademic/CLAUDE.md`
- Hand tracking: `Assets/Scripts/App/HandTracking/CLAUDE.md`

### UI / HUD / 온보딩 / 튜터 흐름
- 기본 규칙: `Assets/Scripts/UI/AGENTS.md`
- 로컬 요약: `Assets/Scripts/UI/CLAUDE.md`
- UI 데이터 타입: `Assets/Scripts/UI/Data/CLAUDE.md`
- 가이드 레슨 UX: `docs/ref/product/ux/guided-lesson.md`
- Sandbox UX: `docs/ref/product/ux/sandbox.md`

### Visualization / donor / gizmo / trail
- 기본 규칙: `Assets/Scripts/Visualization/AGENTS.md`
- 로컬 요약: `Assets/Scripts/Visualization/CLAUDE.md`
- 공용 시각화 컴포넌트: `Assets/Scripts/Visualization/Shared/CLAUDE.md`

### Editor 도구 / QA / unityctl exec helper
- 에디터 도구 요약: `Assets/Editor/KineTutor3D/CLAUDE.md`
- CLI helper 요약: `Assets/Editor/KineTutor3D/CliTools/CLAUDE.md`

### 테스트
- 테스트 루트: `Assets/Tests/CLAUDE.md`
- EditMode 규칙: `Assets/Tests/EditMode/CLAUDE.md`
- PlayMode 규칙: `Assets/Tests/PlayMode/CLAUDE.md`
- CliTools 테스트: `Assets/Tests/EditMode/CliTools/CLAUDE.md`

### 제품 문서 / 계획 / 상태
- 상태 보드: `docs/status/PRODUCT-DOC-BOARD.md`
- PRD: `docs/ref/PRD.md`
- Wireframe: `docs/ref/WIREFRAME.md`
- Roadmap: `docs/ref/PRODUCT-ROADMAP.md`
- 현재 구현 범위: `docs/ref/product/roadmap/current-feature-checklist.md`

## 최신 기준으로 봐야 하는 문서
- 시스템 전체 구조: `docs/ref/architecture-mermaid.md`
- 코드 리뷰 기준 플로우: `docs/ref/project-flow-code-review.md`
- C# 상위 운영 규칙: `docs/ref/csharp-master-harness.md`
- 상세 다이어그램: `docs/ref/architecture-diagrams.md`
- C# 패턴: `docs/ref/code-patterns.md`

## 레거시/히스토리 주의
- `Home`, `Main`, `HomeContinueHub`, `MainLearningTabs`가 보이는 설명은 대체로 히스토리성 기록입니다.
- 현재 구조 판단은 반드시 `SceneCatalog`, `BootSceneRouter`, `RobotLibrary`, `SandboxSceneCoordinator`, `RobotControlSceneCoordinator` 기준으로 합니다.
- 오래된 이름이 남은 문서를 발견하면 최신 구조 기준으로 갱신하거나, 최소한 historical note를 남깁니다.
