# Frame Pose Teaching Notes

## Purpose
- frame, pose, transform 개념을 초보자와 강사 모두에게 설명 가능한 쉬운 언어로 정리한다.

## Parent Doc
- [PRD](../../PRD.md)

## When To Read
- Guided Lesson에서 Lesson 0~3을 Core Track의 frame/DH/matrix 설명으로 연결할 때

## Locked Decisions
- pose는 단순 좌표값이 아니라 `한 프레임이 다른 프레임에 대해 어떻게 놓이는가`로 설명한다
- frame 설명은 수식보다 `축`, `기준점`, `누구 기준인가` 질문으로 시작한다
- beginner track에서는 matrix 자체보다 `프레임 관계`를 먼저 보여준다

## Open Questions
- 이후 2D 미니맵을 frame teaching 전용으로 추가할지

## Downstream Sync
- `docs/ref/product/content/concept-to-ui-map.md`
- `docs/ref/product/ux/guided-lesson.md`
- `docs/ref/tutor-step-plan.md`

## Last Updated
- 2026-03-11 (KST)

## Teaching Notes
- `Frame`
  - 로봇의 각 링크와 끝점은 자기만의 기준점과 축을 가진다.
  - 질문은 `지금 누구 기준으로 보고 있나?`로 시작한다.
- `Pose`
  - pose는 `위치 + 방향`을 합친 것이고, 더 정확히는 `한 프레임이 다른 프레임에 대해 어디에 있고 어떻게 회전했는가`다.
- `Transform`
  - transform은 한 프레임에서 다른 프레임으로 설명을 옮기는 규칙이다.
  - beginner lesson에서는 `이 축에서 저 축으로 설명을 바꾼다`는 말로 먼저 소개한다.

## UI Suggestions
- `Frame Toggle`
  - world / joint / end-effector 기준을 바꿔 보게 한다
- `Pose Delta Card`
  - 현재 pose와 목표 pose의 차이를 단순 언어로 요약한다
- `Who Sees It?`
  - 같은 점도 frame이 바뀌면 숫자가 달라진다는 점을 보여준다
