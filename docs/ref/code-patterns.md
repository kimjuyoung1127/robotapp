# KineTutor3D C# 코딩 패턴 레퍼런스

> 이 문서는 KineTutor3D Unity 프로젝트에서 반복적으로 사용되는 C# 코딩 패턴을 정리한 것입니다.
> 각 패턴은 독립적이며, 구현 시 복사-붙여넣기 템플릿으로 활용할 수 있습니다.

---

## 1. readonly struct 보일러플레이트

`readonly struct`는 불변(immutable) 값 타입을 정의할 때 사용합니다.
힙 할당 없이 스택에서 동작하며, 수학 연산이 빈번한 로보틱스 계산에 적합합니다.

### Vec3D 전체 구현 예제

```csharp
using System;
using System.Globalization;

namespace KineTutor3D.Math
{
    /// <summary>
    /// 3차원 벡터를 나타내는 불변 값 타입.
    /// 모든 필드는 readonly이며, 생성 후 변경할 수 없습니다.
    /// </summary>
    public readonly struct Vec3D : IEquatable<Vec3D>
    {
        // ── 필드 ──────────────────────────────────────────
        public readonly double X;
        public readonly double Y;
        public readonly double Z;

        // ── 상수 ──────────────────────────────────────────
        /// <summary>Equals 비교에 사용하는 기본 허용 오차.</summary>
        public const double DefaultTolerance = 1e-10;

        // ── 팩토리 메서드 (자주 쓰는 벡터) ────────────────
        public static Vec3D Zero  => new(0, 0, 0);
        public static Vec3D UnitX => new(1, 0, 0);
        public static Vec3D UnitY => new(0, 1, 0);
        public static Vec3D UnitZ => new(0, 0, 1);

        // ── 생성자 (NaN/Infinity 가드 포함) ───────────────
        public Vec3D(double x, double y, double z)
        {
            GuardValue(x, nameof(x));
            GuardValue(y, nameof(y));
            GuardValue(z, nameof(z));

            X = x;
            Y = y;
            Z = z;
        }

        // ── NaN/Infinity 가드 ─────────────────────────────
        private static void GuardValue(double value, string paramName)
        {
            if (double.IsNaN(value) || double.IsInfinity(value))
                throw new ArgumentException(
                    $"유효하지 않은 값: {value}", paramName);
        }

        // ── 산술 연산자 ───────────────────────────────────
        public static Vec3D operator +(Vec3D a, Vec3D b)
            => new(a.X + b.X, a.Y + b.Y, a.Z + b.Z);

        public static Vec3D operator -(Vec3D a, Vec3D b)
            => new(a.X - b.X, a.Y - b.Y, a.Z - b.Z);

        public static Vec3D operator -(Vec3D v)
            => new(-v.X, -v.Y, -v.Z);

        public static Vec3D operator *(Vec3D v, double s)
            => new(v.X * s, v.Y * s, v.Z * s);

        public static Vec3D operator *(double s, Vec3D v)
            => new(v.X * s, v.Y * s, v.Z * s);

        // ── 벡터 연산 ─────────────────────────────────────
        public double Magnitude()
            => System.Math.Sqrt(X * X + Y * Y + Z * Z);

        public Vec3D Normalized()
        {
            double mag = Magnitude();
            if (mag < DefaultTolerance)
                throw new InvalidOperationException("영벡터는 정규화할 수 없습니다.");
            return this * (1.0 / mag);
        }

        public double Dot(Vec3D other)
            => X * other.X + Y * other.Y + Z * other.Z;

        public Vec3D Cross(Vec3D other)
            => new(
                Y * other.Z - Z * other.Y,
                Z * other.X - X * other.Z,
                X * other.Y - Y * other.X);

        // ── IEquatable<Vec3D> (허용 오차 기반) ────────────
        public bool Equals(Vec3D other)
            => Equals(other, DefaultTolerance);

        public bool Equals(Vec3D other, double tolerance)
            => System.Math.Abs(X - other.X) < tolerance
            && System.Math.Abs(Y - other.Y) < tolerance
            && System.Math.Abs(Z - other.Z) < tolerance;

        public override bool Equals(object obj)
            => obj is Vec3D other && Equals(other);

        public static bool operator ==(Vec3D a, Vec3D b) => a.Equals(b);
        public static bool operator !=(Vec3D a, Vec3D b) => !a.Equals(b);

        // ── GetHashCode ───────────────────────────────────
        public override int GetHashCode()
            => HashCode.Combine(
                System.Math.Round(X, 8),
                System.Math.Round(Y, 8),
                System.Math.Round(Z, 8));

        // ── ToString ──────────────────────────────────────
        public override string ToString()
            => ToString("F4");

        public string ToString(string format)
            => $"({X.ToString(format, CultureInfo.InvariantCulture)}, "
             + $"{Y.ToString(format, CultureInfo.InvariantCulture)}, "
             + $"{Z.ToString(format, CultureInfo.InvariantCulture)})";
    }
}
```

### 핵심 설계 원칙

| 항목 | 규칙 |
|------|------|
| `readonly struct` | 모든 필드가 `readonly`임을 컴파일러가 보장 |
| 생성자 가드 | NaN/Infinity 값 즉시 차단 |
| 연산자 결과 | 항상 **새 인스턴스** 반환 (불변성 유지) |
| Equals 허용 오차 | 부동소수점 비교 시 `1e-10` 기본 사용 |
| GetHashCode 라운딩 | Equals와 일관성을 위해 8자리 반올림 |

---

## 2. Mat3D / Mat4D 행렬 패턴

4x4 동차 변환 행렬(Homogeneous Transformation Matrix)은 로보틱스 FK/IK 계산의 핵심입니다.
행 우선(row-major) 1차원 배열로 저장하여 캐시 효율성을 확보합니다.

### Mat4D 구현 템플릿

```csharp
using System;

namespace KineTutor3D.Math
{
    /// <summary>
    /// 4x4 동차 변환 행렬. 행 우선(row-major) flat 배열로 저장합니다.
    /// 인덱싱: _m[row * 4 + col]
    /// </summary>
    public readonly struct Mat4D : IEquatable<Mat4D>
    {
        // ── 저장소: 16개 요소의 flat 배열 ─────────────────
        private readonly double[] _m;

        // ── 인덱서 ────────────────────────────────────────
        /// <summary>행(row)과 열(col)로 요소에 접근합니다 (0-based).</summary>
        public double this[int row, int col]
        {
            get
            {
                if (row < 0 || row > 3 || col < 0 || col > 3)
                    throw new IndexOutOfRangeException(
                        $"인덱스 범위 초과: [{row}, {col}]");
                return _m[row * 4 + col];
            }
        }

        // ── 팩토리: 단위 행렬 ─────────────────────────────
        public static Mat4D Identity => new(new double[]
        {
            1, 0, 0, 0,
            0, 1, 0, 0,
            0, 0, 1, 0,
            0, 0, 0, 1
        });

        // ── 생성자 ────────────────────────────────────────
        /// <summary>
        /// 16개 요소의 행 우선 배열로 행렬을 생성합니다.
        /// </summary>
        public Mat4D(double[] elements)
        {
            if (elements == null || elements.Length != 16)
                throw new ArgumentException("16개의 요소가 필요합니다.");

            for (int i = 0; i < 16; i++)
            {
                if (double.IsNaN(elements[i]) || double.IsInfinity(elements[i]))
                    throw new ArgumentException(
                        $"유효하지 않은 값: elements[{i}] = {elements[i]}");
            }

            _m = new double[16];
            Array.Copy(elements, _m, 16);
        }

        // ── 행렬 곱셈: Mat4D × Mat4D ─────────────────────
        public static Mat4D operator *(Mat4D a, Mat4D b)
        {
            var result = new double[16];
            for (int row = 0; row < 4; row++)
            {
                for (int col = 0; col < 4; col++)
                {
                    double sum = 0;
                    for (int k = 0; k < 4; k++)
                        sum += a[row, k] * b[k, col];
                    result[row * 4 + col] = sum;
                }
            }
            return new Mat4D(result);
        }

        // ── 행렬 × 벡터 (위치 변환) ──────────────────────
        /// <summary>
        /// 4x4 행렬로 3D 벡터를 변환합니다 (w=1 동차 좌표 가정).
        /// </summary>
        public Vec3D TransformPoint(Vec3D p)
        {
            double x = _m[0] * p.X + _m[1] * p.Y + _m[2]  * p.Z + _m[3];
            double y = _m[4] * p.X + _m[5] * p.Y + _m[6]  * p.Z + _m[7];
            double z = _m[8] * p.X + _m[9] * p.Y + _m[10] * p.Z + _m[11];
            return new Vec3D(x, y, z);
        }

        /// <summary>
        /// 4x4 행렬로 방향 벡터를 변환합니다 (w=0, 이동 무시).
        /// </summary>
        public Vec3D TransformDirection(Vec3D d)
        {
            double x = _m[0] * d.X + _m[1] * d.Y + _m[2]  * d.Z;
            double y = _m[4] * d.X + _m[5] * d.Y + _m[6]  * d.Z;
            double z = _m[8] * d.X + _m[9] * d.Y + _m[10] * d.Z;
            return new Vec3D(x, y, z);
        }

        // ── 회전 행렬 추출 (3x3 부분) ─────────────────────
        /// <summary>
        /// 4x4 행렬에서 좌상단 3x3 회전 행렬을 추출합니다.
        /// </summary>
        public Mat3D ExtractRotation()
        {
            return new Mat3D(new double[]
            {
                _m[0], _m[1], _m[2],
                _m[4], _m[5], _m[6],
                _m[8], _m[9], _m[10]
            });
        }

        // ── 위치 추출 ─────────────────────────────────────
        /// <summary>
        /// 4x4 행렬에서 이동(translation) 벡터를 추출합니다.
        /// 4번째 열의 상위 3개 요소 [0,3], [1,3], [2,3].
        /// </summary>
        public Vec3D ExtractPosition()
        {
            return new Vec3D(_m[3], _m[7], _m[11]);
        }

        // ── 동차 변환 행렬의 역행렬 ──────────────────────
        /// <summary>
        /// 동차 변환 행렬의 역행렬을 계산합니다.
        /// 회전 부분은 전치(R^T), 이동 부분은 -R^T * p.
        /// 주의: 이 메서드는 정규 직교 회전 행렬을 전제로 합니다.
        /// </summary>
        public Mat4D InverseHomogeneous()
        {
            // R^T (회전 전치)
            double r00 = _m[0], r01 = _m[4], r02 = _m[8];
            double r10 = _m[1], r11 = _m[5], r12 = _m[9];
            double r20 = _m[2], r21 = _m[6], r22 = _m[10];

            // 원래 이동 벡터
            double px = _m[3], py = _m[7], pz = _m[11];

            // -R^T * p
            double tx = -(r00 * px + r01 * py + r02 * pz);
            double ty = -(r10 * px + r11 * py + r12 * pz);
            double tz = -(r20 * px + r21 * py + r22 * pz);

            return new Mat4D(new double[]
            {
                r00, r01, r02, tx,
                r10, r11, r12, ty,
                r20, r21, r22, tz,
                  0,   0,   0,  1
            });
        }

        // ── IEquatable<Mat4D> ─────────────────────────────
        public bool Equals(Mat4D other)
            => Equals(other, 1e-10);

        public bool Equals(Mat4D other, double tolerance)
        {
            if (_m == null || other._m == null) return false;
            for (int i = 0; i < 16; i++)
            {
                if (System.Math.Abs(_m[i] - other._m[i]) >= tolerance)
                    return false;
            }
            return true;
        }

        public override bool Equals(object obj)
            => obj is Mat4D other && Equals(other);

        public override int GetHashCode()
        {
            var hash = new HashCode();
            for (int i = 0; i < 16; i++)
                hash.Add(System.Math.Round(_m[i], 8));
            return hash.ToHashCode();
        }
    }
}
```

### Mat3D 최소 구현 (회전 전용)

```csharp
namespace KineTutor3D.Math
{
    /// <summary>3x3 회전 행렬. 행 우선 flat 배열 저장.</summary>
    public readonly struct Mat3D
    {
        private readonly double[] _m;

        public double this[int row, int col] => _m[row * 3 + col];

        public Mat3D(double[] elements)
        {
            if (elements == null || elements.Length != 9)
                throw new ArgumentException("9개의 요소가 필요합니다.");
            _m = new double[9];
            Array.Copy(elements, _m, 9);
        }

        public static Mat3D Identity => new(new double[]
        {
            1, 0, 0,
            0, 1, 0,
            0, 0, 1
        });

        /// <summary>전치 행렬을 반환합니다 (직교 행렬의 역행렬).</summary>
        public Mat3D Transpose()
        {
            return new Mat3D(new double[]
            {
                _m[0], _m[3], _m[6],
                _m[1], _m[4], _m[7],
                _m[2], _m[5], _m[8]
            });
        }
    }
}
```

---

## 3. NaN/Infinity 가드 정책

### 정책 개요

| 항목 | 내용 |
|------|------|
| **적용 위치** | 수학 타입(Math/, Types/)의 **모든 public 생성자** |
| **검사 대상** | `double.IsNaN()` 및 `double.IsInfinity()` |
| **예외 타입** | `ArgumentException` |
| **메시지 언어** | 한국어 (디버깅 편의) |
| **적용 경계** | Math/, Types/ 모듈에만 적용. UI 레이어는 입력 단계에서 별도로 거부 |

### 가드 코드 패턴

```csharp
/// <summary>
/// 값의 유효성을 검증합니다. NaN 또는 Infinity이면 예외를 발생시킵니다.
/// </summary>
private static void GuardValue(double value, string paramName)
{
    if (double.IsNaN(value) || double.IsInfinity(value))
        throw new ArgumentException(
            $"유효하지 않은 값: {value}", paramName);
}
```

### 단일 값 가드 (인라인)

```csharp
public readonly struct JointAngle
{
    public readonly double Radians;

    public JointAngle(double radians)
    {
        if (double.IsNaN(radians) || double.IsInfinity(radians))
            throw new ArgumentException(
                $"유효하지 않은 값: {radians}", nameof(radians));

        Radians = radians;
    }
}
```

### 배열 가드 (행렬 등)

```csharp
public Mat4D(double[] elements)
{
    if (elements == null || elements.Length != 16)
        throw new ArgumentException("16개의 요소가 필요합니다.");

    for (int i = 0; i < 16; i++)
    {
        if (double.IsNaN(elements[i]) || double.IsInfinity(elements[i]))
            throw new ArgumentException(
                $"유효하지 않은 값: elements[{i}] = {elements[i]}");
    }

    _m = new double[16];
    Array.Copy(elements, _m, 16);
}
```

### 가드가 필요한 이유

```
[관절 각도 입력] → [DH 매개변수] → [삼각함수 계산] → [변환 행렬] → [FK 체인 곱셈]
       ↑                                                              ↓
   NaN이 여기서        NaN이 전파되면 여기서 최종 결과가
   들어오면...         무의미한 값이 됩니다 (silent failure)
```

가드가 없으면 NaN이 FK 체인을 따라 조용히 전파되어, 최종 위치/자세 결과가 무의미해집니다.
생성자에서 즉시 차단하면 **오류 발생 지점을 정확히 특정**할 수 있습니다.

---

## 4. NUnit EditMode 테스트 보일러플레이트

### 프로젝트 구조

```
Assets/
└── Tests/
    └── EditMode/
        ├── KineTutor3D.Tests.EditMode.asmdef    ← 어셈블리 정의
        ├── Vec3DTests.cs
        ├── Mat4DTests.cs
        └── DHStandardTests.cs
```

### 어셈블리 정의 (asmdef) 필수 참조

```json
{
    "name": "KineTutor3D.Tests.EditMode",
    "references": [
        "KineTutor3D.Math"
    ],
    "optionalUnityReferences": [
        "TestAssemblies"
    ],
    "includePlatforms": [
        "Editor"
    ]
}
```

### 네이밍 규칙

| 항목 | 규칙 | 예시 |
|------|------|------|
| 파일명 | `{대상클래스}Tests.cs` | `Vec3DTests.cs` |
| 클래스명 | `{대상클래스}Tests` | `Vec3DTests` |
| 메서드명 | `MethodName_Condition_ExpectedResult` | `Add_TwoVectors_ReturnsSum` |

### 허용 오차(Delta) 기준표

| 용도 | Delta 값 | 비고 |
|------|----------|------|
| 순수 수학 연산 | `1e-10` | 삼각함수, 행렬 곱셈 등 |
| 회전 관련 | `1e-6` | 오일러각, 쿼터니언 변환 |
| 위치/좌표 | `1e-4` | FK 결과 위치 비교 |

### Vec3D 테스트 클래스 전체 예제

```csharp
using NUnit.Framework;
using KineTutor3D.Math;
using System;

namespace KineTutor3D.Tests.EditMode
{
    [TestFixture]
    public class Vec3DTests
    {
        // ── 허용 오차 상수 ────────────────────────────────
        private const double MathDelta    = 1e-10;
        private const double PositionDelta = 1e-4;

        // ── 생성자 테스트 ─────────────────────────────────

        [Test]
        public void Constructor_ValidValues_CreatesVector()
        {
            var v = new Vec3D(1.0, 2.0, 3.0);

            Assert.AreEqual(1.0, v.X, MathDelta);
            Assert.AreEqual(2.0, v.Y, MathDelta);
            Assert.AreEqual(3.0, v.Z, MathDelta);
        }

        [Test]
        public void Constructor_NaNValue_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() =>
                new Vec3D(double.NaN, 0, 0));
        }

        [Test]
        public void Constructor_InfinityValue_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() =>
                new Vec3D(0, double.PositiveInfinity, 0));
        }

        [Test]
        public void Constructor_NegativeInfinity_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() =>
                new Vec3D(0, 0, double.NegativeInfinity));
        }

        // ── 산술 연산 테스트 ──────────────────────────────

        [Test]
        public void Add_TwoVectors_ReturnsSum()
        {
            var a = new Vec3D(1, 2, 3);
            var b = new Vec3D(4, 5, 6);

            var result = a + b;

            Assert.AreEqual(5.0, result.X, MathDelta);
            Assert.AreEqual(7.0, result.Y, MathDelta);
            Assert.AreEqual(9.0, result.Z, MathDelta);
        }

        [Test]
        public void Subtract_TwoVectors_ReturnsDifference()
        {
            var a = new Vec3D(5, 7, 9);
            var b = new Vec3D(1, 2, 3);

            var result = a - b;

            Assert.AreEqual(4.0, result.X, MathDelta);
            Assert.AreEqual(5.0, result.Y, MathDelta);
            Assert.AreEqual(6.0, result.Z, MathDelta);
        }

        [Test]
        public void UnaryMinus_Vector_ReturnsNegated()
        {
            var v = new Vec3D(1, -2, 3);

            var result = -v;

            Assert.AreEqual(-1.0, result.X, MathDelta);
            Assert.AreEqual( 2.0, result.Y, MathDelta);
            Assert.AreEqual(-3.0, result.Z, MathDelta);
        }

        [Test]
        public void Multiply_VectorByScalar_ReturnsScaled()
        {
            var v = new Vec3D(1, 2, 3);

            var result = v * 2.0;

            Assert.AreEqual(2.0, result.X, MathDelta);
            Assert.AreEqual(4.0, result.Y, MathDelta);
            Assert.AreEqual(6.0, result.Z, MathDelta);
        }

        [Test]
        public void Multiply_ScalarByVector_ReturnsScaled()
        {
            var v = new Vec3D(1, 2, 3);

            var result = 3.0 * v;

            Assert.AreEqual(3.0, result.X, MathDelta);
            Assert.AreEqual(6.0, result.Y, MathDelta);
            Assert.AreEqual(9.0, result.Z, MathDelta);
        }

        // ── 벡터 연산 테스트 ──────────────────────────────

        [Test]
        public void Magnitude_UnitVector_ReturnsOne()
        {
            var v = Vec3D.UnitX;
            Assert.AreEqual(1.0, v.Magnitude(), MathDelta);
        }

        [Test]
        public void Magnitude_KnownVector_ReturnsCorrectLength()
        {
            var v = new Vec3D(3, 4, 0);
            Assert.AreEqual(5.0, v.Magnitude(), MathDelta);
        }

        [Test]
        public void Dot_PerpendicularVectors_ReturnsZero()
        {
            var a = Vec3D.UnitX;
            var b = Vec3D.UnitY;
            Assert.AreEqual(0.0, a.Dot(b), MathDelta);
        }

        [Test]
        public void Cross_XcrossY_ReturnsZ()
        {
            var result = Vec3D.UnitX.Cross(Vec3D.UnitY);

            Assert.AreEqual(0.0, result.X, MathDelta);
            Assert.AreEqual(0.0, result.Y, MathDelta);
            Assert.AreEqual(1.0, result.Z, MathDelta);
        }

        // ── 동등성 테스트 ─────────────────────────────────

        [Test]
        public void Equals_SameValues_ReturnsTrue()
        {
            var a = new Vec3D(1, 2, 3);
            var b = new Vec3D(1, 2, 3);
            Assert.IsTrue(a.Equals(b));
        }

        [Test]
        public void Equals_WithinTolerance_ReturnsTrue()
        {
            var a = new Vec3D(1.0, 2.0, 3.0);
            var b = new Vec3D(1.0 + 1e-11, 2.0, 3.0);
            Assert.IsTrue(a.Equals(b));
        }

        [Test]
        public void Equals_OutsideTolerance_ReturnsFalse()
        {
            var a = new Vec3D(1.0, 2.0, 3.0);
            var b = new Vec3D(1.1, 2.0, 3.0);
            Assert.IsFalse(a.Equals(b));
        }

        // ── ToString 테스트 ───────────────────────────────

        [Test]
        public void ToString_Default_ReturnsFormattedString()
        {
            var v = new Vec3D(1.5, 2.5, 3.5);
            Assert.AreEqual("(1.5000, 2.5000, 3.5000)", v.ToString());
        }

        // ── 팩토리 메서드 테스트 ──────────────────────────

        [Test]
        public void Zero_AllComponentsAreZero()
        {
            var v = Vec3D.Zero;
            Assert.AreEqual(0.0, v.X, MathDelta);
            Assert.AreEqual(0.0, v.Y, MathDelta);
            Assert.AreEqual(0.0, v.Z, MathDelta);
        }
    }
}
```

---

## 5. 행렬 비교 헬퍼 패턴

행렬 비교는 단순 `Assert.AreEqual`로는 부족합니다.
요소별 비교와 실패 시 상세 메시지가 필요합니다.

### AssertMatrixEqual 헬퍼 구현

```csharp
using NUnit.Framework;
using KineTutor3D.Math;

namespace KineTutor3D.Tests.EditMode
{
    /// <summary>
    /// 행렬 비교를 위한 테스트 헬퍼 클래스.
    /// </summary>
    public static class MatrixAssert
    {
        /// <summary>
        /// 두 4x4 행렬의 모든 요소를 허용 오차 내에서 비교합니다.
        /// 실패 시 어느 요소에서 차이가 발생했는지 상세히 보고합니다.
        /// </summary>
        public static void AreEqual(
            Mat4D expected,
            Mat4D actual,
            double tolerance,
            string message = "")
        {
            for (int row = 0; row < 4; row++)
            {
                for (int col = 0; col < 4; col++)
                {
                    double diff = System.Math.Abs(
                        expected[row, col] - actual[row, col]);

                    if (diff >= tolerance)
                    {
                        Assert.Fail(
                            $"{message}\n" +
                            $"행렬 요소 [{row},{col}] 불일치.\n" +
                            $"  기대값:  {expected[row, col]:G15}\n" +
                            $"  실제값:  {actual[row, col]:G15}\n" +
                            $"  차이:    {diff:G15}\n" +
                            $"  허용오차: {tolerance:G15}");
                    }
                }
            }
        }

        /// <summary>
        /// 행렬이 단위 행렬인지 검증합니다.
        /// </summary>
        public static void IsIdentity(Mat4D actual, double tolerance)
        {
            AreEqual(Mat4D.Identity, actual, tolerance, "단위 행렬이 아닙니다.");
        }
    }
}
```

### 허용 오차 상수 정의 패턴

```csharp
namespace KineTutor3D.Tests.EditMode
{
    /// <summary>
    /// 테스트에서 사용하는 허용 오차 상수 모음.
    /// </summary>
    public static class TestTolerances
    {
        /// <summary>순수 수학 연산 (삼각함수, 행렬 곱셈).</summary>
        public const double Math     = 1e-10;

        /// <summary>위치/좌표 비교 (FK 결과).</summary>
        public const double Position = 1e-4;

        /// <summary>회전 관련 비교 (회전 행렬 요소, 오일러각).</summary>
        public const double Rotation = 1e-6;
    }
}
```

### DH 변환 테스트에서의 사용 예시

```csharp
[TestFixture]
public class DHStandardTests
{
    [Test]
    public void ComputeA_ZeroParameters_ReturnsIdentity()
    {
        // theta=0, d=0, a=0, alpha=0이면 단위 행렬이어야 합니다.
        var result = DHStandard.ComputeA(
            theta: 0, d: 0, a: 0, alpha: 0);

        MatrixAssert.IsIdentity(result, TestTolerances.Math);
    }

    [Test]
    public void ComputeA_ThetaPi2_CorrectRotation()
    {
        // theta=90도, 나머지 0: Z축 기준 90도 회전
        double theta = System.Math.PI / 2.0;

        var result = DHStandard.ComputeA(
            theta: theta, d: 0, a: 0, alpha: 0);

        var expected = new Mat4D(new double[]
        {
            0, -1, 0, 0,
            1,  0, 0, 0,
            0,  0, 1, 0,
            0,  0, 0, 1
        });

        MatrixAssert.AreEqual(expected, result, TestTolerances.Math,
            "theta=PI/2 회전 행렬 검증");
    }

    [Test]
    public void ForwardKinematics_TwoLinkPlanar_CorrectEndEffector()
    {
        // 2-링크 평면 로봇: 링크 길이 각각 1.0
        var links = new DHLink[]
        {
            new(theta: 0, d: 0, a: 1.0, alpha: 0),
            new(theta: 0, d: 0, a: 1.0, alpha: 0),
        };
        var jointValues = new double[] { 0, 0 };

        var T = DHStandard.ForwardKinematics(links, jointValues);
        var pos = T.ExtractPosition();

        // 두 링크가 모두 0도일 때: x=2, y=0, z=0
        Assert.AreEqual(2.0, pos.X, TestTolerances.Position);
        Assert.AreEqual(0.0, pos.Y, TestTolerances.Position);
        Assert.AreEqual(0.0, pos.Z, TestTolerances.Position);
    }
}
```

---

## 6. DH 알고리즘 구현 패턴

Denavit-Hartenberg 표준 방식의 순운동학(Forward Kinematics) 구현 패턴입니다.
모든 함수는 **순수 함수(pure function)** 원칙을 따릅니다.

### DHLink 데이터 구조

```csharp
namespace KineTutor3D.Math
{
    /// <summary>
    /// DH 링크 매개변수를 나타내는 불변 구조체.
    /// 생성 후에는 어떤 값도 변경할 수 없습니다.
    /// </summary>
    public readonly struct DHLink
    {
        /// <summary>관절 각도 오프셋 (회전 관절: 이 값에 관절 변수를 더합니다).</summary>
        public readonly double Theta;

        /// <summary>링크 오프셋 (Z축 방향 이동).</summary>
        public readonly double D;

        /// <summary>링크 길이 (X축 방향 이동).</summary>
        public readonly double A;

        /// <summary>링크 비틀림 (X축 기준 회전).</summary>
        public readonly double Alpha;

        public DHLink(double theta, double d, double a, double alpha)
        {
            if (double.IsNaN(theta) || double.IsInfinity(theta))
                throw new ArgumentException($"유효하지 않은 값: {theta}", nameof(theta));
            if (double.IsNaN(d) || double.IsInfinity(d))
                throw new ArgumentException($"유효하지 않은 값: {d}", nameof(d));
            if (double.IsNaN(a) || double.IsInfinity(a))
                throw new ArgumentException($"유효하지 않은 값: {a}", nameof(a));
            if (double.IsNaN(alpha) || double.IsInfinity(alpha))
                throw new ArgumentException($"유효하지 않은 값: {alpha}", nameof(alpha));

            Theta = theta;
            D = d;
            A = a;
            Alpha = alpha;
        }
    }
}
```

### DHStandard.cs 기본 구조

```csharp
using System;

namespace KineTutor3D.Math
{
    /// <summary>
    /// 표준 DH(Denavit-Hartenberg) 변환을 구현하는 정적 클래스.
    ///
    /// 설계 원칙:
    /// - 모든 메서드는 static이며, 상태(state)를 갖지 않습니다.
    /// - 입력에 대해 항상 같은 출력을 반환합니다 (순수 함수).
    /// - 부작용(side effect)이 없습니다.
    /// </summary>
    public static class DHStandard
    {
        /// <summary>
        /// 단일 DH 링크에 대한 동차 변환 행렬 A_i를 계산합니다.
        ///
        /// 표준 DH 변환 행렬:
        /// A = Rz(theta) * Tz(d) * Tx(a) * Rx(alpha)
        ///
        ///     [ cos(θ)  -sin(θ)cos(α)   sin(θ)sin(α)  a·cos(θ) ]
        /// A = [ sin(θ)   cos(θ)cos(α)  -cos(θ)sin(α)  a·sin(θ) ]
        ///     [   0        sin(α)          cos(α)          d     ]
        ///     [   0          0                0            1     ]
        /// </summary>
        /// <param name="theta">관절 각도 (라디안)</param>
        /// <param name="d">링크 오프셋</param>
        /// <param name="a">링크 길이</param>
        /// <param name="alpha">링크 비틀림 (라디안)</param>
        /// <returns>4x4 동차 변환 행렬</returns>
        public static Mat4D ComputeA(
            double theta, double d, double a, double alpha)
        {
            double ct = System.Math.Cos(theta);
            double st = System.Math.Sin(theta);
            double ca = System.Math.Cos(alpha);
            double sa = System.Math.Sin(alpha);

            return new Mat4D(new double[]
            {
                ct, -st * ca,  st * sa, a * ct,
                st,  ct * ca, -ct * sa, a * st,
                 0,       sa,       ca,      d,
                 0,        0,        0,      1
            });
        }

        /// <summary>
        /// DH 링크 체인에 대한 순운동학을 계산합니다.
        /// T = A_1 * A_2 * ... * A_n
        ///
        /// 순수 함수: 입력 배열을 변경하지 않으며, 부작용이 없습니다.
        /// </summary>
        /// <param name="links">DH 링크 매개변수 배열</param>
        /// <param name="jointValues">각 관절의 현재 값 (라디안 또는 길이)</param>
        /// <returns>베이스에서 엔드이펙터까지의 변환 행렬</returns>
        public static Mat4D ForwardKinematics(
            DHLink[] links, double[] jointValues)
        {
            if (links == null)
                throw new ArgumentNullException(nameof(links));
            if (jointValues == null)
                throw new ArgumentNullException(nameof(jointValues));
            if (links.Length != jointValues.Length)
                throw new ArgumentException(
                    $"링크 수({links.Length})와 관절 값 수({jointValues.Length})가 " +
                    $"일치하지 않습니다.");

            Mat4D result = Mat4D.Identity;

            for (int i = 0; i < links.Length; i++)
            {
                // 회전 관절: theta에 관절 변수를 더합니다
                double theta = links[i].Theta + jointValues[i];

                Mat4D Ai = ComputeA(
                    theta,
                    links[i].D,
                    links[i].A,
                    links[i].Alpha);

                result = result * Ai;
            }

            return result;
        }
    }
}
```

### 순수 함수 패턴 체크리스트

```
[체크리스트] DHStandard 구현 검증 항목

[x] 모든 메서드가 static인가?
[x] 클래스에 인스턴스 필드가 없는가?
[x] 입력 배열을 수정하지 않는가?
[x] 같은 입력에 항상 같은 출력을 반환하는가?
[x] Unity API (MonoBehaviour, Transform 등)에 의존하지 않는가?
[x] 외부 상태를 읽거나 쓰지 않는가?
```

---

## 7. 연산자 오버로딩 규칙

### 규칙 요약표

| 규칙 | 설명 | 예시 |
|------|------|------|
| 새 인스턴스 반환 | 이항 연산자는 항상 새 인스턴스를 반환 | `a + b` -> `new Vec3D(...)` |
| 양방향 스칼라 곱셈 | `Vec * scalar`와 `scalar * Vec` 모두 지원 | `v * 2.0`, `2.0 * v` |
| 암시적 변환 금지 | 타입 간 `implicit` 변환 연산자 사용 금지 | `Vec3D` -> `Vector3` 암시적 변환 없음 |
| 값 기반 동등성 | 허용 오차 내에서 값이 같으면 동등 | `a == b` (tolerance 기반) |

### 이항 연산자: 새 인스턴스 반환 (불변성)

```csharp
// 올바른 패턴: 새 인스턴스를 반환합니다
public static Vec3D operator +(Vec3D a, Vec3D b)
    => new(a.X + b.X, a.Y + b.Y, a.Z + b.Z);

// 잘못된 패턴: readonly struct에서는 불가능하지만, 원칙적으로 금지
// public static Vec3D operator +(Vec3D a, Vec3D b)
// {
//     a.X += b.X;  // 컴파일 오류 (readonly)
//     return a;
// }
```

### 양방향 스칼라 곱셈

```csharp
// Vec3D * double
public static Vec3D operator *(Vec3D v, double s)
    => new(v.X * s, v.Y * s, v.Z * s);

// double * Vec3D (교환 법칙 지원)
public static Vec3D operator *(double s, Vec3D v)
    => new(v.X * s, v.Y * s, v.Z * s);
```

두 연산자를 모두 정의하지 않으면 `2.0 * v` 같은 자연스러운 수식을 쓸 수 없습니다.

### 단항 부정 연산자

```csharp
public static Vec3D operator -(Vec3D v)
    => new(-v.X, -v.Y, -v.Z);
```

### 암시적 변환 금지 원칙

```csharp
// 금지: 암시적 변환은 의도치 않은 타입 변환을 유발합니다
// public static implicit operator Vector3(Vec3D v) => ...;

// 허용: 명시적 변환은 의도를 명확히 합니다
public static explicit operator UnityEngine.Vector3(Vec3D v)
    => new((float)v.X, (float)v.Y, (float)v.Z);

// 권장: 변환 메서드가 가장 명확합니다
public UnityEngine.Vector3 ToUnityVector3()
    => new((float)X, (float)Y, (float)Z);
```

### 동등성 연산자 (허용 오차 기반)

```csharp
// IEquatable<T> 구현
public bool Equals(Vec3D other)
    => Equals(other, DefaultTolerance);

public bool Equals(Vec3D other, double tolerance)
    => System.Math.Abs(X - other.X) < tolerance
    && System.Math.Abs(Y - other.Y) < tolerance
    && System.Math.Abs(Z - other.Z) < tolerance;

// == 및 != 연산자
public static bool operator ==(Vec3D a, Vec3D b) => a.Equals(b);
public static bool operator !=(Vec3D a, Vec3D b) => !a.Equals(b);

// object.Equals 오버라이드 (박싱된 비교 지원)
public override bool Equals(object obj)
    => obj is Vec3D other && Equals(other);

// GetHashCode: Equals와 일관성 유지를 위해 반올림 사용
public override int GetHashCode()
    => HashCode.Combine(
        System.Math.Round(X, 8),
        System.Math.Round(Y, 8),
        System.Math.Round(Z, 8));
```

> **주의**: 허용 오차 기반 Equals와 GetHashCode의 일관성을 완벽히 보장하기는 어렵습니다.
> `Round(value, 8)`은 `1e-10` 허용 오차에 대해 실용적으로 충분한 수준의 일관성을 제공합니다.
> Dictionary 키 등 해시 기반 컬렉션에 사용할 때는 이 제약을 인지하고 사용하세요.

### 행렬 연산자 패턴

```csharp
// 행렬 × 행렬: 항상 새 Mat4D를 반환합니다
public static Mat4D operator *(Mat4D a, Mat4D b)
{
    var result = new double[16];
    for (int row = 0; row < 4; row++)
        for (int col = 0; col < 4; col++)
        {
            double sum = 0;
            for (int k = 0; k < 4; k++)
                sum += a[row, k] * b[k, col];
            result[row * 4 + col] = sum;
        }
    return new Mat4D(result);
}

// 행렬 동등성: 요소별 허용 오차 비교
public bool Equals(Mat4D other, double tolerance)
{
    for (int i = 0; i < 16; i++)
        if (System.Math.Abs(_m[i] - other._m[i]) >= tolerance)
            return false;
    return true;
}
```

---

## 8. Unity 측 코딩 패턴 (App / UI / Visualization)

> 섹션 1-7은 순수 C# 모듈(Math, Types, Kinematics)을 다룹니다.
> 섹션 8은 Unity API를 사용하는 모듈(App, UI, Visualization)의 공통 규칙입니다.

---

### 8.1 파일 인코딩

| 항목 | 규칙 |
|------|------|
| 인코딩 | UTF-8 **with BOM** (`EF BB BF`) |
| 줄바꿈 | LF (`\n`) |
| 후행 공백 | 제거 |
| 파일 끝 | 빈 줄 1개 |

> **배경**: 현재 11개 파일에 EUC-KR/UTF-8 mojibake가 존재합니다.
> 새 파일은 반드시 UTF-8 BOM으로 생성하고, 기존 파일은 수정 시 인코딩을 교정합니다.

---

### 8.2 네이밍 규칙

```
namespace   KineTutor3D.{Module}          // PascalCase, 모듈명과 일치
class       PascalCase                     // 파일명 == 클래스명
method      PascalCase()                   // public/internal 메서드
field       camelCase                      // private 필드 (접두사 없음)
SerializeField  camelCase                  // [SerializeField] private — 접두사 없음
const       PascalCase                     // 또는 UPPER_SNAKE (기존 관례 유지)
event       On{Action}                     // 예: OnStepChanged, OnJointUpdated
```

**금지**: `_` 접두사, `m_` 헝가리안, `s_` 정적 접두사.

---

### 8.3 접근 제어자

| 패턴 | 용도 | 예시 |
|------|------|------|
| `public class` | 모듈 외부 공개 API | `SceneNavigator`, `StepProgressSaver` |
| `internal sealed class` | 모듈 내부 서비스 | `StepFlowService`, `KinematicsRuntimeService` |
| `public static class` | 순수 유틸리티 | `WhyItMovedFormatter`, `UiRuntimeStyle` |
| `internal class` (비봉인) | **금지** — `sealed` 필수 | — |

> 기본 원칙: **공개하지 않으면 `internal sealed`**.

---

### 8.4 MonoBehaviour 수명주기 패턴

```csharp
public class ExamplePanel : MonoBehaviour
{
    [SerializeField] private Button actionButton;
    private bool listenersBound;

    private void Awake()
    {
        EnsurePresentation();
        BindListeners();
    }

    private void OnEnable()
    {
        EnsurePresentation();
        BindListeners();
    }

    private void OnDisable()
    {
        UnbindListeners();
    }
}
```

**핵심 규칙**:
- `Awake` + `OnEnable`에서 초기화, `OnDisable`에서 정리
- `OnDestroy`는 `OnDisable`에서 처리하지 못하는 자원(네이티브 핸들 등)만
- `[ExecuteAlways]`를 사용하는 경우에도 동일 패턴 유지

---

### 8.5 초기화 메서드명

| 메서드 | 역할 |
|--------|------|
| `EnsurePresentation()` | UI 요소 생성/복원 (멱등성 보장) |
| `BindListeners()` | 이벤트/버튼 리스너 등록 |
| `UnbindListeners()` | 이벤트/버튼 리스너 해제 |

> `Init()`, `Setup()`, `Configure()` 등 대체 이름은 사용하지 않습니다.
> 기존 코드의 `BindButtons()`/`UnbindButtons()`는 `BindListeners()`/`UnbindListeners()`로 점진적 통일.

---

### 8.6 버튼/이벤트 바인딩 패턴

```csharp
private bool listenersBound;

private void BindListeners()
{
    if (listenersBound) return;

    if (actionButton != null)
        actionButton.onClick.AddListener(OnActionClicked);

    listenersBound = true;
}

private void UnbindListeners()
{
    if (!listenersBound) return;

    if (actionButton != null)
        actionButton.onClick.RemoveListener(OnActionClicked);

    listenersBound = false;
}
```

**핵심**: `listenersBound` 플래그로 중복 등록 방지. null 체크 후 Add/Remove.

---

### 8.7 SetVisible 패턴

```csharp
// 기본: gameObject.SetActive
public void SetVisible(bool visible)
{
    gameObject.SetActive(visible);
}

// 페이드가 필요한 경우: CanvasGroup
public void SetVisible(bool visible)
{
    canvasGroup.alpha = visible ? 1f : 0f;
    canvasGroup.blocksRaycasts = visible;
    canvasGroup.interactable = visible;
}
```

> 두 방식을 혼용하지 않습니다. 한 컴포넌트에서 하나만 선택합니다.

---

### 8.8 Material 캐싱 패턴

```csharp
private static Material sharedMaterial;

private static Material GetShared()
{
    if (sharedMaterial == null)
    {
        sharedMaterial = new Material(Shader.Find("..."));
        sharedMaterial.hideFlags = HideFlags.HideAndDontSave;
    }
    return sharedMaterial;
}
```

> `Shader.Find`는 비용이 높으므로 정적 캐시 필수.

---

### 8.9 GameObject 탐색 우선순위

1. **`[SerializeField]`** — 인스펙터 바인딩 (최우선)
2. **`transform.Find("ChildName")`** — 런타임 동적 생성 시
3. **`FindFirstObjectByType<T>()`** — 최후 수단 (성능 비용 높음)

> `GameObject.Find(string)`는 전역 검색이므로 **금지**.

---

### 8.10 XML 문서 언어 규칙

```csharp
/// <summary>
/// 관절 슬라이더 값 변경 시 호출됩니다.
/// </summary>
/// <param name="jointIndex">변경된 관절 인덱스.</param>
/// <exception cref="ArgumentOutOfRangeException">
/// jointIndex가 유효 범위를 벗어날 때.
/// </exception>
```

| 항목 | 언어 |
|------|------|
| `<summary>` | 한국어 |
| `<param>` | 한국어 |
| `<exception>` | 한국어 |
| 인라인 코드 주석 | 한국어 (필요 시만) |
| 테스트 `[Test]` 메서드명 | 영어 PascalCase |
| Assert 메시지 | 영어 (NUnit 호환) |

---

### 8.11 테스트 어설션 스타일

```csharp
// 수치 비교: Assert.AreEqual + delta
Assert.AreEqual(expected, actual, TestTolerances.Position, "message");

// 논리 비교: Assert.That + Is 제약조건
Assert.That(result.IsValid, Is.True, "message");

// 예외: Assert.Throws
Assert.Throws<ArgumentException>(() => BadCall());
```

> `Assert.IsTrue(a == b)` 대신 반드시 `Assert.AreEqual(a, b, delta)` 사용.

---

## 9. 파일 헤더 패턴

모든 C# 파일의 **1행**에 폴더 역할 주석을 작성합니다.

```csharp
// Folder: Math - Pure C# double-precision math; no UnityEngine references.
```

| 모듈 | 헤더 |
|------|------|
| Math | `// Folder: Math - Pure C# double-precision math; no UnityEngine references.` |
| Types | `// Folder: Types - Domain value types; no UnityEngine references.` |
| Kinematics | `// Folder: Kinematics - DH parameter and FK algorithms; no UnityEngine references.` |
| Templates | `// Folder: Templates - Robot configuration templates; no UnityEngine references.` |
| App | `// Folder: App - Application controllers and services; single UnityEngine entry point.` |
| UI | `// Folder: UI - HUD/view components only; no kinematics logic.` |
| Visualization | `// Folder: Visualization - 3D rendering helpers for robot joint/link display.` |

---

## 부록: 패턴 간 관계도

```
순수 C# 계층 (섹션 1-7)
═══════════════════════
Vec3D / Mat3D / Mat4D (readonly struct)
 ├── NaN/Infinity 가드 (생성자)
 ├── 연산자 오버로딩 (+, -, *, ==)
 └── IEquatable<T> (허용 오차 기반)

DHStandard (static class)
 ├── ComputeA → Mat4D 반환
 ├── ForwardKinematics → Mat4D 체인 곱셈
 └── 순수 함수 패턴

테스트 인프라
 ├── NUnit EditMode 보일러플레이트
 ├── MatrixAssert 헬퍼
 └── TestTolerances 상수

Unity 계층 (섹션 8-9)
═══════════════════════
MonoBehaviour (App/UI/Visualization)
 ├── 수명주기: Awake → OnEnable → OnDisable
 ├── EnsurePresentation() — 멱등 UI 초기화
 ├── BindListeners() / UnbindListeners() — 이벤트 바인딩
 └── SetVisible() — 가시성 제어

파일 인프라
 ├── UTF-8 BOM 인코딩
 ├── 1행 Folder 헤더
 └── 한국어 XML doc
```
