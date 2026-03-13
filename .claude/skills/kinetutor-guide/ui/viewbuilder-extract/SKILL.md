---
name: viewbuilder-extract
description: "MonoBehaviour에서 UI 생성 코드를 ViewBuilder 정적 클래스 + Refs struct로 분리"
---

## Trigger
기존 MonoBehaviour에서 UI 생성 코드가 비대할 때 (100줄+), 또는 새 씬/패널의 UI 구조를 코드로 빌드할 때.
키워드: `ViewBuilder`, `UI 분리`, `패널 추출`, `Refs struct`, `UI 생성 코드 정리`

## Input Context
- 대상 MonoBehaviour 파일 (예: `OnboardingManager.cs`)
- 추출 대상: `EnsurePresentation()` / `EnsureLayout()` / `BuildUI()` 내 UI 생성 코드
- 유지 대상: 수명주기, 이벤트 바인딩, 비즈니스 로직

## Read First
1. `Assets/Scripts/UI/CLAUDE.md` — UI 폴더 규칙
2. `docs/ref/code-patterns.md` — §8-9 코딩 패턴 (인코딩, 헤더, 네이밍, 수명주기)
3. 기존 ViewBuilder 예시:
   - `Assets/Scripts/UI/OnboardingViewBuilder.cs` — 단일 화면 (Refs struct + static Build)
   - `Assets/Scripts/UI/HomeContinueHubViewBuilder.cs` — 복합 카드
   - `Assets/Scripts/UI/SandboxActionPanelViewBuilder.cs` — 버튼 그룹
   - `Assets/Scripts/UI/SnapshotLitePanelViewBuilder.cs` — 상태 텍스트 + 버튼 (Refs 패턴)
4. `Assets/Scripts/UI/UIComponentFactory.cs` — 사용 가능한 위젯 빌더 목록
5. `Assets/Scripts/UI/UIDesignTokens.cs` — 색상/크기/간격 토큰

## Do

### Step 1: Refs Struct 정의
MonoBehaviour가 빌드 후 참조해야 하는 UI 요소를 `readonly struct`로 정의합니다.

```csharp
// Folder: UI - HUD/view components only; no kinematics logic.
namespace KineTutor3D.UI
{
    /// <summary>
    /// {PanelName} UI 참조를 보관하는 불변 구조체입니다.
    /// </summary>
    internal readonly struct {PanelName}ViewRefs
    {
        public {PanelName}ViewRefs(RectTransform root, Text titleText, Button actionButton)
        {
            Root = root;
            TitleText = titleText;
            ActionButton = actionButton;
        }

        public RectTransform Root { get; }
        public Text TitleText { get; }
        public Button ActionButton { get; }
    }
}
```

**규칙:**
- 필드명은 UI 역할을 반영 (예: `HeadlineText`, `SkipButton`)
- `readonly struct` + getter-only properties
- 생성자에서 모든 필드 할당
- MonoBehaviour가 참조 불필요한 내부 요소는 포함하지 않음

### Step 2: ViewBuilder 정적 클래스

```csharp
// Folder: UI - HUD/view components only; no kinematics logic.
using UnityEngine;
using UnityEngine.UI;

namespace KineTutor3D.UI
{
    /// <summary>
    /// {PanelName} UI 계층을 코드로 빌드합니다.
    /// </summary>
    internal static class {PanelName}ViewBuilder
    {
        public static {PanelName}ViewRefs Build(Transform canvasRoot, Font fallbackFont)
        {
            // UIComponentFactory + UIDesignTokens 사용
            var root = UIComponentFactory.CreatePanel(canvasRoot, "{PanelName}Root",
                UIDesignTokens.Colors.SurfaceOverlay);

            var title = UIComponentFactory.CreateText(root.transform, "Title",
                TypographyPreset.HeadingSm, UIDesignTokens.Colors.TextPrimary,
                "제목", fallbackFont);

            var btn = UIComponentFactory.CreatePrimaryButton(root.transform, "ActionBtn",
                "확인", fallbackFont);

            return new {PanelName}ViewRefs(root, title, btn.GetComponentInChildren<Text>());
        }
    }
}
```

**규칙:**
- `internal static class` — 인스턴스 불필요
- 반환 타입은 `{PanelName}ViewRefs`
- `UIComponentFactory.Create*()` 사용 필수
- 색상: `UIDesignTokens.Colors.*`
- 크기: `UIDesignTokens.Type.*` / `UIDesignTokens.Space.*`
- `new Color()` 리터럴 금지
- fontSize 매직넘버 금지

### Step 3: MonoBehaviour 리팩터링

원본 MonoBehaviour에서:
1. UI 생성 코드 전부 제거
2. `EnsurePresentation()`에서 `ViewBuilder.Build()` 호출
3. Refs struct 멤버로 UI 참조 보관
4. 이벤트 바인딩/비즈니스 로직만 유지

```csharp
// MonoBehaviour 패턴
{PanelName}ViewRefs refs;

void EnsurePresentation()
{
    if (refs.Root != null) return;
    var canvas = GetComponentInParent<Canvas>()?.transform ?? transform;
    refs = {PanelName}ViewBuilder.Build(canvas, fallbackFont);
}

void BindListeners()
{
    if (listenersBound) return;
    listenersBound = true;
    refs.ActionButton?.onClick.AddListener(OnAction);
}

void UnbindListeners()
{
    if (!listenersBound) return;
    listenersBound = false;
    refs.ActionButton?.onClick.RemoveListener(OnAction);
}
```

### Step 4: 파일 규칙
- 파일명: `{PanelName}ViewBuilder.cs` (같은 UI/ 폴더)
- Refs struct가 작으면 ViewBuilder 파일 상단에 함께 정의
- Refs struct가 크면 (5+ 필드) 별도 파일 `{PanelName}ViewRefs.cs` 분리
- UTF-8 BOM (`EF BB BF`) 필수
- 1행 헤더: `// Folder: UI - HUD/view components only; no kinematics logic.`
- XML doc summary 필수

## Do Not
1. ViewBuilder에 MonoBehaviour 참조 전달 금지 — `Transform`/`Font` 등 순수 데이터만 전달
2. ViewBuilder 내부에서 이벤트 구독 금지 — 이벤트 바인딩은 MonoBehaviour 책임
3. ViewBuilder에서 `SetActive` 호출 금지 — 가시성 제어는 외부 책임
4. `static` 필드/상태 보관 금지 — 순수 빌드 함수만
5. `UiRuntimeStyle` 신규 사용 금지 — `UIComponentFactory` 사용 (기존 ViewBuilder 유지보수 시에만 허용)
6. Refs struct에 `MonoBehaviour` / `Component` 직접 저장 금지 — UI 요소(`RectTransform`, `Text`, `Button` 등)만

## Validation
- [ ] ViewBuilder는 `internal static class`
- [ ] Refs는 `internal readonly struct`
- [ ] `new Color()` 리터럴 0개
- [ ] fontSize 매직넘버 0개
- [ ] `UIComponentFactory.Create*` 사용
- [ ] MonoBehaviour에 UI 생성 코드 0줄 (ViewBuilder 위임)
- [ ] `EnsurePresentation()`에서 null 체크 후 Build 호출
- [ ] `BindListeners()` / `UnbindListeners()` 분리
- [ ] UTF-8 BOM + 1행 헤더
- [ ] Unity 컴파일: 에러 0

## Output Template
```
[viewbuilder-extract 적용]
- 원본: {MonoBehaviour 파일명} ({원본 줄 수}줄)
- 추출: {ViewBuilder 파일명} ({추출 줄 수}줄)
- 원본 축소: {원본 줄 수} → {새 줄 수}줄 ({감소 비율}%)
- Refs 필드: {필드 목록}
- UIComponentFactory 호출: {개수}건
- Unity 컴파일: 에러 0
```

## Dependencies
- `ui-design-system` — 토큰/컴포넌트 팩토리 사용
- `scene-ui-visibility` — SetVisible 패턴 연동
