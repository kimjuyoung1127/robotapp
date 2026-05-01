# FR5 Live Field Checklist

## 목적

- 이 문서는 `현재 실기 세션에서 바로 체크할 항목`만 남긴 current checklist다.
- 과거 현장 로그와 시행착오는 `docs/daily/`와 status roadmap에서 본다.
- 현장 서사와 시행착오를 한 번에 보려면 [fr5-live-field-history.md](./fr5-live-field-history.md)를 본다.
- gripper 상세 성공패턴은 [fr5-gripper-live-success-pattern.md](./fr5-gripper-live-success-pattern.md)를 SSOT로 본다.
- tiny joint 상세 성공패턴은 [fr5-tiny-joint-live-success-pattern.md](./fr5-tiny-joint-live-success-pattern.md)를 SSOT로 본다.

## 현재 연결 기준

- `eth0 = 192.168.57.2`
- `eth1 = 192.168.58.2`
- MacBook Ethernet IP는 실제로 꽂힌 포트 대역과 맞춰야 한다.
- current operator baseline은 `연결 = connect + 현재 위치 읽기`다.

## 세션 시작 체크

1. Unity가 `RobotControlV3`에 정상 진입했는지 확인
2. `연결` 1회로 live connect + sync가 같이 되는지 확인
3. `latest-state.json`이 fresh한지 확인
4. `toolId > 0`, `userId > 0`, `coordSystem` truth 확인
5. `latest-drift.json`이 `severity=ok`인지 확인
6. 이번 세션이 `readback-only`, `gripper-only`, `tiny-movej-only` 중 무엇인지 먼저 잠금

## 읽기 전용 세션 체크

- `clientMode=direct` 또는 허용된 fallback mode인지 확인
- disconnected placeholder zero state가 마지막 정상 evidence를 덮지 않는지 확인
- `33ms 기본 / 50ms 폴백` 정책이 유지되는지 확인
- readback freshness와 preservation policy가 정상인지 확인

## Gripper 체크

- 세션 모드는 `gripper-only`만 허용
- 정상 하드웨어 설정과 discrete success pattern은 [fr5-gripper-live-success-pattern.md](./fr5-gripper-live-success-pattern.md)만 기준으로 본다
- operator flow는 `값 선택 -> 미리보기 적용 또는 실제 이동` 기준으로 본다
- quick button은 현재 `100 / 50 / 0`만 허용한다
- `미리보기 적용`은 실기 write 없이 preview만 바뀌어야 한다
- `실제 이동`만 popup confirm과 live write를 탄다
- slider / input / preset 검증도 같은 SSOT 문서 기준으로만 확장한다

## Tiny Joint 체크

- 세션 모드는 `tiny-movej-only`만 허용
- broad arm motion은 열지 않는다
- tiny joint 허용 범위와 현재 green baseline은 [fr5-tiny-joint-live-success-pattern.md](./fr5-tiny-joint-live-success-pattern.md)를 본다

## 증거 파일 체크

- `Artifacts/live/fr5/latest-state.json`
- `Artifacts/live/fr5/latest-drift.json`
- `Artifacts/live/fr5/sessions/*-readback.ndjson`
- `Artifacts/live/fr5/sessions/*-events.ndjson`
- 필요 시 `Artifacts/live/qa/*.json`

## Go / No-Go

### Go

- live connect + sync 성공
- evidence freshness 정상
- tool/user/coord truth 정상
- drift `ok`
- 이번 세션 허용 범위가 명확하게 잠김

### No-Go

- 네트워크 대역 불일치
- stale evidence
- tool/user/coord truth 불명확
- drift fail
- 허용 세션 모드 없이 broad write를 열어야 하는 상황

## Legacy Note

- `V1`, `V2`는 현재 실기 운영 기준 문서가 아니다.
- V1/V2는 삭제 예정 전제로 `개발 목적`과 `남길 이력`만 보존한다.
- 다음 세션 판단은 legacy 구현 전략이 아니라 current FR5 live SSOT와 success pattern 문서를 기준으로 한다.
