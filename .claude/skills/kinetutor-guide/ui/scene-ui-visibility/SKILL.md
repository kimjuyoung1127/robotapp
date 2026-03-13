---
name: scene-ui-visibility
description: "씬별/모드별 UI 가시성 제어 — scene, panel visibility, overlap, mode isolation, 배타 그룹"
---

## Trigger
씬에 UI 패널을 추가하거나, 패널 겹침 문제가 발생하거나, 새 씬/모드를 만들 때.
키워드: `panel visibility`, `UI 겹침`, `씬 UI`, `SetVisible`, `패널 초기 상태`, `ExecuteAlways`, `mode isolation`

## Input Context
- 대상 씬 (Home, Main, RobotLibrary, Sandbox, Onboarding)
- 대상 모드 (MathReadiness, Beginner, Core, Sandbox)
- 추가/수정할 UI 패널 이름
- 패널이 속하는 영역 (TopBar, LeftPanel, RightPanel, BottomBar, Overlay)

## Read First
1. `Assets/Scripts/App/SceneCatalog.cs` — 씬 목록 (Boot, Onboarding, Home, Main, RobotLibrary, Sandbox)
2. `Assets/Scripts/App/AppController.cs` — `HideAllContentPanels()` + 모드별 메서드
3. `Assets/Scripts/App/AppUiBinder.cs` — UI 자동 바인딩 패턴
4. `Assets/Scripts/App/SandboxSceneCoordinator.cs` — Sandbox 전용 가시성
5. `Assets/Scripts/UI/CLAUDE.md` — UI 폴더 규칙
6. `docs/ref/WIREFRAME.md` — 화면별 패널 배치
7. 이 스킬의 모드-패널 매트릭스 (아래)

## 모드별 패널 가시성 매트릭스 (Source of Truth)

`AppController.ApplyFeatureState()`가 적용하는 가시성 규칙입니다.
**패턴**: `HideAllContentPanels()` → 모드별 `Apply___Visibility()` → config flag 적용

### Main 씬 — 학습 모드별 패널 매트릭스

| 패널 | MathReadiness | Beginner | Core | Sandbox |
|------|:---:|:---:|:---:|:---:|
| **MathReadinessPanel** | ✅ | ❌ | ❌ | ❌ |
| **BeginnerLeftPanel** | ❌ | ✅ (config) | ❌ | ❌ |
| **DHTableEditor** | ❌ | config | config | ❌ |
| **StepTutorPanel** | ❌ | ✅ | ✅ | ❌ |
| **MatrixDisplay** | ❌ | config | config | ❌ |
| **WhyItMovedPanel** | config | config | config | ❌ |
| **TargetFeedbackPanel** | ❌ | config | ❌ | ❌ |
| **TemplateSelector** | ❌ | ✅ | ✅ | ❌ |
| **SandboxActionPanel** | ❌ | ❌ | ❌ | ✅ |
| **SnapshotLitePanel** | ❌ | ❌ | ❌ | ✅ |
| **JointInputRail** | config | config | config | ✅ |
| **StepNavigator** | ✅ | ✅ | ✅ | ❌ |
| **SceneNavigationBar** | ✅ | ✅ | ✅ | ✅ |

- `✅` = 항상 보임
- `❌` = 항상 숨김
- `config` = TutorStepConfig 플래그에 따라 결정

### 씬별 활성 패널 (Main 씬 외)

#### Home 씬
| 패널 | 상태 |
|------|------|
| HomeContinueHubController | Active |
| SceneNavigationBar | Active |
| (나머지 모든 학습 패널) | 없음 (씬에 존재하지 않음) |

#### Onboarding 씬
| 패널 | 상태 |
|------|------|
| OnboardingManager | Active |
| SceneNavigationBar | Active |
| (나머지 모든 학습 패널) | 없음 |

#### RobotLibrary 씬
| 패널 | 상태 |
|------|------|
| RobotLibraryManager | Active |
| RobotDetailDrawer | Inactive (이벤트 구동) |
| SceneNavigationBar | Active |

## 배타 그룹 규칙

같은 영역에서 **배타 그룹**으로 묶인 패널은 동시에 Active일 수 없습니다.

```
left-exclusive:  DHTableEditor | BeginnerLeftPanel | MathReadinessPanel
right-exclusive: StepTutorPanel+MatrixDisplay | WhyItMovedPanel | TargetFeedbackPanel | CompareModePanelHelper
topbar-controls: TemplateSelector (드롭다운+Sandbox+Glossary 버튼)
sandbox-only:    SandboxActionPanel | SnapshotLitePanel
```

**전환 트리거**: `AppController.ApplyFeatureState()` → `HideAllContentPanels()` → `Apply{Mode}Visibility()`

## Do

### 규칙 1: "Reset All → Show Mode" 패턴 준수

모드 전환 시 개별 `SetVisible(false)` 호출을 나열하지 말고, `HideAllContentPanels()`로 전부 끈 뒤 해당 모드 메서드에서 필요한 것만 켜야 합니다:

```csharp
// AppController.cs 패턴
private void ApplyFeatureState(TutorStepConfig config)
{
    HideAllContentPanels();          // Step 1: 전부 끄기
    ApplyCommonVisualization(config); // Step 2: 공통 시각화
    Apply{Mode}Visibility(config);   // Step 3: 모드 전용 켜기
}
```

### 규칙 2: 새 패널 추가 시 3곳 업데이트

새 콘텐츠 패널을 추가하면 반드시:
1. `HideAllContentPanels()`에 `newPanel?.SetVisible(false)` 추가
2. 해당 패널이 보여야 하는 모드별 메서드에 `SetVisible(true)` 추가
3. 이 스킬 문서의 **모드별 매트릭스** 업데이트

### 규칙 3: 새 UI 패널에 SetVisible(bool) 필수

모든 패널 MonoBehaviour에 `SetVisible(bool)` 퍼블릭 메서드를 구현합니다:
```csharp
public void SetVisible(bool visible)
{
    if (panelRoot != null)
    {
        panelRoot.gameObject.SetActive(visible);
    }
}
```

### 규칙 4: EnsureLayout은 SetActive를 변경하지 않음

`EnsureLayout()` / `EnsurePresentation()` 메서드는 UI 계층 **구조**만 생성합니다.
가시성 제어는 반드시 `SetVisible()` 또는 외부 오케스트레이터가 담당합니다.

### 규칙 5: [ExecuteAlways] 사용 시 에디터 가드

`[ExecuteAlways]`가 필요한 경우, 에디터에서 불필요한 활성 상태 변경을 하지 않도록 가드합니다.

**[ExecuteAlways] 필요 여부 판단 기준:**
- **필요**: 에디터에서 Inspector 프리뷰 제공 (`DHTableEditor`, `MatrixDisplay`)
- **불필요**: 런타임에서만 동작 (`ToastNotificationController`, `SpotlightOverlay`)
- 불필요한 `[ExecuteAlways]`는 제거합니다.

### 규칙 6: 배타 그룹 전환 패턴

`HideAllContentPanels()`가 자동으로 배타 그룹을 처리하므로, 개별 모드 메서드에서는 켜기만 하면 됩니다.

### 규칙 7: 새 모드 추가 시

1. `AppController`에 `Apply{NewMode}Visibility(TutorStepConfig)` 메서드 추가
2. `ApplyFeatureState`의 분기에 조건 추가
3. 이 스킬 문서의 매트릭스에 열 추가

### 규칙 8: 씬 간 이동 시 정리 불필요

`SceneNavigator.Load()`가 `LoadSceneMode.Single`을 사용하므로, 씬 전환 시 이전 씬의 모든 오브젝트가 자동 파괴됩니다. `DontDestroyOnLoad`는 사용하지 않습니다.

## Do Not
1. `DontDestroyOnLoad` 사용 금지 — 씬 전환 시 UI 잔류 원인
2. `EnsureLayout()` 안에서 `SetActive(true/false)` 호출 금지 — 구조와 가시성 분리
3. 모드별 메서드에서 다른 모드 패널을 `SetVisible(false)` 호출 금지 — `HideAllContentPanels()`가 이미 처리함
4. 오버레이 패널(Toast, Tooltip, Spotlight)을 씬 로드 시 Active로 설정 금지 — 이벤트 구동만
5. `[ExecuteAlways]` 불필요한 컴포넌트에 남겨두기 금지
6. `HideAllContentPanels()`를 건너뛰고 모드별 메서드만 호출 금지

## 현재 적용 상태 (2026-03-12)

### "Reset All → Show Mode" 리팩터링 완료
- `HideAllContentPanels()` — 11개 패널 일괄 숨김
- `ApplyMathReadinessVisibility()` — MathReadinessPanel만 표시
- `ApplyBeginnerVisibility()` — StepTutor + Template + DH + Matrix + BeginnerLeft + TargetFeedback
- `ApplyCoreVisibility()` — StepTutor + Template + DH + Matrix
- Sandbox: `SandboxSceneCoordinator.ApplySandboxPresentation()` — SandboxAction + SnapshotLite

### SetVisible API 현황
모든 콘텐츠 패널에 `SetVisible(bool)` 구현 완료:
- Left: `DHTableEditor`, `BeginnerLeftPanel`, `MathReadinessPanel`
- Right: `StepTutorPanel`, `MatrixDisplay` (신규), `WhyItMovedPanel`, `TargetFeedbackPanel`, `CompareModePanelHelper`
- TopBar: `TemplateSelector` (신규 — 드롭다운+Sandbox+Glossary 버튼)
- Sandbox: `SandboxActionPanel`, `SnapshotLitePanel`

### Awake/OnEnable 정리 완료
배타 그룹 패널에서 `Awake()`의 `EnsureLayout()` 제거, `OnEnable()`에 `SetVisible(false)` 추가:
- `BeginnerLeftPanel`, `MathReadinessPanel`, `TargetFeedbackPanel`, `WhyItMovedPanel`

### [ExecuteAlways] 정리 완료
런타임 전용 컴포넌트에서 제거:
- `ToastNotificationController`, `SpotlightOverlay`, `HomeContinueHubController`, `SandboxActionPanel`, `SnapshotLitePanel`

## Validation
- [x] 새 패널에 `SetVisible(bool)` 퍼블릭 메서드 존재
- [x] `HideAllContentPanels()`에 모든 콘텐츠 패널 포함
- [x] 모드별 메서드가 해당 모드 패널만 켜기
- [x] 씬 Play 진입 시 배타 그룹 패널이 동시 활성화되지 않음
- [x] 오버레이 패널이 씬 로드 직후 보이지 않음
- [x] `[ExecuteAlways]` 에디터 프리뷰가 필요 없는 컴포넌트에서 제거됨
- [x] 모드별 매트릭스가 실제 코드와 일치
- [ ] `EnsureLayout()`에 `SetActive` 호출 없음
- [ ] Unity 컴파일: 에러 0

## Output Template
```
[scene-ui-visibility 적용]
- 모드: {MathReadiness / Beginner / Core / Sandbox}
- 추가/수정 패널: {패널 이름} ({영역})
- HideAllContentPanels 업데이트: {Y/N}
- Apply{Mode}Visibility 업데이트: {Y/N}
- 매트릭스 업데이트: {Y/N}
- Unity 컴파일: 에러 0
```
