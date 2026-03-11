# Pick Foundation State Machine

## Purpose
- pick-and-place를 고급 planning 없이도 이해할 수 있는 교육용 상태 흐름으로 정의한다.

## Parent Doc
- [PRD](../../PRD.md)

## When To Read
- Sandbox의 pick foundation, target pose 비교, 강사용 demo flow를 설계할 때

## Locked Decisions
- 1차 pick foundation은 planning이 아니라 `상태 이해`가 목적이다
- 상태는 `home -> pre_pick -> pick -> post_pick -> pre_place -> place -> post_place`로 단순화한다
- `object_pose`, `desired_pose`, `grasp_pose`, `pre_grasp_approach`, `post_grasp_retreat`, `grasp_posture`를 반드시 노출한다

## Open Questions
- 이후 object grasp 성공 조건을 단순 거리 기반으로 둘지 trigger 기반으로 둘지

## Downstream Sync
- `docs/ref/product/ux/sandbox.md`
- `docs/ref/product/content/concept-to-ui-map.md`
- `docs/ref/product/roadmap/milestone-backlog.md`

## Last Updated
- 2026-03-11 (KST)

## State Flow
1. `home`
2. `pre_pick`
3. `pick`
4. `post_pick`
5. `pre_place`
6. `place`
7. `post_place`

## Teaching Points
- `object_pose`
  - 물체가 현재 어디에 있는가
- `desired_pose`
  - 끝점이 어디로 가야 하는가
- `grasp_pose`
  - 실제로 잡는 순간의 끝점 자세
- `pre_grasp_approach`
  - 잡기 전에 어떤 방향으로 접근할지
- `post_grasp_retreat`
  - 잡은 뒤 어디로 빠져나올지
- `grasp_posture`
  - 그리퍼가 열려 있어야 하는지, 닫혀 있어야 하는지

## UI Suggestions
- 상태 타임라인
- object pose vs desired pose 비교 카드
- current state / next state 배지
- 강사용 state jump preset
