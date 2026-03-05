# KineTutor3D Tutor Step Plan (Student-Friendly UX)

Version: 1.1.0
Last Updated: 2026-03-05 (KST)

## 핵심 원칙
1. 한 번에 하나만 보여준다.
2. 포커스 존으로 시선을 유도한다.
3. 마이크로 게이트로 학습 행동을 유도한다.
4. 한국어 툴팁/용어사전으로 용어 장벽을 낮춘다.
5. Phase 3 1차 행렬 표시는 `A1/A2/T02` 고정으로 운영한다.

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

## 온보딩 시퀀스 (첫 실행)
1. 환영 모달 표시
2. `학습 시작` 선택 시 Step 1 진입 + 스포트라이트 시퀀스
3. `건너뛰기(숙련자)` 선택 시 Step 8 진입
4. 재방문은 모달 스킵 + 마지막 완료 Step+1 복귀

## 구현 계약
1. Step 상태는 `TutorStepConfig` SO 단일 소스로 관리한다.
2. `InteractionGateController`가 Next 활성/비활성의 단일 결정권을 가진다.
3. `TooltipSystem`은 UI/3D 트리거를 동일 인터페이스로 처리한다.
4. `StepProgressSaver(PlayerPrefs)`로 방문/진행/Reduced Motion 상태를 저장한다.

## 테스트 체크리스트
1. 온보딩 분기(학습 시작/건너뛰기/재방문) 확인
2. Step 1~8 패널 가시성 매트릭스 일치 확인
3. Gate 잠금/해제 + Skip 동작 확인
4. 툴팁(UI/3D), 토스트, 포커스 하이라이트 동시 동작 충돌 없음 확인
