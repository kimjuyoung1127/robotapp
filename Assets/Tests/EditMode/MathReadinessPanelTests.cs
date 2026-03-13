using KineTutor3D.App;
using KineTutor3D.UI;
using KineTutor3D.UI.Data;
using NUnit.Framework;
using System;
using System.Reflection;
using UnityEngine;

namespace KineTutor3D.Tests.EditMode
{
    public class MathReadinessPanelTests
    {
        private GameObject _host;
        private MathReadinessPanel _panel;

        [SetUp]
        public void SetUp()
        {
            _host = new GameObject("MathReadinessHost", typeof(RectTransform));
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

        private void InvokeEnsureLayout()
        {
            var ensureLayout = typeof(MathReadinessPanel).GetMethod("EnsureLayout", BindingFlags.Instance | BindingFlags.NonPublic);
            ensureLayout.Invoke(_panel, null);
        }

        private Transform FindDescendant(string objectName)
        {
            return FindDescendantRecursive(_host.transform, objectName);
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
