# Tests/

Unity Test Runner 테스트 스위트.

## 구조
- `EditMode/` — 순수 로직 테스트 (수학, 기구학, 타입)
- `PlayMode/` — 통합/씬 테스트 (UI, 시각화, 검증)

## 규칙
1. EditMode: NUnit `[Test]` 어트리뷰트, 씬 불필요
2. PlayMode: `[UnityTest]` 어트리뷰트, 씬 로드 가능
3. 부동소수점 비교 허용 오차: 수학 1e-10, 위치 1e-4
4. 테스트 파일 명명: `{TargetClass}Tests.cs`
5. Assembly Definition: 모듈별 .asmdef 파일
