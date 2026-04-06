# Pendant V3 Phase Loop Governance

## Date
- 2026-04-06 (KST)

## Summary
- V3 실행 단위를 `SSOT 정합성 루프`로 닫는 운영 규칙을 문서에 추가했다.
- 각 Phase 종료를 문서 기준, 정적 게이트, 컴파일 게이트, 증빙 게이트, parity review로 강제하도록 정리했다.

## Updated
- `docs/ref/product/pendant-v3/implementation-plan.md`
- `docs/ref/product/pendant-v3/AGENT-CONTRACT.md`

## Locked
- 한 실행 단위 = 한 검증 루프 = 한 커밋 후보
- 문서 변경 턴에는 같은 턴에 최소 다음 실행 단위 1개 착수
- 완료 보고는 `산출물 일치 / 검증 통과 / 남은 불일치` 3줄을 최소 포함

## Next
- `0C` 입력/포커스 계약의 최소 구현을 추가해 첫 governed loop를 시작한다.
