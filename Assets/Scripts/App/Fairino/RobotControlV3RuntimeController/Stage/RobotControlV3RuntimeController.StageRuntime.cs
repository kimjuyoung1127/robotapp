// Folder: Stage - robot stage runtime, attachment visuals, and render-surface helpers for V3.
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using KineTutor3D.Math;
using KineTutor3D.UI.RobotControlV3;
using KineTutor3D.Visualization;
using UnityEngine;

namespace KineTutor3D.App.Fairino
{
    // Handles runtime root/control robot setup, gripper attachment visuals, gizmos, and stage render helpers.
    // Camera-specific controls stay in StageCamera and panel command entry points stay outside this partial.
    public sealed partial class RobotControlV3RuntimeController
    {
        private void EnsureRobotSelection()
        {
            RobotSelectionBridge.SetSelection(templateDefinition.RobotId, RobotSelectionBridge.RobotControlMode);
        }


        private void EnsureRuntimeRoot()
        {
            runtimeRoot = GameObject.Find(templateDefinition.RuntimeRootName)?.transform;
            if (runtimeRoot == null)
            {
                runtimeRoot = new GameObject(templateDefinition.RuntimeRootName).transform;
                runtimeRoot.position = new Vector3(0f, -1000f, 0f);
            }
        }


        private void EnsureControlRobot()
        {
            var existing = runtimeRoot.Find(templateDefinition.ControlRobotInstanceName);
            if (existing != null)
            {
                controlRobotInstance = existing.gameObject;
                StabilizeControlRobot(controlRobotInstance);
                EnsureEndEffectorAttachment();
                return;
            }

            var prefab = Resources.Load<GameObject>(templateDefinition.ControlPrefabResourcePath)
                ?? Resources.Load<GameObject>(templateDefinition.ShowroomPrefabResourcePath);
            if (prefab == null)
            {
                controlRobotInstance = new GameObject(templateDefinition.ControlRobotInstanceName);
                controlRobotInstance.transform.SetParent(runtimeRoot, false);
                return;
            }

            controlRobotInstance = Instantiate(prefab, runtimeRoot);
            controlRobotInstance.name = templateDefinition.ControlRobotInstanceName;
            controlRobotInstance.transform.localPosition = Vector3.zero;
            controlRobotInstance.transform.localRotation = Quaternion.identity;
            RepairVisualMeshes(controlRobotInstance);
            StabilizeControlRobot(controlRobotInstance);
            EnsureEndEffectorAttachment();
        }


        private void EnsureJointDriver()
        {
            if (controlRobotInstance == null)
            {
                return;
            }

            jointDriver = EnsureComponent<FairinoUrdfJointDriver>(controlRobotInstance);
            var baseLink = FindBaseLink(controlRobotInstance.transform);
            if (baseLink != null)
            {
                jointDriver.Inject(baseLink);
            }
        }


        private void EnsureVisualizationHelpers()
        {
            var gizmoHost = runtimeRoot.Find("FrameGizmos") ?? new GameObject("FrameGizmos").transform;
            gizmoHost.SetParent(runtimeRoot, false);
            frameGizmoFactory = EnsureComponent<FrameGizmoFactory>(gizmoHost.gameObject);
            frameGizmoFactory.SetVisible(false);

            var baseFrameHost = runtimeRoot.Find("BaseFrameGizmo") ?? new GameObject("BaseFrameGizmo").transform;
            baseFrameHost.SetParent(runtimeRoot, false);
            baseFrameGizmo = EnsureComponent<FrameGizmo>(baseFrameHost.gameObject);
            baseFrameGizmo.SetLength(0.12f);
            baseFrameGizmo.SetVisible(false);

            var toolFrameHost = runtimeRoot.Find("ToolFrameGizmo") ?? new GameObject("ToolFrameGizmo").transform;
            toolFrameHost.SetParent(runtimeRoot, false);
            toolFrameGizmo = EnsureComponent<FrameGizmo>(toolFrameHost.gameObject);
            toolFrameGizmo.SetLength(0.09f);
            toolFrameGizmo.SetVisible(false);

            var trailHost = runtimeRoot.Find("EETrail") ?? new GameObject("EETrail").transform;
            trailHost.SetParent(runtimeRoot, false);
            eeTrailRenderer = EnsureComponent<EETrailRenderer>(trailHost.gameObject);

            var arrowHost = runtimeRoot.Find("DisplacementArrow") ?? new GameObject("DisplacementArrow").transform;
            arrowHost.SetParent(runtimeRoot, false);
            displacementArrow = EnsureComponent<DisplacementArrow>(arrowHost.gameObject);

            targetMarkerVisual = EnsureComponent<TargetMarkerVisual>(runtimeRoot.gameObject);
            targetMarkerVisual.SetMarkersVisible(false);

            var gridHost = runtimeRoot.Find("StageFloorGrid") ?? new GameObject("StageFloorGrid").transform;
            gridHost.SetParent(runtimeRoot, false);
            stageFloorGrid = EnsureComponent<RobotStageFloorGrid>(gridHost.gameObject);
            stageFloorGrid.SetVisible(true);

            var selectionHost = runtimeRoot.Find("PartSelectionGizmo") ?? new GameObject("PartSelectionGizmo").transform;
            selectionHost.SetParent(runtimeRoot, false);
            partSelectionGizmo = EnsureComponent<RobotPartSelectionGizmo>(selectionHost.gameObject);
            partSelectionGizmo.SetAxisLength(0.08f);
            partSelectionGizmo.Clear();

            var ghostHost = runtimeRoot.Find("GhostRobotVisual") ?? new GameObject("GhostRobotVisual").transform;
            ghostHost.SetParent(runtimeRoot, false);
            ghostRobotVisual = EnsureComponent<GhostRobotVisual>(ghostHost.gameObject);
            ghostRobotVisual.EnsureGhost(controlRobotInstance, templateDefinition.BaseLinkName);

            var pathHost = runtimeRoot.Find("PredictedPath") ?? new GameObject("PredictedPath").transform;
            pathHost.SetParent(runtimeRoot, false);
            predictedPathRenderer = EnsureComponent<PredictedPathRenderer>(pathHost.gameObject);
            predictedPathRenderer.ClearPath();

            selectedLinkHighlighter = EnsureComponent<SelectedLinkHighlighter>(controlRobotInstance);
            EnsureJointHighlightRings();

            EnsureEndEffectorAttachment();
        }


        private void EnsureEndEffectorAttachment()
        {
            if (controlRobotInstance == null)
            {
                return;
            }

            endEffectorAttachment = controlRobotInstance.GetComponentInChildren<FR5EndEffectorAttachment>(true);
            if (endEffectorAttachment != null)
            {
                endEffectorAttachment.RemoveLegacyGripMarkers();
                peripheralFacade?.SetGripperVisualAttached(true);
                ResetStageCameraIfAutomatic();
                return;
            }

            var wrist = FindChildRecursive(controlRobotInstance.transform, "wrist3_link");
            if (wrist == null)
            {
                peripheralFacade?.SetGripperVisualAttached(false);
                return;
            }

            var toolMount = wrist.Find("ToolMount");
            if (toolMount == null)
            {
                toolMount = new GameObject("ToolMount").transform;
                toolMount.SetParent(wrist, false);
                toolMount.localPosition = Vector3.zero;
                toolMount.localRotation = Quaternion.identity;
                toolMount.localScale = Vector3.one;
            }

            var existing = toolMount.Find(PgeaAttachmentId);
            if (existing != null)
            {
                endEffectorAttachment = existing.GetComponent<FR5EndEffectorAttachment>()
                    ?? existing.gameObject.AddComponent<FR5EndEffectorAttachment>();
                ConfigureEndEffectorAttachment(existing);
                peripheralFacade?.SetGripperVisualAttached(true);
                ResetStageCameraIfAutomatic();
                return;
            }

            var prefab = Resources.Load<GameObject>(PgeaAttachmentResourcePath);
            if (prefab == null)
            {
                peripheralFacade?.SetGripperVisualAttached(false);
                return;
            }

            var instance = Instantiate(prefab, toolMount);
            instance.name = PgeaAttachmentId;
            ConfigureEndEffectorAttachment(instance.transform);
            peripheralFacade?.SetGripperVisualAttached(true);
            ResetStageCameraIfAutomatic();
        }


        private void ConfigureEndEffectorAttachment(Transform attachmentRoot)
        {
            attachmentRoot.localPosition = PgeaAttachmentLocalPosition;
            attachmentRoot.localRotation = PgeaAttachmentLocalRotation;
            attachmentRoot.localScale = Vector3.one;

            var visualRoot = attachmentRoot.Find("VisualRoot");
            var tcpFrame = attachmentRoot.Find("TcpFrame");
            var model = visualRoot != null ? visualRoot.Find("PGEA-100-40_Model") : null;
            var fingerLeft = model != null ? model.Find("finger_left") : null;
            var fingerRight = model != null ? model.Find("finger_right") : null;
            if (tcpFrame != null)
            {
                tcpFrame.localPosition = PgeaTcpLocalPosition;
                tcpFrame.localRotation = Quaternion.identity;
            }

            if (model != null)
            {
                var modelLocal = model.localPosition;
                modelLocal.z = PgeaModelLocalZ;
                model.localPosition = modelLocal;
            }

            endEffectorAttachment = attachmentRoot.GetComponent<FR5EndEffectorAttachment>()
                ?? attachmentRoot.gameObject.AddComponent<FR5EndEffectorAttachment>();
            endEffectorAttachment.Configure(PgeaAttachmentId, visualRoot, tcpFrame);
            endEffectorAttachment.ResetDistortedFingerOffsetsForRuntime();
            endEffectorAttachment.SetFingers(fingerLeft, fingerRight);
            endEffectorAttachment.SetGripperOpen(peripheralFacade?.Snapshot.GripperOpenRatio ?? 1f);
        }


        private void ApplyGripperVisual(float openRatio)
        {
            if (endEffectorAttachment == null && controlRobotInstance != null)
            {
                endEffectorAttachment = controlRobotInstance.GetComponentInChildren<FR5EndEffectorAttachment>(true);
            }

            if (endEffectorAttachment != null)
            {
                endEffectorAttachment.SetGripperOpen(openRatio);
                ResetStageCameraIfAutomatic();
            }

            peripheralFacade?.SetGripperVisualAttached(endEffectorAttachment != null);
        }


        private bool TryResolveGripperObjectStopPercent(out int stopPercent)
        {
            stopPercent = 0;
            if (endEffectorAttachment == null && controlRobotInstance != null)
            {
                endEffectorAttachment = controlRobotInstance.GetComponentInChildren<FR5EndEffectorAttachment>(true);
            }

            if (endEffectorAttachment == null || !endEffectorAttachment.TryGetGripObjectStopRatio(out var stopRatio))
            {
                return false;
            }

            stopPercent = ClampPercent(Mathf.RoundToInt(stopRatio * 100f));
            return stopPercent > 0;
        }


        private static int ClampPercent(int value)
        {
            return value < 0 ? 0 : value > 100 ? 100 : value;
        }


        private static float ClampPercent(float value)
        {
            return value < 0f ? 0f : value > 100f ? 100f : value;
        }


        private void ResetStageCameraIfAutomatic()
        {
            if (!stageCameraUserAdjusted || !stageCameraStateValid)
            {
                ResetStageCamera();
            }
        }


        private void EnsureJointHighlightRings()
        {
            if (runtimeRoot == null)
            {
                return;
            }

            var host = runtimeRoot.Find("JointHighlightRings") ?? new GameObject("JointHighlightRings").transform;
            host.SetParent(runtimeRoot, false);
            var colors = new[]
            {
                new Color(0.95f, 0.77f, 0.15f, 1f),
                new Color(0.29f, 0.56f, 0.85f, 1f),
                new Color(0.71f, 0.54f, 0.93f, 1f),
                new Color(0.36f, 0.86f, 0.72f, 1f),
                new Color(0.99f, 0.63f, 0.18f, 1f),
                new Color(0.90f, 0.35f, 0.42f, 1f)
            };
            var radii = new[] { 0.24f, 0.18f, 0.16f, 0.14f, 0.12f, 0.10f };

            var jointCount = templateDefinition != null ? templateDefinition.JointCount : 6;
            jointHighlightRings ??= new JointHighlightRing[jointCount];
            for (var index = 0; index < jointHighlightRings.Length; index++)
            {
                var ringTransform = host.Find($"JointHighlightRing_{index}") ?? new GameObject($"JointHighlightRing_{index}").transform;
                ringTransform.SetParent(host, false);
                var ring = EnsureComponent<JointHighlightRing>(ringTransform.gameObject);
                var jointTransform = jointDriver != null ? jointDriver.GetJointTransform(index) : null;
                if (jointTransform != null)
                {
                    ring.Bind(jointTransform, radii[Mathf.Min(index, radii.Length - 1)], colors[Mathf.Min(index, colors.Length - 1)]);
                }

                ring.SetVisible(false);
                jointHighlightRings[index] = ring;
            }
        }


        private void ApplyBaseAndToolFrameState()
        {
            frameGizmoFactory?.SetVisible(false);
            ApplyFrameGizmo(baseFrameGizmo, ResolveBaseFrameTransform(), showBaseFrame);
            ApplyFrameGizmo(toolFrameGizmo, ResolveToolFrameTransform(), showToolFrame);
        }


        private void ApplyFrameGizmo(FrameGizmo gizmo, Transform target, bool visible)
        {
            if (gizmo == null)
            {
                return;
            }

            var shouldShow = visible && target != null;
            if (shouldShow)
            {
                gizmo.transform.position = target.position;
                gizmo.transform.rotation = target.rotation;
                gizmo.transform.localScale = Vector3.one;
            }

            gizmo.SetVisible(shouldShow);
        }


        private Transform ResolveBaseFrameTransform()
        {
            return FindBaseLink(controlRobotInstance != null ? controlRobotInstance.transform : runtimeRoot)
                ?? controlRobotInstance?.transform
                ?? runtimeRoot;
        }


        private Transform ResolveToolFrameTransform()
        {
            if (endEffectorAttachment?.TcpFrame != null)
            {
                return endEffectorAttachment.TcpFrame;
            }

            var toolMount = controlRobotInstance != null ? FindChildRecursive(controlRobotInstance.transform, "ToolMount") : null;
            if (toolMount != null)
            {
                return toolMount;
            }

            var jointIndex = templateDefinition != null ? templateDefinition.JointCount - 1 : 5;
            return jointDriver?.GetJointTransform(jointIndex);
        }


        private void ApplyJointHighlightState()
        {
            if (jointHighlightRings == null)
            {
                return;
            }

            for (var index = 0; index < jointHighlightRings.Length; index++)
            {
                jointHighlightRings[index]?.SetVisible(index == activeJointHighlightIndex);
            }
        }


        private Transform ResolveSelectablePartTransform(Transform target)
        {
            if (target == null)
            {
                return null;
            }

            var gripperPart = ResolveGripperSelectablePart(target);
            if (gripperPart != null)
            {
                return gripperPart;
            }

            var current = target;
            while (current != null && current != controlRobotInstance?.transform)
            {
                if (IsSelectableLinkTransform(current))
                {
                    return current;
                }

                current = current.parent;
            }

            return target;
        }


        private Transform ResolveGripperSelectablePart(Transform target)
        {
            if (target == null || endEffectorAttachment == null)
            {
                return null;
            }

            if (endEffectorAttachment.FingerLeft != null && target.IsChildOf(endEffectorAttachment.FingerLeft))
            {
                return endEffectorAttachment.FingerLeft;
            }

            if (endEffectorAttachment.FingerRight != null && target.IsChildOf(endEffectorAttachment.FingerRight))
            {
                return endEffectorAttachment.FingerRight;
            }

            if (endEffectorAttachment.ModelRoot != null && target.IsChildOf(endEffectorAttachment.ModelRoot))
            {
                return endEffectorAttachment.ModelRoot;
            }

            return null;
        }


        private static bool IsSelectableLinkTransform(Transform target)
        {
            if (target == null)
            {
                return false;
            }

            var name = target.name;
            return name.IndexOf("link", StringComparison.OrdinalIgnoreCase) >= 0
                || name.IndexOf("tcp", StringComparison.OrdinalIgnoreCase) >= 0
                || name.IndexOf("tool", StringComparison.OrdinalIgnoreCase) >= 0
                || name.IndexOf("gripper", StringComparison.OrdinalIgnoreCase) >= 0;
        }


        private void EnsureStageCameraRig()
        {
            stageCameraPivot = runtimeRoot.Find("V3StageCameraPivot");
            if (stageCameraPivot == null)
            {
                stageCameraPivot = new GameObject("V3StageCameraPivot").transform;
                stageCameraPivot.SetParent(runtimeRoot, false);
            }

            var mainCameraObject = GameObject.Find("Main Camera");
            if (mainCameraObject == null)
            {
                mainCameraObject = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
            }

            var mainCamera = mainCameraObject.GetComponent<Camera>();
            if (mainCamera == null)
            {
                mainCamera = mainCameraObject.AddComponent<Camera>();
            }

            mainCameraObject.tag = "MainCamera";
            mainCamera.targetTexture = null;
            mainCamera.enabled = true;

            var stageCameraTransform = stageCameraPivot.Find("V3StageCamera");
            var existingStageCamera = stageCameraTransform != null ? stageCameraTransform.GetComponent<Camera>() : null;
            if (stageCameraTransform != null && existingStageCamera == null)
            {
                DestroyCameraRigChild(stageCameraTransform.gameObject);
                stageCameraTransform = null;
            }

            if (stageCameraTransform == null)
            {
                stageCameraTransform = new GameObject("V3StageCamera", typeof(Camera)).transform;
                stageCameraTransform.SetParent(stageCameraPivot, false);
            }

            stageCamera = stageCameraTransform.GetComponent<Camera>();
            if (stageCamera == null)
            {
                stageCamera = stageCameraTransform.gameObject.AddComponent<Camera>();
            }

            stageCamera.transform.SetParent(stageCameraPivot, false);
            stageCamera.clearFlags = CameraClearFlags.SolidColor;
            stageCamera.backgroundColor = new Color(0.12f, 0.12f, 0.16f, 1f);
            stageCamera.nearClipPlane = 0.01f;
            stageCamera.farClipPlane = 50f;
            stageCamera.allowHDR = false;
            stageCamera.allowMSAA = true;
            stageCamera.targetDisplay = 0;
            stageCamera.enabled = true;
            ResetStageCamera();

            var lightTransform = runtimeRoot.Find("V3StageLight");
            var existingLight = lightTransform != null ? lightTransform.GetComponent<Light>() : null;
            if (lightTransform != null && existingLight == null)
            {
                DestroyCameraRigChild(lightTransform.gameObject);
                lightTransform = null;
            }

            if (lightTransform == null)
            {
                lightTransform = new GameObject("V3StageLight", typeof(Light)).transform;
                lightTransform.SetParent(runtimeRoot, false);
            }

            stageLight = lightTransform.GetComponent<Light>();
            if (stageLight == null)
            {
                stageLight = lightTransform.gameObject.AddComponent<Light>();
            }

            stageLight.type = LightType.Directional;
            stageLight.intensity = 1.25f;
            lightTransform.rotation = Quaternion.Euler(36f, -35f, 0f);
        }


        private static void DestroyCameraRigChild(GameObject target)
        {
            if (target == null)
            {
                return;
            }

            if (Application.isPlaying)
            {
                Destroy(target);
                return;
            }

            DestroyImmediate(target);
        }


        private static T EnsureComponent<T>(GameObject target) where T : Component
        {
            if (target == null)
            {
                return null;
            }

            var component = target.GetComponent<T>();
            if (component == null)
            {
                component = target.AddComponent<T>();
            }

            return component;
        }
    }
}
