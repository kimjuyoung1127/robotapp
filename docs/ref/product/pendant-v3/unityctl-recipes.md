# V3 unityctl Recipes

## Purpose
- Teaching Pendant V3 구현과 검증에 사용할 `unityctl` 명령 레시피를 고정한다.
- 사람과 AI 에이전트가 같은 순서, 같은 명령, 같은 산출물 기준으로 검증 루프를 돌게 한다.

## Parent Docs
- [README.md](./README.md)
- [implementation-plan.md](./implementation-plan.md)
- [AGENT-CONTRACT.md](./AGENT-CONTRACT.md)

## Last Updated
- 2026-04-06 (KST)

---

## Cross-Platform Note

- 아래 예시는 `pwsh` 기준이다. Windows PowerShell 5보다 PowerShell 7+를 우선한다.
- `unityctl` 경로는 하드코딩하지 않고 `UNITYCTL` 환경 변수 또는 `PATH`에서 찾는다.
- `$project`는 저장소 루트에서 실행한다는 가정으로 현재 작업 폴더를 사용한다.

## Session Bootstrap

```powershell
$unityctl = if ($env:UNITYCTL) { $env:UNITYCTL } else { (Get-Command unityctl -ErrorAction Stop).Source }
$project = (Resolve-Path '.').Path

& $unityctl editor select --project $project --json
& $unityctl editor current --json
& $unityctl ping --project $project --json
& $unityctl status --project $project --wait --json
& $unityctl check --project $project --type compile --json
& $unityctl console get-entries --project $project --json
```

### Use When
- 새 세션 시작
- 에디터/프로젝트 선택 상태가 애매할 때
- Domain Reload 후 연결 상태를 다시 잡을 때

---

## Recipe 0A: Infrastructure Assets

```powershell
& $unityctl check --project $project --type compile --json
& $unityctl exec --project $project --code "KineTutor3D.Editor.CliTools.PendantV3BootstrapTool.EnsurePhase0Assets()" --json
& $unityctl project validate --project $project --json
& $unityctl console get-entries --project $project --json
pwsh -NoLogo -NoProfile -File ./docs/ref/product/pendant-v3/check-v3-static.ps1
```

### Expected
- compile green
- project validate가 치명적 오류 없이 반환
- TextSettings, PanelSettings, SpriteAtlas 생성 후 콘솔 신규 에러 없음
- bootstrap surface가 `unityctl exec`와 editor menu 둘 다로 고정됨

---

## Recipe 0B-1C: Shell And Layout

```powershell
$artifacts = Join-Path 'Artifacts' 'V3'
New-Item -ItemType Directory -Force -Path $artifacts | Out-Null

& $unityctl check --project $project --type compile --json
& $unityctl exec --project $project --code "KineTutor3D.EditorTools.PendantV3SceneBuilder.AuthorScene()" --json
& $unityctl scene open --project $project --name RobotControlV3 --json
& $unityctl scene hierarchy --project $project --json
& $unityctl scene snapshot --project $project --json
& $unityctl screenshot capture --project $project --output (Join-Path $artifacts 'shell.png')
& $unityctl editor focus-gameview --project $project --json
& $unityctl editor focus-sceneview --project $project --json
```

### Use When
- `0B`, `0C`, `1A`, `1B`, `1C`

### Expected
- 5영역 셸 계층 확인
- `RobotControlV3.unity`가 없어서 레시피가 막히는 상태가 아님
- Desktop/Tablet 전환 전후 screenshot 확보
- Scene snapshot 기준 shell 이름과 구조가 문서와 일치

---

## Recipe 2A-2D: Panel UI

```powershell
$artifacts = Join-Path 'Artifacts' 'V3'
New-Item -ItemType Directory -Force -Path $artifacts | Out-Null

& $unityctl check --project $project --type compile --json
& $unityctl scene open --project $project --name RobotControlV3 --json
& $unityctl screenshot capture --project $project --output (Join-Path $artifacts 'panel-ui.png')
& $unityctl console get-entries --project $project --json
```

### Use When
- `2A-1`, `2A-2`, `2B-1`, `2B-2`, `2B-3`, `2B-4`, `2C-1`, `2C-2`, `2D`

### Expected
- 새 패널/팝업 구조가 보임
- 콘솔 에러 없음
- 위험 버튼 설명, 다음 행동, 3D 시각 구분 규칙이 screenshot에서 확인 가능

---

## Recipe 3A-3C: Binder And Mock Flow

```powershell
$artifacts = Join-Path 'Artifacts' 'V3'
New-Item -ItemType Directory -Force -Path $artifacts | Out-Null

& $unityctl check --project $project --type compile --json
& $unityctl test --project $project --mode edit --json
& $unityctl play start --project $project --json
& $unityctl console clear --project $project --json
& $unityctl console get-entries --project $project --json
& $unityctl screenshot capture --project $project --output (Join-Path $artifacts 'mock-flow.png')
& $unityctl play stop --project $project --json
```

### Use When
- `3A`, `3B`, `3C`

### Expected
- EditMode 기준 치명적 회귀 없음
- Mock 연결부터 tablet 전환까지 종단 흐름 확인
- `현재 상태`, `목표 상태`, `예상 경로` 구분이 screenshot에서 드러남

---

## Recipe 3D Viewport Verification

```powershell
$artifacts = Join-Path 'Artifacts' 'V3'
New-Item -ItemType Directory -Force -Path $artifacts | Out-Null

& $unityctl play start --project $project --json
& $unityctl screenshot capture --project $project --output (Join-Path $artifacts 'viewport-desktop.png')
& $unityctl console get-entries --project $project --json
& $unityctl play stop --project $project --json
```

### Expected
- `ViewportHost` 외 UI 입력이 3D로 관통하지 않음
- 고스트, 예상 경로, 위험 구간 표시가 동시에 있어도 현재 로봇과 시각 충돌 없음

---

## Diagnostics Recipe

```powershell
& $unityctl doctor --project $project --json
& $unityctl script get-errors --project $project --json
& $unityctl console get-entries --project $project --json
```

### Use When
- compile 실패
- PlayMode 진입 후 입력 경합 의심
- `UnityEngine.Input` 예외, 포커스 손실, viewport 상호작용 실패 조사

---

## Preflight Recipe

```powershell
& $unityctl check --project $project --type compile --json
& $unityctl test --project $project --mode edit --json
& $unityctl project validate --project $project --json
& $unityctl build --project $project --dry-run
```

### Use When
- Phase 4 비교 평가 직전
- V3 채택 판단 직전
- 기능 브랜치 커밋 전 최종 검증 루프

---

## Workflow Verify Bundle

`workflow verify`는 반복되는 artifact-first 검증을 파일로 묶을 때 사용한다.

```powershell
& $unityctl workflow verify --file './docs/ref/product/pendant-v3/verify-v3.json' --project $project --json
```

### Recommended Use
- `3C` Mock 종단 검증 번들
- `Phase 4` 비교 평가 증빙 수집
- 기본 번들은 빠른 보수 게이트다. `3C` 종료 조건은 이 번들만으로 닫지 않는다.

### Suggested Bundle Contents
- compile
- edit tests
- screenshot desktop
- screenshot tablet
- console entries
- project validate

### Default Bundle
- [verify-v3.json](./verify-v3.json) 를 기본 검증 번들로 사용한다.
- 이 번들은 `projectValidate`, `capture`, `consoleWatch`, `playSmoke`를 포함하는 보수적 기본 세트다.
- `3C` 종료 주장은 이 번들 + `Recipe 3A-3C` + `Recipe 3D Viewport Verification` + desktop/tablet evidence를 모두 요구한다.
