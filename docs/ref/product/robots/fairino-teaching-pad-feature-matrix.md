# FAIRINO 티칭패드 기능 매트릭스

## Purpose
- FAIRINO 공식 티칭패드/WebApp 기능을 `robotapp2`의 현재 Unity 구현과 1대1로 대조한다.
- 제품/UX/구현 우선순위를 `있음 / 부분 구현 / 없음 / 실기 검증 필요` 4단계로 빠르게 판단할 수 있게 한다.
- 이후 `나만의 티칭패드` 구현 범위를 제조사 복제와 KineTutor3D 차별화 관점으로 동시에 정리한다.

## Parent Doc
- [fairino-fr5-integration-reference.md](./fairino-fr5-integration-reference.md)
- [robotcontrol-soft-teaching-pad.md](../ux/robotcontrol-soft-teaching-pad.md)

## When To Read
- FAIRINO FR5용 Unity 소프트 티칭패드 기능 범위를 정할 때
- 제조사 티칭패드와 현재 `RobotControl` 씬의 차이를 빠르게 파악할 때
- 어떤 기능을 먼저 제품화할지 우선순위를 잡을 때

## Status Legend
- `있음`: 현재 저장소에 제품 수준의 기본 구현이 있고 사용 흐름이 성립한다.
- `부분 구현`: 일부 기반은 있으나 제조사 기능을 대체하기엔 UX/기능/구조가 부족하다.
- `없음`: 현재 저장소에 해당 기능 계층이 사실상 없다.
- `실기 검증 필요`: 코드 경로는 있으나 실제 FR5 컨트롤러/현장 하드웨어 기준 검증이 남아 있다.

## Official Source Baseline
- [Teaching pendant software](https://fairino-doc-en.readthedocs.io/latest/CobotsManual/teaching_pendant_software.html)
- [SDK Manual](https://fairino-doc-en.readthedocs.io/latest/SDKManual/index.html)
- [C# Robot Basics](https://fairino-doc-en.readthedocs.io/latest/SDKManual/C%23RobotBase.html)
- [C# Robot Motion](https://fairino-doc-en.readthedocs.io/latest/SDKManual/C%23RobotMovement.html)
- [C# Robot IO](https://fairino-doc-en.readthedocs.io/latest/SDKManual/C%23RobotIO.html)
- [C# Robot Status Check](https://fairino-doc-en.readthedocs.io/latest/SDKManual/C%23RobotStatusInquiry.html)
- [C# Robot Peripherals](https://fairino-doc-en.readthedocs.io/latest/SDKManual/C%23RobotPeripherals.html)
- [C# Other interfaces](https://fairino-doc-en.readthedocs.io/latest/SDKManual/C%23RobotOther.html)
- [FAIRINO SimMachine](https://fairino-doc-en.readthedocs.io/latest/VMMachine/controller_virtual_machine.html)
- [Download hub](https://fairino-doc-en.readthedocs.io/latest/download.html)

## Last Updated
- 2026-03-31 (KST)

## 공식 근거 사용 규칙
- `공식 문서 기준`은 제조사 WebApp/티칭패드에서 사용자가 보는 기능 범위를 뜻한다.
- `대표 API`는 해당 기능을 Unity에서 연결할 때 우선 검토할 SDK 메서드다.
- `공식 출처`는 가능한 한 최신 `latest` 문서를 우선 사용한다.
- 특정 C# 세부 페이지가 `latest`에서 직접 열리지 않거나 인덱스만 확인되는 경우, `SDK Manual index` 기준으로 기능 존재를 확인하고 해당 행의 상태를 보수적으로 유지한다.
- `실기 검증 필요`는 문서와 코드가 있어도 실제 FR5 컨트롤러에서 아직 현장 검증을 마치지 않았다는 뜻이다.

## 1:1 기능 매트릭스

| 제조사 기능 | 공식 문서 기준 | 공식 출처 | 대표 API | 현재 Unity 상태 | 현재 구현 근거 | 핵심 갭 / 다음 액션 |
|---|---|---|---|---|---|---|
| 연결 / 해제 / Enable | 티칭패드 초기 화면 control area | Teaching pendant software, C# Robot Basics | `RPC`, `CloseRPC`, `RobotEnable` | `실기 검증 필요` | `IFairinoRobotClient`, `FairinoConnectionService`, `FairinoConnectionPanel` | 코드 경로는 있으나 실제 FR5 `Connect -> Enable` 현장 검증이 남아 있다 |
| Start / Stop / Pause / Resume 프로그램 실행 | 상단 control area, WebApp program use | Teaching pendant software, SDK Manual index | `ProgramLoad`, `ProgramRun`, `ProgramPause`, `ProgramResume`, `ProgramStop` | `없음` | 현재는 waypoint 재생만 있음 | Lua/WebApp 프로그램 load/run/pause/resume 계층을 별도 설계해야 한다 |
| 수동/자동 모드 표시 | 상태 바 `Automatic mode`, `Manual mode` | Teaching pendant software, C# Robot Basics | `Mode` | `부분 구현` | `SetMode`, `EnsureAutoMode`, 연결 패널 mode 표시 | 모드 전환 UX와 권한/실패 복구 시나리오가 아직 얕다 |
| Drag 상태 표시 / drag teach 종료 | 상태 바 `Drag`, TPD 단계 | Teaching pendant software, C# Robot Basics | `DragTeachSwitch`, `IsInDragTeach` | `부분 구현` | `ExitDragTeach`, `IsInDragTeach`, connection diagnostics | drag 진입/해제 UX와 초보자 안내 흐름이 없다 |
| Tool / Wobj / 외부축 / Load 상태 표시 | 상태 바 및 접힘 패널 | Teaching pendant software, SDK Manual index, C++ Common Settings | `GetActualTCPNum`, `GetActualWObjNum`, `GetCurToolCoord`, `GetCurWObjCoord` | `부분 구현` | Tool/User, fault/safety는 표시됨 | 외부축 번호, payload, load number, 좌표계 이름 표시가 빠져 있다 |
| 실시간 joint / TCP / robot state 표시 | Robot / Status query | C# Robot Status Check | `GetActualJointPosDegree`, `GetRobotRealTimeState` | `있음` | `ReadState`, `FairinoStatePanel`, diagnostics drawer | live 실제 정확도와 polling 품질은 현장 검증이 필요하다 |
| 에러 / fault / safety 표시 | 상태 바 error, safety stop, error clear | Teaching pendant software, C# Robot Status Check, SDK Manual index | `GetSafetyStopState`, `GetSafetyCode`, `ResetAllError` | `실기 검증 필요` | `ReadControllerFault`, `GetSafetyCode`, `ResetErrors` | 하드웨어 fault 종류별 UX와 실기 재현 검증이 남아 있다 |
| Joint slider 다축 링크 조작 | `Joint Jog -> Multi-axis linkage` | Teaching pendant software, C# Robot Motion | `MoveJ` | `있음` | `FairinoJointControlPanel`, 6축 slider, Apply 계열 흐름 | 초보자용 목표/현재/예상 경로 안내를 더해야 한다 |
| Single-axis jog long-press | `Joint Jog -> Single-axis jog` | Teaching pendant software, C# Robot Motion | `StartJOG`, `StopJOG`, `ImmStopJOG` 계열 검토 | `부분 구현` | joint ring handle, slider preview | 제조사식 press-and-hold jog 버튼과 threshold 정책이 아직 없다 |
| Base / Tool / Wobj jog | `TCP -> Base/Tool/Wobj Jog` | Teaching pendant software, C# Robot Motion, SDK Manual index | `ServoCart` 또는 jog 계열 조합 검토 | `없음` | TCP 입력 MoveL만 있음 | 좌표계 선택형 jog panel이 필요하다 |
| Cartesian pose 입력 후 이동 | `Move -> Calculate joint position -> Move to this point` | Teaching pendant software, C# Robot Status Check, C# Robot Motion | `GetInverseKinHasSolution`, `MoveL`, `MoveJ` | `부분 구현` | `FairinoTcpControlPanel`, `MoveL`, FK facade | IK 사전 계산, 도달 가능성 체크, 예측 경로 시각화가 부족하다 |
| MoveJ | SDK motion / WebApp apply | C# Robot Motion | `MoveJ` | `실기 검증 필요` | `MoveJ`, live confirm dialog, speed preset | 실제 FR5 소범위 모션 검증이 남아 있다 |
| MoveL | SDK motion / WebApp move | C# Robot Motion | `MoveL` | `실기 검증 필요` | `MoveL`, TCP panel, dry-run | 실제 FR5 안전 검증과 도달 불가/특이점 UX 보강이 필요하다 |
| ServoJ / ServoCart | SDK servo motion | C# Robot Motion | `ServoMoveStart`, `ServoJ`, `ServoCart` | `부분 구현` | 인터페이스/버튼은 있으나 Live v1에서 비활성화 | 초고속 연속 제어는 후속 안전 정책과 rate limit이 필요하다 |
| Teaching point 저장 | `Teaching point record` | Teaching pendant software, C# Robot Status Check | `GetRobotTeachingPoint` 계열 조회 검토 | `부분 구현` | waypoint save/play/export/import | 공식 point record 개념과 좌표계별 point 관리가 아직 없다 |
| TPD 기록 / 재생 / 편집 | `TPD (Teach-in programming)` | Teaching pendant software, SDK Manual index | `SetTPDParam`, `StartTPDRecord`, `StopTPDRecord`, `LoadTPD`, `MoveTPD` 계열 검토 | `부분 구현` | `WaypointCycleRunner`는 있으나 TPD는 아님 | drag-based trajectory record, preload, trim, playback UI가 필요하다 |
| I/O 수동 제어 / 상태 표시 | `I/O`, status pane | Teaching pendant software, C# Robot IO | `SetDO`, `SetToolDO`, `SetAO`, `GetDI` 계열 | `없음` | 관련 UI/adapter 없음 | DO/AO/DI/AI read/write와 상태 패널을 별도 구현해야 한다 |
| Gripper 상태 / 제어 | status pane / SDK peripheral | Teaching pendant software, C# Robot Peripherals | `SetGripperConfig`, `ActGripper`, `MoveGripper`, `GetGripperMotionDone` | `없음` | SDK 가능성만 문서화 | gripper config/control/status 계층과 UI가 필요하다 |
| 외부축 제어 | `Eaxis move`, ExAxis status | Teaching pendant software, SDK Manual index | extended axis 계열 API 검토 | `없음` | 현재 FR5 본체만 대상 | E-axis는 제품 2차 범위로 분리하는 편이 안전하다 |
| FT / force 관련 기능 | `FT`, force control state | Teaching pendant software, SDK Manual index | force control / sensor 계열 API 검토 | `없음` | 현재 UI/adapter 없음 | 교육용 force 시각화 또는 실기 force pane을 별도 설계해야 한다 |
| 3D 로봇 시뮬레이션 | teaching pendant 3D simulation robot | Teaching pendant software, SimMachine | Unity visualization layer | `있음` | `FairinoUrdfJointDriver`, frame gizmo, trail, orbit camera | 제조사보다 강한 시각 교육층으로 확장할 가치가 높다 |
| 좌표계 시각화 | base/tool/wobj/exaxis coordinate display | Teaching pendant software, SDK Manual index, C++ Common Settings | tool/wobj/exaxis 조회 계열 | `부분 구현` | frame gizmo, Tool/User 숫자, WhyItMoved | 좌표계 토글, 이름, 기준 설명, Wobj 시각화가 더 필요하다 |
| 궤적 표시 | trajectory drawing | Teaching pendant software | Unity trail layer | `있음` | `EETrailRenderer`, `ClearTrail` | 예측 궤적과 실제 궤적 이중 비교까지 가면 차별화된다 |
| 툴 모델 import | import tool model | Teaching pendant software | 별도 SDK보다는 자산 import 흐름 | `없음` | 현재 경로 없음 | 후속 제품화 전에는 낮은 우선순위 |
| 버전 / 진단 표시 | system info, SDK version | C# Robot Basics, C# Other interfaces | `GetSDKVersion`, `GetSoftwareVersion`, `GetFirmwareVersion` | `부분 구현` | version info, diagnostics drawer | 컨트롤러/SDK/펌웨어 표기와 로그 수집 UX를 강화해야 한다 |
| 로그 / 증거 수집 | Log / query / SDK download helpers | C# Robot Basics, C# Other interfaces | `LoggerInit`, `SetLoggerLevel`, `RbLogDownload`, `AllDataSourceDownload`, `DataPackageDownload` | `부분 구현` | diagnostics drawer만 존재 | log export, session report, 현장 점검 팩이 필요하다 |
| 사용자 / 권한 레벨 | user login and permission | Teaching pendant software | Unity 제품 계층 별도 설계 | `없음` | Unity 내 역할 체계 없음 | 초보자/강사/운영자 역할 모드를 제품 레벨에서 설계해야 한다 |
| 다국어 | teach pendant language | Teaching pendant software | Unity localization/token JSON 구조 | `부분 구현` | 저장소 전반 한국어 가능성 있음 | 현재 목표는 한국어 우선, 후속 다국어 구조는 토큰/JSON로 설계 |
| 태블릿 사용성 | 공식 WebApp + project tablet-first policy | Teaching pendant software, SimMachine, tablet-first-policy | Unity `UILayoutProfile` 계열 | `부분 구현` | 저장소 정책은 `Desktop + Tablet first` | `RobotControl`의 L4 태블릿 반응형 검증이 아직 없다 |
| authored-first UI | 저장소 UX 원칙 | 저장소 설계 원칙 | `TryBindExistingLayout(...)` | `부분 구현` | `FairinoRobotControlViewBuilder.TryBindExistingLayout(...)` | 일부 패널은 authored-first 축소가 남아 있다 |
| 디자인 토큰 공유 | 저장소 UI 원칙 | 저장소 설계 원칙 | `UIDesignTokens`, `UIComponentFactory`, `UILayoutProfile` | `부분 구현` | `UIDesignTokens`, `UIComponentFactory` 사용 | `RobotControl` 전면 L3 수준 토큰화가 아직 미완이다 |
| 초보자 안내 팝업 / 친절한 안내 | 제품 요구사항 | 제조사 문서에는 직접 없음, KineTutor3D 차별화 항목 | Unity help/popup layer | `없음` | live confirm dialog 정도만 존재 | 위험/실수/도달 불가/연결 끊김 상황별 친절한 팝업 계층이 필요하다 |
| 로봇 움직임 사전 프리뷰 | Unity 차별화 후보 | 제조사 3D simulation + KineTutor3D 확장 | FK/IK preview + ghost + trail | `부분 구현` | dry-run, preset animation, trail, 3D mirror | 제조사보다 강한 `예상 자세/예상 경로/위험 구간` 프리뷰로 키워야 한다 |

## 공식 제약 메모
- 공식 C# 기본 예제는 문서상 `robot powered on and enabled`, `no interference in workspace`를 기본 전제로 둔다.
- 공식 문서상 controller 기본 IP는 `192.168.58.2`가 반복적으로 등장하므로, Unity 기본값과 현장 IP를 혼동하지 않도록 진단 UI에 명시해야 한다.
- TPD는 제조사 문서상 `manual mode`, `drag teach`, `trajectory point upper bound`, `start pose 일치` 같은 절차 제약이 강하다.
- gripper, conveyor, laser, force, extended axis는 SDK 범위가 넓지만 현재 `RobotControl`의 V1 범위를 크게 넘어선다.
- SimMachine은 실제 하드웨어 없이 WebApp 구조를 미리 확인하는 데 유용하지만, 실기 safety / payload / fault 행위까지 대체 검증하지는 못한다.

## 요약 판정

### 이미 강한 영역
- 3D 시각화
- joint 기반 미리보기
- Why It Moved 설명
- waypoint 기반 간단한 teaching
- authored-first로 갈 수 있는 UI 셸 기반

### 지금 비어 있는 큰 영역
- I/O
- 프로그램 load/run/pause/resume
- TPD 실기 기록/재생
- gripper / peripheral
- 사용자 권한 레벨
- 초보자용 친절 팝업 시스템

### 코드가 있지만 실기 검증이 남은 영역
- Connect / Enable
- MoveJ / MoveL
- fault / safety / reset
- live 상태 동기화 정확도

## 제품 우선순위 제안

### 1단계: 소프트 티칭패드 MVP
- 연결 / enable / sync / moveJ / moveL
- 한국어 상태 패널
- joint/tcp preview
- 친절한 위험 팝업
- authored-first shell + design token 정리
- tablet 레이아웃

### 2단계: 교육형 차별화
- 예상 경로 프리뷰
- 도달 가능 / 불가능 / 특이점 사전 경고
- 초보자 설명 패널
- replay / 비교 / 기록
- guided recipe 방식 teaching

### 3단계: 제조사 패드 대체 범위 확대
- I/O
- point manager
- TPD 유사 기록/재생
- gripper
- 로그 수집 / 현장 점검 보고서

### 4단계: 고급 운영 기능
- 실제 프로그램 실행 연동
- 역할 기반 모드
- 외부축 / force / peripheral
- 다국어

## 반드시 넣을 차별화 포인트
- `Dry-run first`: 실기 명령 전에 Unity에서 먼저 예상 자세와 예상 경로를 보여준다.
- `왜 움직이는지 설명`: 관절 변화가 TCP에 어떤 영향을 주는지 초보자 언어로 바로 설명한다.
- `위험 구간 선제 경고`: joint limit, singularity, unreachable, collision risk를 사전에 시각화한다.
- `학습형 팝업`: 단순 오류창이 아니라 “왜 안 되는지 / 어떻게 고치는지 / 다음으로 뭘 눌러야 하는지”를 안내한다.
- `역할별 모드`: 초보자 / 강사 / 운영자 모드로 UI 밀도를 다르게 만든다.
- `세션 기록`: 누가 어떤 포즈를 저장했고 어떤 경고를 받았는지 남겨 강의/시연/복기에 활용한다.

## 지금 추가하면 좋은 기능
- `예상 자세 고스트`: 현재 로봇과 목표 로봇을 겹쳐 보여주는 ghost preview
- `충돌/간섭 히트맵`: 위험 링크를 빨간색으로 강조
- `한글 작업 레시피`: Home, Pick, Place, Inspect 같은 작업 카드형 매크로
- `실수 복구 패널`: 최근 명령 취소, 이전 안전 포즈로 복귀
- `태블릿 큰 버튼 모드`: 장갑/현장 터치용 대형 버튼 레이아웃
- `강사용 오버레이`: 학생 조작을 미러링하고 설명 텍스트를 함께 띄우는 발표 모드
- `현장 점검 리포트`: 연결 상태, 버전, fault, 마지막 명령, 최근 에러를 한 번에 내보내는 점검 팩
