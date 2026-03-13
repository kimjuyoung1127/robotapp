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
`Assets/Runtime/Resources/UI/Icons/` 에 25개 큐레이션 아이콘:
- Navigation: `icon-home`, `icon-back`, `icon-menu`, `icon-close`
- Actions: `icon-play`, `icon-pause`, `icon-reset`, `icon-save`, `icon-load`, `icon-settings`, `icon-edit`
- Status: `icon-check`, `icon-warning`, `icon-lock`, `icon-unlock`, `icon-info`
- Search: `icon-search`, `icon-zoom-in`, `icon-zoom-out`
- UI: `icon-plus`, `icon-minus`, `icon-award`, `icon-progress`, `icon-user`, `icon-pen`

## Token Migration Procedure (기존 코드 → 토큰 전환)

기존 파일에서 하드코딩된 시각 상수를 토큰으로 교체할 때 사용합니다.

### Step 1: 스캔
```bash
# 하드코딩 색상 찾기 (UI 폴더 대상, UiRuntimeStyle 내부 제외)
grep -rn "new Color(" Assets/Scripts/UI/ --include="*.cs" | grep -v UiRuntimeStyle | grep -v "// token"

# fontSize 매직넘버 찾기
grep -rn "fontSize.*[0-9]" Assets/Scripts/UI/ --include="*.cs" | grep -v UIDesignTokens | grep -v "Type\."
```

### Step 2: 분류
| 유형 | 처리 |
|------|------|
| 정적 색상 리터럴 | → `UIDesignTokens.Colors.*` 매칭 토큰으로 교체 |
| 동적 색상 계산 (`accent.r * 0.35f`) | → 허용 (런타임 파생) |
| `Color.Lerp(tokenA, tokenB, t)` | → 허용 (토큰 조합) |
| fontSize 정수 리터럴 | → 가장 가까운 `UIDesignTokens.Type.*` 매칭 |
| spacing 정수 리터럴 | → `UIDesignTokens.Space.*` 매칭 |

### Step 3: 매칭 테이블

**fontSize → Type 토큰:**
| 원본 | 토큰 | 값 |
|------|------|-----|
| 28+ | `Type.DisplayLg` | 28 |
| 20~24 | `Type.DisplaySm` | 22 |
| 17~19 | `Type.HeadingLg` | 18 |
| 15~16 | `Type.HeadingSm` | 16 |
| 13~14 | `Type.Body` | 14 |
| 11~12 | `Type.Caption` | 12 |
| 9~10 | `Type.Tiny` | 10 |

**흔한 색상 → 토큰:**
| 패턴 | 토큰 |
|------|------|
| 밝은 흰색 텍스트 (0.9+) | `Colors.TextPrimary` |
| 중간 밝기 텍스트 (0.7~0.8) | `Colors.TextSecondary` |
| 어두운 텍스트 (0.5~0.6) | `Colors.TextMuted` |
| 파란 강조 (0.29, 0.56, 0.85) | `Colors.AccentPrimary` |
| 어두운 배경 (0.08~0.10) | `Colors.SurfaceBase` / `SurfaceRaised` |
| 카드 배경 (0.13~0.18) | `Colors.SurfaceCard` |

### Step 4: 새 토큰 필요 시
1. `UIDesignTokens.cs`의 적절한 섹션에 추가
2. XML doc summary 포함
3. 시맨틱 이름 사용 (용도 기반, 값 기반 아님)
4. 기존 토큰과 중복되지 않는지 확인

### Step 5: 검증
```bash
# 교체 후 잔존 매직넘버 재확인
grep -rn "new Color(" {수정파일} | grep -v UIDesignTokens | grep -v "// derived"
grep -rn "fontSize.*= [0-9]" {수정파일} | grep -v UIDesignTokens
```

## State UI Factory Methods (Phase A 추가)

`UIComponentFactory`에 3종 상태 뷰 팩토리 메서드가 추가됨:

```csharp
// 빈 상태 (데이터 없음)
UIComponentFactory.CreateEmptyState(parent, name, message, iconName?, ctaLabel?, onCta?)

// 로딩 상태
UIComponentFactory.CreateLoadingState(parent, name, message?)

// 에러 상태 (재시도 버튼 포함)
UIComponentFactory.CreateErrorState(parent, name, message, retryLabel?, onRetry?)
```

사용 시점: 데이터 로딩/에러/빈 상태를 표시해야 하는 모든 패널에서 직접 구현 대신 팩토리 사용.

## Dependencies
이 스킬은 다른 UI 스킬의 선행 조건:
- `tutor-step-add` → `ui-design-system`
- `student-friendly-ux` → `ui-design-system`
- `scene-scaffold` → `ui-design-system`
- `viewbuilder-extract` → `ui-design-system`
