# Harness Registry (FR5UNITY)

이 레지스트리는 FR5UNITY에서 반복적으로 쓰는 운영 하네스와 meta 스킬 진입점을 정리한 로컬 SSOT다.

## 1. 운영 하네스

| Harness | Trigger | Status | Notes |
|---|---|---|---|
| `docs/ref/csharp-master-harness.md` | C# 생성/수정 전 기본 규칙 확인이 필요할 때 | active | 헤더, 역할 경계, 패턴 기본선 |
| `harness/code-health-audit.md` | 프로젝트 위생 상태, 테스트, compile health를 점검할 때 | active | Unity 중심 품질 게이트 |
| `harness/change-class-doc-sync.md` | 코드 변경 후 companion docs가 헷갈릴 때 | starter | runtime/ui/scene/tests/ops 기준 분류 |
| `harness/session-retro.md` | 세션 종료 전 성공 패턴과 반복 수작업을 남길 때 | starter | command/skill/harness 승격 후보 추출 |
| `harness/socratic-review.md` | 큰 구조 변경이나 scope expansion 전 질문 기반 점검이 필요할 때 | starter | live risk와 verified scope 질문 세트 |

## 2. Meta Skills

| Skill | Trigger | Output |
|---|---|---|
| `meta/task-intake-router` | 새 요청 분류 | verdict, read-first docs, chosen skills |
| `meta/change-impact-map` | 영향 범위 축소 | code/docs/verify companion map |
| `meta/evidence-review` | 완료 전 근거 점검 | verify summary, docs changed, release verdict |
| `meta/session-handoff` | 다음 세션 인계 | next entrypoint, blocker, first verify |

## 3. Claude 운영 커맨드 / 훅

| Name | Use When | Path |
|---|---|---|
| `doc-update` | FR5 코드 변경 후 상태 문서/현장 로그 갱신 | `.claude/commands/doc-update.md` |
| `live-gate-review` | readback-only live 기준선, evidence, motion gate 자기점검 | `.claude/commands/live-gate-review.md` |
| `status-copy-review` | 운영자 상태문구 SSOT/금지 토큰 점검 | `.claude/commands/status-copy-review.md` |
| `intake` | 새 요청 분류 | `.claude/commands/intake.md` |
| `impact-map` | 교차 모듈 영향 범위 축소 | `.claude/commands/impact-map.md` |
| `evidence-review` | 완료 직전 근거 점검 | `.claude/commands/evidence-review.md` |
| `handoff` | 다음 세션 인계 | `.claude/commands/handoff.md` |
| `self-review` | 커밋/마감 전 자기점검 | `.claude/commands/self-review.md` |
| `post-edit-unity-compile` | `.cs/.uxml/.uss/.json` 수정 뒤 `unityctl check --type compile` 자동 실행 | `.claude/hooks/post-edit-unity-compile.sh` |

## 4. Matching Protocol

1. 작업 시작 전 `harness/REGISTRY.md`와 관련 `AGENTS.md`를 먼저 읽는다.
2. 새 요청은 가능하면 `task-intake-router`로 먼저 분류한다.
3. 코드 변경이 있으면 `change-class-doc-sync` 기준으로 companion docs를 같이 확인한다.
4. 완료 선언 전에는 `evidence-review` 또는 `code-health-audit` 기준으로 verify를 명시한다.
5. 반복 패턴이 2회 이상 나오면 command, skill, harness, automation 중 어디로 승격할지 결정한다.
