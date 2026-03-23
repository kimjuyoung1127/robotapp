// Folder: App - Application controllers and services; single UnityEngine entry point.
using System;
using System.Linq;
using KineTutor3D.Math;
using KineTutor3D.Templates;
using KineTutor3D.Types;
using KineTutor3D.UI;
using KineTutor3D.UI.Data;
using KineTutor3D.Visualization;
using UnityEngine;
using UnityEngine.UI;
using TutorPose = KineTutor3D.Types.Pose;

namespace KineTutor3D.App
{
    /// <summary>
    /// 튜토리얼 스텝 전환과 UI 학습 상태 동기화를 담당합니다.
    /// </summary>
    public class AppController : MonoBehaviour
    {
        [Header("Step Data")]
        [SerializeField] private TutorStepConfig[] stepConfigs = Array.Empty<TutorStepConfig>();

        [Header("UI References")]
        [SerializeField] private ProgressiveDisclosureController disclosureController;
        [SerializeField] private InteractionGateController gateController;
        [SerializeField] private StepTutorPanel stepTutorPanel;
        [SerializeField] private StepNavigator stepNavigator;
        [SerializeField] private ToastNotificationController toastController;
        [SerializeField] private FocusZoneHighlighter focusHighlighter;
        [SerializeField] private Slider jointSlider1;
        [SerializeField] private Slider jointSlider2;
        [SerializeField] private DHTableEditor dhTableEditor;
        [SerializeField] private TemplateSelector templateSelector;
        [SerializeField] private MatrixDisplay matrixDisplay;
        [SerializeField] private JointInputRail jointInputRail;
        [SerializeField] private WhyItMovedPanel whyItMovedPanel;
        [SerializeField] private BeginnerLeftPanel beginnerLeftPanel;
        [SerializeField] private MathReadinessPanel mathReadinessPanel;
        [SerializeField] private TargetFeedbackPanel targetFeedbackPanel;
        [SerializeField] private RobotRenderer robotRenderer;
        [SerializeField] private EndEffectorTrail endEffectorTrail;
        [SerializeField] private TargetMarkerVisual targetMarkerVisual;
        [SerializeField] private MathVisualOrchestrator mathVisualOrchestrator;
        [SerializeField] private FKDiagramPanel fkDiagramPanel;

        private int currentStepIndex;
        private bool sliderListenersBound;
        private readonly StepFlowService stepFlowService = new StepFlowService();
        private readonly KinematicsRuntimeService kinematicsService = new KinematicsRuntimeService();
        private readonly AppUiBinder uiBinder = new AppUiBinder();
        private readonly AppSessionContextService sessionContextService = new AppSessionContextService();
        private string currentTrack = StepProgressSaver.CoreKinematicsTrack;
        private bool jointHighlightEnabled;

        public event Action<int, TutorStepConfig> OnStepChanged;
        public event Action<InteractionType, string> OnInteractionEvent;
        public event Action<RobotTemplate> OnTemplateChanged;
        public event Action<Mat4D, Mat4D, Mat4D, TutorPose> OnKinematicsUpdated;
        public event Action<int> OnJointFocusRequested;
        public event Action OnJointFocusCleared;

        public int CurrentStep => currentStepIndex + 1;
        public int TotalSteps => stepConfigs?.Length ?? 0;
        public int CurrentDof => CurrentTemplate != null ? CurrentTemplate.Dof : 0;
        public string CurrentTrack => currentTrack;
        public bool IsCurrentGateSatisfied => gateController == null || gateController.IsGateSatisfied;
        public string CurrentGateProgressText => gateController?.GetProgressText() ?? string.Empty;
        public TutorStepConfig CurrentStepConfig => stepConfigs != null && currentStepIndex >= 0 && currentStepIndex < stepConfigs.Length
            ? stepConfigs[currentStepIndex]
            : null;
        public RobotTemplate CurrentTemplate => kinematicsService.State.CurrentTemplate;
        public DHLink[] CurrentLinks => (DHLink[])kinematicsService.State.CurrentLinks.Clone();
        public double[] CurrentJointValuesRad => (double[])kinematicsService.State.CurrentJointValuesRad.Clone();
        public double[] PreviousJointValuesRad => (double[])kinematicsService.State.PreviousJointValuesRad.Clone();
        public TutorPose CurrentEndEffectorPose => kinematicsService.State.CurrentEndEffectorPose;
        public TutorPose PreviousEndEffectorPose => kinematicsService.State.PreviousEndEffectorPose;
        public Mat4D CurrentEndEffectorTransform => kinematicsService.State.CurrentEndEffectorTransform;
        public Mat4D PreviousEndEffectorTransform => kinematicsService.State.PreviousEndEffectorTransform;
        public Mat4D CurrentA1 => kinematicsService.State.CurrentA1;
        public Mat4D CurrentA2 => kinematicsService.State.CurrentA2;
        public Mat4D CurrentT02 => kinematicsService.State.CurrentT02;
        public int ChangedJointIndex => kinematicsService.State.ChangedJointIndex;
        public RuntimeUpdateCause LastUpdateCause => kinematicsService.State.LastUpdateCause;

        private void Awake()
        {
            EnsureDedicatedMathReadinessTrack();
            AutoWireReferences();
            currentTrack = StepProgressSaver.GetCurrentTrack();
            LoadStepConfigsIfNeeded();

            if (gateController != null)
            {
                gateController.GateStateChanged += HandleGateStateChanged;
            }
        }

        private void Start()
        {
            InitializeTemplateRuntime();
            BindRuntimeUiControllers();
            currentTrack = StepProgressSaver.GetCurrentTrack();
            if (TotalSteps <= 0)
            {
                Debug.LogWarning("[AppController] Step config is empty.");
                return;
            }

            stepNavigator?.Bind(this);
            SetCurrentStep(Mathf.Clamp(StepProgressSaver.GetResumeStep(currentTrack, 1), 1, TotalSteps));
            PersistSessionContext();
        }

        private void OnDestroy()
        {
            if (gateController != null)
            {
                gateController.GateStateChanged -= HandleGateStateChanged;
            }

            UnbindSliderEvents();
        }

        public void SetCurrentStep(int oneBasedStep)
        {
            if (TotalSteps <= 0)
            {
                return;
            }

            currentStepIndex = stepFlowService.ApplyStep(oneBasedStep, stepConfigs, disclosureController, gateController, stepTutorPanel, stepNavigator, focusHighlighter);
            ApplyFeatureState(stepConfigs[currentStepIndex]);
            OnStepChanged?.Invoke(CurrentStep, stepConfigs[currentStepIndex]);
            PersistSessionContext();
        }

        public void NextStep()
        {
            if (CurrentStep >= TotalSteps)
            {
                if (TryAdvanceFromMathReadiness())
                {
                    return;
                }

                return;
            }

            StepProgressSaver.SaveLastCompletedStep(currentTrack, CurrentStep);
            SetCurrentStep(CurrentStep + 1);
        }

        public void PreviousStep()
        {
            SetCurrentStep(CurrentStep - 1);
        }

        public void SkipCurrentStep()
        {
            if (CurrentStep >= TotalSteps)
            {
                return;
            }

            gateController?.SkipCurrentGate();
            NextStep();
        }

        public string[] GetAvailableTemplateNames()
        {
            return RobotCatalog.GetAvailableRobotIds();
        }

        public JointLimit GetJointLimit(int jointIndex)
        {
            return kinematicsService.GetJointLimit(jointIndex);
        }

        public double GetJointAngleDegrees(int jointIndex)
        {
            return kinematicsService.GetJointAngleDegrees(jointIndex);
        }

        public void SelectTemplateByName(string templateName)
        {
            if (string.IsNullOrWhiteSpace(templateName))
            {
                return;
            }

            var template = RobotCatalog.CreateTemplate(templateName);
            if (template != null)
            {
                ApplyTemplate(template);
            }
        }

        /// <summary>
        /// 지정된 DOF로 커스텀 템플릿을 생성하여 적용합니다.
        /// </summary>
        public void ApplyCustomTemplate(int dof)
        {
            var template = CustomTemplateBuilder.Create(dof);
            ApplyTemplate(template);
        }

        public void ApplyTemplate(RobotTemplate template)
        {
            kinematicsService.ApplyTemplate(template, jointSlider1, jointSlider2);
            PublishKinematicsUpdate();
            OnTemplateChanged?.Invoke(CurrentTemplate);
            PersistSessionContext();
        }

        public void SetJointAngleDegrees(int jointIndex, float degrees)
        {
            kinematicsService.SetJointAngleDegrees(jointIndex, degrees, jointSlider1, jointSlider2);
            RequestJointFocus(jointIndex);
            PublishKinematicsUpdate();
            CheckSliderTargetReached(jointIndex, degrees);
        }

        public bool TrySetDhParameter(int linkIndex, DhEditableField field, double value, out string error)
        {
            var success = kinematicsService.TrySetDhParameter(linkIndex, field, value, out error);
            if (success)
            {
                PublishKinematicsUpdate();
            }

            return success;
        }

        public void ReportInteraction(InteractionType interactionType, string targetId)
        {
            OnInteractionEvent?.Invoke(interactionType, targetId);
            gateController?.RegisterInteraction(interactionType, targetId);
            stepTutorPanel?.UpdateGateState(gateController == null || gateController.IsGateSatisfied, gateController?.GetProgressText() ?? string.Empty);
        }

        public void RequestJointFocus(int jointIndex)
        {
            if (!jointHighlightEnabled)
            {
                return;
            }

            OnJointFocusRequested?.Invoke(jointIndex);
        }

        public void ClearJointFocus()
        {
            OnJointFocusCleared?.Invoke();
        }

        private void HandleGateStateChanged(bool gateSatisfied, string completionMessage)
        {
            if (stepNavigator != null)
            {
                stepNavigator.SetNextInteractable(gateSatisfied || CurrentStep >= TotalSteps);
            }

            stepTutorPanel?.UpdateGateState(gateSatisfied, gateController?.GetProgressText() ?? string.Empty);

            if (gateSatisfied && !string.IsNullOrWhiteSpace(completionMessage))
            {
                toastController?.ShowSuccess(completionMessage, 5f);
            }
        }

        private void AutoWireReferences()
        {
            if (IsDedicatedMathReadinessScene())
            {
                ClearSharedLearningShellReferences();
            }

            uiBinder.AutoWire(ref disclosureController, ref gateController, ref stepTutorPanel, ref stepNavigator, ref toastController, ref focusHighlighter, ref jointSlider1, ref jointSlider2, ref dhTableEditor, ref templateSelector, ref matrixDisplay, ref jointInputRail, ref whyItMovedPanel, ref beginnerLeftPanel, ref mathReadinessPanel, ref targetFeedbackPanel, ref robotRenderer, ref endEffectorTrail, ref targetMarkerVisual, ref mathVisualOrchestrator, ref fkDiagramPanel);
        }

        private void EnsureDedicatedMathReadinessTrack()
        {
#if UNITY_EDITOR
            if (!IsDedicatedMathReadinessScene())
            {
                return;
            }

            var track = StepProgressSaver.GetCurrentTrack();
            if (string.Equals(track, StepProgressSaver.MathReadinessTrack, StringComparison.Ordinal))
            {
                return;
            }

            StepProgressSaver.SetCurrentTrack(StepProgressSaver.MathReadinessTrack);
            currentTrack = StepProgressSaver.MathReadinessTrack;
#endif
        }

        private void LoadStepConfigsIfNeeded()
        {
            if (stepConfigs != null && stepConfigs.Length > 0)
            {
                return;
            }

            var track = StepProgressSaver.GetCurrentTrack();
            if (string.Equals(track, StepProgressSaver.MathReadinessTrack, StringComparison.Ordinal))
            {
                stepConfigs = MathReadinessLessonFactory.CreateLessons();
                return;
            }

            if (string.Equals(track, StepProgressSaver.PreKinematicsTrack, StringComparison.Ordinal))
            {
                stepConfigs = BeginnerLessonFactory.CreateLessons();
                return;
            }

            var loaded = Resources.LoadAll<TutorStepConfig>("TutorSteps");
            if (loaded != null && loaded.Length > 0)
            {
                stepConfigs = loaded.OrderBy(x => x.name).ToArray();
                return;
            }

            stepConfigs = TutorStepRuntimeFactory.CreateDefaults();
        }

        private void InitializeTemplateRuntime()
        {
            if (!IsDedicatedMathReadinessScene())
            {
                BindSliderEvents();
            }

            var selectedId = RobotSelectionBridge.GetSelectedRobotId();
            var template = !string.IsNullOrEmpty(selectedId)
                ? RobotCatalog.CreateTemplate(selectedId)
                : null;

            ApplyTemplate(template ?? Template2DOF_RR.Create());
        }

        private void BindRuntimeUiControllers()
        {
            uiBinder.BindRuntimeControllers(this, templateSelector, dhTableEditor, matrixDisplay, jointInputRail, whyItMovedPanel, beginnerLeftPanel, mathReadinessPanel, targetFeedbackPanel, endEffectorTrail, targetMarkerVisual);
        }

        private void BindSliderEvents()
        {
            if (IsDedicatedMathReadinessScene())
            {
                sliderListenersBound = false;
                return;
            }

            uiBinder.BindSliderEvents(jointSlider1, jointSlider2, OnJointSlider1Changed, OnJointSlider2Changed, ref sliderListenersBound);
        }

        private void UnbindSliderEvents()
        {
            if (IsDedicatedMathReadinessScene())
            {
                sliderListenersBound = false;
                return;
            }

            uiBinder.UnbindSliderEvents(jointSlider1, jointSlider2, OnJointSlider1Changed, OnJointSlider2Changed, ref sliderListenersBound);
        }

        private void OnJointSlider1Changed(float value)
        {
            HandleJointSliderChanged(0, value);
        }

        private void OnJointSlider2Changed(float value)
        {
            HandleJointSliderChanged(1, value);
        }

        private void HandleJointSliderChanged(int jointIndex, float valueDegrees)
        {
            kinematicsService.HandleJointSliderChanged(jointIndex, valueDegrees);
            RequestJointFocus(jointIndex);
            PublishKinematicsUpdate();
            CheckSliderTargetReached(jointIndex, valueDegrees);
        }

        private void PublishKinematicsUpdate()
        {
            OnKinematicsUpdated?.Invoke(CurrentA1, CurrentA2, CurrentT02, CurrentEndEffectorPose);
            mathVisualOrchestrator?.UpdateFromJointAngles(CurrentJointValuesRad);
            fkDiagramPanel?.Refresh(CurrentLinks, CurrentJointValuesRad);
        }

        private void ApplyFeatureState(TutorStepConfig config)
        {
            if (config == null)
            {
                return;
            }

            // ── Step 1: 모든 콘텐츠 패널 숨김 (Reset All) ──
            HideAllContentPanels();

            // ── Step 2: 공통 시각화 상태 적용 ──
            jointHighlightEnabled = config.showJointHighlight;
            jointInputRail?.SetInteractiveJointCount(config.mathReadinessMode ? config.interactiveJointCount : 0);
            jointInputRail?.SetRailVisible(config.showJointInputRail);
            whyItMovedPanel?.SetVisible(config.showWhyItMoved);
            endEffectorTrail?.SetTrailVisible(config.showEndEffectorTrail);
            targetMarkerVisual?.SetMarkersVisible(config.showTargetMarkers);
            targetMarkerVisual?.ClearFeedback();

            // ── Step 2b: Math Visual Hints ──
            if (mathVisualOrchestrator != null)
            {
                if (config.showMathVisualHints)
                {
                    InitializeMathVisualIfNeeded();
                    mathVisualOrchestrator.SetVisible(true);
                    mathVisualOrchestrator.ApplyMathPreset(config.mathReadinessContent);
                    mathVisualOrchestrator.UpdateFromJointAngles(CurrentJointValuesRad);
                }
                else
                {
                    mathVisualOrchestrator.SetVisible(false);
                }
            }

            // ── Step 3: 현재 모드 전용 패널만 켜기 ──
            if (config.mathReadinessMode)
            {
                ApplyMathReadinessVisibility(config);
            }
            else if (config.beginnerMode)
            {
                ApplyBeginnerVisibility(config);
            }
            else
            {
                ApplyCoreVisibility(config);
            }

            if (!jointHighlightEnabled)
            {
                ClearJointFocus();
                robotRenderer?.ClearJointHighlight();
            }
        }

        /// <summary>모든 콘텐츠 패널을 숨깁니다. 모드별 메서드가 필요한 것만 다시 켭니다.</summary>
        private void HideAllContentPanels()
        {
            // Left exclusive group
            dhTableEditor?.SetVisible(false);
            beginnerLeftPanel?.SetVisible(false);
            mathReadinessPanel?.SetVisible(false);

            // Right exclusive group
            stepTutorPanel?.SetVisible(false);
            matrixDisplay?.SetVisible(false);
            whyItMovedPanel?.SetVisible(false);
            targetFeedbackPanel?.SetVisible(false);

            // TopBar controls
            templateSelector?.SetVisible(false);
        }

        private void ApplyMathReadinessVisibility(TutorStepConfig config)
        {
            whyItMovedPanel?.SetVisible(false);
            mathReadinessPanel?.ApplyConfig(config);
            mathReadinessPanel?.SetVisible(config.showMathReadinessPanel);
        }

        private void CheckSliderTargetReached(int jointIndex, float valueDegrees)
        {
            if (CurrentStepConfig?.readinessQuestions == null || mathReadinessPanel == null)
            {
                return;
            }

            if (!mathReadinessPanel.TryGetActiveManipulationQuestion(out var question) || string.IsNullOrWhiteSpace(question.targetReachGateId))
            {
                return;
            }

            if (float.IsNaN(question.targetAngleDeg))
            {
                return;
            }

            var movedPrimary = question.targetJointIndex == jointIndex;
            var movedSecondary = question.secondaryTargetJointIndex == jointIndex;
            if (!movedPrimary && !movedSecondary)
            {
                return;
            }

            if (!IsPrimaryTargetSatisfied(question) || !IsSecondaryTargetSatisfied(question))
            {
                return;
            }

            ReportInteraction(InteractionType.SliderReachTarget, question.targetReachGateId);
        }

        private bool IsPrimaryTargetSatisfied(MathReadinessQuestion question)
        {
            if (question == null || float.IsNaN(question.targetAngleDeg))
            {
                return false;
            }

            var primaryDegrees = (float)GetJointAngleDegrees(question.targetJointIndex);
            return Mathf.Abs(primaryDegrees - question.targetAngleDeg) <= question.targetAngleTolerance;
        }

        private bool IsSecondaryTargetSatisfied(MathReadinessQuestion question)
        {
            if (question == null || float.IsNaN(question.secondaryTargetAngleDeg) || question.secondaryTargetJointIndex < 0)
            {
                return true;
            }

            var secondaryDegrees = (float)GetJointAngleDegrees(question.secondaryTargetJointIndex);
            return Mathf.Abs(secondaryDegrees - question.secondaryTargetAngleDeg) <= question.secondaryTargetAngleTolerance;
        }

        private void ApplyBeginnerVisibility(TutorStepConfig config)
        {
            stepTutorPanel?.SetVisible(true);
            templateSelector?.SetVisible(true);
            dhTableEditor?.SetVisible(config.showDHTable);
            matrixDisplay?.SetVisible(config.showMatrices);
            beginnerLeftPanel?.ApplyContent(config.beginnerLeftContent);
            targetFeedbackPanel?.SetVisible(config.showTargetMarkers);
        }

        private void ApplyCoreVisibility(TutorStepConfig config)
        {
            stepTutorPanel?.SetVisible(true);
            templateSelector?.SetVisible(true);
            dhTableEditor?.SetVisible(config.showDHTable);
            matrixDisplay?.SetVisible(config.showMatrices);
        }

        private void PersistSessionContext()
        {
            sessionContextService.SaveCurrent(CurrentTemplate, false, currentTrack, CurrentStep);
        }

        private void InitializeMathVisualIfNeeded()
        {
            if (mathVisualOrchestrator == null || robotRenderer == null)
            {
                return;
            }

            var baseAnchor = robotRenderer.transform.Find("VisualRoot/BaseVisual");
            var link0Anchor = baseAnchor != null ? baseAnchor.Find("Link0Visual") : null;
            var link1Anchor = link0Anchor != null ? link0Anchor.Find("Link1Visual") : null;
            var eeAnchor = robotRenderer.transform.Find("Frame_EE");

            if (baseAnchor != null && link0Anchor != null)
            {
                mathVisualOrchestrator.Initialize(baseAnchor, link0Anchor, link1Anchor, eeAnchor);
            }
        }

        private bool TryAdvanceFromMathReadiness()
        {
            if (!string.Equals(currentTrack, StepProgressSaver.MathReadinessTrack, StringComparison.Ordinal))
            {
                return false;
            }

            StepProgressSaver.SaveLastCompletedStep(currentTrack, CurrentStep);
            currentTrack = StepProgressSaver.PreKinematicsTrack;
            StepProgressSaver.SetCurrentTrack(currentTrack);
            StepProgressSaver.SaveLastCompletedStep(currentTrack, 0);

            if (SceneCatalog.GetCurrentSceneId() == SceneId.MathReadiness)
            {
                RobotSelectionBridge.SetSelection(Template2DOF_RR.Name, RobotSelectionBridge.GuidedLessonMode);
                SceneNavigator.Load(SceneId.RobotLibrary);
                return true;
            }

            stepConfigs = BeginnerLessonFactory.CreateLessons();
            ApplyTemplate(Template2DOF_RR.Create());
            stepNavigator?.Bind(this);
            SetCurrentStep(1);
            toastController?.ShowSuccess("좋아요! 이제 로봇 직관 lesson으로 넘어갈게요.", 4f);
            return true;
        }

        private bool IsDedicatedMathReadinessScene()
        {
            return SceneCatalog.GetCurrentSceneId() == SceneId.MathReadiness;
        }

        private void ClearSharedLearningShellReferences()
        {
            stepTutorPanel = null;
            stepNavigator = null;
            dhTableEditor = null;
            templateSelector = null;
            matrixDisplay = null;
            jointInputRail = null;
            whyItMovedPanel = null;
            beginnerLeftPanel = null;
            targetFeedbackPanel = null;
            jointSlider1 = null;
            jointSlider2 = null;
        }
    }
}
