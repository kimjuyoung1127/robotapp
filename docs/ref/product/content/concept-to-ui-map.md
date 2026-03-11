# Concept To UI Map

## Purpose
- 파생 개념을 lesson과 UI 위치에 매핑한다.

## Parent Doc
- [PRD](../../PRD.md)

## When To Read
- 비공개 강의자료를 Guided Lesson, Glossary, Instructor UI로 변환할 때

## Locked Decisions
- 원본 PNG 메타데이터는 기록하지 않는다
- 개념 단위로만 저장한다
- lesson 연결과 UI 타입을 반드시 명시한다
- `reference_family`는 공개 자료 계열만 허용한다
- `prerequisite_concepts`와 `visualization_mode`를 함께 기록한다

## Open Questions
- concept 우선순위를 lesson 난이도와 같이 관리할지

## Downstream Sync
- `docs/ref/product/content/derived-course-content-policy.md`
- `docs/ref/product/ux/guided-lesson.md`
- `docs/ref/product/content/open-robotics-reference-pack.md`

## Last Updated
- 2026-03-11 (KST)

## Map Table
| concept_id | 개념명 | 쉬운 설명 | 연결 lesson | UI 위치 | UI 타입 | 우선순위 | 강사용 보조 설명 | prerequisite_concepts | reference_family | visualization_mode |
|---|---|---|---|---|---|---|---|---|---|---|
| `concept_frame_origin` | Frame Origin | 각 링크와 관절은 자기 기준점과 축을 가진다 | Guided Lesson Frame Basics | Left Panel | Concept Card | P0 | 예 | - | `modern-robotics` | Axis Overlay |
| `concept_pose_as_frame_relation` | Pose As Frame Relation | 자세는 점 하나가 아니라 한 프레임이 다른 프레임에 대해 어디에 있고 어떻게 놓였는지를 뜻한다 | Guided Lesson Frame Basics | Right Panel | Plain Language Panel | P0 | 예 | `concept_frame_origin` | `modern-robotics` | Frame Relation |
| `concept_rotation_arc` | Rotation Arc | 관절 하나가 회전하면 끝점은 원호처럼 움직인다 | Lesson 1 Rotation Arc | Center Viewport | Trail Overlay | P0 | 아니오 | - | `roboticsbook` | Arc Trail |
| `concept_end_effector_path` | End-Effector Path | 끝점은 관절이 바뀔 때마다 공간에서 경로를 만든다 | Lesson 0 Tip Motion | Center Viewport | Path Trace | P0 | 아니오 | `concept_rotation_arc` | `mit-manipulation` | Tip Trail |
| `concept_joint_to_tip_relationship` | Joint-To-Tip Relationship | 관절을 움직이면 링크만이 아니라 끝점도 함께 따라 움직인다 | Lesson 0 Tip Motion | Bottom Rail | Link Highlight | P0 | 예 | - | `roboticsbook` | Highlight Link |
| `concept_reach_vs_not_reach` | Reach vs Not Reach | 어떤 목표점은 닿고 어떤 목표점은 닿지 않는다 | Lesson 3 Goal Matching | Center Viewport | Reach Marker | P0 | 예 | `concept_joint_to_tip_relationship` | `mit-manipulation` | Reach Badge |
| `concept_goal_matching` | Goal Matching | 목표점과 현재 끝점의 차이를 줄이는 것이 로봇 제어의 시작이다 | Lesson 3 Goal Matching | Right Panel | Goal Card | P0 | 예 | `concept_end_effector_path` | `mit-manipulation` | Target Marker |
| `concept_inverse_thinking` | Inverse Thinking | 끝점을 먼저 정하고 거꾸로 관절값을 생각해야 하는 문제가 있다 | Lesson 3 Inverse Thinking | Right Panel | Plain Language Panel | P0 | 예 | `concept_goal_matching` | `modern-robotics` | Goal-to-Joint |
| `concept_target_pose_delta` | Target Pose Delta | 현재 자세와 목표 자세의 차이를 보면 무엇을 더 움직여야 할지 감을 잡을 수 있다 | Sandbox Target Compare | Right Panel | Compare Card | P1 | 예 | `concept_goal_matching` | `mit-manipulation` | Pose Delta |
| `concept_homogeneous_transform` | Homogeneous Transform | 위치와 회전을 한 행렬로 묶어 다루는 방식 | Guided Lesson FK Basics | Right Panel | Explanation Panel | P0 | 예 | `concept_frame_origin` | `modern-robotics` | Matrix + Frame |
| `concept_dh_params` | DH 파라미터 | 링크와 좌표계 관계를 4개 값으로 정리하는 방식 | Guided Lesson FK Basics | Left Panel | Concept Card | P0 | 예 | `concept_frame_origin` | `roboticsbook` | Parameter Card |
| `concept_dh_vs_mdh` | DH vs MDH | 교재마다 프레임을 붙이는 방식이 달라 같은 표라도 결과가 달라질 수 있다 | Instructor Notes FK | Instructor Notes | Warning Note | P0 | 예 | `concept_dh_params` | `general-robotics-toolbox` | Toggle Illustration |
| `concept_fk_chain` | Forward Kinematics | 각 링크 변환을 순서대로 곱해 끝점 자세를 구하는 방식 | Guided Lesson FK Basics | Right Panel | Explanation Panel | P0 | 예 | `concept_homogeneous_transform` | `modern-robotics` | Chain Build |
| `concept_ai_matrix` | Link Transform A_i | 한 링크에서 다음 링크로 넘어가는 단일 변환 블록 | Guided Lesson Matrix View | Right Panel | Matrix Card | P0 | 예 | `concept_dh_params` | `robotics-toolbox-python` | Matrix Focus |
| `concept_pose_extract` | Pose Extraction | 최종 행렬에서 위치와 방향을 읽어내는 과정 | Guided Lesson Pose Basics | Right Panel | Pose Card | P0 | 예 | `concept_fk_chain` | `roboticsbook` | Pose Badge |
| `concept_joint_variable` | Joint Variable | 회전 관절은 각도, 직선 관절은 거리처럼 각 관절이 바꾸는 값이 다르다 | Guided Lesson Joint Input | Bottom Rail | Input Hint | P0 | 예 | - | `modern-robotics` | Highlight Link |
| `concept_joint_limits` | Joint Limits | 로봇은 무한히 움직이지 않고 각 관절마다 허용 범위가 있다 | Sandbox Limits | Bottom Rail | Constraint Badge | P0 | 예 | `concept_joint_variable` | `robotics-toolbox-python` | Limit Arc |
| `concept_robot_convention` | Robot Convention | 로봇 모델마다 DH, MDH, URDF처럼 표현 규칙이 다를 수 있어 같은 로봇도 설명 방식이 달라진다 | Robot Library Detail | Detail Drawer | Metadata Badge | P1 | 예 | `concept_dh_vs_mdh` | `robotics-toolbox-python` | Convention Badge |
| `concept_workspace` | Workspace | 끝점이 도달할 수 있는 공간 전체를 뜻한다 | Sandbox Workspace | Center Viewport | Workspace Overlay | P1 | 예 | `concept_joint_limits` | `roboticsbook` | Reach Volume |
| `concept_robot_mobility` | Mobility | 로봇이 실제로 움직일 수 있는 자유도를 이해하는 개념 | Robot Library Intro | Glossary | Glossary Card | P1 | 아니오 | - | `roboticsbook` | Simple Diagram |
| `concept_repeatability` | Repeatability | 같은 입력을 반복했을 때 비슷한 자세로 돌아오는 성질 | Sandbox Replay | History Panel | Replay Card | P1 | 예 | `concept_joint_variable` | `mit-manipulation` | Snapshot Compare |
| `concept_constraint_feedback` | Constraint Feedback | 왜 더 못 움직이는지 또는 왜 경고가 뜨는지를 쉬운 언어로 바꾸는 설명 | Sandbox Constraint View | Right Panel | Feedback Panel | P1 | 예 | `concept_joint_limits` | `mit-manipulation` | Warning Overlay |
| `concept_inverse_kinematics` | Inverse Kinematics | 원하는 끝점 자세를 만들기 위해 어떤 joint 값이 필요한지 찾는 과정 | Sandbox IK Intro | Right Panel | Explanation Panel | P1 | 예 | `concept_fk_chain` | `modern-robotics` | Goal-to-Joint |
| `concept_multiple_solutions` | Multiple IK Solutions | 같은 끝점 자세라도 joint 조합이 여러 개일 수 있다 | Sandbox IK Intro | History Panel | Compare Card | P1 | 예 | `concept_inverse_kinematics` | `robotics-toolbox-python` | Compare Paths |
| `concept_singularity` | Singularity Intuition | 특정 자세에서는 로봇이 한 방향으로 아주 민감하거나 거의 못 움직이게 된다 | Sandbox Constraint View | Center Viewport | Warning Overlay | P1 | 예 | `concept_joint_limits` | `modern-robotics` | Axis Alignment |
| `concept_trajectory_legibility` | Trajectory Legibility | 경로를 보면 로봇이 무엇을 하려는지 짐작할 수 있는 정도 | Sandbox Replay | Center Viewport | Ghost Trail | P2 | 예 | `concept_repeatability` | `mit-manipulation` | Trail Line |
| `concept_ghost_trail` | Ghost Trail | 예전 자세를 반투명하게 남겨 움직임의 흔적을 보는 방법 | Sandbox Replay | Center Viewport | Ghost Pose | P2 | 아니오 | `concept_trajectory_legibility` | `mit-manipulation` | Ghost Mesh |
| `concept_dynamics_intuition` | Dynamics Intuition | 같은 자세라도 무게와 관성 때문에 움직임 느낌이 달라질 수 있다는 직관 | Instructor Notes Dynamics | Instructor Notes | Teaching Note | P2 | 예 | `concept_joint_variable` | `roboticsbook` | Force Arrow |
| `concept_pick_foundation` | Pick Foundation | 집기 동작은 접근, 잡기, 후퇴, 놓기 같은 단계로 나눠서 이해할 수 있다 | Sandbox Pick Foundation | Right Panel | State Card | P1 | 예 | `concept_workspace` | `mit-manipulation` | State Flow |
| `concept_pregrasp_pose` | Pre-Grasp Pose | 물체를 바로 잡기 전에 안전하게 접근하는 준비 자세 | Sandbox Pick Foundation | Center Viewport | Goal Marker | P1 | 예 | `concept_pick_foundation` | `mit-manipulation` | Target Marker |
| `concept_pregrasp_approach` | Pre-Grasp Approach | 물체를 향해 들어가는 방향과 거리를 미리 정하면 집기 동작이 더 안정적이다 | Sandbox Pick Foundation | Right Panel | State Card | P1 | 예 | `concept_pregrasp_pose` | `moveit2` | Approach Arrow |
| `concept_grasp_pose` | Grasp Pose | 실제로 물체를 잡는 순간의 끝점 자세를 뜻한다 | Sandbox Pick Foundation | Center Viewport | Goal Marker | P1 | 예 | `concept_pregrasp_pose` | `moveit2` | Grasp Marker |
| `concept_grasp_posture` | Grasp Posture | 잡을 때와 놓을 때 그리퍼 상태가 달라진다 | Sandbox Pick Foundation | Bottom Rail | Gripper Hint | P1 | 예 | `concept_grasp_pose` | `moveit2` | Open Close Badge |
| `concept_postgrasp_retreat` | Post-Grasp Retreat | 물체를 잡은 뒤 바로 빠져나오는 안전한 후퇴 단계가 필요하다 | Sandbox Pick Foundation | Right Panel | State Card | P1 | 예 | `concept_grasp_pose` | `moveit2` | Retreat Arrow |
