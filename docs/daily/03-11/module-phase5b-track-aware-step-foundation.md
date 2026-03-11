# Module Log - Phase 5B Track-Aware Step Foundation

## Date
- 2026-03-11

## Scope
- Phase 5B track-aware step foundation을 구현하고 beginner/core track resume 기반과 step schema 확장을 고정했다.

## Adjustments
- `StepProgressSaver`에 `CurrentTrack`, `PreKinematics.LastCompletedStep`, `CoreKinematics.LastCompletedStep` 키 구조를 추가
- 기존 parameterless progress API는 `core_kinematics` wrapper로 유지해 기존 Core Track 호출부를 깨지 않도록 정리
- `AppController`가 현재 track 기준으로 resume/save를 수행하도록 보정
- `TutorStepConfig`에 beginner mode 관련 표시 플래그와 `BeginnerLeftContent` enum을 추가
- `InteractionType`에 `ObserveMotion`, `CompareArc`, `CompareCombination`, `TargetMatch`를 추가
- `OnboardingManager`에 beginner/core track 저장 진입 메서드를 추가
- `TrackAwareProgressTests`를 추가하고 EditMode `53/53 passed`를 확인

## Self-Review
- App facade 책임은 유지하고, track persistence는 `StepProgressSaver`에 머물도록 제한했다.
- UI/Data schema 확장은 했지만 실제 beginner 분기 UI 노출과 `L0~L3` 자산 연결은 아직 하지 않아 Phase 5E 범위와 충돌하지 않게 유지했다.
- 기존 Core Track `S1~S8` resume 흐름은 wrapper와 default track 보정으로 backward compatible 하게 유지했다.

## Updated Files
- `Assets/Scripts/App/StepProgressSaver.cs`
- `Assets/Scripts/App/AppController.cs`
- `Assets/Scripts/UI/OnboardingManager.cs`
- `Assets/Scripts/UI/Data/TutorStepConfig.cs`
- `Assets/Scripts/UI/Data/InteractionType.cs`
- `Assets/Scripts/UI/Data/BeginnerLeftContent.cs`
- `Assets/Tests/EditMode/TrackAwareProgressTests.cs`
- `docs/status/PROJECT-STATUS.md`
- `docs/status/PHASE-EXECUTION-BOARD.md`
- `docs/weekly/2026-W11.md`
