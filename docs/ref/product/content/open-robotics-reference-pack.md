# Open Robotics Reference Pack

## Purpose
- 공개 로보틱스 자료를 KineTutor3D lesson, glossary, instructor notes에 연결하는 기준 팩을 정의한다.

## Parent Doc
- [PRD](../../PRD.md)

## When To Read
- 공개 자료를 concept-to-ui-map, Guided Lesson, 강사용 노트에 반영할 때

## Locked Decisions
- 공개 자료는 `adopt / exclude / fit` 기준으로만 반영한다
- 비공개 강의자료 원본을 대체하는 것이 아니라 파생 개념의 근거 팩으로 사용한다
- 긴 원문 복사 대신 concept/lesson/UI 적용 관점으로만 요약한다
- 이 reference pack은 공식 문서/공식 프로젝트만 우선 사용한다

## Open Questions
- 이후 robotics dynamics 전용 reference pack을 별도 분리할지

## Downstream Sync
- `docs/ref/product/content/concept-to-ui-map.md`
- `docs/ref/product/content/llm-teaching-strategy.md`
- `docs/ref/product/ux/guided-lesson.md`
- `docs/ref/product/content/frame-pose-teaching-notes.md`
- `docs/ref/product/content/pick-foundation-state-machine.md`
- `docs/ref/product/robots/robot-model-library-spec.md`

## Last Updated
- 2026-03-11 (KST)

## Reference Table
| source | concept coverage | beginner friendliness | best fit | adopt | exclude |
|---|---|---|---|---|---|
| [Modern Robotics](https://modernrobotics.northwestern.edu/nu-gm-book-resource/chapter-2-1-rigid-body-motion/) | frame, pose, transform, FK, numerical IK | 중간 | Guided Lesson bridge, Instructor Notes, IK hint | frame-to-frame 설명, pose를 transform 관계로 보는 관점, numerical IK의 `initial guess -> error reduction` framing | 긴 수학 유도 원문 복사, 앱을 교과서처럼 만드는 구성 |
| [MIT Manipulation](https://manipulation.csail.mit.edu/pick.html) | pick-and-place, pre-pick/pick/post-pick, target pose, state sequencing | 높음 | Guided Lesson, Pick Foundation, Instructor Notes | task-based sequencing, object pose vs desired pose 비교, pre-grasp/grasp/place narrative | full manipulation stack나 복잡한 시스템 전제를 그대로 가져오기 |
| [Robotics Toolbox for Python](https://petercorke.github.io/robotics-toolbox-python/intro.html) | manipulator models, DH/MDH/ETS, joint limits, robot model taxonomy | 중간 | Robot Library metadata, Instructor Notes, concept validation | robot model taxonomy, DH/MDH convention warning, joint limit metadata, model comparison | Python API나 external dependency 흐름을 그대로 옮기기 |
| [MoveIt 2 Pick and Place](https://moveit.picknik.ai/main/doc/examples/pick_place/pick_place_tutorial.html) | grasp pose, pre-grasp approach, post-grasp retreat, grasp posture | 중간 | Pick Foundation, future Sandbox | `grasp_pose`, `pre_grasp_approach`, `post_grasp_retreat`, `grasp_posture` 상태 정의 | full planning pipeline, ROS-heavy architecture, action/service 구조를 그대로 가져오기 |
| [Unity Robotics Hub](https://github.com/Unity-Technologies/Unity-Robotics-Hub) | URDF import, Unity robot sim structure, pick-and-place tutorial organization | 중간 | Unity-side import/sim reference, future robot onboarding | Unity robot import structure, scene organization, pick-and-place tutorial scaffolding | ROS dependency 흐름, external stack 전제를 현재 MVP에 강제하기 |
| [FAIRINO Official Docs](https://fairino-doc-en.readthedocs.io/latest/index.html) | FR5/FR series hardware spec, installation, DH, C# SDK, feedback protocol, command protocol | 중간 | Robot Library metadata, real-robot bridge, Instructor Notes | FR5 실기 제어용 source map, C# SDK motion/status API, drawings/DH downloads, installation/load constraints | 전체 사이트 미러링, ZIP/PDF 바이너리 커밋, firmware/port 동작을 현장 검증 없이 단정하기 |
| [roboticsbook](https://www.roboticsbook.org/) | introductory robotics, state, motion, perception context | 높음 | Glossary, Guided Lesson concepts | beginner wording, structured concept progression | perception/decision 파트를 현 단계 범위 이상으로 확대하기 |
| [Pybotics](https://pybotics.readthedocs.io/) | DH-based modeling, calibration-adjacent concepts | 중간 | Sandbox notes, metadata concepts | robot model structuring, DH-centric thinking | calibration tooling/implementation을 현재 범위에 직접 넣기 |
| [general_robotics_toolbox](https://general-robotics-toolbox.readthedocs.io/en/latest/readme.html) | transform utilities, robot representation | 중간 | Instructor Notes, DH/MDH caution | compact robotics representation ideas, transform vocabulary | library-specific usage 문법을 lesson 본문에 넣기 |

## Evaluation Rule
- `adopt`: KineTutor3D가 가져올 개념, 설명 관점, UI 아이디어
- `exclude`: 직접 복사하지 않거나 현재 제품 범위 밖인 부분
- `fit`: Guided Lesson / Glossary / Instructor Notes / Sandbox 중 어디에 쓰는지

## Immediate Additions For KineTutor3D
- `Modern Robotics` -> `frame-pose-teaching-notes.md`
  - pose를 좌표 하나가 아니라 `frame 관계`로 설명
  - numerical IK는 유도식보다 `오차를 줄이는 반복`으로 먼저 설명
- `MIT Manipulation` -> `pick-foundation-state-machine.md`
  - `pre_pick -> pick -> post_pick -> pre_place -> place -> post_place` 상태를 교육용으로 단순화
  - object pose와 desired pose 비교 패널 도입
- `Robotics Toolbox for Python` -> `robot-model-library-spec.md`
  - `DH/MDH`, `joint_limits`, `home_pose`, `demo_pose`, `supported_lessons` 메타데이터 강화
- `MoveIt 2` -> `sandbox.md`
  - `grasp_pose`, `pre_grasp_approach`, `post_grasp_retreat`, `grasp_posture`를 pick foundation 상태 정의에 반영
- `Unity Robotics Hub` -> future implementation notes
  - URDF-ready import slot, Unity robot scene organization, pick tutorial scaffold를 런타임 고도화 참고로만 사용
- `FAIRINO Official Docs` -> `fairino-fr5-integration-reference.md`
  - FR5 실기 제어용 hardware/SDK/protocol source map을 정리
  - Unity UI -> validation -> simulation -> live adapter 구조와 8083/20004 구분 메모를 남김
