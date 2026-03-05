# 프로젝트 컨텍스트

## 한 줄 정의
KineTutor3D는 DH 파라미터 기반 로봇 기구학을 시각적으로 학습하는 Unity 3D 교육 도구이다.

## 개발 동기
- 로봇공학 교육에서 DH 파라미터 → 변환행렬 → 엔드이펙터 위치의 과정이 추상적
- 3D 시각화를 통해 학습자가 파라미터 변경의 즉각적 효과를 확인할 수 있음
- 단계별 튜터 기능으로 수학 원리를 점진적으로 이해

## 대상 사용자
- 로봇공학 전공 학부생/대학원생
- 로봇공학 교육자
- 산업용 로봇 프로그래밍 입문자

## 핵심 UX 콘셉트
```
DH 파라미터 입력 → FK 계산 → 3D 시각화 → 단계별 튜터
```
1. DH 테이블에서 파라미터(θ, d, a, α) 편집
2. Forward Kinematics 실시간 계산
3. 로봇 관절/링크 3D 렌더링
4. Step Tutor로 각 변환 단계를 하나씩 설명

## 시스템 개요
- **엔진**: Unity 6 (6000.0.64f1), Built-in Render Pipeline
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
- **포함**: FK, Standard DH, 2/3/6 DOF 템플릿, Step Tutor, 3D 시각화
- **미포함**: 역기구학(IK), 경로 계획, 충돌 감지, 다중 로봇
