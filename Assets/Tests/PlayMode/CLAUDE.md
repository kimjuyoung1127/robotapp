# PlayMode/

Play 모드에서 씬 컨텍스트와 함께 실행하는 통합 테스트.

## 구조
- `Smoke/` — 버튼/패널/lesson 핵심 스모크
- `Flows/` — Boot, Onboarding, RobotLibrary, Sandbox, RobotControl 라우팅과 UX 흐름
- `Visuals/` — 공통 비주얼, 패널 디자인 시스템, 렌더링 검증

## 컨벤션
1. 용도: 검증 스모크 테스트, UI 상호작용 테스트
2. `[UnityTest]` 어트리뷰트 + `yield return` 사용
3. 현재 런타임 진실은 `Boot -> Onboarding -> RobotLibrary -> {MathReadiness, Sandbox, RobotControl}` 기준으로 검증
4. Assembly Definition: `KineTutor3D.Tests.PlayMode.asmdef`
