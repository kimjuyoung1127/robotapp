# KineTutor3D Phase 5 Implementation Plan

## Status
- Phase 5 status: `5A~5G Complete (Phase 5 Done, 문서 sync + 테스트 보강 완료)`
- Canonical path: `docs/ref/phase5-implementation-plan.md`
- Entry docs that reference this plan: `AGENTS.md`, `CLAUDE.md`

## Context
Phase 0~4 완료 (Math/Types/Kinematics/2DOF Template/Visualization). 현재 제품은 `2DOF + SCARA` Guided Lesson/Robot Library/Sandbox baseline MVP.
이 문서의 Phase 5 범위는 foundation -> beginner track -> robot library baseline까지 완료했고, 다음 단계는 post-5G P0 UX 정리다.
코덱스가 구현하고 Claude가 UI/UX 디자인 가이드를 제공하는 구조.

## Post-5G Next Focus
- `asset subset Git tracking`
- `Home / Continue Hub`
- `resume / session context`
- `Sandbox polish`
- `tablet 4DOF input usability`
- `snapshot lite`

## Current Reality Note
- 아래 5A~5F 세부 설계는 구현 기록과 검수 기준으로 유지한다.
- 차기 개발 우선순위의 source of truth는 `PRODUCT-ROADMAP.md`, `current-feature-checklist.md`, `PHASE-EXECUTION-BOARD.md`를 우선한다.
- `Onboarding -> Core Step 8` direct jump는 현재 runtime fallback으로만 남고, target UX는 `Home / Continue Hub` 기준이다.

## 읽기 순서
1. 이 문서 전체를 먼저 읽는다
2. 구현 순서(Phase 5A~5G)를 따른다
3. 각 Phase의 "수정 파일"과 "신규 파일"을 확인한다
4. ASCII wireframe은 pixel 단위 레이아웃 가이드다
5. 에셋 매핑표는 어떤 에셋을 어디에 쓸지 명시한다

## 기존 코드 패턴 (반드시 따를 것)
- Auto-wire: `FindFirstObjectByType<T>()` -> `GameObject.Find()` -> `AddComponent<T>()` fallback
- 이벤트 구독: `Bind(AppController)` 패턴, `OnDestroy`에서 해제
- UI 생성: `UiRuntimeStyle.EnsureRectChild()`, `EnsureText()`, `EnsureImage()` 사용
- 색상: `UiRuntimeStyle.PanelBackground`, `AccentBlue`, `AccentYellow`, `TextPrimary` 등
- 게이트: `InteractionGateController`의 `Dictionary<string, int>` 카운터 패턴
- 파일 크기: MonoBehaviour 200줄, static helper 100줄, data 50줄 상한

## Review-Driven Guardrails
- 이전 상태 snapshot은 `RecomputeForwardKinematics()` 안이 아니라, 값 변경 직전(`SetJointAngleDegrees`, `HandleJointSliderChanged`, `TrySetDhParameter`)에 저장한다.
- `Why It Moved`는 `LastUpdateCause + ChangedJointIndex`를 함께 받아서, template apply/DH edit를 관절 이동 설명으로 오해하지 않게 한다.
- `StepProgressSaver`는 `pre_kinematics`와 `core_kinematics`를 별도 resume key로 저장한다.
- `JointInputRail`은 기존 2DOF rail 계약을 유지하되, SCARA 도입 시 추가 joint row를 동적으로 생성하는 방식으로 확장한다.
- `TargetMarkerConfig`는 Unity authoring 안정성을 위해 `Vector3`를 저장하고, runtime에서만 `Vec3D`로 변환한다.
- L2 compare mode는 단일 `TrailRenderer`가 아니라 3개 compare trail 세트를 사용한다.
- `Robot Library MVP`는 이번 문서에 부록으로 남기되, Phase 5 P0 Definition of Done에는 포함하지 않는다.

## Implementation Sequencing

```
Phase 5A: Runtime foundation (snapshot/update cause)
Phase 5B: Track-aware step foundation
Phase 5C: Joint Numeric Input + Joint Highlight
Phase 5D: Visualization Helpers + Why It Moved
Phase 5E: Beginner Lesson L0~L3 integration
Phase 5F: Robot Library MVP (deferred appendix, post-5E)
Phase 5G: Tests + Docs
```

---

## Phase 5A: Runtime foundation

### 수정 파일

**1. `Assets/Scripts/App/Runtime/KinematicsRuntimeState.cs`** (+6줄)
```csharp
// 추가 필드
public double[] PreviousJointValuesRad = Array.Empty<double>();
public TutorPose PreviousEndEffectorPose;
public Vec3D PreviousEndEffectorPosition;
public Mat4D PreviousEndEffectorTransform = Mat4D.Identity;
public int ChangedJointIndex = -1;
public RuntimeUpdateCause LastUpdateCause = RuntimeUpdateCause.None;
```

**2. `Assets/Scripts/App/Runtime/RuntimeUpdateCause.cs`** (신규, ~12줄)
```csharp
public enum RuntimeUpdateCause
{
    None,
    JointAngleChange,
    DhParameterEdit,
    TemplateApply
}
```

**3. `Assets/Scripts/App/Runtime/KinematicsRuntimeService.cs`** (+20줄)
- `RecomputeForwardKinematics()` 안에는 snapshot 로직을 넣지 않는다.
- 아래 helper를 추가하고, mutation 직전에 호출한다:
```csharp
private void CapturePreviousState(int changedJointIndex, RuntimeUpdateCause cause)
{
    State.PreviousJointValuesRad = (double[])State.CurrentJointValuesRad.Clone();
    State.PreviousEndEffectorPose = State.CurrentEndEffectorPose;
    State.PreviousEndEffectorPosition = State.CurrentEndEffectorTransform.ExtractPosition();
    State.PreviousEndEffectorTransform = State.CurrentEndEffectorTransform;
    State.ChangedJointIndex = changedJointIndex;
    State.LastUpdateCause = cause;
}

// 호출 위치
// SetJointAngleDegrees(): current 값 대입 직전
// HandleJointSliderChanged(): current 값 대입 직전
// TrySetDhParameter(): state.CurrentLinks[...] 대입 직전
// ApplyTemplate(): 템플릿 교체 직후 cause=TemplateApply로 마킹
```

**4. `Assets/Scripts/UI/Data/TutorStepConfig.cs`** (+12줄)
```csharp
[Header("Beginner Mode")]
public bool beginnerMode;
public bool showFormula = true;
public bool showPlainLanguage;
public bool showEndEffectorTrail;
public bool showJointHighlight;
public bool showTargetMarkers;
public bool showWhyItMoved;
public bool showJointInputRail = true;
public BeginnerLeftContent beginnerLeftContent = BeginnerLeftContent.None;
```

**5. `Assets/Scripts/UI/Data/BeginnerLeftContent.cs`** (신규, ~10줄)
```csharp
public enum BeginnerLeftContent
{
    None,
    ObserveGuide,
    ArcCompareGuide,
    CombinationGuide,
    TargetHintGuide
}
```

**6. `Assets/Scripts/UI/Data/InteractionType.cs`** (+4줄)
```csharp
// 기존 enum에 추가
ObserveMotion,
CompareArc,
CompareCombination,
TargetMatch
```

**7. `Assets/Scripts/App/Session/StepProgressSaver.cs`** (+20줄)
```csharp
// 추가 키
private const string TrackKey = "KineTutor3D.CurrentTrack"; // "pre_kinematics" | "core_kinematics"
private const string PreKinematicsLastCompletedStepKey = "KineTutor3D.PreKinematics.LastCompletedStep";
private const string CoreKinematicsLastCompletedStepKey = "KineTutor3D.CoreKinematics.LastCompletedStep";

public static string GetCurrentTrack() => PlayerPrefs.GetString(TrackKey, "core_kinematics");
public static void SetCurrentTrack(string track) => PlayerPrefs.SetString(TrackKey, track);

public static void SaveLastCompletedStep(string track, int step)
public static int GetLastCompletedStep(string track)
public static int GetResumeStep(string track, int defaultStep)

// 기존 parameterless Get/Save는 core_kinematics wrapper로 유지 가능
```

---

## Phase 5B: Track-aware step foundation

- 목표: `pre_kinematics`와 `core_kinematics`를 같은 step 시스템 위에서 안전하게 다룰 수 있게 만든다.
- `StepProgressSaver`, `TutorStepConfig`, `InteractionType`, onboarding 진입 규칙을 이 단계에서 먼저 고정한다.
- `Beginner Lesson 0~3`는 이 단계 이후의 consumer layer로 구현한다.

---

## Phase 5C: Joint Numeric Input + Joint Highlight

- 구현 상태: `Done (2026-03-11)`
- 실제 완료 범위: `JointInputRail`, `JointInputValidator`, `JointHighlightRing`, `LinkHighlighter`, `EndEffectorTrail`, `TargetMarkerVisual`, `AppController/AppUiBinder/RobotRenderer` 연결, PlayMode smoke `4/4 passed`

### 신규 파일

**1. `Assets/Scripts/UI/GuidedLesson/WhyItMovedState.cs`** (~50줄)
```
순수 C# 클래스 (MonoBehaviour 아님)
- RuntimeUpdateCause updateCause
- int changedJointIndex
- double previousValueRad, currentValueRad
- double deltaDeg
- Vec3D previousEEPosition, currentEEPosition
- Vec3D eeDisplacement
- double eeDistanceMoved
- string[] affectedLinkNames
- bool isMeaningfulJointChange
- Compute(KinematicsRuntimeState state, int jointCount) 메서드
  - `state.LastUpdateCause != JointAngleChange`면 plain-language 결과를 만들지 않음
```

**2. `Assets/Scripts/UI/GuidedLesson/WhyItMovedFormatter.cs`** (~60줄)
```
static 클래스
- FormatPlainLanguage(WhyItMovedState) -> string
  예: "관절1을 15도 더 돌리니 끝점이 오른쪽 위로 0.14m 움직였어요."
- FormatDeltaText(double deltaDeg) -> string
  예: "+15.000 deg" (양수 노랑, 음수 파랑)
- FormatEEChange(Vec3D displacement) -> string
  예: "x: +0.12  y: +0.08  z: 0.00"
```

**3. `Assets/Scripts/UI/GuidedLesson/WhyItMovedPanel.cs`** (~150줄)
```
[ExecuteAlways] MonoBehaviour
SerializeField: panelRoot, font
Auto-wire: UiRuntimeStyle 패턴 사용

레이아웃 (RightPanel 하단, StepTutorPanel 아래):
+--------------------------------------+
| [→] 변경된 관절: J1                  |  <- TextSecondary 12px Bold
|      30.000 -> 45.000 deg            |  <- TextPrimary 14px
| [Δ] 변화량: +15.000 deg             |  <- AccentYellow if +, AccentBlue if -
| [링크] 영향: Link0, Link1, EE       |  <- TextPrimary 13px
| [EE] 끝점: x:+0.12 y:+0.08 z:0.00  |  <- TextPrimary 13px
| [💬] "관절1을 15도 더 돌리니..."     |  <- TextPrimary 15px Italic
+--------------------------------------+

색상:
- 배경: UiRuntimeStyle.CardBackground
- 구분선: UiRuntimeStyle.BorderSoft
- 섹션 아이콘: Heathen flat PNG (24x24)

메서드:
- Bind(AppController) -> OnKinematicsUpdated 구독
- Refresh(WhyItMovedState) -> 텍스트 갱신
- SetVisible(bool) -> panelRoot 활성/비활성
- `RuntimeUpdateCause != JointAngleChange`면 panel 숨김 또는 neutral state 표시

에셋 매핑:
- 관절 아이콘: Free Flat Arrow 1 E Icon.png
- 변화량 아이콘: Free Flat Arrow 1 N Icon.png (양) / S (음)
- 링크 아이콘: Free Flat Gear 2 Icon.png
- 설명 아이콘: Free Flat Chat 1 Bars Icon.png
```

### 수정 파일

**`Assets/Scripts/App/AppController.cs`** (+15줄)
- WhyItMovedPanel 참조 추가 (SerializeField)
- `kinematicsService.State.LastUpdateCause`와 `ChangedJointIndex`를 기준으로 WhyItMovedState 갱신
- `PublishKinematicsUpdate()`는 기존 event를 유지하되, 패널 refresh는 `JointAngleChange`일 때만 실행

**`Assets/Scripts/App/AppUiBinder.cs`** (+5줄)
- WhyItMovedPanel AutoWire 추가

---

## Phase 5D: Visualization Helpers + Why It Moved

### 신규 파일

**1. `Assets/Scripts/UI/GuidedLesson/JointInputValidator.cs`** (~40줄)
```
static 클래스
- TryParseDegrees(string, double min, double max, out float, out string error) -> bool
- NaN/Infinity 거부, 범위 체크
```

**2. `Assets/Scripts/UI/GuidedLesson/JointInputRow.cs`** (~120줄)
```
internal sealed 클래스 (MonoBehaviour 아님, UI 요소 참조 홀더)
- Slider slider
- InputField inputField
- Text prevText, currText, deltaText, labelText
- int jointIndex

레이아웃 (단일 관절 행):
+--------------------------------------------------------------+
| J1  [-180 ========|O|======== 180]  [ 45.000 ] deg           |
|     prev: 30.000   curr: 45.000   delta: +15.000             |
+--------------------------------------------------------------+
슬라이더 너비: flex (남은 공간)
InputField: 90px, ContentType.DecimalNumber
prev/curr/delta: TextSecondary 11px
delta 색상: +AccentYellow, -AccentBlue, 0=TextMuted
```

**3. `Assets/Scripts/UI/GuidedLesson/JointInputRail.cs`** (~160줄)
```
[ExecuteAlways] MonoBehaviour
BottomBar 영역에 배치
Phase 5 범위에서는 `2DOF adapter`로 구현
- 내부적으로는 `jointSlider1`, `jointSlider2`와 동기화
- `Rebuild(int dof)`는 `dof <= 2`까지만 지원
- SCARA/6DOF용 generic rail은 별도 phase에서 slider-list 계약으로 확장

양방향 동기화:
- Slider -> InputField: slider.onValueChanged -> inputField.text = value
- InputField -> Slider: inputField.onEndEdit -> Validate -> AppController.SetJointAngleDegrees

포커스 이벤트:
- InputField.onSelect / Slider PointerDown -> AppController에 joint focus 알림
- 이 이벤트가 JointHighlight 트리거

메서드:
- Bind(AppController)
- Rebuild(int dof) -> `dof=0..2` 범위에서 행 재생성, 그 이상은 warning
- SetValueWithoutNotify(int index, float degrees) -> 콜백 없이 값 설정
- SetSliderWithoutNotify(int index, float degrees) -> 슬라이더만 갱신
```

**4. `Assets/Scripts/Visualization/Targets/JointHighlightRing.cs`** (~80줄)
```
MonoBehaviour (RobotRenderer 자식으로 생성)
LineRenderer로 관절 축 주위에 원형 링 그리기

- SetTarget(Transform jointPivot, float radius) -> 대상 관절 설정
- Clear() -> 비활성화
- Update에서 alpha pulse (FocusZoneHighlighter 패턴)

색상: AccentBlue, lineWidth 0.008
세그먼트: 32개 점으로 원 근사
```

**5. `Assets/Scripts/Visualization/Targets/LinkHighlighter.cs`** (~60줄)
```
static 클래스
- Highlight(Transform linkVisual, Color color) -> MaterialPropertyBlock._EmissionColor 설정
- Clear(Transform linkVisual) -> emission 제거
URP Lit 셰이더의 _EmissionColor 속성 사용
```

### 수정 파일

**`Assets/Scripts/Visualization/Renderer/RobotRenderer.cs`** (+20줄)
```csharp
// 추가 메서드
public void HighlightJoint(int jointIndex)
// jointIndex -> link0Visual 또는 link1Visual 매핑
// JointHighlightRing.SetTarget(pivot)
// LinkHighlighter.Highlight(linkVisual, AccentBlue)

public void ClearJointHighlight()
// JointHighlightRing.Clear()
// LinkHighlighter.Clear(all links)
```

**`Assets/Scripts/App/AppController.cs`** (+10줄)
- JointInputRail 참조 + AutoWire
- OnJointFocused(int index) / OnJointUnfocused() 이벤트 추가

**`Assets/Scripts/UI/GuidedLesson/StepNavigator.cs`** (+5줄)
- JointInputRail이 BottomBar에 공존하도록 레이아웃 조정

---

## Phase 5E: Beginner Lesson L0~L3 integration

### 신규 파일

**1. `Assets/Scripts/Visualization/Shared/EndEffectorTrail.cs`** (~90줄)
```
MonoBehaviour (Frame_EE 자식으로 부착)
기본 TrailRenderer 1개 + compare용 child TrailRenderer 3개 관리

설정:
- time: 3.0f (3초 trail 유지)
- startWidth: 0.015f, endWidth: 0.003f
- material: Unlit/Color, AccentYellow, alpha 0.7
- minVertexDistance: 0.005f

메서드:
- SetVisible(bool)
- ClearAllTrails()
- SetSingleMode(Color)
- SetCompareModeVisible(bool)
- ConfigureCompareTrail(CompareTrailSlot slot, Color color)

L2 compare mode에서:
- trail_j1_only: AccentYellow
- trail_j2_only: AccentBlue
- trail_both: white
```

**2. `Assets/Scripts/UI/Data/TargetMarkerConfig.cs`** (~30줄)
```csharp
[Serializable]
public class TargetMarkerConfig
{
    public string targetId;
    public Vector3 authoringPosition;
    public float reachThreshold = 0.05f; // meters

    public Vec3D ToRoboticsPosition()
    {
        return new Vec3D(authoringPosition.x, authoringPosition.y, authoringPosition.z);
    }
}
```

**3. `Assets/Scripts/Visualization/Targets/TargetMarkerVisual.cs`** (~100줄)
```
MonoBehaviour
ShootingTarget.prefab를 instantiate하여 목표점에 배치

메서드:
- Setup(TargetMarkerConfig config) -> 위치 설정
- UpdateReachState(Vec3D currentEEPos) -> 거리 계산, reached/not 판정
- 도달 시: Checkmark_3D_Icon 색상 변경 + pulse
- 미도달 시: Warning_3D_Icon + 거리 표시

에셋 매핑:
- 목표점 마커: ShootingTarget.prefab (Assets/Vendors/Archive/GlowingRifts/)
- 도달 아이콘: Checkmark_3D_Icon.prefab (HQP)
- 미도달 아이콘: Warning_3D_Icon.prefab (HQP)
```

### 수정 파일

**`Assets/Scripts/Visualization/Renderer/RobotRenderer.cs`** (+15줄)
- EndEffectorTrail 참조 + step 변경 시 single/compare mode 전환 및 clear
- TargetMarkerVisual[] 관리

---

## Phase 5E: Beginner Lesson L0~L3

### 화면 레이아웃

#### L0: 로봇 팔은 무엇을 움직이는가
```
+----------+-------------------------------------+-----------------+
|LeftPanel |         Center Viewport              | RightPanel      |
|356px     |                                      | 388px           |
|+--------+|    +---+                             |+---------------+|
||Beginner ||   | B |====[Link0]==[Link1]===o EE  ||StepTutorPanel ||
||LeftPanel||   +---+    [highlight ring]         ||(beginner text)||
||         ||                                     ||               ||
||"관절을  ||    ~~~~~~~~ EE trail ~~~~~~~~~~     ||+-------------+||
|| 움직여  ||                                     |||WhyItMoved   |||
|| 보세요."||   frame_0(RGB)  frame_1  Frame_EE  ||| MiniPanel   |||
|+--------+|                                     ||+-------------+||
+----------+-------------------------------------+-----------------+
| BottomBar: JointInputRail + StepNavigator                        |
| [J1 slider + input] [J2 slider + input]                          |
| [<Prev] [Gate: 관절 2개 이상 움직이기]       [Skip] [Next>]      |
+------------------------------------------------------------------+
```

#### L1: 회전하면 끝점이 어떻게 움직이는가
```
+----------+-------------------------------------+-----------------+
|LeftPanel |         Center Viewport              | RightPanel      |
|+--------+|         +---+                        |+---------------+|
||Beginner ||         | B |====[L0]==[L1]===o     ||StepTutorPanel ||
||LeftPanel||         +---+      \                 ||               ||
||         ||                  arc trail          ||+-------------+||
||"같은    ||              .  .  .  .             |||WhyItMoved   |||
|| 관절을  ||           .                .        |||Panel        |||
|| 여러    ||          .  ghost trail     .       |||(expanded)   |||
|| 각도로  ||           .                .        |||J1: 0->45    |||
|| 바꿔    ||              .  .  .  .             |||delta: +45   |||
|| 보세요."||                                      |||EE moved    |||
|+--------+|                                      ||+-------------+||
+----------+-------------------------------------+-----------------+
| BottomBar: [J1 slider + input]                                    |
| [<Prev] [Gate: J1을 3회 이상 변경]            [Skip] [Next>]     |
+------------------------------------------------------------------+
```

#### L2: 두 관절이 같이 움직이면 왜 경로가 바뀌는가
```
+----------+-------------------------------------+-----------------+
|LeftPanel |         Center Viewport              | RightPanel      |
|+--------+|                                      |+---------------+|
||Compare  ||   trail_j1_only (yellow dashed)     ||StepTutorPanel ||
||ModePanel||   trail_j2_only (cyan dashed)       ||               ||
||         ||   trail_both    (white solid)        ||+-------------+||
||"J1만:   ||                                      |||WhyItMoved   |||
|| 원호    ||         +---+                        |||CompareMode  |||
|| J2만:   ||         | B |====[L0]==[L1]===o     ||| J1: 0->30   |||
|| 다른원호||         +---+                        ||| J2: 0->-20  |||
|| 둘 다:  ||                                      ||| combined:   |||
|| 복잡한  ||    previous EE pos (ghost dot)       ||| EE +0.2,+0.1|||
|| 경로"   ||    current  EE pos (solid dot)       ||+-------------+||
|+--------+|                                      |+---------------+|
+----------+-------------------------------------+-----------------+
| BottomBar: [J1 slider + input] [J2 slider + input]               |
| [Reset] [<Prev] [Gate: 3가지 비교 완료]       [Skip] [Next>]    |
+------------------------------------------------------------------+
```

#### L3: 목표점을 맞추려면 왜 거꾸로 생각해야 하는가
```
+----------+-------------------------------------+-----------------+
|LeftPanel |         Center Viewport              | RightPanel      |
|+--------+|                                      |+---------------+|
||Target   ||   +---+                             ||StepTutorPanel ||
||HintPanel||   | B |====[L0]==[L1]===o           ||               ||
||         ||   +---+                             ||+-------------+||
||"목표점을||            (X) ShootingTarget        |||TargetFeedback||
|| 맞춰   ||                                      ||| target_1:   |||
|| 보세요."||     (X) ShootingTarget #2           ||| [NOT REACHED]|||
||         ||                                      ||| dist: 0.45m |||
||[REACH]  ||                                      ||| "J1을 더   |||
||[NOTREACH]|                                      |||  돌려보세요"|||
|+--------+|                                      ||+-------------+||
+----------+-------------------------------------+-----------------+
| BottomBar: [J1 slider + input] [J2 slider + input]               |
| [Reset] [<Prev] [Gate: 목표 2개 도달]          [Skip] [Next>]   |
+------------------------------------------------------------------+
```

### 신규 파일

**1. `Assets/Scripts/App/Lessons/BeginnerLessonFactory.cs`** (~120줄)
```
static 클래스 (TutorStepRuntimeFactory 패턴 따름)
- CreateLessons() -> TutorStepConfig[4]

L0 config:
  beginnerMode=true, showFormula=false, showPlainLanguage=true
  showEndEffectorTrail=true, showJointHighlight=true, showWhyItMoved=true
  showDHTable=false, showMatrices=false
  beginnerLeftContent=ObserveGuide
  leftContent=Hidden, rightContent=Hidden (기존 DH/Matrix 숨김)
  focusTarget=Viewport3D
  conditions: [SliderChange:"joint_slider_1":2, SliderChange:"joint_slider_2":1]
  stepTitleKo="로봇 팔은 무엇을 움직이는가"
  objectiveKo="관절을 움직여 링크와 끝점이 함께 변하는 것을 관찰하세요."

L1 config: (compare_arc)
  conditions: [SliderChange:"joint_slider_1":3]

L2 config: (compare_combination)
  conditions: [StepAction:"compare_j1_only":1, StepAction:"compare_j2_only":1, StepAction:"compare_both":1]

L3 config: (target_match)
  showTargetMarkers=true
  beginnerLeftContent=TargetHintGuide
  conditions: [StepAction:"target_reached":2]
```

**2. `Assets/Scripts/UI/GuidedLesson/BeginnerLeftPanel.cs`** (~120줄)
```
[ExecuteAlways] MonoBehaviour
LeftPanel 영역, beginnerLeftContent에 따라 내용 전환

레이아웃:
+--------+
| 제목   |  <- TextPrimary 18px Bold
| 본문   |  <- TextPrimary 14px, 줄바꿈
| 힌트   |  <- CardBackground, TextSecondary 13px
|[아이콘]|  <- Free Flat Chat 1 Question Icon.png
+--------+

메서드:
- ApplyContent(BeginnerLeftContent content) -> 텍스트 전환
- SetVisible(bool)
```

**3. `Assets/Scripts/UI/GuidedLesson/TargetFeedbackPanel.cs`** (~100줄)
```
[ExecuteAlways] MonoBehaviour
RightPanel 하단 (WhyItMovedPanel 대신 또는 아래)

메서드:
- Setup(TargetMarkerConfig[] targets)
- Evaluate(Vec3D currentEEPos) -> 거리 계산, 도달 판정
- 도달 시 AppController.ReportInteraction(StepAction, "target_reached")

레이아웃:
+--------------------------------------+
| target_1: [V] REACHED (0.03m)        |  <- Checkmark green
| target_2: [!] NOT REACHED (0.45m)    |  <- Warning red
| hint: "J1을 더 돌려보세요"            |
+--------------------------------------+
```

**4. `Assets/Scripts/UI/GuidedLesson/CompareModePanelHelper.cs`** (~80줄)
```
L2 전용 비교 모드 UI 상태 관리
- compare_j1_only, compare_j2_only, compare_both 버튼/상태
- 각 모드에서 trail 색상 변경 요청
- 버튼 클릭 시 AppController.ReportInteraction(StepAction, "compare_xxx")
```

### 수정 파일

| 파일 | 변경 내용 |
|------|----------|
| `AppController.cs` | stepConfigs를 L0~L3 + S1~S8 합산으로 로드; track-aware resume 분기 |
| `StepFlowService.cs` | beginnerMode일 때 BeginnerLeftPanel/WhyItMovedPanel 우선 배선; beginner UI flags를 실제로 반영 |
| `AppUiBinder.cs` | BeginnerLeftPanel, TargetFeedbackPanel, CompareModePanelHelper AutoWire |
| `ProgressiveDisclosureController.cs` | beginnerLeftContent 패널 전환 + `showFormula/showPlainLanguage/showJointInputRail` 같은 beginner flags 반영 |
| `StepTutorPanel.cs` | "Lesson X/4" vs "Step X/8" 제목 포맷 분기 |
| `OnboardingManager.cs` | "완전 초보로 시작" 버튼 -> track="pre_kinematics", step=L0 / 기본 시작 버튼은 core 유지 |
| `InteractionGateController.cs` | 새 InteractionType 처리 (기존 Dictionary 패턴으로 충분) |

---

## Phase 5F: Robot Library MVP (Deferred Appendix)

> Phase 5 P0 Definition of Done에는 포함하지 않는다.
> 아래 내용은 5A~5E 구현 + 테스트 green 이후의 post-5E 작업이다.

### 신규 파일

| 파일 | 줄 | 설명 |
|------|---|------|
| `Assets/Scripts/UI/Data/RobotMetadata.cs` | ~40 | ScriptableObject: name, dof, type, difficulty, supportedModes |
| `Assets/Scripts/UI/RobotLibrary/RobotLibraryPanel.cs` | ~150 | Grid 레이아웃, RobotCard 동적 생성 |
| `Assets/Scripts/UI/RobotLibrary/RobotCard.cs` | ~100 | 카드 1개: RenderTexture 미리보기, 메타데이터, CTA |

### 레이아웃
```
+------------------------------------------------------------------+
| TopBar: Robot Library                              [<Back to Home]|
+------------------------------------------------------------------+
|  +-------------+  +-------------+  +-------------+               |
|  | [3D preview]|  | [3D preview]|  | [3D preview]|               |
|  | 2DOF RR     |  | SCARA       |  | Fanuc CRX   |               |
|  | DOF: 2      |  | DOF: 4      |  | DOF: 6      |               |
|  | Level: Easy |  | Level: Med  |  | Level: Demo |               |
|  |[Start Lesson]| |[Coming Soon]|  |[View Only  ]|               |
|  +-------------+  +-------------+  +-------------+               |
+------------------------------------------------------------------+

에셋 매핑:
- 카드 배경: UiRuntimeStyle.CardBackground
- DOF 배지: code-generated (Image + Text)
- 난이도 배지: code-generated, Easy=green, Med=yellow, Hard=red
- 로봇 프리뷰: RenderTexture (별도 카메라로 프리팹 촬영)
- Home 아이콘: Free Flat Home Icon.png
- Search 아이콘: Free Flat Magnify Icon.png
```

---

## 전체 에셋 매핑 요약

| 기능 | 에셋 | 소스 경로 |
|------|------|----------|
| WhyItMoved 관절 아이콘 | Free Flat Arrow 1 E Icon.png | `Assets/Vendors/Archive/HeathenEngineering/Assets/UX/Icons/Flat Icons [Free]/` |
| WhyItMoved 양수 화살표 | Free Flat Arrow 1 N Icon.png | Heathen |
| WhyItMoved 음수 화살표 | Free Flat Arrow 1 S Icon.png | Heathen |
| WhyItMoved 링크 아이콘 | Free Flat Gear 2 Icon.png | Heathen |
| WhyItMoved 설명 아이콘 | Free Flat Chat 1 Bars Icon.png | Heathen |
| 힌트 카드 아이콘 | Free Flat Chat 1 Question Icon.png | Heathen |
| 목표점 마커 | ShootingTarget.prefab | `Assets/Vendors/Archive/GlowingRifts/Shooting Target/` |
| 도달 표시 | Checkmark_3D_Icon.prefab | `Assets/Vendors/Archive/HQPStudios/Low Poly 3D Icons - Pack Lite/Prefabs/` |
| 미도달 표시 | Warning_3D_Icon.prefab | HQP Studios |
| EE trail | TrailRenderer (코드 생성) | Runtime |
| Joint highlight ring | LineRenderer (코드 생성) | Runtime |
| Reset 버튼 아이콘 | Free Flat Media Left Double Icon.png | Heathen |
| Home 아이콘 | Free Flat Home Icon.png | Heathen |
| Search 아이콘 | Free Flat Magnify Icon.png | Heathen |
| Settings 아이콘 | Free Flat Gear 1 Icon.png | Heathen |
| Lock (gate) 아이콘 | Free Flat Lock Closed Icon.png | Heathen |
| Unlock (gate) 아이콘 | Free Flat Lock Open Icon.png | Heathen |

---

## 전체 신규/수정 파일 요약

### 신규 파일 (20개)

| # | 경로 | 줄 | Phase |
|---|------|---|-------|
| 1 | `Assets/Scripts/UI/Data/BeginnerLeftContent.cs` | ~10 | 5A |
| 2 | `Assets/Scripts/App/Runtime/RuntimeUpdateCause.cs` | ~12 | 5A |
| 3 | `Assets/Scripts/UI/Data/TargetMarkerConfig.cs` | ~30 | 5D |
| 4 | `Assets/Scripts/UI/GuidedLesson/WhyItMovedState.cs` | ~50 | 5B |
| 5 | `Assets/Scripts/UI/GuidedLesson/WhyItMovedFormatter.cs` | ~60 | 5B |
| 6 | `Assets/Scripts/UI/GuidedLesson/WhyItMovedPanel.cs` | ~150 | 5B |
| 7 | `Assets/Scripts/UI/GuidedLesson/JointInputValidator.cs` | ~40 | 5C |
| 8 | `Assets/Scripts/UI/GuidedLesson/JointInputRow.cs` | ~120 | 5C |
| 9 | `Assets/Scripts/UI/GuidedLesson/JointInputRail.cs` | ~160 | 5C |
| 10 | `Assets/Scripts/Visualization/Targets/JointHighlightRing.cs` | ~80 | 5C |
| 11 | `Assets/Scripts/Visualization/Targets/LinkHighlighter.cs` | ~60 | 5C |
| 12 | `Assets/Scripts/Visualization/Shared/EndEffectorTrail.cs` | ~120 | 5D |
| 13 | `Assets/Scripts/Visualization/Targets/TargetMarkerVisual.cs` | ~100 | 5D |
| 14 | `Assets/Scripts/App/Lessons/BeginnerLessonFactory.cs` | ~120 | 5E |
| 15 | `Assets/Scripts/UI/GuidedLesson/BeginnerLeftPanel.cs` | ~120 | 5E |
| 16 | `Assets/Scripts/UI/GuidedLesson/TargetFeedbackPanel.cs` | ~100 | 5E |
| 17 | `Assets/Scripts/UI/GuidedLesson/CompareModePanelHelper.cs` | ~80 | 5E |
| 18 | `Assets/Scripts/UI/Data/RobotMetadata.cs` | ~40 | 5F deferred |
| 19 | `Assets/Scripts/UI/RobotLibrary/RobotLibraryPanel.cs` | ~150 | 5F deferred |
| 20 | `Assets/Scripts/UI/RobotLibrary/RobotCard.cs` | ~100 | 5F deferred |

### 수정 파일 (13개)

| # | 경로 | 변경량 | Phase |
|---|------|-------|-------|
| 1 | `Assets/Scripts/App/Runtime/KinematicsRuntimeState.cs` | +4줄 | 5A |
| 2 | `Assets/Scripts/App/Runtime/KinematicsRuntimeService.cs` | +10줄 | 5A |
| 3 | `Assets/Scripts/UI/Data/TutorStepConfig.cs` | +12줄 | 5A |
| 4 | `Assets/Scripts/UI/Data/InteractionType.cs` | +4줄 | 5A |
| 5 | `Assets/Scripts/App/Session/StepProgressSaver.cs` | +10줄 | 5A |
| 6 | `Assets/Scripts/App/AppController.cs` | +40줄 | 5B+5C+5E |
| 7 | `Assets/Scripts/App/AppUiBinder.cs` | +15줄 | 5B+5C+5E |
| 8 | `Assets/Scripts/Visualization/Renderer/RobotRenderer.cs` | +35줄 | 5C+5D |
| 9 | `Assets/Scripts/UI/GuidedLesson/StepNavigator.cs` | +5줄 | 5C |
| 10 | `Assets/Scripts/App/Lessons/StepFlowService.cs` | +15줄 | 5E |
| 11 | `Assets/Scripts/UI/GuidedLesson/ProgressiveDisclosureController.cs` | +10줄 | 5E |
| 12 | `Assets/Scripts/UI/GuidedLesson/StepTutorPanel.cs` | +5줄 | 5E |
| 13 | `Assets/Scripts/UI/Onboarding/OnboardingManager.cs` | +15줄 | 5E |

---

## 파일 크기 제어

| 규칙 | 기준 |
|------|------|
| MonoBehaviour | 200줄 상한 |
| static helper | 100줄 상한 |
| data class/enum | 50줄 상한 |
| 책임 분리 | 1 MonoBehaviour = 1 UI 영역 |
| 패턴 준수 | UiRuntimeStyle.Ensure* 재사용 |

---

## Gate 조건 매핑

| Lesson | Gate Type | GateCondition[] |
|--------|-----------|----------------|
| L0 | observe_motion | SliderChange:"joint_slider_1":2 + SliderChange:"joint_slider_2":1 |
| L1 | compare_arc | SliderChange:"joint_slider_1":3 |
| L2 | compare_combination | StepAction:"compare_j1_only":1 + StepAction:"compare_j2_only":1 + StepAction:"compare_both":1 |
| L3 | target_match | StepAction:"target_reached":2 |

---

## 데이터 흐름도

```
Slider/Input change
    |
    v
AppController.HandleJointSliderChanged(jointIndex, degrees)
    |
    +---> KinematicsRuntimeService.HandleJointSliderChanged()
    |         |
    |         +---> CapturePreviousState(jointIndex, JointAngleChange)  [before mutation]
    |         +---> update CurrentJointValuesRad[jointIndex]
    |         +---> recompute FK
    |         +---> update CurrentEEPose
    |
    +---> PublishKinematicsUpdate()  [existing event]
    |         |
    |         +---> RobotRenderer.HandleKinematicsUpdated()
    |         |         +---> ApplyTransforms() [existing]
    |         |         +---> EndEffectorTrail single/compare set update [NEW]
    |         |         +---> JointHighlightRing.UpdateTarget() [NEW]
    |         |
    |         +---> WhyItMovedPanel.Refresh(prev, current, cause, changedJointIndex) [NEW]
    |         +---> TargetFeedbackPanel.Evaluate(currentEE, targets) [NEW]
    |         +---> JointInputRail.SetValueWithoutNotify() [NEW]
    |
    +---> ReportInteraction(SliderChange, targetId)
              |
              +---> InteractionGateController.RegisterInteraction()
                        |
                        +---> evaluate gate conditions
                        +---> GateStateChanged event
```

---

## 테스트 계획

### EditMode (신규 파일 2개)
- `Assets/Tests/EditMode/BeginnerLessonFactoryTests.cs` (~80줄)
  - L0~L3 factory creates 4 configs
  - 각 config의 beginnerMode=true, showFormula=false 확인
  - L0 gate: observe_motion 조건 확인
  - L3 gate: target_match 조건 확인
  - WhyItMovedState delta 계산 정확도

- `Assets/Tests/EditMode/JointInputValidatorTests.cs` (~60줄)
  - "45.5" -> 45.5f, true
  - "NaN" -> false, error
  - "Infinity" -> false, error
  - "200" (max=180) -> false, error
  - "-180" (min=-180) -> true

### PlayMode (기존 파일 확장)
- `Assets/Tests/PlayMode/UxFlowSmokeTests.cs` (+40줄)
  - L0 slider change -> EE trail 동작 확인
  - L0 -> L1 step 진행 확인
  - L3 target reach -> gate 충족 확인
  - Joint input 양방향 동기화 확인

### 기준값
- EditMode: 47 -> 약 57개
- PlayMode: 26 -> 약 30개

---

## 문서 Sync 체크리스트 (매 Phase 완료 후)

- [ ] `docs/ref/product/roadmap/current-feature-checklist.md` — 구현 항목 체크
- [ ] `docs/status/PROJECT-STATUS.md` — 반영 항목 추가
- [ ] `docs/ref/tutor-step-plan.md` — L0~L3 구현 상태 갱신
- [ ] `docs/ref/USER-FLOW.md` — beginner 분기 구현 반영
- [ ] `docs/ref/architecture-mermaid.md` — 새 컴포넌트 추가
- [ ] `docs/status/PHASE-EXECUTION-BOARD.md` — Phase 5 진행 표시
- [ ] `CLAUDE.md` — 현재 상태 섹션 갱신

---

## Verification

1. Unity 컴파일 에러 0
2. EditMode 전체 통과
3. PlayMode 전체 통과
4. Play 모드에서 L0 진입 -> 슬라이더 움직이면 EE trail 표시 확인
5. L0 gate 충족 -> Next 활성 -> L1 진입 확인
6. L3 target 도달 -> gate 충족 확인
7. Joint Input 양방향 동기화: 숫자 입력 -> 슬라이더 이동, 슬라이더 -> 숫자 갱신
8. WhyItMoved 패널: 관절 변경 시 실시간 갱신 확인
9. Joint Highlight: 입력 포커스 시 3D 링크 강조 확인
10. `pre_kinematics`와 `core_kinematics` resume 위치가 서로 섞이지 않는지 확인
11. Template apply / DH edit 직후 `Why It Moved`가 잘못된 관절 이동 문구를 띄우지 않는지 확인
