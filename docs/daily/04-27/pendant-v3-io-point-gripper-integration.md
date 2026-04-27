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
  - `openRatio=0.00`
- gripper open:
  - `fingerLeft=(20,0,0)`
  - `fingerRight=(-20,0,0)`
  - `openRatio=1.00`

## Notes

- 실제 finger offset `20`은 donor/template의 40mm stroke를 좌우 절반씩 쓰는 현재 visual 계약과 일치한다.
- full `RunActualUiClickMatrixForDebug()`는 케이스 수가 커져 IPC 30초 제한에 걸릴 수 있으므로, 이번 검증은 tablet/function/block split matrix로 닫았다.
