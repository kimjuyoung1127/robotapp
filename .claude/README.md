# .claude Ops Pack

FR5UNITY에서 Claude 계열 에이전트가 공통으로 참조하는 운영 자산 모음이다.

## Structure

```text
.claude/
├── README.md
├── settings.json
├── commands/
├── automations/
├── hooks/
└── skills/
```

## Defaults

- `commands/`: 반복 작업을 빠르게 호출하는 짧은 절차 문서
- `automations/`: 외부 스케줄러용 반복 프롬프트
- `hooks/`: 편집/커밋 전후 리마인더와 가드
- `skills/`: 도메인 스킬과 meta 운영 스킬

## Recommended Session Order

1. 루트 `AGENTS.md`
2. 루트 `CLAUDE.md`
3. `harness/REGISTRY.md`
4. `docs/ref/architecture-mermaid.md`
5. `docs/status/PROJECT-STATUS.md`
6. 작업 관련 `skills/*/SKILL.md`

## Command Shortcuts

- `/intake`
- `/impact-map`
- `/evidence-review`
- `/handoff`
- `/self-review`

## Rules

1. 범용 운영 규칙은 여기 두고, FR5 live 도메인 truth는 계속 로컬 SSOT 문서에 둔다.
2. 새 스킬은 현재 작업에 필요한 1~2개만 읽게 만든다.
3. 코드 변경이 있으면 관련 문서와 검증 루프를 같은 턴에 묶는다.
