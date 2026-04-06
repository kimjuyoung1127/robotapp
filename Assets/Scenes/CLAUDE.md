# Scenes/

Unity 씬 관리.

## 씬 목록
- `Boot.unity` — 첫 방문/재방문 라우터
- `Onboarding.unity` — 온보딩 진입
- `RobotLibrary.unity` — 메인 진입점
- `Sandbox.unity` — 자유 조작
- `RobotControl.unity` — 기존 RobotControl
- `RobotControlV2.unity` — V2 soft teaching pad 비교 경로
- `RobotControlV3.unity` — V3 UI Toolkit 비교 후보
- `MathReadiness.unity` — 수학 기초 워밍업

## 규칙
1. 현재 런타임 SSOT는 루트 `AGENTS.md`와 `SceneCatalog.cs`
2. 비교용 씬(`RobotControlV2`, `RobotControlV3`)은 채택 결정 전까지 메인 경로로 승격하지 않음
3. 씬 추가/수정 시 `EditorBuildSettings`와 `SceneCatalog` 동기화 확인
