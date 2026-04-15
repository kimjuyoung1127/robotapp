# Pendant V3 Onboarding Direct Path And Tablet Regression

## Date
- 2026-04-07 (KST)

## Summary
- 온보딩의 `V3 제어 둘러보기` CTA를 `RobotLibrary` 경유가 아니라 `FAIRINO_FR5` 선택값을 심은 뒤 `RobotControlV3`로 바로 진입하게 바꿨다.
- 실제 `unityctl ui click` 테스트로 `Onboarding -> RobotControlV3` 직행이 되는 것을 확인했다.
- 직행 확인 뒤 조사한 결과, 데스크탑에서 태블릿형처럼 보이던 주원인은 코드 기본값이 아니라 authored scene에 남아 있던 `previewMode = Auto` 직렬화 값이었다.
- `RobotControlV3` authored scene과 scene builder를 같이 고쳐 기본 저장값을 `Desktop`으로 고정했고, 재검증에서 `width=1280`이어도 `tablet=False`로 확인했다.
- 후속 데스크탑 셸 복구에서 `WorkPanelBody` 공백과 우측 placeholder 문제가 남아 있던 원인은 direct path가 이전 `LocalSettings`를 그대로 먹어 `NavHelp` 같은 stale 상태로 진입하던 점도 컸다.
- 온보딩 `FR5 V3 바로 열기` 진입은 이제 `LocalSettingsStore.Clear()` 뒤 fresh default(`NavMotion / TabEasyMotion`)로 열리게 바꿨고, `EasyMotion` host가 desktop/tablet body 안에서 같이 드러나는 것도 확인했다.

## Updated Files
- `Assets/Scripts/UI/Onboarding/OnboardingManager.cs`
- `Assets/Scripts/UI/Onboarding/OnboardingViewBuilder.cs`
- `Assets/Scripts/UI/RobotLibrary/RobotLibrarySelectionPanel.cs`
- `Assets/Scripts/UI/RobotLibrary/RobotDetailDrawer.cs`
- `Assets/Scripts/UI/RobotControlV3/PendantV3LayoutController.cs`
- `Assets/Scripts/UI/RobotControlV3/EasyMotionController.cs`
- `Assets/Scripts/UI/RobotControlV3/StatusCardController.cs`
- `Assets/Scripts/App/RobotControlV3DebugBridge.cs`
- `Assets/Scripts/App/RobotControlEntryPolicy.cs`
- `Assets/Scripts/UI/RobotControlV3/JointJogController.cs`
- `Assets/Editor/KineTutor3D/PendantV3SceneBuilder.cs`
- `Assets/Scenes/RobotControlV3.unity`
- `Assets/UI/PendantV3/pendant-v3.uxml`
- `Assets/UI/PendantV3/pendant-v3.uss`
- `Assets/UI/PendantV3/joint-jog-panel.uxml`
- `Assets/UI/PendantV3/joint-jog-panel.uss`
- `docs/ref/product/pendant-v3/progress-checklist.md`
- `docs/ref/product/pendant-v3/README.md`

## What Changed
- `Onboarding -> V3` direct path
  - `OpenRobotControlV3Path()`가 이제 `RobotSelectionBridge.SetSelection("FAIRINO_FR5", "robot_control")`를 먼저 기록한다.
  - `RobotControlScenePreference.SetPreferV3(true)`를 적용한 뒤 `SceneId.RobotControlV3`로 직접 이동한다.
- CTA copy
  - 온보딩 CTA 문구를 `FR5 V3 바로 열기`로 바꿨다.
  - 사용자가 `V3` 셸로 직행한다는 기대와 실제 동작을 맞췄다.
- Library label
  - `RobotLibrarySelectionPanel`과 `RobotDetailDrawer`의 Robot Control 라벨이 `preferV3` 상태를 반영해 `Robot Control V3`로 보이게 했다.
- Placeholder reduction
  - `pendant-v3.uxml`에서 `WorkPanel`, `ViewportHost`, `BottomSheet`, `PopupLayer`의 placeholder 라벨 일부를 제거했다.
- Layout default
  - `PendantV3LayoutController`의 기본 preview mode를 `Desktop`으로 고정했다.
- Desktop regression root cause
  - 실제 플레이 조사 결과 `KineTutor3D.EditorTools.PendantV3TabletPreviewBridge.GetSummary()`가 `mode=Auto; width=1280.0; tablet=True`로 찍혔다.
  - 코드 기본값과 별개로 authored scene `Assets/Scenes/RobotControlV3.unity`에 `previewMode: 0`이 직렬화돼 있었고, 이 값이 런타임에서 `Auto`를 유지시켰다.
  - 따라서 데스크탑 모니터 자체가 아니라 `UIDocument` 루트 폭 `1280 <= tabletBreakpoint 1366` 조건 때문에 tablet class가 붙고 있었다.
- Desktop baseline fix
  - `PendantV3SceneBuilder`가 scene author 시 `previewMode = Desktop`, `tabletBreakpoint = 1366`을 serialized field에 직접 기록하게 보강했다.
  - `RobotControlV3.unity`의 현재 저장값도 `previewMode: 1`로 갱신해 기본 authored 상태가 `Desktop`으로 남게 했다.
- Desktop shell polish
  - `pendant-v3.uxml`에 `WorkPanelTitle`, `WorkPanelSummary`, `BottomSheetTitle`, `BottomSheetSummary` 슬롯을 추가해 `ShellStateController`와 `ConnectionHomeController`가 실제 헤더 카피를 갱신할 수 있게 했다.
  - 우측 `ActionHint` / `WhyItMoved` 카드의 placeholder 타이틀을 실제 안내 카드 구조로 바꾸고, `StatusCardController`가 `ActionPrimary`, `ActionWhy`, `ActionNow` 기반 문구를 같이 갱신하게 묶었다.
  - `EasyMotionController`가 desktop/tablet host만이 아니라 `WorkPanelBody`, `BottomSheetBody`까지 함께 unhide 하도록 고쳐서 메인 패널이 빈 것처럼 보이던 상태를 줄였다.
- Direct path state reset
  - `OnboardingManager.OpenRobotControlV3Path()`에서 `LocalSettingsStore.Clear()`를 호출해 stale nav/tab/speed 상태가 direct path 첫 화면을 덮지 않게 했다.
  - 이 변경 뒤 `FR5 V3 바로 열기` 경로는 기본 `NavMotion / TabEasyMotion` 상태로 진입한다.
- Entry policy baseline
  - `RobotControlEntryPolicy`를 추가해 `FreshStart`와 `ResumeLastSession` 의도를 코드에서 명시했다.
  - 온보딩 direct path는 `FreshStart`, `RobotLibrary`에서 Robot Control 진입은 `ResumeLastSession`으로 고정했다.
  - 즉 정책 기준선은 `처음 들어올 땐 reset`, `라이브러리에서 다시 들어올 땐 restore`다.
- Progress checklist baseline
  - `docs/ref/product/pendant-v3/progress-checklist.md`를 추가해 `0A~4` 전체 slice 진행률을 한 문서에서 관리하게 했다.
  - README 인덱스에도 checklist 링크를 추가했다.
- `2B-2` Joint Jog kickoff
  - `joint-jog-panel.uxml` / `.uss`와 `JointJogController.cs`를 추가했다.
  - `JointJogPanelHost`, `JointJogSheetHost`를 V3 셸에 추가해 desktop/tablet 양쪽 host를 마련했다.
  - `SceneBuilder`, `PendantV3Document`, `RobotControlV3DebugBridge`를 갱신해 authored scene bootstrap과 debug summary가 joint jog controller를 포함하게 했다.
  - 현재 kickoff 범위는 슬라이더/단일축 모드 토글, 6축 값 표시, 증분 버튼, 숫자 입력, `FocusIn -> SelectAll()` 구현까지다.
  - 후속 수정으로 `JointJogController`를 `PendantV3ShellStateController.NotifyPanelControllers()` 체인에 연결하고, panel들이 stale `LocalSettings` 대신 셸 state snapshot을 읽도록 바꿨다.
  - direct path 후 `SetShellSelection("NavMotion", "TabJointJog", "BottomTabJointJog")` 검증에서 최종 summary가 `joint desktopVisible=True; tabletVisible=True`로 바뀌는 것까지 확인했다.
  - 실제 입력 검증에서도 `FocusJointInputForDebug(1)` 결과 `focused=True; cursor=0; select=3`이 나와 `0.0` 전체 선택을 확인했다.
  - `SetJointSliderForDebug(1, 12.5)` 결과 `slider=12.5; input=12.5; label=12.5°`로 slider -> input/label 동기화를 확인했다.
  - `SetJointInputForDebug(1, "-45.0")` 결과 `slider=-45.0; input=-45.0; label=-45.0°`로 input -> slider/label 동기화를 확인했다.

## Self Review
- 역할 경계
  - `RobotControlEntryPolicy`는 App 경계에 두고, panel visibility와 focus 검증은 `UI/RobotControlV3` 안에만 남겼다.
  - `RobotControlV3DebugBridge` 확장은 runtime QA 편의를 위한 probe 추가 수준으로 묶었다.
- 구조/품질
  - `2B-2` 관절 조그 shell sync 버그는 닫혔다.
  - 다만 `JointJogController.cs`는 첫 슬라이스 기준으로 파일이 커져서 다음 분리 후보가 명확하다.
    - row/input binding helper
    - debug helper
    - panel state apply
- 남은 리스크
  - `unityctl ui click`으로 UITK `TabJointJog` 자체를 직접 누르는 경로는 아직 불안정하다.
  - 현재 관절 조그 검증은 debug bridge 기반이라, 실제 UITK 탭 클릭 smoke를 별도로 더 닫아야 한다.

## Verification
- `unityctl check --project C:\Users\ezen601\Desktop\Jason\robotapp2 --type compile --json`
  - pass
- `unityctl ui find --project C:\Users\ezen601\Desktop\Jason\robotapp2 --name 'BtnOpenRobotControlV3' --include-inactive --json`
  - `Onboarding` 씬에서 `FR5 V3 바로 열기` 버튼 존재 확인
- `unityctl ui click --project C:\Users\ezen601\Desktop\Jason\robotapp2 --id <BtnOpenRobotControlV3> --mode play --json`
  - 버튼 클릭 이벤트 발생 확인
- `unityctl exec --project C:\Users\ezen601\Desktop\Jason\robotapp2 --code "KineTutor3D.App.SceneCatalog.GetCurrentSceneId()" --json`
  - 클릭 후 `result = 7`
  - `SceneId.RobotControlV3` 진입 확인
- `unityctl exec --project C:\Users\ezen601\Desktop\Jason\robotapp2 --code "KineTutor3D.App.RobotSelectionBridge.GetSelectedRobotId()" --json`
  - `FAIRINO_FR5`
- `unityctl exec --project C:\Users\ezen601\Desktop\Jason\robotapp2 --code "KineTutor3D.App.RobotSelectionBridge.GetSelectedMode()" --json`
  - `robot_control`
- `unityctl exec --project C:\Users\ezen601\Desktop\Jason\robotapp2 --code "KineTutor3D.EditorTools.PendantV3TabletPreviewBridge.GetSummary()" --json`
  - 회귀 조사 중 결과: `mode=Auto; width=1280.0; tablet=True`
- `unityctl exec --project C:\Users\ezen601\Desktop\Jason\robotapp2 --code "KineTutor3D.EditorTools.PendantV3SceneBuilder.AuthorSceneSafe()" --json`
  - `saved=True`
- `Select-String Assets/Scenes/RobotControlV3.unity -Pattern "previewMode|tabletBreakpoint"`
  - `previewMode: 1`
  - `tabletBreakpoint: 1366`
- `unityctl ui click --project C:\Users\ezen601\Desktop\Jason\robotapp2 --id <BtnOpenRobotControlV3> --mode play --json`
  - `Onboarding -> RobotControlV3` 재진입 확인
- `unityctl exec --project C:\Users\ezen601\Desktop\Jason\robotapp2 --code "KineTutor3D.EditorTools.PendantV3TabletPreviewBridge.GetSummary()" --json`
  - 수정 후 결과: `mode=Desktop; width=1280.0; tablet=False`
- `unityctl exec --project C:\Users\ezen601\Desktop\Jason\robotapp2 --code "KineTutor3D.App.RobotControlV3DebugBridge.ClearLocalSettings()" --json`
  - 기본 상태 확인: `nav=NavMotion; work=TabEasyMotion; tablet=BottomTabEasyMotion; coord=Base; speed=30; increment=5; split=0.45; sheetExpanded=True`
- `unityctl exec --project C:\Users\ezen601\Desktop\Jason\robotapp2 --code "KineTutor3D.App.RobotControlV3DebugBridge.GetPanelControllerSummary()" --json`
  - stale 상태일 때: `nav=NavHelp ... easy desktopVisible=False`
  - `LocalSettings` clear + direct path 재진입 뒤: `nav=NavMotion ... easy panelHidden=False; desktopVisible=True; tabletVisible=True`
- `unityctl check --project C:\Users\ezen601\Desktop\Jason\robotapp2 --type compile --json`
  - `RobotControlEntryPolicy` 추가 뒤에도 pass 유지
- `unityctl exec --project C:\Users\ezen601\Desktop\Jason\robotapp2 --code "KineTutor3D.EditorTools.PendantV3SceneBuilder.GetDocumentRootComponentSummary()" --json`
  - refreshed summary 기준 `JointJogController` 포함 확인
- `unityctl exec --project C:\Users\ezen601\Desktop\Jason\robotapp2 --code "KineTutor3D.App.RobotControlV3DebugBridge.GetPanelControllerSummary()" --json`
  - `joint=[initialized=True; desktopVisible=False; tabletVisible=False; mode=Slider; j1=0.0; j6=0.0]`
  - direct path 기본 진입은 아직 `TabEasyMotion`이라 joint panel은 비활성 상태로 초기화만 확인
- `unityctl exec --project C:\Users\ezen601\Desktop\Jason\robotapp2 --code "KineTutor3D.App.RobotControlV3DebugBridge.SetShellSelection(\"NavMotion\", \"TabJointJog\", \"BottomTabJointJog\")" --json`
  - 셸 상태를 관절 조그 탭으로 강제 전환
- `unityctl exec --project C:\Users\ezen601\Desktop\Jason\robotapp2 --code "KineTutor3D.App.RobotControlV3DebugBridge.GetPanelControllerSummary()" --json`
  - 최종 결과: `counts=[shell=1; easy=1; joint=1]`
  - `easy ... desktopVisible=False`
  - `joint ... desktopVisible=True; tabletVisible=True`
- `unityctl exec --project C:\Users\ezen601\Desktop\Jason\robotapp2 --code "KineTutor3D.App.RobotControlV3DebugBridge.FocusJointInputForDebug(1)" --json`
  - `focused=True; cursor=0; select=3`
- `unityctl exec --project C:\Users\ezen601\Desktop\Jason\robotapp2 --code "KineTutor3D.App.RobotControlV3DebugBridge.SetJointSliderForDebug(1, 12.5)" --json`
  - `slider=12.5; input=12.5; label=12.5°`
- `unityctl exec --project C:\Users\ezen601\Desktop\Jason\robotapp2 --code "KineTutor3D.App.RobotControlV3DebugBridge.SetJointInputForDebug(1, \"-45.0\")" --json`
  - `slider=-45.0; input=-45.0; label=-45.0°`
- `unityctl test --project C:\Users\ezen601\Desktop\Jason\robotapp2 --mode edit --json`
  - 전체 EditMode 결과: `436 passed / 18 failed / 0 skipped`
  - 실패 18건은 이번 변경 범위와 직접 무관한 기존 failure 묶음으로 확인
    - `MathReadinessPanelTests` 다수
    - `OnboardingManagerTests`의 `DestroyImmediate` log issue
    - `UIInventoryValidatorTests`의 동일 log issue
    - `RobotControlSceneCoordinatorTests`, `RobotControlShellBinderTests`, `SceneCameraDirectorTests`

## Current Runtime Truth
- 플레이 시작 자체는 여전히 에디터 규칙상 `Onboarding`에서 시작한다.
  - 원인: `Assets/Editor/KineTutor3D/BootScenePlayModeSetup.cs`
  - `Always Start From Onboarding`가 켜져 있으면 editor play start scene이 `Onboarding.unity`로 고정된다.
- 따라서 `RobotControlV3` 검증은 현재 기준으로 `Play -> Onboarding -> FR5 V3 바로 열기` 흐름 안에서 확인해야 한다.

## Current Risk / Debt
- 데스크탑에서 tablet class가 강제로 붙는 회귀는 이번 수정으로 닫았다.
- 다만 tablet 지원 자체는 제거한 게 아니라 기본 authored 저장값만 `Desktop`으로 고정한 상태다.
  - 이후 실제 tablet 적용을 다시 열 때는 `Auto` 또는 `Tablet` 진입 정책을 별도로 정의해야 한다.
  - 그때 `BottomSheet`, `CoordOverlay`, tablet tab 흐름을 실제 UX 기준으로 다시 다듬어야 한다.
- `EasyMotion` host visibility는 direct path 기본 상태 기준으로는 복구됐지만, `RobotLibrary` 같은 다른 진입에서도 stale local state를 유지할지 fresh 진입으로 맞출지는 아직 정책 결정이 필요하다.
- `RobotLibrary` 쪽 `ResumeLastSession` 정책은 코드 기준선으로 박았지만, 실제 사용감 기준으로 restore가 과한지 추가 UX 확인은 남아 있다.
- `2B-2`는 kickoff와 shell state sync 버그 수정까지 끝냈지만, 실제 UITK `TabJointJog` 버튼을 `unityctl ui click`으로 직접 누르는 검증은 아직 불안정하다.
- 전체 EditMode suite에는 기존 실패 18건이 남아 있다. 이번 변경으로 새 red가 늘어난 증거는 아직 없지만, green baseline 자체는 아님.
- `unityctl screenshot capture --view game --include-overlay-ui`는 여전히 검게 저장되는 경우가 있어, 시각 증빙 도구로 신뢰도가 낮다.

## Next Slice
- 데스크탑 기본 authored 상태가 유지되는 동안 `NavRail`, `WorkPanel`, `ContextPanel`, `BottomSheet` 가시성 기준을 다시 점검한다.
- `EasyMotion`을 메인 `WorkPanel`에 확정 노출한다.
- 우측 `StatusCard/CoordStrip` 복원을 데스크탑 first 상태로 다시 맞춘다.
- 남은 placeholder 텍스트를 전부 제거한다.
- `RobotLibrary -> RobotControlV3`에서 restore 정책이 실제로 자연스러운지 플레이 기준으로 추가 QA한다.
- `TabJointJog` 실제 UITK 버튼 클릭을 `unityctl ui click` 경로로 안정화해 debug helper 없이도 재현 가능하게 만든다.
