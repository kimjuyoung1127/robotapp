// Folder: UI - HUD/view components only; no kinematics logic.
using System.Collections;
using KineTutor3D.App;
using KineTutor3D.Templates;
using KineTutor3D.UI.Data;
using UnityEngine;
using UnityEngine.UI;

namespace KineTutor3D.UI
{
    [ExecuteAlways]
    [DisallowMultipleComponent]
    public class MathReadinessPanel : MonoBehaviour, IVisibilityControllable
    {
        private const string JointSlider2Path = "PageFooter/JointControlCard/joint_slider_2";
        private const string LegacyJointSlider2Path = "PageFooter/JointControlCard/legacy_joint_slider_2";

        [SerializeField] private AppController appController;
        [SerializeField] private ToastNotificationController toastController;
        [SerializeField] private RectTransform pageRoot;
        [SerializeField] private CanvasGroup pageCanvasGroup;
        [SerializeField] private Font fallbackFont;

        private TutorStepConfig currentConfig;
        private int currentQuestionIndex;
        private bool warmupCompleted;
        private bool coachVisible;
        private bool allCorrectFirstTry = true;
        private bool manipulationPhase;
        private bool listenersBound;
        private bool appBound;
        private bool layoutBound;
        private bool missingRefsLogged;
        private bool missingOptionalRefsLogged;

        private Button backButton;
        private Button homeButton;
        private Button libraryButton;
        private Button sandboxButton;
        private Text pageTitleText;
        private Text pageContextText;
        private Text viewportPanelTitleText;
        private Text viewportPanelBodyText;

        private Image conceptStripe;
        private Text lessonEyebrowText;
        private Text lessonTitleText;
        private Text lessonGoalText;
        private Text introText;

        private RectTransform warmupBlock;
        private Text warmupSectionLabel;
        private Text warmupPromptText;
        private Text warmupFeedbackText;
        private readonly Button[] warmupButtons = new Button[3];

        private RectTransform manipulationBlock;
        private Text questionSectionLabel;
        private Text manipulationInstructionText;
        private RectTransform targetBadge;
        private Text targetBadgeText;

        private Text questionPromptText;
        private readonly Button[] questionButtons = new Button[3];
        private Color[] questionButtonOriginalColors = System.Array.Empty<Color>();
        private RectTransform feedbackRow;
        private Image feedbackIcon;
        private Text feedbackText;
        private CanvasGroup feedbackCanvasGroup;
        private Text adaptiveHintText;
        private RectTransform progressBadge;
        private Text progressBadgeText;

        private Text commonMistakeText;
        private Button coachToggleButton;
        private Text coachHintText;
        private RectTransform whyMovedSummaryCard;
        private Text whyMovedSummaryText;

        private Slider jointSlider1;
        private Slider jointSlider2;
        private InputField jointInput1;
        private InputField jointInput2;
        private Text joint1ValueText;
        private Text joint2ValueText;
        private Text stepTitleText;
        private Text stepProgressText;
        private Button prevButton;
        private Button skipButton;
        private Button nextButton;

        private readonly WhyItMovedState whyItMovedState = new WhyItMovedState();
        private Coroutine feedbackFadeCoroutine;
        private Coroutine wrongColorResetCoroutine;
        private bool suppressFooterCallbacks;

        public string CurrentFeedbackText => feedbackText != null ? feedbackText.text : string.Empty;
        public bool AllCorrectFirstTry => allCorrectFirstTry;
        public bool IsManipulationPhase => manipulationPhase;

        public int CurrentQuestionAttemptCount
        {
            get
            {
                if (currentConfig == null || currentConfig.readinessQuestions == null || currentConfig.readinessQuestions.Length == 0)
                {
                    return 0;
                }

                var index = Mathf.Clamp(currentQuestionIndex, 0, currentConfig.readinessQuestions.Length - 1);
                return currentConfig.readinessQuestions[index].attemptCount;
            }
        }

        private void Awake()
        {
            fallbackFont = UiRuntimeStyle.ResolveFont(fallbackFont);
            EnsureLayout();
        }

        private void OnEnable()
        {
            EnsureLayout();
            BindListeners();

            if (Application.isPlaying && appController == null)
            {
                appController = FindFirstObjectByType<AppController>(FindObjectsInactive.Include);
                if (appController != null && !appBound)
                {
                    Bind(appController);
                    return;
                }
            }

            if (Application.isPlaying && currentConfig == null)
            {
                SetVisible(false);
            }
        }

        private void OnDisable()
        {
            UnbindListeners();
            Unbind();
        }

        private void OnDestroy()
        {
            UnbindListeners();
            Unbind();
        }

        public void Bind(AppController owner)
        {
            Unbind();
            appController = owner;
            toastController ??= FindFirstObjectByType<ToastNotificationController>(FindObjectsInactive.Include);

            if (appController != null)
            {
                appController.OnStepChanged += HandleStepChanged;
                appController.OnInteractionEvent += HandleInteractionEvent;
                appController.OnKinematicsUpdated += HandleKinematicsUpdated;
                appBound = true;
            }

            EnsureLayout();
            BindListeners();
            EnsureTemplateInitialized();

            if (appController != null && appController.CurrentStepConfig != null && appController.CurrentStepConfig.mathReadinessMode)
            {
                ApplyConfig(appController.CurrentStepConfig);
                SetVisible(appController.CurrentStepConfig.showMathReadinessPanel);
            }
            else
            {
                RefreshFooter();
                RefreshWhyMovedSummary();
            }
        }

        public void SetVisible(bool visible)
        {
            if (pageCanvasGroup != null)
            {
                pageCanvasGroup.alpha = visible ? 1f : 0f;
                pageCanvasGroup.blocksRaycasts = visible;
                pageCanvasGroup.interactable = visible;
                return;
            }

            if (pageRoot != null)
            {
                pageRoot.gameObject.SetActive(visible);
            }
        }

        public void ApplyConfig(TutorStepConfig config)
        {
            currentConfig = config;
            currentQuestionIndex = 0;
            warmupCompleted = string.IsNullOrWhiteSpace(config?.warmupPromptKo);
            coachVisible = false;
            allCorrectFirstTry = true;
            ResetQuestionAttempts();
            manipulationPhase = HasCurrentManipulationRequirement();
            RefreshPage();
        }

        public bool TryGetActiveManipulationQuestion(out MathReadinessQuestion question)
        {
            question = null;
            if (!manipulationPhase || currentConfig?.readinessQuestions == null || currentConfig.readinessQuestions.Length == 0)
            {
                return false;
            }

            var index = Mathf.Clamp(currentQuestionIndex, 0, currentConfig.readinessQuestions.Length - 1);
            question = currentConfig.readinessQuestions[index];
            return question != null && question.requiresManipulationFirst;
        }

        private void EnsureLayout()
        {
            if (layoutBound)
            {
                return;
            }

            fallbackFont = UiRuntimeStyle.ResolveFont(fallbackFont);
            layoutBound = TryBindAuthoredLayout();

            if (!layoutBound && !missingRefsLogged)
            {
                Debug.LogWarning("[MathReadinessPanel] Authored layout is incomplete. MathReadinessPageRoot hierarchy is required.");
                missingRefsLogged = true;
            }
        }

        private bool TryBindAuthoredLayout()
        {
            pageRoot ??= transform as RectTransform;
            if (pageRoot == null)
            {
                return false;
            }

            pageCanvasGroup ??= pageRoot.GetComponent<CanvasGroup>();

            backButton = FindButton("PageTopBar/BackButton");
            homeButton = FindButton("PageTopBar/HomeButton");
            libraryButton = FindButton("PageTopBar/LibraryButton");
            sandboxButton = FindButton("PageTopBar/SandboxButton");
            pageTitleText = FindText("PageTopBar/PageTitleText");
            pageContextText = FindText("PageTopBar/PageContextText");
            viewportPanelTitleText = FindText("PageViewportPanel/ViewportPanelTitleText");
            viewportPanelBodyText = FindText("PageViewportPanel/ViewportPanelBodyText");

            conceptStripe = FindImage("MainContent/QuestionColumn/OverviewCard/MRP_ConceptStripe");
            lessonEyebrowText = FindText("MainContent/QuestionColumn/OverviewCard/MRP_OverviewLabel");
            lessonTitleText = FindText("MainContent/QuestionColumn/OverviewCard/MRP_LessonTitle");
            lessonGoalText = FindText("MainContent/QuestionColumn/OverviewCard/MRP_LessonGoal");
            introText = FindText("MainContent/QuestionColumn/OverviewCard/MRP_Intro");

            warmupBlock = FindRect("MainContent/QuestionColumn/QuestionCard/QuestionStack/WarmupBlock");
            warmupSectionLabel = FindText("MainContent/QuestionColumn/QuestionCard/QuestionStack/WarmupBlock/MRP_WarmupLabel");
            warmupPromptText = FindText("MainContent/QuestionColumn/QuestionCard/QuestionStack/WarmupBlock/MRP_WarmupPrompt");
            warmupFeedbackText = FindText("MainContent/QuestionColumn/QuestionCard/QuestionStack/WarmupBlock/MRP_WarmupFeedback");
            warmupButtons[0] = FindButton("MainContent/QuestionColumn/QuestionCard/QuestionStack/WarmupBlock/BtnWarmupChoice_0");
            warmupButtons[1] = FindButton("MainContent/QuestionColumn/QuestionCard/QuestionStack/WarmupBlock/BtnWarmupChoice_1");
            warmupButtons[2] = FindButton("MainContent/QuestionColumn/QuestionCard/QuestionStack/WarmupBlock/BtnWarmupChoice_2");

            manipulationBlock = FindRect("MainContent/QuestionColumn/QuestionCard/QuestionStack/ManipulationBlock");
            questionSectionLabel = FindText("MainContent/QuestionColumn/QuestionCard/QuestionSectionLabel");
            manipulationInstructionText = FindText("MainContent/QuestionColumn/QuestionCard/QuestionStack/ManipulationBlock/MRP_ManipulationInstruction");
            targetBadge = FindRect("MainContent/QuestionColumn/QuestionCard/QuestionStack/ManipulationBlock/MRP_TargetBadge");
            targetBadgeText = FindText("MainContent/QuestionColumn/QuestionCard/QuestionStack/ManipulationBlock/MRP_TargetBadge/Label");

            questionPromptText = FindText("MainContent/QuestionColumn/QuestionCard/QuestionStack/MRP_QuestionPrompt");
            questionButtons[0] = FindButton("MainContent/QuestionColumn/QuestionCard/QuestionStack/AnswerButtons/BtnReadinessChoice_0");
            questionButtons[1] = FindButton("MainContent/QuestionColumn/QuestionCard/QuestionStack/AnswerButtons/BtnReadinessChoice_1");
            questionButtons[2] = FindButton("MainContent/QuestionColumn/QuestionCard/QuestionStack/AnswerButtons/BtnReadinessChoice_2");
            feedbackRow = FindRect("MainContent/QuestionColumn/QuestionCard/QuestionStack/FeedbackRow");
            feedbackIcon = FindImage("MainContent/QuestionColumn/QuestionCard/QuestionStack/FeedbackRow/MRP_FeedbackIcon");
            feedbackText = FindText("MainContent/QuestionColumn/QuestionCard/QuestionStack/FeedbackRow/MRP_FeedbackText");
            feedbackCanvasGroup = feedbackText != null ? feedbackText.GetComponent<CanvasGroup>() : null;
            adaptiveHintText = FindText("MainContent/QuestionColumn/QuestionCard/QuestionStack/MRP_AdaptiveHint");
            progressBadge = FindRect("MainContent/QuestionColumn/QuestionCard/MRP_ProgressBadge");
            progressBadgeText = FindText("MainContent/QuestionColumn/QuestionCard/MRP_ProgressBadge/Label");

            commonMistakeText = FindText("MainContent/SupportColumn/CoachCard/MRP_CommonMistake");
            coachToggleButton = FindButton("MainContent/SupportColumn/CoachCard/BtnToggleCoachHint");
            coachHintText = FindText("MainContent/SupportColumn/CoachCard/MRP_CoachHintBody");
            whyMovedSummaryCard = FindRect("MainContent/SupportColumn/WhyMovedSummaryCard");
            whyMovedSummaryText = FindText("MainContent/SupportColumn/WhyMovedSummaryCard/WhyMovedSummaryText");

            jointSlider1 = FindSlider("PageFooter/JointControlCard/joint_slider_1");
            jointSlider2 = FindSlider(JointSlider2Path, LegacyJointSlider2Path);
            jointInput1 = FindInput("PageFooter/JointControlCard/Joint1Input");
            jointInput2 = FindInput("PageFooter/JointControlCard/Joint2Input");
            joint1ValueText = FindText("PageFooter/JointControlCard/Joint1ValueText");
            joint2ValueText = FindText("PageFooter/JointControlCard/Joint2ValueText");
            stepTitleText = FindText("PageFooter/FooterStepTitleText");
            stepProgressText = FindText("PageFooter/FooterStepProgressText");
            prevButton = FindButton("PageFooter/BtnPrev");
            skipButton = FindButton("PageFooter/BtnSkip");
            nextButton = FindButton("PageFooter/BtnNext");

            if (feedbackCanvasGroup == null && feedbackText != null)
            {
                feedbackCanvasGroup = feedbackText.gameObject.AddComponent<CanvasGroup>();
            }

            if (System.Array.Exists(warmupButtons, button => button == null) || System.Array.Exists(questionButtons, button => button == null))
            {
                return false;
            }

            var hasCoreRefs =
                backButton != null &&
                homeButton != null &&
                libraryButton != null &&
                sandboxButton != null &&
                pageTitleText != null &&
                pageContextText != null &&
                viewportPanelTitleText != null &&
                viewportPanelBodyText != null &&
                conceptStripe != null &&
                lessonEyebrowText != null &&
                lessonTitleText != null &&
                lessonGoalText != null &&
                introText != null &&
                warmupBlock != null &&
                warmupSectionLabel != null &&
                warmupPromptText != null &&
                warmupFeedbackText != null &&
                manipulationBlock != null &&
                questionSectionLabel != null &&
                manipulationInstructionText != null &&
                targetBadge != null &&
                targetBadgeText != null &&
                questionPromptText != null &&
                feedbackRow != null &&
                feedbackIcon != null &&
                feedbackText != null &&
                adaptiveHintText != null &&
                progressBadge != null &&
                progressBadgeText != null &&
                whyMovedSummaryCard != null &&
                whyMovedSummaryText != null &&
                jointSlider1 != null &&
                jointInput1 != null &&
                joint1ValueText != null &&
                stepTitleText != null &&
                stepProgressText != null &&
                prevButton != null &&
                skipButton != null &&
                nextButton != null;

            if (!hasCoreRefs)
            {
                return false;
            }

            questionButtonOriginalColors = new Color[questionButtons.Length];
            for (var i = 0; i < questionButtons.Length; i++)
            {
                questionButtonOriginalColors[i] = questionButtons[i].colors.normalColor;
            }

            WarnIfOptionalFooterRefsMissing();
            ApplySecondJointUiVisibility(AreSecondJointFooterRefsReady());
            UpdateCoachToggleLabel();
            return true;
        }

        private void BindListeners()
        {
            if (!layoutBound || listenersBound)
            {
                return;
            }

            backButton.onClick.RemoveAllListeners();
            backButton.onClick.AddListener(OnBackClicked);
            homeButton.onClick.RemoveAllListeners();
            homeButton.onClick.AddListener(OnHomeClicked);
            libraryButton.onClick.RemoveAllListeners();
            libraryButton.onClick.AddListener(OnLibraryClicked);
            sandboxButton.onClick.RemoveAllListeners();
            sandboxButton.onClick.AddListener(OnSandboxClicked);

            if (coachToggleButton != null)
            {
                coachToggleButton.onClick.RemoveAllListeners();
                coachToggleButton.onClick.AddListener(ToggleCoachHint);
            }

            prevButton.onClick.RemoveAllListeners();
            prevButton.onClick.AddListener(OnPrevClicked);
            skipButton.onClick.RemoveAllListeners();
            skipButton.onClick.AddListener(OnSkipClicked);
            nextButton.onClick.RemoveAllListeners();
            nextButton.onClick.AddListener(OnNextClicked);

            if (jointSlider1 != null)
            {
                jointSlider1.onValueChanged.RemoveAllListeners();
                jointSlider1.onValueChanged.AddListener(value => HandleSliderChanged(0, value));
            }

            if (jointSlider2 != null)
            {
                jointSlider2.onValueChanged.RemoveAllListeners();
                jointSlider2.onValueChanged.AddListener(value => HandleSliderChanged(1, value));
            }

            if (jointInput1 != null)
            {
                jointInput1.onEndEdit.RemoveAllListeners();
                jointInput1.onEndEdit.AddListener(value => HandleInputCommitted(0, value));
            }

            if (jointInput2 != null)
            {
                jointInput2.onEndEdit.RemoveAllListeners();
                jointInput2.onEndEdit.AddListener(value => HandleInputCommitted(1, value));
            }

            for (var i = 0; i < warmupButtons.Length; i++)
            {
                var capturedIndex = i;
                warmupButtons[i].onClick.RemoveAllListeners();
                warmupButtons[i].onClick.AddListener(() => OnWarmupChoice(capturedIndex));
            }

            for (var i = 0; i < questionButtons.Length; i++)
            {
                var capturedIndex = i;
                questionButtons[i].onClick.RemoveAllListeners();
                questionButtons[i].onClick.AddListener(() => OnQuestionChoice(capturedIndex));
            }

            listenersBound = true;
        }

        private void UnbindListeners()
        {
            listenersBound = false;
        }

        private void Unbind()
        {
            if (!appBound || appController == null)
            {
                return;
            }

            appController.OnStepChanged -= HandleStepChanged;
            appController.OnInteractionEvent -= HandleInteractionEvent;
            appController.OnKinematicsUpdated -= HandleKinematicsUpdated;
            appBound = false;
        }

        private void HandleStepChanged(int _step, TutorStepConfig config)
        {
            if (config != null && config.mathReadinessMode && config.showMathReadinessPanel)
            {
                ApplyConfig(config);
                SetVisible(true);
                return;
            }

            SetVisible(false);
        }

        private void HandleInteractionEvent(InteractionType interactionType, string targetId)
        {
            if (interactionType != InteractionType.SliderReachTarget || !manipulationPhase)
            {
                return;
            }

            if (!TryGetActiveManipulationQuestion(out var question) || string.IsNullOrWhiteSpace(question.targetReachGateId))
            {
                return;
            }

            if (!string.Equals(question.targetReachGateId, targetId, System.StringComparison.Ordinal))
            {
                return;
            }

            manipulationPhase = false;
            RefreshPage();
            ShowFeedbackWithFade("좋아요. 이제 확인 질문을 풀어볼게요.", UIDesignTokens.Colors.TextSecondary);
        }

        private void HandleKinematicsUpdated(KineTutor3D.Math.Mat4D _a1, KineTutor3D.Math.Mat4D _a2, KineTutor3D.Math.Mat4D _t02, KineTutor3D.Types.Pose _pose)
        {
            RefreshFooter();
            RefreshWhyMovedSummary();
        }

        private void RefreshPage()
        {
            if (!layoutBound || currentConfig == null)
            {
                return;
            }

            RefreshTopBar();
            RefreshViewportPanel();
            RefreshOverviewCard();
            RefreshQuestionCard();
            RefreshSupportColumn();
            RefreshFooter();
            RefreshWhyMovedSummary();
        }

        private void RefreshTopBar()
        {
            pageTitleText.text = "Math Readiness";
            pageContextText.text = $"Lesson {appController?.CurrentStep ?? 1}/{appController?.TotalSteps ?? 1}";
        }

        private void RefreshViewportPanel()
        {
            viewportPanelTitleText.text = manipulationPhase ? "먼저 화면을 보고 움직여 보세요" : "이제 화면을 보고 답을 선택해 보세요";
            viewportPanelBodyText.text = manipulationPhase
                ? currentConfig.hintKo
                : "로봇 팔의 방향과 끝점 위치를 보고 직관적으로 답을 골라 보세요.";
        }

        private void RefreshOverviewCard()
        {
            lessonEyebrowText.text = "현재 학습";
            lessonTitleText.text = currentConfig.stepTitleKo;
            lessonGoalText.text = currentConfig.objectiveKo;
            introText.text = currentConfig.hintKo;
            conceptStripe.color = MathReadinessContentTheme.GetStripeColor(currentConfig.mathReadinessContent);
            conceptStripe.gameObject.SetActive(currentConfig.mathReadinessContent != MathReadinessContent.None);
        }

        private void RefreshQuestionCard()
        {
            if (currentConfig.readinessQuestions == null || currentConfig.readinessQuestions.Length == 0)
            {
                return;
            }

            currentQuestionIndex = Mathf.Clamp(currentQuestionIndex, 0, currentConfig.readinessQuestions.Length - 1);
            var question = currentConfig.readinessQuestions[currentQuestionIndex];
            var showWarmup = !warmupCompleted && !string.IsNullOrWhiteSpace(currentConfig.warmupPromptKo);
            var showQuestion = !showWarmup;
            var showManipulation = showQuestion && manipulationPhase;

            warmupBlock.gameObject.SetActive(showWarmup);
            warmupSectionLabel.text = "워밍업";
            warmupPromptText.text = currentConfig.warmupPromptKo;
            warmupFeedbackText.gameObject.SetActive(false);
            warmupFeedbackText.text = string.Empty;

            for (var i = 0; i < warmupButtons.Length; i++)
            {
                var label = warmupButtons[i].GetComponentInChildren<Text>(true);
                if (label != null)
                {
                    label.text = i < currentConfig.warmupChoicesKo.Length ? currentConfig.warmupChoicesKo[i] : $"선택 {i + 1}";
                }

                warmupButtons[i].gameObject.SetActive(showWarmup);
            }

            manipulationBlock.gameObject.SetActive(showManipulation);
            questionSectionLabel.gameObject.SetActive(showQuestion);
            questionSectionLabel.text = showManipulation ? "먼저 조작" : "확인 질문";
            manipulationInstructionText.text = showManipulation ? question.manipulationInstructionKo : string.Empty;
            targetBadge.gameObject.SetActive(showManipulation);
            targetBadgeText.text = showManipulation ? FormatTargetBadge(question) : string.Empty;
            feedbackText.text = string.Empty;
            adaptiveHintText.text = string.Empty;

            questionPromptText.gameObject.SetActive(showQuestion && !showManipulation);
            questionPromptText.text = question.promptKo;

            UpdateProgressBadge();

            for (var i = 0; i < questionButtons.Length; i++)
            {
                var label = questionButtons[i].GetComponentInChildren<Text>(true);
                if (label != null)
                {
                    label.text = i < question.choicesKo.Length ? question.choicesKo[i] : $"선택 {i + 1}";
                }

                ResetButtonColor(i);
                questionButtons[i].gameObject.SetActive(showQuestion && !showManipulation && i < Mathf.Max(question.choicesKo.Length, 3));
                questionButtons[i].interactable = showQuestion && !showManipulation;
            }

            feedbackRow.gameObject.SetActive(showQuestion && !showManipulation);
            feedbackIcon.gameObject.SetActive(false);
            adaptiveHintText.gameObject.SetActive(!string.IsNullOrWhiteSpace(adaptiveHintText.text) && showQuestion && !showManipulation);
        }

        private void RefreshSupportColumn()
        {
            if (commonMistakeText != null)
            {
                commonMistakeText.text = string.IsNullOrWhiteSpace(currentConfig.commonMistakeKo)
                    ? string.Empty
                    : $"많이 헷갈리는 포인트\n{currentConfig.commonMistakeKo}";
                commonMistakeText.gameObject.SetActive(!string.IsNullOrWhiteSpace(commonMistakeText.text));
            }

            if (coachHintText != null)
            {
                coachHintText.text = currentConfig.coachHintKo;
                coachHintText.gameObject.SetActive(coachVisible && !string.IsNullOrWhiteSpace(currentConfig.coachHintKo));
            }

            if (whyMovedSummaryCard != null)
            {
                whyMovedSummaryCard.gameObject.SetActive(currentConfig.showPlainLanguage);
            }

            UpdateCoachToggleLabel();
        }

        private void RefreshFooter()
        {
            if (appController == null)
            {
                return;
            }

            EnsureTemplateInitialized();

            var runtimeReady = appController.CurrentTemplate != null && appController.CurrentDof > 0;

            stepTitleText.text = currentConfig != null ? currentConfig.stepTitleKo : "Math Readiness";
            var gateProgress = appController.CurrentGateProgressText;
            stepProgressText.text = string.IsNullOrWhiteSpace(gateProgress)
                ? $"Step {appController.CurrentStep}/{appController.TotalSteps}"
                : gateProgress;

            prevButton.interactable = appController.CurrentStep > 1;
            skipButton.gameObject.SetActive(appController.CurrentStep < appController.TotalSteps);
            nextButton.interactable = appController.IsCurrentGateSatisfied || appController.CurrentStep >= appController.TotalSteps;

            SyncFooterSlider(0, jointSlider1, jointInput1, joint1ValueText);
            SyncFooterSlider(1, jointSlider2, jointInput2, joint2ValueText);
            if (jointSlider1 != null)
            {
                jointSlider1.interactable = runtimeReady;
            }

            if (jointInput1 != null)
            {
                jointInput1.interactable = runtimeReady;
            }

            var secondJointReady = AreSecondJointFooterRefsReady();
            if (jointSlider2 != null)
            {
                jointSlider2.interactable = runtimeReady && secondJointReady;
            }

            if (jointInput2 != null)
            {
                jointInput2.interactable = runtimeReady && secondJointReady;
            }

            var showSecondJoint = currentConfig != null && currentConfig.interactiveJointCount > 1 && secondJointReady;
            ApplySecondJointUiVisibility(showSecondJoint);
        }

        private void RefreshWhyMovedSummary()
        {
            if (whyMovedSummaryText == null || appController == null)
            {
                return;
            }

            whyItMovedState.Compute(
                appController.LastUpdateCause,
                appController.ChangedJointIndex,
                appController.PreviousJointValuesRad,
                appController.CurrentJointValuesRad,
                appController.PreviousEndEffectorTransform.ExtractPosition(),
                appController.CurrentEndEffectorTransform.ExtractPosition(),
                appController.CurrentDof);

            whyMovedSummaryText.text = MathReadinessFormatter.FormatWhyItMoved(whyItMovedState);
        }

        private void OnBackClicked()
        {
            SceneNavigator.Load(StepProgressSaver.HasVisited() ? SceneId.RobotLibrary : SceneId.Onboarding);
        }

        private void OnHomeClicked()
        {
            SceneNavigator.Load(SceneId.RobotLibrary);
        }

        private void OnLibraryClicked()
        {
            SceneNavigator.Load(SceneId.RobotLibrary);
        }

        private void OnSandboxClicked()
        {
            if (appController != null && appController.CurrentTemplate != null)
            {
                RobotSelectionBridge.SetSelectedRobot(appController.CurrentTemplate.Name);
                RobotSelectionBridge.SetSelectedMode(RobotSelectionBridge.SandboxMode);
            }

            SceneNavigator.Load(SceneId.Sandbox);
        }

        private void OnPrevClicked()
        {
            appController?.PreviousStep();
        }

        private void OnSkipClicked()
        {
            appController?.SkipCurrentStep();
        }

        private void OnNextClicked()
        {
            appController?.NextStep();
        }

        private void HandleSliderChanged(int jointIndex, float value)
        {
            if (suppressFooterCallbacks || appController == null)
            {
                return;
            }

            EnsureTemplateInitialized();

            appController.SetJointAngleDegrees(jointIndex, value);
            appController.ReportInteraction(InteractionType.SliderChange, $"joint_slider_{jointIndex + 1}");
            RefreshFooter();
        }

        private void HandleInputCommitted(int jointIndex, string raw)
        {
            if (appController == null)
            {
                return;
            }

            var slider = jointIndex == 0 ? jointSlider1 : jointSlider2;
            var input = jointIndex == 0 ? jointInput1 : jointInput2;
            if (slider == null || input == null)
            {
                return;
            }

            EnsureTemplateInitialized();

            if (!JointInputValidator.TryParseDegrees(raw, slider.minValue, slider.maxValue, out var parsed, out _))
            {
                input.SetTextWithoutNotify(JointInputValidator.FormatDegrees(slider.value));
                return;
            }

            suppressFooterCallbacks = true;
            slider.SetValueWithoutNotify(parsed);
            input.SetTextWithoutNotify(JointInputValidator.FormatDegrees(parsed));
            suppressFooterCallbacks = false;

            appController.SetJointAngleDegrees(jointIndex, parsed);
            appController.ReportInteraction(InteractionType.SliderChange, $"joint_slider_{jointIndex + 1}");
            RefreshFooter();
        }

        private void OnWarmupChoice(int index)
        {
            if (currentConfig == null)
            {
                return;
            }

            appController?.ReportInteraction(InteractionType.WarmupChoice, $"{currentConfig.name}_warmup_{index}");
            warmupCompleted = true;
            manipulationPhase = HasCurrentManipulationRequirement();
            RefreshPage();

            if (!string.IsNullOrWhiteSpace(currentConfig.warmupFollowupKo))
            {
                ShowFeedbackWithFade(currentConfig.warmupFollowupKo, UIDesignTokens.Colors.TextSecondary);
            }
        }

        private void OnQuestionChoice(int index)
        {
            if (currentConfig == null || currentConfig.readinessQuestions == null || currentConfig.readinessQuestions.Length == 0 || manipulationPhase)
            {
                return;
            }

            var question = currentConfig.readinessQuestions[Mathf.Clamp(currentQuestionIndex, 0, currentConfig.readinessQuestions.Length - 1)];
            appController?.ReportInteraction(InteractionType.ReadinessChoice, $"{currentConfig.name}_choice_{currentQuestionIndex}_{index}");

            if (index == question.correctChoiceIndex)
            {
                HandleCorrectAnswer(question, index);
                return;
            }

            HandleWrongAnswer(question, index);
        }

        private void HandleCorrectAnswer(MathReadinessQuestion question, int index)
        {
            appController?.ReportInteraction(InteractionType.StepAction, question.correctTargetId);
            ApplyButtonFeedbackColor(index, UIDesignTokens.Colors.AccentSuccess);
            ShowFeedbackIcon("icon-check", UIDesignTokens.Colors.AccentSuccess);
            SetQuestionButtonsInteractable(false);

            if (currentQuestionIndex < currentConfig.readinessQuestions.Length - 1)
            {
                currentQuestionIndex++;
                manipulationPhase = HasCurrentManipulationRequirement();
                RefreshPage();
                ShowFeedbackWithFade(
                    manipulationPhase ? "좋아요. 다음 각도로 먼저 움직여볼게요." : "좋아요. 다음 감각 확인도 이어서 해볼게요.",
                    UIDesignTokens.Colors.AccentSuccess);
                return;
            }

            var finalMessage = string.IsNullOrWhiteSpace(currentConfig.successToastKo)
                ? "좋아요. 이제 다음 단계로 갈 수 있어요."
                : currentConfig.successToastKo;

            if (allCorrectFirstTry)
            {
                finalMessage += "\n한 번에 모두 맞혔어요! 빠른 진행이 가능해요.";
            }

            ShowFeedbackWithFade(finalMessage, UIDesignTokens.Colors.AccentSuccess);
            toastController?.ShowSuccess(finalMessage, 3.5f);
            RefreshFooter();
        }

        private void HandleWrongAnswer(MathReadinessQuestion question, int index)
        {
            question.attemptCount++;
            allCorrectFirstTry = false;

            ApplyButtonFeedbackColor(index, UIDesignTokens.Colors.AccentDanger);
            ShowFeedbackIcon("icon-x-circle", UIDesignTokens.Colors.AccentDanger);

            if (Application.isPlaying)
            {
                if (wrongColorResetCoroutine != null)
                {
                    StopCoroutine(wrongColorResetCoroutine);
                }

                wrongColorResetCoroutine = StartCoroutine(ResetButtonColorAfterDelay(index, 1.5f));
            }

            var correction = index < question.correctionMessagesKo.Length
                ? question.correctionMessagesKo[index]
                : string.Empty;
            if (string.IsNullOrWhiteSpace(correction))
            {
                correction = index < currentConfig.correctionMessagesKo.Length
                    ? currentConfig.correctionMessagesKo[index]
                    : "조금만 다시 볼게요. 방향을 먼저 떠올려 보세요.";
            }

            var adaptiveMessage = MathReadinessFormatter.FormatAdaptiveHint(question.attemptCount, currentConfig.coachHintKo);
            adaptiveHintText.text = adaptiveMessage;
            adaptiveHintText.gameObject.SetActive(!string.IsNullOrWhiteSpace(adaptiveMessage));
            adaptiveHintText.color = question.attemptCount >= 3
                ? UIDesignTokens.Colors.AccentDanger
                : UIDesignTokens.Colors.AccentWarning;

            if (question.attemptCount >= 3)
            {
                HighlightCorrectAnswer(question.correctChoiceIndex);
            }

            ShowFeedbackWithFade(correction, UIDesignTokens.Colors.AccentDanger);
        }

        private void ToggleCoachHint()
        {
            coachVisible = !coachVisible;
            RefreshSupportColumn();
        }

        private void ResetQuestionAttempts()
        {
            if (currentConfig?.readinessQuestions == null)
            {
                return;
            }

            foreach (var question in currentConfig.readinessQuestions)
            {
                question.ResetAttempts();
            }
        }

        private void UpdateProgressBadge()
        {
            if (progressBadgeText == null || currentConfig?.readinessQuestions == null || currentConfig.readinessQuestions.Length == 0)
            {
                return;
            }

            progressBadgeText.text = MathReadinessFormatter.FormatProgressMessage(currentQuestionIndex, currentConfig.readinessQuestions.Length);
            progressBadge.gameObject.SetActive(true);
        }

        private void SetQuestionButtonsInteractable(bool interactable)
        {
            for (var i = 0; i < questionButtons.Length; i++)
            {
                if (questionButtons[i] != null)
                {
                    questionButtons[i].interactable = interactable;
                }
            }
        }

        private void ApplyButtonFeedbackColor(int buttonIndex, Color color)
        {
            if (buttonIndex < 0 || buttonIndex >= questionButtons.Length)
            {
                return;
            }

            var button = questionButtons[buttonIndex];
            var colors = button.colors;
            colors.normalColor = color;
            colors.highlightedColor = Color.Lerp(color, Color.white, 0.18f);
            colors.pressedColor = Color.Lerp(color, Color.black, 0.08f);
            button.colors = colors;
        }

        private void ResetButtonColor(int buttonIndex)
        {
            if (buttonIndex < 0 || buttonIndex >= questionButtons.Length || buttonIndex >= questionButtonOriginalColors.Length)
            {
                return;
            }

            questionButtons[buttonIndex].colors = UIDesignTokens.ButtonColors(questionButtonOriginalColors[buttonIndex]);
        }

        private void HighlightCorrectAnswer(int correctIndex)
        {
            if (correctIndex < 0 || correctIndex >= questionButtons.Length)
            {
                return;
            }

            var colors = questionButtons[correctIndex].colors;
            colors.normalColor = UIDesignTokens.Colors.AccentSuccess;
            colors.highlightedColor = Color.Lerp(UIDesignTokens.Colors.AccentSuccess, Color.white, 0.2f);
            colors.pressedColor = Color.Lerp(UIDesignTokens.Colors.AccentSuccess, Color.black, 0.08f);
            questionButtons[correctIndex].colors = colors;
        }

        private void ShowFeedbackIcon(string iconName, Color color)
        {
            if (feedbackIcon == null)
            {
                return;
            }

            UIIconResolver.SetIcon(feedbackIcon, iconName, color);
            feedbackIcon.gameObject.SetActive(true);
        }

        private void ShowFeedbackWithFade(string message, Color textColor)
        {
            if (feedbackText == null)
            {
                return;
            }

            feedbackText.text = message;
            feedbackText.color = textColor;
            feedbackRow?.gameObject.SetActive(true);

            if (Application.isPlaying && feedbackCanvasGroup != null)
            {
                if (feedbackFadeCoroutine != null)
                {
                    StopCoroutine(feedbackFadeCoroutine);
                }

                feedbackFadeCoroutine = StartCoroutine(FadeIn(feedbackCanvasGroup, UIDesignTokens.Anim.FadeNormal));
            }
            else if (feedbackCanvasGroup != null)
            {
                feedbackCanvasGroup.alpha = 1f;
            }
        }

        private void UpdateCoachToggleLabel()
        {
            if (coachToggleButton == null)
            {
                return;
            }

            var label = coachToggleButton.GetComponentInChildren<Text>(true);
            if (label != null)
            {
                label.text = coachVisible ? "교사 힌트 숨기기" : "교사 힌트 보기";
            }
        }

        private void SyncFooterSlider(int jointIndex, Slider slider, InputField inputField, Text valueText)
        {
            if (slider == null || inputField == null || valueText == null || appController == null)
            {
                return;
            }

            var limit = appController.GetJointLimit(jointIndex);
            slider.minValue = (float)(limit.Min * Mathf.Rad2Deg);
            slider.maxValue = (float)(limit.Max * Mathf.Rad2Deg);

            var degrees = (float)appController.GetJointAngleDegrees(jointIndex);
            suppressFooterCallbacks = true;
            slider.SetValueWithoutNotify(degrees);
            inputField.SetTextWithoutNotify(JointInputValidator.FormatDegrees(degrees));
            suppressFooterCallbacks = false;

            valueText.text = $"J{jointIndex + 1}: {JointInputValidator.FormatDegrees(degrees)}";
        }

        private static IEnumerator FadeIn(CanvasGroup group, float duration)
        {
            group.alpha = 0f;
            var elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                group.alpha = Mathf.Clamp01(elapsed / duration);
                yield return null;
            }

            group.alpha = 1f;
        }

        private IEnumerator ResetButtonColorAfterDelay(int buttonIndex, float delay)
        {
            yield return new WaitForSeconds(delay);
            ResetButtonColor(buttonIndex);
        }

        private bool HasCurrentManipulationRequirement()
        {
            return currentConfig != null &&
                currentConfig.readinessQuestions != null &&
                currentQuestionIndex >= 0 &&
                currentQuestionIndex < currentConfig.readinessQuestions.Length &&
                currentConfig.readinessQuestions[currentQuestionIndex] != null &&
                currentConfig.readinessQuestions[currentQuestionIndex].requiresManipulationFirst;
        }

        private void EnsureTemplateInitialized()
        {
            if (appController == null || appController.CurrentTemplate != null)
            {
                return;
            }

            var selectedId = RobotSelectionBridge.GetSelectedRobotId();
            var template = !string.IsNullOrWhiteSpace(selectedId)
                ? RobotCatalog.CreateTemplate(selectedId)
                : null;

            appController.ApplyTemplate(template ?? Template2DOF_RR.Create());
        }

        private static string FormatTargetBadge(MathReadinessQuestion question)
        {
            if (question == null)
            {
                return string.Empty;
            }

            if (!float.IsNaN(question.secondaryTargetAngleDeg) && question.secondaryTargetJointIndex >= 0)
            {
                return $"목표: J{question.targetJointIndex + 1} {question.targetAngleDeg:0}°, J{question.secondaryTargetJointIndex + 1} {question.secondaryTargetAngleDeg:0}°";
            }

            return $"목표: J{question.targetJointIndex + 1} {question.targetAngleDeg:0}°";
        }

        private bool AreSecondJointFooterRefsReady()
        {
            return jointSlider2 != null && jointInput2 != null && joint2ValueText != null;
        }

        private void ApplySecondJointUiVisibility(bool visible)
        {
            if (jointSlider2 != null)
            {
                jointSlider2.gameObject.SetActive(visible);
            }

            if (jointInput2 != null)
            {
                jointInput2.gameObject.SetActive(visible);
            }

            if (joint2ValueText != null)
            {
                joint2ValueText.gameObject.SetActive(visible);
            }
        }

        private void WarnIfOptionalFooterRefsMissing()
        {
            if (missingOptionalRefsLogged || AreSecondJointFooterRefsReady())
            {
                return;
            }

            var missing = new System.Collections.Generic.List<string>();
            if (jointSlider2 == null)
            {
                missing.Add("joint_slider_2");
            }

            if (jointInput2 == null)
            {
                missing.Add("Joint2Input");
            }

            if (joint2ValueText == null)
            {
                missing.Add("Joint2ValueText");
            }

            Debug.LogWarning($"[MathReadinessPanel] Optional second joint footer refs are missing: {string.Join(", ", missing)}");
            missingOptionalRefsLogged = true;
        }

        private RectTransform FindRect(string path)
        {
            return pageRoot != null ? pageRoot.Find(path) as RectTransform : null;
        }

        private Text FindText(string path)
        {
            return pageRoot != null ? pageRoot.Find(path)?.GetComponent<Text>() : null;
        }

        private Button FindButton(string path)
        {
            return pageRoot != null ? pageRoot.Find(path)?.GetComponent<Button>() : null;
        }

        private Image FindImage(string path)
        {
            return pageRoot != null ? pageRoot.Find(path)?.GetComponent<Image>() : null;
        }

        private Slider FindSlider(string path, string fallbackPath = null)
        {
            if (pageRoot == null)
            {
                return null;
            }

            var slider = pageRoot.Find(path)?.GetComponent<Slider>();
            if (slider != null || string.IsNullOrWhiteSpace(fallbackPath))
            {
                return slider;
            }

            return pageRoot.Find(fallbackPath)?.GetComponent<Slider>();
        }

        private InputField FindInput(string path)
        {
            return pageRoot != null ? pageRoot.Find(path)?.GetComponent<InputField>() : null;
        }
    }
}
