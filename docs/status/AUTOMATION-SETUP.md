# 자동화 설정 가이드

## 개요
3개의 자동화 프롬프트를 외부 스케줄러(Claude Code Automation, GitHub Actions, cron 등)에서 실행.

## 스케줄

| 자동화 | 시간 (KST) | 프롬프트 파일 |
|--------|-----------|-------------|
| code-doc-align | 21:30 | `.claude/automations/code-doc-align.prompt.md` |
| docs-nightly-organizer | 22:00 | `.claude/automations/docs-nightly-organizer.prompt.md` |
| automation-health-monitor | 09:30 | `.claude/automations/automation-health-monitor.prompt.md` |

## Lock 파일 규칙
- RUNNING 상태 2시간 초과 시 STUCK으로 판정
- STUCK Lock은 자동 해제 금지 — 수동 개입 필요
- Lock 파일 위치:
  - `docs/.docs-nightly.lock`
  - `docs/status/.code-doc-align.lock`

## DRY_RUN 모드
- 모든 자동화는 `DRY_RUN=true` 설정 시 미리보기만 수행
- 파일 수정 없이 리포트만 출력

## 확장 (미래)
- Slack 웹훅: `SLACK_WEBHOOK_URL` 환경 변수 설정 시 활성화
- Notion 연동: MCP 서버 활용 가능
