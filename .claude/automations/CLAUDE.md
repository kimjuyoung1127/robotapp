# automations/

외부 스케줄러용 자동화 프롬프트 에셋.

## 구성
- `docs-nightly-organizer.prompt.md` — 평일 14:00 KST, 문서 정리 + weekly 롤업
- `code-doc-align.prompt.md` — 평일 13:00 KST, 코드-문서 드리프트 감지
- `automation-health-monitor.prompt.md` — 평일 13:30 KST, 건강 모니터

## 규칙
- 프롬프트는 결정적(deterministic)이고 멱등(idempotent)하게 유지.
- 스케줄/타임존은 각 프롬프트 내에 명시.
- DRY_RUN 모드 지원 필수.
