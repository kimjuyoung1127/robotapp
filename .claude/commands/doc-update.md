# /doc-update — FR5 문서 자동 갱신

코드 변경 후 사용자가 `문서업데이트`를 요청하면 이 프로세스를 실행한다.
정본은 로컬 `CLAUDE.md`, `AGENTS.md`, `docs/status/*`, `docs/ref/product/*` 체계로 유지한다.

## 변경 범위 파악

```bash
git diff --name-only HEAD
git diff --name-only --cached
git status --porcelain
```

## FR5 문서 매핑

| 변경 영역 | 필수 갱신 | 조건부 갱신 |
|-----------|----------|-----------|
| `Assets/Scripts/App/Fairino/**` | `docs/status/FR5-LIVE-INTEGRATION-ROADMAP.md` | `docs/ref/product/roadmap/fr5-live-field-checklist.md` |
| `Assets/Scripts/UI/RobotControlV3/**` | `docs/ref/product/ux/robotcontrol-next-session-handoff.md` | `docs/daily/MM-DD/fr5-live-field-checklist.md` |
| `scripts/tests/run_fr5_live_checks.sh` | `docs/status/FR5-LIVE-INTEGRATION-ROADMAP.md` | `docs/ref/product/roadmap/fr5-live-field-checklist.md` |
| `.claude/**`, `harness/**`, `CLAUDE.md` | `CLAUDE.md`, `harness/REGISTRY.md` | `docs/status/FR5-LIVE-INTEGRATION-ROADMAP.md` |
| 현장 검증/실기 evidence | `docs/daily/MM-DD/fr5-live-field-checklist.md` | `docs/ref/product/ux/robotcontrol-next-session-handoff.md` |

## 실행 규칙

1. 현재 변경이 어떤 영역에 속하는지 분류한다.
2. 위 표의 필수 문서를 먼저 갱신한다.
3. 같은 사실을 여러 문서에 길게 복붙하지 않는다.
4. 상세 사실은 하나의 정본에 남기고, 다른 문서에는 짧은 상태 요약만 남긴다.
5. 실기 세션 결과는 수치, 날짜, 정책 변화만 적고 추측은 적지 않는다.

## FR5 문서화 원칙

- `docs/status/FR5-LIVE-INTEGRATION-ROADMAP.md`
  - 현재 기준선, 완료된 구현, 남은 이슈, 다음 단계만 유지
- `docs/daily/MM-DD/fr5-live-field-checklist.md`
  - 실제로 오늘 확인한 사실, 수치, 실패/성공 기록만 유지
- `docs/ref/product/roadmap/fr5-live-field-checklist.md`
  - 현장 체크리스트, go/no-go, 운영 정책만 유지
- `docs/ref/product/ux/robotcontrol-next-session-handoff.md`
  - 다음 세션이 바로 따라야 하는 절차와 주의점만 유지

## 검증 보고 형식

```text
## Doc Update Report

변경 파일: N개
갱신 문서: M개
- FR5-LIVE-INTEGRATION-ROADMAP.md: 기준선/정책 반영
- fr5-live-field-checklist.md: 현장 로그 반영
- robotcontrol-next-session-handoff.md: 다음 세션 절차 반영
```
