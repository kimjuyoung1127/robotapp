# UI Design System Skill

## Trigger Keywords
`color`, `token`, `typography`, `spacing`, `component`, `button`, `panel`, `card`, `UI 패널`, `새 화면`

## Purpose
모든 UI 코드 작성 시 디자인 토큰과 컴포넌트 팩토리를 강제 사용하여 일관된 디자인을 보장합니다.

## Token Sources (Assets/Scripts/UI/)

| 파일 | 역할 |
|------|------|
| `UIDesignTokens.cs` | 색상, 폰트 크기, 간격, 컴포넌트 치수, 애니메이션 시간 |
| `UITypography.cs` | 타이포그래피 프리셋 (DisplayLg~Tiny 7단계) |
| `UIIconResolver.cs` | Resources/UI/Icons/ 아이콘 로딩 |
| `UIComponentFactory.cs` | 복합 위젯 빌더 (패널, 버튼, 뱃지, 슬라이더 등) |
| `UILayoutProfile.cs` | 태블릿/데스크탑 반응형 보정 |
| `UiRuntimeStyle.cs` | Legacy bridge (Obsolete, 점진 교체 대상) |

## MUST Rules

### Do
- 모든 색상: `UIDesignTokens.Colors.*`
- 모든 폰트 크기: `UIDesignTokens.Type.*` 또는 `TypographyPreset` enum
- 모든 간격: `UIDesignTokens.Space.*`
- 모든 컴포넌트 치수: `UIDesignTokens.Size.*`
- 버튼 ColorBlock: `UIDesignTokens.ButtonColors(color)`
- 난이도 색상: `UIDesignTokens.GetDifficultyColor(difficulty)`
- 아이콘 로딩: `UIIconResolver.Load(name)` / `UIIconResolver.CreateIcon(...)`
- 복합 위젯: `UIComponentFactory.Create*(...)`
- 반응형 치수: `UILayoutProfile.LeftPanelWidth` 등

### Do Not
- `new Color(r, g, b, a)` 리터럴 사용 금지 (UIDesignTokens에 정의된 색상 사용)
- `fontSize = {숫자}` 매직넘버 금지 (Type 상수 또는 프리셋 사용)
- `GameObject.Find("...")` 이름 기반 탐색 금지 (`FindFirstObjectByType<>()` 사용)
- `UnityEngine.UI.Text` 신규 사용 금지 (기존 코드 유지는 허용, 신규는 TMP 권장)
- spacing/dimension에 리터럴 숫자 금지 (Space/Size 상수 사용)

### Exceptions
- `UiRuntimeStyle` 메서드 내부: bridge 역할이므로 허용
- 테스트 코드: assertion용 리터럴 허용
- `Color.Lerp(tokenA, tokenB, ratio)`: 토큰 조합은 허용

## Validation Checklist
파일 작성/수정 후 확인:
1. `new Color(` 리터럴 0개 (토큰 기반 파생 제외)
2. `fontSize =` 매직넘버 0개
3. 모든 버튼 높이 ≥ 44px (`UIDesignTokens.Size.TouchTargetMin`)
4. `GameObject.Find(` 0개

## Icon Registry
`Assets/Resources/UI/Icons/` 에 25개 큐레이션 아이콘:
- Navigation: `icon-home`, `icon-back`, `icon-menu`, `icon-close`
- Actions: `icon-play`, `icon-pause`, `icon-reset`, `icon-save`, `icon-load`, `icon-settings`, `icon-edit`
- Status: `icon-check`, `icon-warning`, `icon-lock`, `icon-unlock`, `icon-info`
- Search: `icon-search`, `icon-zoom-in`, `icon-zoom-out`
- UI: `icon-plus`, `icon-minus`, `icon-award`, `icon-progress`, `icon-user`, `icon-pen`

## Dependencies
이 스킬은 다른 UI 스킬의 선행 조건:
- `tutor-step-add` → `ui-design-system`
- `student-friendly-ux` → `ui-design-system`
- `scene-scaffold` → `ui-design-system`
