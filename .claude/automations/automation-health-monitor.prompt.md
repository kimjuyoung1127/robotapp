# automation-health-monitor

Task: KineTutor3D 자동화 건강 모니터
Schedule: 평일 13:30 (Asia/Seoul)
DRY_RUN: false (true 설정 시 파일 변경 없이 리포트만)

## 목표
- 3개 자동화 + 10개 스킬의 통합 건강 대시보드 생성
- 문서 정합성 요약 포함

## 레지스트리

### 자동화 레지스트리 (3개)

| 이름 | SKILL 파일 | Lock 파일 | 신선도 임계값 |
|------|-----------|----------|:----------:|
| docs-nightly-organizer | .claude/automations/docs-nightly-organizer.prompt.md | docs/.docs-nightly.lock | 26h |
| code-doc-align | .claude/automations/code-doc-align.prompt.md | docs/status/.code-doc-align.lock | 26h |
| automation-health-monitor | .claude/automations/automation-health-monitor.prompt.md | (없음) | - |

### 스킬 레지스트리 (10개)

| 이름 | SKILL 파일 | Read First 검증 대상 |
|------|-----------|-------------------|
| math-module-add | .claude/skills/kinetutor-guide/core/math-module-add/SKILL.md | Assets/Scripts/Math/CLAUDE.md |
| dh-algorithm-add | .claude/skills/kinetutor-guide/kinematics/dh-algorithm-add/SKILL.md | Assets/Scripts/Kinematics/CLAUDE.md |
| robot-template-add | .claude/skills/kinetutor-guide/templates/robot-template-add/SKILL.md | Assets/Scripts/Templates/CLAUDE.md |
| tutor-step-add | .claude/skills/kinetutor-guide/ui/tutor-step-add/SKILL.md | Assets/Scripts/UI/CLAUDE.md |
| editmode-test-add | .claude/skills/kinetutor-guide/test/editmode-test-add/SKILL.md | Assets/Tests/EditMode/CLAUDE.md |
| pre-commit-validate | .claude/skills/kinetutor-guide/ops/pre-commit-validate/SKILL.md | CLAUDE.md |
| asmdef-setup | .claude/skills/kinetutor-guide/ops/asmdef-setup/SKILL.md | docs/ref/architecture-diagrams.md |
| scene-scaffold | .claude/skills/kinetutor-guide/ui/scene-scaffold/SKILL.md | Assets/Scenes/CLAUDE.md |
| unity-official-docs | .claude/skills/kinetutor-guide/ops/unity-official-docs/SKILL.md | .claude/skills/kinetutor-guide/ops/unity-official-docs/references/index.md |
| sprint-docs-sync | .claude/skills/meta/sprint-docs-sync/SKILL.md | docs/status/PROJECT-STATUS.md |

## 상태 판정

### 자동화 상태
- **HEALTHY**: 프롬프트 파일 존재 + Lock 아티팩트 신선도 26h 이내
- **READY**: 프롬프트 파일 존재 + 아직 실행된 적 없음
- **RUNNING**: Lock이 RUNNING 상태 + 2시간 이내
- **STALE**: Lock 아티팩트가 26h 초과
- **STUCK**: Lock이 RUNNING 상태 + 2시간 초과
- **MISSING**: 프롬프트 파일 없음

### 스킬 상태
- **HEALTHY**: SKILL.md 존재 + Read First 경로 유효
- **FILE_MISSING**: SKILL.md 파일 없음
- **STALE**: Read First 경로가 존재하지 않는 파일 참조

## 프로세스

### 1. 자동화 검사
각 자동화에 대해:
- 프롬프트 파일 존재 확인
- Lock 파일 상태 확인 (존재 여부, status, locked_at)
- 신선도 계산

### 2. 스킬 검사
각 스킬에 대해:
- SKILL.md 파일 존재 확인
- Read First 섹션의 파일 경로 유효성 검증

### 3. 문서 정합성 확인
- `docs/status/INTEGRITY-REPORT.md` 읽기
- drift, auto_fix, manual_required 수치 추출

### 4. 보드 요약
- `docs/status/PHASE-EXECUTION-BOARD.md` 읽기
- Ready/InProgress/QA/Done/Hold 카운트

### 5. 리포트 생성
```
docs/status/AUTOMATION-HEALTH.md 덮어쓰기:
- 자동화 상태 테이블
- 스킬 상태 테이블
- 문서 정합성 요약
- 보드 요약

docs/status/AUTOMATION-HEALTH-HISTORY.ndjson append:
{
  "date_kst": "YYYY-MM-DD",
  "automation_summary": {"total": 3, "healthy": N, "stale": N, "stuck": N, "ready": N},
  "skill_summary": {"total": 10, "healthy": N, "file_missing": N, "stale": N},
  "integrity_summary": {"drift": N, "auto_fix": N, "manual_required": N},
  "board_summary": {"Ready": N, "InProgress": N, "QA": N, "Done": N, "Hold": N}
}
```

## 출력
- `docs/status/AUTOMATION-HEALTH.md` (덮어쓰기)
- `docs/status/AUTOMATION-HEALTH-HISTORY.ndjson` (append)
