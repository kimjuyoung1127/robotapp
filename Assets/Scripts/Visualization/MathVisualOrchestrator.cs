// Folder: Visualization - Unity-side rendering and FK binding.
using KineTutor3D.UI.Data;
using UnityEngine;

namespace KineTutor3D.Visualization
{
    /// <summary>
    /// MathReadiness 레슨별 시각 힌트를 오케스트레이션합니다.
    /// 각 레슨(M0~M3)에서 어떤 시각 요소를 보여줄지 제어합니다.
    /// </summary>
    public class MathVisualOrchestrator : MonoBehaviour
    {
        private SimpleArmOverlay armOverlay;
        private AngleArcVisual angleArc0;
        private AngleArcVisual angleArc1;
        private AngleReferenceMarker angleRef0;
        private AngleReferenceMarker angleRef1;
        private LengthLabelVisual lengthLabel0;
        private LengthLabelVisual lengthLabel1;
        private CoordinateGridVisual coordGrid;

        private Transform[] jointAnchors;
        private bool initialized;
        private MathReadinessContent currentContent;
        private Color currentThemeColor;

        /// <summary>
        /// 조인트 앵커를 설정하고 서브 컴포넌트를 초기화합니다.
        /// </summary>
        public void Initialize(Transform baseAnchor, Transform joint1Anchor, Transform joint2Anchor, Transform eeAnchor)
        {
            jointAnchors = new[] { baseAnchor, joint1Anchor, joint2Anchor, eeAnchor };
            EnsureComponents();
            initialized = true;
        }

        /// <summary>
        /// MathReadiness 레슨 프리셋을 적용합니다.
        /// </summary>
        public void ApplyMathPreset(MathReadinessContent content)
        {
            if (!initialized)
            {
                return;
            }

            currentContent = content;
            currentThemeColor = MathReadinessContentTheme.GetAccentColor(content);
            HideAll();

            switch (content)
            {
                case MathReadinessContent.AngleDirection:
                    ApplyM0AngleDirection();
                    break;
                case MathReadinessContent.LengthAngleToPoint:
                    ApplyM1LengthAngle();
                    break;
                case MathReadinessContent.DiagonalIntuition:
                    ApplyM2Diagonal();
                    break;
                case MathReadinessContent.TwoLinkComposition:
                    ApplyM3TwoLink();
                    break;
            }
        }

        /// <summary>
        /// 현재 조인트 각도로 시각 요소를 갱신합니다.
        /// </summary>
        public void UpdateFromJointAngles(double[] thetasRad)
        {
            if (!initialized || thetasRad == null)
            {
                return;
            }

            armOverlay?.UpdateVisual();

            var theta0Deg = thetasRad.Length > 0 ? (float)(thetasRad[0] * Mathf.Rad2Deg) : 0f;
            var theta1Deg = thetasRad.Length > 1 ? (float)(thetasRad[1] * Mathf.Rad2Deg) : 0f;

            if (angleArc0 != null && angleArc0.IsVisible && jointAnchors[1] != null)
            {
                var sweep = theta0Deg;
                angleArc0.ShowArc(jointAnchors[1], 0f, sweep, currentThemeColor);
            }

            if (angleRef0 != null && angleRef0.IsVisible && jointAnchors[1] != null)
            {
                angleRef0.ShowMarkers(jointAnchors[1], currentThemeColor);
            }

            if (angleArc1 != null && angleArc1.IsVisible && jointAnchors[2] != null)
            {
                angleArc1.ShowArc(jointAnchors[2], 0f, theta1Deg, currentThemeColor);
            }

            if (angleRef1 != null && angleRef1.IsVisible && jointAnchors[2] != null)
            {
                angleRef1.ShowMarkers(jointAnchors[2], currentThemeColor);
            }

            if (lengthLabel0 != null && lengthLabel0.IsVisible && jointAnchors[1] != null && jointAnchors[2] != null)
            {
                lengthLabel0.ShowLength(jointAnchors[1].position, jointAnchors[2].position, currentThemeColor);
            }

            if (lengthLabel1 != null && lengthLabel1.IsVisible && jointAnchors[2] != null && jointAnchors[3] != null)
            {
                lengthLabel1.ShowLength(jointAnchors[2].position, jointAnchors[3].position, currentThemeColor);
            }
        }

        /// <summary>
        /// 모든 시각 힌트를 숨깁니다.
        /// </summary>
        public void HideAll()
        {
            armOverlay?.SetVisible(false);
            angleArc0?.SetVisible(false);
            angleArc1?.SetVisible(false);
            angleRef0?.SetVisible(false);
            angleRef1?.SetVisible(false);
            lengthLabel0?.SetVisible(false);
            lengthLabel1?.SetVisible(false);
            if (coordGrid != null)
            {
                coordGrid.SetVisible(false);
                coordGrid.ShowDiagonalGuide(false);
            }
        }

        /// <summary>
        /// 전체 오케스트레이터 표시/숨김을 제어합니다.
        /// </summary>
        public void SetVisible(bool visible)
        {
            gameObject.SetActive(visible);
        }

        private void ApplyM0AngleDirection()
        {
            armOverlay?.SetColor(currentThemeColor);
            armOverlay?.SetVisible(true);
            angleArc0?.SetVisible(true);
            angleRef0?.SetVisible(true);
        }

        private void ApplyM1LengthAngle()
        {
            armOverlay?.SetColor(currentThemeColor);
            armOverlay?.SetVisible(true);
            angleArc0?.SetVisible(true);
            angleRef0?.SetVisible(true);
            lengthLabel0?.SetVisible(true);
        }

        private void ApplyM2Diagonal()
        {
            armOverlay?.SetColor(currentThemeColor);
            armOverlay?.SetVisible(true);
            angleArc0?.SetVisible(true);
            angleRef0?.SetVisible(true);
            if (coordGrid != null)
            {
                coordGrid.SetVisible(true);
                coordGrid.ShowDiagonalGuide(true, false);
                coordGrid.SetDiagonalColor(currentThemeColor);
            }
        }

        private void ApplyM3TwoLink()
        {
            armOverlay?.SetColor(currentThemeColor);
            armOverlay?.SetVisible(true);
            angleArc0?.SetVisible(true);
            angleArc1?.SetVisible(true);
            angleRef0?.SetVisible(true);
            angleRef1?.SetVisible(false);
            lengthLabel0?.SetVisible(true);
            lengthLabel1?.SetVisible(true);
            if (coordGrid != null)
            {
                coordGrid.SetVisible(true);
                coordGrid.ShowDiagonalGuide(true, true);
                coordGrid.SetDiagonalColor(currentThemeColor);
            }
        }

        private void EnsureComponents()
        {
            armOverlay = GetOrAddChild<SimpleArmOverlay>("ArmOverlay");
            armOverlay.Initialize(jointAnchors, Color.white);

            angleArc0 = GetOrAddChild<AngleArcVisual>("AngleArc_J0");
            angleArc1 = GetOrAddChild<AngleArcVisual>("AngleArc_J1");
            angleRef0 = GetOrAddChild<AngleReferenceMarker>("AngleReference_J0");
            angleRef1 = GetOrAddChild<AngleReferenceMarker>("AngleReference_J1");

            lengthLabel0 = GetOrAddChild<LengthLabelVisual>("LengthLabel_L0");
            lengthLabel1 = GetOrAddChild<LengthLabelVisual>("LengthLabel_L1");

            coordGrid = GetOrAddChild<CoordinateGridVisual>("CoordinateGrid");
            coordGrid.Initialize();

            HideAll();
        }

        private T GetOrAddChild<T>(string childName) where T : MonoBehaviour
        {
            var child = transform.Find(childName);
            if (child != null)
            {
                var existing = child.GetComponent<T>();
                if (existing != null)
                {
                    return existing;
                }
            }

            var go = child != null ? child.gameObject : new GameObject(childName);
            if (child == null)
            {
                go.transform.SetParent(transform, false);
            }

            return go.AddComponent<T>();
        }
    }
}
