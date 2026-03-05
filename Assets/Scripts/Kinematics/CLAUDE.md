# Kinematics/

DH 파라미터 알고리즘 및 Forward Kinematics 엔진.

## 파일 (예정)
- `DHStandard.cs` — 표준 DH: A_i = Rz(θ)·Tz(d)·Tx(a)·Rx(α) **(베이스라인)**
- `ForwardKinematics.cs` — 누적곱 T = A₁···Aₖ, R과 p 추출

## 규칙
1. **DHStandard.cs는 베이스라인** — 명시적 요청 없이 수정 금지
2. 새 DH 변형은 별도 파일 (DHModified.cs 등)
3. 모든 알고리즘은 XML doc에 수학 공식 참조 필수
4. 출력은 항상 `Mat4D`
5. 허용 오차: 위치 < 1e-4 m, 회전 < 1e-3 rad
6. 좌표 규약: 로보틱스 표준 (Unity 매핑은 별도 문서화)

## 관련 스킬
- `dh-algorithm-add` — 새 알고리즘 추가 시 사용
