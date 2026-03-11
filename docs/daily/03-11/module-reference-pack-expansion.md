# Module Log - Reference Pack Expansion

Date: 2026-03-11 (KST)

## What Changed
- `open-robotics-reference-pack.md`에 Modern Robotics, MIT Manipulation, Robotics Toolbox for Python, MoveIt 2, Unity Robotics Hub 기준의 적용 포인트를 구체화했다.
- `concept-to-ui-map.md`에 frame/pose bridge, target pose delta, convention badge, pick foundation 관련 개념을 추가했다.
- `robot-model-library-spec.md`에 `convention`, `joint_limits`, `home_pose`, `demo_pose`, `pick_foundation_supported` 등 metadata 필드를 확장했다.
- `sandbox.md`의 pick foundation 상태를 `pre_pick -> pick -> post_pick -> pre_place -> place -> post_place`로 정리했다.
- `frame-pose-teaching-notes.md`, `pick-foundation-state-machine.md`를 새로 추가했다.

## Why
- 공개 공식 자료에서 바로 흡수 가능한 내용은 `설명 방식`, `상태 구조`, `로봇 메타데이터`, `pick foundation` 쪽이어서 문서 기준선을 먼저 강화했다.
- 임베딩은 나중에 필요 시 도입하고, 현재는 deterministic runtime + local docs + reference pack 구조를 더 단단하게 만드는 쪽이 맞다고 판단했다.
