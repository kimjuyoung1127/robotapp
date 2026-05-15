# Skills Index

이 디렉토리는 FR5UNITY에서 반복적으로 쓰는 실행형 스킬 인덱스다.

## Start Here

1. 요청을 먼저 `구현`, `검증`, `문서 동기화`, `운영 분류`, `인수인계` 중 어디에 가까운지 분류한다.
2. 아래 표에서 가장 가까운 스킬 1~2개만 고른다.
3. `SKILL.md`를 읽고 실제 파일/문서/검증 명령에 맞게 적용한다.

## Domain Skills

| Skill Family | Use When | Representative Skills |
|---|---|---|
| `kinetutor-guide/core` | 수학/기초 도메인 로직 수정 | `math-module-add` |
| `kinetutor-guide/kinematics` | DH/FK/관절 계산 수정 | `dh-algorithm-add` |
| `kinetutor-guide/templates` | 로봇 템플릿/카탈로그 추가 | `robot-template-add` |
| `kinetutor-guide/ui` | 화면/패널/시각화 surface 작업 | `tutor-step-add`, `scene-scaffold`, `student-friendly-ux`, `ui-design-system` |
| `kinetutor-guide/ops` | Unity 운영 규칙, debug capture, 검증 루프 | `pre-commit-validate`, `asmdef-setup`, `unity-official-docs`, `debug-success-capture` |
| `kinetutor-guide/content` | FR5 참조자료/로봇 콘텐츠 정리 | `fairino-fr5-integration`, `robotics-reference-to-lesson` |

## Meta Skills

| Skill | Use When | Output |
|---|---|---|
| `meta/task-intake-router` | 새 요청을 먼저 분류해야 할 때 | intake verdict, 추천 스킬, 다음 문서 |
| `meta/change-impact-map` | 변경 영향 범위를 먼저 좁혀야 할 때 | 코드/문서/검증 companion map |
| `meta/evidence-review` | 완료 선언 전에 근거를 점검할 때 | executed verify, doc sync, release verdict |
| `meta/session-handoff` | 다음 세션으로 넘길 때 | next entrypoint, blocker, first verify |

## Selection Rule Of Thumb

- "어디서 시작하지?" -> `meta/task-intake-router`
- "이 변경이 어디까지 번지지?" -> `meta/change-impact-map`
- "완료라고 해도 되나?" -> `meta/evidence-review`
- "다음 세션용으로 짧게 남겨줘" -> `meta/session-handoff`
- "디버깅 성공을 자산화해줘" -> `kinetutor-guide/ops/debug-success-capture`

## Writing Rules

1. 현재 저장소 스킬 포맷은 `Front matter + Trigger/Input Context/Read First/Do/Do Not/Validation/Output Template`를 유지한다.
2. 스킬이 길어지면 `references/`로 분리한다.
3. 도메인 규칙은 `kinetutor-guide/`에 두고, 범용 운영 규칙은 `meta/`에 둔다.
