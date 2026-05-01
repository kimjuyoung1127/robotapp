// Folder: EasyMotion - gripper operator control and live gripper write orchestration for Easy Motion.
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
    // Handles gripper probes, percent apply paths, operator/live routing, and authored gripper visual helpers.
    // Evidence truth and live gate summaries are delegated to StatusSafety/Shared partials.
    public sealed partial class RobotControlV3RuntimeController
    {
        public string GetGripperVisualSummaryForDebug()
        {
            ForceInitialize();
            EnsureEndEffectorAttachment();
            if (endEffectorAttachment == null)
            {
                return "attached=False";
            }

            var root = endEffectorAttachment.transform;
            var renderers = root.GetComponentsInChildren<Renderer>(true);
            var meshFilters = root.GetComponentsInChildren<MeshFilter>(true);
            var activeRendererCount = 0;
            var hasBounds = false;
            var bounds = new Bounds(root.position, Vector3.zero);
            for (var i = 0; i < renderers.Length; i++)
            {
                var renderer = renderers[i];
                if (renderer == null || !renderer.enabled || !renderer.gameObject.activeInHierarchy)
                {
                    continue;
                }

                activeRendererCount++;
                if (!hasBounds)
                {
                    bounds = renderer.bounds;
                    hasBounds = true;
                }
                else
                {
                    bounds.Encapsulate(renderer.bounds);
                }
            }

            var cameraVisible = false;
            if (stageCamera != null && hasBounds)
            {
                var view = stageCamera.WorldToViewportPoint(bounds.center);
                cameraVisible = view.z > 0f && view.x >= 0f && view.x <= 1f && view.y >= 0f && view.y <= 1f;
            }

            var local = root.localPosition;
            var euler = root.localEulerAngles;
            var viewport = stageCamera != null && hasBounds ? stageCamera.WorldToViewportPoint(bounds.center) : Vector3.zero;
            var tcpLocal = endEffectorAttachment.TcpFrame != null ? endEffectorAttachment.TcpFrame.localPosition : Vector3.zero;
            var modelLocal = endEffectorAttachment.ModelRoot != null ? endEffectorAttachment.ModelRoot.localPosition : Vector3.zero;
            var fingerLeftLocal = endEffectorAttachment.FingerLeft != null ? endEffectorAttachment.FingerLeft.localPosition : Vector3.zero;
            var fingerRightLocal = endEffectorAttachment.FingerRight != null ? endEffectorAttachment.FingerRight.localPosition : Vector3.zero;
            return $"attached=True; active={root.gameObject.activeInHierarchy}; renderers={renderers.Length}; activeRenderers={activeRendererCount}; meshFilters={meshFilters.Length}; local=({local.x:0.###},{local.y:0.###},{local.z:0.###}); rot=({euler.x:0.#},{euler.y:0.#},{euler.z:0.#}); scale=({root.localScale.x:0.###},{root.localScale.y:0.###},{root.localScale.z:0.###}); tcpLocal=({tcpLocal.x:0.####},{tcpLocal.y:0.####},{tcpLocal.z:0.####}); modelLocal=({modelLocal.x:0.####},{modelLocal.y:0.####},{modelLocal.z:0.####}); fingerLeft=({fingerLeftLocal.x:0.####},{fingerLeftLocal.y:0.####},{fingerLeftLocal.z:0.####}); fingerRight=({fingerRightLocal.x:0.####},{fingerRightLocal.y:0.####},{fingerRightLocal.z:0.####}); closure=({endEffectorAttachment.BuildClosureDebugSummary()}); boundsCenter=({bounds.center.x:0.###},{bounds.center.y:0.###},{bounds.center.z:0.###}); boundsSize=({bounds.size.x:0.###},{bounds.size.y:0.###},{bounds.size.z:0.###}); viewport=({viewport.x:0.###},{viewport.y:0.###},{viewport.z:0.###}); cameraVisible={cameraVisible}; openRatio={endEffectorAttachment.GripperOpenRatio:0.00}";
        }


        public string RecaptureGripperAuthoredOpenForDebug()
        {
            ForceInitialize();
            EnsureEndEffectorAttachment();
            if (endEffectorAttachment == null)
            {
                return "attached=False";
            }

            endEffectorAttachment.RecaptureAuthoredOpenPose();
            ApplyGripperVisual(peripheralFacade?.Snapshot.GripperOpenRatio ?? 1f);
            return GetGripperVisualSummaryForDebug();
        }


        public string RecaptureGripperAuthoredClosedForDebug()
        {
            ForceInitialize();
            EnsureEndEffectorAttachment();
            if (endEffectorAttachment == null)
            {
                return "attached=False";
            }

            endEffectorAttachment.RecaptureAuthoredClosedPose();
            ApplyGripperVisual(0f);
            return GetGripperVisualSummaryForDebug();
        }


        public string ClearGripperAuthoredClosedForDebug()
        {
            ForceInitialize();
            EnsureEndEffectorAttachment();
            if (endEffectorAttachment == null)
            {
                return "attached=False";
            }

            endEffectorAttachment.ClearAuthoredClosedPose();
            ApplyGripperVisual(peripheralFacade?.Snapshot.GripperOpenRatio ?? 1f);
            return GetGripperVisualSummaryForDebug();
        }


        public string GetGripperSdkSummaryForDebug(bool includeReadback)
        {
            ForceInitialize();
            var summary = peripheralFacade != null
                ? peripheralFacade.GetGripperSdkSummary(includeReadback)
                : "sdkGripper=blocked; reason=peripheral facade missing";
            RefreshSnapshot();
            return summary;
        }


        public void SimulateGripper(bool open)
        {
            SetGripperOpen(open);
        }


        public FairinoResult SetGripperOpen(bool open)
        {
            return SetGripperPositionPercent(open ? 100f : 0f);
        }


        public string ProbeLiveGripperForDebug()
        {
            if (!EnsureReadyForCommand("gripper probe"))
            {
                return lastInitializationError;
            }

            if (connectionService == null || connectionService.IsMockMode)
            {
                return "live gripper probe unavailable: mock mode";
            }

            var sibling = connectionService.CreateMotionSiblingSession();
            if (!sibling.IsSuccess)
            {
                return sibling.Message;
            }

            var liveService = sibling.Value;
            var expectedProfile = config?.GetGripperProfile() ?? FairinoGripperProfile.Pgea10040Default;
            var capability = liveService.ProbeGripperCapability();
            var gripperConfig = liveService.ReadGripperConfig();
            var gripperStatus = liveService.ReadGripperStatus();
            var modeLabel = ResolveControllerModeLabel();
            var expectedSummary = $"{expectedProfile}";
            var configSummary = gripperConfig.IsSuccess
                ? $"{gripperConfig.Value}; matchesExpected={gripperConfig.Value.Matches(expectedProfile)}"
                : $"FAIL({gripperConfig.ErrorCode}:{gripperConfig.Message})";
            var statusSummary = gripperStatus.IsSuccess
                ? gripperStatus.Value.ToString()
                : $"FAIL({gripperStatus.ErrorCode}:{gripperStatus.Message})";
            var capabilitySummary = capability.IsSuccess
                ? capability.Value.ToString()
                : $"FAIL({capability.ErrorCode}:{capability.Message})";
            return $"mode={modeLabel}; expectedProfile=({expectedSummary}); capability=({capabilitySummary}); sdkConfig=({configSummary}); status=({statusSummary})";
        }


        public string ProbeLiveGripperIndexCandidatesForDebug(int minIndex = 1, int maxIndex = 4)
        {
            if (!EnsureReadyForCommand("gripper index probe"))
            {
                return lastInitializationError;
            }

            if (connectionService == null || connectionService.IsMockMode)
            {
                return "live gripper index probe unavailable: mock mode";
            }

            if (minIndex > maxIndex)
            {
                (minIndex, maxIndex) = (maxIndex, minIndex);
            }

            minIndex = Mathf.Clamp(minIndex, 1, 8);
            maxIndex = Mathf.Clamp(maxIndex, minIndex, 8);

            var sibling = connectionService.CreateMotionSiblingSession();
            if (!sibling.IsSuccess)
            {
                return sibling.Message;
            }

            var liveService = sibling.Value;
            var baseProfile = config?.GetGripperProfile() ?? FairinoGripperProfile.Pgea10040Default;
            var modeLabel = ResolveControllerModeLabel();
            var lines = new System.Text.StringBuilder();
            lines.Append($"mode={modeLabel}; baseProfile=({baseProfile})");
            for (var index = minIndex; index <= maxIndex; index++)
            {
                var candidate = new FairinoGripperProfile(
                    baseProfile.Company,
                    baseProfile.Device,
                    baseProfile.SoftVersion,
                    baseProfile.Bus,
                    index);
                lines.AppendLine();
                lines.Append($"index{index}: ");
                lines.Append(RobotControlPeripheralFacade.ProbeLiveGripperProfileForDebug(liveService, candidate));
            }

            return lines.ToString();
        }


        public string ProbeLiveGripperActivationSequencesForDebug()
        {
            if (!EnsureReadyForCommand("gripper activation sequence probe"))
            {
                return lastInitializationError;
            }

            if (connectionService == null || connectionService.IsMockMode)
            {
                return "live gripper activation sequence probe unavailable: mock mode";
            }

            var sibling = connectionService.CreateMotionSiblingSession();
            if (!sibling.IsSuccess)
            {
                return sibling.Message;
            }

            var liveService = sibling.Value;
            var profile = config?.GetGripperProfile() ?? FairinoGripperProfile.Pgea10040Default;
            var modeLabel = ResolveControllerModeLabel();
            return $"mode={modeLabel}; {RobotControlPeripheralFacade.ProbeLiveGripperActivationSequencesForDebug(liveService, profile)}";
        }


        public FairinoResult SetGripperPositionPercent(int positionPercent)
        {
            return SetGripperPositionPercent((float)positionPercent);
        }


        public FairinoResult SetGripperPositionPercent(float positionPercent)
        {
            var clampedPosition = ClampPercent(positionPercent);
            if (!EnsureReadyForCommand($"그리퍼 {clampedPosition}%"))
            {
                return FairinoResult.Fail(-30, lastInitializationError);
            }

            FairinoConnectionService liveGripperService = null;
            var hasGripperReadback = true;
            var requestedSpeedPercent = ResolveRequestedSpeedPercent();
            if (!snapshot.DryRunEnabled && connectionService != null && !connectionService.IsMockMode)
            {
                if (CanUseCurrentLiveSessionForGripper())
                {
                    liveGripperService = connectionService;
                    var capability = liveGripperService.ProbeGripperCapability();
                    hasGripperReadback = capability.IsSuccess
                        && (capability.Value.CanReadPosition || capability.Value.CanReadMotion);
                    requestedSpeedPercent = Mathf.Min(requestedSpeedPercent, LiveCommandSafetyGate.DefaultLiveSpeedCapPercent);
                }
                else if (HasDedicatedLiveGripperSmokePathConfigured())
                {
                    var sibling = connectionService.CreateMotionSiblingSession();
                    if (!sibling.IsSuccess)
                    {
                        PushFeedback(sibling.Message);
                        RememberOperatorBlockedReason(sibling.Message);
                        RefreshSnapshot();
                        return new FairinoResult(sibling.ErrorCode, sibling.Message);
                    }

                    liveGripperService = sibling.Value;
                    var capability = liveGripperService.ProbeGripperCapability();
                    hasGripperReadback = capability.IsSuccess
                        && (capability.Value.CanReadPosition || capability.Value.CanReadMotion);
                    requestedSpeedPercent = Mathf.Min(requestedSpeedPercent, LiveCommandSafetyGate.DefaultLiveSpeedCapPercent);
                }

                var gate = EvaluateLiveCommandSafety(
                    LiveCommandKind.MoveGripper,
                    requestedSpeedPercent,
                    productionIkSafe: true,
                    boundaryReady: true,
                    collisionReady: true,
                    hasGripperReadback: hasGripperReadback,
                    gateConnectionService: liveGripperService);
                if (!gate.CanExecuteLive)
                {
                    return BlockLiveCommand(gate, "live-gripper-blocked");
                }
            }

            var objectDetected = TryResolveGripperObjectStopPercent(out var objectStopPercent);
            var result = peripheralFacade.SetGripperPosition(
                clampedPosition,
                snapshot.DryRunEnabled,
                objectDetected,
                objectStopPercent,
                liveGripperService);
            ApplyGripperVisual(peripheralFacade.Snapshot.GripperOpenRatio);
            PushFeedback(result.Message);
            if (result.IsSuccess)
            {
                ClearRememberedOperatorBlockedReason();
            }
            else
            {
                RememberOperatorBlockedReason(result.Message);
            }
            ResetLiveSessionModeAfterLiveAttempt(LiveCommandKind.MoveGripper, result);
            RefreshSnapshot();
            return result;
        }


        private bool CanUseCurrentLiveSessionForGripper()
        {
            return connectionService != null
                && !connectionService.IsMockMode
                && connectionService.Client is IFairinoLiveClientDiagnostics { IsReadbackOnly: false };
        }


        public FairinoResult SetGripperPositionPercentFromOperator(int positionPercent)
        {
            return SetGripperPositionPercentFromOperator((float)positionPercent);
        }


        public FairinoResult SetGripperPositionPercentFromOperator(float positionPercent)
        {
            var restoreDryRun = false;
            if (ShouldUseLiveGripperOperatorPath())
            {
                var preflight = PreflightLiveGripperOperatorPath(allowWarmup: true);
                if (!preflight.IsSuccess)
                {
                    RememberOperatorBlockedReason(preflight.Message);
                    PushFeedback(preflight.Message);
                    RefreshSnapshot();
                    return preflight;
                }

                SetLiveSessionMode(LiveCommandSessionMode.LiveControl);
                if (snapshot.DryRunEnabled)
                {
                    snapshot.DryRunEnabled = false;
                    restoreDryRun = true;
                }

                snapshot.LiveBlockedReason = string.Empty;
            }

            var result = SetGripperPositionPercent(positionPercent);

            if (restoreDryRun && !snapshot.DryRunEnabled)
            {
                snapshot.DryRunEnabled = true;
                InvalidateLiveApprovalContext();
                PushFeedback($"{result.Message} · DryRun으로 다시 잠갔다.");
                RefreshSnapshot();
            }

            return result;
        }

    }
}
