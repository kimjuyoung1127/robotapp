---
name: dh-algorithm-add
description: "DH 변형 또는 기구학 알고리즘 추가 — DH, Modified DH, 역기구학, IK, 자코비안, Jacobian, 기구학 알고리즘"
---

## Trigger
새로운 DH 변형(Modified DH, Product of Exponentials) 또는 기구학 알고리즘(IK, Jacobian) 요청 시.

## Input Context
- 알고리즘 이름
- 수학 공식
- 기존 알고리즘 확장인지 대체인지 여부

## Read First
1. `Assets/Scripts/Kinematics/CLAUDE.md` — 기구학 모듈 규칙
2. `Assets/Scripts/Kinematics/DHStandard.cs` — 참조 구현 (베이스라인 보호)
3. `Assets/Scripts/Kinematics/ForwardKinematics.cs` — FK 엔진
4. `Assets/Scripts/Types/DHLink.cs` — DH 파라미터 데이터 구조
5. `docs/ref/dh-reference.md` — 수학 공식
6. `docs/ref/coordinate-mapping.md` — 좌표 규약

## Do
1. `Assets/Scripts/Kinematics/{AlgorithmName}.cs` 생성
2. XML doc에 수학 공식 참조 (예: `A_i = Rz(θ)·Tz(d)·Tx(a)·Rx(α)`)
3. 출력이 `Mat4D` (동차 변환 행렬)인지 확인
4. 한국어 XML doc summary 추가
5. 알려진 수치 케이스로 EditMode 테스트 생성 (`editmode-test-add` 호출)
6. `DHStandard.cs` 미변경 확인 (`git diff` 체크)
7. 전체 EditMode 테스트 실행

## Do Not
1. `DHStandard.cs` 수정 금지 (베이스라인 보호)
2. 모든 소비자 업데이트 없이 `DHLink` 데이터 구조 변경 금지
3. 기구학 계산에서 단정밀도(`float`) 사용 금지
4. 수치 참조 검증 생략 금지

## Validation
- [ ] DHStandard.cs 미변경 (diff 체크)
- [ ] 새 알고리즘 출력이 Mat4D
- [ ] 코드 주석에 수학 공식 참조
- [ ] EditMode 테스트에 수치 검증 (허용 오차 < 1e-10)
- [ ] 모든 기존 테스트 통과
- [ ] Unity 컴파일: 에러 0

## Output Template
```
[dh-algorithm-add 완료]
- 알고리즘: {AlgorithmName}
- 파일: Assets/Scripts/Kinematics/{AlgorithmName}.cs
- 테스트: Assets/Tests/EditMode/{AlgorithmName}Tests.cs
- DHStandard.cs: 미변경 확인
- 수치 검증: 허용 오차 내
- EditMode 테스트: {n}/{n} 통과
```
