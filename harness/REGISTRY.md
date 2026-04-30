# Harness Registry (FR5UNITY)

이 레지스트리는 `jasonob`의 하네스 패턴을 프로젝트에 이식하여, 특정 작업 상황별 에이전트의 동작 지침(Harness)과 스킬을 정의합니다.

## 1. 운영 하네스 (Operational Harnesses)

| 하네스 명 | 용도 | 위치 |
|-----------|------|------|
| **C# Master Harness** | C# 코드 작성 및 품질 관리 기본 규칙 | `docs/ref/csharp-master-harness.md` |
| **Code Health Audit** | 프로젝트 전체 건강 검진 및 테스트 자동화 | `harness/code-health-audit.md` |
| **Header Injection** | 파일별 AI 컨텍스트 헤더 일괄 삽입/갱신 가이드 | `harness/header-injection.md` |
| **Document Tiering** | 컨텍스트 효율을 위한 문서 계층화 지침 | `harness/doc-tiering.md` |

## 2. 작업 자동화 스킬 (Automation Skills)

- **`skill-header-manager`**: 모든 `.cs` 파일의 헤더 상태를 스캔하고 누락된 경우 자동 삽입.
- **`skill-quality-gate`**: `unityctl`을 이용해 현재 변경 사항이 품질 게이트를 통과하는지 검증.

## 3. Claude 운영 커맨드 / 훅

| 이름 | 용도 | 위치 |
|------|------|------|
| **doc-update** | FR5 코드 변경 후 상태 문서/현장 로그 갱신 기준 | `.claude/commands/doc-update.md` |
| **live-gate-review** | readback-only live 기준선, evidence, gate 요약 자기점검 | `.claude/commands/live-gate-review.md` |
| **status-copy-review** | 운영자 상태문구 SSOT/금지 토큰 점검 | `.claude/commands/status-copy-review.md` |
| **post-edit-unity-compile** | `.cs/.uxml/.uss/.json` 수정 뒤 `unityctl check --type compile` 자동 실행 | `.claude/hooks/post-edit-unity-compile.sh` |

## 4. 하네스 적용 규칙 (Matching Protocol)
1. 에이전트는 작업 시작 전 `harness/REGISTRY.md`를 읽어 현재 작업에 적합한 하네스가 있는지 확인한다.
2. 매칭된 하네스가 있다면, 해당 하네스의 `Quality Gate` 또는 `Checklist`를 세션 종료 조건으로 삼는다.
3. 새로운 반복 패턴이 발견되면 별도의 하네스로 분리하여 이 레지스트리에 등록한다.
