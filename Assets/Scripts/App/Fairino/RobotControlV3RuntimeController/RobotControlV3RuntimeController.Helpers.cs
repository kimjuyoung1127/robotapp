// Folder: App - Application controllers and services; single UnityEngine entry point.
using System;
using System.Collections.Generic;
using KineTutor3D.Math;
using KineTutor3D.UI.RobotControlV3;
using KineTutor3D.Visualization;
using UnityEngine;

namespace KineTutor3D.App.Fairino
{
    public sealed partial class RobotControlV3RuntimeController
    {
        private bool IsReadbackOnlyLiveClient()
        {
            return IsReadbackOnlyLiveClient(connectionService);
        }

        private bool ShouldUseLiveGripperOperatorPath()
        {
            return hasCurrentPositionReadComplete
                && connectionService != null
                && !connectionService.IsMockMode
                && connectionService.Client.IsConnected
                && connectionService.Client is IFairinoLiveClientDiagnostics { IsReadbackOnly: false };
        }

        private bool ShouldUseLiveMoveJOperatorPath()
        {
            return hasCurrentPositionReadComplete
                && connectionService != null
                && !connectionService.IsMockMode
                && connectionService.Client.IsConnected
                && connectionService.Client is IFairinoLiveClientDiagnostics { IsReadbackOnly: false }
                && HasDedicatedTinyMoveJLivePathConfigured();
        }

        private bool ShouldUseSavedPointMoveJOperatorPath()
        {
            return connectionService != null
                && !connectionService.IsMockMode
                && connectionService.Client.IsConnected
                && (hasCurrentPositionReadComplete
                    || snapshot.CurrentPositionReadComplete
                    || HasStableLiveEvidenceForDebug());
        }

        private static bool IsReadbackOnlyLiveClient(FairinoConnectionService service)
        {
            return service != null
                && !service.IsMockMode
                && service.Client is IFairinoLiveClientDiagnostics { IsReadbackOnly: true };
        }

        private void PushFeedback(string message)
        {
            snapshot.LastFeedback = string.IsNullOrWhiteSpace(message) ? "..." : message;
        }

        private void RememberOperatorBlockedReason(string message)
        {
            retainedOperatorBlockedReason = string.IsNullOrWhiteSpace(message) ? string.Empty : message;
            retainedOperatorFailureCategory = ClassifyFailureCategory(retainedOperatorBlockedReason, snapshot.LastFeedback ?? string.Empty);
            retainedOperatorNextAction = BuildOperatorNextAction(retainedOperatorFailureCategory, snapshot.MotionGateNextStep);
            snapshot.LiveBlockedReason = retainedOperatorBlockedReason;
        }

        private void ClearRememberedOperatorBlockedReason()
        {
            retainedOperatorBlockedReason = string.Empty;
            retainedOperatorFailureCategory = "ready";
            retainedOperatorNextAction = string.Empty;
            snapshot.LiveBlockedReason = string.Empty;
        }

        private string ResolveEffectiveOperatorBlockedReason()
        {
            return !string.IsNullOrWhiteSpace(retainedOperatorBlockedReason)
                ? retainedOperatorBlockedReason
                : snapshot.LiveBlockedReason ?? string.Empty;
        }

        private void ApplyRetainedOperatorBlockedReasonToSnapshot()
        {
            if (!string.IsNullOrWhiteSpace(retainedOperatorBlockedReason))
            {
                snapshot.LiveBlockedReason = retainedOperatorBlockedReason;
                snapshot.FailureCategory = retainedOperatorFailureCategory;
                snapshot.OperatorNextAction = retainedOperatorNextAction;
            }
        }

        private void ResetLiveSessionModeAfterLiveAttempt(LiveCommandKind kind, FairinoResult result)
        {
            if (snapshot.DryRunEnabled || kind == LiveCommandKind.ReadbackOnly || currentLiveSessionMode == LiveCommandSessionMode.LiveControl)
            {
                return;
            }

            currentLiveSessionMode = LiveCommandSessionMode.LiveControl;
            InvalidateLiveApprovalContext();
            if (result.IsSuccess)
            {
                ClearRememberedOperatorBlockedReason();
                PushFeedback($"{result.Message} · 세션을 통합 live 제어 상태로 유지한다.");
            }
        }

        private int ResolveRequestedSpeedPercent()
        {
            var shellState = GetComponent<PendantV3ShellStateController>();
            return shellState != null
                ? Mathf.Clamp(shellState.GetStateSnapshot().SpeedPercent, 1, 100)
                : 30;
        }

        private void RecordUndo(double[] jointAnglesDeg)
        {
            if (jointAnglesDeg == null)
            {
                return;
            }

            undoJointHistory.Push(CopyJointArray(jointAnglesDeg));
            redoJointHistory.Clear();
        }

        private double[] ComputeDisplayedTcpPose()
        {
            if (previewTcpPose != null && !previewUsesJointPose)
            {
                return CopyPoseArray(previewTcpPose);
            }

            if (previewUsesJointPose && previewJointAnglesDeg != null)
            {
                return ComputeTcpPoseFromJoints(previewJointAnglesDeg);
            }

            return currentState.TcpPose != null && currentState.TcpPose.Length >= 6
                ? CopyPoseArray(currentState.TcpPose)
                : ComputeTcpPoseFromJoints(currentState.JointPosDeg);
        }

        private double[] ComputeTcpPoseFromJoints(double[] jointAnglesDeg)
        {
            if (kinematicsFacade == null || jointAnglesDeg == null)
            {
                return new double[6];
            }

            kinematicsFacade.SetJointAnglesDegrees(jointAnglesDeg);
            var ee = kinematicsFacade.EndEffectorTransform;
            var position = ee.ExtractPosition();
            return new[]
            {
                position.X * 1000.0,
                position.Y * 1000.0,
                position.Z * 1000.0,
                180.0,
                0.0,
                90.0,
            };
        }

        private FairinoResult TrySolvePointMoveJoints(double[] targetTcpPose, out double[] jointTarget)
        {
            jointTarget = null;
            if (targetTcpPose == null || targetTcpPose.Length < 6)
            {
                return FairinoResult.Fail(-34, "Point MoveJ 대상 TCP 값이 부족하다.");
            }

            if (previewKinematicsFacade == null)
            {
                return FairinoResult.Fail(-35, "Point MoveJ IK 계산기가 아직 준비되지 않았다.");
            }

            var targetPosition = new Vec3D(
                targetTcpPose[0] / 1000.0,
                targetTcpPose[1] / 1000.0,
                targetTcpPose[2] / 1000.0);
            var seed = previewUsesJointPose && previewJointAnglesDeg != null
                ? previewJointAnglesDeg
                : currentState.JointPosDeg;
            var candidate = ClampJointTarget(CopyJointArray(seed));
            var bestErrorMm = ComputePreviewPositionErrorMm(candidate, targetPosition);
            var stepDeg = 12.0;

            for (var iteration = 0; iteration < 72 && bestErrorMm > 2.0; iteration++)
            {
                var improved = false;
                for (var jointIndex = 0; jointIndex < templateDefinition.JointCount; jointIndex++)
                {
                    improved |= TryImproveJoint(candidate, jointIndex, stepDeg, targetPosition, ref bestErrorMm);
                }

                if (!improved)
                {
                    stepDeg *= 0.55;
                    if (stepDeg < 0.05)
                    {
                        break;
                    }
                }
            }

            if (bestErrorMm > 8.0)
            {
                return FairinoResult.Fail(-36, $"Point MoveJ IK 실패 · 위치 오차 {bestErrorMm:0.0}mm");
            }

            jointTarget = candidate;
            return FairinoResult.Ok($"Point MoveJ IK 완료 · 위치 오차 {bestErrorMm:0.0}mm");
        }

        private bool TryImproveJoint(double[] candidate, int jointIndex, double stepDeg, Vec3D targetPosition, ref double bestErrorMm)
        {
            var original = candidate[jointIndex];
            var bestValue = original;
            var improved = false;

            for (var direction = -1; direction <= 1; direction += 2)
            {
                candidate[jointIndex] = ClampJointValue(jointIndex, original + (direction * stepDeg));
                var errorMm = ComputePreviewPositionErrorMm(candidate, targetPosition);
                if (errorMm + 0.001 < bestErrorMm)
                {
                    bestErrorMm = errorMm;
                    bestValue = candidate[jointIndex];
                    improved = true;
                }
            }

            candidate[jointIndex] = bestValue;
            return improved;
        }

        private double ComputePreviewPositionErrorMm(double[] jointsDeg, Vec3D targetPosition)
        {
            previewKinematicsFacade.SetJointAnglesDegrees(jointsDeg);
            var position = previewKinematicsFacade.EndEffectorTransform.ExtractPosition();
            return (position - targetPosition).Magnitude() * 1000.0;
        }

        private double[] ClampJointTarget(double[] jointsDeg)
        {
            var result = jointsDeg ?? new double[templateDefinition.JointCount];
            for (var index = 0; index < templateDefinition.JointCount && index < result.Length; index++)
            {
                result[index] = ClampJointValue(index, result[index]);
            }

            return result;
        }

        private double ClampJointValue(int jointIndex, double value)
        {
            if (config?.jointLimits != null && jointIndex < config.jointLimits.Length && config.jointLimits[jointIndex] != null)
            {
                var limit = config.jointLimits[jointIndex];
                return System.Math.Max(limit.minDeg, System.Math.Min(limit.maxDeg, value));
            }

            return System.Math.Max(-360.0, System.Math.Min(360.0, value));
        }

        private List<Vector3> BuildJointPreviewPath(double[] startJointAnglesDeg, double[] endJointAnglesDeg)
        {
            var result = new List<Vector3>();
            if (previewKinematicsFacade == null || startJointAnglesDeg == null || endJointAnglesDeg == null)
            {
                return result;
            }

            const int sampleCount = 24;
            var lerped = new double[templateDefinition.JointCount];
            for (var sampleIndex = 0; sampleIndex <= sampleCount; sampleIndex++)
            {
                var t = sampleIndex / (double)sampleCount;
                for (var jointIndex = 0; jointIndex < templateDefinition.JointCount; jointIndex++)
                {
                    var start = jointIndex < startJointAnglesDeg.Length ? startJointAnglesDeg[jointIndex] : 0d;
                    var end = jointIndex < endJointAnglesDeg.Length ? endJointAnglesDeg[jointIndex] : start;
                    lerped[jointIndex] = Mathf.Lerp((float)start, (float)end, (float)t);
                }

                previewKinematicsFacade.SetJointAnglesDegrees(lerped);
                result.Add(CoordConverter.ToUnityPosition(previewKinematicsFacade.EndEffectorTransform.ExtractPosition()));
            }

            return result;
        }

        private List<Vector3> BuildCartesianPreviewPath(double[] startTcpPose, double[] endTcpPose)
        {
            var result = new List<Vector3>();
            if (startTcpPose == null || endTcpPose == null || startTcpPose.Length < 3 || endTcpPose.Length < 3)
            {
                return result;
            }

            const int sampleCount = 18;
            var start = new Vec3D(startTcpPose[0] / 1000.0, startTcpPose[1] / 1000.0, startTcpPose[2] / 1000.0);
            var end = new Vec3D(endTcpPose[0] / 1000.0, endTcpPose[1] / 1000.0, endTcpPose[2] / 1000.0);
            for (var sampleIndex = 0; sampleIndex <= sampleCount; sampleIndex++)
            {
                var t = sampleIndex / (double)sampleCount;
                var point = new Vec3D(
                    start.X + ((end.X - start.X) * t),
                    start.Y + ((end.Y - start.Y) * t),
                    start.Z + ((end.Z - start.Z) * t));
                result.Add(CoordConverter.ToUnityPosition(point));
            }

            return result;
        }

        private void ApplySelectedPartSnapshot()
        {
            var selectedTarget = partSelectionGizmo != null ? partSelectionGizmo.SelectedTarget : null;
            snapshot.HasSelectedPart = selectedTarget != null;
            snapshot.SelectedPartName = selectedTarget != null ? selectedTarget.name : "선택된 파츠 없음";
            snapshot.SelectedPartHint = selectedTarget != null
                ? "선택 링크를 강조하고 작은 좌표축만 붙인다. 자세한 값은 여기서 본다."
                : "메인 로봇 메시를 클릭하면 선택 파츠 정보를 여기서 본다.";

            if (selectedTarget == null)
            {
                snapshot.SelectedPartPose = "XYZ -- / ROT --";
                return;
            }

            var roboticsPosition = CoordConverter.FromUnityPosition(selectedTarget.position);
            var rotation = selectedTarget.rotation.eulerAngles;
            snapshot.SelectedPartPose =
                $"XYZ {roboticsPosition.X * 1000.0:0.#}, {roboticsPosition.Y * 1000.0:0.#}, {roboticsPosition.Z * 1000.0:0.#} / ROT {rotation.x:0.#}, {rotation.y:0.#}, {rotation.z:0.#}";
        }

        private static string[] FormatValues(double[] values, string format)
        {
            var result = new string[6];
            for (var i = 0; i < result.Length; i++)
            {
                result[i] = values != null && i < values.Length
                    ? values[i].ToString(format, System.Globalization.CultureInfo.InvariantCulture)
                    : "--";
            }

            return result;
        }

        private FR5PosePresets.Preset? ResolvePreset(string presetName)
        {
            foreach (var preset in FR5PosePresets.All)
            {
                if (string.Equals(preset.Name, presetName, StringComparison.OrdinalIgnoreCase))
                {
                    return preset;
                }
            }

            return null;
        }

        private static double[] CopyJointArray(double[] source)
        {
            return source != null ? (double[])source.Clone() : new double[6];
        }

        private static double[] CopyPoseArray(double[] source)
        {
            return source != null ? (double[])source.Clone() : new double[6];
        }

        private Transform FindBaseLink(Transform root)
        {
            if (root == null)
            {
                return null;
            }

            var baseLinkName = !string.IsNullOrWhiteSpace(templateDefinition?.BaseLinkName)
                ? templateDefinition.BaseLinkName
                : "base_link";
            if (root.name == baseLinkName)
            {
                return root;
            }

            for (var i = 0; i < root.childCount; i++)
            {
                var found = FindBaseLink(root.GetChild(i));
                if (found != null)
                {
                    return found;
                }
            }

            return null;
        }

        private static Transform FindChildRecursive(Transform root, string childName)
        {
            if (root == null || string.IsNullOrWhiteSpace(childName))
            {
                return null;
            }

            if (root.name == childName)
            {
                return root;
            }

            for (var i = 0; i < root.childCount; i++)
            {
                var found = FindChildRecursive(root.GetChild(i), childName);
                if (found != null)
                {
                    return found;
                }
            }

            return null;
        }

        private static void RepairVisualMeshes(GameObject controlRoot)
        {
            if (controlRoot == null)
            {
                return;
            }

            var meshFilters = controlRoot.GetComponentsInChildren<MeshFilter>(true);
            for (var i = 0; i < meshFilters.Length; i++)
            {
                var meshFilter = meshFilters[i];
                if (meshFilter == null)
                {
                    continue;
                }

                var mesh = meshFilter.sharedMesh != null ? meshFilter.sharedMesh : meshFilter.mesh;
                if (mesh == null)
                {
                    continue;
                }

                mesh.RecalculateBounds();
                if (meshFilter.sharedMesh == null)
                {
                    meshFilter.sharedMesh = mesh;
                }
            }
        }

        private void StabilizeControlRobot(GameObject controlRoot)
        {
            if (controlRoot == null)
            {
                return;
            }

            var components = controlRoot.GetComponentsInChildren<MonoBehaviour>(true);
            for (var i = 0; i < components.Length; i++)
            {
                var component = components[i];
                if (component == null)
                {
                    continue;
                }

                if (component.GetType().FullName == "Unity.Robotics.UrdfImporter.Control.Controller")
                {
                    component.enabled = false;
                }
            }

            var articulationBodies = controlRoot.GetComponentsInChildren<ArticulationBody>(true);
            for (var i = 0; i < articulationBodies.Length; i++)
            {
                var body = articulationBodies[i];
                if (body == null)
                {
                    continue;
                }

                body.useGravity = false;
                body.linearVelocity = Vector3.zero;
                body.angularVelocity = Vector3.zero;
                if (!body.isRoot)
                {
                    body.enabled = false;
                }
            }

            var baseLink = FindBaseLink(controlRoot.transform);
            var baseBody = baseLink != null ? baseLink.GetComponent<ArticulationBody>() : null;
            if (baseBody != null)
            {
                baseBody.immovable = true;
            }
        }

        private static PendantV3PreviewState.Kind ToPreviewKind(RobotControlV3RuntimeStatusKind kind)
        {
            return kind switch
            {
                RobotControlV3RuntimeStatusKind.ConnectedServoOff => PendantV3PreviewState.Kind.ConnectedServoOff,
                RobotControlV3RuntimeStatusKind.ConnectedUnsynced => PendantV3PreviewState.Kind.ConnectedUnsynced,
                RobotControlV3RuntimeStatusKind.ReadyToJog => PendantV3PreviewState.Kind.ReadyToJog,
                RobotControlV3RuntimeStatusKind.Fault => PendantV3PreviewState.Kind.Fault,
                RobotControlV3RuntimeStatusKind.AutoReconnect => PendantV3PreviewState.Kind.AutoReconnect,
                _ => PendantV3PreviewState.Kind.Disconnected,
            };
        }
    }
}
