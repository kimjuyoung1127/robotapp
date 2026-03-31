# Tests/

Unity Test Runner 테스트 스위트.

## 구조
- `EditMode/` — Play 없이 도는 테스트
- `EditMode/Core/` — 수학, FK, 템플릿, 저장/상태 같은 핵심 계약
- `EditMode/Integration/` — UI/runtime/Fairino 결합 테스트
- `EditMode/Validation/` — 씬 전수조사, 외부 SDK, 자산 구조 검증
- `PlayMode/` — 씬을 띄우는 통합 테스트
- `PlayMode/Smoke/` — 빠른 스모크 체크
- `PlayMode/Flows/` — 씬 이동, UX 흐름, 라우팅
- `PlayMode/Visuals/` — 시각/패널/렌더링 검증

## 규칙
1. EditMode: NUnit `[Test]` 어트리뷰트, 씬 불필요
2. PlayMode: `[UnityTest]` 어트리뷰트, 씬 로드 가능
3. 부동소수점 비교 허용 오차: 수학 1e-10, 위치 1e-4
4. 테스트 파일 명명: `{TargetClass}Tests.cs`
5. Assembly Definition: 모듈별 .asmdef 파일
6. 자주 도는 회귀는 `Core/`와 `Smoke/`에 두고, 유지비 큰 검증은 `Validation/` 또는 `Flows/Visuals/`로 분리
