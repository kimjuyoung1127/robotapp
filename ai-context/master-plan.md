# 마스터 플랜

최종 업데이트: 2026-03-11 (KST)

## 현재 Phase
- **Phase 0: Foundation** (완료)
- **Phase 1: Types + Math** (완료)
- **Phase 2: Kinematics Core** (완료)
- 현재 진행: **Phase 6 (CI/CD)** + **Product Docs Governance**

## Phase 로드맵

| Phase | 이름 | 핵심 산출물 | 상태 |
|-------|------|-----------|------|
| 0 | Foundation | Git, .gitignore, 문서, 클린 컴파일 | 완료 |
| 1 | 핵심 타입 & 수학 | JointType, DHLink, RobotTemplate, Pose, Vec3D, Mat3D, Mat4D | 완료 |
| 2 | 기구학 코어 | Standard DH, Forward Kinematics + EditMode 테스트 | 완료 |
| 3 | 최소 튜터 UI | 2DOF 런타임, 슬라이더, DH 테이블, Step Tutor | 진행 중 |
| 4 | 시각화 & 검증기 | Frame gizmo, 벡터 화살표, 검증 메트릭 | 대기 |
| 5 | 3DOF/6DOF 템플릿 | 확장 구조 | 대기 |
| 6 | CI/CD | GitHub Actions 파이프라인 | 진행 중 |
| 7 | 문서화 | README, 릴리스 노트, 마이그레이션 노트 | 진행 중 |
| 8 | Product Docs Governance | PRD / Wireframe / Roadmap / Product Doc Board | 진행 중 |

## 최근 완료
- GameLab-style 문서 관리 이식 1차: `docs/ref/PRD.md`, `docs/ref/WIREFRAME.md`, `docs/ref/PRODUCT-ROADMAP.md`, `docs/status/PRODUCT-DOC-BOARD.md`
- `PROJECT-STATUS`/`SKILL-DOC-MATRIX`/`INTEGRITY-REPORT`에 제품 문서 drift 관리 규칙 연결
- 문서 자동화 & 개발관리 시스템 이식 (GameLab 방법론 기반)
- 스킬 7개 + 자동화 3개 정의
- 폴더 CLAUDE.md 체계 구축
- Phase 2 구현 완료: `DHStandard`, `ForwardKinematics`, `DHStandardTests`, `FKTests`
- Phase 3 MVP 착수: `Template2DOF_RR`, `AppController` 슬라이더→FK 연동
- 검증 완료: Unity Test Runner EditMode 38/38, PlayMode 7/7

## 다음 우선순위
1. Beginner Lesson 0~3와 Core Track을 기준으로 `USER-FLOW`/`tutor-step-plan`/`architecture-diagrams`를 세분화
2. CI의 EditMode/PlayMode 실행 자동화 고정
3. Home/Guided Lesson/Sandbox/Challenge 중심 정보구조 재설계 반영
4. `docs/ref/product/roadmap/current-feature-checklist.md` 기준으로 현재 기능/부족 기능/우선 기능을 구현 백로그와 연결
5. 완전 초보자도 진입 가능한 `Pre-Kinematics Lesson 0~3`를 Guided Lesson P0 구현 기준으로 유지

## 운영 규칙
- Phase 상태는 `docs/status/PHASE-EXECUTION-BOARD.md`에서 추적
- 제품 문서 상태는 `docs/status/PRODUCT-DOC-BOARD.md`에서 추적
- 모듈별 스킬 매핑은 `docs/status/SKILL-DOC-MATRIX.md`에서 관리
- Phase 완료 시 `sprint-docs-sync` 스킬 실행
