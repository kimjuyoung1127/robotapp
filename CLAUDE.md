# KineTutor3D 오케스트레이션 인덱스

로봇 기구학 학습 도구 — 자동화 우선 운영 인덱스.

## 저장소 경계
- Write Repo: `C:\Users\ezen601\Desktop\Jason\robotapp2`

## 컨텍스트 로딩 순서 (새 세션)
1. 이 파일 — 구조 + 규칙 + 스킬/자동화 인덱스
2. `ai-context/START-HERE.md` — 프로젝트 엔트리포인트
3. `ai-context/master-plan.md` — 현재 Phase + 우선순위
4. `docs/status/PROJECT-STATUS.md` — 기능 상태 + Phase 추적
5. 대상 폴더 `CLAUDE.md` — 도메인 규칙

## 실행 규칙 (MUST)
1. 수정 전 현재 파일 내용을 반드시 읽을 것.
2. 기존 타입/수학/유틸리티 우선 재사용; 중복 구현 금지.
3. 핵심 수학(Vec3D/Mat3D/Mat4D)은 순수 C# + double 정밀도 — UnityEngine 의존 금지.
4. `Assets/Scripts/` 폴더 구조가 모듈 경계의 Source of Truth.
5. CLAUDE.md는 간결하게 유지; 상세 내용은 `docs/` 또는 `ai-context/`에 기록.
6. 명시적 요청 없이 파괴적 git 작업 금지.
7. 작업 완료 시 일일 로그 + 보드 상태 + 정합성 노트 동기화.
8. 모든 새 C# 파일에 XML doc summary (한국어 1-3줄) 필수.
9. 모든 DH/FK 구현에 사용한 수학 공식 참조 포함.

## 스킬 (.claude/skills/)

| # | 스킬명 | 트리거 키워드 | 경로 |
|---|--------|------------|------|
| 1 | math-module-add | 수학, vector, matrix, 새 타입 | kinetutor-guide/core/math-module-add/ |
| 2 | dh-algorithm-add | DH, 역기구학, 자코비안 | kinetutor-guide/kinematics/dh-algorithm-add/ |
| 3 | robot-template-add | 새 로봇, 4DOF, SCARA | kinetutor-guide/templates/robot-template-add/ |
| 4 | tutor-step-add | 튜터, 학습 단계, step | kinetutor-guide/ui/tutor-step-add/ |
| 5 | editmode-test-add | 테스트, EditMode | kinetutor-guide/test/editmode-test-add/ |
| 6 | pre-commit-validate | 커밋, 검증, pre-commit | kinetutor-guide/ops/pre-commit-validate/ |
| 7 | sprint-docs-sync | 문서 동기화, phase 완료 | meta/sprint-docs-sync/ |
| 8 | asmdef-setup | assembly definition, asmdef, 모듈 경계 | kinetutor-guide/ops/asmdef-setup/ |
| 9 | scene-scaffold | scene, Main.unity, 카메라, 씬 설정 | kinetutor-guide/ui/scene-scaffold/ |

### 스킬 의존성
```
robot-template-add → dh-algorithm-add + editmode-test-add
tutor-step-add → robot-template-add (템플릿 존재 필수)
pre-commit-validate → editmode-test-add (검증만)
scene-scaffold → tutor-step-plan.md (UI 레이아웃 참조)
asmdef-setup → architecture-diagrams.md (의존성 그래프 참조)
```

### 스킬 사용법
작업 키워드가 스킬 트리거와 매칭되면, 해당 스킬의 Read First 파일을 읽고,
Do 단계를 순서대로 실행, Validation 체크리스트로 확인.

## 자동화 프롬프트 (외부 스케줄러)

| 자동화 | 스케줄 | 목적 |
|--------|--------|------|
| docs-nightly-organizer | 22:00 KST | daily→weekly 롤업, 깨진 링크 체크 |
| code-doc-align | 21:30 KST | Scripts/ vs BOARD vs MATRIX 드리프트 감지 |
| automation-health-monitor | 09:30 KST | 자동화 + 스킬 건강 체크 |

## 현재 상태
- **Phase 0** — 문서/스킬/자동화 체계 구축 완료, C# 구현 대기

## Source of Truth 문서
- 실행 계획: `KineTutor3D_Execution_Plan.md`
- 운영 상태: `docs/status/PROJECT-STATUS.md`
- 실행 보드: `docs/status/PHASE-EXECUTION-BOARD.md`
- 스킬 매트릭스: `docs/status/SKILL-DOC-MATRIX.md`
- 아키텍처: `docs/ref/architecture-diagrams.md`

## 전문 참조 문서 (docs/ref/)

| 문서 | 내용 | 용도 |
|------|------|------|
| `dh-reference.md` | DH 수학 (기본 변환행렬, Standard/Modified DH, 회전행렬 성질) | 모든 기구학 구현의 수학 근거 |
| `test-reference-values.md` | 검증 기준값 (2DOF 4×4 행렬, Vec/Mat 연산, SCARA, PUMA 560) | TDD 테스트 expected 값 |
| `code-patterns.md` | C# 패턴 (readonly struct, NaN 가드, NUnit 보일러플레이트, 행렬 비교) | 코드 구현 템플릿 |
| `tutor-step-plan.md` | 8개 튜토리얼 스텝 (학습목표, UI, 시각화, 인터랙션) | Step Tutor UI 스펙 |
| `coordinate-mapping.md` | 로보틱스↔Unity 좌표 변환 규칙 | Visualization 모듈 전용 |
| `architecture-diagrams.md` | 모듈 의존성, 데이터 흐름, 9개 asmdef 구조 | 전체 설계 참조 |

## 폴더 CLAUDE.md 맵

| 폴더 | 목적 |
|------|------|
| Assets/Scripts/ | 소스코드 루트 — 모듈 경계 정의 |
| Assets/Scripts/Types/ | 도메인 타입 (JointType, DHLink, RobotTemplate, Pose) |
| Assets/Scripts/Math/ | 수학 라이브러리 (Vec3D, Mat3D, Mat4D — pure C#, double) |
| Assets/Scripts/Kinematics/ | DH/FK 알고리즘 (DHStandard 베이스라인 보호) |
| Assets/Scripts/Templates/ | 로봇 설정 템플릿 |
| Assets/Scripts/UI/ | UI 패널 |
| Assets/Scripts/Visualization/ | 3D 렌더링 (double→float 변환 경계) |
| Assets/Scripts/App/ | 앱 컨트롤러 |
| Assets/Tests/ | 테스트 루트 |
| Assets/Tests/EditMode/ | 순수 로직 테스트 (수학, 기구학) |
| Assets/Tests/PlayMode/ | 통합/씬 테스트 (UI, 시각화) |
| Assets/Scenes/ | 씬 관리 |

## 테스트 정책

| 영역 | 테스트 수준 | 방법 | 이유 |
|------|-----------|------|------|
| 수학 모듈 (Vec3D/Mat3D/Mat4D) | TDD | 실패 테스트 → 최소 구현 → 리팩터 | 입출력 결정적 |
| 기구학 (DH, FK) | TDD | 알려진 수치 기준값 | 수학 정확성 필수 |
| 템플릿 | 통합 | 로드 + FK 업데이트 사이클 | 템플릿-엔진 동기화 |
| UI 컴포넌트 | 빌드 검증 | Unity 컴파일 + PlayMode 스모크 | 잦은 변경, 낮은 테스트 ROI |
| 시각화 | PlayMode 스모크 | 검증 임계값 기반 시각 확인 | 시각 요소 단위 테스트 어려움 |

### 테스트 규칙
1. 수학/기구학 변경은 테스트 우선 (Red → Green → Refactor).
2. 새 로봇 템플릿은 최소 1개 FK 수치 검증 필수.
3. 구조 변경(리팩터)과 동작 변경(기능)은 별도 커밋.
4. 테스트 실패 상태로 커밋 금지 (pre-commit-validate 스킬 사용).

## 빠른 명령어
- Unity 컴파일 체크: Unity Editor 열기, Console 에러 확인
- EditMode 테스트: Unity > Window > General > Test Runner > EditMode > Run All
- PlayMode 테스트: Unity > Window > General > Test Runner > PlayMode > Run All
- 커밋 전 검증: pre-commit-validate 스킬 사용

## 완료 보고 형식
```
- 범위: [변경 내용]
- 파일: [수정된 파일 목록]
- 검증: [통과한 체크 항목]
- 일일 동기화: [업데이트된 문서]
- 위험 요소: [잠재적 문제]
- 다음 권장: [다음 작업]
```
