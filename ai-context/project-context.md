# 프로젝트 컨텍스트

## 한 줄 정의
KineTutor3D는 완전 초보자도 `Pre-Kinematics Lesson 0~3`로 진입할 수 있는 guided-first 로보틱스 학습용 Unity 3D 교육 도구이다.

## 개발 동기
- 로봇공학 교육에서 DH 파라미터 → 변환행렬 → 엔드이펙터 위치의 과정이 추상적
- 많은 학습자가 sin/cos, 삼각형, IK 유도, 행렬/DH에서 바로 막히므로 수학 이전의 직관 단계가 필요함
- 3D 시각화를 통해 학습자가 파라미터 변경의 즉각적 효과를 확인할 수 있음
- 단계별 튜터 기능으로 수학 원리를 점진적으로 이해

## 대상 사용자
- 산업용 로봇 프로그래밍 입문자
- 로봇공학 교육자
- 수학과 기구학 개념이 낯선 완전 초보 학습자

## 제품 문서 Source of Truth
- `docs/ref/PRD.md` — 타겟, 문제 정의, 가치, 기능 범위
- `docs/ref/WIREFRAME.md` — 정보 구조, 핵심 화면, 사용자 흐름
- `docs/ref/PRODUCT-ROADMAP.md` — 30/60/90일 제품 로드맵과 릴리스 게이트
- `docs/status/PRODUCT-DOC-BOARD.md` — 제품 문서 3종의 상태 추적

## 핵심 UX 콘셉트
```
움직임 직관(Lesson 0~3) -> DH/FK 연결(Core Track) -> 3D 시각화 -> 단계별 튜터
```
1. Lesson 0~3에서 trail, target marker, why-it-moved로 움직임 감각 형성
2. Core Track에서 DH/FK와 행렬을 직관과 연결
3. 로봇 관절/링크 3D 렌더링
4. Guided Lesson으로 각 변환 단계를 하나씩 설명

## 시스템 개요
- **엔진**: Unity 6 (6000.0.64f1), URP
- **언어**: C# (Unity 2022 기준)
- **수학 정밀도**: Double-precision (`Vec3D/Mat3D/Mat4D`)
- **MCP**: CoplayDev `unity-mcp` 패키지 설치됨

## 핵심 도메인 모델

### 타입 (Assets/Scripts/Types/)
- `JointType` — 관절 타입 열거형 (Revolute, Prismatic)
- `DHLink` — DH 파라미터 구조체 (theta, d, a, alpha)
- `RobotTemplate` — 로봇 설정 (이름, DOF, DHLink[], jointLimits)
- `Pose` — 엔드이펙터 자세 (위치 Vec3D, 회전 Mat3D)

### 수학 (Assets/Scripts/Math/)
- `Vec3D` — 3D 벡터 (double x, y, z)
- `Mat3D` — 3×3 회전 행렬
- `Mat4D` — 4×4 동차 변환 행렬

### 기구학 (Assets/Scripts/Kinematics/)
- `DHStandard` — 표준 DH: A_i = Rz(θ)·Tz(d)·Tx(a)·Rx(α)
- `ForwardKinematics` — 누적곱 T = A₁···Aₖ, R과 p 추출

## 모듈 경계

```
[Types] ← [Math] ← [Kinematics] ← [Templates]
                                        ↓
                    [App] → [UI] + [Visualization]
```

| 모듈 | UnityEngine 허용 | 정밀도 |
|------|:----------------:|:------:|
| Types | ✗ | double |
| Math | ✗ | double |
| Kinematics | ✗ | double |
| Templates | ✗ | double |
| UI | ✓ | - |
| Visualization | ✓ | double→float 경계 |
| App | ✓ | - |

## 테스트 전략
- **EditMode**: 수학, 기구학, 타입 — 순수 로직 테스트 (Unity 씬 불필요)
- **PlayMode**: UI, 시각화, 검증 — 통합/씬 테스트

## 고위험 항목
1. **좌표계 불일치**: 로보틱스(오른손 법칙, Z-up) vs Unity(왼손, Y-up)
2. **정밀도 드리프트**: 긴 체인에서 double→float 변환 시 오차 누적
3. **6DOF 복잡성**: 6자유도 로봇의 DH 파라미터 검증 난이도

## 범위 & 제외사항
- **포함**: Beginner Lesson 0~3, FK, Standard DH, 2/3/6 DOF 템플릿, Step Tutor, 3D 시각화
- **미포함**: IK 유도식 구현, 경로 계획, 충돌 감지, 다중 로봇 완성형 런타임

## 현재 제품 운영 방향
- 현재 런타임은 `Boot -> Onboarding -> Main + RobotLibrary` 학습 흐름의 Guided Lesson P0 완료 상태이다.
- Phase 5 완료: runtime snapshot, track-aware step, joint input/highlight, Why It Moved, Beginner L0~L3, Robot Library MVP 셸
- 제품화 방향은 `Home / Guided Lesson / Sandbox / Challenge / Progress / Settings` 구조로 확장한다.
- 온보딩은 계속 유지하되, 제품의 메인 진입점은 향후 Home/Dashboard로 이동한다.
- Guided Lesson 내부 기본 경로는 `완전 초보 -> Pre-Kinematics Lesson 0~3 -> Core Track Step 1~8`로 본다.
- 테스트 기준: EditMode 107/107, PlayMode 30/30
