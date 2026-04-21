# unity-cli 커스텀 도구 가이드

> Archive Note: 이 문서는 `unityctl` 표준화 이전의 legacy unity-cli 운영 문서라 archive로 이동했다.

KineTutor3D 프로젝트의 unity-cli 커스텀 도구 카탈로그, 사용법, 개발 가이드.

## 개요

unity-cli 커스텀 도구는 Unity Editor 내에서 실행되며, CLI를 통해 컴파일 확인, 씬 검증, 로봇 기구학 계산 등을 자동화합니다. MCP 디버깅 기능의 약 80%를 대체할 수 있습니다.

**전제조건:**
- Unity Editor 실행 중
- `com.youngwoocho02.unity-cli-connector` 패키지 활성화
- `unity-cli` Go 바이너리 PATH에 설치

## Windows / Codex 사용 메모

- Codex 앱에서는 `unity-cli`가 PATH에 있어야 합니다.
- 현재 워크스테이션 기준 설치 경로: `C:\Users\ezen601\AppData\Local\unity-cli\unity-cli.exe`
- Codex shim 경로: `C:\Users\ezen601\AppData\Local\OpenAI\Codex\bin\unity-cli.cmd`
- 확인 명령:

```powershell
Get-Command unity-cli
unity-cli --version
unity-cli status
```

- PowerShell에서는 아래 설정을 먼저 두는 편이 안전합니다.

```powershell
$PSNativeCommandArgumentPassing = "Standard"
```

- custom tool 호출은 `compile-check` 형태가 아니라 실제 등록명인 `compile_check_tool` 형태를 사용합니다.
- 문자열/쉼표가 들어가는 인자는 PowerShell에서 `--params '{"key":"value"}'` 형태가 가장 안정적입니다.

---

## 도구 카탈로그 (26개)

### Tier 1: CI/QA 필수

| 도구 | 명령어 | 설명 |
|------|--------|------|
| CompileCheckTool | `unity-cli compile_check_tool` | 컴파일 에러/경고 카운트 |
| ConsoleCheckTool | `unity-cli console_check_tool --params '{"type":"error"}'` | 콘솔 로그 조회 (error/warn/all) |
| SceneValidateTool | `unity-cli scene_validate_tool --params '{"name":"all"}'` | 씬 missing script 검사 |
| RunTestsTool | `unity-cli run_tests_tool --params '{"mode":"edit"}'` | EditMode/PlayMode 테스트 실행 |

### Tier 2: MCP 대체

| 도구 | 명령어 | 설명 |
|------|--------|------|
| SceneHierarchyTool | `unity-cli scene_hierarchy_tool --params '{"depth":3}'` | 씬 GameObject 트리 |
| ComponentInspectTool | `unity-cli component_inspect_tool --params '{"path":"Canvas"}'` | 컴포넌트 속성 조회 |
| PrefabValidateTool | `unity-cli prefab_validate_tool --params '{"path":"Assets/..."}'` | 프리팹 무결성 검증 |

### Tier 3: KineTutor3D 전용

| 도구 | 명령어 | 설명 |
|------|--------|------|
| RobotCatalogTool | `unity-cli robot_catalog_tool` | 로봇 카탈로그 전체 목록 |
| FkComputeTool | `unity-cli fk_compute_tool --params '{"template":"FR5","joints":"0,-45,0,-59,-92,-42"}'` | FK 계산 |
| QaPrepTool | `unity-cli qa_prep_tool --params '{"scenario":"first-time"}'` | QA 시나리오 PlayerPrefs 설정 |
| DhTableTool | `unity-cli dh_table_tool --params '{"template":"FR5"}'` | DH 파라미터 테이블 덤프 |
| JointLimitTool | `unity-cli joint_limit_tool --params '{"template":"2DOF_RR"}'` | 관절 제한 범위 조회 |
| BuildSettingsTool | `unity-cli build_settings_tool` | Build Settings 씬 목록 검증 |
| CanvasValidateTool | `unity-cli canvas_validate_tool` | Canvas UI 무결성 검사 |
| AsmdefValidateTool | `unity-cli asmdef_validate_tool` | Assembly Definition 참조 검증 |
| PlayerPrefsInspectTool | `unity-cli player_prefs_inspect_tool` | PlayerPrefs 키/값 조회 |
| ResourceValidateTool | `unity-cli resource_validate_tool` | Resources 폴더 무결성 검증 |
| SessionContextTool | `unity-cli session_context_tool` | 세션 컨텍스트/진행 상태 조회 |
| TutorStepValidateTool | `unity-cli tutor_step_validate_tool` | TutorStep 에셋(S01~S08) 검증 |
| GlossaryValidateTool | `unity-cli glossary_validate_tool` | Glossary 데이터베이스 검증 |
| FR5DiagnosticTool | `unity-cli fr5_diagnostic_tool` | FR5 연결 상태/기구학 진단 (Play Mode) |
| AssetSizeTool | `unity-cli asset_size_tool --params '{"top":10}'` | Resources 에셋 크기 분석 |
| SceneDiffTool | `unity-cli scene_diff_tool --params '{"scene_a":"Boot","scene_b":"Home"}'` | 씬 간 루트 GameObject 비교 |
| PoseCompareTool | `unity-cli pose_compare_tool --params '{"template":"FR5","joints_a":"...","joints_b":"..."}'` | 두 포즈 EE 거리 비교 |
| LearningTabsTool | `unity-cli learning_tabs_tool --params '{"robot_id":"all"}'` | LearningTabs JSON 검증/요약 |
| CameraCaptureTool | `unity-cli camera_capture_tool --params '{"action":"capture"}'` | 카메라 위치 캡처/저장/적용 |

---

## 파라미터 레퍼런스

### 공통 파라미터

| 파라미터 | 타입 | 설명 |
|----------|------|------|
| `--verbose` | bool | 상세 출력 모드 (CompileCheck, ConsoleCheck, SceneValidate, RunTests) |

### CompileCheckTool

```bash
unity-cli compile-check
unity-cli compile-check --verbose true
```

| 파라미터 | 필수 | 기본값 | 설명 |
|----------|------|--------|------|
| verbose | N | false | true 시 경고 메시지 상세 포함 |

### ConsoleCheckTool

```bash
unity-cli console-check --type error
unity-cli console-check --type all --lines 100 --verbose true
```

| 파라미터 | 필수 | 기본값 | 설명 |
|----------|------|--------|------|
| type | N | error | 필터: error, warn, all |
| lines | N | 50 | 최대 반환 항목 수 |
| verbose | N | false | true 시 stackTrace 포함 |

### SceneValidateTool

```bash
unity-cli scene-validate --name all
unity-cli scene-validate --name RobotControl --verbose true
```

| 파라미터 | 필수 | 기본값 | 설명 |
|----------|------|--------|------|
| name | N | all | 씬 이름 또는 'all' |
| verbose | N | false | true 시 missing script GameObject 경로 상세 |

### RunTestsTool

```bash
unity-cli run-tests --mode edit
unity-cli run-tests --results true --verbose true
```

| 파라미터 | 필수 | 기본값 | 설명 |
|----------|------|--------|------|
| mode | N | edit | edit 또는 play |
| filter | N | - | 테스트 이름 필터 |
| results | N | false | 이전 실행 결과 조회 |
| verbose | N | false | true 시 전체 테스트 이름 포함 |

### FkComputeTool

```bash
unity-cli fk-compute --template FR5 --joints "0,-45,0,-59,-92,-42"
```

| 파라미터 | 필수 | 기본값 | 설명 |
|----------|------|--------|------|
| template | N | 2DOF_RR | 로봇 템플릿 이름 |
| joints | **Y** | - | 쉼표 구분 관절 각도 (도) |

### DhTableTool

```bash
unity-cli dh-table --template SCARA_RV
```

| 파라미터 | 필수 | 기본값 | 설명 |
|----------|------|--------|------|
| template | N | 2DOF_RR | 로봇 템플릿 이름 |

### JointLimitTool

```bash
unity-cli joint-limit --template FR5
```

| 파라미터 | 필수 | 기본값 | 설명 |
|----------|------|--------|------|
| template | N | 2DOF_RR | 로봇 템플릿 이름 |

### BuildSettingsTool

```bash
unity-cli build-settings
```

파라미터 없음. 8개 known scene(Boot, Onboarding, Home, Main, MathReadiness, RobotLibrary, Sandbox, RobotControl) 대비 검증.

### CanvasValidateTool

```bash
unity-cli canvas-validate
```

파라미터 없음. 현재 씬의 Canvas 검사:
- EventSystem 존재 여부
- GraphicRaycaster 부착 여부
- sortingOrder 중복

### AsmdefValidateTool

```bash
unity-cli asmdef-validate
```

파라미터 없음. 프로젝트 전체 asmdef 검증:
- 존재하지 않는 참조 이름 탐지
- 순환 참조 탐지 (DFS)

### PlayerPrefsInspectTool

```bash
unity-cli playerprefs-inspect
```

파라미터 없음. KineTutor3D 관련 9개 PlayerPrefs 키를 조회:
- `HasVisited`, `CurrentTrack`, `SelectedRobotId`, `SelectedMode`
- `SessionContextJson` (JSON 파싱 포함)
- 3개 트랙별 `LastCompletedStep`, `ReducedMotion`

### ResourceValidateTool

```bash
unity-cli resource-validate
```

파라미터 없음. `Assets/Runtime/Resources/` 하위 5개 섹션 검증:
- TutorSteps (S01~S08)
- LearningTabs (6개 로봇 JSON)
- Robot Prefabs (5개)
- Glossary (DB + 용어 에셋)
- Onboarding 설정

### SessionContextTool

```bash
unity-cli session-context
```

파라미터 없음. 현재 세션 상태 종합 조회:
- 사용자 상태 (visited, track, robot, mode)
- 트랙별 진행도 (math/pre-kin/core step)
- 세션 JSON 파싱 결과
- 예상 다음 씬 추론

### TutorStepValidateTool

```bash
unity-cli tutorstep-validate
```

파라미터 없음. TutorStep 에셋 S01~S08의 존재 + 필드 무결성 검증 (title, description 리플렉션).

### GlossaryValidateTool

```bash
unity-cli glossary-validate
```

파라미터 없음. GlossaryDatabase + 용어 에셋 검증 (term/definition 리플렉션, DB 내부 참조 카운트).

### FR5DiagnosticTool

```bash
unity-cli fr5-diagnostic
```

파라미터 없음. Play Mode + RobotControl 씬에서만 동작:
- Mock/Live 모드 확인
- 마지막 로봇 상태 (JointPosDeg, TcpPose)
- 기구학 상태 (JointValuesRad, EE 위치)

### AssetSizeTool

```bash
unity-cli asset-size --top 5
```

| 파라미터 | 필수 | 기본값 | 설명 |
|----------|------|--------|------|
| top | N | 10 | 가장 큰 파일 상위 N개 표시 |

Resources/ 하위 폴더별 파일 수, 크기(KB), 총합 분석.

### SceneDiffTool

```bash
unity-cli scene-diff --scene_a Boot --scene_b Home
```

| 파라미터 | 필수 | 기본값 | 설명 |
|----------|------|--------|------|
| scene_a | **Y** | - | 비교할 첫 번째 씬 |
| scene_b | **Y** | - | 비교할 두 번째 씬 |

두 씬의 루트 GameObject 이름 비교 → added/removed/common 분류.

### PoseCompareTool

```bash
unity-cli pose-compare --template FR5 --joints_a "0,-45,0,-59,-92,-42" --joints_b "10,-30,15,-45,-80,-30"
```

| 파라미터 | 필수 | 기본값 | 설명 |
|----------|------|--------|------|
| template | N | 2DOF_RR | 로봇 템플릿 이름 |
| joints_a | **Y** | - | 첫 번째 관절 각도 (도, 쉼표 구분) |
| joints_b | **Y** | - | 두 번째 관절 각도 (도, 쉼표 구분) |

FK 계산 후 EE 위치 유클리드 거리 + XYZ delta 반환.

### LearningTabsTool

```bash
unity-cli learning-tabs --robot_id all
```

| 파라미터 | 필수 | 기본값 | 설명 |
|----------|------|--------|------|
| robot_id | N | all | 로봇 ID 또는 'all' |

LearningTabs JSON 파일의 구조 검증 (robotId, displayTitle, tabs 배열) + 탭별 카드 수 요약.

### CameraCaptureTool

```bash
unity-cli camera-capture --action capture --name "my-angle"
unity-cli camera-capture --action current
unity-cli camera-capture --action list
unity-cli camera-capture --action apply --name "my-angle" --scene RobotControl
unity-cli camera-capture --action delete --name "my-angle"
```

| 파라미터 | 필수 | 기본값 | 설명 |
|----------|------|--------|------|
| action | N | capture | 동작: capture, current, list, apply, delete |
| name | 조건부 | 자동생성 | 스냅샷 이름 (capture/apply/delete 시 필요) |
| scene | N | 캡처 당시 씬 | apply 시 대상 씬 ID 오버라이드 |

Play Mode에서 카메라 위치를 캡처 → EditorPrefs에 저장 → Play Mode 종료 후 apply로 SceneCameraDirector 오버라이드 등록.
`SceneCameraDirector`는 씬 로드 시 오버라이드가 있으면 하드코딩 프로필 대신 적용.

---

## 스크립트

### `scripts/validate.sh`
전체 검증 파이프라인: dotnet build → compile-check → pre-commit-check → EditMode tests

```bash
./scripts/validate.sh
```

### `scripts/cli-integration-test.ps1`
Windows 기준 authoritative 통합 테스트 러너입니다. 26개 커스텀 도구 전체를 검증하고,
각 명령의 raw output/parsed JSON/요약 리포트를 `TestResults/UnityCli/<timestamp>/`에 저장합니다.
`component-inspect`, 상태 시나리오 검증, `run-tests --results` 폴링, `camera-capture` apply/delete 검증까지 포함합니다.

```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\cli-integration-test.ps1
```

주요 기본값:
- `-OutputRoot "TestResults/UnityCli"`
- `-PollIntervalSeconds 3`
- `-RunTestsTimeoutSeconds 900`
- `-SnapshotName "robotcontrol_baseline"`

### `scripts/cli-integration-test.sh`
기존 bash 기반 스모크 스크립트입니다. 빠른 수동 확인용이며, Windows 기본 검증 러너로는 사용하지 않습니다.
`component-inspect`, 상태 필드 assertion, `run-tests` 완료 폴링, evidence 저장은 PowerShell 러너가 담당합니다.

```bash
./scripts/cli-integration-test.sh
```

## 현재 검증 상태 (2026-03-17)

- Codex 앱에서 `unity-cli` 사용 가능:
  - `Get-Command unity-cli`
  - `unity-cli status`
  - `compile_check_tool`, `console_check_tool`, `fk_compute_tool`, `fr5_diagnostic_tool` 직접 실행 확인
- EditMode:
  - `run_tests_tool --params '{"mode":"edit"}'`
  - `run_tests_tool --params '{"results":true,"verbose":true}'`
  - 현재 기준 `401/401` passed
- PlayMode:
  - `run_tests_tool --params '{"mode":"play"}'` 는 `launched` 반환
  - 하지만 결과 조회 시 `finished=false`, `total=0` 상태로 멈출 수 있음
  - 콘솔에는 `InvalidOperationException: This cannot be used during play mode.` 와 `An unexpected error happened while running tests.` 가 관찰됨
  - 따라서 현재 PlayMode 자동 검증은 **완전 신뢰 상태가 아님**

권장 운영:
- CI/로컬 품질 게이트는 우선 EditMode + 개별 CLI 도구 검증으로 사용
- PlayMode는 `run_tests_tool` 안정화 전까지 콘솔/수동 smoke와 병행

---

## MCP → CLI 대체 매핑

| MCP 기능 | CLI 대체 도구 | Tier |
|----------|-------------|------|
| `read_console` | console-check | 1 |
| `run_tests` | run-tests | 1 |
| `find_gameobjects` | scene-hierarchy | 2 |
| 컴포넌트 상태 조회 | component-inspect | 2 |
| `manage_prefabs` (읽기) | prefab-validate | 2 |
| `manage_scene` (읽기) | scene-validate | 1 |
| `refresh_unity` | 내장 `refresh` | 기존 |
| `execute_menu_item` | 내장 `menu` | 기존 |
| `manage_editor` | 내장 `play/pause` | 기존 |

---

## MCP → CLI 자동 전환 파이프라인

MCP 도구 사용 패턴을 자동 수집하고, 반복 패턴을 CLI 도구로 전환하는 3단계 파이프라인:

```
MCP 호출 → Hook 로깅 → Gap 분석 → Scaffold 생성 → 구현 → CLI 도구
```

### 구성 요소

| 컴포넌트 | 파일 | 역할 |
|---------|------|------|
| MCP Fallback Logger | `.claude/hooks/mcp-fallback-logger.sh` | PreToolUse/PostToolUse 훅으로 MCP 호출을 `logs/mcp-usage.jsonl`에 기록 |
| Known CLI Tools | `.claude/known-cli-tools.txt` | 기존 CLI 도구 목록 (Hook이 중복 사용 감지에 활용) |
| CLI Gap Analyzer | `scripts/cli-gap-analyzer.sh` | MCP 로그를 분석해 빈도, 파라미터 패턴, 대체 후보 리포트 생성 |
| CLI Tool Scaffold | `scripts/cli-tool-scaffold.sh` | Gap 후보를 `[UnityCliTool]` 보일러플레이트로 자동 생성 |

### 워크플로우

```bash
# 1. MCP 사용 후 gap 확인
./scripts/cli-gap-analyzer.sh              # 마크다운 리포트
./scripts/cli-gap-analyzer.sh --json       # JSON (자동화용)
./scripts/cli-gap-analyzer.sh --top 3      # 상위 3개만

# 2. 후보 목록 확인
./scripts/cli-tool-scaffold.sh --list-gaps

# 3. 일괄 스캐폴드 생성
./scripts/cli-tool-scaffold.sh --from-gap

# 4. 단일 도구 생성
./scripts/cli-tool-scaffold.sh mesh-inspect --desc "메시 버텍스/폴리곤 수 검사"

# 5. 미리보기 (파일 미생성)
./scripts/cli-tool-scaffold.sh --dry-run mesh-inspect --desc "메시 버텍스/폴리곤 수 검사"
```

### 로그 형식 (mcp-usage.jsonl)

```json
{
  "timestamp": "2026-03-16T10:00:00Z",
  "event": "PreToolUse",
  "tool": "mcp__unity__get_component",
  "server": "unity",
  "operation": "get_component",
  "param_keys": ["gameObject", "component"],
  "session_id": "abc123",
  "cli_substitute": "none"
}
```

- `cli_substitute: "none"` → Gap (CLI 대체 도구 없음)
- `cli_substitute: "scene-hierarchy"` → 중복 사용 (CLI 존재하나 MCP 사용)

---

## 새 도구 추가 가이드

### 1. 파일 생성

위치: `Assets/Editor/KineTutor3D/CliTools/{ToolName}Tool.cs`

```csharp
// Folder: Editor/CliTools - unity-cli 커스텀 도구: {설명}
using Newtonsoft.Json.Linq;
using UnityCliConnector;

namespace KineTutor3D.Editor.CliTools
{
    /// <summary>
    /// {한국어 설명}
    /// </summary>
    [UnityCliTool(Description = "{영문 설명}")]
    public static class {ToolName}Tool
    {
        public static object HandleCommand(JObject @params)
        {
            var p = new ToolParams(@params);
            // 구현
            return new SuccessResponse("메시지", new { data });
        }
    }
}
```

### 2. 코딩 규칙 (code-patterns.md §8-9)

- UTF-8 with BOM 인코딩
- 1행: `// Folder: Editor/CliTools - unity-cli 커스텀 도구: {설명}`
- XML doc summary 한국어
- private 필드 camelCase (접두사 없음)
- 수치 입력: NaN/Infinity 가드 필수

### 3. 테스트 추가

- EditMode: `Assets/Tests/EditMode/CliTools/CliToolsCoreLogicTests.cs`에 코어 로직 테스트
- 통합: `scripts/cli-integration-test.ps1`(Windows authoritative) 또는 `scripts/cli-integration-test.sh`(bash smoke)에 CLI 호출 테스트 항목 추가

### 4. 문서 업데이트

- 이 파일(`docs/archive/legacy/unity-cli/cli-tools-guide.md`)에 새 도구 등록
- `CLAUDE.md` Skill 인덱스에 관련 정보 반영

---

## 파일 구조

```
Assets/Editor/KineTutor3D/CliTools/
├── AsmdefValidateTool.cs       (Tier 3)
├── AssetSizeTool.cs            (Tier 3)
├── BuildSettingsTool.cs        (Tier 3)
├── CameraCaptureTool.cs       (Tier 3)
├── CanvasValidateTool.cs       (Tier 3)
├── CompileCheckTool.cs         (Tier 1)
├── ComponentInspectTool.cs     (Tier 2)
├── ConsoleCheckTool.cs         (Tier 1)
├── DhTableTool.cs              (Tier 3)
├── FkComputeTool.cs            (Tier 3)
├── FR5DiagnosticTool.cs        (Tier 3)
├── GlossaryValidateTool.cs     (Tier 3)
├── JointLimitTool.cs           (Tier 3)
├── LearningTabsTool.cs         (Tier 3)
├── PlayerPrefsInspectTool.cs   (Tier 3)
├── PoseCompareTool.cs          (Tier 3)
├── PrefabValidateTool.cs       (Tier 2)
├── QaPrepTool.cs               (Tier 3)
├── ResourceValidateTool.cs     (Tier 3)
├── RobotCatalogTool.cs         (Tier 3)
├── RunTestsTool.cs             (Tier 1)
├── SceneDiffTool.cs            (Tier 3)
├── SceneHierarchyTool.cs       (Tier 2)
├── SceneValidateTool.cs        (Tier 1)
├── SessionContextTool.cs       (Tier 3)
├── TemplateResolver.cs         (공용 헬퍼)
└── TutorStepValidateTool.cs    (Tier 3)
```
