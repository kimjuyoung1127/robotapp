---
name: math-module-add
description: "수학 모듈 추가 또는 기존 Vec3D/Mat3D/Mat4D 확장 — 수학, vector, matrix, quaternion, 새 수학 타입"
---

## Trigger
새로운 수학 타입, 벡터 연산, 행렬 연산 요청 또는 기존 수학 모듈 확장 시.

## Input Context
- 새 타입 이름
- 필요한 연산 목록
- 정밀도 요구사항 (항상 double)

## Read First
1. `Assets/Scripts/Math/CLAUDE.md` — 수학 모듈 규칙
2. `docs/ref/code-patterns.md` — C# readonly struct / NaN 가드 / 행렬 비교 패턴
3. `Assets/Scripts/Math/Vec3D.cs` — 기존 벡터 구현 패턴 (존재 시)
4. `Assets/Scripts/Math/Mat4D.cs` — 기존 행렬 구현 패턴 (존재 시)
5. `docs/ref/dh-reference.md` — 수학 레퍼런스
6. `Assets/Tests/EditMode/CLAUDE.md` — 테스트 컨벤션

## Do
1. `Assets/Scripts/Math/{TypeName}.cs` 생성 (기존 패턴 따름: 순수 C#, double 정밀도)
2. 클래스 레벨에 한국어 XML doc summary 추가
3. 생성자와 연산자에 NaN/Infinity 가드 구현
4. 해당하는 경우 Identity/Zero 팩토리 메서드 추가
5. `Assets/Tests/EditMode/{TypeName}Tests.cs` 생성 — 항등 케이스 + 알려진 값 케이스 (`editmode-test-add` 호출)
6. Unity 컴파일 성공 확인 (Console 에러 0)
7. Test Runner에서 EditMode 테스트 실행

## Do Not
1. 수학 모듈에서 `using UnityEngine` 임포트 금지
2. `float` 사용 금지 — 모든 값은 `double`
3. NaN/Infinity 가드 생략 금지
4. 대응하는 EditMode 테스트 없이 수학 타입 생성 금지

## Validation
- [ ] 순수 C# (`using UnityEngine` 없음)
- [ ] 모든 필드/연산이 `double` 사용
- [ ] 생성자에 NaN/Infinity 가드
- [ ] XML doc summary 존재
- [ ] EditMode 테스트 존재 및 통과
- [ ] Unity 컴파일: 에러 0

## Output Template
```
[math-module-add 완료]
- 타입: {TypeName}
- 파일: Assets/Scripts/Math/{TypeName}.cs
- 테스트: Assets/Tests/EditMode/{TypeName}Tests.cs
- 정밀도: double
- Unity 컴파일: 통과
- EditMode 테스트: {n}/{n} 통과
```
