# commands

FR5UNITY에서 반복 호출하는 Claude command 문서 모음입니다.

## Commands
- `doc-update.md`: 코드 변경 후 필수 상태 문서/현장 로그를 어디까지 갱신할지 정리하고 실제 수정까지 수행하는 규칙
- `live-gate-review.md`: FR5 readback-only live 기준선, evidence freshness, motion gate, poll baseline을 빠르게 자기점검하는 규칙
- `status-copy-review.md`: Pendant V3 운영자 상태문구가 SSOT와 금지 토큰 규칙을 지키는지 점검하는 규칙

## Rules
- command는 점검만 하지 말고, 가능한 범위에서는 실제 수정/실행까지 이어간다.
- `unityctl`이 가능한 작업은 shell보다 `unityctl` 중심으로 적는다.
- FR5 live 관련 command는 항상 `readback-only` 정책을 유지한다.
