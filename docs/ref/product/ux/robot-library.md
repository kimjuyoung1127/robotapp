# Robot Library

## Purpose
- 여러 로봇 모델을 선택하고 비교하는 진입 화면의 구조를 정의한다.

## Parent Doc
- [WIREFRAME](../../WIREFRAME.md)

## When To Read
- 다중 로봇 진입 흐름, 로봇 카드 정보, 필터 구조를 설계할 때

## Locked Decisions
- Robot Library는 다중 로봇 구조의 허브
- 사용자는 여기서 Guided Lesson 또는 Sandbox로 들어간다
- 로봇 카드는 난이도와 지원 모드를 반드시 표시한다
- 비교는 최대 2개 로봇까지 허용한다
- `6DOF`는 초기에는 `demo-first` 정책으로 다룬다

## Open Questions
- 강사용 추천 로봇 preset을 별도 섹션으로 둘지

## Downstream Sync
- `docs/ref/WIREFRAME.md`
- `docs/ref/product/robots/robot-model-library-spec.md`

## Last Updated
- 2026-03-11 (KST)

## Screen Contract
### `RL-01 Grid`
- 목적: 전체 로봇 목록을 한 번에 훑고 입문자/강사가 빠르게 고르도록 한다
- 기본 필터: `DOF`, `robot_type`, `difficulty`, `mode_support`
- 카드 CTA: `Start Guided Lesson`, `Open Sandbox`, `View Details`

### `RL-02 Detail Drawer`
- 목적: 선택한 로봇의 구조와 학습 적합성을 자세히 설명한다
- 필수 블록:
  - 로봇 설명
  - 자유도/구조
  - 지원 lesson
  - 입력 모드
  - 강사용 추천 포인트
  - `demo-first` 여부

### `RL-03 Compare Strip`
- 목적: 최대 2개 로봇의 구조, 난이도, lesson 적합성을 나란히 본다
- 비교 항목:
  - DOF
  - robot type
  - workspace intuition
  - Guided Lesson 지원
  - Sandbox 지원
  - Instructor 추천도

### `RL-04 Mode Routing`
- 목적: 사용자의 의도에 따라 바로 진입하게 한다
- CTA:
  - `Start Guided Lesson`
  - `Open Sandbox`
  - `Instructor Demo`

## Card Fields
- `robot_id`
- 로봇 이름
- 자유도
- 유형
- 난이도
- Guided Lesson 지원 여부
- Sandbox 지원 여부
- 강사용 추천 여부
- `supported_lessons`
- `input_modes`
- `visualization_level`
- `description`

## Decision Rules
- `2DOF`는 baseline lesson의 기본 진입점이다.
- `SCARA`는 산업 입문자 설명용 첫 확장이다.
- `3DOF`는 구조 비교용 교육 모델이다.
- `6DOF`는 초기에 시연/비교 중심이며 full interaction은 후속 단계다.
- Robot Library는 robot metadata를 추정하지 않고 문서화된 값만 사용한다.
- `RoboDK`/산업툴식 복잡한 파라미터 중심 브라우저는 배제하고, 입문자용 `difficulty`, `supported_lessons`, `recommended_for`를 전면에 둔다.
