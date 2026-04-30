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
        [SerializeField] private Transform gripObjectRoot;
        [SerializeField] private Transform fingerLeft;
        [SerializeField] private Transform fingerRight;
        [SerializeField] private bool useAuthoredClosedPose;
        [SerializeField] private Vector3 fingerLeftClosed;
        [SerializeField] private Vector3 fingerRightClosed;
        [SerializeField, Range(0f, 1f)] private float gripperOpenRatio = 1f;
        [SerializeField, Range(0f, 0.95f)] private float visualClosedAtOpenRatio = 0.6f;
        [SerializeField, Range(0.05f, 2f)] private float gripperMotionDuration = 0.55f;
        [SerializeField, Range(0.005f, 0.2f)] private float gripProbeRadius = 0.035f;
        [SerializeField, Range(0.05f, 0.95f)] private float detectedObjectStopRatio = 0.7f;

        private static readonly Color BodyColor = new(0.13f, 0.18f, 0.22f, 1f);
        private static readonly Color BodyAccentColor = new(0.18f, 0.52f, 0.68f, 1f);
        private static readonly Color FingerColor = new(1f, 0.58f, 0.14f, 1f);
        private Vector3 fingerLeftOpen;
        private Vector3 fingerRightOpen;
        private Vector3 fingerLeftCloseTravel;
        private Vector3 fingerRightCloseTravel;
        private bool fingerBaseCaptured;
        private bool hasGripObject;
        private float gripObjectStopRatio;
        private Coroutine gripperMotionCoroutine;
        private const float DistortedFingerLocalPositionSqrMagnitude = 1f;
        // Fallback only. Prefer an authored closed pose for the PGEA finger mesh.
        // Bump the fallback close travel so user 0% appears much nearer to real contact
        // when we do not yet have reliable live gripper readback or an authored closed pose.
        private const float ClosedContactTravelScale = 1.6f;

        public string AttachmentId => attachmentId;
        public Transform VisualRoot => visualRoot;
        public Transform ModelRoot => visualRoot != null ? visualRoot.Find("PGEA-100-40_Model") : null;
        public Transform TcpFrame => tcpFrame;
        public Transform GripTarget => ResolveGripTargetTransform();
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
            RemoveLegacyGripMarkers();
            ApplyVisibilityMaterials();
        }

        public void RemoveLegacyGripMarkers()
        {
            RefreshExistingReferences();
            var legacyMarker = tcpFrame != null ? tcpFrame.Find("TcpMarker") : null;
            if (legacyMarker == null)
            {
                return;
            }

            if (Application.isPlaying)
            {
                Destroy(legacyMarker.gameObject);
            }
            else
            {
                DestroyImmediate(legacyMarker.gameObject);
            }
        }

        public void SetFingers(Transform left, Transform right)
        {
            fingerLeft = left;
            fingerRight = right;
            fingerBaseCaptured = false;
            ApplyGripperPose();
        }

        public void ResetDistortedFingerOffsetsForRuntime()
        {
            RefreshExistingReferences();
            if (fingerLeft == null || fingerRight == null)
            {
                return;
            }

            if (fingerLeft.localPosition.sqrMagnitude <= DistortedFingerLocalPositionSqrMagnitude
                && fingerRight.localPosition.sqrMagnitude <= DistortedFingerLocalPositionSqrMagnitude)
            {
                return;
            }

            StopGripperMotion();
            fingerLeft.localPosition = Vector3.zero;
            fingerRight.localPosition = Vector3.zero;
            fingerBaseCaptured = false;
        }

        public string BuildClosureDebugSummary()
        {
            RefreshExistingReferences();
            var hasTarget = TryResolveGripTargetPosition(out var targetPosition, out var targetName);
            var leftDistance = GetFingerTargetDistance(fingerLeft, hasTarget, targetPosition);
            var rightDistance = GetFingerTargetDistance(fingerRight, hasTarget, targetPosition);
            var objectDetected = TryGetGripObjectStopRatio(out var stopRatio);
            var poseOpenRatio = ResolveVisualPoseOpenRatio(gripperOpenRatio);
            return $"target={targetName}; authoredOpenCaptured={fingerBaseCaptured}; authoredClosed={useAuthoredClosedPose}; visualClosedAt={visualClosedAtOpenRatio:0.##}; visualInputOpenRatio={gripperOpenRatio:0.##}; poseOpenRatio={poseOpenRatio:0.##}; objectDetected={objectDetected}; objectStop={stopRatio:0.##}; leftDistance={leftDistance:0.####}; rightDistance={rightDistance:0.####}; leftOpen=({fingerLeftOpen.x:0.####},{fingerLeftOpen.y:0.####},{fingerLeftOpen.z:0.####}); rightOpen=({fingerRightOpen.x:0.####},{fingerRightOpen.y:0.####},{fingerRightOpen.z:0.####}); leftClosed=({fingerLeftClosed.x:0.####},{fingerLeftClosed.y:0.####},{fingerLeftClosed.z:0.####}); rightClosed=({fingerRightClosed.x:0.####},{fingerRightClosed.y:0.####},{fingerRightClosed.z:0.####}); leftCloseTravel=({fingerLeftCloseTravel.x:0.####},{fingerLeftCloseTravel.y:0.####},{fingerLeftCloseTravel.z:0.####}); rightCloseTravel=({fingerRightCloseTravel.x:0.####},{fingerRightCloseTravel.y:0.####},{fingerRightCloseTravel.z:0.####})";
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

        public void RecaptureAuthoredClosedPose()
        {
            StopGripperMotion();
            RefreshExistingReferences();
            if (fingerLeft == null || fingerRight == null)
            {
                return;
            }

            if (!fingerBaseCaptured)
            {
                fingerLeftOpen = Vector3.zero;
                fingerRightOpen = Vector3.zero;
                fingerBaseCaptured = true;
            }

            fingerLeftClosed = fingerLeft.localPosition;
            fingerRightClosed = fingerRight.localPosition;
            useAuthoredClosedPose = true;
            CaptureCloseTravel();
            SetGripperOpenImmediate(0f);
        }

        public void ClearAuthoredClosedPose()
        {
            useAuthoredClosedPose = false;
            CaptureCloseTravel();
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

            fingerLeftOpen = fingerLeft.localPosition;
            fingerRightOpen = fingerRight.localPosition;
            CaptureCloseTravel();
            fingerBaseCaptured = true;
        }

        private void CaptureCloseTravel()
        {
            if (useAuthoredClosedPose)
            {
                fingerLeftCloseTravel = fingerLeftClosed - fingerLeftOpen;
                fingerRightCloseTravel = fingerRightClosed - fingerRightOpen;
                return;
            }

            ResolveCloseTravelFromRenderedCenters(out fingerLeftCloseTravel, out fingerRightCloseTravel);
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

            var closeAmount = 1f - ResolveVisualPoseOpenRatio(gripperOpenRatio);
            fingerLeft.localPosition = fingerLeftOpen + fingerLeftCloseTravel * closeAmount;
            fingerRight.localPosition = fingerRightOpen + fingerRightCloseTravel * closeAmount;
        }

        private float ResolveVisualPoseOpenRatio(float openRatio)
        {
            var clampedRatio = Mathf.Clamp01(openRatio);
            if (visualClosedAtOpenRatio <= 0.0001f)
            {
                return clampedRatio;
            }

            return Mathf.InverseLerp(visualClosedAtOpenRatio, 1f, clampedRatio);
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

        private Transform ResolveGripTargetTransform()
        {
            if (TryResolveGripObject(out var objectRoot, out _))
            {
                return objectRoot;
            }

            if (tcpFrame == null)
            {
                return null;
            }

            return tcpFrame;
        }

        private void RefreshGripObjectStopRatio()
        {
            hasGripObject = false;
            gripObjectStopRatio = 0f;

            if (!TryResolveGripObject(out _, out _))
            {
                return;
            }

            hasGripObject = true;
            gripObjectStopRatio = detectedObjectStopRatio;
        }

        private bool TryResolveGripTargetPosition(out Vector3 targetPosition, out string targetName)
        {
            if (TryResolveGripObject(out var objectRoot, out targetPosition))
            {
                targetName = objectRoot != null ? objectRoot.name : "grip-object";
                return true;
            }

            if (tcpFrame != null)
            {
                targetPosition = tcpFrame.position;
                targetName = tcpFrame.name;
                return true;
            }

            targetPosition = Vector3.zero;
            targetName = "missing";
            return false;
        }

        private bool TryResolveGripObject(out Transform objectRoot, out Vector3 objectCenter)
        {
            objectRoot = null;
            objectCenter = Vector3.zero;
            if (TryResolveExplicitGripObject(out objectRoot, out objectCenter))
            {
                return true;
            }

            if (!TryResolveGripProbe(out var probeCenter, out var probeRadius))
            {
                return false;
            }

            Physics.SyncTransforms();
            var bestDistance = float.PositiveInfinity;
            var found = false;
            var colliders = Physics.OverlapSphere(probeCenter, probeRadius, ~0, QueryTriggerInteraction.Collide);
            for (var i = 0; i < colliders.Length; i++)
            {
                var candidate = colliders[i];
                if (candidate == null || IsOwnedByRobot(candidate.transform))
                {
                    continue;
                }

                if (TryAcceptGripCandidate(candidate.transform, candidate.bounds, probeCenter, probeRadius, ref bestDistance, ref objectRoot, ref objectCenter))
                {
                    found = true;
                }
            }

            var renderers = UnityEngine.Object.FindObjectsByType<Renderer>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            for (var i = 0; i < renderers.Length; i++)
            {
                var candidate = renderers[i];
                if (candidate == null || !candidate.enabled || IsOwnedByRobot(candidate.transform))
                {
                    continue;
                }

                if (TryAcceptGripCandidate(candidate.transform, candidate.bounds, probeCenter, probeRadius, ref bestDistance, ref objectRoot, ref objectCenter))
                {
                    found = true;
                }
            }

            return found;
        }

        private bool TryResolveExplicitGripObject(out Transform objectRoot, out Vector3 objectCenter)
        {
            objectRoot = null;
            objectCenter = Vector3.zero;
            if (gripObjectRoot == null || !gripObjectRoot.gameObject.activeInHierarchy)
            {
                return false;
            }

            objectRoot = gripObjectRoot;
            objectCenter = ResolveRendererCenter(gripObjectRoot);
            return true;
        }

        private bool TryResolveGripProbe(out Vector3 probeCenter, out float probeRadius)
        {
            probeCenter = Vector3.zero;
            probeRadius = 0f;
            if (fingerLeft == null || fingerRight == null)
            {
                return false;
            }

            var leftCenter = ResolveRendererCenter(fingerLeft);
            var rightCenter = ResolveRendererCenter(fingerRight);
            var span = Vector3.Distance(leftCenter, rightCenter);
            if (span < 0.0001f)
            {
                return false;
            }

            probeCenter = (leftCenter + rightCenter) * 0.5f;
            probeRadius = Mathf.Max(gripProbeRadius, span * 0.75f);
            return true;
        }

        private bool TryAcceptGripCandidate(
            Transform candidate,
            Bounds candidateBounds,
            Vector3 probeCenter,
            float probeRadius,
            ref float bestDistance,
            ref Transform objectRoot,
            ref Vector3 objectCenter)
        {
            var closest = candidateBounds.ClosestPoint(probeCenter);
            var distance = Vector3.Distance(closest, probeCenter);
            if (distance > probeRadius || distance >= bestDistance)
            {
                return false;
            }

            bestDistance = distance;
            objectRoot = candidate;
            objectCenter = candidateBounds.center;
            return true;
        }

        private bool IsOwnedByRobot(Transform candidate)
        {
            if (candidate == null)
            {
                return true;
            }

            if (gripObjectRoot != null && (candidate == gripObjectRoot || candidate.IsChildOf(gripObjectRoot)))
            {
                return false;
            }

            if (candidate.name == "TcpMarker")
            {
                return true;
            }

            if (candidate == transform || candidate.IsChildOf(transform))
            {
                return true;
            }

            return transform.root != null && candidate.IsChildOf(transform.root);
        }

        private void ResolveCloseTravelFromRenderedCenters(out Vector3 leftTravel, out Vector3 rightTravel)
        {
            leftTravel = Vector3.zero;
            rightTravel = Vector3.zero;
            if (fingerLeft == null || fingerRight == null || fingerLeft.parent == null || fingerRight.parent == null)
            {
                return;
            }

            var leftCenter = ResolveRendererCenter(fingerLeft);
            var rightCenter = ResolveRendererCenter(fingerRight);
            var spanWorld = rightCenter - leftCenter;
            if (spanWorld.sqrMagnitude < 0.000001f)
            {
                return;
            }

            var axisWorld = spanWorld.normalized;
            var midpointWorld = (leftCenter + rightCenter) * 0.5f;
            var closeCenterWorld = midpointWorld;
            if (TryResolveGripTargetPosition(out var targetPosition, out _))
            {
                var halfSpan = spanWorld.magnitude * 0.5f;
                var offset = Mathf.Clamp(Vector3.Dot(targetPosition - midpointWorld, axisWorld), -halfSpan, halfSpan);
                closeCenterWorld = midpointWorld + axisWorld * offset;
            }

            leftTravel = fingerLeft.parent.InverseTransformVector((closeCenterWorld - leftCenter) * ClosedContactTravelScale);
            rightTravel = fingerRight.parent.InverseTransformVector((closeCenterWorld - rightCenter) * ClosedContactTravelScale);
        }

        private static Vector3 ResolveRendererCenter(Transform root)
        {
            return TryResolveRendererBounds(root, out var bounds) ? bounds.center : root != null ? root.position : Vector3.zero;
        }

        private static bool TryResolveRendererBounds(Transform root, out Bounds bounds)
        {
            var renderers = root != null ? root.GetComponentsInChildren<Renderer>(true) : null;
            if (renderers == null || renderers.Length == 0)
            {
                bounds = new Bounds(root != null ? root.position : Vector3.zero, Vector3.zero);
                return false;
            }

            var found = false;
            bounds = new Bounds(root.position, Vector3.zero);
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

            return found;
        }

        private static float GetFingerTargetDistance(Transform finger, bool hasTarget, Vector3 targetPosition)
        {
            if (finger == null || !hasTarget)
            {
                return -1f;
            }

            return Vector3.Distance(ResolveRendererCenter(finger), targetPosition);
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
