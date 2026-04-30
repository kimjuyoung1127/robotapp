# KineTutor3D 프로젝트 상태

최종 업데이트: 2026-03-31 (KST)
기준 문서: `CLAUDE.md`, `KineTutor3D_Execution_Plan.md`

> Note: 이 문서는 누적 turn log 성격이라 예전 항목 중 `Home/Main` 언급은 당시 기준 기록일 수 있다.
> 현재 runtime truth는 `Boot -> Onboarding -> RobotLibrary -> {MathReadiness, Sandbox, RobotControl}` 기준으로 읽는다.

## 현재 Phase
- **Phase 0: Foundation** (완료)
- **Phase 1: Types + Math (TDD)** (완료)
- **Phase 2: Kinematics Core (DH + FK)** (완료)
- **Phase 3: Template 2DOF + App/UI 연결 (MVP)** (완료)
- **Phase 4: Visualization (FrameGizmo + RobotRenderer Core)** (완료)
- **Phase 5: Guided Lesson P0 구현** (Done: 5A~5G Complete)
- **Phase 6: CI/CD (Unity tests workflow)** (Hold — 로컬 테스트 전용, runner 미등록)
- 병행 작업: **Phase 3 Student-Friendly UX 런타임 연결/데이터 실체화** 완료
- 병행 작업: **GameLab-style Product Docs Governance** 진행 중

## 이번 턴 반영 내용 (FAIRINO 소프트 티칭패드 기획 문서화)
1. 제조사 티칭패드/WebApp 기능을 현재 `RobotControl` 구현과 1대1로 대조한 `docs/ref/product/robots/fairino-teaching-pad-feature-matrix.md`를 추가했다.
2. 한국어 초보자용 `나만의 티칭패드` 목표를 기준으로 `docs/ref/product/ux/robotcontrol-soft-teaching-pad.md`를 추가했다.
3. 문서에는 `있음 / 부분 구현 / 없음 / 실기 검증 필요` 4단계 매트릭스, authored-first / 디자인 토큰 / 태블릿 / 프리뷰 / 차별화 포인트를 함께 정리했다.

## 이번 턴 반영 내용 (FAIRINO 소프트 티칭패드 V1 backlog 고정)
1. `docs/ref/product/roadmap/robotcontrol-soft-teaching-pad-v1-backlog.md`를 추가해 V1 범위를 실제 작업 순서표로 잘랐다.
2. V1 목표를 `연결 -> 동기화 -> 프리뷰 -> 안전 확인 -> 작은 MoveJ/MoveL`로 고정했다.
3. `실기 핵심 + 초보자 가치 + 태블릿 대응`만 V1 범위에 넣고, `Program/TPD/I/O/Gripper/Servo Live`는 V1 범위 밖으로 명시했다.

## 이번 턴 반영 내용 (SimMachine 구조 분석)
1. 로컬 `FAIRINO_SimMachine_v3.9.3` VM 이미지와 `FAIRINO SimMachine Software-V3.9.3-20260210` 패키지를 기준으로 SimMachine 화면 구조 초안 문서를 추가했다.
2. `web/frontend/index.html`, `routeConfigFn`, `pages/*.html`, `ko.json`을 바탕으로 메인 route, 메뉴 구조, TPD/I/O/Gripper 관련 화면 축을 정리했다.
3. 현재 세션에서는 VMware 실행 파일이 확인되지 않아 VM 직접 부팅은 보류했고, 대신 수동 실행 체크리스트를 문서에 남겼다.

## 이번 턴 반영 내용 (공식 기능 1:1 와이어프레임 설계)
1. 실제 SimMachine 캡처와 공식 문서 기능을 1대1로 매칭해 `RobotControl` Desktop/Tablet 와이어프레임을 구체화했다.
2. `docs/ref/product/ux/robotcontrol-soft-teaching-pad.md`에 공식 기능 매칭 표, Desktop/Tablet 와이어프레임, 패널별 버튼 이름, 라벨 표준을 추가했다.
3. `docs/ref/product/robots/fairino-simmachine-screen-structure-draft.md`에 실제 캡처 검증 메모를 추가해 추정 구조를 보정했다.

## 이번 턴 반영 내용 (RobotControl 씬 계층 구조 설계 + SimMachine 한글 점검 메모)
1. `docs/ref/product/ux/robotcontrol-scene-hierarchy.md`를 추가해 새 `RobotControl.unity` 권장 씬 계층 구조를 문서화했다.
2. 문서에는 `SceneBootstrap`, `RuntimeRoot`, `RobotControlShell`, `BottomSheets`, `Popups` 기준의 authored-first 계층과 기존 컴포넌트 매핑을 함께 정리했다.
3. `fairino-simmachine-screen-structure-draft.md`에는 SimMachine 한국어 리소스/언어 전환 구조 점검 메모를 추가했다.

## 이번 턴 반영 내용 (소프트 티칭패드 범위 컷 정리)
1. `robotcontrol-soft-teaching-pad.md`에 `필수 / 선택 / 제외` 범위를 추가해 SimMachine 전체 기능을 그대로 가져오지 않는 기준을 명시했다.
2. `robotcontrol-soft-teaching-pad-v1-backlog.md`에도 같은 분류를 반영해 V1 범위를 더 분명하게 고정했다.
3. 제외 항목에는 설치형 상세 설정, 안전 상세 설정, 산업별 주변장치, Program/Graphical/Node editor 계열을 명시했다.

## 이번 턴 반영 내용 (Unity 전용 Program 탭 설계)
1. `robotcontrol-soft-teaching-pad.md`에 Unity 전용 Program 탭 상세 설계를 추가했다.
2. Program 탭은 SimMachine의 `Coding / Graphical / Node Graph`를 복제하지 않고, `3D 기반 동작 시퀀서 + 교육형 디버거`로 재해석하는 방향으로 고정했다.
3. V1 backlog에는 `포인트 기반 동작 시퀀스 + 시뮬레이션 + 선택 블록 상세 편집`만 선택 범위로 추가했다.

## 이번 턴 반영 내용 (Unity 전용 Status 탭 설계)
1. `robotcontrol-soft-teaching-pad.md`에 Unity 전용 Status 탭 설계를 추가했다.
2. Status 탭은 SimMachine의 `Log / Query`를 그대로 복제하지 않고, `상태 요약 + 최근 이벤트 + 세션 리포트` 중심으로 축소하는 방향으로 정리했다.
3. V1 backlog에는 `Status 탭 최소 버전`을 선택 범위로 추가하고, 전문 로그/파형 조회기 복제는 제외 항목으로 명시했다.

## 이번 턴 반영 내용 (Unity 전용 Application 탭 해석)
1. `robotcontrol-soft-teaching-pad.md`에 Application 탭 해석을 추가했다.
2. Application은 `Tool App + Process Package` 성격의 산업 확장 기능으로 보고, 메인 소프트 티칭패드 V1에서는 전체 제외로 정리했다.
3. 단, `Data recording`, `Drag locking`, `Intersection Generation`, `Palletization`, `Conveyor Tracking`은 후속 후보로만 메모했다.

## 이번 턴 반영 내용 (SimMachine 전체 IA 통합 문서화)
1. `docs/ref/product/ux/fairino-simmachine-ia-map.md`를 추가해 `Initial / Program / Status / Application / System` 전체 구조를 하나의 문서로 묶었다.
2. 각 카테고리별로 기능 트리, 공식 근거, 캡처 근거, Unity 반영 위치, `필수 / 선택 / 제외` 판단을 함께 정리했다.
3. 이 문서를 기준으로 SimMachine 전체 복제가 아니라 `Unity 최종 컷`을 한 번에 볼 수 있게 만들었다.

## 이번 턴 반영 내용 (IA → 씬 계층 → 구현 브리지)
1. `docs/ref/product/ux/robotcontrol-implementation-bridge.md`를 추가해 통합 IA, 씬 계층, 실제 구현 순서를 연결했다.
2. 각 IA 항목별로 씬 위치, 패널/오브젝트 이름, 담당 스크립트, 재사용 여부, `unityctl` 검증 루프를 표로 정리했다.
3. 구현 순서를 `셸 -> 필수 패널 -> 3D 차별화 -> UX 보강 -> Tablet`으로 고정했다.

## 이번 턴 반영 내용 (새 세션 인수인계 문서)
1. `docs/ref/product/ux/robotcontrol-next-session-handoff.md`를 추가해 새 세션 구현용 실행 인덱스를 만들었다.
2. 문서에는 SSOT 링크, 브랜치 전략, 먼저 만들 폴더 구조, 구현 순서, 검증 루프, V1 제외 범위를 한 번에 정리했다.
3. 새 세션에서는 이 문서와 기존 SSOT만 읽고 바로 폴더 생성부터 시작하도록 기준을 고정했다.

## 이번 턴 반영 내용 (온보딩 네비 정리 + RobotControl 이중 진입)
1. `SceneId`/`SceneCatalog`를 확장해 기존 `RobotControl`과 새 `RobotControlV2`를 동시에 내비게이션에 노출하도록 정리했다.
2. `Onboarding.unity`에서 비활성 legacy `TopBarBackground`와 `SceneNavButtons`를 삭제했다.
3. `SceneNavigationBar.cs`에서 legacy 숨김/호환 코드를 제거하고 `TopBarRect` 기반 hosted 네비만 사용하도록 단순화했다.

## 이번 턴 반영 내용 (구현 규율 보정)
1. `App/Fairino` 폴더 구조를 `Template` 단일 폴더 기준으로 다시 정리했다.
2. `SceneNavigationBar`는 child index 재사용 대신 엔트리 이름 기반 버튼 생성으로 수정했다.
3. `TopStatusBar`의 기본 표시 문자열을 한국어 우선 기준으로 맞췄다.

## 이번 턴 반영 내용 (문서 업데이트 후 진행 반복 규칙)
1. `AGENTS.md`, `CLAUDE.md`, `robotcontrol-next-session-handoff.md`에 공통 규칙을 추가했다.
2. 문서 업데이트는 종료 조건이 아니라 다음 구현/검증 단위로 넘어가는 트리거로 명시했다.
3. 문서 업데이트가 발생한 턴에는 최소 `다음 실행 단위 1개`를 바로 진행하도록 기준을 고정했다.

## 이번 턴 반영 내용 (Onboarding 전역 네비 중복 제거 + RobotControl 라우팅 검증)
1. `SceneNavigationBar`가 `SceneNavButtons` 하위 버튼을 재생성하기 전에 즉시 비우도록 수정해, 플레이 중 `Sandbox` 등 전역 네비 버튼이 중복으로 보이던 현상을 제거했다.
2. 버튼 이름은 표시 문자열 기준 공백 제거 규칙(`NavRobotControl`, `NavRobotControlV2`)으로 정리해 stale name 재사용을 줄였다.
3. PlayMode에서 `Onboarding` 기준 전역 네비 버튼이 `Math Readiness`, `Robot Library`, `Sandbox`, `Robot Control`, `Robot Control V2` 5개만 보이는 상태를 확인했다.
4. PlayMode 실클릭 검증 결과:
   - `Robot Control V2` -> `Assets/Scenes/RobotControlV2.unity`
   - `Robot Control` -> `Assets/Scenes/RobotControl.unity`
5. 남은 작업은 라우팅이 아니라 `RobotControlV2` 셸 본체를 새 구조에 맞게 연결하는 것이다.

## 이번 턴 반영 내용 (RobotControlV2 최소 셸 전환)
1. `RobotControlSceneCoordinator`는 `SceneId.RobotControlV2`에서 legacy `FairinoRobotControlViewBuilder` 경로를 타지 않고, 새 `RobotControlShell` 최소 구조만 띄우도록 분기했다.
2. `RobotControlShell.EnsureV2Shell(...)`를 추가해 V2 씬 진입 시 old `ConnectionPanel`, `JointControlPanel`, `StatePanel`, `TopBar` 등 legacy 자식을 숨기고 제거한 뒤 `SafeArea/TopStatusBar/LeftRail/CenterViewport/RightRail/BottomSheets/Popups/DebugOnly` 루트만 보장하게 했다.
3. PlayMode 검증 결과 `Robot Control V2` 클릭 후 old 버튼은 사라지고, `TopStatusBar`의 6개 버튼(`서보`, `시작`, `정지`, `일시정지`, `Sync`, `오류 초기화`)만 보이는 최소 셸 상태를 확인했다.

## 이번 턴 반영 내용 (URDF legacy Input 예외 방어)
1. `Assets/Scripts/App/UrdfLegacyControllerGuard.cs`를 추가해 씬 로드 직후 `Unity.Robotics.UrdfImporter.Control.Controller`를 전역으로 찾아 비활성화하도록 했다.
2. `RobotControlSceneCoordinator`와 `FR5TemplateMinimalController`의 개별 방어 코드도 루트 오브젝트만 보지 않고 `GetComponentsInChildren<MonoBehaviour>(true)`로 자식 전체를 훑도록 보강했다.
3. 최신 콘솔 루프에서는 기존 `InvalidOperationException: You are trying to read Input using the UnityEngine.Input class...`가 재현되지 않았다.
4. 현재 남은 콘솔 노이즈는 `SceneNavigationBar`가 편집 단계에서 버튼을 재생성할 때 생기는 `SendMessage ... Awake/OnValidate` 계열이며, 이는 별도 정리 대상으로 남는다.

## 이번 턴 반영 내용 (RobotControlV2 정식 디자인 토큰 승격)
1. `UIDesignTokens.cs`에 `UIDesignTokens.RobotControlV2` 섹션을 추가해 `Colors`와 `Size` 기준 토큰을 정식 정의했다.
2. `RobotControlShell`과 `TopStatusBar`는 더 이상 로컬 색 상수를 사용하지 않고 `UIDesignTokens.RobotControlV2.*`를 직접 읽도록 정리했다.
3. 시안 방향은 `Taupe + Slate` 기반의 연한 갈색 계열이다.
   - 다크 베이스 유지
   - 따뜻한 샌드/토프 accent
   - muted text / border / card tone도 같은 계열로 통일
4. `RobotControlV2`는 authored-first 최소 셸 위에 좌측 `COMMAND CENTER`, 중앙 `WORKSPACE`, 우측 `INSPECTOR`, 하단 `MODULE STRIP` 시안 레이아웃을 갖도록 확장했다.
5. 마지막 자동 compile 검증은 Unity `domain reload / IPC not ready` 상태 때문에 완료하지 못했고, 다음 세션 첫 루프에서 재검증이 필요하다.

## 이번 턴 반영 내용 (구조 정리 + realvirtual 의존 축소 1차)
1. `Assets/Scripts/App`, `Assets/Scripts/UI`, `Assets/Scripts/Visualization`를 역할/페이지 기준 하위 폴더 구조로 재정리했다.
   - `App`: `Runtime/`, `Session/`, `Lessons/`, `Fairino/Template/`
   - `UI`: `GuidedLesson/`, `Onboarding/`, `RobotLibrary/`, `RobotControl/`, `MathReadiness/`, `Sandbox/`, `Shared/`, `DesignSystem/`
   - `Visualization`: `Renderer/`, `RobotLibrary/`, `RobotControl/`, `MathReadiness/`, `Targets/`, `Shared/`
2. 각 폴더의 `AGENTS.md` / `CLAUDE.md`와 루트 `AGENTS.md`를 새 구조 기준으로 동기화했다.
3. `realvirtual` 직접 코드 의존을 1차 축소했다.
   - `RobotCatalog`의 `SCARA/Fanuc/igus` importSource를 `Assets/Runtime/Resources/Robots/...`로 전환
   - `ScaraDonorStructureTests`를 런타임 donor prefab 기준으로 전환
   - `RobotPreviewPod`에서 `realvirtual.Drive` reflection 경로 제거
4. 관련 문서(`asset-registry`, `asset-validation-report`, `PROJECT-STATUS`, `SKILL-DOC-MATRIX` 등)는 `Assets/Runtime/Resources/Robots`를 **현재 donor/runtime 경로**, `Assets/Runtime/Robots/Common`을 **공용 donor 복구 자산층**으로 정리했다.
5. `realvirtual` 런타임 prefab sanitation을 완료해 `realvirtualController` 직렬화 흔적과 대표 vendor component를 runtime robot prefab에서 제거했다.
6. `realvirtual` editor bootstrap 중 `OnLoad.cs`의 동기 `PackageManager.Client.List()` 대기를 비동기 polling으로 완화해 domain reload 정체 가능성을 줄였다.
7. 리로드 정체 조사 결과, 병목 구간은 `ProcessInitializeOnLoadAttributes`이며 `realvirtual`의 `QuickEditToolbarIMGUI`가 `delayCall + update + hierarchyChanged + afterAssemblyReload`를 동시에 묶는 가장 유력한 후보로 확인했다.
8. `realvirtual/private/Editor/QuickEditToolbarIMGUI.cs`는 기본 부트스트랩을 비활성화해(`KineTutor3D.EnableRealvirtualQuickEditBootstrap=false` 기본값) 원인 범위를 더 좁힐 수 있는 상태로 만들었다.
9. `docs/status/REALVIRTUAL-REMOVAL-PLAN.md`를 추가해 삭제 보류 이유, 남은 runtime prefab 리스크, 단계별 제거 순서를 고정했다.
10. `QaToolsMenu.SanitizeRuntimeRobotPrefabs()`를 통해 `ScaraRobot`, `FanucCRX-10iA_L`, `igusRebel` 런타임 prefab에서 총 38개 `realvirtual` component를 제거했고, 관련 donor/preview EditMode 테스트와 compile green을 유지했다.
11. `Assets/Runtime/RenderPipelines/URP/` 아래에 project-owned URP pipeline asset 세트를 복제/정리하고, copied renderer asset에서 `realvirtual` 전용 renderer feature block을 제거했다.
12. `GraphicsSettings`와 모든 `QualitySettings` tier를 새 `KineTutor3D-URP.asset` GUID로 repoint했고, `Assets/Runtime` + `ProjectSettings` 범위 검색 기준 old realvirtual URP GUID / renderer GUID / renderer feature GUID가 더 이상 직접 검출되지 않는다.
13. `Assets/realvirtual` + `Assets/Gizmos/realvirtual`를 실제 삭제했고, 삭제 상태에서 compile green과 `realvirtual.base` 비포함 assembly 목록을 확인했다.
14. 삭제 후 `Boot` play smoke, `RobotLibrary`/`Sandbox`/`RobotControl` scene open smoke를 확인했고, 콘솔에는 `unityctl` IPC noise 외에 신규 삭제 유발 오류가 없었다.
15. 삭제 직후 깨진 `SCARA`, `FANUC CRX-10iA/L`, `IGUS REBEL` donor 시각 참조는 `Assets/Runtime/Robots/Common/` 아래 최소 mesh/material/prefab 복구 자산으로 복원했고, `MathReadiness`의 stale donor prefab 참조와 `FrameGizmo` shared material 적용도 함께 보정했다.
7. 검증:
   - `unityctl check --project ... --type compile` 통과
   - `KineTutor3D.Tests.EditMode.ScaraDonorStructureTests` 2개 통과

## 이번 턴 반영 내용 (FAIRINO live SDK staging)
1. 공식 `fairino-csharp-sdk` ZIP을 다시 확인해 실제 DLL 이름이 `libfairino.dll`, `CookComputing.XmlRpcV2.dll`임을 검증했다.
2. 로컬 `Assets/Plugins/Fairino/`에 두 DLL을 staging 해 Live 연동 준비 상태를 만들었다.
3. `Assets/Plugins/Fairino/README.md`를 실제 배포 파일명 기준으로 수정했다.
4. SDK 추가 후 드러난 compile blocker 4건을 정리했다.
   - `SceneCameraDirector`의 `Object` 모호성 1건
   - `Editor/CliTools`의 `int? -> int` 변환 3건
5. 현재 남은 blocker는 SDK 부재가 아니라 `실기 컨트롤러 IP 미확정`이다.
6. 현장 검증 순서는 `Connect -> GetVersion/ReadState -> Enable -> small MoveJ`로 고정했다.

## 현재 사이클 반영 내용 (SCARA + Sandbox baseline + asset subset)
1. 복구된 vendor source(`HQP Studios`, `_Heathen Engineering`, `Glowing Rifts`)를 확인하고 curated runtime subset 경로를 추가했다.
2. `Assets/Runtime/Prefabs/Teaching/Markers/`, `Assets/Runtime/Prefabs/Teaching/RobotLibrary/`, `Assets/Runtime/Art/UI/Icons/`를 기준 자산 경로로 도입했다.
3. `TargetMarkerVisual`은 curated subset 우선, vendor source 차선, primitive fallback 최후 순서로 prefab을 해석하도록 변경했다.
4. `SCARA_RV`를 실제 template 지원 항목으로 승격하고 `RobotCatalog`/`RobotMetadataInfo`를 확장했다.
5. `JointInputRail`은 2DOF 기본 레일을 유지하면서 4DOF 추가 row를 동적으로 생성하도록 확장했다.
6. `Robot Library`에서 `Guided Lesson`과 `Sandbox` 진입을 모두 지원하도록 CTA와 selection bridge를 확장했다.
7. `Sandbox.unity` 씬과 build settings entry를 추가해 sandbox 라우팅 기반을 만들었다.
8. 검증 결과: EditMode `107/107`, PlayMode `31/31`.

## 이번 턴 반영 내용 (RobotControl Phase 8: 애니메이션 + Speed + 안전)
1. **프리셋 애니메이션**: `PresetTransitionAnimator` 신규 (EaseInOutCubic 1.5초 보간, 슬라이더/핸들 드래그 시 Cancel)
2. **Speed Selector**: JointControlPanel + TcpControlPanel에 `[Slow 10%] [Medium 30%] [Fast 60%]` 3단 버튼, `FairinoRobotConfig.GetSpeedAcc(preset)` 추가
3. **연결 끊김 안전**: `FairinoConnectionService` 3회 연속 에러 → `OnConnectionLost` + 자동 Disconnect, `SetControlsEnabled(false)`, 빨간색 "연결 끊김 — 재연결하세요" 안내
4. **연결 초기화 보간**: 첫 연결 시 현재 포즈 → 실제 로봇 포즈 0.8초 보간, `isFirstStateAfterConnect` 플래그
5. **UI 토큰**: `UIDesignTokens.Anim.PresetTransition=1.5f`, `ConnectionSync=0.8f`
6. **자기리뷰 2건 수정**: ShowConnectionLost RefreshUI 코드 침범(컴파일 에러), duration<=0 가드
7. **테스트**: `PresetTransitionAnimatorTests` 13개 신규, `FairinoConnectionServiceTests` Mock 전환 순서 수정. EditMode 345/351 (6 failed=기존)
8. **문서**: ROBOTCONTROL-IMPL-BOARD.md에 UI 계층 트리 + 데이터 흐름 다이어그램 + 실기 연결 체크리스트 추가

## 이전 턴 반영 내용 (RobotControl P0~P5 확장 + 공용화)
1. **P0 카메라**: `SceneCameraDirector` RobotControl 프로파일 좌표 확정 (pos 1.0/0.75/1.0, euler 22/215/0, FOV 40)
2. **P1 회전 핸들**: `JointRotationHandle` 6관절 1축 회전 링 (LineRenderer 원호, 마우스 드래그→슬라이더 양방향 동기화, 드래그 중 OrbitCamera 잠금)
3. **P2 TCP 제어**: `FairinoTcpControlPanel` (X/Y/Z/Rx/Ry/Rz InputField + MoveL/ServoCart + DryRun), `IFairinoRobotClient.MoveL` 인터페이스 + Mock/Live 구현, ViewBuilder 탭 "DH Params"→"TCP Control" 교체
4. **P3 동기화**: `FairinoConnectionService.SyncCurrentState()`, `FairinoJointControlPanel` Sync 버튼 (Mock 비활성), `FR5PosePresets.Current` 동적 프리셋 + 캐시
5. **P4 변위 화살표**: `DisplacementArrow` EE 변위 벡터 (LineRenderer+원뿔 헤드, 0.01m 임계), `FairinoWhyItMovedLabel` 다관절 상위 2개 요약 + XYZ 성분별 표시
6. **P5 UI 편의**: TopBar에 기즈모 토글 + 트레일 Clear 버튼, `FairinoStatePanel` EE XYZ RGB 색상 코딩
7. **공용 컴포넌트 추출**: `SharedLineMaterial` (EETrailRenderer/DisplacementArrow/JointRotationHandle Material 통합), `FairinoRobotConfig.GetMediumSpeedAcc()`, `FR5PosePresets.All` 캐시
8. **자기리뷰 버그 6건 수정**: OnHandleDragged 슬라이더 동기화, 핸들 Ray-plane 히트 판정, EndDrag 선택 해제, 공유 Material 색상 오염, TCP 패널 재진입 가드, 핸들 이벤트 중복 바인딩
9. 검증: EditMode 테스트 green. PlayMode 시각 검증 미완 (MCP 스크린샷 대기)

## 이전 턴 반영 내용 (FR5 RobotControl + Camera Director)
1. `RobotControl.unity`를 추가하고 Build Settings index `6`에 등록해 FR5 전용 제어 콘솔 진입점을 만들었다.
2. `RobotControlSceneCoordinator`는 `Mock ON` 기본 시작, FR5 강제 selection, panel auto-wire, control prefab 복원을 담당하도록 보강했다.
3. FR5 사용 경로를 분리했다.
   - showroom preview: `Assets/Runtime/Resources/Robots/FAIRINO_FR5.prefab`
   - robot control: `Assets/Runtime/Resources/Robots/FAIRINO_FR5_Control.prefab`
4. `QaToolsMenu`의 FR5 import 흐름은 preview prefab과 control prefab을 각각 저장하도록 확장했다.
5. `RobotLibrary`/detail drawer에서 FR5에 한해 `Robot Control` CTA가 `SceneId.RobotControl`로 이동하도록 연결했다.
6. 게임 씬 메인 카메라는 `SceneCameraDirector`로 중앙 관리하도록 바꿨다.
   - `Main`, `Sandbox`, `RobotControl`, `Onboarding`, `Home`의 메인 카메라 profile을 한 파일에서 관리
   - `RobotLibrary` showroom 카메라는 기존 `RobotLibraryManager`가 별도 관리
7. 카메라 줌아웃을 위해 gameplay profile과 showroom camera framing을 한 단계 더 완화했다.
8. 검증:
   - `RobotControl.unity` 씬 자산 생성 확인
   - Build Settings에서 `RobotControl` buildIndex `6` 확인
   - `SceneCameraDirectorTests` 통과
   - FAIRINO 관련 EditMode 테스트 묶음 통과
9. 남은 확인 항목은 PlayMode에서 `Robot Library -> FR5 -> Robot Control` 진입 후 실제 3D control prefab 시각 확인이다.

## 현재 사이클 반영 내용 (Home/Math Readiness + UI DS 2차 적용)
1. `Home.unity`를 재진입 허브로 실제 연결하고 `HomeContinueHubController`/`HomeContinueHubFlowService`를 기준으로 이어하기/새로 시작/수학 기초 워밍업/로봇 선택/샌드박스 흐름을 고정했다.
2. `StepProgressSaver`에 `math_readiness` track을 추가하고 `MathReadinessLessonFactory` + `MathReadinessPanel`로 완전 초보 수학 학습자용 보강 레이어를 구현했다.
3. `resume / session context`, `snapshot lite`, `Sandbox actions`를 실제 runtime baseline으로 연결했다.
4. `HomeContinueHubViewBuilder`, `MathReadinessPanel`, `SnapshotLitePanelViewBuilder`, `SandboxActionPanelViewBuilder`를 UI Design System 2차 적용 대상으로 리팩터링했다.
5. `UIComponentFactory`에 leading icon / button row helper를 추가하고, 4개 핵심 패널이 `UIDesignTokens`, `UITypography`, `UILayoutProfile`, `UIIconResolver`를 직접 소비하도록 정리했다.
6. Home 화면은 실제 렌더 확인을 마쳤고, Sandbox는 새 패널 노출까지 확인했지만 버튼/아이콘 가독성과 일부 panel overlap 정리는 아직 진행 중이다.

## 이번 턴 반영 내용 (Asset vendor/runtime hierarchy normalization)
1. `Assets/realvirtual`은 레거시 vendor source 구역으로 유지하고, 나머지 외부 소스 폴더를 `Assets/Vendors/Archive/` 아래로 재배치했다.
2. curated runtime 자산은 `Assets/Runtime/Art`, `Assets/Runtime/Prefabs`, `Assets/Runtime/Resources` 아래로 통합했다.
3. `UxDataSeeder`, `TargetMarkerVisual`, `TargetMarkerAssetResolutionTests`, 관련 문서들이 새 경로를 사용하도록 동기화했다.
4. 확인 결과 새 경로(`Assets/Vendors/Archive/*`, `Assets/Runtime/*`)는 존재하고, 기존 루트 경로(`Assets/Art`, `Assets/Prefabs`, `Assets/Resources`, `_Heathen Engineering`, `Glowing Rifts`, `HQP Studios`, `DemoRealvirtual`)는 제거되었다.
5. 검증: `dotnet build KineTutor3D.Runtime.csproj` 성공. Unity refresh/compile 성공. `pre-commit-check.sh --all`은 저장소 전반의 기존 BOM/헤더/인코딩 규칙 위반으로 `BLOCKED: 303 에러` 상태였고, 이번 폴더 이동 자체와는 별개의 기존 품질 이슈로 분류했다.

## 이번 턴 반영 내용 (Robot Library showroom direct practice flow)
1. `RobotLibrary` showroom은 첫 페이지 가운데 hero를 기준으로 좌우 3 pod가 안정적으로 보이도록 page hero 규칙과 camera framing을 정리했다.
2. `showroomOutput` RenderTexture와 camera framing은 실제 viewport rect 기준으로 계산하도록 바꿔 Game view에서 과도하게 작아 보이는 문제를 줄였다.
3. `CompareStrip`은 제거하고, 카드 클릭과 3D showroom 로봇 클릭 모두 해당 로봇의 기본 실습 경로(Guided Lesson 우선, Sandbox 차선)로 즉시 이동하도록 단순화했다.
4. `RobotPreviewPod`에 클릭용 collider를 추가해 showroom의 로봇 자체를 직접 클릭할 수 있게 했다.
5. `robot-showroom-debug` 스킬을 추가해 showroomoutput, page hero, runtime root 중복, Game/Scene fit 문제를 재사용 가능한 디버그 패턴으로 정리했다.
6. 검증: `dotnet build KineTutor3D.Runtime.csproj` 성공.

## 문서 우선순위 재정렬 (2026-03-12)
1. 실제 파일 기준으로 `Robot Library MVP`, `SCARA`, `Sandbox baseline`은 완료된 기반으로 재분류했다.
2. 차기 P0를 `Home / Continue Hub -> Sandbox polish -> resume / session context -> tablet 4DOF input usability -> snapshot lite` 순서로 재정렬했다.
3. 온보딩 `건너뛰기`는 target 기준 `Core Step 8` 직접 점프가 아니라 `Home / Continue Hub`로 연결하는 정책으로 수정했다.
4. `replay / compare`, `constraint preview`, `Instructor demo mode`, `3DOF / 6DOF`, `URDF Import`는 P1 후속 확장으로 유지했다.

## Phase 5 실행 원칙
1. Phase 5 P0는 기능 추가보다 `기반층 선행`이 우선이다.
2. 구현 순서는 `Runtime foundation -> Track-aware step foundation -> 공통 input/visualization -> Why It Moved -> Beginner Lesson 0~3`로 고정한다.
3. `Beginner Lesson 0~3`는 P0 범위지만 첫 구현 대상이 아니라 foundation 이후 consumer layer로 본다.
4. 문서 선행 sync 1회 후, 각 phase는 `구현 -> 테스트 -> self-review -> 문서 반영 -> git commit` 단위로 종료한다.

## 이번 턴 반영 내용 (Phase 5A~5B Foundation)
1. runtime state에 previous joint values, previous EE pose/position/transform, changed joint index, update cause를 추가했다.
2. `KinematicsRuntimeService`가 mutation 직전에 snapshot을 저장하고, `RecomputeForwardKinematics()`는 순수 재계산만 담당하게 유지했다.
3. `AppController` public facade에 previous/current runtime foundation 접근자를 추가했다.
4. `StepProgressSaver`를 track-aware 구조로 확장해 `KineTutor3D.CurrentTrack`, `KineTutor3D.PreKinematics.LastCompletedStep`, `KineTutor3D.CoreKinematics.LastCompletedStep`를 기준 키로 고정했다.
5. 기존 parameterless progress API는 `core_kinematics` wrapper로 유지해 Core Track MVP 하위 호환을 보존했다.
6. `TutorStepConfig`, `InteractionType`에 beginner track/consumer layer를 위한 schema 확장을 추가했다.
7. `OnboardingManager`에 beginner/core track 저장 진입점을 추가했지만, 실제 분기 UI 노출은 Phase 5E에서 연결한다.
8. EditMode 테스트를 `53/53` 통과해 기존 회귀와 track-aware foundation 계약이 함께 유지됨을 확인했다.

## Product Docs Governance
1. Canonical product docs 3종을 `docs/ref/PRD.md`, `docs/ref/WIREFRAME.md`, `docs/ref/PRODUCT-ROADMAP.md`로 고정했다.
2. 제품 상세 문서는 `docs/ref/product/` 아래로 가지치고, 루트 3문서는 요약/잠금 결정만 유지한다.
3. 제품 문서 상태는 `docs/status/PRODUCT-DOC-BOARD.md`에서만 추적한다.
4. 제품 문서 변경 시 downstream sync는 아래 규칙을 따른다.
   - `PRD.md` -> `PROJECT-STATUS.md`, `ai-context/project-context.md`, `ai-context/master-plan.md`
   - `WIREFRAME.md` -> `docs/ref/USER-FLOW.md`, `docs/ref/tutor-step-plan.md`, 필요 시 `docs/ref/architecture-diagrams.md`
   - `PRODUCT-ROADMAP.md` -> `PROJECT-STATUS.md`, `docs/status/PHASE-EXECUTION-BOARD.md`, `ai-context/master-plan.md`
5. 제품 문서 변경은 반드시 `docs/daily/MM-DD/` 로그를 남기고, 마일스톤 단위 변경이면 주간 롤업까지 반영한다.

## 이번 턴 반영 내용 (LLM / Mobile / UX-Concept 확장)
1. UX leaf 문서 확장
   - `guided-lesson.md`를 화면 단위 계약(`GL-01`~`GL-06`)으로 확장
   - `robot-library.md`를 grid/detail drawer/compare strip/mode routing 흐름으로 확장
   - `sandbox.md`를 numeric input, why-it-moved, snapshot/sequence, constraint preview, pick foundation까지 확장
2. concept 문서 확장
   - `concept-to-ui-map.md`를 15개 이상 concept와 `reference_family`, `prerequisite_concepts`, `visualization_mode` 기준으로 확장
   - 공개 자료 기준 문서 `open-robotics-reference-pack.md` 추가
3. LLM 후속 도입 문서화
   - `llm-teaching-strategy.md` 추가
   - deterministic runtime / teaching context / LLM response layer 분리 원칙 고정
4. 모바일 배포 문서화
   - `mobile-release-checklist.md` 추가
   - Android 태블릿 우선, iPad 후속, Play/App Store 준비 항목 고정
5. skill 확장
   - 기존 skill 6종에 `robot.md` 기반 규칙을 흡수
   - 새 skill `robotics-reference-to-lesson` 추가
   - `AGENTS.md`, `CLAUDE.md`, `SKILL-DOC-MATRIX.md`에 routing/매트릭스 반영
6. 현재 제품 상태 요약 문서화
   - `current-feature-checklist.md` 추가
   - 현재 있는 기능 / 없는 기능 / 우선 추가할 기능을 roadmap leaf 문서로 고정

## 이번 턴 반영 내용 (초보자 Lesson 0~3)
1. 초보자 진입 계층 추가
   - `lesson-framework.md`에 `Pre-Kinematics Lesson 0~3` / `Core Kinematics Step 1~8` 구조를 추가
   - `guided-lesson.md`에 `Beginner Mode`와 공식/행렬 최소화 규칙을 반영
2. 흐름 문서 동기화
   - `tutor-step-plan.md`를 `L0~L3 -> S1~S8` 구조로 확장
   - `USER-FLOW.md`에 `완전 초보` / `기본 개념 이해자` onboarding 분기를 추가
3. 개념/백로그 반영
   - `concept-to-ui-map.md`에 회전 원호, 끝점 경로, reach/not reach, inverse thinking 개념을 추가
   - `current-feature-checklist.md`, `milestone-backlog.md`에 `Beginner Lesson 0~3`를 P0로 반영
   - `competitive-synthesis.md`에 공식-first 진입 배제 원칙을 추가

## 이번 턴 반영 내용 (공개 레퍼런스 확장)
1. 공식 레퍼런스 적용 포인트 강화
   - `open-robotics-reference-pack.md`에 Modern Robotics, MIT Manipulation, Robotics Toolbox for Python, MoveIt 2, Unity Robotics Hub의 적용 포인트를 구체화
2. 새 content leaf 문서 추가
   - `frame-pose-teaching-notes.md`
   - `pick-foundation-state-machine.md`
3. 모델/실습 문서 동기화
   - `robot-model-library-spec.md`에 convention, joint limits, pose preset, import source 메타데이터를 추가
   - `sandbox.md`에 pick foundation 상태를 `pre_pick -> pick -> post_pick -> pre_place -> place -> post_place`로 정리

## 이번 턴 반영 내용 (에셋 수집 가이드)
1. `asset-sourcing-checklist.md`를 roadmap leaf 문서로 추가
2. 무료 소스 사이트, 검색어, intake checklist, folder placement 규칙을 고정
3. `asset-registry.md`에서 에셋 수집 기준 문서를 참조하도록 연결

## 이번 턴 반영 내용 (내부 에셋 기준선 정정)
1. 현재 Git baseline에는 `Assets/KineTutor_AssetCuration_BACKUP/`가 없고, 복구된 vendor source는 로컬 상태로 존재한다.
2. 전체 vendor 폴더를 Git에 올리는 대신, 실제 런타임에 필요한 subset만 `Assets/Runtime/Art` / `Assets/Runtime/Prefabs/Teaching` 아래로 추적한다.
3. `realvirtual`은 vendor source이면서 동시에 현재 제품에서 가장 안정적으로 재사용 가능한 기본 자산 세트로 유지한다.
4. 문서와 코드가 충돌할 때는 현재 repo 실파일 상태를 우선한다.

## 이번 턴 반영 내용 (에셋 hierarchy 검증 메모)
1. 기존 `KineTutor_AssetCuration_BACKUP` 기준 검증 문서는 참고 자료로만 유지한다.
2. 현재 구현 우선순위는 curated subset + vendor fallback이 실제 런타임에서 resolve되는지 확인하는 쪽으로 이동했다.

## 이번 턴 반영 내용 (Product Docs Governance 이식)
1. `docs/status/PRODUCT-DOC-BOARD.md` 추가
   - `prd`, `wireframe`, `product-roadmap` 3개 canonical 문서 상태를 전담 추적
2. canonical 제품 문서 3종 추가
   - `docs/ref/PRD.md`
   - `docs/ref/WIREFRAME.md`
   - `docs/ref/PRODUCT-ROADMAP.md`
3. branching 전략 반영
   - `docs/ref/product/` 아래에 foundation/ux/content/robots/roadmap leaf 문서를 추가
   - root canonical 문서는 잠금 결정 + 링크 + downstream sync만 유지하는 summary 문서로 재정리
4. 인덱스/컨텍스트 문서 동기화
   - 루트 `AGENTS.md`, `CLAUDE.md`, `docs/CLAUDE.md`, `ai-context/START-HERE.md`
   - `ai-context/master-plan.md`, `ai-context/project-context.md`
5. 운영 문서/자동화 연계 강화
   - `SKILL-DOC-MATRIX`, `INTEGRITY-REPORT`, `sprint-docs-sync`, `code-doc-align`, `docs-nightly-organizer`에 제품 문서 drift 규칙 연결

## Phase 0 체크리스트
- [x] Unity 프로젝트 생성
- [x] unity-mcp 패키지 설치
- [x] Git 초기화 및 Unity `.gitignore` 적용
- [x] 씬 baseline 확정 (`Assets/Scenes/Boot.unity`, `Assets/Scenes/Onboarding.unity`, `Assets/Scenes/Main.unity`)
- [x] Build Settings 순서 설정 (`Boot` 0, `Onboarding` 1, `Home` 2, `Main` 3, `RobotLibrary` 4, `Sandbox` 5, `RobotControl` 6, `MathReadiness` 7)
- [x] MCP 연결 스모크 확인 (telemetry/scene/console 응답)
- [x] Unity Console 컴파일 에러 0 최종 확인 (MCP 시스템 로그 제외 기준)
- [x] 공식문서 근거 검증 완료 (`docs.unity3d.com` 링크 첨부 규칙)

## 이번 턴 반영 내용 (안정성 우선 컴포넌트화 + AGENTS 계층)
1. Visualization 리팩터링
   - `RobotRenderer`를 facade로 축소
   - `RobotRigBinder`, `ScaraDonorMapper`, `DonorMeshCopier`, `RobotVisibilityProbe` 추가
   - donor path / canonical frame / visibility probe 계약 유지
2. App 리팩터링
   - `AppController`를 facade로 축소
   - `StepFlowService`, `KinematicsRuntimeService`, `KinematicsRuntimeState`, `AppUiBinder` 추가
   - step 흐름, FK 재계산, UI auto-wire 책임 분리
3. UI 경량 분리
   - `DHTableEditor`의 parse/build 책임을 helper로 분리
   - `DHTableValueFormatter`, `DHTableViewBuilder`, `DHTableRowRefs` 추가
4. 구조 문서화
   - 루트 `AGENTS.md`
   - `Assets/Scripts/App/AGENTS.md`
   - `Assets/Scripts/UI/AGENTS.md`
   - `Assets/Scripts/Visualization/AGENTS.md`
   - `docs/ref/architecture-mermaid.md`
5. 회귀 결과
   - `KineTutor3D.Runtime.csproj` 빌드 성공
   - EditMode 47/47, PlayMode 26/26 유지

## 이전 턴 반영 내용 (씬 분리 + 전역 네비게이션)
1. 씬 분리 완료
   - `Boot.unity`: 첫 방문 여부 판단 후 즉시 씬 전환
   - `Onboarding.unity`: 환영 패널과 시작/건너뛰기만 담당
   - `Main.unity`: 로봇/HUD/Visualization 전용 씬
2. 전역 씬 이동 추가
   - `Assets/Scripts/App/SceneId.cs`
   - `Assets/Scripts/App/SceneCatalog.cs`
   - `Assets/Scripts/App/SceneNavigator.cs`
   - `Assets/Scripts/App/BootSceneRouter.cs`
   - `Assets/Scripts/UI/Shared/SceneNavigationBar.cs`
3. `Main` 온보딩 의존 제거
   - `AppController`에서 `OnboardingManager.Initialize()` 경로 제거
   - `Canvas`의 `OnboardingManager`, `SpotlightOverlay` 제거
   - `TopBar`에 `SceneNavigationBar` 추가
4. 테스트 확장
   - `Assets/Tests/PlayMode/SceneFlowSmokeTests.cs` 추가
   - 결과: EditMode 47/47, PlayMode 26/26
5. HUD 아티팩트 후속 정리
   - `GlossaryPanel` 기본 활성 상태 제거 및 inactive-safe 검색으로 중앙 파란 박스 원인 경로 차단
   - `SceneNavigationBar`/공통 UI 스타일 경로 보강으로 상단 네비 버튼 red X/미표시 문제 수정
   - 현재 남은 정리 대상: `Main` 중앙 회색 상태 박스와 focus/highlight overlay 잔여물

## 이전 턴 반영 내용 (Phase 4 Visualization 마감 + URP 정상화)
1. Visualization 코어 3개 유지
   - `Assets/Scripts/Visualization/Shared/CoordConverter.cs`
   - `Assets/Scripts/Visualization/Shared/FrameGizmo.cs`
   - `Assets/Scripts/Visualization/Renderer/RobotRenderer.cs`
2. `Main.unity` canonical frame 통합
   - 기존 `frame_0`, `frame_1`을 world/joint-1의 단일 source로 승격
   - `Frame_EE`는 EE 전용 표준 frame으로 유지
   - legacy duplicate frame(`WorldFrame`, `Frame_1`)은 비활성화
3. donor mesh 교체
   - 당시 `Assets/realvirtual/3DPrefabs/ScaraRobot.prefab`을 donor source로 사용했으나, 현재 카탈로그/검증 기준은 `Assets/Runtime/Resources/Robots/ScaraRobot.prefab` 쪽으로 이관 중
   - vendor runtime 없이 `BaseVisual`, `Link0Visual`, `Link1Visual`, `EndEffectorVisualMesh`에 mesh-only 복제
   - 기존 primitive visual marker는 숨기고 FK 기반 anchor만 유지
4. 렌더 파이프라인/씬 정상화
   - `Packages/manifest.json`에 `com.unity.render-pipelines.universal@17.0.4` 추가
   - `ProjectSettings/GraphicsSettings.asset`, `ProjectSettings/QualitySettings.asset`를 `Assets/realvirtual/RenderPipelines/Resources/URP/URP-Default.asset`로 고정
   - URP 전환 과정에서 생성된 global settings / volume profile 자산을 프로젝트 기준값으로 반영
5. 학습 화면 MVP 정리
   - `TopBar` / `LeftPanel` / `RightPanel` / `BottomBar` 4영역 surface를 런타임 공통 스타일로 정리
   - 주요 UI 스크립트에 `ExecuteAlways`를 적용해 Scene View에서도 배치 구조가 보이도록 정리
   - `TemplateSelector`, `DHTableEditor`, `StepTutorPanel`, `MatrixDisplay`, `StepNavigator`가 씬 오브젝트 우선 배선 + 최소 fallback 생성 정책으로 동작
   - `TooltipSystem`, `ToastNotificationController`의 기본 시각을 디버그 텍스트에서 실제 패널 스타일로 교체
   - `Main Camera`를 Solid Color + 2DOF 학습 구도로 조정
   - Play 중 중앙 흰 사각형을 만들던 `WelcomeModal` placeholder와 viewport focus overlay를 기본 비활성으로 전환
   - 유효한 온보딩 모달이 없을 때는 placeholder를 띄우지 않고 Step 흐름으로 즉시 진입하도록 보정
6. 테스트 확장
   - EditMode: `CoordConverterTests` 추가
   - PlayMode: `VisualizationSmokeTests`에 Canvas HUD, explicit donor path, on-screen EE motion 검증 추가
   - 결과: EditMode 47/47, PlayMode 20/20

## 이전 턴 반영 내용 (Phase 0+1)
1. 공식문서 근거 문서 추가
   - `docs/ref/unity-official-evidence-phase01.md`
   - 주제: asmdef / test runner / serialization / script compilation / API compatibility
2. asmdef 3개 생성
   - `Assets/Scripts/Math/KineTutor3D.Math.asmdef`
   - `Assets/Scripts/Types/KineTutor3D.Types.asmdef`
   - `Assets/Tests/EditMode/KineTutor3D.Tests.EditMode.asmdef`
3. Phase 1 코드 구현
   - Types: `JointType`, `DHLink`, `RobotTemplate`, `Pose`
   - Math: `Vec3D`, `Mat3D`, `Mat4D`
4. EditMode 테스트 자산 추가
   - `TestTolerances`, `MatrixAssert`
   - `Vec3DTests`, `Mat3DTests`, `Mat4DTests`, `DHLinkTests`
5. 회귀 기준 유지
   - 초기 PlayMode 스모크 5건 기준을 구축했고, 현재는 `UxFlowSmokeTests` 11건 + `VisualizationSmokeTests` 4건으로 확장 완료

## Self-Review Gate (Cycle Result)
1. 기능 리뷰
   - `Main.unity` 기준 QA 범위를 고정하고 Phase 3 UI 흐름을 재검증함
   - `TemplateSelector` / `DHTableEditor` / `MatrixDisplay`가 현재 MVP 계약과 일치함
2. 코드 리뷰
   - UI 편집 경로가 기존 App/FK 파이프라인을 우회하지 않음을 유지함
   - `theta` 단일 소스 규칙(Slider only)과 입력 가드 정책이 유지됨
3. 테스트 리뷰
   - Unity Test Runner: EditMode 47/47 통과, PlayMode 20/20 통과
   - 씬 저장 확인: `Boot.unity`, `Onboarding.unity`, `Main.unity` 저장 완료
   - Unity Console 에러는 MCP 시스템 로그 외 프로젝트 코드 에러 0
   - `KineTutor3D.Runtime.csproj` 빌드는 경고만 있고 성공
   - `Assembly-CSharp.csproj`는 현재 QA 완료 기준이 아니며, 생성 csproj 불일치 이슈는 후속 추적으로 유지
4. 문서 리뷰
   - `CLAUDE.md` / `KineTutor3D_Execution_Plan` / `PROJECT-STATUS` / `PHASE-EXECUTION-BOARD` / `SKILL-DOC-MATRIX` 정합성 동기화
   - Phase 4 Visualization을 `Done`으로 유지하고 scene split/전역 네비게이션 정책을 문서에 고정함
5. 운영 스킬화
   - 기존 `debug-success-capture` 포맷으로 결과 기록 유지

## 이번 턴 반영 내용 (Phase 5D~5E 구현)
1. Phase 5D: Why It Moved explanation layer 구현
   - `WhyItMovedState`, `WhyItMovedFormatter`, `WhyItMovedPanel` 추가
   - 관절 변화 시 한국어 평문 설명, 변위 표시, 영향 링크 표시
   - EditMode 테스트 21건 추가 (WhyItMovedStateTests 8, WhyItMovedFormatterTests 13)
2. Phase 5E: Beginner Lesson L0~L3 통합
   - `BeginnerLessonFactory` (L0~L3 4개 레슨 config 팩토리)
   - `BeginnerLeftPanel` (레슨별 안내 텍스트 + CompareMode 버튼 영역)
   - `CompareModePanelHelper` (L2 J1만/J2만/둘다 비교 모드)
   - `TargetFeedbackPanel` (L3 타깃 거리 측정 + 도달 gate 보고)
   - `OnboardingManager`에 "초보자 시작" 버튼 추가
   - `AppController` track-based config loading 추가
   - EditMode 테스트 8건 추가 (BeginnerLessonFactoryTests)
3. 테스트 현황: EditMode 87/87, PlayMode 31/31

## 이번 턴 반영 내용 (Phase 5F Robot Library MVP)
1. Data Layer
   - `RobotMetadataInfo` (Types, readonly struct): 로봇 메타데이터 (id, name, dof, type, difficulty, convention, lesson/sandbox/instructor flags)
   - `RobotCatalogEntry` (Types, sealed class): 메타데이터 + nullable TemplateFactory
   - `RobotCatalog` (Templates, static class): 5개 로봇 등록 (2DOF_RR=template 있음, SCARA/6DOF/Fanuc/igus=데모퍼스트)
   - `AppController.GetAvailableTemplateNames()` → `RobotCatalog.GetAvailableRobotIds()` (템플릿 있는 것만)
   - `AppController.SelectTemplateByName()` → `RobotCatalog.CreateTemplate()` (하드코딩 제거)
   - `AppController.InitializeTemplateRuntime()` → `RobotSelectionBridge` 확인 후 적용
2. Scene Infrastructure
   - `RobotSelectionBridge` (App, static class): PlayerPrefs 기반 씬 간 로봇 선택 전달
   - `SceneId.RobotLibrary = 4` 추가 (Home 씬 도입으로 재번호), `SceneCatalog`에 등록
   - `RobotLibrary.unity` 씬 생성 (Camera + Light + Canvas + EventSystem + RobotLibraryManager)
   - `EditorBuildSettings`에 빌드 인덱스 4로 등록 (Home 씬 도입 이후 재번호)
3. UI Shell
   - `RobotLibraryManager` (UI, [ExecuteAlways]): TopBar + ScrollRect 그리드 + 상세 패널 통합
   - `RobotCardBuilder` (UI, static): 로봇 카드 UI (이름, DOF 배지, 난이도, 설명, CTA)
   - `RobotDetailDrawer` (UI, [ExecuteAlways]): 오른쪽 상세 패널 (스펙, 모드, CTA)
   - GuidedLessonSupported=true → CTA "학습 시작" → RobotSelectionBridge → Main
   - GuidedLessonSupported=false → CTA "Coming Soon" (disabled)
4. Bug Fix
   - `SceneNavigationBar.ResolveOrCreateButton()`에서 `SetAsLastSibling()` 제거: 3개 이상 navigable entry에서 child index 셔플로 인한 onClick 미스매핑 수정
5. Tests
   - `RobotCatalogTests` (8건), `RobotMetadataInfoTests` (4건), `RobotSelectionBridgeTests` (4건)
   - EditMode 107/107, PlayMode 31/31

## 이번 턴 반영 내용 (씬 UI 가시성 관리 정비)
1. 배타 그룹 패널 초기 깜빡임 수정
   - `BeginnerLeftPanel`, `MathReadinessPanel`, `TargetFeedbackPanel`, `WhyItMovedPanel`의 `Awake()`에서 `EnsureLayout()` 제거
   - `OnEnable()`에서 `EnsureLayout()` 후 `SetVisible(false)` 호출로 기본 숨김 상태 보장
   - `AppController.Start()` → `ApplyFeatureState()`가 최종 가시성을 1프레임 내에 결정
2. 불필요한 `[ExecuteAlways]` 제거 (5건)
   - `ToastNotificationController`, `SpotlightOverlay`, `HomeContinueHubController`, `SandboxActionPanel`, `SnapshotLitePanel`
   - 에디터 프리뷰가 불필요한 런타임 전용 컴포넌트에서 제거
3. Awake/OnEnable 중복 호출 정리 (9건)
   - `StepTutorPanel`, `CompareModePanelHelper`, `SceneNavigationBar`, `FocusZoneHighlighter`, `JointInputRail`, `HomeContinueHubController`, `SandboxActionPanel`, `SnapshotLitePanel`
   - `Awake()`는 cheap init(`ResolveFont()` 등)만, `OnEnable()`이 레이아웃 보장 담당
4. `SetVisible(bool)` API 표준화 (2건)
   - `StepTutorPanel`, `DHTableEditor`에 신규 추가
5. `scene-ui-visibility` 스킬 문서 현재 적용 상태 반영

## 이번 턴 반영 내용 (QA 흐름 + Sandbox 패널 겹침 수정)
1. Editor QA 인프라 추가
   - `BootScenePlayModeSetup`: `EditorSceneManager.playModeStartScene`을 Onboarding.unity로 고정하는 Editor 스크립트 (`KineTutor3D > Always Start From Onboarding` 메뉴 토글)
   - `QaToolsMenu`: PlayerPrefs 리셋 메뉴 2종 (`QA: Reset to First-Time User`, `QA: Reset to Returning User`)
   - 어떤 씬이 열려있든 Boot → Onboarding → Home → Main/Sandbox 전체 흐름 QA 가능
2. Sandbox 패널 겹침 해결
   - `SandboxActionPanel`, `SnapshotLitePanel`에 `SetVisible(bool)` API 추가
   - `SandboxSceneCoordinator.ApplySandboxPresentation()`이 학습 패널 GameObject를 `SetActive(false)`로 완전히 숨기도록 변경
   - `AppController.ApplyFeatureState()`에서 학습 모드 시 Sandbox 전용 패널을 숨기는 배타 제어 추가
   - `AppUiBinder.AutoWire()`에 `SandboxActionPanel`/`SnapshotLitePanel` 탐색 추가
3. 빌드 검증: 전체 솔루션 오류 0개

## 이번 턴 반영 내용 (MathReadiness UX 고도화 A+B+C + 모드 기반 패널 격리)
1. Phase A: 시각 피드백 강화
   - 정답/오답 버튼에 `AccentSuccess`/`AccentDanger` 색상 + `icon-check`/`icon-x-circle` 아이콘 피드백 추가
   - "Q1/2" 형식 진행 뱃지(`UIComponentFactory.CreateBadge`) 추가
   - 워밍업/본문제 시각 분리: 섹션 라벨 + 디바이더 + 배경색 차이
   - 코치 힌트 버튼에 `icon-help` leading icon 추가
2. Phase B: 마이크로 애니메이션 + 적응형 피드백
   - 피드백 텍스트 fade-in (CanvasGroup + 코루틴, 0.25s)
   - 워밍업→본문제 카드 전환 slide 애니메이션 (0.3s)
   - 적응형 힌트: `MathReadinessQuestion.attemptCount` 추적, 2회 오답 시 코치 힌트 자동 노출, 3회 오답 시 정답 선택지 하이라이트
   - `MathReadinessFormatter`에 `FormatProgressMessage`, `FormatAdaptiveHint`, `GetDirectionIconName` 추가
3. Phase C: 컨셉 시각화 + 학습 경로
   - `MathReadinessContentTheme`: 컨셉별 테마 색상 매핑 (AngleDirection=Orange, LengthAngleToPoint=Blue, DiagonalIntuition=Purple, TwoLinkComposition=Green)
   - 패널 상단 accent stripe로 현재 컨셉 시각 표시
   - `WhyItMovedPanel`에 방향 화살표 아이콘 추가 (EE 변위 기반)
   - `allCorrectFirstTry` 마스터리 추적 플래그 추가
4. 모드 기반 패널 격리 (scene-ui-visibility 스킬 적용)
   - `MatrixDisplay`, `TemplateSelector`에 `SetVisible(bool)` API 추가
   - `AppController.ApplyFeatureState()`를 `HideAllContentPanels() → Apply{Mode}Visibility()` 패턴으로 리팩터링
   - `ApplyMathReadinessVisibility()`, `ApplyBeginnerVisibility()`, `ApplyCoreVisibility()` 분리
   - MathReadiness 모드에서 StepTutorPanel/DHTableEditor/MatrixDisplay/TemplateSelector 완전 숨김
   - `scene-ui-visibility` 스킬 문서에 모드별 패널 가시성 매트릭스 추가
5. 검증: EditMode 테스트 green, PlayMode smoke 통과 (적응형 힌트 + 진행 뱃지 시나리오 추가)

## 이번 턴 반영 내용 (MathReadiness lesson shell 정리)
1. `MathReadinessPanel`을 `현재 학습 / 현재 행동 / 도움말` 3블록 구조로 다시 정리했다.
2. 첫 화면에서는 `현재 학습 + 워밍업`만 보이고, 워밍업 선택 후에만 `현재 문제`가 나타나도록 순서를 고정했다.
3. `현재 학습` 카드에서 장문 rationale은 기본 노출에서 빼고 `큰 제목 + 목표 + 짧은 설명` 중심으로 밀도를 낮췄다.
4. MathReadiness에서는 우측 패널을 숨기고, 상단 바는 `KineTutor3D + 수학 기초 워밍업 · 단계` 중심의 조용한 구조로 정리했다.
5. 하단은 조인트 조작과 `Prev / Next / Skip`을 하나의 컨트롤 바로 재정렬했고, `Next`를 메인 액션으로, `Skip`을 약한 보조 액션으로 낮췄다.
6. 검증: `dotnet build robotapp2.sln` 성공, EditMode `201/201`, PlayMode `45/45` 통과.

## 이번 턴 반영 내용 (MathReadiness 기준선 + 조작 먼저 흐름)
1. `AngleReferenceMarker`를 추가해 MathReadiness 3D 뷰포트에서 0°/90°/180° 기준선과 라벨을 표시하도록 했다.
2. `MathVisualOrchestrator`가 M0~M3 프리셋에서 기준선 마커를 관리하도록 확장했다.
3. `MathReadinessQuestion`에 목표 각도/조인트/지시문 필드를 추가하고, `InteractionType.SliderReachTarget`로 목표 도달 게이트를 분리했다.
4. `MathReadinessPanel`은 `조작 지시 -> 목표 도달 -> 확인 질문` 상태머신으로 전환했고, `targetBadge`와 `manipulationInstruction`을 추가했다.
5. `MathReadinessLessonFactory`는 M0~M3를 실모델 기준 좌표와 조작 우선 시나리오로 다시 작성했다.
6. 검증: `dotnet build robotapp2.sln` 성공, EditMode `210/210` 통과, MathReadiness PlayMode 핵심 시나리오(`M0 조작 시작`, `기준선 노출`, `목표 도달 후 질문`, `정답 후 Next`, `M3 두 관절 순서 무관`) 개별 통과. 전체 PlayMode assembly는 기존 Visualization smoke 실패로 별도 후속 정리가 필요하다.

## 이번 턴 반영 내용 (Phase 5G Tests + Docs 최종 정리)
1. 문서 Sync 체크리스트 7항목 점검 완료:
   - `current-feature-checklist.md`: FR5/Camera/IVisibilityControllable/ViewBuilder/TokenMigration 7개 항목 추가
   - `architecture-mermaid.md`: 8개 씬 전체 flowchart + Scene Build Settings 표 + 씬 coordinator 반영
   - `PHASE-EXECUTION-BOARD.md`: IVisibilityControllable/OnboardingViewBuilder/5G Tests+Docs 행 추가
   - `tutor-step-plan.md`: 동기화 완료 확인 (변경 불필요)
   - `USER-FLOW.md`: 동기화 완료 확인 (변경 불필요)
2. EditMode 테스트 보강: `OnboardingViewBuilderTests`, `VisibilityControllableContractTests`, `TargetFeedbackPanelTests` 추가
3. 검증: `dotnet build KineTutor3D.Runtime.csproj` 성공

## 이번 턴 반영 내용 (Phase 5G Tests + Docs 최종 정리)
1. Phase 5 전체 완료 (5A~5G Done)
   - 5A: Runtime foundation (snapshot/update cause)
   - 5B: Track-aware step foundation
   - 5C: Joint Numeric Input + Highlight
   - 5D: Why It Moved explanation layer
   - 5E: Beginner Lesson L0~L3 integration
   - 5F: Robot Library MVP
   - 5G: Tests + Docs sync
2. `current-feature-checklist.md` 갱신: 완료된 기능 8개를 "현재 있는 기능"으로 이동
3. 전체 문서 동기화: PROJECT-STATUS, PHASE-EXECUTION-BOARD, PRODUCT-DOC-BOARD, SKILL-DOC-MATRIX, INTEGRITY-REPORT, master-plan, project-context, CLAUDE.md, architecture-mermaid
4. 스킬 라우팅 검증 리포트 추가: 13/13 스킬, 112/114 문서 도달 가능 (98.2%)
5. 테스트 현황: EditMode 107/107, PlayMode 30/30

## 다음 작업
1. Sandbox polish 마감: 버튼/아이콘 가독성 정리
2. tablet 4DOF rail 사용성 보정
3. `asset subset Git tracking` 마무리
4. replay / constraint preview 설계 진입
5. Phase 6 CI/CD 실주행 확인

## 이번 턴 반영 내용 (Page QA Matrix baseline)
1. `docs/archive/legacy/page-qa/PAGE-QA-MATRIX-2026-03-23.md`를 추가해 당시 실제 진입 가능한 페이지 기준 QA baseline을 잠갔다.
2. 감사 범위를 `Onboarding`, `Home / Continue Hub`, `Guided Lesson`, `Math Readiness`, `Robot Library`, `Sandbox`로 고정하고, `Boot`는 route-only로 분리했다.
3. `Instructor Mode`, `Progress`, `Settings`는 이번 패스에서 점수 제외 IA gap으로만 기록했다.
4. 페이지별 공통 판정 축을 `기능 충족도 / 진입 가능성 / 레이아웃 무결성 / UI 일관성 / UX 흐름 품질`로 고정했다.
5. 우선순위 결과는 `Sandbox -> Guided Lesson -> Robot Library -> Math Readiness -> Home -> Onboarding` 순으로 정리했다.
6. Unity Console 기준 `Assets/Scripts/App/AppController.cs(358,37)` compile error를 `Guided Lesson`과 `Sandbox` 공통 blocker로 기록했다.

## 이번 턴 반영 내용 (Sandbox layout hardening)
1. `SandboxActionPanelViewBuilder`를 수정해 Sandbox 액션 패널이 독립 overlay처럼 뜨지 않고 `LeftPanel` 내부를 채우도록 정리했다.
2. `SnapshotLitePanelViewBuilder`를 수정해 Snapshot 패널이 독립 overlay 대신 `RightPanel` 내부를 채우도록 정리했다.
3. 고정 좌표 기반 배치를 줄이고 side-panel 귀속 구조로 바꿔 세로 비율/태블릿에서의 패널 겹침 위험을 낮췄다.
4. `dotnet build KineTutor3D.Runtime.csproj`는 계속 성공했고, Unity refresh 후 콘솔 재확인에서는 앞서 보이던 compile error가 재발하지 않았다.

## 이번 턴 반영 내용 (Guided Lesson sandbox entry + Robot Library modal)
1. `AppController.OpenCurrentRobotSandbox()`를 추가해 현재 학습 중인 로봇 상태로 Sandbox 씬으로 바로 이동할 수 있게 했다.
2. `TemplateSelector` 상단 바에 `BtnLessonOpenSandbox`를 추가해 `Main` 씬에서 lesson shell 안쪽의 Sandbox 진입 CTA를 명시적으로 제공하도록 정리했다.
3. `RobotDetailDrawer`는 우측 drawer overlay 대신 dimmed modal overlay 구조로 바꿔 grid를 가리는 충돌을 줄였다.
4. `dotnet build KineTutor3D.Runtime.csproj`는 계속 green이며, Unity Console 재확인에서도 새 compile error는 보이지 않았다.

## 이번 턴 반영 내용 (Page-by-page QA packets)
1. `docs/status/page-qa/README.md`를 추가해 공통 QA 준비 절차와 페이지별 runbook 진입점을 정리했다.
2. `Onboarding`, `Home / Continue Hub`, `Guided Lesson`, `Math Readiness`, `Robot Library`, `Sandbox` 각각에 대해 개별 QA 체크시트를 만들었다.
3. `QaToolsMenu`에 페이지별 준비 메뉴(`Prep Home`, `Prep Guided Lesson`, `Prep Math Readiness`, `Prep Robot Library`, `Prep Sandbox`)를 추가해 수동 QA 진입 비용을 줄였다.
4. 각 runbook에는 진입 경로, 핵심 CTA, 겹침 체크, UI 일관성 체크, UX 체크, 오브젝트 이름까지 포함해 바로 검수 가능한 형태로 준비했다.

## 이번 턴 반영 내용 (FAIRINO FR5 reference + skill)
1. `docs/ref/product/robots/fairino-fr5-integration-reference.md`를 추가해 FAIRINO FR5 6축 실기 로봇 연동용 공식 source map을 정리했다.
2. 문서에는 FR5 하드웨어 baseline, 설치 조건, load curve, DH 다운로드, FR5 drawings, C# SDK, status feedback protocol, command protocol 링크를 묶었다.
3. `FR5 연결 패널`, `IFairinoRobotClient adapter`, `errcode UI 번역`이 무엇인지 plain-language 설명을 추가해 문서만 읽어도 역할을 이해할 수 있게 했다.
4. `.claude/skills/kinetutor-guide/content/fairino-fr5-integration/SKILL.md`를 추가해 이후 FR5 Unity 제어/SDK 연결/상태 피드백 작업에 재사용할 수 있게 했다.
5. `open-robotics-reference-pack`, `CLAUDE.md`, `SKILL-DOC-MATRIX`, `docs/daily/03-13/fairino-fr5-reference-and-skill.md`와 함께 동기화했다.

## 이번 턴 반영 내용 (Scene hierarchy normalization)
1. `Boot.unity`에 실제 `Main Camera` 루트를 추가해 라우터-only 씬에서도 최소 시각 기준을 갖추도록 정리했다.
2. `Home.unity`에 실제 `Main Camera` 루트를 추가해 fallback runtime camera 대신 씬 구조 자체를 정상화했다.
3. `Onboarding.unity`에서는 `Canvas`의 `SceneNavigationBar` 컴포넌트를 제거해 온보딩 모달과 전역 네비가 구조적으로 섞이지 않도록 정리했다.
4. `RobotLibrary.unity`의 `EventSystem`은 `StandaloneInputModule`에서 `InputSystemUIInputModule`로 통일했고, `Main Camera`에 `AudioListener`를 추가했다.
5. 결과적으로 `Boot / Home / Onboarding / RobotLibrary`가 `카메라 + EventSystem + Canvas + 페이지 전용 루트` 기준에 더 가깝게 정렬되었다.

## 이번 턴 반영 내용 (Static-scene-first UI: Onboarding / Home / RobotLibrary)
1. `Onboarding`은 카드형 모달 구조를 유지하되, 편집 가능한 `SerializeField` 레이아웃 값을 기준으로 Scene 저장 상태를 우선 사용하는 방향으로 정리했다.
2. `HomeContinueHubController`는 저장된 `HomeContinueHubRoot`와 버튼/텍스트를 먼저 바인딩하고, 없을 때만 runtime build를 수행하도록 바꿨다.
3. `RobotLibraryManager`는 저장된 `TopBar`, `ScrollArea`, `GridContent`, `RobotDetailDrawer`를 먼저 바인딩하고, 누락 시에만 fallback 생성하도록 바꿨다.
4. 결과적으로 `Onboarding`, `Home`, `RobotLibrary`는 사람이 씬에서 직접 다듬은 UI 구조를 우선 유지하는 정적 씬 UI 우선 구조로 이동했다.

## 이번 턴 반영 내용 (Onboarding beginner direct-to-math-readiness)
1. `OnboardingManager.BeginAsBeginner()`를 수정해 `초보자 시작` 클릭 시 `Home`을 거치지 않고 `Main`의 `Math Readiness`로 직접 진입하도록 바꿨다.
2. 진입 시 `MathReadinessTrack`과 `2DOF_RR` 선택을 함께 저장해 첫 화면부터 보강 학습 패널이 열리도록 정리했다.
3. `MathReadinessFlowSmokeTests`를 `Onboarding -> Main` 직행 기준으로 갱신했다.
4. `USER-FLOW`, `tutor-step-plan`, `page-qa/onboarding` 문서를 현재 runtime에 맞춰 동기화했다.
