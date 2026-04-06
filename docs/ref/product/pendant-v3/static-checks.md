# Teaching Pendant V3 Static Checks

## Purpose
- Teaching Pendant V3 작업에서 자주 어기는 규칙을 빠르게 점검하기 위한 정적 체크 기준을 정리한다.
- 사람이 수동으로 보거나, 추후 스크립트/CI로 자동화할 수 있는 패턴을 먼저 고정한다.

## Parent Docs
- [README.md](./README.md)
- [AGENT-CONTRACT.md](./AGENT-CONTRACT.md)
- [implementation-plan.md](./implementation-plan.md)
- [unityctl-recipes.md](./unityctl-recipes.md)

## Related Docs
- [check-v3-static.ps1](./check-v3-static.ps1)

## Last Updated
- 2026-04-06 (KST)

---

## Scope

- C# 기본 대상: `Assets/Scripts/UI/RobotControlV3/`
- UXML/USS 기본 대상: `Assets/UI/PendantV3/`
- V3 작업 중 concrete 의존 조사 대상: `Assets/Scripts/UI/`

---

## Gate Levels

### Blocker
- 하나라도 발견되면 작업 완료 주장 금지
- 수정 또는 명시적 예외 문서화가 필요

### Warning
- 바로 실패는 아니지만 같은 턴에 검토/언급 필요
- false positive면 예외 이유를 남긴다

---

## High Priority Checks

### 1. Legacy Input 금지
- 대상 경로: `Assets/Scripts/UI/RobotControlV3/`
- 금지 패턴: `UnityEngine.Input`
- 이유: V3는 `Input System Package`만 허용
- 등급: `Blocker`

### 2. Controller 생명주기 규칙
- 대상 경로: `Assets/Scripts/UI/RobotControlV3/`
- 권장 패턴: `OnEnable`, `OnDisable`
- 경고 패턴: `Awake`, `Start`에서 UI 초기화
- 이유: UI Toolkit 초기화 시점 고정
- 등급: `Blocker`

### 3. ListView 기본 규칙
- 대상: 포인트/시퀀스/로그/히스토리 목록
- 경고 패턴: 대형 반복 목록을 `ScrollView`로 직접 생성
- 권장 패턴: `ListView` + `FixedHeight` + `fixedItemHeight`
- 등급: `Warning`

### 4. Layout 애니메이션 금지
- 금지 패턴: `style.width`, `style.height`, `style.top`, `style.left`를 전환 애니메이션에 직접 사용
- 권장 패턴: `translate`, `scale`, `opacity`
- 등급: `Blocker`

### 5. 바인딩 이중 소유 금지
- 경고 패턴: 같은 UI 요소에 `Runtime Data Binding`과 수동 `SetValueWithoutNotify` 또는 직접 값 쓰기가 동시에 존재
- 이유: 상태 원천 혼선 방지
- 등급: `Warning`

---

## Quick Gate Commands

아래 5개는 작업 완료 전에 최소 1회 실행한다.

```powershell
rg -n "UnityEngine\\.Input" Assets/Scripts/UI/RobotControlV3
rg -n "\\bAwake\\b|\\bStart\\b" Assets/Scripts/UI/RobotControlV3
rg -n "style\\.(width|height|top|left)" Assets/Scripts/UI/RobotControlV3
rg -n "(MockFairinoClient|LiveFairinoClient)" Assets/Scripts/UI
rg -n "(RobotRenderer|SceneCameraDirector)" Assets/Scripts/UI
```

### Pass / Fail 해석

- 위 1~4번째 명령 결과가 비어 있어야 한다
- 5번째 명령은 기본 `Warning`이다
- 단, concrete renderer/client 직접 의존이 새로 추가됐다면 `Blocker`로 승격한다

---

## Extended rg Checks

```powershell
rg -n "new ScrollView|ScrollView\\(" Assets/Scripts/UI/RobotControlV3
rg -n "\\bListView\\b" Assets/Scripts/UI/RobotControlV3
rg -n "fixedItemHeight|CollectionVirtualizationMethod\\.FixedHeight" Assets/Scripts/UI/RobotControlV3
rg -n "SetValueWithoutNotify|RegisterValueChangedCallback|dataSourcePath|binding-path" Assets/Scripts/UI/RobotControlV3
rg -n "style\\.(display|visibility)" Assets/Scripts/UI/RobotControlV3
rg -n "style\\.(translate|scale|rotate|opacity)" Assets/Scripts/UI/RobotControlV3
rg -n "style=" Assets/UI/PendantV3 -g "*.uxml"
rg -n "binding-path|data-source-path|view-data-key|viewDataKey" Assets/UI/PendantV3 Assets/Scripts/UI/RobotControlV3
```

### 해석 규칙

- `ScrollView`가 보이면 목록 규모와 virtualization 필요 여부를 같은 턴에 검토한다
- `ListView`가 있는데 `fixedItemHeight`나 `FixedHeight`가 없으면 `Warning`
- `style=` 인라인 UXML 스타일은 임시 실험이 아닌 이상 `Warning`
- `SetValueWithoutNotify`와 바인딩 키워드가 같은 파일에 같이 보이면 이중 소유 가능성 조사

---

## Recommended One-Shot Script

```powershell
pwsh -NoLogo -NoProfile -File ./docs/ref/product/pendant-v3/check-v3-static.ps1
```

이 스크립트는 `Blocker`와 `Warning`을 분리 출력하고, `Blocker`가 있으면 non-zero exit code를 반환한다.

---

## Allowlist Policy

- false positive가 반복되면 패턴을 무시하지 말고 이유를 문서화한다
- 예외는 같은 턴에 아래 3가지를 남길 때만 허용한다
  1. 왜 예외인지
  2. 어떤 실행 단위에서 허용했는지
  3. 언제 제거할지

---

## Future Automation Hooks

- 추후 CI에서는 `check-v3-static.ps1`를 preflight의 정적 게이트로 연결한다
- `verify-v3.json` 실행 전, 이 스크립트를 먼저 통과하도록 묶는 것을 권장한다

---

## Example Review Flow

```powershell
pwsh -NoLogo -NoProfile -File ./docs/ref/product/pendant-v3/check-v3-static.ps1
& $unityctl check --project $project --type compile --json
& $unityctl workflow verify --file './docs/ref/product/pendant-v3/verify-v3.json' --project $project --json
```

---

## Review Questions

1. 이 변경이 현재 실행 단위 범위를 넘는가?
2. `PanelSettings`, `Input`, `Focus`, `3D boundary` 잠금 규칙을 깨는가?
3. `unityctl` 레시피에 없는 수동 검증 가정이 들어갔는가?
4. 현재/목표/경로 시각 구분을 약하게 만드는가?
5. 초보자 모드 기본값을 약화시키는가?
