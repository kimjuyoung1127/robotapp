---
name: editmode-test-add
description: "EditMode 테스트 추가 — 테스트, EditMode, 단위 테스트, NUnit, 수학 테스트"
---

## Trigger
새로운 EditMode 테스트 요청 시, 또는 다른 스킬에서 테스트 생성이 필요할 때.

## Input Context
- 대상 클래스/메서드
- 기대 동작
- 알려진 참조값

## Read First
1. `docs/ref/code-patterns.md` — C# 코딩 패턴 (§4 테스트 보일러플레이트, §5 행렬 비교 헬퍼)
2. `Assets/Tests/EditMode/CLAUDE.md` — EditMode 테스트 컨벤션
2. `Assets/Tests/EditMode/` 기존 테스트 파일들 — 패턴 참조
3. `docs/ref/test-reference-values.md` — 알려진 기준값
4. 테스트 대상 소스 파일

## Do
1. `Assets/Tests/EditMode/{TargetClass}Tests.cs` 생성 (또는 기존 파일에 추가)
2. NUnit `[Test]` 어트리뷰트 사용
3. 최소한 항등 케이스 + 알려진 값 케이스 1개 포함
4. double 비교에 허용 오차 사용: `Assert.AreEqual(expected, actual, delta: 1e-10)`
5. 테스트 대상 설명하는 XML doc summary 추가
6. 기대값 출처 참조 (수식 또는 test-reference-values.md)
7. Unity Test Runner > EditMode > Run All로 실행
8. 모든 테스트 통과 확인

## Do Not
1. 부동소수점 비교에서 허용 오차 없이 `Assert.AreEqual` 사용 금지
2. 알려진 기대값 없이 테스트 생성 금지
3. EditMode와 PlayMode 테스트 어트리뷰트 혼용 금지
4. 새 테스트 추가 후 전체 테스트 스위트 실행 생략 금지

## Validation
- [ ] 테스트 파일이 Assets/Tests/EditMode/에 위치
- [ ] 테스트 메서드에 [Test] 어트리뷰트
- [ ] 항등 케이스 존재
- [ ] 참조가 있는 알려진 값 케이스
- [ ] double 비교에 허용 오차 사용
- [ ] 모든 EditMode 테스트 통과
- [ ] Unity 컴파일: 에러 0

## Output Template
```
[editmode-test-add 완료]
- 대상: {TargetClass}
- 파일: Assets/Tests/EditMode/{TargetClass}Tests.cs
- 테스트 수: {n}
- 항등 케이스: 포함
- 참조값 케이스: 포함 ({출처})
- 전체 테스트: {n}/{n} 통과
```
