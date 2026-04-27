# Pendant V3 I/O Point Integration + Gripper Visual Fix

Date: 2026-04-27 (KST)

## Decision

- 왼쪽 `I/O` 전용 탭은 제거한다.
- I/O와 그리퍼는 `Point` 탭의 보조 조작으로 통합한다.
- 이유: 포인트 저장/묶음/함수 등록 흐름에서 gripper와 DO 조작은 따로 떨어진 상위 모드가 아니라 teaching context에 붙는 실행 보조 기능이다.

## Changed

- `NavIo`, `BottomTabIo`를 UXML과 shell controller cache에서 제거했다.
- `IoPanelController` 표시 조건을 `NavPoints` / `BottomTabPointMove`로 바꿨다.
- 이전 local state의 `NavIo`, `BottomTabIo`는 각각 `NavPoints`, `BottomTabPointMove`로 normalize한다.
- gripper open 명령에서 visual open ratio가 `0`으로 남던 bug를 수정했다.
- FAIRINO 공식 C# SDK의 gripper 흐름을 다시 확인했다: `SetGripperConfig(...)` -> `ActGripper(...)` -> `MoveGripper(index, pos, vel, force, max_time, block)`.
- finger visual은 더 이상 고정 X축 `+/-20` offset을 쓰지 않고, `TcpMarker` 구체를 기준으로 각 finger renderer center가 멀어지는 방향을 open direction으로 잡는다.
- stale recalled point가 현재 저장소에 없을 때 function 생성 source로 쓰이지 않게 막았다.
- 대표 버튼 debug click fallback을 추가해 matrix가 Unity internal click dispatch 차이에 덜 흔들리게 했다.

## Verification

- `unityctl check --type compile`: pass
- `RunTabletBottomActualClickMatrixForDebug()`: `15/15 PASS`
- `RunFunctionActualClickMatrixForDebug()`: `7/7 PASS`
- `RunTeachingBlockSequenceMatrixForDebug()`: `9/9 PASS`
- gripper close:
  - `fingerLeft=(0,0,0)`
  - `fingerRight=(0,0,0)`
  - `leftDistance=0.0149`, `rightDistance=0.0127`
  - `openRatio=0.00`
- gripper open:
  - `fingerLeft=(12.6937,-0.5475,15.4457)`
  - `fingerRight=(14.865,0.5113,-13.3705)`
  - `leftDistance=0.0348`, `rightDistance=0.0327`
  - `openRatio=1.00`

## Notes

- close에서 open으로 갈 때 두 finger 모두 `TcpMarker` 구체와의 거리가 증가한다. 따라서 close 동작은 같은 벡터를 역으로 타고 구체 방향으로 닫힌다.
- 실제 stroke는 donor/template의 40mm stroke를 유지하되, 축은 prefab의 현재 배치와 `TcpMarker` 위치에서 계산한다.
- full `RunActualUiClickMatrixForDebug()`는 케이스 수가 커져 IPC 30초 제한에 걸릴 수 있으므로, 이번 검증은 tablet/function/block split matrix로 닫았다.
- 후속 검증 중 Unity IPC가 재기동 타이밍에 걸려 추가 matrix 재실행은 대기 상태였다.
