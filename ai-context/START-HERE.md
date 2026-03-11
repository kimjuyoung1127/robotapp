# AI Context — 시작점

기준일: 2026-03-11 (KST)

## 활성 문서 (읽기 순서)
1. `ai-context/START-HERE.md` (이 파일)
2. `ai-context/master-plan.md`
3. `ai-context/project-context.md`
4. `docs/status/PROJECT-STATUS.md`
5. `docs/status/PRODUCT-DOC-BOARD.md`
6. `docs/ref/PRD.md`
7. `docs/ref/WIREFRAME.md`
8. `docs/ref/PRODUCT-ROADMAP.md`
9. `ai-context/coding-guideline.md`

## 현재 상태 스냅샷
- 현재 Phase: Phase 6 (CI/CD) + Product Docs Governance InProgress
- 최신: 2DOF 학습 런타임, scene split, student-friendly UX, visualization core, CI 초안까지 완료
- 상세 상태: `docs/status/PROJECT-STATUS.md`

## Source of Truth
- 실행 계획: `KineTutor3D_Execution_Plan.md`
- 운영 상태: `docs/status/PROJECT-STATUS.md`
- 제품 문서 보드: `docs/status/PRODUCT-DOC-BOARD.md`
- 제품 요구사항: `docs/ref/PRD.md`
- 제품 와이어프레임: `docs/ref/WIREFRAME.md`
- 제품 로드맵: `docs/ref/PRODUCT-ROADMAP.md`
- 아키텍처: `docs/ref/architecture-diagrams.md`

## 작업 규칙
- 일반 작업: 활성 문서 순서대로 읽은 후 구현.
- 제품/기획 작업: `PRODUCT-DOC-BOARD -> PRD -> WIREFRAME -> PRODUCT-ROADMAP` 순서를 반드시 포함한다.
- 수학/타입 변경: 순수 C# double 정밀도, UnityEngine 임포트 금지.
- 템플릿 변경: DH 테이블/슬라이더/모델 동기화 + EditMode 테스트 추가.
- 기구학 변경: 참조 수치값 대비 검증 필수.
