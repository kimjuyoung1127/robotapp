# Module Log - Phase 5A Runtime Foundation

## Date
- 2026-03-11

## Scope
- Phase 5A runtime foundation을 구현하고 snapshot/update cause 기반을 고정했다.

## Adjustments
- `KinematicsRuntimeState`에 previous state, changed joint index, update cause 필드를 추가
- `RuntimeUpdateCause` enum을 추가하고 `JointAngleChange`, `DhParameterEdit`, `TemplateApply`를 표준 원인으로 사용
- `KinematicsRuntimeService`가 값 변경 직전에 snapshot을 저장하도록 보정
- `AppController` public facade에 previous/current runtime foundation 접근자를 추가
- `RuntimeFoundationTests`를 추가하고 EditMode `50/50 passed`를 확인

## Self-Review
- App facade 책임은 유지되고, snapshot/update cause 계산은 service 내부에 머물도록 유지했다.
- UI/App/Visualization 폴더 책임을 넘지 않도록 Phase 5A 범위를 App runtime foundation으로만 제한했다.
- 기존 FK/DHTable/MatrixDisplay 이벤트 표면은 유지해 Core Track MVP 회귀를 피했다.

## Updated Files
- `Assets/Scripts/App/KinematicsRuntimeState.cs`
- `Assets/Scripts/App/RuntimeUpdateCause.cs`
- `Assets/Scripts/App/KinematicsRuntimeService.cs`
- `Assets/Scripts/App/AppController.cs`
- `Assets/Tests/EditMode/RuntimeFoundationTests.cs`
- `docs/status/PROJECT-STATUS.md`
- `docs/status/PHASE-EXECUTION-BOARD.md`
