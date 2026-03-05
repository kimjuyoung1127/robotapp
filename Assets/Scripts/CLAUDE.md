# Assets/Scripts/

KineTutor3D 소스코드 루트.

## 모듈 구조
- `Types/` — 도메인 타입 (JointType, DHLink, RobotTemplate, Pose)
- `Math/` — Double 정밀도 수학 (Vec3D, Mat3D, Mat4D)
- `Kinematics/` — DH 파라미터 및 FK 알고리즘
- `Templates/` — 로봇 설정 템플릿
- `UI/` — 사용자 인터페이스 패널
- `Visualization/` — 3D 렌더링 헬퍼
- `App/` — 애플리케이션 컨트롤러

## 규칙
1. 핵심 모듈(Types, Math, Kinematics)은 `using UnityEngine` 금지
2. UI, Visualization, App만 Unity API 참조 가능
3. 모든 공개 타입에 XML doc summary 필수
4. 각 하위 폴더에 자체 CLAUDE.md 존재 — 도메인 규칙 확인 필수

## Source of Truth
이 디렉토리 구조가 모듈 경계의 Source of Truth.
`PHASE-EXECUTION-BOARD.md`와 `SKILL-DOC-MATRIX.md`는 이 구조와 동기화되어야 함.
