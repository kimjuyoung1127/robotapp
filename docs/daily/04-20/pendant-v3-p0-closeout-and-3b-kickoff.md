# Pendant V3 P0 Closeout And 3B Kickoff

## 요약
- `Pendant V3` P0 closeout 범위에서 `컨텍스트 패널 마감 -> popup/help policy 연결 -> PointMove polish -> 3B 첫 슬라이스`를 한 턴으로 묶었다.
- direct fresh start 전용 `first-run guide` 1회 노출 정책을 추가했고, `PointMove`는 `MoveL`뿐 아니라 `MoveJ` actual dispatch까지 닫았다.
- `3B`는 full 범위가 아니라 `UI-local Undo/Redo + point draft/local state 확장`까지만 먼저 열었다.

## 반영 파일
- `Assets/Scripts/App/Session/PendantV3LocalState.cs`
- `Assets/Scripts/App/Session/LocalSettingsStore.cs`
- `Assets/Scripts/App/RobotControlEntryPolicy.cs`
- `Assets/Scripts/UI/RobotControlV3/PopupCoordinatorV3.cs`
- `Assets/Scripts/UI/RobotControlV3/HelpPanelController.cs`
- `Assets/Scripts/UI/RobotControlV3/ConnectionHomeController.cs`
- `Assets/Scripts/UI/RobotControlV3/PointMoveController.cs`
- `Assets/Scripts/UI/RobotControlV3/PendantV3ShellStateController.cs`
- `Assets/UI/PendantV3/pendant-v3.uxml`
- `Assets/UI/PendantV3/pendant-v3.uss`
- `Assets/UI/PendantV3/point-move-panel.uxml`
- `Assets/UI/PendantV3/popups/first-run-guide.uxml`
- `Assets/Editor/KineTutor3D/PendantV3SceneBuilder.cs`
- `Assets/Scenes/RobotControlV3.unity`

## 구현 메모
- `first-run guide`
  - `Onboarding -> FR5 V3 바로 열기`에서만 pending
  - guide를 한 번 확인하면 `HasShownFirstRunGuide=true`로 저장
  - `RobotLibrary -> RobotControlV3` resume에서는 띄우지 않는 정책 유지
- `PointMove`
  - `MoveL`은 TCP target draft를 계속 사용
  - `MoveJ`는 joint target draft를 따로 저장하고 `DispatchMoveJ`로 직접 전송
  - `Step◀`는 unsaved point draft가 남아 있으면 discard popup을 거친 뒤 `EasyMotion`으로 돌아가게 연결
- `3B`
  - `Undo/Redo`는 nav/work/tab/coord/speed/point draft 같은 UI-local snapshot만 대상으로 시작
  - autoreconnect와 visualization-side history는 이번 범위에 넣지 않았다

## 검증
- `unityctl check --type compile`: pass
- `unityctl test --mode edit --filter KineTutor3D.Tests.EditMode.LocalSettingsStoreTests`: `2 passed`
- `unityctl test --mode edit --filter KineTutor3D.Tests.EditMode.RobotControlEntryPolicyTests`: `3 passed`
- `unityctl test --mode edit --filter KineTutor3D.Tests.EditMode.RobotControlMotionRuntimeTests`: `3 passed`
- `dotnet build KineTutor3D.Runtime.csproj`: pass
- `dotnet build KineTutor3D.Tests.EditMode.csproj`: pass
- actual smoke
  - `Onboarding -> FR5 V3 바로 열기`: pass
  - `PopupCoordinatorSummary`: `initialized=True`, `popupOpen=True`, `kind=FirstRunGuide`
  - `SetPointMoveMotionKindForDebug("MoveJ") -> ApplyPointMoveForDebug()`: pass
  - `SetPointMoveMotionKindForDebug("MoveL") -> ApplyPointMoveForDebug()`: pass

## 이슈/메모
- 중간에 Unity editor test runner가 `unityctl` IPC를 물고 timeout이 나는 문제가 있었고, editor를 kill 후 재기동해서 검증을 다시 닫았다.
- scene builder가 `firstRunGuideTemplate` serialized reference를 runtime scene에 실제로 꽂지 못한 상태가 한 번 있었고, `AuthorSceneSafe()` 재실행 + scene 저장으로 수정했다.
- same-session direct re-entry에서 guide 비재노출 actual click은 play마다 `BtnOpenRobotControlV3` `globalObjectId`가 바뀌어서 이 턴에서는 완전 자동화까지 닫지 못했다. 대신 entry policy test와 stored `firstRunGuide=True` 상태는 확인했다.

## 3B-2 Reconnect Integration

### 요약
- `3B` 다음 단계로 `UI-local Undo/Redo` 범위를 고정하고, `FairinoConnectionService` 기준 reconnect 상태를 V3 UI 전체가 같은 소스로 읽게 정리했다.
- 이번 턴에서는 motion history/replay는 열지 않았고, `autoreconnect`와 local-state 경계만 닫았다.

### 반영 파일
- `Assets/Scripts/App/Fairino/Connection/PendantV3ConnectionSessionState.cs`
- `Assets/Scripts/App/Fairino/Connection/PendantV3ConnectionSessionAdapter.cs`
- `Assets/Scripts/App/PendantV3SceneCoordinator.cs`
- `Assets/Scripts/App/RobotControlV3DebugBridge.cs`
- `Assets/Scripts/UI/RobotControlV3/ConnectionHomeController.cs`
- `Assets/Scripts/UI/RobotControlV3/StatusCardController.cs`
- `Assets/Scripts/UI/RobotControlV3/SafetyDiagnosticsController.cs`
- `Assets/Scripts/UI/RobotControlV3/HelpPanelController.cs`
- `Assets/Editor/KineTutor3D/PendantV3SceneBuilder.cs`
- `Assets/Scenes/RobotControlV3.unity`
- `Assets/Tests/EditMode/Core/PendantV3ConnectionSessionAdapterTests.cs`

### 구현 메모
- reconnect 상태 SSOT를 `PendantV3ConnectionSessionAdapter`로 추가했다.
  - `ConnectedServoOff`
  - `ConnectedUnsynced`
  - `ReadyToJog`
  - `Fault`
  - `AutoReconnect`
  - `Disconnected`
- retry 정책은 문서 잠금값 그대로 유지했다.
  - 3초 간격
  - 최대 10회
- `ConnectionHome / StatusCard / SafetyDiagnostics / HelpPanel`은 이제 같은 reconnect snapshot을 읽고 같은 failure/retry 문구 축으로 움직인다.
- `RobotControlV3DebugBridge`에는 reconnect summary와 debug lost/retry/failure/success trigger를 추가했다.
- `Undo/Redo`는 계속 `UI-local only`로 두고, 실제 `MoveJ/MoveL` history replay는 제외했다.

### 검증
- `unityctl check --type compile`: pass
- `dotnet build KineTutor3D.Runtime.csproj`: pass
- `dotnet build KineTutor3D.Tests.EditMode.csproj`: pass
- `unityctl test --mode edit --filter KineTutor3D.Tests.EditMode.PendantV3ConnectionSessionAdapterTests`: `3 passed`
- reconnect smoke
  - baseline `ConnectedServoOff`
  - `TriggerConnectionLostForDebug()` -> `AutoReconnect`
  - `CompleteReconnectForDebug(false)` -> `Disconnected + reconnectFailed=True + 수동 연결 필요`
  - `CompleteReconnectForDebug(true)` -> `ConnectedServoOff` 복귀
  - `GetPanelControllerSummary()` 기준 `Status / Safety / Home` failure 문구 동기화 확인

### 남은 메모
- 이번 smoke는 `RobotControlV3` active scene + debug bridge 기준으로 닫은 거라 live 장비 `OnConnectionLost` actual path는 아직 후속이다.
- same-session re-entry no-guide actual click은 여전히 버튼 `globalObjectId` 변동 때문에 완전 자동화가 남아 있다.

## V3 Viewport Camera And Visibility Follow-up

### 요약
- `RobotControlV3`에 V2 기반 카메라 프로필을 붙이고, `PendantV3VisualizationDriver`가 `Main Camera -> OrbitCameraController -> base_link target`까지 책임지게 바꿨다.
- summary 기준으로는 `RobotActual`과 카메라 target이 정상인데, 실제 Game view 캡처에서는 중앙 `ViewportHost` 안 로봇이 여전히 안 보인다.

### 반영 파일
- `Assets/Scripts/App/SceneCameraDirector.cs`
- `Assets/Scripts/Visualization/RobotControl/PendantV3VisualizationDriver.cs`
- `Assets/Scripts/App/RobotControlV3DebugBridge.cs`

### 검증
- `unityctl check --type compile`: pass
- `AuthorSceneSafe()`: `saved=True`
- edit `RobotControlV3` 기준 viewport summary
  - `scene=RobotControlV3`
  - `actualVisible=True`
  - `ghostVisible=False`
  - `cameraReady=True`
  - `cameraTarget=base_link`
  - `cameraPos=(-1.39, 0.55, -2.35)`
  - `fov=40.0`
- play 중 `SceneNavigator.LoadByName("RobotControlV3")` 뒤 viewport summary
  - `scene=RobotControlV3`
  - `actualVisible=True`
  - `ghostVisible=False`
  - `cameraTarget=base_link`
  - `cameraFramed=True`
- game capture
  - [v3-viewport-game.png](C:/Users/ezen601/Desktop/Jason/robotapp2/Artifacts/v3-viewport-game.png)
  - 결과: 중앙 `ViewportHost` 안 로봇 미노출

### 판정
- 현재 이 문제는 `로봇 생성 실패`가 아니라 `렌더링/카메라/뷰포트 불일치` 쪽이다.
- 다음 작업은 `RobotActual` renderer/bounds/layer/cullingMask/clip/frustum`을 직접 점검하는 디버그 slice로 잡는다.

## V3 Viewport RT And Overlay Split

### 요약
- 메인 카메라 뒤에 로봇을 까는 접근이 계속 꼬여서, `ViewportHost` 전용 `camera + render texture + overlay split` 구조로 방향을 바꿨다.
- clean render 기준으론 중앙 뷰포트 안 로봇 노출까지 닫았고, overlay 포함 상태에선 툴바 뒤 일부 겹침만 남았다.

### 반영 파일
- `Assets/Scripts/Visualization/Shared/OrbitCameraController.cs`
- `Assets/Scripts/Visualization/RobotControl/PendantV3VisualizationDriver.cs`
- `Assets/Scripts/UI/RobotControlV3/PendantV3ShellStateController.State.cs`
- `Assets/Scripts/App/Session/PendantV3LocalState.cs`
- `Assets/UI/PendantV3/pendant-v3.uss`
- `ProjectSettings/TagManager.asset`

### 구현 메모
- `RobotControlViewport` 레이어를 추가하고, V3 runtime root를 전용 viewport 카메라만 보게 분리했다.
- `ViewportHost` 내부에는 `ViewportRenderSurface`를 만들고 `RenderTexture`를 background image로 붙였다.
- `Main Camera rect` 잘라쓰기는 더 이상 주 경로로 보지 않고, `ViewportHost` 내부 렌더를 기준으로 고정했다.
- desktop split ratio와 context/tool panel 폭을 줄여 viewport 면적을 더 확보했다.
- 툴바 영역은 safe-area/no-fly-zone으로 보고 camera pivot offset과 initial distance를 별도로 보정했다.

### 검증
- `unityctl check --type compile`: pass
- clean render
  - [v3-viewport-rendertexture-clean.png](C:/Users/ezen601/Desktop/Jason/robotapp2/Artifacts/v3-viewport-rendertexture-clean.png)
  - 결과: 로봇이 중앙 viewport 내부에 실노출
- overlay 포함
  - [v3-viewport-rendertexture-overlay.png](C:/Users/ezen601/Desktop/Jason/robotapp2/Artifacts/v3-viewport-rendertexture-overlay.png)
  - [v3-viewport-safe-overlay-4.png](C:/Users/ezen601/Desktop/Jason/robotapp2/Artifacts/v3-viewport-safe-overlay-4.png)
  - 결과: 패널 뒤 완전 미노출 상태는 해소됐지만, toolbar 뒤 일부 겹침은 남음

### 판정
- `ViewportHost 안 전용 RT 렌더` 방향 자체는 맞다.
- 남은 건 `툴바 no-fly-zone 100% 회피`로, 이건 camera offset만 더 미는 것보다 `toolbar compact/foldable` 또는 viewport 레이아웃 재배치까지 같이 봐야 한다.
