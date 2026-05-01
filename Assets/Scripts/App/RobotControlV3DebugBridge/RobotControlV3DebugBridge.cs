// Folder: App - Application controllers and services; single UnityEngine entry point.
using System.Collections.Generic;
using System.IO;
using System.Text;
using KineTutor3D.UI.RobotControlV3;
using KineTutor3D.App.Fairino;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace KineTutor3D.App
{
    /// <summary>
    /// RobotControlV3 입력 계약을 `unityctl exec`로 점검하기 위한 디버그 브리지입니다.
    /// </summary>
    public static partial class RobotControlV3DebugBridge
    {
        private static PendantV3InputContract GetInputContract()
        {
            var scene = SceneManager.GetActiveScene();
            if (!scene.IsValid() || scene.path != "Assets/Scenes/RobotControlV3.unity")
            {
                throw new System.InvalidOperationException($"RobotControlV3 scene must be active. Current: {scene.path}");
            }

            var contract = Object.FindFirstObjectByType<PendantV3InputContract>(FindObjectsInactive.Include);
            if (contract == null)
            {
                throw new MissingReferenceException("PendantV3InputContract not found in RobotControlV3 scene.");
            }

            return contract;
        }

        private static JointJogController GetJointJogController()
        {
            var scene = SceneManager.GetActiveScene();
            if (!scene.IsValid() || scene.path != "Assets/Scenes/RobotControlV3.unity")
            {
                throw new System.InvalidOperationException($"RobotControlV3 scene must be active. Current: {scene.path}");
            }

            var jointJog = Object.FindFirstObjectByType<JointJogController>(FindObjectsInactive.Include);
            if (jointJog == null)
            {
                throw new MissingReferenceException("JointJogController not found in RobotControlV3 scene.");
            }

            return jointJog;
        }

        private static TcpJogController GetTcpJogController()
        {
            var scene = SceneManager.GetActiveScene();
            if (!scene.IsValid() || scene.path != "Assets/Scenes/RobotControlV3.unity")
            {
                throw new System.InvalidOperationException($"RobotControlV3 scene must be active. Current: {scene.path}");
            }

            var tcpJog = Object.FindFirstObjectByType<TcpJogController>(FindObjectsInactive.Include);
            if (tcpJog == null)
            {
                throw new MissingReferenceException("TcpJogController not found in RobotControlV3 scene.");
            }

            return tcpJog;
        }

        private static PointMoveController GetPointMoveController()
        {
            var scene = SceneManager.GetActiveScene();
            if (!scene.IsValid() || scene.path != "Assets/Scenes/RobotControlV3.unity")
            {
                throw new System.InvalidOperationException($"RobotControlV3 scene must be active. Current: {scene.path}");
            }

            var pointMove = Object.FindFirstObjectByType<PointMoveController>(FindObjectsInactive.Include);
            if (pointMove == null)
            {
                throw new MissingReferenceException("PointMoveController not found in RobotControlV3 scene.");
            }

            return pointMove;
        }

        private static RobotControlV3RuntimeController GetRuntimeController()
        {
            var scene = SceneManager.GetActiveScene();
            if (!scene.IsValid() || scene.path != "Assets/Scenes/RobotControlV3.unity")
            {
                throw new System.InvalidOperationException($"RobotControlV3 scene must be active. Current: {scene.path}");
            }

            var runtime = Object.FindFirstObjectByType<RobotControlV3RuntimeController>(FindObjectsInactive.Include);
            if (runtime == null)
            {
                throw new MissingReferenceException("RobotControlV3RuntimeController not found in RobotControlV3 scene.");
            }

            runtime.ForceInitialize();
            return runtime;
        }

        private static int CountDescendants(VisualElement root)
        {
            var total = 0;
            using var iterator = root.Children().GetEnumerator();
            while (iterator.MoveNext())
            {
                var child = iterator.Current;
                total++;
                total += CountDescendants(child);
            }

            return total;
        }

        private static string GetScrollLayoutSummary(string label, VisualElement host, ScrollView scrollView)
        {
            if (scrollView == null)
            {
                return $"{label}=missing";
            }

            var viewportWidth = scrollView.contentViewport?.worldBound.width ?? 0f;
            var viewportHeight = scrollView.contentViewport?.worldBound.height ?? 0f;
            var contentWidth = scrollView.contentContainer?.worldBound.width ?? 0f;
            var contentHeight = scrollView.contentContainer?.worldBound.height ?? 0f;
            var hostHeight = host?.worldBound.height ?? 0f;
            var scrollShare = hostHeight > 0.1f ? viewportHeight / hostHeight : 0f;
            var horizontalVisible = scrollView.horizontalScroller != null
                && scrollView.horizontalScroller.resolvedStyle.display != DisplayStyle.None;
            var clipped = CountHorizontallyClippedDescendants(
                scrollView.contentContainer,
                scrollView.contentViewport?.worldBound ?? Rect.zero);
            return $"{label}Mode={scrollView.mode}; {label}Viewport={viewportWidth:F1}x{viewportHeight:F1}; {label}Content={contentWidth:F1}x{contentHeight:F1}; {label}ScrollShare={scrollShare:F2}; {label}HorizontalVisible={horizontalVisible}; {label}Clipped={clipped}";
        }

        private static string BuildDirectChildOrder(VisualElement parent)
        {
            if (parent == null)
            {
                return "missing";
            }

            var names = new List<string>();
            using var iterator = parent.Children().GetEnumerator();
            while (iterator.MoveNext())
            {
                var child = iterator.Current;
                names.Add(string.IsNullOrWhiteSpace(child.name) ? child.GetType().Name : child.name);
            }

            return string.Join(",", names);
        }

        private static int CountHorizontallyClippedDescendants(VisualElement element, Rect clipBounds)
        {
            if (element == null || clipBounds.width <= 0.1f)
            {
                return 0;
            }

            var total = 0;
            using var iterator = element.Children().GetEnumerator();
            while (iterator.MoveNext())
            {
                var child = iterator.Current;
                if (IsVisibleForLayout(child))
                {
                    var bounds = child.worldBound;
                    if (bounds.width > 0.5f
                        && (bounds.xMin < clipBounds.xMin - 0.5f || bounds.xMax > clipBounds.xMax + 0.5f))
                    {
                        total++;
                    }
                }

                total += CountHorizontallyClippedDescendants(child, clipBounds);
            }

            return total;
        }

        private static bool IsVisibleForLayout(VisualElement element)
        {
            return element.resolvedStyle.display != DisplayStyle.None
                && element.resolvedStyle.visibility != Visibility.Hidden
                && element.worldBound.width > 0.5f
                && element.worldBound.height > 0.5f;
        }

        private static string ClickUiButton(string buttonName, string prefer, out bool found, out bool enabled, out string path)
        {
            var documents = Object.FindObjectsByType<UIDocument>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            if (documents == null || documents.Length == 0)
            {
                found = false;
                enabled = false;
                path = string.Empty;
                return "document-missing";
            }

            var buttons = new List<Button>();
            for (var i = 0; i < documents.Length; i++)
            {
                var root = documents[i]?.rootVisualElement;
                root?.Query<Button>(name: buttonName).ForEach(button => buttons.Add(button));
            }

            var pointMove = Object.FindFirstObjectByType<PointMoveController>(FindObjectsInactive.Include);
            pointMove?.CollectButtonsForDebug(buttonName, buttons);
            buttons.RemoveAll(button => button == null || button.panel == null);

            if (buttons.Count == 0)
            {
                found = false;
                enabled = false;
                path = string.Empty;
                return $"not-found; available={BuildAvailableButtonNameSummary(documents, pointMove)}";
            }

            var selected = SelectButton(buttons, prefer);
            found = selected != null;
            enabled = selected != null && selected.enabledInHierarchy;
            path = selected != null ? BuildElementPath(selected) : string.Empty;
            if (selected == null)
            {
                return "not-found";
            }

            if (!selected.enabledInHierarchy)
            {
                return "disabled";
            }

            if (TryInvokeKnownButtonAction(buttonName, prefer))
            {
                return $"clicked:{buttonName};fallback";
            }

            using var clickEvent = ClickEvent.GetPooled();
            clickEvent.target = selected;
            if (selected.clickable != null)
            {
                var simulate = typeof(Clickable).GetMethod(
                    "SimulateSingleClick",
                    System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
                var invoke = typeof(Clickable).GetMethod(
                    "Invoke",
                    System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
                if (simulate != null)
                {
                    simulate.Invoke(selected.clickable, new object[] { clickEvent, 0 });
                }
                else if (invoke != null)
                {
                    invoke.Invoke(selected.clickable, new object[] { clickEvent });
                }
                else
                {
                    selected.SendEvent(clickEvent);
                }
            }
            else
            {
                selected.SendEvent(clickEvent);
            }
            return $"clicked:{buttonName}";
        }

        private static bool TryInvokeKnownButtonAction(string buttonName, string prefer)
        {
            var shell = Object.FindFirstObjectByType<PendantV3ShellStateController>(FindObjectsInactive.Include);
            if (shell != null)
            {
                var state = shell.GetStateSnapshot();
                if (buttonName is "NavHome" or "NavMotion" or "NavPoints" or "NavStatus" or "NavHelp")
                {
                    shell.SetDebugSelection(buttonName, state.ActiveWorkTab, state.ActiveTabletTab);
                    return true;
                }

                if (buttonName is "TabEasyMotion" or "TabJointJog" or "TabTcpJog")
                {
                    shell.SetDebugSelection(state.ActiveNavSection, buttonName, state.ActiveTabletTab);
                    return true;
                }

                if (buttonName is "BottomTabEasyMotion" or "BottomTabJointJog" or "BottomTabTcpJog" or "BottomTabPointMove" or "BottomTabStatus" or "BottomTabHelp")
                {
                    shell.SetDebugSelection(state.ActiveNavSection, state.ActiveWorkTab, buttonName);
                    return true;
                }
            }

            var runtime = Object.FindFirstObjectByType<RobotControlV3RuntimeController>(FindObjectsInactive.Include);
            if (runtime == null)
            {
                return false;
            }

            runtime.ForceInitialize();
            switch (buttonName)
            {
                case "BtnEasyHome":
                    runtime.PreviewPreset("Home");
                    return true;
                case "BtnEasyReady":
                    runtime.PreviewPreset("Ready");
                    return true;
                case "BtnEasyFolded":
                    runtime.PreviewPreset("Folded");
                    return true;
                case "BtnEasyZero":
                    runtime.PreviewPreset("Zero");
                    return true;
                case "BtnIoGripperOpen":
                case "BtnGripperOpen":
                    runtime.SetGripperOpen(true);
                    return true;
                case "BtnIoGripperApply":
                    runtime.SetGripperPositionPercent(runtime.CurrentSnapshot.GripperCommandedPositionPercent);
                    return true;
                case "BtnIoGripperClose":
                case "BtnGripperClose":
                    runtime.SetGripperOpen(false);
                    return true;
                case "BtnEasyGripper100":
                    SetEasyMotionGripperInputForDebug(100f, apply: false);
                    return true;
                case "BtnEasyGripper50":
                    SetEasyMotionGripperInputForDebug(50f, apply: false);
                    return true;
                case "BtnEasyGripper0":
                    SetEasyMotionGripperInputForDebug(0f, apply: false);
                    return true;
                case "BtnEasyGripperPreviewApply":
                case "BtnEasyGripperLiveApply":
                    SetEasyMotionGripperInputForDebug(runtime.CurrentSnapshot.GripperCommandedPositionPercent, apply: true);
                    return true;
                case "BtnHeaderModeAuto":
                case "BtnModeAuto":
                    runtime.RequestAutoMode();
                    return true;
                case "BtnHeaderModeManual":
                case "BtnModeManual":
                    runtime.RequestManualMode();
                    return true;
                case "BtnRun":
                case "BtnRunBottom":
                    runtime.ExecutePrimaryAction();
                    return true;
                case "BtnStop":
                case "BtnStopBottom":
                    runtime.StopMotion();
                    return true;
                case "BtnPopupConfirm":
                    Object.FindFirstObjectByType<PopupCoordinatorV3>(FindObjectsInactive.Include)
                        ?.ConfirmActivePopupForDebug();
                    return true;
                case "BtnPopupCancel":
                    Object.FindFirstObjectByType<PopupCoordinatorV3>(FindObjectsInactive.Include)
                        ?.CancelActivePopupForDebug();
                    return true;
            }

            if (TryParseStepButton(buttonName, "BtnJoint", 6, out var jointAxis, out var jointDirection))
            {
                GetJointJogController().NudgeJointForDebug(jointAxis, jointDirection);
                return true;
            }

            if (TryParseStepButton(buttonName, "BtnTcp", 6, out var tcpAxis, out var tcpDirection))
            {
                var labels = new[] { "X", "Y", "Z", "RX", "RY", "RZ" };
                GetTcpJogController().NudgeAxisForDebug(labels[tcpAxis - 1], tcpDirection);
                return true;
            }

            return false;
        }

        private static bool TryParseStepButton(string buttonName, string prefix, int maxAxis, out int axisNumber, out int direction)
        {
            axisNumber = 0;
            direction = 0;
            if (string.IsNullOrWhiteSpace(buttonName) || !buttonName.StartsWith(prefix, System.StringComparison.Ordinal))
            {
                return false;
            }

            var suffix = buttonName.Substring(prefix.Length);
            var isPlus = suffix.EndsWith("Plus", System.StringComparison.Ordinal);
            var isMinus = suffix.EndsWith("Minus", System.StringComparison.Ordinal);
            if (!isPlus && !isMinus)
            {
                return false;
            }

            var axisText = suffix.Substring(0, suffix.Length - (isPlus ? "Plus".Length : "Minus".Length));
            if (!int.TryParse(axisText, out axisNumber) || axisNumber < 1 || axisNumber > maxAxis)
            {
                return false;
            }

            direction = isPlus ? 1 : -1;
            return true;
        }

        private static string BuildAvailableButtonNameSummary(UIDocument[] documents, PointMoveController pointMove)
        {
            if (documents == null || documents.Length == 0)
            {
                return string.Empty;
            }

            var names = new List<string>();
            for (var i = 0; i < documents.Length; i++)
            {
                var root = documents[i]?.rootVisualElement;
                root?.Query<Button>().ForEach(button =>
                {
                    if (!string.IsNullOrWhiteSpace(button.name)
                        && names.Count < 128
                        && !names.Contains(button.name))
                    {
                        names.Add(button.name);
                    }
                });
            }

            var pointButtons = new List<Button>();
            pointMove?.CollectButtonsForDebug(pointButtons);
            for (var i = 0; i < pointButtons.Count && names.Count < 128; i++)
            {
                var buttonName = pointButtons[i]?.name;
                if (!string.IsNullOrWhiteSpace(buttonName) && !names.Contains(buttonName))
                {
                    names.Add(buttonName);
                }
            }

            names.Sort(System.StringComparer.Ordinal);
            return string.Join(",", names);
        }

        private static FairinoConnectionService GetRuntimeConnectionService()
        {
            var runtime = GetRuntimeController();
            return runtime.ConnectionServiceForDebug;
        }

        private static LiveCommandSafetyGateRequest NewGateRequest(
            FairinoConnectionService service,
            LiveCommandKind kind,
            bool dryRun,
            bool confirmed,
            int speed = 10,
            bool boundary = false,
            bool collision = false,
            bool productionIk = true,
            bool gripperReadback = false)
        {
            return new LiveCommandSafetyGateRequest
            {
                Kind = kind,
                ConnectionService = service,
                AllowDryRun = dryRun,
                OperatorConfirmed = confirmed,
                RequestedSpeedPercent = speed,
                SpeedCapPercent = LiveCommandSafetyGate.DefaultLiveSpeedCapPercent,
                HasDryRunPreviewArtifact = true,
                IsProductionIkSafe = productionIk,
                IsBoundaryDataReady = boundary,
                IsTargetWithinBoundary = boundary,
                IsCollisionDataReady = collision,
                IsPredictedPathCollisionFree = collision,
                HasGripperReadback = gripperReadback,
                TreatMockAsLiveForDebug = true,
            };
        }

        private static double[] BuildOffsetTcpTarget(double dxMm, double dyMm, double dzMm)
        {
            var snapshot = GetRuntimeController().CurrentSnapshot;
            var target = new double[6];
            for (var index = 0; index < target.Length; index++)
            {
                target[index] = ParseSnapshotValue(snapshot.TcpValues, index);
            }

            target[0] += dxMm;
            target[1] += dyMm;
            target[2] += dzMm;
            return target;
        }

        private static double ParseSnapshotValue(string[] values, int index)
        {
            if (values == null || index < 0 || index >= values.Length)
            {
                return 0.0;
            }

            return double.TryParse(values[index], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var parsed)
                ? parsed
                : 0.0;
        }

        private static void SeedRecordedPath()
        {
            var runtime = GetRuntimeController();
            EnsureRuntimeReady(runtime);
            runtime.StartTeachingPathRecording();
            runtime.ApplyTcpPose(BuildOffsetTcpTarget(10.0, 0.0, 0.0), "seed recorded path A");
            runtime.CaptureTeachingPathFrameForDebug();
            runtime.ApplyTcpPose(BuildOffsetTcpTarget(10.0, 10.0, 0.0), "seed recorded path B");
            runtime.CaptureTeachingPathFrameForDebug();
            runtime.StopTeachingPathRecording();
        }

        private static void EnsureRuntimeReady(RobotControlV3RuntimeController runtime)
        {
            runtime.StopMotion();
            runtime.Disconnect();
            runtime.ConnectDefault();
            runtime.EnableServo();
            runtime.SetTeachingLoopEnabled(false);
            if (!runtime.CurrentSnapshot.DryRunEnabled)
            {
                runtime.ToggleDryRun();
            }
        }

        private static void AddPointGuard(GenericMatrixPayload payload, string name, System.Func<FairinoResult> action, string needle)
        {
            var result = new GenericMatrixResult
            {
                name = name,
                expected = needle ?? string.Empty,
            };

            try
            {
                var actionResult = action();
                result.message = actionResult.Message;
                result.after = GetMovementStateSummaryForDebug();
                result.passed = string.IsNullOrEmpty(needle)
                    || actionResult.Message.Contains(needle)
                    || result.after.Contains(needle);
                if (!result.passed)
                {
                    result.failureClass = "runtime";
                }
            }
            catch (System.Exception ex)
            {
                result.passed = false;
                result.failureClass = "exception";
                result.after = $"{ex.GetType().Name}: {ex.Message}";
            }

            payload.results.Add(result);
        }

        private static string SnapshotPoseSignature(RobotControlV3RuntimeController runtime)
        {
            var snapshot = runtime.CurrentSnapshot;
            var joints = snapshot.JointValues != null ? string.Join(",", snapshot.JointValues) : string.Empty;
            var tcp = snapshot.TcpValues != null ? string.Join(",", snapshot.TcpValues) : string.Empty;
            return $"joints={joints};tcp={tcp}";
        }

        private static string BuildStagePoseSignature(RobotControlV3RuntimeController runtime)
        {
            var snapshot = runtime.CurrentSnapshot;
            var joints = snapshot.JointValues != null ? string.Join(",", snapshot.JointValues) : string.Empty;
            var tcp = snapshot.TcpValues != null ? string.Join(",", snapshot.TcpValues) : string.Empty;
            var visual = GetGripperVisualSummaryForDebug();
            return $"status={snapshot.StatusKind};pending={snapshot.PendingCommandSummary};feedback={snapshot.LastFeedback};joints={joints};tcp={tcp};ghost={snapshot.HasGhostPreview};path={snapshot.HasPredictedPath};visual=[{visual}]";
        }

        private static string CompleteGenericMatrix(GenericMatrixPayload payload, string artifactName, string label)
        {
            var passCount = 0;
            var failCount = 0;
            var failures = new StringBuilder();
            foreach (var result in payload.results)
            {
                if (result.passed)
                {
                    passCount++;
                }
                else
                {
                    failCount++;
                    failures.Append(result.name)
                        .Append('(')
                        .Append(result.failureClass)
                        .Append("),");
                }
            }

            payload.caseCount = payload.results.Count;
            payload.passCount = passCount;
            payload.failCount = failCount;

            var project = payload.project;
            var artifactPath = Path.Combine(project, "Artifacts", artifactName);
            Directory.CreateDirectory(Path.GetDirectoryName(artifactPath));
            File.WriteAllText(artifactPath, JsonUtility.ToJson(payload, true), Encoding.UTF8);
            return $"{label} pass={passCount}; fail={failCount}; artifact={artifactPath}; failures={failures}";
        }

        private static string CaptureStageAngle(RobotControlV3RuntimeController runtime, string label, Vector3 direction, string outputPath)
        {
            runtime.ForceInitialize();
            var camera = runtime.StageCamera;
            if (camera == null)
            {
                return $"{label}:camera-missing";
            }

            if (!TryGetSceneRendererBounds(out var bounds))
            {
                return $"{label}:bounds-missing";
            }

            var focus = bounds.center;
            var safeDirection = direction.sqrMagnitude > 0.001f ? direction.normalized : new Vector3(0f, 0.55f, -1f).normalized;
            var radius = Mathf.Max(bounds.extents.magnitude * 2.15f, 1.6f);
            camera.transform.position = focus + safeDirection * radius;
            camera.transform.LookAt(focus);
            return CaptureCamera(camera, outputPath);
        }

        private static bool TryGetSceneRendererBounds(out Bounds bounds)
        {
            bounds = default;
            var renderers = Object.FindObjectsByType<Renderer>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            var found = false;
            for (var i = 0; i < renderers.Length; i++)
            {
                var renderer = renderers[i];
                if (renderer == null || renderer.GetComponentInParent<UIDocument>() != null)
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

            return found && bounds.size.sqrMagnitude > 0.0001f;
        }

        private static string CaptureCamera(Camera camera, string outputPath, int width = 1280, int height = 720)
        {
            var fullPath = Path.GetFullPath(outputPath);
            Directory.CreateDirectory(Path.GetDirectoryName(fullPath));
            var previousTarget = camera.targetTexture;
            var previousActive = RenderTexture.active;
            var renderTexture = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32)
            {
                name = "RobotControlV3StageAngleCapture"
            };
            var texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
            try
            {
                camera.targetTexture = renderTexture;
                RenderTexture.active = renderTexture;
                camera.Render();
                texture.ReadPixels(new Rect(0, 0, width, height), 0, 0);
                texture.Apply();
                File.WriteAllBytes(fullPath, texture.EncodeToPNG());
            }
            finally
            {
                camera.targetTexture = previousTarget;
                RenderTexture.active = previousActive;
                if (Application.isPlaying)
                {
                    Object.Destroy(renderTexture);
                    Object.Destroy(texture);
                }
                else
                {
                    Object.DestroyImmediate(renderTexture);
                    Object.DestroyImmediate(texture);
                }
            }

            return $"{Path.GetFileName(fullPath)}:{width}x{height}";
        }

        private static Button SelectButton(List<Button> buttons, string prefer)
        {
            if (buttons == null || buttons.Count == 0)
            {
                return null;
            }

            if (string.Equals(prefer, "tablet", System.StringComparison.OrdinalIgnoreCase))
            {
                for (var i = 0; i < buttons.Count; i++)
                {
                    if (HasAncestor(buttons[i], "BottomSheet") || HasAncestor(buttons[i], "BottomBar"))
                    {
                        return buttons[i];
                    }
                }
            }

            for (var i = 0; i < buttons.Count; i++)
            {
                if (!HasAncestor(buttons[i], "BottomSheet"))
                {
                    return buttons[i];
                }
            }

            return buttons[0];
        }

        private static bool HasAncestor(VisualElement element, string ancestorName)
        {
            for (var current = element; current != null; current = current.parent)
            {
                if (current.name == ancestorName)
                {
                    return true;
                }
            }

            return false;
        }

        private static string BuildElementPath(VisualElement element)
        {
            if (element == null)
            {
                return string.Empty;
            }

            var stack = new Stack<string>();
            for (var current = element; current != null; current = current.parent)
            {
                stack.Push(string.IsNullOrEmpty(current.name) ? current.GetType().Name : current.name);
            }

            return string.Join("/", stack);
        }

        [System.Serializable]
        private sealed class ActualClickMatrixPayload
        {
            public string generatedAt;
            public string project;
            public int caseCount;
            public int passCount;
            public int failCount;
            public List<ActualClickMatrixResult> results = new();
        }

        [System.Serializable]
        private sealed class ActualClickMatrixResult
        {
            public string name;
            public string prefer;
            public string expected;
            public bool passed;
            public string failureClass;
            public bool found;
            public bool enabled;
            public string path;
            public string before;
            public string after;
            public string clickMessage;
        }

        [System.Serializable]
        private sealed class GenericMatrixPayload
        {
            public string generatedAt;
            public string project;
            public string name;
            public int caseCount;
            public int passCount;
            public int failCount;
            public List<GenericMatrixResult> results = new();
        }

        [System.Serializable]
        private sealed class GenericMatrixResult
        {
            public string name;
            public string expected;
            public bool passed;
            public string failureClass;
            public string path;
            public string before;
            public string after;
            public string message;
        }

        private static ScrollView GetContextPanelScrollView()
        {
            var contract = GetInputContract();
            var scrollView = contract.GetComponent<UIDocument>()?.rootVisualElement?.Q<ScrollView>("ContextPanelScroll");
            if (scrollView == null)
            {
                throw new MissingReferenceException("ContextPanelScroll not found in RobotControlV3 document.");
            }

            return scrollView;
        }
    }
}
