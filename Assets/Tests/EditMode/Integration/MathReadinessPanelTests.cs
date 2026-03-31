// Folder: Tests/EditMode - EditMode tests for runtime, math, and tooling behaviors.
// MathReadinessPanel 동작을 검증하는 EditMode 테스트입니다.
using KineTutor3D.App;
using KineTutor3D.UI;
using KineTutor3D.UI.Data;
using NUnit.Framework;
using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

namespace KineTutor3D.Tests.EditMode
{
    public class MathReadinessPanelTests
    {
        private GameObject _host;
        private MathReadinessPanel _panel;

        [SetUp]
        public void SetUp()
        {
            _host = new GameObject("MathReadinessPageRoot", typeof(RectTransform), typeof(CanvasGroup));
            BuildAuthoredHierarchy(_host.transform);
            _panel = _host.AddComponent<MathReadinessPanel>();
        }

        [TearDown]
        public void TearDown()
        {
            UnityEngine.Object.DestroyImmediate(_host);
        }

        [Test]
        public void CoachHint_StartsCollapsed()
        {
            InvokeEnsureLayout();
            var coach = FindDescendant("MRP_CoachHintBody");

            Assert.IsNotNull(coach);
            Assert.IsFalse(coach.gameObject.activeSelf);
        }

        [Test]
        public void ProgressBadge_IsCreated()
        {
            InvokeEnsureLayout();
            var badge = FindDescendant("MRP_ProgressBadge");

            Assert.IsNotNull(badge, "Progress badge should exist");
        }

        [Test]
        public void ConceptStripe_IsCreatedAndHiddenByDefault()
        {
            InvokeEnsureLayout();
            var stripe = FindDescendant("MRP_ConceptStripe");

            Assert.IsNotNull(stripe, "Concept stripe should exist");
            Assert.IsFalse(stripe.gameObject.activeSelf, "Stripe hidden by default");
        }

        [Test]
        public void FeedbackIcon_IsCreatedAndHiddenByDefault()
        {
            InvokeEnsureLayout();
            var icon = FindDescendant("MRP_FeedbackIcon");

            Assert.IsNotNull(icon, "Feedback icon should exist");
            Assert.IsFalse(icon.gameObject.activeSelf, "Feedback icon hidden by default");
        }

        [Test]
        public void AdaptiveHint_IsCreatedAndHiddenByDefault()
        {
            InvokeEnsureLayout();
            var hint = FindDescendant("MRP_AdaptiveHint");

            Assert.IsNotNull(hint, "Adaptive hint should exist");
            Assert.IsFalse(hint.gameObject.activeSelf, "Adaptive hint hidden by default");
        }

        [Test]
        public void WarmupSectionLabel_IsCreated()
        {
            InvokeEnsureLayout();
            var label = FindDescendant("MRP_WarmupLabel");

            Assert.IsNotNull(label, "Warmup section label should exist");
        }

        [Test]
        public void QuestionSectionLabel_IsCreated()
        {
            InvokeEnsureLayout();
            var label = FindDescendant("MRP_QuestionLabel");

            Assert.IsNotNull(label, "Question section label should exist");
        }

        [Test]
        public void Divider_IsCreated()
        {
            InvokeEnsureLayout();
            var divider = FindDescendant("MRP_Divider");

            Assert.IsNotNull(divider, "Divider should exist");
        }

        [Test]
        public void CoachToggle_HasLeadingIcon()
        {
            InvokeEnsureLayout();
            var coachBtn = FindDescendant("BtnToggleCoachHint");

            Assert.IsNotNull(coachBtn, "Coach toggle button should exist");
            var icon = coachBtn.Find("LeadingIcon");
            Assert.IsNotNull(icon, "Coach toggle should have leading icon");
        }

        [Test]
        public void Layout_IsReorganizedInto_ThreePrimaryBlocks()
        {
            InvokeEnsureLayout();

            Assert.IsNotNull(FindDescendant("MRP_OverviewCard"));
            Assert.IsNotNull(FindDescendant("MRP_QuestionCard"));
            Assert.IsNotNull(FindDescendant("MRP_HelpCard"));
        }

        [Test]
        public void ManipulationInstruction_AndTargetBadge_AreCreated()
        {
            InvokeEnsureLayout();

            Assert.IsNotNull(FindDescendant("MRP_ManipulationInstruction"));
            Assert.IsNotNull(FindDescendant("MRP_TargetBadge"));
        }

        [Test]
        public void MathReadinessQuestion_AttemptCount_DefaultsToZero()
        {
            var q = new MathReadinessQuestion
            {
                promptKo = "test",
                choicesKo = new[] { "a", "b" },
                correctChoiceIndex = 0
            };

            Assert.AreEqual(0, q.attemptCount);
        }

        [Test]
        public void MathReadinessQuestion_ResetAttempts_ClearsCount()
        {
            var q = new MathReadinessQuestion();
            q.attemptCount = 5;
            q.ResetAttempts();

            Assert.AreEqual(0, q.attemptCount);
        }

        [Test]
        public void MathReadinessQuestion_ManipulationFields_HaveSafeDefaults()
        {
            var q = new MathReadinessQuestion();

            Assert.That(q.requiresManipulationFirst, Is.False);
            Assert.That(float.IsNaN(q.targetAngleDeg), Is.True);
            Assert.That(q.targetJointIndex, Is.EqualTo(0));
            Assert.That(q.targetAngleTolerance, Is.EqualTo(5f));
            Assert.That(q.targetReachGateId, Is.Empty);
            Assert.That(q.manipulationInstructionKo, Is.Empty);
        }

        [Test]
        public void MathReadinessContentTheme_ReturnsCorrectColors()
        {
            var orange = MathReadinessContentTheme.GetAccentColor(MathReadinessContent.AngleDirection);
            var blue = MathReadinessContentTheme.GetAccentColor(MathReadinessContent.LengthAngleToPoint);
            var purple = MathReadinessContentTheme.GetAccentColor(MathReadinessContent.DiagonalIntuition);
            var green = MathReadinessContentTheme.GetAccentColor(MathReadinessContent.TwoLinkComposition);

            // Each concept should have a distinct color
            Assert.AreNotEqual(orange, blue);
            Assert.AreNotEqual(blue, purple);
            Assert.AreNotEqual(purple, green);
        }

        [Test]
        public void MathReadinessContentTheme_StripeColor_IsSemiTransparent()
        {
            var stripe = MathReadinessContentTheme.GetStripeColor(MathReadinessContent.AngleDirection);
            Assert.AreEqual(0.22f, stripe.a, 0.01f);
        }

        [Test]
        public void MathReadinessFormatter_FormatProgressMessage()
        {
            Assert.AreEqual("Q1/3", MathReadinessFormatter.FormatProgressMessage(0, 3));
            Assert.AreEqual("Q2/3", MathReadinessFormatter.FormatProgressMessage(1, 3));
            Assert.AreEqual("Q3/3", MathReadinessFormatter.FormatProgressMessage(2, 3));
        }

        [Test]
        public void MathReadinessFormatter_FormatAdaptiveHint_NoHintUnder2Attempts()
        {
            var result = MathReadinessFormatter.FormatAdaptiveHint(1, "some hint");
            Assert.IsEmpty(result);
        }

        [Test]
        public void MathReadinessFormatter_FormatAdaptiveHint_ShowsCoachAt2Attempts()
        {
            var result = MathReadinessFormatter.FormatAdaptiveHint(2, "힌트입니다");
            Assert.AreEqual("힌트입니다", result);
        }

        [Test]
        public void MathReadinessFormatter_FormatAdaptiveHint_ShowsAnswerAt3Attempts()
        {
            var result = MathReadinessFormatter.FormatAdaptiveHint(3, "힌트입니다");
            StringAssert.Contains("정답을 확인", result);
        }

        [Test]
        public void MathReadinessFormatter_GetDirectionIconName_Right()
        {
            var icon = MathReadinessFormatter.GetDirectionIconName(new KineTutor3D.Math.Vec3D(1, 0, 0));
            Assert.AreEqual("icon-arrow-right", icon);
        }

        [Test]
        public void MathReadinessFormatter_GetDirectionIconName_UpRight()
        {
            var icon = MathReadinessFormatter.GetDirectionIconName(new KineTutor3D.Math.Vec3D(1, 1, 0));
            Assert.AreEqual("icon-arrow-up-right", icon);
        }

        [Test]
        public void MathReadinessFormatter_GetDirectionIconName_NoMovement()
        {
            var icon = MathReadinessFormatter.GetDirectionIconName(new KineTutor3D.Math.Vec3D(0, 0, 0));
            Assert.AreEqual("icon-help", icon);
        }

        [Test]
        public void EnsureLayout_WithStandardJointSlider2_Succeeds()
        {
            InvokeEnsureLayout();

            Assert.That(GetPrivateField<bool>("layoutBound"), Is.True, "Layout should bind with standard slider names.");
        }

        [Test]
        public void EnsureLayout_WithLegacyJointSlider2_FallsBackSuccessfully()
        {
            var jointSlider2 = FindDescendant("joint_slider_2");
            jointSlider2.name = "legacy_joint_slider_2";
            RecreatePanel();

            InvokeEnsureLayout();

            Assert.That(GetPrivateField<bool>("layoutBound"), Is.True, "Layout should bind with legacy second slider name.");
        }

        [Test]
        public void EnsureLayout_WithoutCoachCard_StillSucceeds()
        {
            UnityEngine.Object.DestroyImmediate(FindDescendant("CoachCard").gameObject);
            RecreatePanel();

            InvokeEnsureLayout();

            Assert.That(GetPrivateField<bool>("layoutBound"), Is.True, "Layout should still bind without coach UI.");
        }

        [Test]
        public void RefreshFooter_WithoutSecondJointRefs_DoesNotThrow()
        {
            UnityEngine.Object.DestroyImmediate(FindDescendant("joint_slider_2").gameObject);
            UnityEngine.Object.DestroyImmediate(FindDescendant("Joint2Input").gameObject);
            UnityEngine.Object.DestroyImmediate(FindDescendant("Joint2ValueText").gameObject);
            RecreatePanel();
            InvokeEnsureLayout();

            var appController = CreateAppController();
            SetPrivateField("appController", appController);

            Assert.DoesNotThrow(InvokeRefreshFooter, "RefreshFooter should tolerate missing second joint footer refs.");
        }

        [Test]
        public void SliderValueChange_UpdatesAppControllerJointState()
        {
            InvokeEnsureLayout();

            var appController = CreateAppController();
            SetPrivateField("appController", appController);
            InvokePrivateMethod("BindListeners");

            var slider = FindDescendant("joint_slider_1").GetComponent<Slider>();
            slider.onValueChanged.Invoke(30f);

            Assert.That(appController.CurrentTemplate, Is.Not.Null, "Slider change should initialize the template runtime.");
            Assert.AreEqual(30f * Mathf.Deg2Rad, appController.CurrentJointValuesRad[0], 1e-5, "Slider change should update joint 1 radians.");
        }

        [Test]
        public void SliderValueChange_WithoutCoachCard_StillUpdatesAppControllerJointState()
        {
            UnityEngine.Object.DestroyImmediate(FindDescendant("CoachCard").gameObject);
            RecreatePanel();
            InvokeEnsureLayout();

            var appController = CreateAppController();
            SetPrivateField("appController", appController);
            InvokePrivateMethod("BindListeners");

            var slider = FindDescendant("joint_slider_1").GetComponent<Slider>();
            slider.onValueChanged.Invoke(45f);

            Assert.That(appController.CurrentTemplate, Is.Not.Null, "Slider change should still initialize the template runtime without coach UI.");
            Assert.AreEqual(45f * Mathf.Deg2Rad, appController.CurrentJointValuesRad[0], 1e-5, "Slider change should still update joint 1 radians without coach UI.");
        }

        private void InvokeEnsureLayout()
        {
            InvokePrivateMethod("EnsureLayout");
        }

        private void RecreatePanel()
        {
            UnityEngine.Object.DestroyImmediate(_panel);
            _panel = _host.AddComponent<MathReadinessPanel>();
        }

        private Transform FindDescendant(string objectName)
        {
            return FindDescendantRecursive(_host.transform, objectName);
        }

        private AppController CreateAppController()
        {
            var appHost = new GameObject("AppControllerHost");
            appHost.transform.SetParent(_host.transform, false);
            return appHost.AddComponent<AppController>();
        }

        private T GetPrivateField<T>(string fieldName)
        {
            var field = typeof(MathReadinessPanel).GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            return (T)field.GetValue(_panel);
        }

        private void SetPrivateField(string fieldName, object value)
        {
            var field = typeof(MathReadinessPanel).GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            field.SetValue(_panel, value);
        }

        private void InvokeRefreshFooter()
        {
            InvokePrivateMethod("RefreshFooter");
        }

        private void InvokePrivateMethod(string methodName)
        {
            var method = typeof(MathReadinessPanel).GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic);
            method.Invoke(_panel, null);
        }

        private static void BuildAuthoredHierarchy(Transform root)
        {
            CreateRect(root, "PageTopBar");
            CreateButton(root, "PageTopBar/BackButton");
            CreateButton(root, "PageTopBar/HomeButton");
            CreateButton(root, "PageTopBar/LibraryButton");
            CreateButton(root, "PageTopBar/SandboxButton");
            CreateText(root, "PageTopBar/PageTitleText");
            CreateText(root, "PageTopBar/PageContextText");

            CreateRect(root, "PageViewportPanel");
            CreateText(root, "PageViewportPanel/ViewportPanelTitleText");
            CreateText(root, "PageViewportPanel/ViewportPanelBodyText");

            CreateRect(root, "MainContent/QuestionColumn/OverviewCard");
            CreateImage(root, "MainContent/QuestionColumn/OverviewCard/MRP_ConceptStripe");
            CreateText(root, "MainContent/QuestionColumn/OverviewCard/MRP_OverviewLabel");
            CreateText(root, "MainContent/QuestionColumn/OverviewCard/MRP_LessonTitle");
            CreateText(root, "MainContent/QuestionColumn/OverviewCard/MRP_LessonGoal");
            CreateText(root, "MainContent/QuestionColumn/OverviewCard/MRP_Intro");

            CreateRect(root, "MainContent/QuestionColumn/QuestionCard");
            CreateText(root, "MainContent/QuestionColumn/QuestionCard/QuestionSectionLabel");
            CreateRect(root, "MainContent/QuestionColumn/QuestionCard/MRP_ProgressBadge");
            CreateText(root, "MainContent/QuestionColumn/QuestionCard/MRP_ProgressBadge/Label");
            CreateRect(root, "MainContent/QuestionColumn/QuestionCard/QuestionStack/WarmupBlock");
            CreateText(root, "MainContent/QuestionColumn/QuestionCard/QuestionStack/WarmupBlock/MRP_WarmupLabel");
            CreateText(root, "MainContent/QuestionColumn/QuestionCard/QuestionStack/WarmupBlock/MRP_WarmupPrompt");
            CreateText(root, "MainContent/QuestionColumn/QuestionCard/QuestionStack/WarmupBlock/MRP_WarmupFeedback");
            CreateButton(root, "MainContent/QuestionColumn/QuestionCard/QuestionStack/WarmupBlock/BtnWarmupChoice_0");
            CreateButton(root, "MainContent/QuestionColumn/QuestionCard/QuestionStack/WarmupBlock/BtnWarmupChoice_1");
            CreateButton(root, "MainContent/QuestionColumn/QuestionCard/QuestionStack/WarmupBlock/BtnWarmupChoice_2");
            CreateRect(root, "MainContent/QuestionColumn/QuestionCard/QuestionStack/ManipulationBlock");
            CreateText(root, "MainContent/QuestionColumn/QuestionCard/QuestionStack/ManipulationBlock/MRP_ManipulationInstruction");
            CreateRect(root, "MainContent/QuestionColumn/QuestionCard/QuestionStack/ManipulationBlock/MRP_TargetBadge");
            CreateText(root, "MainContent/QuestionColumn/QuestionCard/QuestionStack/ManipulationBlock/MRP_TargetBadge/Label");
            CreateText(root, "MainContent/QuestionColumn/QuestionCard/QuestionStack/MRP_QuestionPrompt");
            CreateRect(root, "MainContent/QuestionColumn/QuestionCard/QuestionStack/AnswerButtons");
            CreateButton(root, "MainContent/QuestionColumn/QuestionCard/QuestionStack/AnswerButtons/BtnReadinessChoice_0");
            CreateButton(root, "MainContent/QuestionColumn/QuestionCard/QuestionStack/AnswerButtons/BtnReadinessChoice_1");
            CreateButton(root, "MainContent/QuestionColumn/QuestionCard/QuestionStack/AnswerButtons/BtnReadinessChoice_2");
            CreateRect(root, "MainContent/QuestionColumn/QuestionCard/QuestionStack/FeedbackRow");
            CreateImage(root, "MainContent/QuestionColumn/QuestionCard/QuestionStack/FeedbackRow/MRP_FeedbackIcon");
            CreateText(root, "MainContent/QuestionColumn/QuestionCard/QuestionStack/FeedbackRow/MRP_FeedbackText");
            CreateText(root, "MainContent/QuestionColumn/QuestionCard/QuestionStack/MRP_AdaptiveHint");

            CreateRect(root, "MainContent/SupportColumn/CoachCard");
            CreateText(root, "MainContent/SupportColumn/CoachCard/MRP_CommonMistake");
            CreateButton(root, "MainContent/SupportColumn/CoachCard/BtnToggleCoachHint");
            CreateImage(root, "MainContent/SupportColumn/CoachCard/BtnToggleCoachHint/LeadingIcon");
            CreateText(root, "MainContent/SupportColumn/CoachCard/MRP_CoachHintBody");
            CreateRect(root, "MainContent/SupportColumn/WhyMovedSummaryCard");
            CreateText(root, "MainContent/SupportColumn/WhyMovedSummaryCard/WhyMovedSummaryText");

            CreateRect(root, "PageFooter/JointControlCard");
            CreateSlider(root, "PageFooter/JointControlCard/joint_slider_1");
            CreateSlider(root, "PageFooter/JointControlCard/joint_slider_2");
            CreateInputField(root, "PageFooter/JointControlCard/Joint1Input");
            CreateInputField(root, "PageFooter/JointControlCard/Joint2Input");
            CreateText(root, "PageFooter/JointControlCard/Joint1ValueText");
            CreateText(root, "PageFooter/JointControlCard/Joint2ValueText");
            CreateText(root, "PageFooter/FooterStepTitleText");
            CreateText(root, "PageFooter/FooterStepProgressText");
            CreateButton(root, "PageFooter/BtnPrev");
            CreateButton(root, "PageFooter/BtnSkip");
            CreateButton(root, "PageFooter/BtnNext");
        }

        private static Transform CreateRect(Transform root, string path)
        {
            return EnsurePath(root, path, typeof(RectTransform));
        }

        private static Transform CreateText(Transform root, string path)
        {
            var transform = EnsurePath(root, path, typeof(RectTransform), typeof(CanvasRenderer), typeof(UnityEngine.UI.Text));
            var text = transform.GetComponent<UnityEngine.UI.Text>();
            text.text = path;
            return transform;
        }

        private static Transform CreateImage(Transform root, string path)
        {
            return EnsurePath(root, path, typeof(RectTransform), typeof(CanvasRenderer), typeof(UnityEngine.UI.Image));
        }

        private static Transform CreateButton(Transform root, string path)
        {
            var transform = EnsurePath(root, path, typeof(RectTransform), typeof(CanvasRenderer), typeof(UnityEngine.UI.Image), typeof(UnityEngine.UI.Button));
            CreateText(transform, "Label");
            return transform;
        }

        private static Transform CreateSlider(Transform root, string path)
        {
            return EnsurePath(root, path, typeof(RectTransform), typeof(CanvasRenderer), typeof(UnityEngine.UI.Image), typeof(UnityEngine.UI.Slider));
        }

        private static Transform CreateInputField(Transform root, string path)
        {
            var transform = EnsurePath(root, path, typeof(RectTransform), typeof(CanvasRenderer), typeof(UnityEngine.UI.Image), typeof(UnityEngine.UI.InputField));
            CreateText(transform, "Text");
            CreateText(transform, "Placeholder");
            return transform;
        }

        private static Transform EnsurePath(Transform root, string path, params Type[] leafComponents)
        {
            var parts = path.Split('/');
            var current = root;
            for (var i = 0; i < parts.Length; i++)
            {
                var child = current.Find(parts[i]);
                if (child == null)
                {
                    var go = new GameObject(parts[i], i == parts.Length - 1 ? leafComponents : new[] { typeof(RectTransform) });
                    child = go.transform;
                    child.SetParent(current, false);
                }

                current = child;
            }

            return current;
        }

        private static Transform FindDescendantRecursive(Transform root, string objectName)
        {
            if (root == null)
            {
                return null;
            }

            if (root.name == objectName)
            {
                return root;
            }

            for (var i = 0; i < root.childCount; i++)
            {
                var found = FindDescendantRecursive(root.GetChild(i), objectName);
                if (found != null)
                {
                    return found;
                }
            }

            return null;
        }
    }
}
