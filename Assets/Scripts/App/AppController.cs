using System;
using System.Linq;
using KineTutor3D.Kinematics;
using KineTutor3D.Math;
using KineTutor3D.Templates;
using KineTutor3D.Types;
using KineTutor3D.UI;
using KineTutor3D.UI.Data;
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
        [SerializeField] private OnboardingManager onboardingManager;
        [SerializeField] private ToastNotificationController toastController;
        [SerializeField] private FocusZoneHighlighter focusHighlighter;
        [SerializeField] private Slider jointSlider1;
        [SerializeField] private Slider jointSlider2;
        [SerializeField] private DHTableEditor dhTableEditor;
        [SerializeField] private TemplateSelector templateSelector;
        [SerializeField] private MatrixDisplay matrixDisplay;

        private int currentStepIndex;
        private RobotTemplate currentTemplate;
        private DHLink[] currentLinks = Array.Empty<DHLink>();
        private double[] currentJointValuesRad = Array.Empty<double>();
        private TutorPose currentEndEffectorPose;
        private Mat4D currentEndEffectorTransform = Mat4D.Identity;
        private Mat4D currentA1 = Mat4D.Identity;
        private Mat4D currentA2 = Mat4D.Identity;
        private Mat4D currentT02 = Mat4D.Identity;
        private bool sliderListenersBound;

        public event Action<int, TutorStepConfig> OnStepChanged;
        public event Action<InteractionType, string> OnInteractionEvent;
        public event Action<RobotTemplate> OnTemplateChanged;
        public event Action<Mat4D, Mat4D, Mat4D, TutorPose> OnKinematicsUpdated;

        public int CurrentStep => currentStepIndex + 1;
        public int TotalSteps => stepConfigs?.Length ?? 0;
        public RobotTemplate CurrentTemplate => currentTemplate;
        public DHLink[] CurrentLinks => (DHLink[])currentLinks.Clone();
        public double[] CurrentJointValuesRad => (double[])currentJointValuesRad.Clone();
        public TutorPose CurrentEndEffectorPose => currentEndEffectorPose;
        public Mat4D CurrentEndEffectorTransform => currentEndEffectorTransform;
        public Mat4D CurrentA1 => currentA1;
        public Mat4D CurrentA2 => currentA2;
        public Mat4D CurrentT02 => currentT02;

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

            if (TotalSteps <= 0)
            {
                Debug.LogWarning("[AppController] Step config is empty.");
                return;
            }

            stepNavigator?.Bind(this);

            if (onboardingManager != null)
            {
                onboardingManager.Initialize(this);
                return;
            }

            SetCurrentStep(Mathf.Clamp(StepProgressSaver.GetResumeStep(1), 1, TotalSteps));
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

            currentStepIndex = Mathf.Clamp(oneBasedStep - 1, 0, TotalSteps - 1);
            var config = stepConfigs[currentStepIndex];

            disclosureController?.ApplyStep(config);
            focusHighlighter?.ApplyFocus(config.focusTarget, config.focusHighlightColor);
            gateController?.LoadStep(config, CurrentStep);
            stepTutorPanel?.ApplyStep(config, CurrentStep, TotalSteps, gateController == null || gateController.IsGateSatisfied, gateController?.GetProgressText() ?? string.Empty);

            if (stepNavigator != null)
            {
                stepNavigator.UpdateStep(CurrentStep, TotalSteps);
                stepNavigator.SetPreviousInteractable(CurrentStep > 1);
                stepNavigator.SetSkipVisible(CurrentStep < TotalSteps);
                stepNavigator.SetNextInteractable(gateController == null || gateController.IsGateSatisfied || CurrentStep >= TotalSteps);
            }

            OnStepChanged?.Invoke(CurrentStep, config);
        }

        public void NextStep()
        {
            if (CurrentStep >= TotalSteps)
            {
                return;
            }

            StepProgressSaver.SaveLastCompletedStep(CurrentStep);
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

        /// <summary>
        /// 템플릿 선택 드롭다운에 제공할 템플릿 목록을 반환합니다.
        /// </summary>
        public string[] GetAvailableTemplateNames()
        {
            return new[] { Template2DOF_RR.Name };
        }

        /// <summary>
        /// 템플릿 이름으로 템플릿을 적용합니다.
        /// </summary>
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

        /// <summary>
        /// 템플릿을 적용하고 관련 런타임 상태를 초기화합니다.
        /// </summary>
        public void ApplyTemplate(RobotTemplate template)
        {
            if (template == null)
            {
                throw new ArgumentNullException(nameof(template));
            }

            currentTemplate = template;
            currentLinks = template.GetLinks();
            currentJointValuesRad = new double[template.Dof];

            ConfigureJointSlider(0, jointSlider1);
            ConfigureJointSlider(1, jointSlider2);
            RecomputeForwardKinematics();
            OnTemplateChanged?.Invoke(currentTemplate);
        }

        /// <summary>
        /// 특정 관절의 각도를 degree 단위로 설정하고 FK를 갱신합니다.
        /// </summary>
        public void SetJointAngleDegrees(int jointIndex, float degrees)
        {
            if (currentTemplate == null)
            {
                return;
            }

            if (jointIndex < 0 || jointIndex >= currentJointValuesRad.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(jointIndex));
            }

            if (float.IsNaN(degrees) || float.IsInfinity(degrees))
            {
                throw new ArgumentException($"유효하지 않은 관절 각도입니다: {degrees}", nameof(degrees));
            }

            currentJointValuesRad[jointIndex] = DegreesToRadians(degrees);
            SetSliderValueWithoutCallback(jointIndex, degrees);
            RecomputeForwardKinematics();
        }

        /// <summary>
        /// DH 필드를 갱신합니다. Theta는 읽기전용으로 거부됩니다.
        /// </summary>
        public bool TrySetDhParameter(int linkIndex, DhEditableField field, double value, out string error)
        {
            error = string.Empty;

            if (currentTemplate == null || currentLinks == null || currentLinks.Length == 0)
            {
                error = "템플릿이 초기화되지 않았습니다.";
                return false;
            }

            if (linkIndex < 0 || linkIndex >= currentLinks.Length)
            {
                error = $"유효하지 않은 링크 인덱스입니다: {linkIndex}";
                return false;
            }

            if (double.IsNaN(value) || double.IsInfinity(value))
            {
                error = "NaN/Infinity는 허용되지 않습니다.";
                return false;
            }

            if (field == DhEditableField.Theta)
            {
                error = "theta는 읽기전용입니다. 슬라이더를 사용하세요.";
                return false;
            }

            var link = currentLinks[linkIndex];
            DHLink updated;

            switch (field)
            {
                case DhEditableField.D:
                    updated = new DHLink(link.Theta, value, link.A, link.Alpha, link.JointType);
                    break;
                case DhEditableField.A:
                    updated = new DHLink(link.Theta, link.D, value, link.Alpha, link.JointType);
                    break;
                case DhEditableField.Alpha:
                    updated = new DHLink(link.Theta, link.D, link.A, value, link.JointType);
                    break;
                default:
                    error = "지원하지 않는 DH 필드입니다.";
                    return false;
            }

            currentLinks[linkIndex] = updated;
            RecomputeForwardKinematics();
            return true;
        }

        public void ReportInteraction(InteractionType interactionType, string targetId)
        {
            OnInteractionEvent?.Invoke(interactionType, targetId);
            gateController?.RegisterInteraction(interactionType, targetId);
            stepTutorPanel?.UpdateGateState(gateController == null || gateController.IsGateSatisfied, gateController?.GetProgressText() ?? string.Empty);
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
            disclosureController ??= FindFirstObjectByType<ProgressiveDisclosureController>(FindObjectsInactive.Include);
            gateController ??= FindFirstObjectByType<InteractionGateController>(FindObjectsInactive.Include);
            stepTutorPanel ??= FindFirstObjectByType<StepTutorPanel>(FindObjectsInactive.Include);
            stepNavigator ??= FindFirstObjectByType<StepNavigator>(FindObjectsInactive.Include);
            onboardingManager ??= FindFirstObjectByType<OnboardingManager>(FindObjectsInactive.Include);
            toastController ??= FindFirstObjectByType<ToastNotificationController>(FindObjectsInactive.Include);
            focusHighlighter ??= FindFirstObjectByType<FocusZoneHighlighter>(FindObjectsInactive.Include);
            dhTableEditor ??= FindFirstObjectByType<DHTableEditor>(FindObjectsInactive.Include);
            templateSelector ??= FindFirstObjectByType<TemplateSelector>(FindObjectsInactive.Include);
            matrixDisplay ??= FindFirstObjectByType<MatrixDisplay>(FindObjectsInactive.Include);

            if (jointSlider1 == null)
            {
                var go = GameObject.Find("joint_slider_1");
                if (go != null)
                {
                    jointSlider1 = go.GetComponent<Slider>();
                }
            }

            if (jointSlider2 == null)
            {
                var go = GameObject.Find("joint_slider_2");
                if (go != null)
                {
                    jointSlider2 = go.GetComponent<Slider>();
                }
            }

            if (templateSelector == null)
            {
                var topBar = GameObject.Find("TopBar");
                if (topBar != null)
                {
                    templateSelector = topBar.GetComponent<TemplateSelector>() ?? topBar.AddComponent<TemplateSelector>();
                }
            }

            if (dhTableEditor == null)
            {
                var leftPanel = GameObject.Find("LeftPanel");
                if (leftPanel != null)
                {
                    dhTableEditor = leftPanel.GetComponent<DHTableEditor>() ?? leftPanel.AddComponent<DHTableEditor>();
                }
            }

            if (matrixDisplay == null)
            {
                var rightPanel = GameObject.Find("RightPanel");
                if (rightPanel != null)
                {
                    matrixDisplay = rightPanel.GetComponent<MatrixDisplay>() ?? rightPanel.AddComponent<MatrixDisplay>();
                }
            }
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
            templateSelector?.Bind(this);
            dhTableEditor?.Bind(this);
            matrixDisplay?.Bind(this);
        }

        private void ConfigureJointSlider(int jointIndex, Slider slider)
        {
            if (slider == null)
            {
                return;
            }

            if (currentTemplate == null || jointIndex >= currentTemplate.Dof)
            {
                slider.gameObject.SetActive(false);
                return;
            }

            slider.gameObject.SetActive(true);

            var limit = currentTemplate.GetJointLimit(jointIndex);
            var minDeg = (float)(limit.Min * Mathf.Rad2Deg);
            var maxDeg = (float)(limit.Max * Mathf.Rad2Deg);

            slider.minValue = minDeg;
            slider.maxValue = maxDeg;
            slider.wholeNumbers = false;
            slider.SetValueWithoutNotify(0f);
            currentJointValuesRad[jointIndex] = 0.0;
        }

        private void BindSliderEvents()
        {
            if (sliderListenersBound)
            {
                return;
            }

            if (jointSlider1 != null)
            {
                jointSlider1.onValueChanged.AddListener(OnJointSlider1Changed);
            }

            if (jointSlider2 != null)
            {
                jointSlider2.onValueChanged.AddListener(OnJointSlider2Changed);
            }

            sliderListenersBound = true;
        }

        private void UnbindSliderEvents()
        {
            if (!sliderListenersBound)
            {
                return;
            }

            if (jointSlider1 != null)
            {
                jointSlider1.onValueChanged.RemoveListener(OnJointSlider1Changed);
            }

            if (jointSlider2 != null)
            {
                jointSlider2.onValueChanged.RemoveListener(OnJointSlider2Changed);
            }

            sliderListenersBound = false;
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
            if (currentTemplate == null || jointIndex >= currentJointValuesRad.Length)
            {
                return;
            }

            currentJointValuesRad[jointIndex] = DegreesToRadians(valueDegrees);
            RecomputeForwardKinematics();
        }

        private void RecomputeForwardKinematics()
        {
            if (currentLinks == null || currentLinks.Length == 0 || currentJointValuesRad == null || currentJointValuesRad.Length == 0)
            {
                currentA1 = Mat4D.Identity;
                currentA2 = Mat4D.Identity;
                currentT02 = Mat4D.Identity;
                currentEndEffectorTransform = Mat4D.Identity;
                currentEndEffectorPose = new TutorPose(currentEndEffectorTransform.ExtractPosition(), currentEndEffectorTransform.ExtractRotation());
                OnKinematicsUpdated?.Invoke(currentA1, currentA2, currentT02, currentEndEffectorPose);
                return;
            }

            currentA1 = currentLinks.Length > 0 ? DHStandard.ComputeA(currentLinks[0], currentJointValuesRad[0]) : Mat4D.Identity;
            currentA2 = currentLinks.Length > 1 ? DHStandard.ComputeA(currentLinks[1], currentJointValuesRad[1]) : Mat4D.Identity;

            var all = ForwardKinematics.ComputeAll(currentLinks, currentJointValuesRad);
            currentEndEffectorTransform = all[all.Length - 1];
            currentT02 = all[System.Math.Min(1, all.Length - 1)];
            currentEndEffectorPose = new TutorPose(currentEndEffectorTransform.ExtractPosition(), currentEndEffectorTransform.ExtractRotation());

            OnKinematicsUpdated?.Invoke(currentA1, currentA2, currentT02, currentEndEffectorPose);
        }

        private void SetSliderValueWithoutCallback(int jointIndex, float valueDegrees)
        {
            var slider = jointIndex switch
            {
                0 => jointSlider1,
                1 => jointSlider2,
                _ => null
            };

            if (slider == null)
            {
                return;
            }

            slider.SetValueWithoutNotify(valueDegrees);
        }

        private static double DegreesToRadians(float degrees)
        {
            return degrees * (System.Math.PI / 180.0);
        }
    }
}
