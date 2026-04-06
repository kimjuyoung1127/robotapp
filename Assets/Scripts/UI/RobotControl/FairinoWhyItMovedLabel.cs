// Folder: UI - HUD/view components only; no kinematics logic.
using System.Globalization;
using KineTutor3D.App;
using KineTutor3D.App.Fairino;
using KineTutor3D.Math;
using UnityEngine;
using UnityEngine.UI;

namespace KineTutor3D.UI
{
    /// <summary>
    /// FR5 State 탭 하단에 FK 기반 TCP 변화를 한국어로 설명하는 라벨입니다.
    /// WhyItMovedFormatter를 6DOF로 확장하여 재활용합니다.
    /// </summary>
    public class FairinoWhyItMovedLabel : MonoBehaviour, IVisibilityControllable
    {
        [SerializeField] private Text explanationLabel;
        [SerializeField] private Font fallbackFont;

        private RobotKinematicsFacade kinematicsFacade;
        private Vec3D previousEEPosition;
        private double[] previousJointsDeg;
        private bool hasPrevious;

        /// <summary>
        /// FK facade를 주입합니다.
        /// </summary>
        public void Inject(RobotKinematicsFacade facade)
        {
            kinematicsFacade = facade;
            hasPrevious = false;
        }

        private void Awake()
        {
            EnsurePresentation();
        }

        private void OnEnable()
        {
            EnsurePresentation();
        }

        private void OnDisable()
        {
        }

        /// <summary>
        /// 상태 갱신 시 호출하여 변화 설명을 갱신합니다.
        /// </summary>
        public void HandleStateUpdated(FairinoRobotState state)
        {
            if (kinematicsFacade == null || explanationLabel == null)
            {
                return;
            }

            var currentJointsDeg = state.JointPosDeg;
            if (currentJointsDeg == null || currentJointsDeg.Length < 6)
            {
                return;
            }

            var currentEE = kinematicsFacade.EndEffectorTransform.ExtractPosition();

            if (!hasPrevious)
            {
                previousJointsDeg = (double[])currentJointsDeg.Clone();
                previousEEPosition = currentEE;
                hasPrevious = true;
                explanationLabel.text = "관절을 이동하면 변화 설명이 표시됩니다.";
                return;
            }

            var topTwo = FindTopTwoChangedJoints(previousJointsDeg, currentJointsDeg);
            if (topTwo.first < 0)
            {
                return;
            }

            var displacement = currentEE - previousEEPosition;
            var distance = displacement.Magnitude();
            if (distance < 0.0001)
            {
                previousJointsDeg = (double[])currentJointsDeg.Clone();
                previousEEPosition = currentEE;
                return;
            }

            var directionText = DescribeDirection(displacement);
            var distText = distance.ToString("F4", CultureInfo.InvariantCulture);
            var dxText = System.Math.Abs(displacement.X).ToString("F3", CultureInfo.InvariantCulture);
            var dyText = System.Math.Abs(displacement.Y).ToString("F3", CultureInfo.InvariantCulture);
            var dzText = System.Math.Abs(displacement.Z).ToString("F3", CultureInfo.InvariantCulture);

            var jointSummary = FormatJointDelta(topTwo.first, previousJointsDeg, currentJointsDeg);
            if (topTwo.second >= 0)
            {
                jointSummary += " + " + FormatJointDelta(topTwo.second, previousJointsDeg, currentJointsDeg);
            }

            explanationLabel.text = $"{jointSummary}\nTCP {directionText} {distText}m (X:{dxText} Y:{dyText} Z:{dzText})";
            explanationLabel.color = UIDesignTokens.Colors.TextSecondary;

            previousJointsDeg = (double[])currentJointsDeg.Clone();
            previousEEPosition = currentEE;
        }

        /// <summary>
        /// 패널 가시성을 설정합니다.
        /// </summary>
        public void SetVisible(bool visible)
        {
            gameObject.SetActive(visible);
        }

        private void EnsurePresentation()
        {
            fallbackFont = UiRuntimeStyle.ResolveFont(fallbackFont);
            var root = transform as RectTransform;
            if (root == null)
            {
                return;
            }

            var background = root.GetComponent<Image>() ?? root.gameObject.AddComponent<Image>();
            background.color = UIDesignTokens.Colors.SurfaceRaisedAlt;

            if (TryBindExistingPresentation(root))
            {
                return;
            }

            var title = UiRuntimeStyle.EnsureText(root, "WhyTitle", fallbackFont, UIDesignTokens.Type.Caption, FontStyle.Bold, TextAnchor.UpperLeft, UIDesignTokens.Colors.AccentSecondary);
            UiRuntimeStyle.Anchor(title.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(220f, 14f), new Vector2(10f, -8f));
            title.text = "움직임 해설";

            explanationLabel = UiRuntimeStyle.EnsureText(root, "WhyLabel", fallbackFont, UIDesignTokens.Type.Caption, FontStyle.Normal, TextAnchor.UpperLeft, UIDesignTokens.Colors.TextSecondary);
            UiRuntimeStyle.Anchor(explanationLabel.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(340f, 84f), new Vector2(10f, -26f));
            explanationLabel.text = "관절을 이동하면 변화 설명이 표시됩니다.";
        }

        private bool TryBindExistingPresentation(RectTransform root)
        {
            explanationLabel = root.Find("WhyLabel")?.GetComponent<Text>();
            if (explanationLabel == null)
            {
                return false;
            }

            var title = root.Find("WhyTitle")?.GetComponent<Text>();
            if (title != null)
            {
                title.text = "움직임 해설";
            }

            if (string.IsNullOrWhiteSpace(explanationLabel.text))
            {
                explanationLabel.text = "관절을 이동하면 변화 설명이 표시됩니다.";
            }

            return true;
        }

        private static string FormatJointDelta(int index, double[] prev, double[] curr)
        {
            var deltaDeg = curr[index] - prev[index];
            var absDelta = System.Math.Abs(deltaDeg).ToString("F1", CultureInfo.InvariantCulture);
            var direction = deltaDeg > 0 ? "+" : "-";
            return $"J{index + 1}({direction}{absDelta}°)";
        }

        private static (int first, int second) FindTopTwoChangedJoints(double[] prev, double[] curr)
        {
            var firstIndex = -1;
            var secondIndex = -1;
            var firstDelta = 0.0;
            var secondDelta = 0.0;
            var count = System.Math.Min(prev.Length, curr.Length);

            for (var i = 0; i < count; i++)
            {
                var delta = System.Math.Abs(curr[i] - prev[i]);
                if (delta > firstDelta)
                {
                    secondDelta = firstDelta;
                    secondIndex = firstIndex;
                    firstDelta = delta;
                    firstIndex = i;
                }
                else if (delta > secondDelta)
                {
                    secondDelta = delta;
                    secondIndex = i;
                }
            }

            // 두번째 관절은 0.5° 이상 변한 경우만 표시
            if (secondDelta < 0.5)
            {
                secondIndex = -1;
            }

            return (firstIndex, secondIndex);
        }

        private static string DescribeDirection(Vec3D displacement)
        {
            var absX = System.Math.Abs(displacement.X);
            var absY = System.Math.Abs(displacement.Y);
            var absZ = System.Math.Abs(displacement.Z);

            if (absX < 1e-4 && absY < 1e-4 && absZ < 1e-4)
            {
                return "거의 같은 위치에서";
            }

            var parts = new System.Collections.Generic.List<string>();
            if (absX > 1e-4) parts.Add(displacement.X > 0 ? "오른쪽" : "왼쪽");
            if (absY > 1e-4) parts.Add(displacement.Y > 0 ? "앞" : "뒤");
            if (absZ > 1e-4) parts.Add(displacement.Z > 0 ? "위" : "아래");

            return parts.Count > 0 ? string.Join(" ", parts) + "으로" : "거의 같은 위치에서";
        }
    }
}
