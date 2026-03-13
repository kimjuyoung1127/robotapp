// Folder: UI - HUD/view components only; no kinematics logic.
using System;
using KineTutor3D.UI.Data;
using UnityEngine;

namespace KineTutor3D.UI.Data
{
    /// <summary>
    /// 튜토리얼 스텝의 UI 가시성/포커스/게이트를 정의하는 설정 데이터입니다.
    /// </summary>
    [CreateAssetMenu(menuName = "KineTutor3D/UI/Tutor Step Config", fileName = "TutorStepConfig")]
    public class TutorStepConfig : ScriptableObject
    {
        [Header("패널 가시성")]
        public bool showLeftPanel = true;
        public bool showRightPanel = true;
        public bool showBottomBar = true;
        public LeftPanelContent leftContent = LeftPanelContent.DHTable;
        public RightPanelContent rightContent = RightPanelContent.Hidden;

        [Header("포커스")]
        public FocusTarget focusTarget = FocusTarget.None;
        public Color focusHighlightColor = default;

        [Header("시각화 토글")]
        public bool showFrameGizmos = true;
        public bool showMatrices = true;
        public bool showDHTable = true;
        public bool showSliders = true;
        public bool showAnimation = true;
        public bool showEEHighlight = false;

        [Header("Beginner Mode")]
        public bool beginnerMode;
        public bool mathReadinessMode;
        public bool showMathReadinessPanel;
        public bool showFormula = true;
        public bool showPlainLanguage;
        public bool showEndEffectorTrail;
        public bool showJointHighlight;
        public bool showTargetMarkers;
        public bool showWhyItMoved;
        public bool showMathVisualHints;
        public bool showJointInputRail = true;
        public int interactiveJointCount;
        public BeginnerLeftContent beginnerLeftContent = BeginnerLeftContent.None;
        public MathReadinessContent mathReadinessContent = MathReadinessContent.None;
        [TextArea(1, 3)] public string rationaleKo = string.Empty;
        [TextArea(1, 3)] public string commonMistakeKo = string.Empty;
        [TextArea(1, 3)] public string coachHintKo = string.Empty;
        [TextArea(1, 3)] public string warmupPromptKo = string.Empty;
        public string[] warmupChoicesKo = Array.Empty<string>();
        [TextArea(1, 3)] public string warmupFollowupKo = string.Empty;
        [TextArea(1, 3)] public string successToastKo = string.Empty;
        public string[] correctionMessagesKo = Array.Empty<string>();
        public MathReadinessQuestion[] readinessQuestions = Array.Empty<MathReadinessQuestion>();

        [Header("툴팁")]
        public TooltipEntry[] tooltips = Array.Empty<TooltipEntry>();

        [Header("게이트")]
        public GateCondition[] conditions = Array.Empty<GateCondition>();
        public string gateMeetToastKo = "학습 목표를 달성했습니다.";

        [Header("텍스트")]
        public string stepTitleKo = "Step";
        [TextArea(2, 6)] public string objectiveKo = string.Empty;
        [TextArea(2, 6)] public string hintKo = string.Empty;
    }

    /// <summary>
    /// 좌측 패널 콘텐츠 타입입니다.
    /// </summary>
    public enum LeftPanelContent
    {
        Hidden,
        DHTable,
        FourMatrices,
        MultiplicationProgress,
        DHReference,
        CumulativeProduct,
        T0nAndExtract,
        FullDH
    }

    /// <summary>
    /// 우측 패널 콘텐츠 타입입니다.
    /// </summary>
    public enum RightPanelContent
    {
        Hidden,
        FrameInfoOverlay,
        AiColorCoding,
        A1A2Reference,
        PoseExtract,
        FullMatrices
    }

    /// <summary>
    /// 포커스 하이라이트 타겟 타입입니다.
    /// </summary>
    public enum FocusTarget
    {
        None,
        LeftPanel,
        RightPanel,
        BottomBar,
        Viewport3D,
        DHTable,
        MatrixPanel,
        EndEffectorFrame
    }
}

