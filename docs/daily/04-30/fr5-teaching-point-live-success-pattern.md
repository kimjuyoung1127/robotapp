# FR5 Teaching Point Live Success Pattern

## Summary

- saved `MoveJ` point single live apply와 `PendantV3Points` two-point live run-once를 현재 branch 기준선으로 잠갔다.
- old `named sequence live 실행은 v1에서 잠금` 경로는 current `run once`에 한해 해제했다.
- broad teaching runtime green으로 일반화하지 않고, `single point / two-point once / loop locked`로 범위를 고정했다.

## Verified Today

- live baseline:
  - `latest-state.json`: `connected=true`, `mode=0`, `clientMode=direct`
  - current readback before sequence: `J6 ≈ 3.2`
- saved points:
  - `LIVE_P1`: `MoveJ`, `J6 ≈ 2.2`
  - `LIVE_P2`: `MoveJ`, `J6 ≈ 3.2`
- sequence:
  - `SelectTeachingSequenceForDebug("PendantV3Points")`
  - `RunSelectedTeachingSequenceOnceForDebug()`
  - popup confirm
  - feedback: `[Sequence Run] PendantV3Points live 1회 실행 완료 · 2개 포인트`
- post-run truth:
  - refreshed `/Users/family/jason/FR5UNITY/robotapp/Artifacts/live/fr5/latest-state.json`
  - `mode=0`
  - `J6 ≈ 3.1999`

## Implementation Notes

- `PointMoveController`는 current live path에서 sequence once를 popup confirm 경로로 라우팅한다.
- runtime은 `PendantV3Points`를 load한 뒤 각 saved point를 `ExecuteTeachingWaypoint()`로 순차 dispatch한다.
- live loop는 아직 잠금이다.

## Follow-Up

- next:
  - live loop unlock 여부는 별도 track으로 둔다
  - arbitrary named sequence/general program runtime green으로 일반화하지 않는다
  - if needed, point/sequence live popup token surface를 더 정리한다
