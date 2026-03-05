# Math/

Double 정밀도 수학 라이브러리 (기구학 계산용).

## 파일 (예정)
- `Vec3D.cs` — 3D 벡터 (double x, y, z)
- `Mat3D.cs` — 3×3 회전 행렬
- `Mat4D.cs` — 4×4 동차 변환 행렬

## 규칙
1. **순수 C#만** (`using UnityEngine` 절대 금지)
2. 모든 값은 `double` — `float` 절대 사용 금지
3. 생성자/연산자에서 NaN/Infinity 가드 필수
4. Factory 메서드 제공: `Identity`, `Zero`
5. 연산자 오버로딩: `+`, `-`, `*`, `==`
6. 모든 타입에 대응하는 EditMode 테스트 필수

## 관련 스킬
- `math-module-add` — 새 수학 타입 추가 시 사용
