---
name: robot-template-add
description: "로봇 설정 템플릿 추가 — 새 로봇, 4DOF, 5DOF, SCARA, 산업용 로봇, 커스텀 로봇"
---

## Trigger
새로운 로봇 설정(4DOF, 5DOF, 커스텀 로봇, SCARA 변형 등) 요청 시.

## Input Context
- DOF 수
- 관절 타입들 (Revolute/Prismatic)
- DH 파라미터 기본값
- 관절 한계(joint limits)
- 지원 모드 (`guided lesson`, `sandbox`, `instructor`)
- 입력 모드 (`slider`, `numeric`, `history`, `pick-foundation`)

## Read First
1. `docs/ref/code-patterns.md` — C# 코딩 패턴 (§8 Unity 측 규칙 포함)
2. `Assets/Scripts/Templates/CLAUDE.md` — 템플릿 컨벤션
3. `Assets/Scripts/Templates/Template2DOF_RR.cs` — 참조 템플릿 (존재 시)
3. `Assets/Scripts/Types/RobotTemplate.cs` — 기본 템플릿 타입
4. `Assets/Scripts/Types/JointType.cs` — 관절 타입 열거형
5. `Assets/Scripts/Types/DHLink.cs` — DH 파라미터 구조체
6. `docs/ref/test-reference-values.md` — 기준값
7. `docs/status/PHASE-EXECUTION-BOARD.md` — 현재 템플릿 상태
8. `docs/ref/product/robots/robot-model-library-spec.md`
9. `docs/ref/product/robots/robot-template-expansion.md`

## Do
1. `Assets/Scripts/Templates/Template{N}DOF_{Name}.cs` 생성 (Template2DOF_RR 패턴 따름)
2. 기본 DH 파라미터 배열 정의
3. 모든 관절에 대한 관절 한계 정의
4. 로봇 metadata(`robot_id`, `display_name`, `difficulty`, `supported_modes`, `input_modes`, `visualization_level`)를 함께 정의한다.
5. 확장 순서(`2DOF -> SCARA -> 3DOF -> 6DOF`)와 demo-first 정책에 맞는지 확인한다.
6. FK 계산이 템플릿과 동작하는지 확인 (필요 시 `dh-algorithm-add` 호출)
7. 최소 1개 알려진 설정에 대한 EditMode 테스트 추가 (`editmode-test-add` 호출)
8. `docs/status/PHASE-EXECUTION-BOARD.md`에 항목 추가
9. `docs/status/SKILL-DOC-MATRIX.md`의 primary_code_paths 업데이트
10. Unity 컴파일 및 전체 테스트 통과 확인

## Do Not
1. DH 파라미터 기본값 없이 템플릿 생성 금지
2. 관절 한계 정의 생략 금지
3. 수치 검증 없이 EditMode 테스트 생략 금지
4. 새 템플릿 추가 시 기존 템플릿 수정 금지
5. 로봇 metadata 없이 UI 진입 경로를 추론하지 않는다.

## Validation
- [ ] 템플릿 파일이 명명 규칙 준수
- [ ] DH 파라미터 기본값 정의됨
- [ ] 모든 관절에 관절 한계 정의됨
- [ ] robot metadata와 지원 모드 정의됨
- [ ] 확장 순서와 demo-first 정책에 부합
- [ ] 알려진 설정으로 FK 테스트 통과
- [ ] PHASE-EXECUTION-BOARD.md 업데이트됨
- [ ] SKILL-DOC-MATRIX.md 업데이트됨
- [ ] Unity 컴파일: 에러 0
- [ ] 모든 EditMode 테스트 통과

## Output Template
```
[robot-template-add 완료]
- 템플릿: Template{N}DOF_{Name}
- 파일: Assets/Scripts/Templates/Template{N}DOF_{Name}.cs
- 테스트: Assets/Tests/EditMode/Template{N}DOF_{Name}Tests.cs
- DH 파라미터: {N}개 링크 정의
- 관절 한계: 정의됨
- FK 검증: 통과
- 보드/매트릭스: 업데이트 완료
```
