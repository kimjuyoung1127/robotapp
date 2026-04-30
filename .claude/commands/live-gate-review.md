# /live-gate-review — FR5 live gate 자기점검

FR5 `readback-only` live 기준선이 아직 안전한지 빠르게 확인할 때 사용한다.

## 목표

- 현재 세션이 `실기 readback`을 truth로 유지하는지 확인
- stale evidence, zero-state overwrite, gate 문구 drift를 빨리 찾기
- motion을 열지 않고도 `go/no-go`를 판정

## 실행 순서

### 1. 현장 네트워크 확인

```bash
ping -c 2 192.168.58.2
python3 - <<'PY'
import socket
s=socket.socket()
s.settimeout(3)
s.connect(('192.168.58.2',8080))
s.close()
print('8080 open')
PY
```

### 2. Unity 상태 확인

```bash
unityctl status --project /Users/family/jason/FR5UNITY/robotapp --json
unityctl check --project /Users/family/jason/FR5UNITY/robotapp --type compile --json
```

### 3. FR5 live gate 확인

```bash
scripts/tests/run_fr5_live_checks.sh --live --no-edit-tests
```

### 4. 런타임 truth 비교

```bash
unityctl exec --project /Users/family/jason/FR5UNITY/robotapp --code 'KineTutor3D.App.RobotControlV3DebugBridge.GetLiveStateComparisonForDebug()' --json
unityctl exec --project /Users/family/jason/FR5UNITY/robotapp --code 'KineTutor3D.App.RobotControlV3DebugBridge.GetTinyMoveJGateSummaryForDebug()' --json
```

## 핵심 판정 기준

- `clientRead`, `serviceLast`, `runtimeCurrent`가 같은 방향이면 숫자 정합성은 green
- `latest-state.json`이 현재 세션에서 갱신되면 freshness green
- `toolId > 0`, `userId > 0`, `coordSystem in {Base, Tool, User}`면 context green
- tiny MoveJ gate는 `status=ReadbackOnly`여야 정상
- `dry-run simulation` 문구가 live readback-only에서 보이면 회귀

## Poll baseline

- 기본 poll: `33ms`
- 안정성 폴백: `50ms`
- 참고 실측:
  - `~8.93Hz @100ms`
  - `~18.66Hz @50ms`
  - `~27.37Hz @33ms`

## 보고 형식

```text
## Live Gate Review

- Network: PASS/FAIL
- Compile: PASS/FAIL
- Live checks: PASS/FAIL
- Evidence freshness: PASS/FAIL
- Motion gate wording: PASS/FAIL
- Poll baseline: 33ms / fallback 50ms
```
