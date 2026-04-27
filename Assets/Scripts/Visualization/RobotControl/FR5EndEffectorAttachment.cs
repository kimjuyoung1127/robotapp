// Folder: Visualization - RobotControl-specific rendering and overlay drivers.
using System.Collections;
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
        [SerializeField, Range(0f, 1f)] private float gripperOpenRatio = 1f;
        [SerializeField, Range(0.05f, 2f)] private float gripperMotionDuration = 0.55f;

        private const float StrokeMm = 40f;
        private static readonly Color BodyColor = new(0.13f, 0.18f, 0.22f, 1f);
        private static readonly Color BodyAccentColor = new(0.18f, 0.52f, 0.68f, 1f);
        private static readonly Color FingerColor = new(1f, 0.58f, 0.14f, 1f);
        private Vector3 fingerLeftOpen;
        private Vector3 fingerRightOpen;
        private Vector3 fingerLeftOpenDirection = Vector3.right;
        private Vector3 fingerRightOpenDirection = Vector3.left;
        private bool fingerBaseCaptured;
        private bool hasGripObject;
        private float gripObjectStopRatio;
        private Coroutine gripperMotionCoroutine;

        public string AttachmentId => attachmentId;
        public Transform VisualRoot => visualRoot;
        public Transform ModelRoot => visualRoot != null ? visualRoot.Find("PGEA-100-40_Model") : null;
        public Transform TcpFrame => tcpFrame;
        public Transform GripTarget => ResolveGripTarget();
        public Transform FingerLeft => fingerLeft;
        public Transform FingerRight => fingerRight;
        public float GripperOpenRatio => gripperOpenRatio;
        public bool HasGripObject => TryGetGripObjectStopRatio(out _);

        public void Configure(string id, Transform visual, Transform tcp)
        {
            attachmentId = string.IsNullOrWhiteSpace(id) ? attachmentId : id;
            visualRoot = visual;
            tcpFrame = tcp;
            RefreshExistingReferences();
            ApplyVisibilityMaterials();
        }

        public void SetFingers(Transform left, Transform right)
        {
            fingerLeft = left;
            fingerRight = right;
            fingerBaseCaptured = false;
            ApplyGripperPose();
        }

        public string BuildClosureDebugSummary()
        {
            RefreshExistingReferences();
            var target = ResolveGripTarget();
            var leftDistance = GetFingerTargetDistance(fingerLeft, target);
            var rightDistance = GetFingerTargetDistance(fingerRight, target);
            var objectDetected = TryGetGripObjectStopRatio(out var stopRatio);
            return $"target={target?.name ?? "missing"}; authoredOpenCaptured={fingerBaseCaptured}; objectDetected={objectDetected}; objectStop={stopRatio:0.##}; leftDistance={leftDistance:0.####}; rightDistance={rightDistance:0.####}; leftOpen=({fingerLeftOpen.x:0.####},{fingerLeftOpen.y:0.####},{fingerLeftOpen.z:0.####}); rightOpen=({fingerRightOpen.x:0.####},{fingerRightOpen.y:0.####},{fingerRightOpen.z:0.####}); leftOpenDir=({fingerLeftOpenDirection.x:0.###},{fingerLeftOpenDirection.y:0.###},{fingerLeftOpenDirection.z:0.###}); rightOpenDir=({fingerRightOpenDirection.x:0.###},{fingerRightOpenDirection.y:0.###},{fingerRightOpenDirection.z:0.###})";
        }

        private void ApplyVisibilityMaterials()
        {
            if (visualRoot == null)
            {
                return;
            }

            foreach (var meshRenderer in visualRoot.GetComponentsInChildren<MeshRenderer>(true))
            {
                var lowerName = meshRenderer.name.ToLowerInvariant();
                var color = lowerName.Contains("finger")
                    ? FingerColor
                    : lowerName.Contains("body")
                        ? BodyColor
                        : BodyAccentColor;

                var source = meshRenderer.sharedMaterial;
                var material = source != null ? new Material(source) : new Material(Shader.Find("Standard"));
                material.name = $"{attachmentId}_{meshRenderer.name}_Runtime";
                material.color = color;
                meshRenderer.sharedMaterial = material;
            }
        }

        public void SetGripperOpen(float ratio)
        {
            var targetRatio = Mathf.Clamp01(ratio);
            if (!Application.isPlaying || !isActiveAndEnabled)
            {
                SetGripperOpenImmediate(targetRatio);
                return;
            }

            if (Mathf.Abs(gripperOpenRatio - targetRatio) < 0.001f)
            {
                SetGripperOpenImmediate(targetRatio);
                return;
            }

            if (gripperMotionCoroutine != null)
            {
                StopCoroutine(gripperMotionCoroutine);
            }

            gripperMotionCoroutine = StartCoroutine(AnimateGripperOpenRatio(targetRatio));
        }

        public bool TryGetGripObjectStopRatio(out float stopRatio)
        {
            RefreshExistingReferences();
            RefreshGripObjectStopRatio();
            stopRatio = gripObjectStopRatio;
            return hasGripObject;
        }

        public void RecaptureAuthoredOpenPose()
        {
            StopGripperMotion();
            fingerBaseCaptured = false;
            CaptureFingerBase();
            SetGripperOpenImmediate(1f);
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

            fingerLeftOpen = fingerLeft.localPosition;
            fingerRightOpen = fingerRight.localPosition;
            CaptureOpenDirections();
            fingerBaseCaptured = true;
        }

        private void CaptureOpenDirections()
        {
            var target = ResolveGripTarget();
            fingerLeftOpenDirection = ResolveOpenDirection(fingerLeft, target, Vector3.right);
            fingerRightOpenDirection = ResolveOpenDirection(fingerRight, target, Vector3.left);
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

            var closeTravel = StrokeMm * 0.5f * (1f - gripperOpenRatio);
            fingerLeft.localPosition = fingerLeftOpen - fingerLeftOpenDirection * closeTravel;
            fingerRight.localPosition = fingerRightOpen - fingerRightOpenDirection * closeTravel;
        }

        private IEnumerator AnimateGripperOpenRatio(float targetRatio)
        {
            var startRatio = gripperOpenRatio;
            var duration = Mathf.Max(0.05f, gripperMotionDuration);
            var elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                var t = Mathf.Clamp01(elapsed / duration);
                gripperOpenRatio = Mathf.Lerp(startRatio, targetRatio, Mathf.SmoothStep(0f, 1f, t));
                ApplyGripperPose();
                yield return null;
            }

            gripperOpenRatio = targetRatio;
            ApplyGripperPose();
            gripperMotionCoroutine = null;
        }

        private void SetGripperOpenImmediate(float ratio)
        {
            gripperOpenRatio = Mathf.Clamp01(ratio);
            ApplyGripperPose();
        }

        private void StopGripperMotion()
        {
            if (gripperMotionCoroutine == null)
            {
                return;
            }

            StopCoroutine(gripperMotionCoroutine);
            gripperMotionCoroutine = null;
        }

        private Transform ResolveGripTarget()
        {
            if (tcpFrame == null)
            {
                return null;
            }

            return tcpFrame.Find("TcpMarker") ?? tcpFrame;
        }

        private void RefreshGripObjectStopRatio()
        {
            hasGripObject = false;
            gripObjectStopRatio = 0f;
            var target = ResolveGripTarget();
            if (target == null || target == tcpFrame)
            {
                return;
            }

            var renderers = target.GetComponentsInChildren<Renderer>(true);
            if (renderers == null || renderers.Length == 0)
            {
                return;
            }

            hasGripObject = true;
            gripObjectStopRatio = 0.35f;
        }

        private Vector3 ResolveOpenDirection(Transform finger, Transform target, Vector3 fallback)
        {
            if (finger == null || target == null || finger.parent == null)
            {
                return fallback;
            }

            var fingerCenter = ResolveRendererCenter(finger);
            var awayWorld = fingerCenter - target.position;
            if (awayWorld.sqrMagnitude < 0.000001f)
            {
                return fallback;
            }

            var local = finger.parent.InverseTransformVector(awayWorld).normalized;
            return local.sqrMagnitude > 0.000001f ? local : fallback;
        }

        private static Vector3 ResolveRendererCenter(Transform root)
        {
            var renderers = root != null ? root.GetComponentsInChildren<Renderer>(true) : null;
            if (renderers == null || renderers.Length == 0)
            {
                return root != null ? root.position : Vector3.zero;
            }

            var found = false;
            var bounds = new Bounds(root.position, Vector3.zero);
            for (var i = 0; i < renderers.Length; i++)
            {
                var renderer = renderers[i];
                if (renderer == null)
                {
                    continue;
                }

                if (!found)
                {
                    bounds = renderer.bounds;
                    found = true;
                }
                else
                {
                    bounds.Encapsulate(renderer.bounds);
                }
            }

            return found ? bounds.center : root.position;
        }

        private static float GetFingerTargetDistance(Transform finger, Transform target)
        {
            if (finger == null || target == null)
            {
                return -1f;
            }

            return Vector3.Distance(ResolveRendererCenter(finger), target.position);
        }

        private void OnValidate()
        {
            RefreshExistingReferences();
            SetGripperOpenImmediate(gripperOpenRatio);
        }

        private void OnDisable()
        {
            StopGripperMotion();
        }
    }
}
