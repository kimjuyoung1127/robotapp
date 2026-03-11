---
name: robotics-reference-to-lesson
description: "공개 로보틱스 자료를 Guided Lesson, Glossary, Instructor Notes용 파생 설명으로 변환할 때 사용하는 스킬"
---

## Trigger
아래 요청에서 사용:
- `로보틱스 자료를 lesson에 반영`
- `공개 레퍼런스를 수업 UI로 정리`
- `concept-to-ui-map 확장`
- `강사용 설명/글로서리 작성`

## Input Context
- 대상 개념 범위
- 대상 화면 (`Guided Lesson`, `Glossary`, `Instructor Notes`, `Sandbox`)
- 대상 학습자 (`beginner`, `instructor`)
- 공개 참고자료 목록

## Read First
1. `docs/ref/product/content/derived-course-content-policy.md`
2. `docs/ref/product/content/concept-to-ui-map.md`
3. `docs/ref/product/content/open-robotics-reference-pack.md`
4. `docs/ref/product/content/lesson-framework.md`
5. `docs/ref/product/ux/guided-lesson.md`

## Do
1. 공개 자료를 `adopt / exclude / fit` 기준으로 평가한다.
2. 개념을 `쉬운 설명`, `강사용 설명`, `lesson 연결`, `UI 위치`로 분해한다.
3. `DH vs MDH` 혼동 가능성이 있으면 명시적으로 경고한다.
4. `pose history`, `ghost trail`, `trajectory legibility`는 교육 목적을 설명과 시각화 요구사항으로만 변환한다.
5. `pick-and-place`는 `foundation only` 범위로 유지한다.
6. 결과를 `concept-to-ui-map`, glossary card, instructor note에 연결한다.

## Do Not
1. 비공개 강의 PNG 원본, 파일명, 로컬 경로를 기록하지 않는다.
2. 공개 자료 문장을 길게 복사하지 않는다.
3. 계산 엔진의 진실값을 공개 자료 설명으로 대체하지 않는다.

## Validation
- [ ] 비공개 원본 자료 비노출 정책 준수
- [ ] concept가 lesson/UI/instructor 용도로 분해됨
- [ ] adopt/exclude/fit 판단이 명시됨
- [ ] DH/MDH 혼동 가능성 처리됨
- [ ] pick-and-place가 foundation only 범위 유지

## Output Template
```
[robotics-reference-to-lesson 완료]
- 대상 개념: {ConceptGroup}
- 반영 화면: {Guided Lesson / Glossary / Instructor Notes / Sandbox}
- adopt: {가져올 요소}
- exclude: {직접 복사하지 않을 요소}
- fit: {KineTutor3D 적용 방식}
- 비공개 원본 노출: 없음
```
