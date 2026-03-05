# 마스터 플랜

최종 업데이트: 2026-03-05 (KST)

## 현재 Phase
- **Phase 0: Foundation** (진행 중)
- 목표: Git 저장소 + 문서 체계 + Unity 클린 컴파일

## Phase 로드맵

| Phase | 이름 | 핵심 산출물 | 상태 |
|-------|------|-----------|------|
| 0 | Foundation | Git, .gitignore, 문서, 클린 컴파일 | 진행 중 |
| 1 | 핵심 타입 & 수학 | JointType, DHLink, RobotTemplate, Pose, Vec3D, Mat3D, Mat4D | 대기 |
| 2 | 기구학 코어 | Standard DH, Forward Kinematics + EditMode 테스트 | 대기 |
| 3 | 최소 튜터 UI | 2DOF 런타임, 슬라이더, DH 테이블, Step Tutor | 대기 |
| 4 | 시각화 & 검증기 | Frame gizmo, 벡터 화살표, 검증 메트릭 | 대기 |
| 5 | 3DOF/6DOF 템플릿 | 확장 구조 | 대기 |
| 6 | CI/CD | GitHub Actions 파이프라인 | 대기 |
| 7 | 문서화 | README, 릴리스 노트, 마이그레이션 노트 | 대기 |

## 최근 완료
- 문서 자동화 & 개발관리 시스템 이식 (GameLab 방법론 기반)
- 스킬 7개 + 자동화 3개 정의
- 폴더 CLAUDE.md 체계 구축

## 다음 우선순위
1. Phase 0 기반 작업 완료 (Git init, .gitignore, 클린 컴파일 확인)
2. Phase 1 진입: Types + Math 모듈 구현 (TDD)

## 운영 규칙
- Phase 상태는 `docs/status/PHASE-EXECUTION-BOARD.md`에서 추적
- 모듈별 스킬 매핑은 `docs/status/SKILL-DOC-MATRIX.md`에서 관리
- Phase 완료 시 `sprint-docs-sync` 스킬 실행
