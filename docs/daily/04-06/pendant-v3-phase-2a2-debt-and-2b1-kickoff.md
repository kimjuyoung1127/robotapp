# Pendant V3 Phase 2A-2 Debt + 2B-1 Kickoff

## Date
- 2026-04-06 (KST)

## Summary
- `2A-2` 우측 `StatusCard + CoordStrip` 구현 뒤 남아 있던 시각 검증 debt를 정리하기 시작했다.
- `RobotControlV3` 진입 경로를 에디터 검증 기준으로 `Onboarding -> RobotLibrary -> RobotControlV3`까지 연결되게 얇은 scene preference 레이어를 추가했다.
- `2B-1` 범위로 `EasyMotion` 패널 시안 자산과 controller를 추가하고, desktop/tablet host를 분리해 이후 패널들이 같은 body를 덮어쓰지 않도록 구조를 바꿨다.
- `RobotControlV3DebugBridge`는 `exec`에서 바로 읽을 수 있는 라우팅/문서 상태 확인용 callable을 확장했다.

## Updated Files
- `Assets/Scripts/App/RobotControlScenePreference.cs`
- `Assets/Scripts/App/RobotControlV3DebugBridge.cs`
- `Assets/Scripts/UI/RobotControlV3/EasyMotionController.cs`
- `Assets/Scripts/UI/RobotControlV3/ConnectionHomeController.cs`
- `Assets/Scripts/UI/RobotControlV3/StatusCardController.cs`
- `Assets/Scripts/UI/RobotControlV3/PendantV3Document.cs`
- `Assets/Scripts/UI/RobotControlV3/PendantV3ShellStateController.cs`
- `Assets/Scripts/UI/RobotControlV3/PendantV3ShellStateController.State.cs`
- `Assets/Scripts/UI/RobotLibrary/RobotLibraryManager.cs`
- `Assets/Scripts/UI/RobotLibrary/RobotDetailDrawer.cs`
- `Assets/Scripts/UI/RobotControlV3/CLAUDE.md`
- `Assets/UI/PendantV3/pendant-v3.uxml`
- `Assets/UI/PendantV3/pendant-v3.uss`
- `Assets/UI/PendantV3/easy-motion-panel.uxml`
- `Assets/UI/PendantV3/easy-motion-panel.uss`
- `Assets/Editor/KineTutor3D/PendantV3SceneBuilder.cs`

## What Changed
- `RobotControlScenePreference`
  - 기본 `RobotControl` 진입 씬을 선택하는 얇은 preference 레이어를 추가했다.
  - 에디터 검증 기준 기본값은 `V3` 선호로 두고, 실제 scene id는 `GetPreferredSceneId()`로 읽게 했다.
- `RobotLibrary -> RobotControl`
  - `RobotLibraryManager`와 `RobotDetailDrawer`의 Robot Control 진입 경로를 `SceneId.RobotControl` 고정 대신 `RobotControlScenePreference.GetPreferredSceneId()`로 바꿨다.
- `RobotControlV3DebugBridge`
  - `GetSceneRouteSummary`, `SetPreferV3Route`를 추가했다.
  - `GetDocumentDebugSummary()`에 `UIDocument` 루트/호스트 존재 여부/descendant 수를 같이 넣어 `exec` 한 번으로 현재 V3 루트 상태를 읽게 했다.
- `EasyMotion`
  - `easy-motion-panel.uxml` / `.uss`와 `EasyMotionController.cs`를 추가했다.
  - Home / Ready / Folded / Zero + 그리퍼 열기 / 닫기 + 미리보기 / 실제이동 시안까지 포함했다.
  - preview state에 따라 프리셋/그리퍼/실행 버튼 활성 규칙을 나눴다.
- `Desktop/Tablet host split`
  - `WorkPanelBody`와 `BottomSheetBody` 안에 `HomePanelHost`, `EasyMotionPanelHost`, `HomeSheetHost`, `EasyMotionSheetHost`를 따로 두어 `2A-1`과 `2B-1`이 같은 host를 서로 `Clear()`하지 않게 바꿨다.
- `UIDocument bootstrap`
  - `PendantV3Document`에 `DefaultExecutionOrder(-1000)`를 부여했다.
  - scene builder가 `panelSettings`, `rootVisualTree`, 각 panel template reference를 serialized field에 직접 채워 넣게 보강했다.

## Verification
- `pwsh -NoLogo -NoProfile -File ./docs/ref/product/pendant-v3/check-v3-static.ps1`
  - pass, 기존 warning group 1건 유지: `RegisterValueChangedCallback` + `SetValueWithoutNotify`
- `unityctl status --project C:\Users\ezen601\Desktop\Jason\robotapp2 --wait --json`
  - Ready
- `unityctl check --project C:\Users\ezen601\Desktop\Jason\robotapp2 --type compile --json`
  - 반복적으로 pass/compile in progress 상태를 확인했고, 최종 Ready 루프 기준 compile pass를 확보했다
- `unityctl exec --project C:\Users\ezen601\Desktop\Jason\robotapp2 --code "KineTutor3D.EditorTools.PendantV3SceneBuilder.AuthorSceneSafe()" --json`
  - `RobotControlV3.unity` 저장 확인
- `unityctl exec list-callables --project C:\Users\ezen601\Desktop\Jason\robotapp2 --filter 'RobotControlV3DebugBridge' --json`
  - `GetSceneRouteSummary`, `SetPreferV3Route` 노출 확인
- `unityctl exec --project C:\Users\ezen601\Desktop\Jason\robotapp2 --code "KineTutor3D.App.RobotControlV3DebugBridge.GetSceneRouteSummary()" --json`
  - `preferV3=True; scene=RobotControlV3`
- `unityctl exec --project C:\Users\ezen601\Desktop\Jason\robotapp2 --code "KineTutor3D.App.RobotControlV3DebugBridge.GetDocumentDebugSummary()" --json`
  - `panel=PendantV3PanelSettings; tree=pendant-v3; rootChildren=1; rootName=PendantV3Root-container; bridge=PendantV3Document; robotName=FR5 Pendant V3; homeHost=True; easyHost=True; context=True; descendants=101`

## Current Risk / Debt
- `unityctl screenshot capture --view game --include-overlay-ui` 결과는 여전히 검게 저장된다.
  - 따라서 `2A-2` 시각 debt는 "루트가 안 붙는다" 단계는 지나갔지만, "GameView 캡처가 검다" 단계가 남아 있다.
- `GetDocumentDebugSummary()` 기준 `easyHome=False`가 계속 남아 있다.
  - 이는 `EasyMotion` panel clone이 실제 runtime query 기준으로는 아직 안 잡히는 상태일 수 있다.
  - root/host 자체는 보이므로, 다음 세션에서는 `EasyMotionController` 초기화 타이밍 또는 host child build 상태를 더 좁혀야 한다.
- 콘솔의 `[unityctl] IPC connection error: Pipe closed before full message was read.`는 계속 남는다.
  - 현재는 프로젝트 코드 에러보다 `unityctl` 통신 잡음으로 판단한다.

## Next Slice
- `2A-2` debt closure:
  - `EasyMotion` host child count와 실제 visual tree clone 여부를 안정적으로 확인
  - GameView black capture가 panel 미표시인지, 캡처 경로 문제인지 분리
- `2B-1` finish:
  - `EasyMotion` panel runtime visibility를 desktop/tablet 둘 다 확정
  - 필요 시 `DryRun` / preview copy와 badge 문구를 상태별로 더 세분화
