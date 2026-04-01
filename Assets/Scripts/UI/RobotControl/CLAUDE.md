# UI/RobotControl

RobotControl 페이지 전용 UI.

## 포함 대상
- Fairino/FR5 control panels
- diagnostics drawer와 axis overlay
- hand-input diagnostics 표현 상태/formatter

## 새 구조 기준 하위 폴더
- `Shell/` — `RobotControlShell`, `TopStatusBar`, 탭 셸
- `Connection/` — connect/enable/status strip UI
- `Motion/` — easy motion, joint jog, TCP jog, point move
- `Teaching/` — points/sequence 교육형 편집 UI
- `Status/` — 상태 요약, 최근 이벤트, 세션 리포트 최소 버전
- `Help/` — 안전 안내와 도움말
- `Popups/` — move confirm, reconnect prompt
- `Diagnostics/` — 개발/진단용 drawer와 보조 surface

## 운영 규칙
1. UI는 authored-first로 다룬다. 씬에 authored shell이 있으면 `FairinoRobotControlViewBuilder.TryBindExistingLayout(...)` 경로를 우선 사용한다.
2. 새 시각 요소를 추가해도 `UIDesignTokens`, `UiRuntimeStyle`, `UIComponentFactory`를 통해 shared 디자인 토큰을 유지한다.
3. `RobotControlV2`의 시안/정식 색상과 레이아웃 치수는 `UIDesignTokens.RobotControlV2`를 SSOT로 사용한다. 로컬 색 상수나 패널별 별도 팔레트를 다시 만들지 않는다.
4. authored 위치를 지킬 때는 `play 시작 1회 authored-lock`만 허용한다. Update/LateUpdate/반복 훅으로 UI 위치를 계속 덮어쓰지 않는다.
5. `RobotControlV2` 검증도 `Onboarding` 강제 시작을 유지한 채 실제 사용자 진입 흐름 안에서 본다.
6. 실기 bring-up 상태 표시는 `Mode`, `Drag`, `Tool/User`, `Safety`, `Fault`를 우선한다.
7. Live v1 경로에서는 `ServoJ` / `ServoCart`를 실제 조작 버튼으로 보지 않는다. 버튼은 남겨도 비활성화 상태와 이유 텍스트를 명확히 보여준다.
8. 구현은 `TopStatusBar -> EasyMotion -> TcpJog -> JointJog -> PointMove -> StatusSummary -> Popups -> Tablet` 순서를 기본으로 진행한다.
9. 각 UI 구현 Phase 종료 전에는 자기리뷰를 수행한다.
   - 토큰 우회 여부
   - `UIDesignTokens.RobotControlV2` 직접 사용 여부
   - authored-first 바인딩 여부
   - 패널 비대화 여부
   - `필수 / 선택 / 제외` 범위 준수 여부
10. 각 UI 구현 Phase 종료 전에는 `unityctl`로 최소 `compile`, 관련 `EditMode`, 필요 시 `play start`와 `screenshot capture`를 수행한다.
11. UI 구현은 브랜치에서 진행하고, 검증이 끝난 Phase만 커밋한다.
12. `Initial`, `Application`, `System`의 설정형/산업형 화면을 메인 `RobotControl` V1 패널에 넣지 않는다.
