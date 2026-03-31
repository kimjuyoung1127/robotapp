# EditMode/

Play 모드 진입 없이 실행하는 순수 로직 테스트.

## 구조
- `Core/` — 순수 계약, 템플릿, 상태 저장, 수학/FK 테스트
- `Integration/` — UI 패널, coordinator, 서비스 결합 테스트
- `Validation/` — 외부 SDK, 씬/자산 전수조사, 구조 검증
- `CliTools/` — CLI 도구 전용 테스트

## 컨벤션
1. 대상 클래스당 1개 테스트 파일 (예: `Vec3DTests.cs`, `DHStandardTests.cs`)
2. 최소 테스트 케이스: 항등 케이스 + 알려진 값 케이스 1개
3. 참조값: `docs/ref/test-reference-values.md`
4. double 비교: `Assert.AreEqual(expected, actual, delta)` 사용
5. Assembly Definition: `KineTutor3D.Tests.EditMode.asmdef`
6. `Core/`는 빠르고 안정적이어야 하며, authored hierarchy나 scene asset 스캔은 `Validation/`으로 보낸다

## 관련 스킬
- `editmode-test-add` — 새 EditMode 테스트 추가 시 사용
