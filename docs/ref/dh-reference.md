# DH 파라미터 수학 레퍼런스

> **KineTutor3D** -- Unity/C# 기반 로보틱스 교육 도구를 위한 수학 참조 문서.
> 이 문서는 Denavit-Hartenberg 표기법의 정의, 행렬 전개, 단계별 예제,
> 그리고 프로젝트 코드와의 매핑을 포괄한다.

---

## 1. 파라미터 정의

| 파라미터 | 기호 | 설명 | 단위 |
|---------|------|------|------|
| Joint angle | θ_i | Z축 주위 회전 (Revolute 조인트의 변수) | rad |
| Link offset | d_i | Z축 방향 이동 (Prismatic 조인트의 변수) | m |
| Link length | a_i | X축 방향 이동 (공통 법선 길이) | m |
| Link twist | α_i | X축 주위 회전 (인접 Z축 사이 각도) | rad |

**Revolute 조인트**: θ_i 가 관절 변수, 나머지는 상수.
**Prismatic 조인트**: d_i 가 관절 변수, 나머지는 상수.

---

## 2. 기본 변환행렬 4개 (Elementary Transforms)

Standard DH 변환은 네 개의 기본(elementary) 동차 변환행렬의 곱으로 정의된다.
각 행렬은 단일 축에 대한 회전 또는 이동만을 나타낸다.

### 2.1 Rz(θ) -- Z축 회전

Z축을 중심으로 θ만큼 회전한다. XY 평면 내에서 좌표계가 회전한다.

```
Rz(θ) = [ cos(θ)  -sin(θ)  0  0 ]
         [ sin(θ)   cos(θ)  0  0 ]
         [   0        0     1  0 ]
         [   0        0     0  1 ]
```

- 3×3 좌상 블록: 표준 2D 회전행렬을 Z축으로 확장한 형태.
- 이동 성분(4열): 모두 0 -- 순수 회전.
- Revolute 조인트에서 θ는 관절 변수이므로 이 행렬이 운동 시 변한다.

### 2.2 Tz(d) -- Z축 이동

Z축 방향으로 d만큼 이동한다.

```
Tz(d) = [ 1  0  0  0 ]
         [ 0  1  0  0 ]
         [ 0  0  1  d ]
         [ 0  0  0  1 ]
```

- 회전 블록: 단위행렬 I₃ -- 방향 변화 없음.
- 이동 성분: Z 방향에만 d.
- Prismatic 조인트에서 d는 관절 변수이므로 이 행렬이 운동 시 변한다.

### 2.3 Tx(a) -- X축 이동

X축 방향으로 a(링크 길이)만큼 이동한다.

```
Tx(a) = [ 1  0  0  a ]
         [ 0  1  0  0 ]
         [ 0  0  1  0 ]
         [ 0  0  0  1 ]
```

- 회전 블록: 단위행렬 I₃.
- 이동 성분: X 방향에만 a.
- a는 보통 상수이며, 링크의 공통 법선(common normal) 길이를 나타낸다.

### 2.4 Rx(α) -- X축 회전

X축을 중심으로 α(링크 비틀림)만큼 회전한다.

```
Rx(α) = [ 1    0       0     0 ]
         [ 0  cos(α)  -sin(α) 0 ]
         [ 0  sin(α)   cos(α) 0 ]
         [ 0    0       0     1 ]
```

- YZ 평면 내에서 좌표계가 회전한다.
- 이동 성분: 모두 0 -- 순수 회전.
- α는 인접한 두 Z축 사이의 각도를 나타낸다.

---

## 3. Standard DH Convention

### 3.1 정의

Standard DH (Denavit-Hartenberg, 1955) 에서 링크 i의 동차 변환행렬 A_i 는
네 개의 기본 변환을 **왼쪽에서 오른쪽 순서**로 곱하여 얻는다:

```
A_i = Rz(θ_i) · Tz(d_i) · Tx(a_i) · Rx(α_i)
```

이 순서의 물리적 의미:
1. **Rz(θ)**: 현재 프레임의 Z축 주위로 θ만큼 회전하여 X축을 공통 법선 방향으로 정렬.
2. **Tz(d)**: Z축 방향으로 d만큼 이동하여 공통 법선 위치로 이동.
3. **Tx(a)**: X축 방향으로 a만큼 이동하여 다음 프레임의 원점으로 이동.
4. **Rx(α)**: X축 주위로 α만큼 회전하여 Z축을 다음 관절의 축으로 정렬.

### 3.2 전개된 4×4 결과 행렬

네 행렬의 곱을 전개하면 다음과 같은 단일 4×4 행렬을 얻는다:

```
A_i = [ cos(θ)   -sin(θ)cos(α)    sin(θ)sin(α)   a·cos(θ) ]
      [ sin(θ)    cos(θ)cos(α)   -cos(θ)sin(α)   a·sin(θ) ]
      [   0         sin(α)          cos(α)           d      ]
      [   0           0               0              1      ]
```

**행렬 구조 해석:**

- **3×3 좌상 블록** (R): 회전행렬.
  - 1열: 다음 프레임 X축의 현재 프레임 좌표.
  - 2열: 다음 프레임 Y축의 현재 프레임 좌표.
  - 3열: 다음 프레임 Z축의 현재 프레임 좌표.
- **3×1 우측 열** (p): 이동 벡터.
  - p_x = a·cos(θ), p_y = a·sin(θ), p_z = d.
- **마지막 행**: [0 0 0 1] -- 동차 좌표계의 표준 형식.

---

## 4. 단계별 곱셈 예제

단일 Revolute 조인트에 대해 다음 DH 파라미터를 사용한다:

| θ | d | a | α |
|---|---|---|---|
| π/2 | 0 | 1 | 0 |

목표: Rz(π/2) · Tz(0) · Tx(1) · Rx(0) 을 단계별로 계산한다.

### Step 1: 각 기본 행렬 대입

```
Rz(π/2) = [ cos(π/2)  -sin(π/2)  0  0 ]     [ 0  -1  0  0 ]
           [ sin(π/2)   cos(π/2)  0  0 ]  =  [ 1   0  0  0 ]
           [    0          0      1  0 ]     [ 0   0  1  0 ]
           [    0          0      0  1 ]     [ 0   0  0  1 ]
```

```
Tz(0) = [ 1  0  0  0 ]
         [ 0  1  0  0 ]
         [ 0  0  1  0 ]
         [ 0  0  0  1 ]
```

Tz(0) = I₄ (단위행렬). d=0이므로 이동 없음.

```
Tx(1) = [ 1  0  0  1 ]
         [ 0  1  0  0 ]
         [ 0  0  1  0 ]
         [ 0  0  0  1 ]
```

```
Rx(0) = [ 1  0  0  0 ]
         [ 0  1  0  0 ]
         [ 0  0  1  0 ]
         [ 0  0  0  1 ]
```

Rx(0) = I₄ (단위행렬). α=0이므로 비틀림 없음.

### Step 2: Rz(π/2) · Tz(0)

Tz(0) = I₄ 이므로:

```
Rz(π/2) · Tz(0) = Rz(π/2) = [ 0  -1  0  0 ]
                                [ 1   0  0  0 ]
                                [ 0   0  1  0 ]
                                [ 0   0  0  1 ]
```

### Step 3: (Rz · Tz) · Tx(1)

```
[ 0  -1  0  0 ]   [ 1  0  0  1 ]   [ 0  -1  0  0 ]
[ 1   0  0  0 ] · [ 0  1  0  0 ] = [ 1   0  0  1 ]
[ 0   0  1  0 ]   [ 0  0  1  0 ]   [ 0   0  1  0 ]
[ 0   0  0  1 ]   [ 0  0  0  1 ]   [ 0   0  0  1 ]
```

곱셈 상세 (이동 열 계산):
- p_x: 0·1 + (-1)·0 + 0·0 + 0·1 = 0
- p_y: 1·1 + 0·0 + 0·0 + 0·1 = 1
- p_z: 0·1 + 0·0 + 1·0 + 0·1 = 0

따라서 이동 벡터 p = (0, 1, 0).

### Step 4: (Rz · Tz · Tx) · Rx(0)

Rx(0) = I₄ 이므로:

```
A = [ 0  -1  0  0 ]
    [ 1   0  0  1 ]
    [ 0   0  1  0 ]
    [ 0   0  0  1 ]
```

### 검증: 전개 공식과 직접 비교

전개된 공식에 θ=π/2, d=0, a=1, α=0을 대입:

```
A = [ cos(π/2)   -sin(π/2)cos(0)    sin(π/2)sin(0)   1·cos(π/2) ]
    [ sin(π/2)    cos(π/2)cos(0)   -cos(π/2)sin(0)   1·sin(π/2) ]
    [    0           sin(0)            cos(0)            0        ]
    [    0             0                 0               1        ]

  = [ 0   -1·1    1·0   1·0 ]     [ 0  -1  0  0 ]
    [ 1    0·1   -0·0   1·1 ]  =  [ 1   0  0  1 ]
    [ 0     0      1     0  ]     [ 0   0  1  0 ]
    [ 0     0      0     1  ]     [ 0   0  0  1 ]
```

단계별 곱셈 결과와 전개 공식 결과가 **일치**한다.

**물리적 해석**: 원점에서 Z축 주위로 90도 회전 후, (원래의) X축 방향으로 1m 이동.
결과적으로 엔드이펙터는 (0, 1, 0) 위치에 있고, X축이 -Y 방향, Y축이 +X 방향을 향한다.

---

## 5. Forward Kinematics (순기구학)

### 5.1 연쇄 곱

n개의 링크로 이루어진 로봇의 기저(base)에서 엔드이펙터(end-effector)까지의
전체 동차 변환은 각 링크 변환의 순차적 곱으로 구한다:

```
T₀ₙ = A₁ · A₂ · A₃ · ... · Aₙ
```

여기서:
- T₀ₙ: 프레임 0(기저)에서 프레임 n(엔드이펙터)까지의 4×4 동차 변환행렬.
- A_i: 프레임 (i-1)에서 프레임 i까지의 변환 (Section 3에서 정의).

### 5.2 중간 프레임

임의의 중간 프레임 k (0 < k < n)까지의 변환:

```
T₀ₖ = A₁ · A₂ · ... · Aₖ
```

프레임 i에서 프레임 j까지의 변환 (i < j):

```
Tᵢⱼ = A_{i+1} · A_{i+2} · ... · Aⱼ
```

### 5.3 결과 추출

최종 변환행렬 T₀ₙ에서 물리량을 추출하는 방법:

```
T₀ₙ = [ r₁₁  r₁₂  r₁₃  p_x ]
       [ r₂₁  r₂₂  r₂₃  p_y ]
       [ r₃₁  r₃₂  r₃₃  p_z ]
       [  0    0    0    1   ]
```

- **위치 벡터 (position)**: p = T₀ₙ[0:3, 3] = (p_x, p_y, p_z)
  - 엔드이펙터 원점의 기저 프레임 좌표.
- **회전행렬 (rotation)**: R = T₀ₙ[0:3, 0:3]
  - 엔드이펙터 프레임의 자세(orientation).
  - 각 열은 엔드이펙터 좌표축이 기저 프레임에서 어느 방향인지 나타낸다.

---

## 6. 회전행렬 성질 (Rotation Matrix Properties)

3×3 회전행렬 R ∈ SO(3)은 다음 성질을 만족한다.
이 성질들은 EditMode 테스트에서 검증 가능한 불변량(testable invariants)이다.

### 6.1 직교성 (Orthogonality)

```
R^T · R = I₃
```

```
R · R^T = I₃
```

즉, R의 전치행렬은 R의 역행렬과 같다. 이는 R의 열(column)들이
서로 직교하는 단위벡터임을 의미한다.

**테스트 방법**: R^T · R 의 각 원소가 단위행렬의 해당 원소와
허용 오차(1e-6) 이내인지 확인.

### 6.2 행렬식 (Determinant)

```
det(R) = +1
```

행렬식이 +1이면 **고유 회전**(proper rotation)이다.
-1이면 반전(reflection)이 포함된 것이므로 유효한 회전이 아니다.

**테스트 방법**: `Mathf.Abs(det(R) - 1.0) < 1e-6` 확인.

### 6.3 역행렬 = 전치행렬

```
R⁻¹ = R^T
```

역변환을 구할 때 비용이 큰 행렬 역산(matrix inversion) 대신
단순 전치(transpose)만 하면 된다. 계산 효율성의 핵심.

### 6.4 열벡터의 단위 크기

R의 각 열벡터 c_j 에 대해:

```
‖c_j‖ = 1,   j = 1, 2, 3
```

**테스트 방법**: 각 열의 L2 노름이 1과 허용 오차 이내인지 확인.

### 6.5 열벡터의 상호 직교

임의의 두 열 c_i, c_j (i ≠ j) 에 대해:

```
c_i · c_j = 0
```

**테스트 방법**: 내적(dot product)이 0과 허용 오차 이내인지 확인.

---

## 7. 동차변환 규칙 (Homogeneous Transform Rules)

4×4 동차 변환행렬의 연산 규칙. 순기구학 체인 구성의 수학적 기초.

### 7.1 합성 (Composition)

연속된 변환은 행렬 곱으로 합성한다:

```
T₀₂ = T₀₁ · T₁₂
```

일반화:

```
T₀ₙ = T₀₁ · T₁₂ · T₂₃ · ... · T_{(n-1)n}
```

**주의**: 행렬 곱은 교환법칙이 성립하지 않는다 (AB ≠ BA, 일반적으로).
곱셈 순서가 물리적 의미를 결정한다.

### 7.2 역변환 (Inverse)

동차 변환행렬 T가 다음과 같을 때:

```
T = [ R  p ]
    [ 0  1 ]
```

여기서 R은 3×3 회전행렬, p는 3×1 이동벡터이면, 역변환은:

```
T⁻¹ = [ R^T   -R^T · p ]
       [ 0  0  0     1   ]
```

이 공식은 일반적인 4×4 역행렬 계산(가우스 소거법 등)보다 훨씬 효율적이다.
R이 직교행렬이라는 성질(R⁻¹ = R^T)을 활용하기 때문이다.

**유도 과정**:
1. T · T⁻¹ = I 에서 시작.
2. R · R^T = I₃ (직교성)으로부터 좌상 블록 해결.
3. R · (-R^T · p) + p = 0 으로부터 이동 블록 해결.

### 7.3 결합법칙 (Associativity)

```
(A · B) · C = A · (B · C)
```

행렬 곱은 결합법칙을 만족한다. 따라서 순기구학 체인을 어떤 순서로
묶어 계산하든 최종 결과는 동일하다. 이는 중간 프레임 캐싱의 수학적 근거이다.

예: 6축 로봇에서 T₀₆ = (A₁·A₂·A₃) · (A₄·A₅·A₆)으로 나누어
계산할 수 있다 (팔-손목 분리 등).

### 7.4 단위 변환 (Identity)

```
I₄ · T = T · I₄ = T
```

4×4 단위행렬은 "변환 없음"을 나타낸다. 기저 프레임의 초기 상태.

---

## 8. Modified DH Convention (Craig)

> **참고**: 본 프로젝트(KineTutor3D)는 Standard DH를 기본으로 사용한다.
> Modified DH는 참조 및 향후 확장을 위해 문서화한다.

### 8.1 정의

Modified DH Convention (Craig, 2005)에서 링크 i의 변환은:

```
A_i = Rx(α_{i-1}) · Tx(a_{i-1}) · Rz(θ_i) · Tz(d_i)
```

Standard DH와의 핵심 차이:
- α와 a의 인덱스가 (i-1)이다 -- 이전 링크의 파라미터를 사용.
- 변환 순서가 다르다: X축 변환이 먼저, Z축 변환이 나중.
- 기저 프레임(frame 0)의 정의가 다르다.

### 8.2 전개된 4×4 결과 행렬

```
A_i = [ cos(θ_i)                -sin(θ_i)               0               a_{i-1}              ]
      [ sin(θ_i)cos(α_{i-1})     cos(θ_i)cos(α_{i-1})  -sin(α_{i-1})   -sin(α_{i-1})·d_i    ]
      [ sin(θ_i)sin(α_{i-1})     cos(θ_i)sin(α_{i-1})   cos(α_{i-1})    cos(α_{i-1})·d_i    ]
      [ 0                        0                       0               1                    ]
```

### 8.3 Standard vs Modified 비교

| 항목 | Standard DH | Modified DH (Craig) |
|------|-------------|---------------------|
| 곱셈 순서 | Rz·Tz·Tx·Rx | Rx·Tx·Rz·Tz |
| α, a 인덱스 | i (현재 링크) | i-1 (이전 링크) |
| 프레임 부착 | 링크 끝 | 링크 시작 |
| 기저 프레임 | 조인트 1에 부착 | 독립적 정의 가능 |
| 주요 교과서 | Paul (1981) | Craig (2005) |

**주의**: 같은 로봇이라도 Convention에 따라 DH 파라미터 값이 달라진다.
두 Convention을 혼용하면 안 된다.

---

## 9. 수치 정밀도 정책

| 항목 | 허용 오차 | 비고 |
|------|----------|------|
| 위치 (position) | < 1e-4 m | 엔드이펙터 좌표 비교 시 |
| 회전 (rotation) | < 1e-3 rad | 오일러각 또는 축-각 비교 시 |
| 직교성 검증 | < 1e-6 | R^T·R ≈ I₃ 확인 시 |
| 행렬식 검증 | < 1e-6 | det(R) ≈ 1 확인 시 |
| 내부 연산 | double (64-bit) | System.Double, 모든 기구학 계산 |
| 렌더링 출력 | float (32-bit) | Unity Transform 경계에서만 변환 |

**정밀도 경계(Precision Boundary)**:
- 기구학 엔진 내부: 모든 연산을 double로 수행.
- Unity 렌더링: Transform.position, Transform.rotation은 float.
- 변환 지점: `ForwardKinematics.Compute()` 결과를 Unity에 적용할 때
  double -> float 캐스팅 발생. 이 경계에서만 정밀도 손실이 허용된다.

---

## 10. 구현 매핑

### 10.1 핵심 타입 매핑

| 수학 개념 | C# 타입 | 파일 |
|----------|---------|------|
| 3D 벡터 (p ∈ R³) | `Vec3D` | `Assets/Scripts/Math/Vec3D.cs` |
| 3×3 회전행렬 (R ∈ SO(3)) | `Mat3D` | `Assets/Scripts/Math/Mat3D.cs` |
| 4×4 동차변환행렬 (T ∈ SE(3)) | `Mat4D` | `Assets/Scripts/Math/Mat4D.cs` |
| DH 파라미터 세트 (θ, d, a, α) | `DHLink` | `Assets/Scripts/Types/DHLink.cs` |

### 10.2 연산 매핑

| 수학 연산 | C# 메서드 | 파일 |
|----------|----------|------|
| 기본 변환 Rz, Tz, Tx, Rx | `Mat4D.RotZ()`, `Mat4D.TransZ()`, `Mat4D.TransX()`, `Mat4D.RotX()` | `Assets/Scripts/Math/Mat4D.cs` |
| 개별 링크 변환 A_i | `DHStandard.ComputeA()` | `Assets/Scripts/Kinematics/DHStandard.cs` |
| 순기구학 T₀ₙ | `ForwardKinematics.Compute()` | `Assets/Scripts/Kinematics/ForwardKinematics.cs` |
| 행렬 곱 (4×4) | `Mat4D.operator *` 또는 `Mat4D.Multiply()` | `Assets/Scripts/Math/Mat4D.cs` |
| 역변환 T⁻¹ | `Mat4D.Inverse()` | `Assets/Scripts/Math/Mat4D.cs` |
| 회전행렬 전치 R^T | `Mat3D.Transpose()` | `Assets/Scripts/Math/Mat3D.cs` |
| 위치 추출 p | `Mat4D.GetPosition()` 또는 인덱서 `[0:3, 3]` | `Assets/Scripts/Math/Mat4D.cs` |
| 회전 추출 R | `Mat4D.GetRotation()` 또는 인덱서 `[0:3, 0:3]` | `Assets/Scripts/Math/Mat4D.cs` |

### 10.3 테스트에서의 불변량 검증

| 검증 항목 | 수학 조건 | 테스트 방식 |
|----------|----------|------------|
| 직교성 | R^T · R = I₃ | 결과 행렬의 각 원소와 I₃ 비교 |
| 행렬식 | det(R) = +1 | `Mat3D.Determinant()` 결과 검증 |
| 열벡터 크기 | ‖c_j‖ = 1 | 각 열의 L2 노름 계산 |
| 역변환 왕복 | T · T⁻¹ = I₄ | 곱의 결과가 단위행렬인지 검증 |
| 전개 공식 일치 | A_i(step-by-step) = A_i(closed-form) | 두 방법의 결과 비교 |

---

## 11. 참조 출처

1. **Denavit, J. & Hartenberg, R.S.** (1955). "A kinematic notation for lower-pair mechanisms based on matrices." *ASME Journal of Applied Mechanics*, 22, 215-221.
   - DH 파라미터 표기법의 원론.

2. **Craig, J.J.** (2005). *Introduction to Robotics: Mechanics and Control* (3rd ed.). Pearson Prentice Hall.
   - Modified DH Convention의 주요 출처. 교육용 표준 교과서.

3. **Paul, R.P.** (1981). *Robot Manipulators: Mathematics, Programming, and Control*. MIT Press.
   - Standard DH Convention의 체계적 정리.

4. **Corke, P.** Robotics Toolbox for Python.
   - 수치 검증 및 시각화 참조. `roboticstoolbox-python`.

5. **Wikipedia**: Denavit-Hartenberg parameters.
   - https://en.wikipedia.org/wiki/Denavit%E2%80%93Hartenberg_parameters
   - 두 Convention의 비교 및 다이어그램 참조.

---

> *이 문서는 KineTutor3D 프로젝트의 수학적 기초를 정의한다.*
> *코드 구현은 반드시 이 문서의 공식과 일치해야 하며,*
> *EditMode 테스트에서 Section 6의 불변량이 검증되어야 한다.*
