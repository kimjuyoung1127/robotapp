# FR5 Live Field Checklist

Last Updated: 2026-04-28 (KST)

## Purpose

맥북을 랜선으로 FAIRINO FR5 컨트롤러에 직접 연결하기 전, 현재 Pendant V3 코드가 실제 로봇 readback/live 연동으로 넘어갈 준비가 되었는지 판단하는 체크리스트다.

이 문서는 `실기 전 점검 -> SDK 교차검증 -> V3 main 병합 -> 레거시 정리 판단 -> 실시간 추적 설계` 순서로 쓴다.

## Read Order On The MacBook

현장 맥북에서는 아래 순서로만 읽는다.

1. `docs/ref/product/ux/robotcontrol-next-session-handoff.md`
2. `docs/ref/product/roadmap/fr5-live-field-checklist.md`
3. `docs/ref/product/robots/fairino-fr5-integration-reference.md`
4. 필요할 때만 FAIRINO 공식 SDK 문서와 support/download 페이지를 연다.

첫날 목표는 로봇을 움직이는 게 아니다. 맥북과 FR5가 서로 보이고, SDK 또는 bridge가 현재 관절/TCP 값을 읽고, 그 값이 파일로 계속 남는지 확인하는 것이다.

## Current Verdict

- `Pendant V3`는 아직 `main`에 바로 병합해도 된다고 보지 않는다. 맥북 현장 검증은 먼저 `codex/robotcontrol-v3-toolkit` 브랜치에서 한다.
- `V1/V2`는 지금 삭제하지 않는다. V3가 main에서 안정화되고, 실제 FR5 readback 세션이 최소 2회 성공한 뒤 제거 판단한다.
- 실제 FR5 연결 첫날 목표는 `움직임`이 아니라 `연결 성공 + 현재 값 읽기 + UI/3D 값 비교`다.
- 현재 저장소의 FAIRINO SDK staging은 `Assets/Plugins/Fairino/libfairino.dll` 기준이다. 이 파일은 Windows native DLL로 단정하지 않고 managed .NET assembly 가능성까지 probe한다. 다만 FAIRINO 공식 문서가 macOS Unity direct 실행을 명확히 보장하지 않으므로 현장에서는 direct 실패 가능성을 정상 시나리오로 본다.
- direct SDK가 실패하면 `FAIRINO_BRIDGE_URL` 기반 bridge fallback으로 readback-only를 재시도한다.
- 현재 구현 기준 커밋은 `d8c0726 Add FR5 readback-only live monitor`다.
- AI 에이전트는 live 값을 읽고 코드/시뮬레이션과 비교할 수 있어야 하지만, 실제 로봇 보정 이동은 자동 실행하지 않는다. 차이가 나면 `차이 감지 -> 실행 차단 -> 사람 확인 -> 수정/재동기화`로 간다.

## Official Source Check

- FAIRINO C# SDK 문서는 연결, enable, mode, 상태 조회, 이동 API를 제공한다.
- FAIRINO 문서의 C# 위치 단위는 position `mm`, attitude `degree`다.
- 현재 저장소 문서 기준의 `ReadState()` 경량 폴링 정책은 유지한다.
- 2026-04-28 재확인 기준, 공식 문서 최신 경로는 `fairino-doc-en.readthedocs.io/latest/SDKManual/index.html`이고, 검색 결과에는 Fairino support/download 쪽 최신 소프트웨어 목록도 별도로 노출된다. 현장에서는 실제 컨트롤러 software version을 반드시 기록한다.

Sources:
- https://fairino-doc-en.readthedocs.io/latest/SDKManual/index.html
- https://fairino-doc-en.readthedocs.io/3.7.2/SDKManual/c%23_intro.html
- https://fairino.support/
- `docs/ref/product/robots/fairino-fr5-integration-reference.md`
- `Assets/Scripts/App/Fairino/LiveFairinoClient.cs`
- `Assets/Editor/KineTutor3D/FairinoLiveSmokeTools.cs`

## 1. Physical Safety Gate

- [ ] FR5 주변 1m 이상 작업 공간 확보.
- [ ] 사람이 로봇 작업 반경 안에 들어가지 않는 상태로 테스트 시작.
- [ ] 실제 E-stop 위치와 작동 담당자 지정.
- [ ] 컨트롤 박스 전원, 티칭펜던트, 로봇 상태등 확인.
- [ ] 툴/그리퍼 장착 상태 확인.
- [ ] 툴 TCP가 실제 장착물 기준인지 확인. 모르면 `MoveL` live 테스트 금지.
- [ ] payload / CoG 값이 기본값과 다른지 확인.
- [ ] 첫날 live motion은 금지. 허용 범위는 `Connect`, `GetVersion`, `ReadState`, `현재 위치 읽기`, 상태 파일 기록까지다. `Enable`도 이번 slice에서는 차단한다.

## 2. MacBook Ethernet Gate

권장 기본값:

- FR5 controller IP: `192.168.58.2`
- MacBook Ethernet IP 후보: `192.168.58.10` 또는 `192.168.58.100`
- Subnet: `255.255.255.0`
- Router/Gateway: 비워두거나 `192.168.58.1`

체크:

- [ ] USB-C Ethernet 어댑터 인식 확인.
- [ ] Wi-Fi, VPN, 보안 프록시가 `192.168.58.*` 라우팅을 빼앗지 않는지 확인.
- [ ] macOS 네트워크 설정에서 Ethernet 수동 IP 지정.
- [ ] `ping 192.168.58.2` 성공.
- [ ] `nc -vz 192.168.58.2 8080` 또는 동등한 포트 연결 확인.
- [ ] 필요 시 20004/8083 feedback 관련 포트는 공식 문서와 컨트롤러 설정에서 별도 확인.
- [ ] 맥북에서 Unity를 직접 실행할지, 맥북은 네트워크/AI agent만 맡고 SDK bridge는 Windows 머신에서 돌릴지 결정.

현장 명령:

```bash
ping 192.168.58.2
nc -vz 192.168.58.2 8080
open http://192.168.58.2
```

해석:

- `ping ok` + `nc ok`: FR5 컨트롤러와 8080 RPC 포트가 보인다. direct SDK readback을 시도한다.
- `ping ok` + `nc fail`: 랜선/IP는 맞지만 SDK 포트나 controller 설정이 막혔을 수 있다. FR5 설정과 bridge fallback을 확인한다.
- `ping fail`: Unity를 열기 전에 Ethernet IP, 케이블, FR5 전원, 컨트롤러 IP부터 다시 확인한다.

Mac compatibility warning:

- 현재 repo에는 `libfairino.dll`이 들어 있다.
- macOS Unity Editor/Player에서 direct C# SDK가 반드시 된다고 보면 안 된다.
- `FairinoRobotClientFactory`는 SDK assembly load, `fairino.Robot` 타입, 생성 가능 여부, version 조회 가능성을 먼저 probe한다.
- direct 실패 메시지가 `SDK 로딩 실패 / bridge 필요 / readback-only 유지`로 나오면 정상적인 차단이다. 이때 motion을 열지 않는다.

## 3. Repo / Git Gate

맥북 fresh clone:

```bash
git clone -b codex/robotcontrol-v3-toolkit https://github.com/Jason-hub-star/robotapp.git
cd robotapp
git log -1 --oneline
```

기대값:

```text
d8c0726 Add FR5 readback-only live monitor
```

이미 clone되어 있으면:

```bash
git fetch origin
git checkout codex/robotcontrol-v3-toolkit
git pull
git log -1 --oneline
```

V3를 main에 병합하기 전:

- [ ] 현재 브랜치에서 변경 범위를 분리한다. UI 토큰/레이아웃, 실기 연결, 문서 변경을 한 커밋에 섞지 않는다.
- [ ] `unityctl check --type compile` 통과.
- [ ] Pendant V3 관련 EditMode 테스트 통과.
- [ ] `RobotControlV3.unity` 직접 진입과 `Onboarding -> RobotLibrary -> RobotControlV3` 흐름 둘 다 확인.
- [ ] 현재 헤더/하단 정리처럼 runtime C#이 UXML을 덮어쓰는 케이스가 없는지 UITK 조회로 확인.
- [ ] live command safety gate가 `MoveJ/MoveL/IO/Gripper`를 현장 승인 전 차단하는지 확인.
- [ ] field-readback 전용 태그를 만든다. 예: `pre-fr5-live-readback-YYYYMMDD`.

Merge policy:

- [ ] 맥북 현장 검증은 `main`이 아니라 `codex/robotcontrol-v3-toolkit` 브랜치에서 먼저 한다.
- [ ] V3는 `main`에 병합 가능하되, 실제 FR5 readback 성공 전에는 `live-ready`라고 표기하지 않는다.
- [ ] `main` 병합 PR에는 `Mock 검증`, `SDK readback gate`, `실기 미검증 항목`을 분리해서 적는다.
- [ ] main 병합 후 첫 field session은 `readback-only`로 진행한다.

## 4. Legacy V1/V2 Removal Decision

지금 결정:

- [ ] V1/V2 즉시 삭제 금지.
- [ ] 먼저 build setting / RobotLibrary 진입점에서 V3를 기본으로 만든다.
- [ ] V1/V2는 `legacy fallback`으로 남기되, 사용자 진입점에서는 숨긴다.
- [ ] V3가 main에서 1주 이상 유지되고, 실제 FR5 readback 세션 2회 이상 성공하면 제거 PR을 따로 만든다.
- [ ] 제거 PR 전 `docs/archive/legacy/`에 스크린샷, 씬 이름, 핵심 기능, 롤백 방법을 남긴다.

삭제 가능 조건:

- [ ] V3가 `Connect -> Enable -> 현재 위치 읽기 -> 값 표시 -> 저장 위치 readback`을 안정적으로 처리.
- [ ] V1/V2에서만 가능한 기능이 feature matrix에 남아 있지 않음.
- [ ] 레거시 씬/코드 삭제 후 compile/test green.
- [ ] field rollback branch 또는 tag 존재.

## 5. SDK Cross-Verification Gate

공식 SDK/문서와 현재 코드 매핑:

| Need | Official concept | Current repo path | Field status |
|---|---|---|---|
| Connect | `RPC(ip)` | `LiveFairinoClient.Connect` | needs robot response |
| Disconnect | `CloseRPC` | `LiveFairinoClient.Disconnect` | needs robot response |
| Servo/Enable | `RobotEnable(1)` | `LiveFairinoClient.Enable` | needs field safety confirm |
| Mode | manual/auto mode API | `BestEffortPrepareForLiveMotion` path | verify on controller |
| Drag teach off | `DragTeachSwitch(0)` | `BestEffortPrepareForLiveMotion` path | verify on controller |
| Current joints | actual joint position query / realtime state | `ReadState()` | first live goal |
| Current TCP | actual TCP pose query / realtime state | `ReadState()` | first live goal |
| Tool/User | actual TCP/WObj query | coord context cache | verify before `MoveL` |
| MoveJ | point-to-point joint motion | `MoveJ` | blocked until readback green |
| MoveL | linear cartesian motion | `MoveL` | blocked until TCP verified |
| ServoJ/ServoCart | streaming motion | intentionally disabled | keep disabled |
| Error reset | `ResetAllError` | `ResetErrors` | field verify |

### Direct SDK Attempt

- [ ] `FAIRINO_BRIDGE_URL`이 비어 있는지 확인한다.
- [ ] Unity를 맥북에서 같은 환경으로 실행한다.
- [ ] `FairinoRobotClientFactory`가 `direct` client를 선택하는지 확인한다.
- [ ] SDK probe 결과가 `latest-state.json`의 `sdkLoadStatus`, `sdkRuntime`, `sdkVersion`, `clientMode`에 남는지 확인한다.
- [ ] `ping`과 `nc -vz 192.168.58.2 8080` 성공 뒤에만 `RPC(ip)`를 시도한다.

### Bridge Fallback Attempt

direct SDK가 macOS에서 실패하면 bridge fallback으로 간다.

```bash
export FAIRINO_BRIDGE_URL=http://127.0.0.1:5055
```

중요:

- Unity는 이 환경변수가 보이는 shell에서 실행해야 한다.
- bridge는 readback-only다. `Enable`, `MoveJ`, `MoveL`, `ServoJ`, `ServoCart`는 명시적으로 실패해야 정상이다.
- bridge contract:
  - `POST /connect { "ip": "192.168.58.2", "port": 8080 }`
  - `POST /disconnect`
  - `GET /version`
  - `GET /state`

Smoke/readback order:

- [ ] Set `FAIRINO_IP=192.168.58.2`.
- [ ] Set `FAIRINO_PORT=8080`.
- [ ] Run `KineTutor3D/RobotControl/Run FAIRINO Live Smoke Test`.
- [ ] Expected success message starts with `[FAIRINO LIVE SMOKE] CONNECT_OK`.
- [ ] It must include firmware/software/SDK version if available.
- [ ] It must include `joints=[...] tcp=[...]`.
- [ ] On failure, expected message starts with `[FAIRINO LIVE SMOKE] CONNECT_FAIL`.

UI success messages to expose:

- `FR5 연결됨`
- `SDK 확인 완료`
- `현재 위치 읽기 완료`
- `관절/TCP 값 수신 중`
- `시뮬레이션과 실제 위치 차이 없음`

UI warning/error messages to expose:

- `FR5 연결 실패: IP/랜선/전원 확인`
- `SDK 버전 확인 실패`
- `SDK 로딩 실패: bridge 필요`
- `현재 위치 읽기 실패`
- `실제 위치와 화면 위치가 다름`
- `실기 이동 차단됨: 현장 확인 필요`

## 6. Live State Tracking Design

큰 JSON 파일 하나에 계속 덮어쓰는 방식은 비추천이다. 읽기는 쉽지만 파일이 커질수록 느려지고, 쓰는 중 깨질 위험이 있다.

권장 구조:

```text
Artifacts/live/fr5/latest-state.json
Artifacts/live/fr5/latest-drift.json
Artifacts/live/fr5/sessions/20260428-153000-readback.ndjson
Artifacts/live/fr5/sessions/20260428-153000-events.ndjson
```

Roles:

- `latest-state.json`: AI agent와 UI가 읽는 최신 상태. 매 샘플마다 원자적 교체.
- `latest-drift.json`: 실제 로봇값과 Unity 시뮬레이션값 차이만 요약.
- `sessions/*.ndjson`: 한 줄에 한 샘플/이벤트. 장기 로그와 재현용.
- 필요해지면 SQLite로 승격. 첫 field day에는 NDJSON이 단순하고 안전하다.

Sample `latest-state.json` shape:

```json
{
  "sessionId": "20260428-153000-readback",
  "source": "fairino-live",
  "robotId": "FR5",
  "ip": "192.168.58.2",
  "timestampUtc": "2026-04-28T06:30:00.000Z",
  "connected": true,
  "enabled": false,
  "mode": "manual",
  "toolId": 1,
  "userId": 0,
  "jointsDeg": [0, -30, 90, 0, 45, 0],
  "tcpMmDeg": [-497, -130, 477, 180, 0, 90],
  "safety": "normal",
  "fault": {
    "hasFault": false,
    "mainCode": 0,
    "subCode": 0
  },
  "sdk": {
    "sdkLoadStatus": "loaded",
    "sdkVersion": "",
    "sdkRuntime": "Unity macOS Editor",
    "clientMode": "direct",
    "softwareVersion": "",
    "firmwareVersion": ""
  }
}
```

Sample `latest-drift.json` shape:

```json
{
  "sessionId": "20260428-153000-readback",
  "timestampUtc": "2026-04-28T06:30:01.000Z",
  "severity": "ok",
  "maxJointDeg": 0.1,
  "maxTcpMm": 0.5,
  "maxTcpRotDeg": 0.2,
  "liveBlockedReason": ""
}
```

Drift thresholds for first field session:

- Joint warning: `> 0.5 deg`
- Joint danger: `> 2.0 deg`
- TCP position warning: `> 2 mm`
- TCP position danger: `> 10 mm`
- TCP rotation warning: `> 1 deg`
- TCP rotation danger: `> 5 deg`

Agent behavior:

- [ ] Agent may read `latest-state.json`.
- [ ] Agent may compare with Unity expected pose.
- [ ] Agent may write a suggested diagnosis to `latest-drift.json`.
- [ ] Agent must not directly send live robot movement.
- [ ] If drift exceeds warning threshold, block live move and ask for `현재 위치 읽기`.
- [ ] If drift exceeds danger threshold, keep live motion disabled until operator confirms physical state.

## 7. First Field Day Runbook

Readback-only:

- [ ] Boot Mac/Windows host.
- [ ] Connect Ethernet to FR5 controller.
- [ ] Confirm static IP.
- [ ] Ping controller.
- [ ] Confirm `nc -vz 192.168.58.2 8080`.
- [ ] Clone/pull `codex/robotcontrol-v3-toolkit`.
- [ ] Confirm latest commit includes `d8c0726 Add FR5 readback-only live monitor` or newer.
- [ ] Start Unity.
- [ ] Open V3 scene through normal flow.
- [ ] Confirm header shows only necessary live actions. `서보ON` must not behave as a casual movement button.
- [ ] Run live smoke test.
- [ ] Press/connect through UI if smoke succeeds.
- [ ] Press `현재 위치 읽기`.
- [ ] Confirm UI displays `현재 위치 읽기 완료`.
- [ ] Confirm actual joints/TCP appear in status panel.
- [ ] Confirm `latest-state.json` updates.
- [ ] Confirm `latest-drift.json` updates.
- [ ] Confirm `sessions/*-readback.ndjson` and `sessions/*-events.ndjson` append.
- [ ] Confirm `clientMode` is `direct` or `bridge`.
- [ ] Confirm AI agent can read `latest-state.json`.
- [ ] Save session log.

Only after readback green:

- [ ] Do not enable servo in this implementation slice. Enable remains blocked by readback-only policy.
- [ ] Do not run MoveJ/MoveL yet.
- [ ] Validate tool/user/TCP context.
- [ ] Validate visual robot mirror against actual resting pose.
- [ ] Decide whether a tiny MoveJ can be scheduled for a later controlled test in a separate approved phase.

## 8. Go / No-Go

Go for readback-only when:

- [ ] Ethernet ping succeeds.
- [ ] SDK smoke `CONNECT_OK`.
- [ ] `ReadState()` returns joints/TCP.
- [ ] UI prints success message.
- [ ] `latest-state.json` updates atomically.
- [ ] No safety/fault code present.

No-go for live movement when:

- [ ] SDK version unknown.
- [ ] Tool/User context unknown.
- [ ] TCP calibration unknown.
- [ ] Unity visual pose and actual pose differ above warning threshold.
- [ ] Error/fault/safety stop present.
- [ ] Operator cannot reach E-stop.
- [ ] Running on macOS without confirmed FAIRINO SDK plugin compatibility.

## Current Implementation Slice Delivered

- [x] Add `Fr5LiveStateRecorder`.
- [x] Write `latest-state.json`, `latest-drift.json`, and `sessions/*.ndjson`.
- [x] Add drift comparison between V3 snapshot and live readback.
- [x] Add direct SDK probe and bridge fallback client factory.
- [x] Keep `Enable`, `MoveJ`, `MoveL`, `IO`, `Gripper`, `ServoJ`, `ServoCart` blocked in readback-only path.

## Next Field Evidence To Capture

- [ ] Screenshot or copy of `ping 192.168.58.2`.
- [ ] Screenshot or copy of `nc -vz 192.168.58.2 8080`.
- [ ] `latest-state.json` with `connected=true` and live joint/TCP values.
- [ ] `latest-drift.json` with `severity=ok` or documented warning reason.
- [ ] `sessions/*-events.ndjson` lines showing connect, SDK check, readback, disconnect.
