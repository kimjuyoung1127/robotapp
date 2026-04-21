// Folder: Visualization - RobotControl-specific rendering and overlay drivers.
using UnityEngine;

namespace KineTutor3D.Visualization
{
    /// <summary>
    /// FR5 PGEA 계열 그리퍼의 visual root, TCP frame, finger 개폐를 관리합니다.
    /// robottemplete의 PGEA-100-40 성공 패턴을 V3 런타임에서 재사용합니다.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class FR5EndEffectorAttachment : MonoBehaviour
    {
        [SerializeField] private string attachmentId = "PGEA_100_40";
        [SerializeField] private Transform visualRoot;
        [SerializeField] private Transform tcpFrame;
        [SerializeField] private Transform fingerLeft;
        [SerializeField] private Transform fingerRight;
        [SerializeField, Range(0f, 1f)] private float gripperOpenRatio;

        private const float StrokeMm = 40f;
        private Vector3 fingerLeftClosed;
        private Vector3 fingerRightClosed;
        private bool fingerBaseCaptured;

        public string AttachmentId => attachmentId;
        public float GripperOpenRatio => gripperOpenRatio;

        public void Configure(string id, Transform visual, Transform tcp)
        {
            attachmentId = string.IsNullOrWhiteSpace(id) ? attachmentId : id;
            visualRoot = visual;
            tcpFrame = tcp;
            RefreshExistingReferences();
        }

        public void SetFingers(Transform left, Transform right)
        {
            fingerLeft = left;
            fingerRight = right;
            fingerBaseCaptured = false;
            ApplyGripperPose();
        }

        public void SetGripperOpen(float ratio)
        {
            gripperOpenRatio = Mathf.Clamp01(ratio);
            ApplyGripperPose();
        }

        private void RefreshExistingReferences()
        {
            visualRoot ??= transform.Find("VisualRoot");
            tcpFrame ??= transform.Find("TcpFrame");
            if (fingerLeft != null && fingerRight != null)
            {
                return;
            }

            var model = visualRoot != null ? visualRoot.Find("PGEA-100-40_Model") : null;
            if (model != null)
            {
                fingerLeft ??= model.Find("finger_left");
                fingerRight ??= model.Find("finger_right");
            }
        }

        private void CaptureFingerBase()
        {
            if (fingerBaseCaptured || fingerLeft == null || fingerRight == null)
            {
                return;
            }

            if (gripperOpenRatio > 0.001f)
            {
                return;
            }

            fingerLeftClosed = fingerLeft.localPosition;
            fingerRightClosed = fingerRight.localPosition;
            fingerBaseCaptured = true;
        }

        private void ApplyGripperPose()
        {
            RefreshExistingReferences();
            if (fingerLeft == null || fingerRight == null)
            {
                return;
            }

            CaptureFingerBase();
            if (!fingerBaseCaptured)
            {
                return;
            }

            var halfStroke = StrokeMm * 0.5f * gripperOpenRatio;
            fingerLeft.localPosition = fingerLeftClosed + new Vector3(halfStroke, 0f, 0f);
            fingerRight.localPosition = fingerRightClosed + new Vector3(-halfStroke, 0f, 0f);
        }

        private void OnValidate()
        {
            RefreshExistingReferences();
            ApplyGripperPose();
        }
    }
}
