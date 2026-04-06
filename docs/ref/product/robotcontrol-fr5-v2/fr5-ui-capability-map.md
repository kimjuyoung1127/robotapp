# FR5 UI Capability Map

## Purpose

현재 V2 UI에 무엇을 반드시 넣고, 무엇을 참고만 하고, 무엇을 아직 빼둘지 분리한다.

- FAIRINO SimMachine 스크린샷은 `visual reference`
- 공식 FAIRINO 문서/SDK는 `functional reference`
- UR / Doosan 공식 teaching pad는 `interaction reference`

## Core Principle

1. `기능 계약`이 먼저다.
2. UI는 기능 계약을 소비하는 surface로 설계한다.
3. 기존 V1 패널을 통째로 복붙하지 않고 `분해 후 재배치`한다.
4. 현재 씬 authoring 편의를 위한 alpha 조절은 허용하지만, 이름 계약과 기능 계약은 흔들지 않는다.

## V2 Panels

| panel | goal | must-have | optional | excluded-for-now |
|---|---|---|---|---|
| `TopStatusBar` | 연결/모드/즉시 액션 허브 | connect, enable, sync, stop, mode, fault, safety | run/pause placeholder | 고급 system 설정 |
| `EasyMotionPanel` | 큰 버튼 프리셋 이동 | Home, Ready, Folded, Zero, preview, apply | 최근 프리셋 | 고급 program 편집 |
| `JointJogPanel` | slider 대신 화살표 기반 미세 조정 | `J1~J6 -/+`, 현재 각도, 적용, 복원 | hold jog 속도 선택 | ServoJ live |
| `TcpJogPanel` | TCP 증분 이동 | Base/Tool/Wobj, increment, `X/Y/Z/Rx/Ry/Rz -/+`, preview, move | current fill | ServoCart live |
| `PointMovePanel` | 목표 pose 계산/이동 | 현재 pose, 목표 pose 입력, calculate, move, restore | inverse solve 상세 | 전체 program tree |
| `TeachingPanel` | 포인트 저장/호출/반복 | remember current pose, save point, load point, run point, loop, stop loop | named points, quick save | full TPD editor |
| `StatusSummaryPanel` | 상태 요약 | connection, mode, fault, safety, tool/user, speed | recent event | raw controller logs |
| `WhyItMovedPanel` | 움직임 해설 | delta summary, target summary | axis detail | 별도 분석기 |
| `RecoveryGuidePanel` | 실패/중단 복구 | reconnect, re-enable, re-sync, retry hint | scenario-specific flow | service diagnostics |
| `CenterViewport` | preview와 공간 이해 | robot, ghost, path estimate, floor grid | tool/base axes toggle | dense engineering overlays |

## UI Reference Policy

### FAIRINO SimMachine

- 가져올 것
  - 작업 단위 중심의 수동 조작 흐름
  - point / jog / move 중심 정보구조
  - operator가 즉시 이해할 수 있는 control density
- 그대로 복제하지 않을 것
  - 전체 프로그램 IDE
  - 시스템/설치/애플리케이션 깊은 메뉴
  - 현재 제품 범위를 넘는 산업 패키지

### Competitive Official References

- UR에서 참고할 것
  - waypoint affordance
  - move / jog / preview 흐름 분리
- Doosan에서 참고할 것
  - jog step / axis switching
  - operator-friendly arrow jog affordance

## Authored Scene Editing Rule

1. alpha 조절로 패널 전환하며 위치 잡는 것은 허용한다.
2. 아래 항목은 편집 중에도 바꾸지 않는다.
   - panel root 이름
   - required child 이름
   - panel root component
   - `CommandId` 의미
3. 아래 항목은 계속 authoring 가능하다.
   - `RectTransform`
   - `CanvasGroup alpha`
   - spacing / padding / font size
   - 일시적 active 상태

## Not SSOT

아래는 SSOT가 아니다.

- 기존 V1 패널의 버튼 배치
- SimMachine 캡처 한 장의 레이아웃
- 현재 임시 alpha 상태
- placeholder 문구만 있는 fallback surface

## Lock Before Coding

1. 각 패널이 어떤 `command_id`를 소비하는지 표기
2. `must-have`와 `excluded-for-now`를 확정
3. `joint arrow jog`, `ghost`, `floor grid`, `remember current pose`를 V2 필수로 명시
