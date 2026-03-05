---
name: student-friendly-ux
description: "학생 친화 UX 통합 워크플로. 점진적 공개, 온보딩, 툴팁, 용어 사전, 학습 게이트를 설계/구현/검증할 때 사용"
---

## Trigger
아래 키워드가 포함된 요청에서 사용:
- `점진적 공개`
- `온보딩`
- `툴팁`
- `용어 사전`
- `학습 게이트`
- `학생 친화 UX`

## Input Context
- 대상 Step 범위 (기본: 1~8)
- 게이트 정책 (`Hard gate + Skip` 기본)
- Reduced Motion 적용 여부

## Read First
1. `references/step-visibility-matrix.md`
2. `references/gate-catalog.md`
3. `references/glossary-ko.md`
4. `docs/ref/tutor-step-plan.md`
5. `docs/ref/USER-FLOW.md`

## Dependencies
- `tutor-step-add`
- `scene-scaffold`
- `unity-official-docs` (직렬화/컴파일/asmdef 영향 검토 시)

## Do
1. `TutorStepConfig`를 기준으로 Step별 패널 상태를 단일 소스로 고정한다.
2. `InteractionGateController`로 Next 잠금/해제를 강제하고 Skip 버튼을 제공한다.
3. `TooltipSystem` + `TooltipTriggerUI/3D`로 UI/3D 안내를 통일한다.
4. `OnboardingManager` + `SpotlightOverlay`로 첫 실행 시퀀스를 구현한다.
5. `GlossaryPanelController`에서 쉬운 설명/수학 설명 모드를 제공한다.
6. `StepProgressSaver`로 방문/진행/설정 상태를 저장한다.

## Do Not
1. 기구학 계산 로직을 UX 컴포넌트로 이동하지 않는다.
2. Step 상태를 하드코딩 분기로 중복 관리하지 않는다.
3. 게이트 판정을 UI 버튼 이벤트에서 직접 구현하지 않는다.

## Validation
- [ ] Step 1~8 패널 가시성 매트릭스 일치
- [ ] 온보딩 첫 실행/재방문 분기 동작
- [ ] Next 잠금/해제 + Skip 동작
- [ ] 툴팁(UI/3D) 위치 및 표시 안정성
- [ ] 용어 사전 쉬운/수학 모드 전환

## Output Template
```
[student-friendly-ux 완료]
- 적용 범위: {Step 범위}
- 게이트 정책: Hard gate + Skip
- 구현 컴포넌트: {목록}
- 검증 결과: {핵심 시나리오 통과 여부}
- 잔여 리스크: {있으면 기재}
```
