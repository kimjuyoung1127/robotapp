# KineTutor3D User Flow

Version: 1.5.1
Last Updated: 2026-03-13 (KST)
Implementation Status: Phase 5 Complete (Boot/Onboarding/Home/Main/RobotLibrary/Sandbox/RobotControl/MathReadiness flow implemented)

## 목표
- 초보 학습자가 수학 이전의 직관 lesson을 거쳐 8단계 코어 튜토리얼까지 압도감 없이 완료한다.
- `Hard gate + Skip` 정책으로 학습 집중과 이탈 방지를 동시에 달성한다.

## Current Runtime vs Product Target
- 현재 런타임 baseline은 `Boot -> Onboarding -> Home/Main -> Main/MathReadiness/Sandbox/RobotLibrary` 구조가 실제 구현 완료되었다.
- `Home / Continue Hub`가 재방문 허브 역할을 하며, Onboarding의 `알고 있어요`/`둘러보기`는 Home으로, `처음이에요`는 별도 `MathReadiness.unity`로 직접 연결된다.
- Editor QA: `BootScenePlayModeSetup`에 의해 어떤 씬이 열려있든 Onboarding.unity부터 시작. `QaToolsMenu`로 First-Time/Returning User 리셋 가능.
- 본 문서는 현재 런타임 규칙과 차기 P0 target을 함께 추적한다.

## End-to-End Flow (Current Runtime — 실제 구현 기준)
1. 앱 시작
2. `Boot.unity`에서 첫 방문 여부 확인 (`StepProgressSaver.HasVisited()`)
3. 분기
   - 첫 방문: `Onboarding.unity`
   - 재방문: `Home.unity` (Continue Hub)
4. `Onboarding.unity`
   - `학습 시작` (기본 개념 이해): CoreKinematics 트랙 설정 → `Home.unity`
   - `초보자 시작` (완전 초보): MathReadiness 트랙 설정 + `2DOF_RR` 선택 → `MathReadiness.unity`
   - `건너뛰기`: CoreKinematics 트랙 설정 → `Home.unity`
   - QA/디버그 시에는 상단 전역 네비게이션으로 `Onboarding / Home / Main / Robot Library / Sandbox`를 직접 이동할 수 있다
5. `Home.unity` (Continue Hub)
   - `이어하기`: SessionContextStore 복구 → Main / MathReadiness / Sandbox
   - `학습 시작`: CoreKinematics → Main
   - `수학 기초`: MathReadiness → MathReadiness
   - `로봇 라이브러리`: RobotLibrary.unity
   - `Sandbox`: Sandbox.unity
6. `MathReadiness.unity`
   - `Math Readiness Track`: 0°/90°/180° 기준선 확인 -> 슬라이더 조작 -> 목표 각도 도달 -> 확인 질문
   - M3 완료 시 `Main.unity`로 이동하며 `Pre-Kinematics Track`으로 bridge한다
7. `Main.unity`
   - `Pre-Kinematics Track`: trail, target, why-it-moved 중심 입력
   - `Core Track`: Step 진행 중 입력(호버/클릭/슬라이더)
   - Slider: `theta` 단일 소스(deg 입력 -> rad 변환)
   - DH Table: `d/a/alpha`만 편집 가능(`theta` read-only)
8. Gate 조건 평가
   - 미충족: Next 비활성, 힌트/토스트 안내
   - 충족: Next 활성, 완료 토스트 표시
9. Step 전환 시 패널/포커스/툴팁 상태 동기화
10. 재방문 시 `Boot -> Home`으로 복귀하고 Continue Hub에서 이어하기/새로 시작 선택. track-aware last-completed-step key(`KineTutor3D.MathReadiness.LastCompletedStep`, `KineTutor3D.PreKinematics.LastCompletedStep`, `KineTutor3D.CoreKinematics.LastCompletedStep`) 기준으로 시작

## Next P0 Target Flow
1. 첫 방문은 `Boot -> Onboarding -> Home / Continue Hub`로 연결한다.
2. `완전 초보`는 `Lesson 0`, `기본 개념 이해`는 `Core Track Step 1`로 진입한다.
3. `건너뛰기`는 고급 스텝 직접 점프가 아니라 `Home / Continue Hub`로 이동한다.
4. 재방문 기본 진입점도 `Home / Continue Hub`로 바꾼다.
5. `Home / Continue Hub`는 `이어하기 / Guided Lesson / Robot Library / Sandbox / Progress / Settings`를 제공한다.

## Flow Diagram (현재 구현 기준)
```mermaid
flowchart TD
  A["App Start"] --> B["Boot.unity"]
  B --> C{"First Visit?"}
  C -->|"yes"| D["Onboarding.unity"]
  C -->|"no"| H["Home.unity (Continue Hub)"]

  D -->|"학습 시작"| H
  D -->|"초보자 시작"| G["MathReadiness.unity -> Math Readiness"]
  D -->|"건너뛰기"| H

  H -->|"이어하기"| E["Main/MathReadiness/Sandbox Resume"]
  H -->|"학습 시작"| F["Main.unity -> Core Step"]
  H -->|"수학 기초"| G["MathReadiness.unity -> Math Readiness"]
  H -->|"로봇 라이브러리"| R["RobotLibrary.unity"]
  H -->|"Sandbox"| S["Sandbox.unity"]

  E --> I["User Interaction"]
  F --> I
  G --> I
  R -->|"학습 시작"| F
  R -->|"Sandbox"| S
  S --> I

  I --> J["Gate Evaluation"]
  J -->|"not met"| K["Next Disabled + Hint"] --> I
  J -->|"met"| L["Next Enabled + Toast"]

  L --> M{"Next or Skip?"}
  M -->|"next"| N["Move to Next Lesson/Step"]
  M -->|"skip"| O["Force Pass Current Gate"] --> N
```

## 상태 전이
- Session: `init -> boot -> onboarding|main -> learning -> completed`
- Track: `pre_kinematics | core_kinematics`
- Lesson/Step: `lesson_0 ... lesson_3`, `step_1 ... step_8`
- Gate: `locked -> unlocked`

## Product Target Navigation
1. `Onboarding`은 계속 첫 방문 진입점으로 유지한다.
2. 재방문 기본 진입점은 `Main`에서 `Home / Continue Hub`로 이동한다.
3. `Home / Continue Hub`는 `Guided Lesson`, `Robot Library`, `Sandbox`, `Progress`, `Settings` 진입 허브가 된다.
4. 온보딩의 `건너뛰기`도 `Home / Continue Hub`로 연결하고, direct late-step jump는 제거한다.
5. future product target의 `Guided Lesson`은 `완전 초보 -> Pre-Kinematics Lesson 0~3 -> Core Track Step 1~8` 구조를 기본 경로로 가진다.

## UX 게이트 규칙
1. 기본 정책: `Hard gate` (조건 충족 전 Next 비활성)
2. 예외 정책: `Skip` 버튼은 lesson 내부 예외 제어로만 허용하고, 온보딩 shortcut으로 사용하지 않는다.
3. 검증 실패 입력(예: NaN/Infinity)은 계산 파이프라인 진입 금지
4. `Lesson 0~3`에서는 행렬/공식 대신 `trail`, `target marker`, `reach/not reach`, `Why It Moved`를 우선한다.
5. `Math Readiness M0~M3`에서는 질문보다 먼저 3D 각도 기준선과 슬라이더 조작을 제시하고, 목표 각도 도달 후에만 확인 질문을 노출한다.
6. 구현 순서는 `track-aware step foundation`과 `공통 input/visualization`을 먼저 고정한 뒤 `Lesson 0~3`를 연결한다.

## 수용 기준
1. 첫 방문은 `Boot -> Onboarding -> Home/Main`, 재방문은 `Boot -> Home`으로 분기된다.
2. Onboarding 이후 `완전 초보 -> MathReadiness.unity -> Lesson 0`, `기본 개념 이해자 -> Core Track Step 1` 분기가 존재한다.
3. Gate 완료 전 Next 비활성, 완료 후 활성으로 정확히 전환된다.
4. Skip 사용 시 현재 Lesson/Step을 건너뛰고 다음 단계로 정상 진입한다.
5. MatrixDisplay가 `Core Track`에서 `A1/A2/T02`를 이벤트 기반으로 실시간 갱신한다.
6. `Lesson 0~3` 문서 어디에도 sin/cos 공식, 삼각형 유도, Jacobian, DH 표가 핵심 화면으로 요구되지 않는다.
7. `Home / Continue Hub`가 재방문과 온보딩 skip의 공통 착지점으로 구현 완료되었다.
8. Sandbox에서 학습 패널과 Sandbox 전용 패널이 배타적으로 제어되어 겹치지 않는다.
9. Editor QA: `BootScenePlayModeSetup`으로 어떤 씬에서든 Onboarding부터 시작, `QaToolsMenu`로 PlayerPrefs 리셋 가능.
10. Onboarding에도 `SceneNavigationBar`를 유지해 페이지 간 즉시 이동과 상태 점검이 가능하다.
