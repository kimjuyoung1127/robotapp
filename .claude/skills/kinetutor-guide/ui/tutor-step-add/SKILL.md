---
name: tutor-step-add
description: "튜토리얼 스텝 추가 — 튜터, 학습 단계, step, 튜토리얼, 교육 콘텐츠"
---

## Trigger
새로운 튜토리얼 스텝, 학습 콘텐츠, Step Tutor UI 패널 요청 시.

## Input Context
- 스텝 번호
- 스텝 설명
- 표시할 행렬/값 (A_i, T_0n, EE pose 등)
- 관련 로봇 템플릿
- 연결 concept ids
- `why it moved` 설명 필요 여부

## Read First
1. `docs/ref/code-patterns.md` — C# 코딩 패턴 (§8 Unity 측 규칙 포함)
2. `docs/ref/tutor-step-plan.md` — 8개 튜토리얼 스텝 정의 및 요구사항
3. `Assets/Scripts/UI/CLAUDE.md` — UI 모듈 규칙
3. `Assets/Scripts/UI/StepTutorPanel.cs` — Step Tutor 구현 (존재 시)
4. `Assets/Scripts/UI/DHTableEditor.cs` — DH 테이블 표시 (존재 시)
5. `Assets/Scripts/UI/JointSliderPanel.cs` — 슬라이더 컨트롤 (존재 시)
6. `Assets/Scripts/App/AppController.cs` — 앱 오케스트레이터 (존재 시)
7. `docs/status/PROJECT-STATUS.md` — 현재 Step Tutor 상태
8. `docs/ref/product/ux/guided-lesson.md`
9. `docs/ref/product/content/concept-to-ui-map.md`

## Do
1. StepTutorPanel 스텝 배열/설정에 스텝 정의 추가
2. 해당 스텝의 행렬 표시(A_i, T_0n, EE pose) 업데이트 확인
3. DH 테이블과 슬라이더 패널이 스텝 컨텍스트를 반영하는지 확인
4. 스텝에 대한 한국어 설명 텍스트 추가
5. 각 step에 `학습목표 / 핵심 행동 / 완료 조건 / 실수 포인트`를 명시한다.
6. 필요한 경우 `왜 그렇게 움직였는지` 설명 블록과 `입력값 -> 변화량 -> EE 변화` 요약을 추가한다.
7. 외부 교재 혼동이 예상되면 `DH vs MDH` 주석 또는 강사용 노트를 포함한다.
8. step의 concept refs를 `concept-to-ui-map`와 연결한다.
9. `docs/status/PROJECT-STATUS.md`에 새 스텝 수 또는 범위 업데이트
10. PlayMode 검증: 스텝 텍스트와 행렬 표시가 함께 업데이트되는지 확인

## Do Not
1. 올바른 수학 컨텍스트 참조 없이 튜토리얼 콘텐츠 추가 금지
2. 표시 문자열 하드코딩 금지 (설정 가능한 스텝 데이터 사용)
3. 기존 스텝 내비게이션 흐름 파손 금지
4. PlayMode 시각 검증 생략 금지
5. `theta read-only` 규칙을 깨는 조작 단계를 추가하지 않는다.

## Validation
- [ ] 스텝 설정에 스텝 정의 추가됨
- [ ] 해당 스텝에서 행렬 표시 올바르게 업데이트
- [ ] DH 테이블이 스텝 컨텍스트 반영
- [ ] 한국어 설명 텍스트 존재
- [ ] `학습목표 / 핵심 행동 / 완료 조건 / 실수 포인트` 존재
- [ ] 필요한 step에 `why it moved` 설명 블록 존재
- [ ] DH/MDH 혼동 가능 step에 주석 또는 강사용 노트 존재
- [ ] concept refs가 `concept-to-ui-map`와 연결됨
- [ ] PROJECT-STATUS.md 업데이트됨
- [ ] Unity 컴파일: 에러 0
- [ ] PlayMode 시각 확인 통과

## Output Template
```
[tutor-step-add 완료]
- 스텝: #{StepNumber} - {Description}
- 표시 행렬: {A_i / T_0n / EE pose}
- 관련 템플릿: {TemplateName}
- PROJECT-STATUS: 업데이트 완료
- PlayMode 검증: 통과
```
