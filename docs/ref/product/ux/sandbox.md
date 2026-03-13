# Sandbox

## Purpose
- Guided Lesson 이후 자유 실습 공간의 범위와 동작을 정의한다.

## Parent Doc
- [WIREFRAME](../../WIREFRAME.md)

## When To Read
- 자유 조작, 저장/불러오기, 카메라 프리셋, 비교 실험 기능을 설계할 때

## Locked Decisions
- Sandbox는 Guided Lesson과 분리된 자유 실습 공간
- `zero/home/demo/reset`과 `snapshot lite`는 MVP 핵심 기능이다
- 로봇별 조작 UI는 동일 패턴을 재사용하되 DOF에 맞게 변형한다
- progress/settings는 기존 `PlayerPrefs`를 유지한다
- pose/history는 `local JSON store` 우선으로 저장한다
- 저장 구조는 후속 클라우드 확장을 위해 abstraction을 가진다
- pick-and-place는 1차에서 `foundation only` 범위로 다룬다
- `RoboDK`의 실습성은 흡수하되 `CoppeliaSim`식 범용 툴화는 배제한다
- `pose history`, `ghost trail`, `repeatability/constraint learning`은 MVP 이후 follow-up 가치로 유지한다

## Open Questions
- 자세 비교를 하나의 화면에서 할지 history 기반으로 할지

## Downstream Sync
- `docs/ref/WIREFRAME.md`
- `docs/ref/product/robots/robot-model-library-spec.md`

## Last Updated
- 2026-03-12 (KST)

## Screen Contract
### `SB-01 Free Manipulation`
- joint slider와 direct numeric input을 모두 제공한다
- 현재값, 이전값, delta, limit 상태를 함께 보여준다

### `SB-02 Numeric Joint Input`
- 목적: 소수 둘째/셋째 자리 단위의 정밀 입력
- 규칙:
  - degrees 기준 입력
  - invalid input은 즉시 거부
  - slider와 양방향 동기화

### `SB-03 Why It Moved`
- 목적: 조인트 변화가 링크/프레임/EE에 미친 영향을 설명
- 필수 정보:
  - joint 변화량
  - 영향 프레임
  - EE 변화
  - 쉬운 설명
  - 강사용 설명

### `SB-04 Snapshot Lite`
- 목적: 현재 자세를 저장하고 zero/home/demo/reset 사이를 빠르게 비교
- 지원 동작:
  - snapshot 저장
  - snapshot 불러오기
  - 현재 자세 vs 저장 자세 빠른 비교
  - zero/home/demo/reset
  - snapshot 이름 또는 짧은 note 저장

### `SB-05 Pose History & Replay`
- 목적: 반복성, 제한성, 재현 가능성을 학습
- 지원 동작:
  - 이전 시도 비교
  - sequence 다시보기

### `SB-06 Constraint Preview`
- 목적: workspace, joint limit, singularity intuition을 시각화
- 표시 요소:
  - limit warning
  - reachable / unreachable 상태
  - constraint feedback

### `SB-07 Pick Foundation`
- 목적: pick-and-place의 개념 뼈대를 먼저 이해
- 상태:
  - `home`
  - `pre_pick`
  - `pick`
  - `post_pick`
  - `pre_place`
  - `place`
  - `post_place`
- 제외:
  - full IK automation
  - advanced physics grasp stabilization
- 필수 설명:
  - `object_pose`
  - `desired_pose`
  - `grasp_pose`
  - `pre_grasp_approach`
  - `post_grasp_retreat`
  - `grasp_posture`

## Storage Contract
### `pose_snapshot`
- `snapshot_id`
- `robot_id`
- `context_mode`
- `joint_values_deg`
- `ee_pose`
- `created_at`
- `note`

### `motion_sequence`
- `sequence_id`
- `robot_id`
- `steps[]`
- `constraint_flags`
- `result_pose`

## Core Actions
- 자유 조작
- numeric joint input
- 저장/불러오기
- zero/home/demo/reset
- 자세 빠른 비교
- sequence 다시보기
- 리셋
- 카메라 프리셋
- 자세 비교
- constraint preview
- pick foundation 상태 전환
- object pose vs desired pose 비교
- exit clarity (`Home / Continue Hub` 또는 `Robot Library`로 복귀)
