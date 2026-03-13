# Guided Lesson

## Purpose
- 핵심 학습 화면의 레이아웃, 패널 책임, lesson 데이터 요구사항을 정의한다.

## Parent Doc
- [WIREFRAME](../../WIREFRAME.md)

## When To Read
- Guided Lesson UX, step flow, AI 설명 방식, lesson 콘텐츠 구조를 설계할 때

## Locked Decisions
- Guided Lesson이 제품의 중심 경험
- 4영역 레이아웃을 유지
- 각 step은 `학습목표 / 핵심 행동 / 완료 조건 / 실수 포인트`를 가진다
- `theta`는 DH 테이블에서 계속 read-only다
- joint 직접 입력은 별도 `Joint Control Rail`에서만 허용한다
- joint 입력은 `slider + numeric input` 양방향 동기화로 간다
- 표시 정밀도는 소수 셋째 자리까지 허용하고 내부 계산은 `double radians`를 유지한다
- lesson 흐름은 `scaffolded`, `student-paced`, `instructor-led`를 동시에 지원한다
- `Why It Moved`는 보조 설명이 아니라 core panel이다
- `Lesson 0~3`은 기존 Step 1~8 앞에 붙는 `Pre-Kinematics` 입문 트랙이다
- `Lesson 0~3`에서는 숫자보다 움직임, 공식보다 plain language를 우선한다
- 온보딩 skip은 late-step direct jump가 아니라 `Home / Continue Hub`에서의 경로 선택으로 연결한다

## Math Readiness Entry Flow

### 진입 경로
```text
Onboarding "처음이에요" 카드 → MathReadinessTrack 설정 → MathReadiness.unity
Home → "수학 기초 워밍업" 카드 → MathReadiness.unity → MathReadinessLessonFactory M0~M3 로드
MathReadiness M3 완료 → Main.unity → PreKinematics Lesson L0~L3
```

### MathReadiness Left Panel 전략
- `leftContent = LeftPanelContent.Hidden` — Math Readiness 모드에서는 기존 Left Panel(공식/용어)을 숨김
- 대신 `MathReadinessPanel`이 중앙~좌측 영역에서 조작 지시 + 확인 질문 + 피드백을 전담
- 핵심 원칙: 공식/행렬/DH 테이블 완전 비노출 (`showFormula=false`, `showMatrices=false`, `showDHTable=false`)
- 시각 피드백 우선: trail, joint-highlight, why-it-moved, angle reference marker는 활성

### MathReadiness 화면 계약
- Top Bar는 `KineTutor3D + 수학 기초 워밍업 · 현재 단계`만 강조하고, `Onboarding / Main` 같은 내부 상태성 탭은 학습 중에는 전면 노출하지 않는다.
- Left Panel은 `현재 학습 / 현재 행동(조작 또는 확인 질문) / 도움말` 3블록 구조를 유지한다.
- 첫 화면에서는 `현재 학습 + 조작 지시`만 보이고, 목표 각도에 도달한 뒤에만 `확인 질문`을 노출한다.
- `현재 학습` 블록은 `큰 제목 + 한 줄 목표 + 짧은 설명`만 유지하고, 장문의 rationale은 기본 화면에서 뺀다.
- 도움말 블록은 `많이 헷갈리는 포인트 + 교사/보호자 멘트`만 담는 보조 영역으로 유지한다.
- Right Panel은 Math Readiness 모드에서 숨기고, 설명 텍스트와 게이트 진행은 좌측/하단으로만 모은다.
- Center Viewport는 로봇을 보여주는 공간으로 두고, 긴 오버레이 텍스트 대신 0°/90°/180° 기준선과 현재 조작 결과를 중심으로 보여준다.
- Bottom Bar는 `관절 조작 + Prev / Next / Skip`가 한 컨트롤 바로 묶여 보여야 하며, `Next`를 가장 강하게, `Skip`은 약하게 표현한다.

### MathReadiness 패널 구성
| 요소 | 역할 |
|---|---|
| `manipulationInstruction` | 먼저 해볼 조작 지시문 |
| `targetBadge` | 목표 각도/조인트 배지 |
| `readinessQuestions[]` | 본 질문 (정답 판정 + 오답 피드백) |
| `angleReferenceMarker` | 0°/90°/180° 기준선 + 라벨 |
| `successToast` | Gate 충족 시 토스트 |
| `rationale` | 왜 이 lesson이 필요한지 |
| `commonMistake` | 흔한 실수 안내 |
| `coachHint` | 강사용 가이드 |

### Track 전환
- M3 완료 → `PreKinematicsTrack` (Lesson L0~L3) 자동 전환
- L3 완료 → `CoreKinematicsTrack` (Step S1~S8) 수동 또는 자동 전환

## Open Questions
- AI 설명을 텍스트 요약 중심으로 둘지 대화형 힌트까지 확장할지

## Downstream Sync
- `docs/ref/WIREFRAME.md`
- `docs/ref/USER-FLOW.md`
- `docs/ref/tutor-step-plan.md`

## Last Updated
- 2026-03-12 (KST)

## Layout Contract
- Top Bar: 현재 로봇, lesson, 단계, glossary, 강사모드
- Left Panel: 개념 설명, 목표, 공식/용어
- Center Viewport: 3D 로봇, 프레임, 카메라 프리셋
- Right Panel: 행렬, 프레임 정보, 피드백
- Bottom Bar: 슬라이더, step navigation, 힌트, 리셋

### MathReadiness Layout Override
- Top Bar: `KineTutor3D`와 `수학 기초 워밍업 · N/4`를 2줄 정보 구조로 유지
- Left Panel: `현재 학습`, `조작/확인 질문`, `도움말` 3블록
- Center Viewport: 로봇 + 0°/90°/180° 기준선 + 최소한의 시각 피드백
- Right Panel: 숨김
- Bottom Bar: 조인트 슬라이더와 진행 버튼을 한 줄 컨트롤 바로 정리

## Screen Contract
### `GL-01 Lesson Shell`
- `screen_id`: `guided_lesson_shell`
- `panel_id`: `top_bar`, `left_concept_panel`, `center_viewport`, `right_result_panel`, `bottom_joint_rail`
- 목적: 현재 lesson과 robot, step 상태를 항상 한 화면에서 유지
- 기본 CTA: `Back`, `Glossary`, `Instructor Mode`, `Open Sandbox`

### `GL-02 Step Intro`
- `screen_id`: `step_intro`
- `interaction_type`: `read`, `hover`, `tap`
- 내용: `학습목표`, `핵심 행동`, `완료 조건`, `실수 포인트`
- `step_exit_condition`: 현재 step gate 해제 또는 instructor override

### `GL-02B Beginner Mode`
- `beginner_mode`: `true`
- 목적: 완전 초보자가 수학 없이도 움직임 감각을 만든다
- 계약:
  - `show_formula = false`
  - `show_matrix = minimal`
  - `show_axis_overlay = true`
  - `show_plain_language = high`
  - `show_2d_feedback = preferred`

### `GL-03 Joint Input`
- `screen_id`: `joint_input`
- `interaction_type`: `slider`, `numeric_input`, `tap_focus`
- 규칙:
  - Math Readiness에서는 `조작 먼저 -> 확인 질문` 순서를 우선한다
  - 목표 각도에 도달하기 전에는 확인 질문 선택지를 숨긴다
  - joint별 현재값, 이전값, delta를 함께 보여준다
  - numeric input은 degrees 기준이며 소수 셋째 자리까지 허용한다
  - 허용하지 않는 값(NaN/Infinity, limit 초과)은 즉시 거부하고 기존 값을 유지한다
  - slider를 잡거나 입력칸을 포커스하면 해당 링크/축/프레임을 강조한다
  - 4DOF 이상 robot은 tablet 기준에서 rail 접기/스크롤/분할 구성을 허용한다

### `GL-04 Why It Moved`
- `screen_id`: `why_it_moved`
- `feedback_type`: `joint_delta`, `frame_change`, `ee_summary`, `plain_language`
- 필수 항목:
  - 입력 joint
  - 이전값 -> 현재값
  - 변화량
  - 영향 받은 링크/프레임
  - EE 위치/방향 변화 요약
  - 쉬운 설명 1~2문장
  - 강사용 설명 1단락

### `GL-05 Matrix & Frame View`
- `screen_id`: `matrix_frame_view`
- `panel_id`: `ai_matrix`, `cumulative_matrix`, `pose_extract`, `frame_list`
- 목적: A_i, T_0n, pose, frame 이동을 현재 step과 연결해서 보여준다
- 규칙: `A1/A2/T02`는 baseline 유지, future robot에서는 robot metadata에 따라 동적 확장

### `GL-06 Save & Replay Entry`
- `screen_id`: `save_replay_entry`
- `interaction_type`: `save_snapshot`, `compare_snapshot`, `open_sandbox`
- 목적: Guided Lesson에서 학습 중인 상태를 Sandbox replay로 연결
- `step_exit_condition`: snapshot 저장 또는 Sandbox 진입

### `GL-07 Continue Hub Return`
- 목적: Guided Lesson이 종료되거나 이탈될 때 `Home / Continue Hub`로 안전하게 복귀시킨다
- 규칙:
  - 온보딩 skip은 late-step direct jump보다 hub 복귀를 우선한다
  - `Back`은 lesson shell 밖으로 나갈 때 Home 허브를 기본 착지점으로 본다
  - continue context는 `robot_id + mode + track + step` 기준을 재사용한다

## Interaction Rules
- `DHTable`은 `d/a/alpha`만 편집 가능하고 `theta`는 읽기 전용이다.
- numeric joint input은 `Bottom Joint Rail` 또는 `Right Result Panel`의 dedicated control만 사용한다.
- 각 joint 입력은 `current`, `previous`, `delta`, `limit_state`를 보여준다.
- touch-first 기준으로 입력칸과 slider 손잡이는 태블릿에서도 충분한 크기를 유지한다.
- tablet에서 4DOF 이상 joint rail은 3D viewport를 침범하지 않도록 접기/스크롤/분할 layout을 우선한다.
- Guided Lesson은 `RoboX`/`UR Academy`류의 scaffolded progression을 흡수하되, vendor-specific workflow는 포함하지 않는다.
- `Lesson 0~3`에서는 행렬 패널을 숨기거나 최소화하고, trail / target marker / link highlight / why-it-moved를 우선 노출한다.
- `Lesson 0~3`에서는 `sin/cos`, 삼각형 유도, Jacobian, DH 표를 핵심 화면으로 요구하지 않는다.

## Feedback Contract
- `feedback_type`: `success`, `limit_warning`, `invalid_input`, `concept_hint`, `instructor_note`
- invalid input 예:
  - NaN/Infinity
  - degrees 범위 초과
  - robot DOF보다 큰 joint index
- limit warning은 단순 오류가 아니라 `왜 더 못 가는지`를 쉬운 설명으로 바꿔야 한다.

## Lesson Data Needs
- `lesson_id`
- `lesson_level`
- `beginner_mode`
- `robot_support`
- `step_count`
- `objective`
- `interaction_gate`
- `concept_refs`
- `screen_id`
- `panel_id`
- `interaction_type`
- `feedback_type`
- `step_exit_condition`
- `show_formula`
- `show_matrix`
- `show_plain_language`
