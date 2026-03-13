# KineTutor3D Tutor Step Plan (Student-Friendly UX)

Version: 1.4.0
Last Updated: 2026-03-12 (KST)
Implementation Status: Phase 5 Complete (L0~L3 + S1~S8 all implemented)

## 핵심 원칙
1. 한 번에 하나만 보여준다.
2. 포커스 존으로 시선을 유도한다.
3. 마이크로 게이트로 학습 행동을 유도한다.
4. 한국어 툴팁/용어사전으로 용어 장벽을 낮춘다.
5. Phase 3 1차 행렬 표시는 `A1/A2/T02` 고정으로 운영한다.
6. 튜터 스텝은 `Guided Lesson` 전용이며, 향후 `Home`/`Sandbox`/`Challenge` 상위 구조와 분리해 관리한다.
7. 완전 초보자는 `Pre-Kinematics Lesson 0~3`로 먼저 진입하고, 기존 `Step 1~8`은 `Core Track`으로 유지한다.

## 트랙 구조

| track_id | range | purpose | math_prerequisite |
|---|---|---|---|
| `pre_kinematics` | `L0~L3` | 수학 이전의 움직임 직관 형성 | `none` |
| `core_kinematics` | `S1~S8` | DH / frame / matrix / FK를 직관과 연결 | `light -> required` |

## Pre-Kinematics Lesson 0~3

| lesson_id | lesson_title | visual_goal | gate_type | 핵심 UI | bridge |
|---|---|---|---|---|---|
| `L0` | 로봇 팔은 무엇을 움직이는가 | 관절을 움직이면 링크와 끝점이 함께 움직인다는 감각 형성 | `observe_motion` | joint slider, link highlight, end-effector trail | 회전은 끝점이 그리는 경로를 만든다 |
| `L1` | 회전하면 끝점이 어떻게 움직이는가 | 원호/원형 경로를 눈으로 이해 | `compare_arc` | slider, ghost trail, why-it-moved | 관절이 두 개면 경로가 더 복잡해진다 |
| `L2` | 두 관절이 같이 움직이면 왜 경로가 바뀌는가 | joint 조합이 끝점 위치를 바꾼다는 점 이해 | `compare_combination` | compare mode, previous/current/delta, end-effector trail | 목표 위치를 맞추려면 거꾸로 생각해야 한다 |
| `L3` | 목표점을 맞추려면 왜 거꾸로 생각해야 하는가 | IK를 유도식이 아닌 문제 정의로 먼저 이해 | `target_match` | target marker, reach/not reach, simple hint | 이제 이 움직임을 말과 식으로 정리할 수 있다 |

## 스텝별 점진적 공개 매트릭스

| Step | Left | Right | Bottom | Focus |
|---|---|---|---|---|
| S1 | DHTable | Hidden | Hidden | DHTable |
| S2 | Hidden | FrameInfoOverlay | Hidden | Viewport3D |
| S3 | FourMatrices | Hidden | Slider(1) | MatrixPanel |
| S4 | MultiplicationProgress | Hidden | Hidden | MatrixPanel |
| S5 | DHReference | AiColorCoding | Slider(1) | RightPanel |
| S6 | CumulativeProduct | A1A2Reference | Slider(2) | Viewport3D |
| S7 | T0nAndExtract | PoseExtract | Slider(2) | EndEffectorFrame |
| S8 | FullDH | FullMatrices | AllSliders | None |

## Core Track 브리지 규칙
1. `S1~S8`은 삭제하지 않고 `Core Kinematics Track`으로 재명명한다.
2. `S1~S8` 시작 전 브리지 문장을 반드시 노출한다.
   - 예: `지금 보는 DH는 Lesson 0~3에서 본 움직임을 정리하는 방법이다.`
3. `S1~S8`에서는 직관 lesson에서 본 trail, target, why-it-moved 언어를 가능한 한 유지한 채 행렬/프레임/공식으로 연결한다.

## 게이트 조건 카탈로그

| Step | Gate Condition | 완료 메시지 |
|---|---|---|
| S1 | DH 헤더 2회 이상 호버 | DH 파라미터의 의미를 확인했습니다! |
| S2 | 프레임 2개 이상 클릭 | 프레임 배치 규칙을 확인했습니다! |
| S3 | 4개 변환 클릭 + 슬라이더 1회 | 4가지 기본 변환을 확인했습니다! |
| S4 | 곱셈 단계 4회 진행 | A_i = Rz·Tz·Tx·Rx 완성! |
| S5 | θ1 슬라이더 + R/p 영역 호버 | 회전(R)과 위치(p)를 구분했습니다! |
| S6 | 체인 완료 + 슬라이더 1회 | 순기구학 체인이 완성되었습니다! |
| S7 | 위치 열 + 회전 열 클릭 | EE 위치와 방향을 추출했습니다! |
| S8 | 없음(샌드박스) | - |

## Pre-Kinematics Gate 카탈로그

| lesson_id | gate_type | Gate Condition | 완료 메시지 |
|---|---|---|---|
| `L0` | `observe_motion` | joint를 움직여 링크와 끝점의 동시 변화 관찰 | 링크와 끝점이 함께 움직이는 것을 확인했습니다! |
| `L1` | `compare_arc` | 같은 joint를 여러 각도로 바꾸며 trail 확인 | 회전이 경로를 만든다는 점을 확인했습니다! |
| `L2` | `compare_combination` | joint1만 / joint2만 / 둘 다 움직인 경우 비교 | joint 조합이 끝점 위치를 바꾼다는 점을 확인했습니다! |
| `L3` | `target_match` | 목표점 여러 개를 맞추거나 reach/not reach 피드백 확인 | 목표를 맞추려면 joint를 거꾸로 찾아야 한다는 점을 이해했습니다! |

## Math Readiness Gate 카탈로그

| lesson_id | gate_type | Gate Condition | 완료 메시지 |
|---|---|---|---|
| `M0` | `manipulate -> confirm` | `SliderReachTarget:math_m0_pose_ready` × 1 + `StepAction:math_m0_correct` × 1 | 좋아요! 이제 90° 방향을 구분할 수 있어요. |
| `M1` | `manipulate -> confirm` | `SliderReachTarget:math_m1_zero_pose_ready` × 1 + `StepAction:math_m1_zero_correct` × 1 + `SliderReachTarget:math_m1_ninety_pose_ready` × 1 + `StepAction:math_m1_ninety_correct` × 1 | 좋아요! 이제 방향과 길이로 위치를 짐작할 수 있어요. |
| `M2` | `manipulate -> confirm` | `SliderReachTarget:math_m2_pose_ready` × 1 + `StepAction:math_m2_correct` × 1 | 좋아요! 이제 45°를 대각선 감각으로 볼 수 있어요. |
| `M3` | `manipulate -> confirm (2 joints)` | `SliderReachTarget:math_m3_pose_ready` × 1 + `StepAction:math_m3_correct` × 1 | 좋아요! 방금 한 게 기구학의 시작이에요. |

### Math Readiness Lesson 요약

| lesson_id | title | manipulation_goal | confirm_question | interactive_joints | visual_features |
|---|---|---|---|---|---|
| `M0` | 각도는 방향이다 | `J1 -> 90°` | 방금 팔이 향한 방향은? | 1 | angle reference, trail, why-it-moved, joint-highlight |
| `M1` | 길이와 각도로 위치를 짐작해요 | `J1 -> 0°`, `J1 -> 90°` | 끝점이 어디에 있나요? | 1 | angle reference, trail, length cue, joint-highlight |
| `M2` | 45°는 대각선이다 | `J1 -> 45°` | 끝점이 어디에 가까운가요? | 1 | angle reference, trail, diagonal cue, joint-highlight |
| `M3` | 두 링크를 합치면 더 멀리 간다 | `J1 -> 45°`, `J2 -> -45°` | 관절 하나만 있을 때와 뭐가 달라졌나요? | 2 | dual angle reference, trail, why-it-moved, joint-highlight |

### Math Readiness → Pre-Kinematics 전환
- M3 완료 시 `MathReadinessTrack` → `PreKinematicsTrack`으로 자동 전환 (`AppController.TryAdvanceFromMathReadiness()`)
- 전환 토스트: "좋아요! 이제 로봇 직관 lesson으로 넘어갈게요."
- 전환 후 `Template2DOF_RR` 적용, `BeginnerLessonFactory.CreateLessons()` 로드, Step 1 진입

## 온보딩 시퀀스 (첫 실행)
1. 환영 모달 표시
2. `완전 초보로 시작` 선택 시 현재 runtime 기준 `Math Readiness M0` 직행, 이후 `Pre-Kinematics Lesson 0`으로 연결된다.
3. `기본 개념은 알고 있음` 선택 시 `Core Track Step 1` 진입
4. `건너뛰기(숙련자)`는 target 기준 `Home / Continue Hub`로 이동하고, 현재 runtime fallback은 `Core Track Step 8`이다.
5. 재방문은 현재 runtime에서 track-aware key 기준 복귀하고, target 기준으로는 `Home / Continue Hub`를 거쳐 이어하기를 선택한다.
6. 차기 P0에서는 `Continue Latest Context`가 `robot_id + mode + track + step + preset`을 함께 복귀시켜야 한다.

## Product Target Alignment
1. 현재 `Step 8`은 Guided Lesson 내부의 자유 실습 상태로 유지한다.
2. future product target에서는 `Sandbox`를 별도 화면으로 승격하고, Step 8은 Guided Lesson 완료 직전의 통합 실습 단계로 재정의할 수 있다.
3. `Challenge`는 본 문서의 step matrix 밖에서 운영하며, 튜터 스텝 완료 이후 진입하는 별도 평가 모드로 본다.
4. `Lesson 0~3`은 별도 앱이 아니라 기존 Guided Lesson 앞단에 붙는 초보자 진입 계층이다.
5. 온보딩 skip은 lesson late-step direct jump보다 `Home / Continue Hub` 선택 흐름을 우선한다.

## 구현 계약
1. Step 상태는 `TutorStepConfig` SO 단일 소스로 관리한다.
2. `InteractionGateController`가 Next 활성/비활성의 단일 결정권을 가진다.
3. `TooltipSystem`은 UI/3D 트리거를 동일 인터페이스로 처리한다.
4. `StepProgressSaver(PlayerPrefs)`로 방문/진행/Reduced Motion 상태를 저장한다.
5. 저장 키는 `KineTutor3D.PreKinematics.LastCompletedStep`, `KineTutor3D.CoreKinematics.LastCompletedStep`처럼 트랙 단위 resume를 구분할 수 있어야 한다.
6. 구현 순서는 `runtime snapshot/update cause -> track-aware step foundation -> joint input/highlight/trail/target -> Why It Moved -> Lesson 0~3`를 따른다.

## 테스트 체크리스트
1. 온보딩 분기(`완전 초보`, `기본 개념 이해자`, `건너뛰기`, 재방문)와 `Home / Continue Hub` 목표 흐름 확인
2. `Lesson 0~3`에서 sin/cos, 삼각형 유도, Jacobian, DH 표가 핵심 화면으로 노출되지 않는지 확인
3. Step 1~8 패널 가시성 매트릭스 일치 확인
4. Gate 잠금/해제 + Skip 동작 확인
5. 툴팁(UI/3D), 토스트, 포커스 하이라이트 동시 동작 충돌 없음 확인
