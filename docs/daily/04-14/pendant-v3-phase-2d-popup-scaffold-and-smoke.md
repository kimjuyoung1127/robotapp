# Pendant V3 Phase 2D Popup Scaffold And Smoke

## Date
- 2026-04-14 (KST)

## Design Lock
- 이번 `2D` 단위는 `move-confirm / warning / recovery` 3종 body + popup orchestration + focus/close smoke까지만 닫는다.
- `help-panel`, `WhyItMoved`, 실제 `PointMove apply` 인터셉트 정책은 다음 단위로 남긴다.
- 실기 계약 literal(`FAIRINO_FR5`, live IP/port)은 예외 허용하고, popup/viewport UI copy는 asset SSOT로 유지한다.

## Summary
- `PopupCoordinatorV3`에 `move / warning / recovery` popup path를 추가했다.
- popup 제목/요약/확인/취소 문구는 계속 UXML meta copy에서 읽게 유지했다.
- `BtnStopBottom` -> `warning-dialog`, `BtnFaultOverlayReset` -> `recovery-dialog`, debug path `move/warning/recovery`를 열 수 있게 만들었다.
- popup confirm/cancel이 실제로 닫히도록 명시 close handler를 추가했다.

## Updated Files
- `Assets/Scripts/UI/RobotControlV3/PopupCoordinatorV3.cs`
- `Assets/Scripts/App/RobotControlV3DebugBridge.cs`
- `Assets/Editor/KineTutor3D/PendantV3SceneBuilder.cs`
- `Assets/UI/PendantV3/popups/move-confirm.uxml`
- `Assets/UI/PendantV3/popups/warning-dialog.uxml`
- `Assets/UI/PendantV3/popups/recovery-dialog.uxml`
- `Assets/UI/PendantV3/popups/CLAUDE.md`
- `docs/ref/product/pendant-v3/progress-checklist.md`

## Verification
- `unityctl check --type compile`
  - pass
- `unityctl exec ... AuthorSceneSafe()`
  - `saved=True`
- `GetDocumentRootComponentSummary()`
  - `PopupCoordinatorV3` 포함 확인
- play smoke
  - debug `OpenPopupForDebug("move")`
    - `title=이동 실행 확인`
    - `confirm=이동 실행`
  - debug `OpenPopupForDebug("recovery")`
    - `title=복구 순서 안내`
    - `confirm=순서 확인`
  - debug `OpenPopupForDebug("warning")`
    - `title=정지 안내`
  - actual `BtnPopupConfirm` click
    - `popupActive=False`
    - focus=`BtnPopupProbe` 복귀 확인
  - actual `BtnStopBottom` click
    - `PopupCardTitle=정지 안내`

## Self Review
- 역할 경계
  - popup body/copy는 UXML asset에 두고, controller는 orchestration만 담당하게 유지했다.
  - 실기 명령 policy를 popup controller가 직접 먹지 않도록 debug path와 body scaffold 수준에서 멈췄다.
- 남은 리스크
  - `BtnPointApply` 같은 실제 이동 버튼은 아직 popup confirm을 거쳐 dispatch되게 연결하지 않았다.
  - `help-panel`, `WhyItMoved`는 아직 미구현이라 `2D` 전체 완료는 아니다.
