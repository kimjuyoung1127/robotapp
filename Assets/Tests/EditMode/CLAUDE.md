# EditMode/

Play 모드 진입 없이 실행하는 순수 로직 테스트.

## 컨벤션
1. 대상 클래스당 1개 테스트 파일 (예: `Vec3DTests.cs`, `DHStandardTests.cs`)
2. 최소 테스트 케이스: 항등 케이스 + 알려진 값 케이스 1개
3. 참조값: `docs/ref/test-reference-values.md`
4. double 비교: `Assert.AreEqual(expected, actual, delta)` 사용
5. Assembly Definition: `KineTutor3D.Tests.EditMode.asmdef`

## 관련 스킬
- `editmode-test-add` — 새 EditMode 테스트 추가 시 사용
