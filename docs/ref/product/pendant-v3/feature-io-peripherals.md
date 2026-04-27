# IO/그리퍼/주변장치 제어

## Purpose
- 디지털/아날로그 IO, 그리퍼, 외부 장치의 상태 표시와 제어 UI를 정의한다.

## Parent Doc
- [README.md](./README.md)

## Last Updated
- 2026-04-03 (KST)

## SSOT 상태
- **SSOT 확정**: V3에서 P1 (Phase 6)으로 포함. IO 제어 + 그리퍼 개폐 구현
- V1 Backlog "선택" → V3에서 필수로 승격

---

## IO 패널 (NavRail > I/O)

```text
┌─ IO 제어 ────────────────────────────────────┐
│                                               │
│  탭: [디지털 출력] [디지털 입력] [아날로그]   │
│       [그리퍼]                                │
│                                               │
│  ┌─ 디지털 출력 (DO) ────────────────────┐   │
│  │                                        │   │
│  │  DO 0  [━ON━]  ○OFF   "그리퍼"        │   │
│  │  DO 1   ON    [━OFF━]  "클램프"       │   │
│  │  DO 2   ON    [━OFF━]  "진공"         │   │
│  │  DO 3   ON    [━OFF━]  --             │   │
│  │  DO 4   ON    [━OFF━]  --             │   │
│  │  DO 5   ON    [━OFF━]  --             │   │
│  │  DO 6   ON    [━OFF━]  --             │   │
│  │  DO 7   ON    [━OFF━]  --             │   │
│  │                                        │   │
│  │  Tool DO                               │   │
│  │  TDO 0  [━ON━]  ○OFF  "센서"         │   │
│  │  TDO 1   ON    [━OFF━]  --            │   │
│  │                                        │   │
│  └────────────────────────────────────────┘   │
│                                               │
│  ┌─ 디지털 입력 (DI) ────────────────────┐   │
│  │                                        │   │
│  │  DI 0  ● ON   "부품 감지"             │   │
│  │  DI 1  ○ OFF  "안전 센서"             │   │
│  │  DI 2  ○ OFF  --                      │   │
│  │  ...                                   │   │
│  │                                        │   │
│  │  (입력은 읽기 전용)                    │   │
│  └────────────────────────────────────────┘   │
│                                               │
└───────────────────────────────────────────────┘
```

### 그리퍼 전용 탭
```text
┌─ 그리퍼 ─────────────────────────────────────┐
│                                               │
│  그리퍼 타입: [전기식 ▼]                     │
│                                               │
│  상태: ● 닫힘                                 │
│  위치: 45mm / 100mm                           │
│  힘: 20N                                      │
│                                               │
│  ┌─ 빠른 제어 ───────────────────────────┐   │
│  │                                        │   │
│  │  [━━ 열기 ━━]     [━━ 닫기 ━━]        │   │
│  │                                        │   │
│  │  위치: [━━━━━●━━━] 45mm               │   │
│  │  힘:   [━━●━━━━━━] 20N               │   │
│  │                                        │   │
│  └────────────────────────────────────────┘   │
│                                               │
│  대표 API:                                    │
│  SetGripperConfig, ActGripper, MoveGripper   │
│                                               │
└───────────────────────────────────────────────┘
```

---

## IO 이름 매핑

| DO/DI | 기본 이름 | 사용자 지정 가능 |
|-------|----------|----------------|
| DO 0 | -- | O (예: "그리퍼") |
| DO 1 | -- | O (예: "클램프") |
| DI 0 | -- | O (예: "부품 감지") |

- 이름은 로컬 JSON 파일에 저장
- 이름이 없으면 "--" 표시

---

## 대표 API (FAIRINO SDK)

| 기능 | API |
|------|-----|
| DO 출력 | `SetDO(id, value)` |
| Tool DO | `SetToolDO(id, value)` |
| AO 출력 | `SetAO(id, value)` |
| DI 읽기 | `GetDI(id)` |
| 그리퍼 설정 | `SetGripperConfig(...)` |
| 그리퍼 동작 | `ActGripper(id, action)` |
| 그리퍼 이동 | `MoveGripper(index, pos, vel, force, max_time, block, type, rotNum, rotVel, rotTorque)` |
| 그리퍼 완료 확인 | `GetGripperMotionDone(id)` |

## 2026-04-27 Position Control Update

- `그리퍼 / I/O` 패널은 `Point`가 아니라 `조작 > 기본` 흐름에 둔다.
- 그리퍼 기본값은 `pos=100` 완전 열림이다.
- 완전 닫힘은 `pos=0`이며, object가 없을 때 finger 안쪽이 서로 닿는 상태다.
- 가운데 object가 감지되면 close 명령은 mock/visual에서 object stop percent에 멈추고 `holdingObject` 상태를 표시한다.
- 실기 object 감지는 공식 SDK readback의 position/current/motion status를 비교한 뒤 force/current threshold 정책으로 확정한다.
