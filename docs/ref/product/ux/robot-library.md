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
- showroom은 기본적으로 `가운데 hero + 좌우 context` 3 pod를 한 화면에 보여준다
- `CompareStrip`는 유지하지 않고, 선택은 카드/3D showroom 직접 클릭으로 단순화한다
- `6DOF`는 초기에는 `demo-first` 정책으로 다룬다

## Open Questions
- 강사용 추천 로봇 preset을 별도 섹션으로 둘지

## Downstream Sync
- `docs/ref/WIREFRAME.md`
- `docs/ref/product/robots/robot-model-library-spec.md`

## Last Updated
- 2026-03-13 (KST)

## Screen Contract
### `RL-01 Grid`
- 목적: 전체 로봇 목록을 한 번에 훑고 입문자/강사가 빠르게 고르도록 한다
- 기본 필터: `DOF`, `robot_type`, `difficulty`, `mode_support`
- 카드 CTA: `Start Guided Lesson`, `Open Sandbox`
- 카드 본문 또는 카드 surface 클릭 시 기본 진입 경로로 바로 이동한다

### `RL-02 Direct Practice Routing`
- 목적: 선택 직후 사용자를 바로 실습 화면으로 연결한다
- 규칙:
  - Guided Lesson 지원 로봇은 `Main`으로 즉시 진입한다
  - Guided Lesson이 없고 Sandbox만 지원하는 로봇은 `Sandbox`로 즉시 진입한다
  - 카드 클릭과 showroom 3D 로봇 클릭은 같은 기본 진입 규칙을 사용한다

### `RL-05 Showroom Viewport`
- 목적: 카드 그리드 위에서 실제 3D 로봇 비교를 먼저 보여준다
- 규칙:
  - 첫 페이지 기본 hero는 가운데 로봇이다
  - 좌우 화살표는 페이지 단위로 이동하되, 각 페이지의 hero를 안정적으로 복원한다
  - `showroomOutput`은 실제 viewport rect 기준으로 RenderTexture와 camera framing을 맞춘다
  - Game view와 Scene view 모두에서 로봇 크기가 과도하게 작아지지 않도록 viewport rect와 visible pod 수 기준으로 프레이밍한다
  - 3D 로봇 자체를 클릭하면 해당 로봇을 선택하고 기본 실습 경로로 즉시 이동한다

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
- `SCARA`는 산업 입문자 설명용 첫 확장이며, 현재 runtime baseline에서 실제 lesson/sandbox 진입을 지원한다.
- `3DOF`는 구조 비교용 교육 모델이다.
- `6DOF`는 초기에 시연/비교 중심이며 full interaction은 후속 단계다.
- showroom camera는 `Screen`이 아니라 실제 viewport rect와 visible pod 수를 기준으로 프레이밍한다.
- Robot Library는 비교 정보를 별도 strip으로 유지하기보다 카드 정보와 3D hero 선택을 통해 즉시 실습으로 연결한다.
- Robot Library는 robot metadata를 추정하지 않고 문서화된 값만 사용한다.
- `RoboDK`/산업툴식 복잡한 파라미터 중심 브라우저는 배제하고, 입문자용 `difficulty`, `supported_lessons`, `recommended_for`를 전면에 둔다.
- `Home / Continue Hub` 구현 이후에도 Robot Library는 `새 로봇/새 모드 탐색` 허브 역할을 유지한다.
