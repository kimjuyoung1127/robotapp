# Module Log: Math Readiness Shell Polish

Date: 2026-03-12 (KST)

## Summary
- MathReadiness 화면을 `현재 학습 / 현재 행동 / 도움말` 3블록 구조로 다시 정리했다.
- 첫 화면에서는 워밍업만 보이고, 워밍업 완료 후 본 문제를 여는 순차 흐름으로 고쳤다.
- 상단 바와 하단 바도 학습 우선 구조에 맞게 단순화했다.

## Updated Docs
- `docs/ref/product/ux/guided-lesson.md`
- `docs/ref/product/roadmap/current-feature-checklist.md`
- `docs/status/PROJECT-STATUS.md`

## Verification Notes
- `dotnet build robotapp2.sln` green
- Unity EditMode `201/201`
- Unity PlayMode `45/45`
- 첫 화면에서 `BtnReadinessChoice_*`가 보이지 않고, 워밍업 후에만 나타나는 흐름을 PlayMode smoke로 확인
