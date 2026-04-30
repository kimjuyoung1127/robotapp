# FR5 Teaching Point Live Success Pattern

Last Updated: 2026-04-30 (KST)

## Purpose

이 문서는 `/Users/family/jason/FR5UNITY/robotapp`의 현재 `NavPoints -> PointMove -> FR5 live` 성공패턴을 잠그는 SSOT다.

범위는 좁게 고정한다.

- saved `MoveJ` point single live apply
- `PendantV3Points` two-point live run once

이 문서는 broad teaching runtime green 선언이 아니다.

아래는 아직 범위 밖이다.

- live loop
- arbitrary named sequence live
- IO/gripper sequence blocks
- `MoveL` numerical IK live
- broad program runtime

## Current Green Scope

- `LIVE_P1` 같은 saved `MoveJ` point를 recall 후 live apply 가능
- `LIVE_P2` 같은 saved `MoveJ` point를 recall 후 live apply 가능
- default saved-point sequence `PendantV3Points`를 live로 `1회 실행` 가능
- current verified two-point sequence is `LIVE_P1 -> LIVE_P2`
- point/sequence live는 이제 `save / apply / run` 앞마다 `Sync + RefreshLiveEvidence`를 강제하는 operator gate를 탄다
- `QA0430_HOME_2PT_T6` one-shot live 실행 가능
- `QA0430_HOME_MULTIAXIS_T5_T6` one-shot live 실행 가능

## Required Preconditions

1. `192.168.57.2:8080` reachable
2. `latest-state.json` fresh
3. `mode=0`
4. `clientMode=direct` or equivalent live truth
5. `toolId > 0`, `userId > 0`, `coordSystem` truth present
6. `dryRun=False`
7. saved point move type is `MoveJ`
8. operator가 manual mode에서 실제 로봇 자세를 바꿨다면, 그 자세를 먼저 `Home` point로 저장하고 그 다음 repeatability를 시작한다

## Home Capture Before Repeatability

수동으로 실제 로봇 자세를 바꾼 뒤 teaching repeatability를 시험할 때는 아래 순서를 먼저 잠근다.

1. `현재 위치 읽기`
2. `Sync + RefreshLiveEvidence`
3. Unity에 보이는 joints/TCP와 refreshed `latest-state.json`이 같은 단계값인지 확인
4. 그 자세를 `Home` point로 저장
5. 그 다음에만 `2-point` 또는 `multi-axis` teaching sequence 검증으로 들어간다

## Canonical Operator Flow

### Single Point

1. `연결`
2. `현재 위치 읽기`
3. Unity joints/TCP가 refreshed `latest-state.json`과 맞는지 확인
4. `NavPoints -> 포인트`
5. saved point recall
6. `적용 전 Sync + RefreshLiveEvidence`
7. `적용`
8. `이동 실행 확인`
9. post-run `Sync + RefreshLiveEvidence`

### Two-Point Sequence Once

1. `연결`
2. `현재 위치 읽기`
3. Unity joints/TCP가 refreshed `latest-state.json`과 맞는지 확인
4. `NavPoints -> 시퀀스`
5. `PendantV3Points` 선택
6. `실행 전 Sync + RefreshLiveEvidence`
7. `실행`
8. `실행 확인`
9. post-run `Sync + RefreshLiveEvidence`

## Canonical Debug Helper Order

- `SetMockModeForDebug(false)`
- `ConnectDefaultForDebug()`
- `SyncCurrentStateForDebug()`
- `RefreshLiveEvidenceForDebug()`
- `SetShellSelection("NavPoints", "TabTcpJog", "BottomTabPointMove")`
- optional when operator changed pose manually:
  - `SetPointMoveNameForDebug("<HOME_NAME>")`
  - `SyncCurrentStateForDebug()`
  - `RefreshLiveEvidenceForDebug()`
  - `SavePointMoveForDebug()`
- single point:
  - `RecallPointMoveForDebug("LIVE_P1")`
  - `SyncCurrentStateForDebug()`
  - `RefreshLiveEvidenceForDebug()`
  - `ApplyPointMoveForDebug()`
- two-point once:
  - `SelectTeachingSequenceForDebug("PendantV3Points")`
  - `SyncCurrentStateForDebug()`
  - `RefreshLiveEvidenceForDebug()`
  - `RunSelectedTeachingSequenceOnceForDebug()`
- `ConfirmPopupForDebug()`
- `SyncCurrentStateForDebug()`
- `RefreshLiveEvidenceForDebug()`

## Verified 2026-04-30 Baseline

- saved points currently present:
  - `LIVE_P1`: `MoveJ`, `J6 ≈ 2.2`
  - `LIVE_P2`: `MoveJ`, `J6 ≈ 3.2`
- `PendantV3Points` contains `2` saved points
- `RunSelectedTeachingSequenceOnceForDebug()` now no longer returns the old v1 lock text
- current live result is:
  - `pendingSequenceApproval=True; sequence=PendantV3Points; count=2; dryRun=False`
  - popup open
  - confirm
  - `[Sequence Run] PendantV3Points live 1회 실행 완료 · 2개 포인트`
- post-run truth is judged from refreshed `Artifacts/live/fr5/latest-state.json`

## Staged Repeatability Inventory

아래는 `manual mode`에서 실제 자세를 바꾼 뒤 current live readback으로 저장해둔 repeatability staging 이름이다.

- `QA0430_MANUAL_HOME_T6`
- `QA0430_HOME_2PT_T6`
- `QA0430_HOME_MULTIAXIS_T5_T6`

이 이름들은 현재 point/sequence 저장소 반영뿐 아니라 2026-04-30 same-session live re-smoke까지 끝난 상태다.

## Truth Rule

성공 판정은 아래 네 개를 같이 본다.

1. popup confirm path reached
2. execute feedback did not fail
3. post-run `SyncCurrentStateForDebug()` completed
4. refreshed `Artifacts/live/fr5/latest-state.json`

UI 문구만 보고 green으로 선언하지 않는다.

## Repeatability Rule

- `2-point / multi-axis` teaching repeatability를 검증할 때도 각 단계는 `Sync -> action -> Sync`를 유지한다.
- `base/home 복귀`, `offset point 저장`, `point apply`, `sequence run once`는 모두 각각 post-sync evidence를 남긴다.
- `QA0430_HOME_2PT_T6`는 same-session 2회 연속 live 1회 실행 후 post-sync 기준으로 repeatability를 확인했다.
- `QA0430_HOME_MULTIAXIS_T5_T6`는 same-session 2회 연속 live 1회 실행 후, 최종 `sleep + post-sync` 기준으로 `Home` 복귀를 확인했다.
- current branch에서 broad named sequence green을 일반화하지 않는다. repeatability 확장은 `Home` 기준 saved `MoveJ` path를 좁게 늘리는 식으로만 본다.

## Locked Constraints

- live sequence는 현재 `MoveJ` saved point만 지원
- current live success is `run once` only
- live loop는 아직 잠금
- sequence live는 여전히 runtime safety gate와 operator confirm을 우회하지 않는다
- gripper/IO mixed sequence live는 아직 열지 않는다
- `Home` 기준 custom repeatability sequence는 현재 `QA0430_HOME_2PT_T6`와 `QA0430_HOME_MULTIAXIS_T5_T6`에 한해 live-green이다

## Stop Conditions

- `8080` port unstable
- stale evidence
- `mode != 0`
- `toolId/userId` missing
- saved point가 `MoveJ`가 아님
- sequence loop 요청
- unexpected controller fault
- popup/gate path failure
