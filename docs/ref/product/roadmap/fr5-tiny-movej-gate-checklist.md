# FR5 Tiny MoveJ Approval Gate Checklist

Last Updated: 2026-04-28 (KST)

## Purpose

이 문서는 `tiny MoveJ`를 실제로 열기 전에 무엇을 승인 조건으로 고정하고, 어떤 테스트를 병렬로 검증할지 체크리스트로 잠그기 위한 SSOT다.

여기서 말하는 `tiny MoveJ`는 다음 조건을 모두 만족할 때만 허용하는 첫 motion gate다.

- 1회
- 저속
- 작은 범위
- 실패 시 즉시 다시 잠금

## Gate Meaning

`tiny MoveJ 승인 게이트`는 MoveJ 기능 추가가 아니다.

- 실제 로봇을 조금이라도 움직여도 되는지 코드가 먼저 판정한다.
- 조건 하나라도 빠지면 차단하고, 운영자 화면에 이유를 보여준다.
- 승인이 되어도 `1회 승인`만 허용하고, 실행 뒤에는 다시 잠근다.

## Approval Checklist

모두 만족해야 승인 가능이다.

- [ ] live readback이 정상이다.
- [ ] 현재 세션이 `readback-only`가 아니다.
- [ ] 로봇 연결이 살아 있다.
- [ ] 서보가 켜져 있다.
- [ ] `toolId > 0`
- [ ] `userId > 0`
- [ ] `coordSystem`이 `Base`, `Tool`, `User` 중 하나로 명확하다.
- [ ] `latest-state.json`이 최신 세션 기준으로 fresh하다.
- [ ] `latest-drift.json`이 최신 세션 기준으로 fresh하다.
- [ ] drift 판정이 통과다.
- [ ] speed cap 이내다.
- [ ] preview artifact가 있다.
- [ ] production IK guard가 통과다.
- [ ] boundary guard가 통과다.
- [ ] collision guard가 통과다.
- [ ] operator confirm token이 현재 명령에 대해 유효하다.

## Block Checklist

하나라도 걸리면 차단이다.

- [ ] 미연결
- [ ] 서보 OFF
- [ ] `readback-only` live client
- [ ] `toolId == 0`
- [ ] `userId == 0`
- [ ] `coordSystem` 불명확
- [ ] evidence stale
- [ ] drift fail
- [ ] speed cap 초과
- [ ] fault / e-stop / safety stop / collision flag
- [ ] motion queue 잔존
- [ ] preview artifact 없음
- [ ] IK / boundary / collision guard 실패
- [ ] confirm token 없음 또는 만료

## Relock Checklist

- [ ] 승인 토큰은 `MoveJ` 1회용이다.
- [ ] gate 실패 시 즉시 승인 상태를 재잠금한다.
- [ ] 실행 실패 시 즉시 승인 상태를 재잠금한다.
- [ ] 다음 시도는 fresh readback + fresh confirm부터 다시 시작한다.

## Operator UI Checklist

- [ ] 대표 상태가 `미연결 / 연결됨 · 위치 확인 전 / 연결됨 · 위치 확인 완료 / 실제 이동: 잠겨 있음` 기준으로 읽힌다.
- [ ] `현재 위치 읽음` 완료 여부가 바로 보인다.
- [ ] `도구 설정 / 작업 기준 / 좌표 기준`이 일반어로 보인다.
- [ ] `실제 이동 가능/잠김`과 `잠금 이유`가 같은 화면에서 함께 보인다.
- [ ] `왜 잠겨 있는지`에 최소 `readback-only`, `evidence freshness`, `drift`, `operator confirm` 중 무엇이 걸렸는지 드러난다.
- [ ] `언제 풀리는지`가 다음 행동 문구로 보인다.
- [ ] confirm popup은 토큰 숫자보다 승인 대상 설명이 먼저 보인다.
- [ ] `readback-only` 세션에서는 어떤 경로에서도 motion enable이 열리지 않는다.

## Parallel Test Cards

### Card A. Gate Matrix

목표:
- 공통 gate가 tiny `MoveJ` 승인/차단 조건을 정확히 계산하는지 검증

케이스:
- [ ] not connected
- [ ] servo disabled
- [ ] readback-only
- [ ] tool missing
- [ ] user missing
- [ ] coordSystem unresolved
- [ ] evidence stale
- [ ] drift fail
- [ ] speed cap exceeded
- [ ] preview missing
- [ ] IK fail
- [ ] boundary fail
- [ ] collision fail
- [ ] operator confirm required
- [ ] operator confirm accepted once
- [ ] confirm after failure relocks

추천 파일:
- `Assets/Scripts/App/Fairino/LiveCommandSafetyGate.cs`
- `Assets/Tests/EditMode/Integration/LiveCommandSafetyGateTests.cs`

### Card B. Runtime Approval Flow

목표:
- V3 runtime이 tiny `MoveJ` 승인 토큰과 1회 소비, 재잠금을 정확히 묶는지 검증

케이스:
- [ ] `BeginLiveCommandApprovalForProduct()`가 pending token을 만든다.
- [ ] invalid token은 차단된다.
- [ ] valid token은 1회 승인으로 전환된다.
- [ ] gate 소비 후 승인 상태가 다시 비워진다.
- [ ] readback-only 세션이면 confirm이 있어도 계속 잠긴다.

추천 파일:
- `Assets/Scripts/App/Fairino/RobotControlV3RuntimeController.cs`
- `Assets/Scripts/UI/RobotControlV3/PopupCoordinatorV3.cs`
- `Assets/Tests/EditMode/Integration/Fr5LiveReadbackTests.cs`

### Card C. Operator UI Surface

목표:
- 운영자가 디버그 브리지 없이도 `왜 잠겨 있는지 / 언제 풀리는지`를 읽을 수 있는지 검증

케이스:
- [ ] 연결 전
- [ ] 연결 후 readback 전
- [ ] readback 완료 후
- [ ] readback-only live
- [ ] evidence stale 가정
- [ ] drift fail 가정
- [ ] confirm required 상태
- [ ] popup confirm open/cancel
- [ ] header / diagnostics / status card 문구 정합성

추천 파일:
- `Assets/Scripts/App/Fairino/RobotControlV3RuntimeSnapshot.cs`
- `Assets/Scripts/UI/RobotControlV3/SafetyDiagnosticsController.cs`
- `Assets/Scripts/UI/RobotControlV3/StatusCardController.cs`
- `Assets/Scripts/UI/RobotControlV3/ConnectionHomeController.cs`

## Priority Files

1. `Assets/Scripts/App/Fairino/RobotControlV3RuntimeController.cs`
2. `Assets/Scripts/App/Fairino/LiveCommandSafetyGate.cs`
3. `Assets/Scripts/UI/RobotControlV3/PopupCoordinatorV3.cs`
4. `Assets/Scripts/App/Fairino/RobotControlMotionRuntime.cs`
5. `Assets/Scripts/App/Fairino/RobotControlV3RuntimeSnapshot.cs`
6. `Assets/Scripts/UI/RobotControlV3/SafetyDiagnosticsController.cs`
7. `Assets/Scripts/UI/RobotControlV3/StatusCardController.cs`
8. `Assets/Tests/EditMode/Integration/LiveCommandSafetyGateTests.cs`

## Current Recommendation

다음 구현 순서는 아래를 권장한다.

1. gate 입력 확장
2. runtime 승인/재잠금 연결
3. operator UI 노출 정리
4. gate matrix 테스트 추가
5. runtime approval 테스트 추가
6. UI 상태 노출 테스트 확인
