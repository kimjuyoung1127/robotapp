// Folder: App - Application controllers and services; single UnityEngine entry point.
using System.IO;
using UnityEngine;

namespace KineTutor3D.App.Fairino
{
    public sealed partial class RobotControlV3RuntimeController
    {
        public string CaptureStageCameraForDebug(string outputPath, int width = 1280, int height = 720)
        {
            ForceInitialize();
            EnsureEndEffectorAttachment();
            ResetStageCamera();
            if (stageCamera == null)
            {
                return "stageCamera=missing";
            }

            width = Mathf.Clamp(width, 320, 4096);
            height = Mathf.Clamp(height, 240, 4096);
            if (string.IsNullOrWhiteSpace(outputPath))
            {
                outputPath = Path.Combine(Application.dataPath, "..", "Artifacts", "robotcontrolv3-stage-camera.png");
            }

            var fullPath = Path.GetFullPath(outputPath);
            Directory.CreateDirectory(Path.GetDirectoryName(fullPath));
            var previousTarget = stageCamera.targetTexture;
            var previousActive = RenderTexture.active;
            var renderTexture = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32)
            {
                name = "RobotControlV3StageDebugCapture"
            };
            var texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
            try
            {
                stageCamera.targetTexture = renderTexture;
                RenderTexture.active = renderTexture;
                stageCamera.Render();
                texture.ReadPixels(new Rect(0, 0, width, height), 0, 0);
                texture.Apply();
                File.WriteAllBytes(fullPath, texture.EncodeToPNG());
            }
            finally
            {
                stageCamera.targetTexture = previousTarget;
                RenderTexture.active = previousActive;
                if (Application.isPlaying)
                {
                    Destroy(renderTexture);
                    Destroy(texture);
                }
                else
                {
                    DestroyImmediate(renderTexture);
                    DestroyImmediate(texture);
                }
            }

            return $"captured={fullPath}; {GetGripperVisualSummaryForDebug()}";
        }

        public void SetStageTargetTexture(RenderTexture texture)
        {
            if (stageCamera == null)
            {
                return;
            }

            stageCamera.targetTexture = texture;
        }

        public void RefreshStageCameraView()
        {
            if (stageCamera == null)
            {
                return;
            }

            stageCamera.fieldOfView = StageCameraFov;
            if (!stageCameraStateValid)
            {
                ResetStageCamera();
                return;
            }

            ApplyStageCameraState();
        }

        public void ResetStageCamera()
        {
            if (stageCameraPivot == null || stageCamera == null)
            {
                return;
            }

            stageCamera.transform.SetParent(stageCameraPivot, false);
            stageCamera.fieldOfView = StageCameraFov;
            if (TryGetControlBounds(out var bounds))
            {
                var focusPoint = bounds.center + new Vector3(0f, bounds.extents.y * 0.09f, 0f);
                var halfVerticalFovRad = stageCamera.fieldOfView * 0.5f * Mathf.Deg2Rad;
                var halfHorizontalFovRad = Mathf.Atan(Mathf.Tan(halfVerticalFovRad) * stageCamera.aspect);
                var distanceForWidth = bounds.extents.x / Mathf.Max(0.1f, Mathf.Tan(halfHorizontalFovRad));
                var distanceForHeight = bounds.extents.y / Mathf.Max(0.1f, Mathf.Tan(halfVerticalFovRad));
                var radius = bounds.extents.magnitude;
                var minHalfFovRad = Mathf.Min(halfVerticalFovRad, halfHorizontalFovRad);
                var distanceForRadius = radius / Mathf.Max(0.12f, Mathf.Sin(minHalfFovRad));
                var distance = Mathf.Max(distanceForWidth, distanceForHeight, distanceForRadius * 0.72f, bounds.extents.z * 1.28f) * 0.94f;
                var offsetDirection = new Vector3(0.14f, 0.12f, -1f).normalized;
                stageCamera.transform.position = focusPoint + offsetDirection * distance;
                stageCamera.transform.LookAt(focusPoint);
                SetStageCameraStateFromCurrentPose(focusPoint, bounds, distance);
                return;
            }

            var target = FindBaseLink(controlRobotInstance != null ? controlRobotInstance.transform : runtimeRoot) ?? runtimeRoot;
            var targetPosition = target != null ? target.position + new Vector3(0f, 0.8f, 0f) : Vector3.zero;
            stageCamera.transform.position = targetPosition + new Vector3(0f, 1.6f, -4.8f);
            stageCamera.transform.LookAt(targetPosition);
            SetStageCameraStateFromCurrentPose(targetPosition, null, 5.1f);
        }

        public void SetStageCameraPreset(string presetName)
        {
            if (!EnsureStageCameraState())
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(presetName))
            {
                return;
            }

            switch (presetName.Trim().ToUpperInvariant())
            {
                case "FRONT":
                    ApplyStageCameraPreset(0f, 8f, 1.02f);
                    break;
                case "RIGHT":
                    ApplyStageCameraPreset(90f, 8f, 1.02f);
                    break;
                case "TOP":
                    ApplyStageCameraPreset(0f, 88f, 1.12f);
                    break;
                case "ISO":
                    ResetStageCamera();
                    break;
            }
        }

        public void OrbitStageCamera(Vector2 deltaPixels)
        {
            if (!EnsureStageCameraState())
            {
                return;
            }

            stageCameraYaw += deltaPixels.x * StageCameraRotationSpeed;
            stageCameraPitch = Mathf.Clamp(
                stageCameraPitch - (deltaPixels.y * StageCameraRotationSpeed),
                StageCameraMinPitch,
                StageCameraMaxPitch);
            stageCameraUserAdjusted = true;
            ApplyStageCameraState();
        }

        public void PanStageCamera(Vector2 deltaPixels)
        {
            if (!EnsureStageCameraState())
            {
                return;
            }

            var scale = StageCameraPanSpeed * Mathf.Max(stageCameraDistance, 0.01f);
            var cameraTransform = stageCamera.transform;
            stageCameraPanOffset -= cameraTransform.right * (deltaPixels.x * scale);
            stageCameraPanOffset -= cameraTransform.up * (deltaPixels.y * scale);
            stageCameraUserAdjusted = true;
            ApplyStageCameraState();
        }

        public void ZoomStageCamera(float wheelDelta)
        {
            if (!EnsureStageCameraState())
            {
                return;
            }

            var clampedDelta = Mathf.Clamp(wheelDelta, -12f, 12f);
            stageCameraDistance = Mathf.Clamp(
                stageCameraDistance + (clampedDelta * StageCameraZoomSpeed),
                stageCameraMinDistance,
                stageCameraMaxDistance);
            stageCameraUserAdjusted = true;
            ApplyStageCameraState();
        }

        public string SelectRobotPartAtViewport(Vector2 normalizedViewport)
        {
            if (stageCamera == null || controlRobotInstance == null)
            {
                return "camera-or-robot-missing";
            }

            var clampedViewport = new Vector3(
                Mathf.Clamp01(normalizedViewport.x),
                Mathf.Clamp01(normalizedViewport.y),
                0f);
            var ray = stageCamera.ViewportPointToRay(clampedViewport);
            if (Physics.Raycast(ray, out var hit, 20f) && hit.transform != null && hit.transform.IsChildOf(controlRobotInstance.transform))
            {
                var selected = ResolveSelectablePartTransform(hit.collider != null ? hit.collider.transform : hit.transform);
                partSelectionGizmo?.Select(selected);
                selectedLinkHighlighter?.Select(selected);
                lastSelectedPartName = selected.name;
                PushFeedback($"[Select] {lastSelectedPartName} 선택");
                RefreshSnapshot();
                return lastSelectedPartName;
            }

            var fallbackTarget = FindRendererHit(ray);
            if (fallbackTarget != null)
            {
                var selected = ResolveSelectablePartTransform(fallbackTarget);
                partSelectionGizmo?.Select(selected);
                selectedLinkHighlighter?.Select(selected);
                lastSelectedPartName = selected.name;
                PushFeedback($"[Select] {lastSelectedPartName} 선택");
                RefreshSnapshot();
                return lastSelectedPartName;
            }

            partSelectionGizmo?.Clear();
            selectedLinkHighlighter?.Clear();
            lastSelectedPartName = "없음";
            PushFeedback("[Select] 선택 해제");
            RefreshSnapshot();
            return lastSelectedPartName;
        }

        private bool EnsureStageCameraState()
        {
            if (stageCamera == null)
            {
                return false;
            }

            if (stageCameraStateValid)
            {
                return true;
            }

            ResetStageCamera();
            return stageCameraStateValid;
        }

        private void ApplyStageCameraState()
        {
            if (stageCamera == null || !stageCameraStateValid)
            {
                return;
            }

            var rotation = Quaternion.Euler(stageCameraPitch, stageCameraYaw, 0f);
            var focusPoint = stageCameraFocusPoint + stageCameraPanOffset;
            stageCamera.transform.position = focusPoint + (rotation * new Vector3(0f, 0f, -stageCameraDistance));
            stageCamera.transform.LookAt(focusPoint);
        }

        private void ApplyStageCameraPreset(float yawDeg, float pitchDeg, float distanceMultiplier)
        {
            stageCameraYaw = yawDeg;
            stageCameraPitch = Mathf.Clamp(pitchDeg, StageCameraMinPitch, StageCameraMaxPitch);
            stageCameraDistance = Mathf.Clamp(
                stageCameraDistance * Mathf.Max(0.85f, distanceMultiplier),
                stageCameraMinDistance,
                stageCameraMaxDistance);
            stageCameraUserAdjusted = true;
            ApplyStageCameraState();
        }

        private void SetStageCameraStateFromCurrentPose(Vector3 focusPoint, Bounds? bounds, float fallbackDistance)
        {
            if (stageCamera == null)
            {
                stageCameraStateValid = false;
                return;
            }

            var offset = stageCamera.transform.position - focusPoint;
            var distance = offset.magnitude > 0.01f ? offset.magnitude : Mathf.Max(0.35f, fallbackDistance);
            var lookDirection = offset.sqrMagnitude > 0.0001f
                ? (-offset).normalized
                : stageCamera.transform.forward.normalized;
            stageCameraFocusPoint = focusPoint;
            stageCameraPanOffset = Vector3.zero;
            stageCameraDistance = Mathf.Max(0.01f, distance);
            if (bounds.HasValue)
            {
                var extentsMagnitude = bounds.Value.extents.magnitude;
                stageCameraMinDistance = Mathf.Max(0.25f, extentsMagnitude * 0.22f);
                stageCameraMaxDistance = Mathf.Max(stageCameraMinDistance + 1f, stageCameraDistance * 3.2f, extentsMagnitude * 4.5f);
            }
            else
            {
                stageCameraMinDistance = 0.35f;
                stageCameraMaxDistance = Mathf.Max(6f, stageCameraDistance * 2.5f);
            }

            stageCameraDistance = Mathf.Clamp(stageCameraDistance, stageCameraMinDistance, stageCameraMaxDistance);
            stageCameraYaw = Mathf.Atan2(lookDirection.x, lookDirection.z) * Mathf.Rad2Deg;
            stageCameraPitch = Mathf.Clamp(
                -Mathf.Asin(Mathf.Clamp(lookDirection.y, -1f, 1f)) * Mathf.Rad2Deg,
                StageCameraMinPitch,
                StageCameraMaxPitch);
            stageCameraStateValid = true;
            stageCameraUserAdjusted = false;
        }

        private Transform FindRendererHit(Ray ray)
        {
            if (controlRobotInstance == null)
            {
                return null;
            }

            var renderers = controlRobotInstance.GetComponentsInChildren<Renderer>(true);
            Transform bestTarget = null;
            var bestDistance = float.MaxValue;
            for (var index = 0; index < renderers.Length; index++)
            {
                var renderer = renderers[index];
                if (renderer == null || !renderer.bounds.IntersectRay(ray, out var distance) || distance >= bestDistance)
                {
                    continue;
                }

                bestDistance = distance;
                bestTarget = renderer.transform;
            }

            return bestTarget;
        }

        private bool TryGetControlBounds(out Bounds bounds)
        {
            bounds = default;
            if (controlRobotInstance == null)
            {
                return false;
            }

            var renderers = controlRobotInstance.GetComponentsInChildren<Renderer>(true);
            if (renderers == null || renderers.Length == 0)
            {
                return false;
            }

            var found = false;
            for (var i = 0; i < renderers.Length; i++)
            {
                if (renderers[i] == null)
                {
                    continue;
                }

                if (!found)
                {
                    bounds = renderers[i].bounds;
                    found = true;
                    continue;
                }

                bounds.Encapsulate(renderers[i].bounds);
            }

            return found && bounds.size.sqrMagnitude > 0.0001f;
        }
    }
}
