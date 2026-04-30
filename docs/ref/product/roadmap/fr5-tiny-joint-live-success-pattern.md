---
title: "FR5 Tiny Joint Live Success Pattern"
doc_type: "reference"
status: "active"
domain: "fr5-live"
audience: "human-and-agent"
last_updated: "2026-04-29"
---

# FR5 Tiny Joint Live Success Pattern

## Purpose

이 문서는 `/Users/family/jason/FR5UNITY/robotapp`에서 현재 실기 검증이 끝난 가장 좁은 arm motion success pattern을 잠그는 SSOT다.

현재 범위는 broad arm motion이 아니라 아래 narrow path만 포함한다.

- `Pendant auto mode`가 이미 맞춰진 상태
- `speed=10`
- `jog increment=1`
- `tiny MoveJ`
- verified joints: `J1` through `J6`
- verified delta: `+1deg`, `-1deg`

## Do Not Generalize

아직 이 문서에서 일반화하면 안 되는 것:

- helper에 `3deg`를 넣었다고 실제 `3deg` tiny path가 검증됐다고 보지 않는다.
- `TCP jog`, `MoveL`, large-range joint move가 green이라고 보지 않는다.
- immediate QA artifact `afterMovement`만으로 real joint delta를 확정하지 않는다.
- pendant 없이 app alone이 수동/자동 전환까지 완전 대체한다고 보지 않는다.

## Preconditions

- FR5 controller port baseline: `eth0 = 192.168.57.2`
- MacBook NIC baseline: `192.168.57.10/24`
- `ping 192.168.57.2` success
- `nc -vz 192.168.57.2 8080` success
- Unity `RobotControlV3` is playing
- `unityctl` bridge is healthy
- live evidence freshness is available
- `toolId=1`, `userId=1`, `coordSystem=Base`
- operator or teach pendant has already placed the robot in `auto mode`

## Required Code Path

현재 success pattern은 아래 narrow path를 전제로 한다.

- motion-capable sibling session must recheck `mode=0` before dispatch
- tiny `MoveJ` range/speed guard must still stay enabled
- operator confirm path stays enabled
- after execute, runtime relocks back to readback-safe mode

## Minimum Workflow

1. `SetMockModeForDebug(false)`
2. `ConnectDefaultForDebug()`
3. `SyncCurrentStateForDebug()`
4. `RefreshLiveEvidenceForDebug()`
5. `SetShellSpeedPercentForDebug(10)`
6. `SetShellJogIncrementForDebug(1)`
7. `RunLiveJointNudgeQaForDebug(jointIndex, deltaDeg, true)`
8. after the run, call `SyncCurrentStateForDebug()` again
9. call `RefreshLiveEvidenceForDebug()` again
10. inspect `Artifacts/live/fr5/latest-state.json`

## Truth Rule

real joint movement truth는 아래 순서로 판단한다.

1. QA artifact execute summary says `OK`
2. post-run `SyncCurrentStateForDebug()` succeeds
3. post-run `RefreshLiveEvidenceForDebug()` succeeds
4. `Artifacts/live/fr5/latest-state.json` reflects the new joint value

즉, immediate artifact `afterMovement`가 stale할 수 있다는 점을 항상 기억한다.

## Verified Evidence On 2026-04-29

### J6 +1deg

- baseline readback: `J6 = 1.1638981103897095`
- post-run readback: `J6 = 2.1635451316833496`
- observed delta: `+0.9996deg`
- artifact: `Artifacts/live/qa/20260429-174934-joint-j6-plus.json`

### J6 -1deg

- baseline readback: `J6 = 2.1637628078460693`
- post-run readback: `J6 = 1.1636805534362793`
- observed delta: `-1.0001deg`
- artifact: `Artifacts/live/qa/20260429-175135-joint-j6-minus.json`

### J5 +1deg

- baseline readback: `J5 = -90.0217514038086`
- post-run readback: `J5 = -89.02188873291016`
- observed delta: `+0.9999deg`
- artifact: `Artifacts/live/qa/20260429-175158-joint-j5-plus.json`

### J5 -1deg

- baseline readback: `J5 = -89.02167510986328`
- post-run readback: `J5 = -90.02153778076172`
- observed delta: `-0.9999deg`
- artifact: `Artifacts/live/qa/20260429-180034-joint-j5-minus.json`

### J1~J4 all-joint verify

- `J1 + / -`
  - `-80.0535659790039 -> -79.05239868164062`
  - `-79.05239868164062 -> -80.05400085449219`
  - artifacts:
    - `Artifacts/live/qa/20260429-180640-joint-j1-plus.json`
    - `Artifacts/live/qa/20260429-180644-joint-j1-minus.json`
- `J2 + / -`
  - `-89.86425018310547 -> -88.86329650878906`
  - `-88.86329650878906 -> -89.86425018310547`
  - artifacts:
    - `Artifacts/live/qa/20260429-180649-joint-j2-plus.json`
    - `Artifacts/live/qa/20260429-180653-joint-j2-minus.json`
- `J3 + / -`
  - `90.04438018798828 -> 91.04598236083984`
  - `91.04598236083984 -> 90.04307556152344`
  - artifacts:
    - `Artifacts/live/qa/20260429-180658-joint-j3-plus.json`
    - `Artifacts/live/qa/20260429-180703-joint-j3-minus.json`
- `J4 + / -`
  - `-89.9071044921875 -> -88.90680694580078`
  - `-88.90680694580078 -> -89.90666961669922`
  - artifacts:
    - `Artifacts/live/qa/20260429-180706-joint-j4-plus.json`
    - `Artifacts/live/qa/20260429-180711-joint-j4-minus.json`

## Repeatability Verify On 2026-04-29

### J5 repeat cycle

- `J5 +1deg`
  - before `-90.02153778076172`
  - after `-89.01688385009766`
  - delta `+1.0047deg`
  - artifact: `Artifacts/live/qa/20260429-180121-joint-j5-plus.json`
- `J5 -1deg`
  - before `-89.021240234375`
  - after `-90.0221939086914`
  - delta `-1.0010deg`
  - artifact: `Artifacts/live/qa/20260429-180154-joint-j5-minus.json`
- `J5 +1deg` repeat
  - before `-90.0221939086914`
  - after `-89.01644897460938`
  - delta `+1.0057deg`
  - artifact: `Artifacts/live/qa/20260429-180157-joint-j5-plus.json`

### J6 repeat cycle

- `J6 +1deg`
  - before `1.163245439529419`
  - after `2.1676785945892334`
  - delta `+1.0044deg`
  - artifact: `Artifacts/live/qa/20260429-180125-joint-j6-plus.json`
- `J6 -1deg`
  - before `2.1676785945892334`
  - after `1.1630278825759888`
  - delta `-1.0047deg`
  - artifact: `Artifacts/live/qa/20260429-180128-joint-j6-minus.json`

## Direct-Input Delta Interpretation

- old shell-increment helper path did not validate a true `3deg` move
- that older path still means:
  - `all joints J1~J6 move on the current effective tiny helper path`
  - `current effective move size is about 1deg`
- current branch now has a separate direct-input path that accepts `requestedDeltaDeg` explicitly
- 2026-04-30 verified result on the new path:
  - `J6 true +3deg` is green
  - artifact: `Artifacts/live/qa/20260430-140235-joint-j6-plus-3.json`
  - `requestedDeltaDeg=3.0`
  - `actualDeltaDeg=3.0004deg`
- 2026-04-30 follow-up result after tolerance tuning:
  - `J6 true +5deg` is green
  - artifact: `Artifacts/live/qa/20260430-141424-joint-j6-plus-5.json`
  - `requestedDeltaDeg=5.0`
  - `actualDeltaDeg=5.0002deg`
  - `J5 true +5deg` is green
  - artifact: `Artifacts/live/qa/20260430-141428-joint-j5-plus-5.json`
  - `actualDeltaDeg=4.9999deg`
  - `J5 true -5deg` is green
  - artifact: `Artifacts/live/qa/20260430-141441-joint-j5-minus-5.json`
  - `actualDeltaDeg=-5.0001deg`
- current open edge:
  - `+6deg` still blocks correctly as `tiny MoveJ range exceeded`
  - blocked artifact: `Artifacts/live/qa/20260430-141616-joint-j6-plus-6.json`
  - header/SafetyDiagnostics operator copy is still too generic after this blocked state and needs a follow-up copy-layer fix

## Current Known-Good Scope

- `J1 +1deg / -1deg effective tiny path`
- `J2 +1deg / -1deg effective tiny path`
- `J3 +1deg / -1deg effective tiny path`
- `J4 +1deg / -1deg effective tiny path`
- `J5 +1deg`
- `J5 -1deg`
- `J6 +1deg`
- `J6 -1deg`
- `J1~J6 all-joint effective tiny motion`
- `J5/J6 both directions + same-day repeatability`
- `same-day repeatability on the narrow tiny-joint path`
- `J6 true +3deg` on the direct-input popup-confirm path
- `J6 true +5deg` on the direct-input popup-confirm path
- `J5 true +5deg / -5deg` on the direct-input popup-confirm path

이 범위에서는 `actual hardware move + readback delta`까지 확인됐다.

## Stop Conditions

아래가 나오면 바로 broad expansion으로 가지 않는다.

- `8080` port unstable
- motion sibling still reads `mode=1`
- `toolId` or `userId` missing
- evidence freshness stale
- unexpected controller fault
- range/speed cap failure
- artifact says `executed=False`

## Next Expansion

다음 순서는 이 문서 범위를 벗어나지 않는 선에서만 넓힌다.

1. `J6/J5`에서 닫힌 `5deg` acceptance를 다른 일부 축으로 넓힐지 결정
2. `J1~J6` repeatability를 2~3 full cycle까지 넓힐지 결정
3. `tiny range exceeded` 같은 real blocked state에서도 header `다음 행동`과 Safety/Diagnostics taxonomy가 generic readback-only 문구로 되돌아가지 않게 copy-layer를 보강
4. only then consider broader joint spread or `TCP`
