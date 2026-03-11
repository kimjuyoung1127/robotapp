// Folder: App - application orchestration and runtime state.
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
        [SerializeField] private RobotRenderer robotRenderer;
        [SerializeField] private EndEffectorTrail endEffectorTrail;
        [SerializeField] private TargetMarkerVisual targetMarkerVisual;

        private int currentStepIndex;
        private bool sliderListenersBound;
        private readonly StepFlowService stepFlowService = new StepFlowService();
        private readonly KinematicsRuntimeService kinematicsService = new KinematicsRuntimeService();
        private readonly AppUiBinder uiBinder = new AppUiBinder();
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
        public string CurrentTrack => currentTrack;
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
            AutoWireReferences();
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
        }

        public void NextStep()
        {
            if (CurrentStep >= TotalSteps)
            {
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

        public void JumpToSandbox()
        {
            SetCurrentStep(TotalSteps);
        }

        public string[] GetAvailableTemplateNames()
        {
            return new[] { Template2DOF_RR.Name };
        }

        public void SelectTemplateByName(string templateName)
        {
            if (string.IsNullOrWhiteSpace(templateName))
            {
                return;
            }

            if (string.Equals(templateName, Template2DOF_RR.Name, StringComparison.Ordinal))
            {
                ApplyTemplate(Template2DOF_RR.Create());
            }
        }

        public void ApplyTemplate(RobotTemplate template)
        {
            kinematicsService.ApplyTemplate(template, jointSlider1, jointSlider2);
            PublishKinematicsUpdate();
            OnTemplateChanged?.Invoke(CurrentTemplate);
        }

        public void SetJointAngleDegrees(int jointIndex, float degrees)
        {
            kinematicsService.SetJointAngleDegrees(jointIndex, degrees, jointSlider1, jointSlider2);
            RequestJointFocus(jointIndex);
            PublishKinematicsUpdate();
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
            uiBinder.AutoWire(ref disclosureController, ref gateController, ref stepTutorPanel, ref stepNavigator, ref toastController, ref focusHighlighter, ref jointSlider1, ref jointSlider2, ref dhTableEditor, ref templateSelector, ref matrixDisplay, ref jointInputRail, ref robotRenderer, ref endEffectorTrail, ref targetMarkerVisual);
        }

        private void LoadStepConfigsIfNeeded()
        {
            if (stepConfigs != null && stepConfigs.Length > 0)
            {
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
            BindSliderEvents();
            ApplyTemplate(Template2DOF_RR.Create());
        }

        private void BindRuntimeUiControllers()
        {
            uiBinder.BindRuntimeControllers(this, templateSelector, dhTableEditor, matrixDisplay, jointInputRail, endEffectorTrail, targetMarkerVisual);
        }

        private void BindSliderEvents()
        {
            uiBinder.BindSliderEvents(jointSlider1, jointSlider2, OnJointSlider1Changed, OnJointSlider2Changed, ref sliderListenersBound);
        }

        private void UnbindSliderEvents()
        {
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
        }

        private void PublishKinematicsUpdate()
        {
            OnKinematicsUpdated?.Invoke(CurrentA1, CurrentA2, CurrentT02, CurrentEndEffectorPose);
        }

        private void ApplyFeatureState(TutorStepConfig config)
        {
            if (config == null)
            {
                return;
            }

            jointHighlightEnabled = config.showJointHighlight;
            jointInputRail?.SetRailVisible(config.showJointInputRail);
            endEffectorTrail?.SetTrailVisible(config.showEndEffectorTrail);
            targetMarkerVisual?.SetMarkersVisible(config.showTargetMarkers);
            targetMarkerVisual?.ClearFeedback();

            if (!jointHighlightEnabled)
            {
                ClearJointFocus();
                robotRenderer?.ClearJointHighlight();
            }
        }
    }
}
