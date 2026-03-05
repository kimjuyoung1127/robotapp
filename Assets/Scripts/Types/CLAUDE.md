# Types/

로봇 기구학 도메인 타입.

## 파일 (예정)
- `JointType.cs` — 관절 타입 열거형 (Revolute, Prismatic)
- `DHLink.cs` — DH 파라미터 구조체 (theta, d, a, alpha)
- `RobotTemplate.cs` — 로봇 설정 (이름, DOF, DHLink[], jointLimits)
- `Pose.cs` — 엔드이펙터 자세 (위치 Vec3D, 회전 Mat3D)

## 규칙
1. 모든 구조체는 `double` 정밀도 사용
2. `using UnityEngine` 금지
3. 불변(immutable) 선호: `readonly struct` 사용
