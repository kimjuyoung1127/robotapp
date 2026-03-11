# 마스터 플랜

최종 업데이트: 2026-03-12 (KST)

## 현재 Phase
- **Phase 0~5: Foundation → Guided Lesson P0** (모두 완료)
- **Phase 6: CI/CD** (Hold — runner 미등록)
- **Product Docs Governance** (진행 중)

## Phase 로드맵

| Phase | 이름 | 핵심 산출물 | 상태 |
|-------|------|-----------|------|
| 0 | Foundation | Git, .gitignore, 문서, 클린 컴파일 | 완료 |
| 1 | 핵심 타입 & 수학 | JointType, DHLink, RobotTemplate, Pose, Vec3D, Mat3D, Mat4D | 완료 |
| 2 | 기구학 코어 | Standard DH, Forward Kinematics + EditMode 테스트 | 완료 |
| 3 | 최소 튜터 UI | 2DOF 런타임, 슬라이더, DH 테이블, Step Tutor, Student UX | 완료 |
| 4 | 시각화 | Frame gizmo, RobotRenderer, Scene Flow, URP | 완료 |
| 5 | Guided Lesson P0 | Runtime foundation, Track-aware step, Joint Input, Why It Moved, Beginner L0~L3, Robot Library MVP | 완료 |
| 6 | CI/CD | GitHub Actions 파이프라인 | Hold |
| 7 | 문서화 | AGENTS hierarchy, architecture-mermaid | 완료 |

## 최근 완료
- Phase 5 전체 완료 (5A~5G): runtime snapshot, track-aware step, joint input/highlight, Why It Moved, Beginner L0~L3, Robot Library MVP, Tests+Docs sync
- 테스트 기준: EditMode 107/107, PlayMode 30/30
- 스킬 라우팅 검증: 13/13 스킬, 112/114 문서 도달 가능 (98.2%)
- 안정성 리팩터링 완료: AppController/RobotRenderer/DHTableEditor facade + helper 분리

## 다음 우선순위
1. Phase 6 CI/CD: self-hosted runner 등록 후 실주행 1회 확인
2. SCARA/3DOF/6DOF template 실제 기구학 연결 (Robot Library 데모퍼스트 → 실동작)
3. Sandbox MVP 화면 구현
4. Workspace Envelope 시각화
5. Instructor Demo Mode

## 운영 규칙
- Phase 상태는 `docs/status/PHASE-EXECUTION-BOARD.md`에서 추적
- 제품 문서 상태는 `docs/status/PRODUCT-DOC-BOARD.md`에서 추적
- 모듈별 스킬 매핑은 `docs/status/SKILL-DOC-MATRIX.md`에서 관리
- Phase 완료 시 `sprint-docs-sync` 스킬 실행
