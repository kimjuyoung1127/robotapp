# 야간 실행 로그

`docs-nightly-organizer` 자동화의 append-only 실행 기록.

## 형식
```
[docs nightly organizer 완료] YYYY-MM-DD HH:mm
- moved_ref_count: X
- moved_daily_count: X
- weekly_created_or_updated: <파일|none>
- broken_links: X
- manual_required: X
```

## 기록

[docs nightly organizer 완료] 2026-03-11 13:09
- moved_daily_count: 2
- weekly_created_or_updated: 2026-W11.md (already up to date — 03-11 entries confirmed)
- broken_links: 0
- manual_required: 0
- note: DRY_RUN=true (lock 미생성)
