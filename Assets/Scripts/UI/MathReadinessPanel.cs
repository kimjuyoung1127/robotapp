// Folder: UI - HUD/view components only; no kinematics logic.
using System.Collections;
using KineTutor3D.App;
using KineTutor3D.UI.Data;
using UnityEngine;
using UnityEngine.UI;

namespace KineTutor3D.UI
{
    [ExecuteAlways]
    [DisallowMultipleComponent]
    public class MathReadinessPanel : MonoBehaviour
    {
        [SerializeField] private AppController appController;
        [SerializeField] private ToastNotificationController toastController;
        [SerializeField] private RectTransform panelRoot;
        [SerializeField] private Font fallbackFont;

        private TutorStepConfig currentConfig;
        private int currentQuestionIndex;
        private bool warmupCompleted;
        private bool coachVisible;
        private bool allCorrectFirstTry;
        private bool manipulationPhase;

        private RectTransform overviewCard;
        private RectTransform questionCard;
        private RectTransform helpCard;
        private RectTransform warmupCard;
        private RectTransform progressBadge;
        private RectTransform targetBadge;

        private Text lessonEyebrowText;
        private Text lessonTitleText;
        private Text lessonGoalText;
        private Text introText;
        private Text rationaleText;
        private Text warmupPromptText;
        private Text warmupFeedbackText;
        private Text warmupSectionLabel;
        private Text questionPromptText;
        private Text manipulationInstructionText;
        private Text feedbackText;
        private Text commonMistakeText;
        private Text coachHintText;
        private Text questionSectionLabel;
        private Text adaptiveHintText;
        private Text progressBadgeText;
        private Text targetBadgeText;
        private Image conceptStripe;
        private Image warmupDivider;
        private Button coachToggleButton;
        private Image coachToggleIcon;
        private Image feedbackIcon;
        private CanvasGroup feedbackCanvasGroup;
        private readonly Button[] warmupButtons = new Button[3];
        private readonly Button[] questionButtons = new Button[3];
        private Color[] questionButtonOriginalColors;
        private Coroutine feedbackFadeCoroutine;
        private Coroutine wrongColorResetCoroutine;

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

                var idx = Mathf.Clamp(currentQuestionIndex, 0, currentConfig.readinessQuestions.Length - 1);
                return currentConfig.readinessQuestions[idx].attemptCount;
            }
        }

        private void Awake()
        {
            fallbackFont = UiRuntimeStyle.ResolveFont(fallbackFont);
        }

        private void OnEnable()
        {
            EnsureLayout();
            SetVisible(false);
        }

        private void OnDisable()
        {
            Unbind();
        }

        private void OnDestroy()
        {
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
            }

            EnsureLayout();
        }

        public void SetVisible(bool visible)
        {
            if (panelRoot != null)
            {
                panelRoot.gameObject.SetActive(visible);
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
            EnsureLayout();
            RefreshView();
        }

        private void HandleStepChanged(int _step, TutorStepConfig config)
        {
            if (config != null && config.mathReadinessMode && config.showMathReadinessPanel)
            {
                ApplyConfig(config);
                SetVisible(true);
            }
            else
            {
                SetVisible(false);
            }
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
            RefreshQuestion();
            ShowFeedbackWithFade("좋아요. 이제 확인 질문을 풀어볼게요.", UIDesignTokens.Colors.TextSecondary);
        }

        private void ResetQuestionAttempts()
        {
            if (currentConfig?.readinessQuestions == null)
            {
                return;
            }

            foreach (var q in currentConfig.readinessQuestions)
            {
                q.ResetAttempts();
            }
        }

        private void RefreshView()
        {
            if (currentConfig == null)
            {
                return;
            }

            lessonEyebrowText.text = "현재 학습";
            lessonTitleText.text = currentConfig.stepTitleKo;
            lessonGoalText.text = currentConfig.objectiveKo;
            introText.text = currentConfig.hintKo;
            rationaleText.text = currentConfig.rationaleKo;
            rationaleText.gameObject.SetActive(false);

            commonMistakeText.text = string.IsNullOrWhiteSpace(currentConfig.commonMistakeKo)
                ? string.Empty
                : $"많이 헷갈리는 포인트\n{currentConfig.commonMistakeKo}";
            commonMistakeText.gameObject.SetActive(!string.IsNullOrWhiteSpace(commonMistakeText.text));

            coachHintText.text = currentConfig.coachHintKo;
            coachHintText.gameObject.SetActive(coachVisible && !string.IsNullOrWhiteSpace(currentConfig.coachHintKo));

            ApplyConceptTheme();
            LayoutQuestionCard();
            LayoutHelpCard();

            if (feedbackIcon != null)
            {
                feedbackIcon.gameObject.SetActive(false);
            }

            if (adaptiveHintText != null)
            {
                adaptiveHintText.gameObject.SetActive(false);
            }

            RefreshQuestion();
        }

        private void RefreshQuestion()
        {
            if (currentConfig == null || currentConfig.readinessQuestions == null || currentConfig.readinessQuestions.Length == 0)
            {
                questionPromptText.text = string.Empty;
                manipulationInstructionText.text = string.Empty;
                feedbackText.text = string.Empty;
                UpdateProgressBadge();
                LayoutQuestionCard();
                return;
            }

            currentQuestionIndex = Mathf.Clamp(currentQuestionIndex, 0, currentConfig.readinessQuestions.Length - 1);
            var question = currentConfig.readinessQuestions[currentQuestionIndex];
            questionPromptText.text = question.promptKo;
            var showQuestion = warmupCompleted;
            if (question.requiresManipulationFirst && manipulationPhase)
            {
                manipulationInstructionText.text = question.manipulationInstructionKo;
            }
            else
            {
                manipulationInstructionText.text = string.Empty;
            }

            UpdateProgressBadge();

            for (var i = 0; i < questionButtons.Length; i++)
            {
                var label = questionButtons[i].GetComponentInChildren<Text>();
                if (label != null)
                {
                    label.text = i < question.choicesKo.Length ? question.choicesKo[i] : $"선택 {i + 1}";
                }

                ResetButtonColor(i);
                questionButtons[i].interactable = showQuestion;
                questionButtons[i].gameObject.SetActive(showQuestion && i < Mathf.Max(question.choicesKo.Length, 3));
            }

            if (!showQuestion)
            {
                feedbackText.text = string.Empty;
                SetQuestionButtonsInteractable(false);
            }
            else
            {
                feedbackText.text = string.Empty;
                SetQuestionButtonsInteractable(true);
            }

            if (adaptiveHintText != null)
            {
                adaptiveHintText.gameObject.SetActive(false);
            }

            if (feedbackIcon != null)
            {
                feedbackIcon.gameObject.SetActive(false);
            }

            LayoutQuestionCard();
        }

        private void UpdateProgressBadge()
        {
            if (progressBadgeText == null)
            {
                return;
            }

            if (currentConfig?.readinessQuestions != null && currentConfig.readinessQuestions.Length > 0)
            {
                progressBadgeText.text = MathReadinessFormatter.FormatProgressMessage(currentQuestionIndex, currentConfig.readinessQuestions.Length);
                if (progressBadge != null)
                {
                    progressBadge.gameObject.SetActive(true);
                }
            }
            else if (progressBadge != null)
            {
                progressBadge.gameObject.SetActive(false);
            }
        }

        private void SetQuestionButtonsInteractable(bool interactable)
        {
            for (var i = 0; i < questionButtons.Length; i++)
            {
                questionButtons[i].interactable = interactable;
            }
        }

        private void OnWarmupChoice(int index)
        {
            if (currentConfig == null)
            {
                return;
            }

            appController?.ReportInteraction(InteractionType.WarmupChoice, $"{currentConfig.name}_warmup_{index}");
            warmupCompleted = true;
            LayoutQuestionCard();
            RefreshQuestion();

            if (!string.IsNullOrWhiteSpace(currentConfig.warmupFollowupKo))
            {
                ShowFeedbackWithFade(currentConfig.warmupFollowupKo, UIDesignTokens.Colors.TextSecondary);
            }
        }

        private void OnQuestionChoice(int index)
        {
            if (currentConfig == null || currentConfig.readinessQuestions == null || currentConfig.readinessQuestions.Length == 0)
            {
                return;
            }

            if (manipulationPhase)
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
                RefreshQuestion();
                ShowFeedbackWithFade(manipulationPhase
                    ? "좋아요. 다음 각도로 먼저 움직여볼게요."
                    : "좋아요. 다음 감각 확인도 이어서 해볼게요.", UIDesignTokens.Colors.AccentSuccess);
                return;
            }

            var finalMsg = string.IsNullOrWhiteSpace(currentConfig.successToastKo)
                ? "좋아요. 이제 다음 단계로 갈 수 있어요."
                : currentConfig.successToastKo;

            if (allCorrectFirstTry)
            {
                finalMsg += "\n한 번에 모두 맞혔어요! 빠른 진행이 가능해요.";
            }

            ShowFeedbackWithFade(finalMsg, UIDesignTokens.Colors.AccentSuccess);
            toastController?.ShowSuccess(finalMsg, 3.5f);
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

            var adaptiveMsg = MathReadinessFormatter.FormatAdaptiveHint(question.attemptCount, currentConfig.coachHintKo);
            if (!string.IsNullOrEmpty(adaptiveMsg))
            {
                adaptiveHintText.text = adaptiveMsg;
                adaptiveHintText.gameObject.SetActive(true);
                adaptiveHintText.color = question.attemptCount >= 3
                    ? UIDesignTokens.Colors.AccentDanger
                    : UIDesignTokens.Colors.AccentWarning;

                if (question.attemptCount >= 3)
                {
                    HighlightCorrectAnswer(question.correctChoiceIndex);
                }
            }

            ShowFeedbackWithFade(correction, UIDesignTokens.Colors.AccentDanger);
        }

        private void HighlightCorrectAnswer(int correctIndex)
        {
            if (correctIndex < 0 || correctIndex >= questionButtons.Length)
            {
                return;
            }

            var cb = questionButtons[correctIndex].colors;
            cb.normalColor = UIDesignTokens.Colors.AccentSuccess;
            cb.highlightedColor = Color.Lerp(UIDesignTokens.Colors.AccentSuccess, Color.white, 0.2f);
            cb.pressedColor = Color.Lerp(UIDesignTokens.Colors.AccentSuccess, Color.black, 0.08f);
            questionButtons[correctIndex].colors = cb;
        }

        private void ApplyButtonFeedbackColor(int buttonIndex, Color color)
        {
            if (buttonIndex < 0 || buttonIndex >= questionButtons.Length)
            {
                return;
            }

            var btn = questionButtons[buttonIndex];
            var cb = btn.colors;
            cb.normalColor = color;
            cb.highlightedColor = Color.Lerp(color, Color.white, 0.18f);
            cb.pressedColor = Color.Lerp(color, Color.black, 0.08f);
            btn.colors = cb;
        }

        private void ResetButtonColor(int buttonIndex)
        {
            if (buttonIndex < 0 || buttonIndex >= questionButtons.Length || questionButtonOriginalColors == null || buttonIndex >= questionButtonOriginalColors.Length)
            {
                return;
            }

            questionButtons[buttonIndex].colors = UIDesignTokens.ButtonColors(questionButtonOriginalColors[buttonIndex]);
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
            feedbackText.text = message;
            feedbackText.color = textColor;

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

        private static IEnumerator FadeIn(CanvasGroup cg, float duration)
        {
            cg.alpha = 0f;
            var elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                cg.alpha = Mathf.Clamp01(elapsed / duration);
                yield return null;
            }

            cg.alpha = 1f;
        }

        private IEnumerator ResetButtonColorAfterDelay(int buttonIndex, float delay)
        {
            yield return new WaitForSeconds(delay);
            ResetButtonColor(buttonIndex);
        }

        private void ApplyConceptTheme()
        {
            if (currentConfig == null || conceptStripe == null)
            {
                return;
            }

            var content = currentConfig.mathReadinessContent;
            conceptStripe.color = MathReadinessContentTheme.GetStripeColor(content);
            conceptStripe.gameObject.SetActive(content != MathReadinessContent.None);
        }

        private void ToggleCoachHint()
        {
            coachVisible = !coachVisible;
            coachHintText.gameObject.SetActive(coachVisible && currentConfig != null && !string.IsNullOrWhiteSpace(currentConfig.coachHintKo));

            var label = coachToggleButton != null ? coachToggleButton.GetComponentInChildren<Text>() : null;
            if (label != null)
            {
                label.text = coachVisible ? "교사/보호자 멘트 숨기기" : "교사/보호자 멘트 보기";
            }

            LayoutHelpCard();
        }

        private void LayoutQuestionCard()
        {
            var showWarmup = currentConfig != null && !warmupCompleted && !string.IsNullOrWhiteSpace(currentConfig.warmupPromptKo);
            var showQuestion = currentConfig != null && currentConfig.readinessQuestions != null && currentConfig.readinessQuestions.Length > 0 && !showWarmup;
            var showManipulation = showQuestion && manipulationPhase;

            if (warmupSectionLabel != null)
            {
                warmupSectionLabel.text = "워밍업";
                warmupSectionLabel.gameObject.SetActive(showWarmup);
                UiRuntimeStyle.Anchor(warmupSectionLabel.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(220f, 18f), new Vector2(20f, -16f));
            }

            if (warmupCard != null)
            {
                warmupCard.gameObject.SetActive(showWarmup);
                UiRuntimeStyle.Anchor(warmupCard, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(320f, 212f), new Vector2(16f, -38f));
            }

            if (warmupPromptText != null)
            {
                warmupPromptText.text = currentConfig != null ? currentConfig.warmupPromptKo : string.Empty;
                UiRuntimeStyle.Anchor(warmupPromptText.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(288f, 42f), new Vector2(16f, -16f));
            }

            if (warmupFeedbackText != null)
            {
                warmupFeedbackText.text = string.Empty;
                warmupFeedbackText.gameObject.SetActive(false);
                UiRuntimeStyle.Anchor(warmupFeedbackText.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(288f, 24f), new Vector2(16f, -16f));
            }

            for (var i = 0; i < warmupButtons.Length; i++)
            {
                var label = warmupButtons[i].GetComponentInChildren<Text>();
                if (label != null && currentConfig != null)
                {
                    label.text = i < currentConfig.warmupChoicesKo.Length ? currentConfig.warmupChoicesKo[i] : $"선택 {i + 1}";
                }

                warmupButtons[i].gameObject.SetActive(showWarmup);
                UiRuntimeStyle.Anchor((RectTransform)warmupButtons[i].transform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(288f, 40f), new Vector2(16f, -72f - (46f * i)));
            }

            if (warmupDivider != null)
            {
                warmupDivider.gameObject.SetActive(showWarmup && showQuestion);
                UiRuntimeStyle.Anchor((RectTransform)warmupDivider.transform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(320f, 1f), new Vector2(16f, -238f));
            }

            var questionTop = showWarmup ? -270f : -20f;

            if (questionSectionLabel != null)
            {
                questionSectionLabel.text = showManipulation ? "먼저 조작" : "확인 질문";
                questionSectionLabel.gameObject.SetActive(showQuestion);
                UiRuntimeStyle.Anchor(questionSectionLabel.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(220f, 18f), new Vector2(20f, questionTop));
            }

            questionPromptText.gameObject.SetActive(showQuestion && !showManipulation);
            UiRuntimeStyle.Anchor(questionPromptText.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(248f, 48f), new Vector2(20f, questionTop - 26f));
            manipulationInstructionText.gameObject.SetActive(showManipulation);
            UiRuntimeStyle.Anchor(manipulationInstructionText.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(280f, 54f), new Vector2(20f, questionTop - 32f));
            targetBadge.gameObject.SetActive(showManipulation);
            UiRuntimeStyle.Anchor(targetBadge, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(176f, UIDesignTokens.Size.BadgeHeight), new Vector2(-20f - 176f, questionTop - 4f));
            if (targetBadgeText != null)
            {
                var targetBadgeLabel = string.Empty;
                if (showManipulation && TryGetActiveManipulationQuestion(out var targetQuestion))
                {
                    targetBadgeLabel = FormatTargetBadge(targetQuestion);
                }

                targetBadgeText.text = targetBadgeLabel;
            }
            progressBadge.gameObject.SetActive(showQuestion && !showManipulation);
            UiRuntimeStyle.Anchor(progressBadge, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(UIDesignTokens.Size.BadgeWidth, UIDesignTokens.Size.BadgeHeight), new Vector2(-20f - UIDesignTokens.Size.BadgeWidth, questionTop - 4f));

            var buttonStartY = showManipulation ? questionTop - 112f : questionTop - 88f;
            for (var i = 0; i < questionButtons.Length; i++)
            {
                questionButtons[i].gameObject.SetActive(showQuestion && !showManipulation);
                UiRuntimeStyle.Anchor((RectTransform)questionButtons[i].transform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(304f, 44f), new Vector2(20f, buttonStartY - (50f * i)));
            }

            feedbackIcon.gameObject.SetActive(showQuestion && !showManipulation && feedbackIcon.gameObject.activeSelf);
            feedbackText.gameObject.SetActive(showQuestion && !showManipulation);
            adaptiveHintText.gameObject.SetActive(showQuestion && !showManipulation && adaptiveHintText.gameObject.activeSelf);
            UiRuntimeStyle.Anchor(feedbackIcon.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(UIDesignTokens.Size.IconSm, UIDesignTokens.Size.IconSm), new Vector2(20f, buttonStartY - 164f));
            UiRuntimeStyle.Anchor(feedbackText.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(280f, 48f), new Vector2(44f, buttonStartY - 168f));
            UiRuntimeStyle.Anchor(adaptiveHintText.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(304f, 32f), new Vector2(20f, buttonStartY - 220f));
        }

        private void LayoutHelpCard()
        {
            if (helpCard == null || commonMistakeText == null || coachToggleButton == null || coachHintText == null)
            {
                return;
            }

            var hasMistake = !string.IsNullOrWhiteSpace(commonMistakeText.text);
            var hasCoach = coachVisible && currentConfig != null && !string.IsNullOrWhiteSpace(currentConfig.coachHintKo);
            var height = hasCoach ? 188f : 132f;

            UiRuntimeStyle.Anchor(helpCard, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(352f, height), new Vector2(UIDesignTokens.Space.Md, UIDesignTokens.Space.Md));
            commonMistakeText.gameObject.SetActive(hasMistake);
            UiRuntimeStyle.Anchor(commonMistakeText.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(312f, hasCoach ? 56f : 46f), new Vector2(16f, -16f));
            UiRuntimeStyle.Anchor((RectTransform)coachToggleButton.transform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(208f, UIDesignTokens.Size.ButtonHeightMd), new Vector2(16f, hasMistake ? -82f : -16f));
            UiRuntimeStyle.Anchor(coachHintText.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(312f, 56f), new Vector2(16f, hasMistake ? -126f : -60f));
            coachHintText.gameObject.SetActive(hasCoach);
        }

        private void EnsureLayout()
        {
            fallbackFont = UiRuntimeStyle.ResolveFont(fallbackFont);
            toastController ??= FindFirstObjectByType<ToastNotificationController>(FindObjectsInactive.Include);

            panelRoot ??= UiRuntimeStyle.EnsureHostedRoot(this, "MathReadinessRect");
            UiRuntimeStyle.Stretch(panelRoot,
                new Vector2(0f, 0f), new Vector2(0f, 1f),
                new Vector2(UIDesignTokens.Space.Md, 146f),
                new Vector2(UILayoutProfile.LeftPanelWidth + UIDesignTokens.Space.Md, -92f));

            var content = UIComponentFactory.CreatePanel(panelRoot, "MRP_Content", UIDesignTokens.Colors.SurfaceRaised);
            UiRuntimeStyle.Stretch(content, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

            conceptStripe = UiRuntimeStyle.EnsureImage(content, "MRP_ConceptStripe", UIDesignTokens.Colors.AccentPrimary);
            UiRuntimeStyle.Stretch((RectTransform)conceptStripe.transform,
                new Vector2(0f, 1f), new Vector2(1f, 1f),
                new Vector2(0f, -4f), new Vector2(0f, 0f));
            conceptStripe.gameObject.SetActive(false);

            overviewCard = UIComponentFactory.CreatePanel(content, "MRP_OverviewCard", UIDesignTokens.Colors.SurfaceCard);
            UiRuntimeStyle.Anchor(overviewCard, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(352f, 164f), new Vector2(UIDesignTokens.Space.Md, -UIDesignTokens.Space.Md));

            lessonEyebrowText = UIComponentFactory.CreateText(overviewCard, "MRP_OverviewLabel", TypographyPreset.Caption, UIDesignTokens.Colors.TextMuted, string.Empty, fallbackFont);
            lessonEyebrowText.fontStyle = FontStyle.Bold;
            UiRuntimeStyle.Anchor(lessonEyebrowText.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(140f, 18f), new Vector2(16f, -16f));

            lessonTitleText = UIComponentFactory.CreateText(overviewCard, "MRP_LessonTitle", TypographyPreset.HeadingSm, UIDesignTokens.Colors.TextPrimary, string.Empty, fallbackFont);
            lessonTitleText.fontSize = 22;
            lessonTitleText.fontStyle = FontStyle.Bold;
            UiRuntimeStyle.Anchor(lessonTitleText.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(300f, 54f), new Vector2(16f, -38f));

            lessonGoalText = UIComponentFactory.CreateText(overviewCard, "MRP_LessonGoal", TypographyPreset.Body, UIDesignTokens.Colors.TextPrimary, string.Empty, fallbackFont);
            lessonGoalText.fontSize = 15;
            lessonGoalText.fontStyle = FontStyle.Bold;
            UiRuntimeStyle.Anchor(lessonGoalText.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(316f, 42f), new Vector2(16f, -92f));

            introText = UIComponentFactory.CreateText(overviewCard, "MRP_Intro", TypographyPreset.Body, UIDesignTokens.Colors.TextSecondary, string.Empty, fallbackFont);
            introText.fontSize = 13;
            UiRuntimeStyle.Anchor(introText.rectTransform, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(316f, 34f), new Vector2(16f, 18f));

            rationaleText = UIComponentFactory.CreateText(overviewCard, "MRP_Rationale", TypographyPreset.Caption, UIDesignTokens.Colors.TextMuted, string.Empty, fallbackFont);
            rationaleText.fontSize = 12;
            rationaleText.fontStyle = FontStyle.Italic;
            UiRuntimeStyle.Anchor(rationaleText.rectTransform, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(316f, 24f), new Vector2(16f, 10f));

            questionCard = UIComponentFactory.CreatePanel(content, "MRP_QuestionCard", UIDesignTokens.Colors.SurfaceRaisedAlt);
            UiRuntimeStyle.Stretch(questionCard, new Vector2(0f, 0f), new Vector2(0f, 1f), new Vector2(UIDesignTokens.Space.Md, 164f), new Vector2(-(UIDesignTokens.Space.Md + 4f), -196f));

            progressBadge = UIComponentFactory.CreateBadge(questionCard, "MRP_ProgressBadge", "Q1/1", UIDesignTokens.Colors.AccentSecondary, fallbackFont);
            progressBadgeText = progressBadge.GetComponentInChildren<Text>();
            targetBadge = UIComponentFactory.CreateBadge(questionCard, "MRP_TargetBadge", "목표: 90°", UIDesignTokens.Colors.AccentPrimary, fallbackFont);
            targetBadgeText = targetBadge.GetComponentInChildren<Text>();
            targetBadge.gameObject.SetActive(false);

            warmupSectionLabel = UIComponentFactory.CreateText(questionCard, "MRP_WarmupLabel", TypographyPreset.Caption, UIDesignTokens.Colors.TextMuted, "워밍업", fallbackFont);

            var warmupCardColor = UIDesignTokens.Colors.SurfaceCard;
            warmupCardColor.a = 0.55f;
            warmupCard = UIComponentFactory.CreatePanel(questionCard, "MRP_WarmupCard", warmupCardColor);
            warmupPromptText = UIComponentFactory.CreateText(warmupCard, "MRP_WarmupPrompt", TypographyPreset.Body, UIDesignTokens.Colors.TextPrimary, string.Empty, fallbackFont);
            warmupPromptText.fontSize = 15;
            warmupPromptText.fontStyle = FontStyle.Bold;

            warmupFeedbackText = UIComponentFactory.CreateText(questionCard, "MRP_WarmupFeedback", TypographyPreset.Caption, UIDesignTokens.Colors.TextMuted, string.Empty, fallbackFont);
            warmupFeedbackText.fontStyle = FontStyle.Italic;
            warmupFeedbackText.gameObject.SetActive(false);

            for (var i = 0; i < warmupButtons.Length; i++)
            {
                var capturedIndex = i;
                warmupButtons[i] = EnsureChoiceButton(warmupCard, $"BtnWarmupChoice_{i}", () => OnWarmupChoice(capturedIndex), 288f, 40f);
            }

            warmupDivider = UIComponentFactory.CreateDivider(questionCard, "MRP_Divider");

            questionSectionLabel = UIComponentFactory.CreateText(questionCard, "MRP_QuestionLabel", TypographyPreset.Caption, UIDesignTokens.Colors.TextMuted, "현재 문제", fallbackFont);
            manipulationInstructionText = UIComponentFactory.CreateText(questionCard, "MRP_ManipulationInstruction", TypographyPreset.Body, UIDesignTokens.Colors.TextPrimary, string.Empty, fallbackFont);
            manipulationInstructionText.fontStyle = FontStyle.Bold;
            manipulationInstructionText.fontSize = 16;
            manipulationInstructionText.gameObject.SetActive(false);
            questionPromptText = UIComponentFactory.CreateText(questionCard, "MRP_QuestionPrompt", TypographyPreset.Body, UIDesignTokens.Colors.TextPrimary, string.Empty, fallbackFont);
            questionPromptText.fontStyle = FontStyle.Bold;
            questionPromptText.fontSize = 17;

            feedbackIcon = UIIconResolver.CreateIcon(questionCard, "MRP_FeedbackIcon", "icon-check", UIDesignTokens.Size.IconSm, UIDesignTokens.Colors.AccentSuccess);
            feedbackIcon.gameObject.SetActive(false);

            feedbackText = UIComponentFactory.CreateText(questionCard, "MRP_FeedbackText", TypographyPreset.Caption, UIDesignTokens.Colors.TextSecondary, string.Empty, fallbackFont);
            feedbackCanvasGroup = feedbackText.GetComponent<CanvasGroup>();
            if (feedbackCanvasGroup == null)
            {
                feedbackCanvasGroup = feedbackText.gameObject.AddComponent<CanvasGroup>();
            }

            adaptiveHintText = UIComponentFactory.CreateText(questionCard, "MRP_AdaptiveHint", TypographyPreset.Caption, UIDesignTokens.Colors.AccentWarning, string.Empty, fallbackFont);
            adaptiveHintText.fontStyle = FontStyle.Bold;
            adaptiveHintText.gameObject.SetActive(false);

            questionButtonOriginalColors = new Color[questionButtons.Length];
            for (var i = 0; i < questionButtons.Length; i++)
            {
                var capturedIndex = i;
                questionButtons[i] = EnsureChoiceButton(questionCard, $"BtnReadinessChoice_{i}", () => OnQuestionChoice(capturedIndex), 304f, 44f);
                questionButtonOriginalColors[i] = questionButtons[i].colors.normalColor;
            }

            helpCard = UIComponentFactory.CreatePanel(content, "MRP_HelpCard", UIDesignTokens.Colors.SurfaceCard);
            UiRuntimeStyle.Anchor(helpCard, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(352f, 132f), new Vector2(UIDesignTokens.Space.Md, UIDesignTokens.Space.Md));

            commonMistakeText = UIComponentFactory.CreateText(helpCard, "MRP_CommonMistake", TypographyPreset.Caption, UIDesignTokens.Colors.TextSecondary, string.Empty, fallbackFont);
            commonMistakeText.fontSize = 13;
            UiRuntimeStyle.Anchor(commonMistakeText.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(312f, 46f), new Vector2(16f, -16f));

            coachToggleButton = UIComponentFactory.CreateSecondaryButton(helpCard, "BtnToggleCoachHint", "교사/보호자 멘트 보기", fallbackFont, 208f);
            UiRuntimeStyle.Anchor((RectTransform)coachToggleButton.transform, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(208f, UIDesignTokens.Size.ButtonHeightMd), new Vector2(16f, 16f));
            coachToggleButton.onClick.RemoveAllListeners();
            coachToggleButton.onClick.AddListener(ToggleCoachHint);
            coachToggleIcon = UIComponentFactory.AttachLeadingIcon(coachToggleButton, "icon-help", UIDesignTokens.Size.IconSm);

            coachHintText = UIComponentFactory.CreateText(helpCard, "MRP_CoachHintBody", TypographyPreset.Caption, UIDesignTokens.Colors.TextMuted, string.Empty, fallbackFont);
            coachHintText.fontSize = 12;
            coachHintText.fontStyle = FontStyle.Italic;
            UiRuntimeStyle.Anchor(coachHintText.rectTransform, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(312f, 34f), new Vector2(16f, 56f));
            coachHintText.gameObject.SetActive(false);

            LayoutHelpCard();
        }

        private Button EnsureChoiceButton(Transform parent, string name, UnityEngine.Events.UnityAction onClick, float width, float height)
        {
            var button = UIComponentFactory.CreateSecondaryButton(parent, name, "...", fallbackFont, width);
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(onClick);

            var label = button.GetComponentInChildren<Text>();
            if (label != null)
            {
                label.fontSize = UIDesignTokens.Type.Body;
                label.alignment = TextAnchor.MiddleLeft;
                UiRuntimeStyle.Stretch(label.rectTransform, Vector2.zero, Vector2.one, new Vector2(16f, 6f), new Vector2(-16f, -6f));
            }

            button.colors = UIDesignTokens.ButtonColors(UIDesignTokens.Colors.SurfaceCard);
            ((RectTransform)button.transform).sizeDelta = new Vector2(width, height);
            return button;
        }

        public bool TryGetActiveManipulationQuestion(out MathReadinessQuestion question)
        {
            question = null;
            if (!manipulationPhase || currentConfig?.readinessQuestions == null || currentConfig.readinessQuestions.Length == 0)
            {
                return false;
            }

            if (currentQuestionIndex < 0 || currentQuestionIndex >= currentConfig.readinessQuestions.Length)
            {
                return false;
            }

            question = currentConfig.readinessQuestions[currentQuestionIndex];
            return question != null && question.requiresManipulationFirst;
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

        private void Unbind()
        {
            if (appController != null)
            {
                appController.OnStepChanged -= HandleStepChanged;
                appController.OnInteractionEvent -= HandleInteractionEvent;
            }
        }
    }
}
