# 좌표계 매핑: 로보틱스 ↔ Unity

## 좌표계 비교

| 특성 | 로보틱스 (표준) | Unity |
|------|:-------------:|:-----:|
| 손잡이 | 오른손 법칙 | 왼손 법칙 |
| 위쪽 축 | Z-up | Y-up |
| 앞쪽 축 | X-forward | Z-forward |

## 변환 규칙

로보틱스 좌표 `(xR, yR, zR)` → Unity 좌표 `(xU, yU, zU)`:

```
xU =  xR
yU =  zR    (로보틱스 Z-up → Unity Y-up)
zU =  yR    (로보틱스 Y → Unity Z)
```

> **주의**: 왼손/오른손 전환으로 인해 회전 방향이 반대가 됨.
> 회전 행렬 변환 시 적절한 부호 반전 필요.

## 적용 경계

```
[순수 C# 도메인]                    [Unity 렌더링 도메인]

Vec3D(x, y, z)  ──── 변환 ────→  Vector3(x, z, y)
Mat4D           ──── 변환 ────→  Matrix4x4
double          ──── 캐스트 ───→  float

변환 위치: Assets/Scripts/Visualization/ 모듈에서만 수행
```

## 변환 함수 (예정)

```csharp
// Visualization/ 모듈에서만 사용
public static Vector3 ToUnity(Vec3D v)
    => new Vector3((float)v.X, (float)v.Z, (float)v.Y);

public static Vec3D FromUnity(Vector3 v)
    => new Vec3D(v.x, v.z, v.y);
```

## 규칙
1. Types/, Math/, Kinematics/, Templates/ 모듈은 **로보틱스 좌표계만** 사용
2. Unity 좌표 변환은 **Visualization/ 모듈의 렌더링 경계**에서만 수행
3. 테스트 기준값은 모두 **로보틱스 좌표계** 기준
4. UI에서 사용자에게 표시하는 값은 로보틱스 좌표계 (교육 목적)
