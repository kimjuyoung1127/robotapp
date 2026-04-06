# Pendant V3 Phase Split

## Date
- 2026-04-06 (KST)

## Summary
- `docs/ref/product/pendant-v3/implementation-plan.md`의 초기 실행 단위를 `Phase 0A~3C`로 세분화했다.
- 큰 묶음형 `Phase 0~3`만으로는 UI Toolkit 인프라, 입력, 포커스, 반응형 셸, 바인딩, 로컬 서비스, Mock 종단 검증이 한 번에 섞여 회귀 범위가 너무 넓다고 판단했다.
- 새 기준은 `한 실행 단위 = 한 검증 루프 = 한 커밋 후보`다.

## Updated Docs
- `docs/ref/product/pendant-v3/implementation-plan.md`
- `docs/ref/product/pendant-v3/README.md`

## New Execution Slices
- `0A`: PanelSettings / TextSettings / SpriteAtlas / UIDocument
- `0B`: 루트 셸 UXML/USS 5영역
- `0C`: InputSystem / EventSystem / 포커스 순서
- `1A`: Desktop 셸
- `1B`: Tablet 셸 + 시안 리뷰
- `1C`: 탭 지속성 / 로컬 레이아웃 상태
- `2A-1`: 연결 홈
- `2A-2`: 상태/좌표 패널
- `2B-1`: 쉬운 조작
- `2B-2`: 관절 조그
- `2B-3`: TCP 조그
- `2B-4`: 포인트 이동
- `2C-1`: 안전/진단
- `2C-2`: 뷰포트 툴바 / 작업공간 경계 / 충돌 표시
- `2D`: 팝업/도움말
- `3A`: Binder / Scene bootstrap
- `3B`: Undo/Redo / LocalSettings / AutoReconnect
- `3C`: Mock 종단 검증

## Why This Split
- UI Toolkit 초기 리스크를 `패널 자산`, `입력`, `포커스`, `반응형`, `바인딩`, `서비스`, `종단 검증`으로 분리해 문제 범위를 줄이기 위함이다.
- V2 대비 비교 평가도 이제 셸, 입력, 조그, 상태, Mock 검증 단위로 더 정확히 할 수 있다.
