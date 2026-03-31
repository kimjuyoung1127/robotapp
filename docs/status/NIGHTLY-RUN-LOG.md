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

[docs nightly organizer 완료] 2026-03-12 13:09
- moved_daily_count: 2
- weekly_created_or_updated: 2026-W11.md (03-12 entries added: Home/Sandbox sync, P0/P1 reprioritization)
- broken_links: 1 (docs/ref/PRODUCT-ROADMAP.md → ./product/roadmap/asset-sourcing-checklist.md — 파일 없음)
- manual_required: 0

[docs nightly organizer 완료] 2026-03-27 09:31
- moved_daily_count: 5 (W12: 03-16×3, 03-17×2) + 4 (W13: 03-23×4) = 9 daily logs 처리
- weekly_created_or_updated: 2026-W12.md (신규 생성 — W12 daily logs 5건 롤업), 2026-W13.md (신규 생성 — W13 daily logs 4건 롤업)
- broken_links: 0 (docs/ 전체 Python 스캔 완료 — 이전 broken link asset-sourcing-checklist.md 해소 확인)
- manual_required: 0
- note: 2026-W12 및 2026-W13 주간 파일 누락 감지 및 생성 완료. 정합성 drift 0, 스킬 10/10 HEALTHY.

[docs nightly organizer 완료] 2026-03-27 13:08
- moved_daily_count: 0 (W13 이미 처리 완료 — 09:31 실행 시 처리)
- weekly_created_or_updated: 2026-W13.md (updated — 13:08 재실행 노트 추가)
- broken_links: 0
- manual_required: 0
- note: 당일 2차 실행. daily 신규 로그 없음(03-27 folder 미생성). W13 파일 최신화 확인.
