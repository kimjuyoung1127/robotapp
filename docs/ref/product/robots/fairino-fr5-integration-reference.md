# FAIRINO FR5 Integration Reference

## Purpose
- FAIRINO FR5 6-axis collaborative robot의 공식 문서를 한 곳에 묶고, Unity UI 기반 실기 제어 프로젝트에서 바로 재사용할 수 있는 source map을 남긴다.
- 저장소 안에는 요약과 연결 규칙만 남기고, 대용량 ZIP/PDF/SDK 바이너리는 직접 커밋하지 않는다.

## Parent Doc
- [robot-model-library-spec.md](./robot-model-library-spec.md)
- [open-robotics-reference-pack.md](../content/open-robotics-reference-pack.md)

## When To Read
- FR5를 Robot Library 후보나 실기 제어 대상 로봇으로 다룰 때
- Unity UI 입력값을 FAIRINO C# SDK 호출로 연결할 때
- FR5 하드웨어 사양, 설치 조건, DH 파라미터, 피드백 포트, 명령 프로토콜의 공식 출처가 필요할 때
- 공식 C# SDK 문서와 현재 repo/live evidence가 실제로 어디서 맞고 어긋나는지 봐야 할 때는 `docs/ref/product/roadmap/fr5-live-official-sdk-audit.md`를 먼저 연다

## Status
- Research / integration planning + initial runtime baseline
- 문서화와 skillization 완료
- 2026-04-29 기준 official SDK 문서 vs current repo vs field evidence를 대조한 current audit SSOT는 `docs/ref/product/roadmap/fr5-live-official-sdk-audit.md`다
- `RobotControl.unity`, `FairinoConnectionService`, FR5 config/template, preview/control prefab split까지는 코드 반영됨
- 남은 핵심 이슈는 `RobotControl`에서 URDF control prefab의 최종 visible state와 FR5 state -> 3D joint mirror 경로다
- 2026-03-17 기준 공식 C# SDK ZIP에서 `libfairino.dll`, `CookComputing.XmlRpcV2.dll`을 로컬 `Assets/Plugins/Fairino/`에 staging 완료
- 2026-03-31 기준 `LiveFairinoClient` / `FairinoConnectionService`에 아래 bring-up hardening이 반영되었다.
  - tool/user context cache
  - controller fault / safety stop cache
  - drag teach 종료 + auto mode 준비
  - reconnect / realtime sample defaults
  - Live v1에서 `ServoJ` / `ServoCart` 비활성화
- 현재 이 문서는 source map과 구조 기준선 역할로 유지한다.
- current branch의 실제 live truth, field blocker, 공식 SDK 대비 mismatch/backlog는 `fr5-live-official-sdk-audit.md`와 `FR5-LIVE-INTEGRATION-ROADMAP.md`를 우선한다.

## Last Updated
- 2026-04-29 (KST)

## Collection Verdict
- `가능`: FR5 관련 공식 자료는 FAIRINO Read the Docs, FAIRINO 공식 제품 페이지, 공식 GitHub SDK 링크에서 수집할 수 있다.
- `권장 방식`: 저장소에는 source map, 적용 규칙, 안전 메모만 남기고 실제 ZIP/PDF/SDK는 필요할 때 내려받아 작업 디렉터리 밖 또는 무시 경로에 둔다.
- `비권장`: 전체 사이트 미러링, ZIP/PDF 대량 커밋, 비공식 블로그/재배포 문서를 source of truth로 채택하는 방식.

## Official Source Map
| source | URL | collect | why it matters |
|---|---|---|---|
| FAIRINO docs index | https://fairino-doc-en.readthedocs.io/latest/index.html | 전체 탐색 진입점, chapter 구조 | FR5 관련 HTML/PDF/ZIP 링크를 공식 기준으로 다시 찾을 수 있다 |
| FAIRINO FR5 product page | https://fairino.com/en/products/123.html | 모델 요약 사양 | FR5가 `6-axis`, `5kg (Max 7kg)`, `Reach 922mm`, `Repeatability ±0.02mm`인 공식 제품 baseline을 잡는다 |
| Robot brief introduction | https://fairino-doc-en.readthedocs.io/latest/CobotsManual/robot_brief_introduction.html | FR5 movement range, DH section, DH download link | Unity 시각화/FK/robot metadata에 필요한 FR5 고유 모델 자료를 잡는다 |
| Installation manual | https://fairino-doc-en.readthedocs.io/latest/CobotsManual/installation.html#installation-requirements-for-fr5-robot | 설치 평면도, 고정 볼트, 공압 배관, 정비 여유 공간 | 실제 로봇을 준비한 현장에서 UI 프로젝트와 별도로 반드시 확인해야 할 물리 설치 기준이다 |
| Load curve section | https://fairino-doc-en.readthedocs.io/latest/CobotsManual/installation.html#load-curves-for-all-fr-series-models | FR5 load curve, CoG 대응 payload | Unity 입력값 검증과 운영 가드에서 payload 조건을 단순 중량이 아니라 CoG까지 포함해 판단하게 해준다 |
| Download hub | https://fairino-doc-en.readthedocs.io/latest/download.html | Drawings, SDK, protocol PDFs | 필요한 외부 자산을 공식 링크로 다시 내려받는 허브다 |
| FR5 drawings ZIP | https://fairino-doc-en.readthedocs.io/latest/_downloads/d0aec4a78c7e5520e502d33d18425649/FR5%20Drawings.zip | 2D/3D 치수 자료 | Unity side robot proxy, fixture clearance, UI에 표시할 envelope 정보를 보강할 수 있다 |
| FAIRINO product catalogue PDF | https://fairino-doc-en.readthedocs.io/latest/_downloads/bd8a0e09a044448752c2d1bc3273832e/FAIRINO%20Product%20Catalogue.pdf | 라인업/요약 스펙 | Robot Library card/drawer용 비교 메타데이터를 잡는다 |
| FR robots DH transformation XLSX | https://fairino-doc-en.readthedocs.io/latest/_downloads/e40731353903151d7e48b23399b3ea18/FR%20Robots%20DH%20Transformation.xlsx | FR 시리즈 DH 테이블 | FR5 template/FK 검증값의 공식 원천으로 사용한다 |
| C# SDK download | https://github.com/FAIR-INNOVATION/fairino-csharp-sdk/archive/refs/heads/main.zip | SDK source/binaries | Unity에서 가장 자연스럽게 붙일 수 있는 공식 language path다 |
| C# SDK releases | https://github.com/FAIR-INNOVATION/fairino-csharp-sdk/releases | 버전 추적 | 현장 PC와 코드 버전 호환을 명시적으로 관리할 수 있다 |
| C# robot basics | https://fairino-doc-en.readthedocs.io/latest/SDKManual/C%23RobotBase.html | connect/enable/mode/reconnect | Unity live mode의 세션 수명주기와 상태 전환 기준을 만든다 |
| C# motion manual | https://fairino-doc-en.readthedocs.io/latest/SDKManual/C%23RobotMovement.html | MoveJ, MoveL, ServoJ, ServoCart | UI 입력값을 실제 동작 명령으로 매핑하는 핵심 문서다 |
| C# data structures | https://fairino-doc-en.readthedocs.io/latest/SDKManual/C%23DataStructure.html | joint/pose/state structs | Unity <-> SDK DTO 설계와 실시간 상태 바인딩에 필요하다 |
| C# status inquiry | https://fairino-doc-en.readthedocs.io/latest/SDKManual/C%23RobotStatusInquiry.html | joint/tool pose/state getters | 화면에 실제 로봇 상태를 재투영할 때 사용한다 |
| C# others | https://fairino-doc-en.readthedocs.io/latest/SDKManual/C%23RobotOthers.html | `SetStatePeriod`, `GetStatePeriod` | 20004 port feedback cycle 관련 설정 기준이다 |
| 8083 status feedback protocol PDF | https://fairino-doc-en.readthedocs.io/latest/_downloads/7ed2c1d8ecad7b4456849d74c8fd05be/Robot%208083%20Port%20Status%20Feedback%20User%20Manual.pdf | binary feedback packet layout | 고주기 상태 수신기를 직접 만들 때 필요하다 |
| controller communication protocol PDF | https://fairino-doc-en.readthedocs.io/latest/_downloads/8111110bc2f62d9fc04a4a9c346359b9/Collaborative%20Robot%20Controller%20Communication%20Command%20Protocol%20User%20Manual.pdf | RPC ports, command taxonomy | SDK wrapper가 아닌 저수준 통신/문제해결 기준으로 사용한다 |

## Confirmed FR5 Baseline
- 모델: 6-axis articulated collaborative robot
- 정격 payload: 5 kg
- 최대 payload: 7 kg
- reach: 922 mm
- repeatability: +-0.02 mm
- 공식 DH 자료: HTML 표 + XLSX 다운로드 둘 다 존재
- 공식 도면 자료: `FR5 Drawings.zip` 제공
- 공식 C# SDK: GitHub zip + releases 제공

## Additional High-Value Official Assets
- `FRCobots-V5.0 STEP Models`, `FRCobots-V6.0 STEP Models`가 공식 다운로드에 있다.
  - Unity donor mesh, collision proxy, fixture clearance mockup을 만들 때 유용하다.
- `FRCobots-V6.0 DWG Format`이 공식 다운로드에 있다.
  - 현장 설비 배치, base plate, enclosure clearance 검토에 유용하다.
- `FAIRINO SimMachine VMware`, `FAIRINO SimMachine Docker`, `FAIRINO_SimMachine_Software`가 공식 다운로드에 있다.
  - 실제 FR5를 바로 만지지 않고도 controller-like 환경과 절차를 미리 점검할 수 있다.
- `FAIRINO ROS1`, `FAIRINO ROS2`, `moveIt2` 오픈 플랫폼 링크가 공식 문서에 있다.
  - 현재 Unity direct SDK 경로가 1순위지만, 향후 ROS bridge나 planner 연동으로 확장할 근거가 된다.
- FR5는 공식 다운로드 페이지의 robot certification 표에서 IP65, NSF, crash force, functional safety, CE-MD/EMC 등 다수 인증 항목이 체크되어 있다.
  - 교육/데모/납품 문서에서 compliance summary가 필요할 때 참고 가치가 있다.

## Confirmed Installation Notes
- FR5 설치 요구사항 문서에는 foundation flatness `<= 0.8 mm`가 명시되어 있다.
- 베이스 고정은 `4 x M8`, performance class `8.8`, tightening torque `25 Nm` 기준이 명시되어 있다.
- 외부 air pipe는 outer diameter `6 mm`가 명시되어 있다.
- 디버깅/정비를 위해 robot 주변 `1000 mm` 최소 공간이 권장된다.

## Confirmed Load / Kinematics Notes
- FR5 load curve는 payload를 단순 무게만이 아니라 load center distance와 함께 판단하게 한다.
- 문서 표에는 FR5 maximum load가 `7 kg`로 제시되고, FR5 load curve는 CoG가 멀어질수록 허용 load가 감소하는 형태로 제공된다.
- `robot_brief_introduction.html`에는 FR5 Denavit-Hartenberg table이 직접 포함되어 있고, 별도의 `FR Robots DH Transformation.xlsx`도 내려받을 수 있다.

## Version-Dependent Notes Worth Tracking
- 최신 version intro에는 `FR5 linear speed`가 `1.0 m/s -> 1.7 m/s`로 증가했다는 항목이 있다.
- 따라서 실기 제어 앱에서 속도 슬라이더 상한을 고정 상수로 두지 말고, controller software version 또는 현장 운영 정책으로 분리하는 편이 좋다.
- 최신 download 페이지에는 `FAIRINO-CobotSoftware-QX-V3.9.3.1-20260304.zip`, `FAIRINO-CobotSoftware-LA-V3.9.3.1-20260304.zip`가 노출된다.
- 현장 controller가 구버전이면 문서상 최신 SDK 기능이나 속도 상한이 바로 적용되지 않을 수 있으니, `SDK version + robot software version + control box type(QX/LA)`를 함께 기록해야 한다.

## C# SDK Interfaces To Reuse
### Session / control state
- `Robot.RPC(ip)`로 controller 통신 세션을 연다.
- `RobotEnable(1)` / `RobotEnable(0)`로 enable/disable을 제어한다.
- `Mode(mode)`로 manual/automatic mode 전환을 다룬다.
- `DragTeachSwitch(0)` / `IsInDragTeach(...)`로 drag teach 상태를 정리/확인한다.
- `SetReConnectParam(...)` 계열 API는 reconnect 정책을 명시적으로 설정할 때 사용한다.

### Point-to-point motion
- `MoveJ(...)`: joint target 기반의 기본 PTP 이동
- `MoveL(...)`: Cartesian linear move
- UI에 joint 숫자를 직접 입력하는 모드는 `MoveJ`에 먼저 매핑하는 편이 안전하다.
- UI에 pose 숫자를 입력하는 모드는 `MoveL` 또는 IK 선행 후 `MoveJ`로 분기하는 것이 좋다.
- 현재 저장소의 Live v1 경로는 `MoveJ` / `MoveL`만 실기 조작 대상으로 본다.
- `MoveJ` / `MoveL`은 controller의 현재 활성 `tool/user` 문맥을 읽어 그대로 사용한다.

### Streaming / teleop style motion
- `ServoMoveStart()`로 servo streaming 세션을 시작한다.
- `ServoJ(...)`는 joint space servo 입력에 대응한다.
- `ServoCart(...)`는 Cartesian servo 입력에 대응한다.
- slider/joystick/drag-style 조작은 일반 `MoveJ/MoveL`보다 servo API 경로가 더 자연스럽다.
- 다만 현재 KineTutor3D의 FR5 실기 bring-up 경로에서는 `ServoJ` / `ServoCart`를 일반 사용자 동작에서 의도적으로 막고, 이후 연속 제어 단계에서만 다시 검토한다.

### Status / feedback
- `GetActualJointPosDegree(...)`로 실제 joint 값을 읽는다.
- `GetRobotRealTimeState(...)`로 실시간 상태 구조체를 읽는다.
- `ROBOT_STATE_PKG` 구조체에는 frame header, robot state, joint positions, TCP pose, IO 등 화면 바인딩에 유용한 상태가 포함된다.
- `SetStatePeriod(period_ms)` / `GetStatePeriod(...)`는 20004 port feedback cycle 관리에 사용된다.
- `GetActualTCPNum(...)`, `GetActualWObjNum(...)`, `GetCurToolCoord(...)`, `GetCurWObjCoord(...)`는 현재 controller가 사용하는 좌표 문맥을 화면/명령과 맞추는 데 중요하다.

### Diagnostics / recovery
- Error code table이 별도 공식 문서로 존재한다.
  - 특히 `-3 xmlrpc communication failed`, `-4 xmlrpc interface execution failed`, `4 Interface parameter value exception`, `129 The given pose cannot be reached`는 UI 메시지 매핑 가치가 높다.
- `GetSafetyCode()`는 safety stop trigger 상태를 읽는 데 유용하다.
- `GetRobotErrorCode(...)`, `GetSafetyStopState(...)`, `ResetAllError()`는 bring-up 단계에서 가장 먼저 연결해야 할 진단/복구 API다.
- `MotionQueueClear()`는 queue 누적 상태를 정리할 때 유용하다.
- `SingularAvoidStart(...)` / `SingularAvoidEnd()`는 특이점 접근 제어를 런타임 옵션으로 붙일 여지가 있다.
- `RbLogDownload(savePath)`, `AllDataSourceDownload(savePath)`, `DataPackageDownload(savePath)`, `RobotMCULogCollect()`는 현장 이슈 재현/수집에 유용하다.

## Important Protocol Boundary
- 공식 다운로드에는 `8083 Port Status Feedback Protocol` PDF가 따로 있다.
- C# SDK 문서에는 `20004 port feedback cycle` 설정 인터페이스가 따로 있다.
- 따라서 현재 단계에서는 `8083 binary packet`과 `20004 feedback configuration`을 같은 것으로 단정하지 말고, 현장 controller firmware에서 각각 어떻게 노출되는지 별도 검증해야 한다.
- Unity live integration에서는 이 차이를 문서화된 risk로 남기고, 초기 구현은 `C# SDK status API` 우선으로 시작하는 편이 안전하다.
- 현재 branch의 실제 판정과 backlog는 위 risk까지 포함해서 `fr5-live-official-sdk-audit.md`에 유지한다.

## Recommended Unity Architecture
1. `UI input layer`
   - joint value 입력, pose 입력, 속도/가속도 입력, live/sim mode 토글을 분리한다.
2. `Validation layer`
   - joint limits, payload profile, workspace, mode, connection state를 먼저 점검한다.
3. `Simulation layer`
   - 현재 KineTutor3D FK/robot visualization을 먼저 갱신해 dry-run 결과를 보여준다.
4. `Fairino adapter layer`
   - Unity UI가 SDK 메서드를 직접 호출하지 않고 `IFairinoRobotClient` 같은 adapter를 통해 `Connect`, `Enable`, `SetMode`, `MoveJ`, `MoveL`, `ServoJ`, `ServoCart`, `ReadState`만 다루게 한다.
5. `Live robot layer`
   - 실제 명령 전송은 adapter 안에서만 일어나게 하고, 실패/에러 코드를 UI에 다시 올린다.

## Current Repo Reality Check
- 이 섹션은 `공식 source map -> Unity 구조`를 설명하는 기반 메모다.
- 현재 branch의 정확한 live truth, mismatch, backlog 판정은 `docs/ref/product/roadmap/fr5-live-official-sdk-audit.md`와 `docs/status/FR5-LIVE-INTEGRATION-ROADMAP.md`를 우선한다.
- `RobotLibrary` / showroom은 `RobotPreviewFactory`가 `Resources/Robots/FAIRINO_FR5.prefab` donor preview를 mesh-only clone으로 띄우는 경로가 이미 안정적이다.
- `RobotControl`은 `Resources/Robots/FAIRINO_FR5_Control.prefab` URDF control prefab을 사용한다.
- `Assets/Plugins/Fairino/`에는 공식 SDK ZIP에서 확인한 `libfairino.dll`, `CookComputing.XmlRpcV2.dll`이 로컬 staging 되었다.
- `LiveFairinoClient`는 현재 아래 SDK 경로를 직접 다루도록 보강되었다.
  - `RPC`
  - `RobotEnable`
  - `Mode`
  - `DragTeachSwitch`
  - `MoveJ`
  - `MoveL`
  - `GetRobotRealTimeState`
  - `GetActualJointPosDegree`
  - `GetActualTCPPose`
  - `GetActualTCPNum`
  - `GetActualWObjNum`
  - `GetCurToolCoord`
  - `GetCurWObjCoord`
  - `GetRobotErrorCode`
  - `GetSafetyStopState`
  - `ResetAllError`
  - `GetSDKVersion`
  - `GetSoftwareVersion`
  - `GetFirmwareVersion`
- 현재 `RobotControl`에서는 다음이 확인되었다.
  - control prefab 로드 성공
  - runtime gravity 낙하 방지 적용
  - URDF 기본 `Controller` 비활성화로 Input System 예외 제거
  - 연결 패널/진단 서랍에 `Mode`, `Drag`, `Tool/User`, `Safety`, `Fault`, `Reset Error` 표시 추가
  - Live v1에서는 `ServoJ` / `ServoCart` 비활성화
- 폴링 정책은 `ReadState()` 경량화가 기준이다.
  - 상세 `coord/fault/safety/sample` 조회는 `Connect`, `Enable`, `Sync`, `ResetErrors` 같은 명시적 시점에만 갱신한다.
- SDK 추가 후 드러난 Unity compile blocker는 수정 완료했고, compile 기준선은 다시 회복되었다.
- `Assets/Editor/KineTutor3D/FairinoLiveSmokeTools.cs`를 통해 `Connect -> GetVersion -> ReadState` smoke test를 바로 실행할 수 있다.
- 하지만 `FairinoConnectionService` 상태를 3D joint pose로 반영하는 전용 visual adapter는 아직 없다.
- 따라서 단기적으로는:
  - `visual-only`는 showroom donor preview 경로 재사용
  - `live-control-ready 3D mirror`는 FR5 전용 adapter 신규 구현
  로 나누어 보는 것이 안전하다.

## Current Live-Test Blocker
- 현재 남은 blocker는 SDK 부재가 아니라 `실기 컨트롤러 네트워크 응답 부재`다.
- 로컬 smoke 결과:
  - 기본 IP `192.168.58.2`
  - 기본 port `8080`
  - `CONNECT_FAIL`
- 즉 현재 로컬 프로젝트는 Live SDK 로드와 호출 준비가 되었지만, 테스트 머신에서 실제 컨트롤러 응답이 없다.
- 다음 현장 검증 순서는 아래 순서를 권장한다.
  1. Live 모드에서 `Connect`
  2. `GetVersion`, `ReadState`, `tool/user`, `fault` 확인
  3. `Drag=Off`, `Mode=Auto` 확인
  4. `Enable`
  5. `Sync`
  6. 아주 작은 범위의 `MoveJ`
  7. 아주 작은 범위의 `MoveL`
  8. `Reset Error`, `StopMotion`, reconnect UI 확인

## Live Smoke Test Entry

- Editor menu:
  - `KineTutor3D/RobotControl/Run FAIRINO Live Smoke Test`
- environment override:
  - `FAIRINO_IP`
  - `FAIRINO_PORT`
- expected order:
  - `Connect`
  - `GetVersion`
  - `ReadState`
  - `Disconnect`

이 smoke test는 의도적으로 비파괴 순서만 수행한다.
실제 모션 명령은 현장 승인 전까지 포함하지 않는다.

## Plain-Language Explanation
### `FR5 연결 패널`
- Unity 안에 만드는 실제 조작/상태 확인용 화면이다.
- 예: controller IP 입력, `Connect`, `Disconnect`, `Enable`, `Auto/Manual`, 현재 joint 값, 현재 TCP pose, 마지막 에러 메시지.
- 목적은 "사용자가 로봇 연결 상태를 확인하고, 안전하게 명령을 보내는 UI"를 만드는 것이다.

### `IFairinoRobotClient adapter`
- Unity UI 코드와 FAIRINO C# SDK 사이에 두는 중간 인터페이스다.
- Unity 버튼이 SDK DLL이나 `Robot.RPC()`를 직접 만지지 않고, adapter만 호출하게 만드는 구조다.
- 예를 들면 아래처럼 설계할 수 있다.

```csharp
public interface IFairinoRobotClient
{
    bool Connect(string ip);
    void Disconnect();
    int EnableRobot(bool enable);
    int SetMode(int mode);
    int MoveJ(double[] joints);
    FairinoRobotState ReadState();
}
```

- 이 구조를 쓰면 나중에
  - 실제 FR5 연결 구현
  - 테스트용 가짜 로봇 구현
  - SimMachine용 구현
  을 바꿔 끼우기 쉬워진다.

### `errcode UI 번역`
- FAIRINO SDK는 보통 숫자 error code를 반환하므로, 그대로 보여주면 사용자가 이해하기 어렵다.
- 그래서 UI에서는 숫자 코드를 사람이 읽을 문장으로 바꿔서 보여준다.
- 예:
  - `-3` -> `컨트롤러와 통신할 수 없습니다`
  - `4` -> `입력값 범위를 확인하세요`
  - `129` -> `현재 자세로는 목표 위치에 도달할 수 없습니다`
- 목적은 "SDK 오류를 현장 운영자가 바로 이해하고 조치할 수 있게 만드는 것"이다.

## Why These Three Matter Together
- `FR5 연결 패널`은 사용자가 직접 보는 화면이다.
- `IFairinoRobotClient adapter`는 그 화면과 실제 SDK 사이를 안전하게 분리하는 구조다.
- `errcode UI 번역`은 SDK가 주는 숫자 결과를 사람이 이해할 메시지로 바꿔 준다.
- 즉, 셋을 같이 두어야 `보이는 화면`, `안전한 연결 구조`, `이해 가능한 오류 처리`가 한 세트로 완성된다.

## Recommended Operational Guardrails
1. `Version handshake`
   - 연결 직후 SDK version, robot software version, hardware version, firmware version을 읽어 세션 헤더에 표시한다.
2. `Dry-run before live-run`
   - 같은 입력값을 먼저 KineTutor3D 내부 FK/시각화에 적용하고, 사용자가 확인한 뒤 live command를 보내게 한다.
3. `Error translation`
   - errcode를 그대로 보여주지 말고 `네트워크`, `파라미터 범위`, `도달 불가`, `세이프티 정지`, `컨트롤러 내부 오류`로 묶어 사용자 친화적으로 표시한다.
4. `Log capture`
   - live mode에서 0이 아닌 errcode가 반복되면 controller log/data package 수집 버튼을 노출한다.
5. `SimMachine path`
   - 실제 로봇을 쓰기 전, SimMachine 또는 offline STEP proxy로 연결 순서와 UI 플로우를 검증한다.

## Recommended First Implementation Slice
1. `FR5 metadata`
   - Robot Library에 FR5를 `6DOF / FAIRINO / real-robot-ready / C# SDK` badge로 추가한다.
2. `Connection panel`
   - controller IP, connect state, enable state, manual/auto mode 상태를 볼 수 있게 한다.
3. `Joint move panel`
   - 6개 joint numeric input + `MoveJ`만 먼저 붙인다.
4. `State mirror panel`
   - actual joints, TCP pose, robot state를 읽어 Unity 3D 프록시에 반영한다.
5. `Servo mode`
   - slider streaming이 필요할 때만 `ServoMoveStart + ServoJ/ServoCart`를 붙인다.

## Do Not
- 공식 ZIP/PDF/SDK를 저장소에 그대로 커밋하지 않는다.
- UI 숫자 입력값을 검증 없이 실제 로봇으로 바로 흘려보내지 않는다.
- 8083/20004 동작을 현장 검증 없이 하나의 동일 채널로 가정하지 않는다.
- KineTutor3D의 교육용 시각화와 실기 명령 전송 책임을 같은 컴포넌트에 섞지 않는다.
- firmware/software update API를 앱의 일반 사용자 흐름에 바로 노출하지 않는다.

## Downstream Sync
- `docs/ref/product/content/open-robotics-reference-pack.md`
- `.claude/skills/kinetutor-guide/content/fairino-fr5-integration/SKILL.md`
- `docs/status/SKILL-DOC-MATRIX.md`
