// Folder: UI - sandbox robot picker overlay using the shared showroom shell.
using System;
using KineTutor3D.App;
using KineTutor3D.Templates;
using UnityEngine;
using UnityEngine.UI;

namespace KineTutor3D.UI
{
    /// <summary>
    /// 샌드박스에서 로봇을 교체할 때 사용하는 오버레이 쇼룸입니다.
    /// </summary>
    public class SandboxRobotPickerOverlay : MonoBehaviour
    {
        [SerializeField] private RectTransform overlayRoot;
        [SerializeField] private RawImage showroomOutput;
        [SerializeField] private Button previousPageButton;
        [SerializeField] private Button nextPageButton;
        [SerializeField] private Button selectButton;
        [SerializeField] private Button closeButton;
        [SerializeField] private Text titleText;
        [SerializeField] private Text subtitleText;
        [SerializeField] private Text pageStatusText;
        [SerializeField] private Font fallbackFont;

        private RobotShowroomManager showroomManager;
        private Camera showroomCamera;
        private Light showroomLight;
        private RenderTexture showroomTexture;
        private Action<string> onSelected;

        public bool IsVisible => overlayRoot != null && overlayRoot.gameObject.activeSelf;

        public void Show(RectTransform parent, Font font, Action<string> onSelectedCallback)
        {
            fallbackFont = UiRuntimeStyle.ResolveFont(font);
            onSelected = onSelectedCallback;
            EnsurePresentation(parent);
            ConfigureShowroom();
            overlayRoot.gameObject.SetActive(true);
            overlayRoot.SetAsLastSibling();
            UpdateSelectionLabel(showroomManager != null ? showroomManager.GetCurrentHeroId() : string.Empty);
        }

        public void Hide()
        {
            if (overlayRoot != null)
            {
                overlayRoot.gameObject.SetActive(false);
            }
        }

        private void OnDestroy()
        {
            ReleaseTexture();
        }

        private void EnsurePresentation(RectTransform parent)
        {
            if (parent == null)
            {
                return;
            }

            overlayRoot ??= UiRuntimeStyle.EnsureRectChild(parent, "SandboxRobotPickerOverlay");
            UiRuntimeStyle.Stretch(overlayRoot, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

            var overlayBg = UiRuntimeStyle.EnsureImage(overlayRoot, "OverlayBg", UIDesignTokens.Colors.SurfaceOverlay);
            UiRuntimeStyle.Stretch((RectTransform)overlayBg.transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            overlayBg.raycastTarget = true;

            var panelRoot = UiRuntimeStyle.EnsureRectChild(overlayRoot, "PickerPanel");
            UiRuntimeStyle.Anchor(panelRoot, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(1040f, 620f), Vector2.zero);
            var panelBg = UiRuntimeStyle.EnsureImage(panelRoot, "PanelBg", UIDesignTokens.Colors.SurfaceRaised);
            UiRuntimeStyle.Stretch((RectTransform)panelBg.transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

            titleText = UiRuntimeStyle.EnsureText(panelRoot, "Title", fallbackFont, UIDesignTokens.Type.DisplaySm, FontStyle.Bold, TextAnchor.MiddleCenter, UIDesignTokens.Colors.TextPrimary);
            UiRuntimeStyle.Anchor(titleText.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(480f, 32f), new Vector2(0f, -24f));
            titleText.text = "Change Robot";

            subtitleText = UiRuntimeStyle.EnsureText(panelRoot, "Subtitle", fallbackFont, UIDesignTokens.Type.Body, FontStyle.Normal, TextAnchor.MiddleCenter, UIDesignTokens.Colors.TextSecondary);
            UiRuntimeStyle.Anchor(subtitleText.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(560f, 22f), new Vector2(0f, -58f));

            var viewportRoot = UiRuntimeStyle.EnsureRectChild(panelRoot, "ViewportRoot");
            UiRuntimeStyle.Anchor(viewportRoot, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(960f, 420f), new Vector2(0f, 18f));
            var viewportBg = UiRuntimeStyle.EnsureImage(viewportRoot, "ViewportBg", UIDesignTokens.Colors.SurfaceBase);
            UiRuntimeStyle.Stretch((RectTransform)viewportBg.transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

            showroomOutput = viewportRoot.Find("ShowroomOutput")?.GetComponent<RawImage>();
            if (showroomOutput == null)
            {
                var outputGo = new GameObject("ShowroomOutput", typeof(RectTransform), typeof(RawImage));
                outputGo.transform.SetParent(viewportRoot, false);
                showroomOutput = outputGo.GetComponent<RawImage>();
            }
            UiRuntimeStyle.Stretch((RectTransform)showroomOutput.transform, Vector2.zero, Vector2.one, new Vector2(12f, 12f), new Vector2(-12f, -12f));

            previousPageButton = EnsureButton(viewportRoot, "BtnPrevPage", "<", new Vector2(0f, 0.5f), new Vector2(44f, 44f), new Vector2(26f, 0f));
            nextPageButton = EnsureButton(viewportRoot, "BtnNextPage", ">", new Vector2(1f, 0.5f), new Vector2(44f, 44f), new Vector2(-26f, 0f));
            pageStatusText = UiRuntimeStyle.EnsureText(viewportRoot, "PageStatus", fallbackFont, UIDesignTokens.Type.Caption, FontStyle.Bold, TextAnchor.MiddleCenter, UIDesignTokens.Colors.TextSecondary);
            UiRuntimeStyle.Anchor(pageStatusText.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(100f, 20f), new Vector2(0f, -12f));

            selectButton = EnsureButton(panelRoot, "BtnSelectRobot", "Use This Robot", new Vector2(0.5f, 0f), new Vector2(220f, 40f), new Vector2(-90f, 24f));
            closeButton = EnsureButton(panelRoot, "BtnClosePicker", "Close", new Vector2(0.5f, 0f), new Vector2(140f, 40f), new Vector2(120f, 24f));

            EnsureShowroomRuntime();

            previousPageButton.onClick.RemoveAllListeners();
            previousPageButton.onClick.AddListener(() => showroomManager?.PreviousPage());
            nextPageButton.onClick.RemoveAllListeners();
            nextPageButton.onClick.AddListener(() => showroomManager?.NextPage());
            selectButton.onClick.RemoveAllListeners();
            selectButton.onClick.AddListener(SelectCurrentRobot);
            closeButton.onClick.RemoveAllListeners();
            closeButton.onClick.AddListener(Hide);

            overlayRoot.gameObject.SetActive(false);
        }

        private Button EnsureButton(Transform parent, string name, string label, Vector2 anchor, Vector2 size, Vector2 position)
        {
            var existing = parent.Find(name);
            var button = existing != null ? existing.GetComponent<Button>() : null;
            if (button == null)
            {
                var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
                go.transform.SetParent(parent, false);
                button = go.GetComponent<Button>();
            }

            UiRuntimeStyle.Anchor(button.transform as RectTransform, anchor, anchor, size, position);
            UiRuntimeStyle.EnsureButtonLabel(button, fallbackFont, label, UIDesignTokens.Colors.SurfaceRaisedAlt);
            return button;
        }

        private void EnsureShowroomRuntime()
        {
            if (showroomManager != null)
            {
                EnsureTexture();
                return;
            }

            var runtimeRoot = new GameObject("SandboxRobotPickerRuntime");
            runtimeRoot.transform.SetParent(transform, false);
            runtimeRoot.transform.position = new Vector3(0f, -1200f, 0f);

            showroomManager = runtimeRoot.AddComponent<RobotShowroomManager>();
            showroomManager.OnRobotSelected += UpdateSelectionLabel;
            showroomManager.OnPageChanged += HandlePageChanged;

            var cameraGo = new GameObject("PickerCamera");
            cameraGo.transform.SetParent(runtimeRoot.transform, false);
            cameraGo.transform.localPosition = new Vector3(0f, 1.1f, -4.5f);
            cameraGo.transform.localRotation = Quaternion.Euler(10f, 0f, 0f);
            showroomCamera = cameraGo.AddComponent<Camera>();
            showroomCamera.clearFlags = CameraClearFlags.SolidColor;
            showroomCamera.backgroundColor = UIDesignTokens.Colors.SurfaceBase;
            showroomCamera.fieldOfView = 28f;
            showroomCamera.nearClipPlane = 0.1f;
            showroomCamera.farClipPlane = 30f;

            var lightGo = new GameObject("PickerLight");
            lightGo.transform.SetParent(runtimeRoot.transform, false);
            lightGo.transform.localRotation = Quaternion.Euler(36f, -35f, 0f);
            showroomLight = lightGo.AddComponent<Light>();
            showroomLight.type = LightType.Directional;
            showroomLight.intensity = 1.25f;

            EnsureTexture();
        }

        private void EnsureTexture()
        {
            if (showroomOutput == null || showroomCamera == null)
            {
                return;
            }

            int width = 1024;
            int height = 512;
            bool needsNewTexture = showroomTexture == null || showroomTexture.width != width || showroomTexture.height != height;
            if (!needsNewTexture)
            {
                return;
            }

            ReleaseTexture();
            showroomTexture = new RenderTexture(width, height, 16, RenderTextureFormat.ARGB32)
            {
                name = "SandboxRobotPickerRT"
            };
            showroomTexture.Create();
            showroomCamera.targetTexture = showroomTexture;
            showroomOutput.texture = showroomTexture;
        }

        private void ReleaseTexture()
        {
            if (showroomCamera != null)
            {
                showroomCamera.targetTexture = null;
            }

            if (showroomOutput != null)
            {
                showroomOutput.texture = null;
            }

            if (showroomTexture != null)
            {
                showroomTexture.Release();
                SafeDestroy(showroomTexture);
                showroomTexture = null;
            }
        }

        private void ConfigureShowroom()
        {
            if (showroomManager == null)
            {
                return;
            }

            var selectedRobotId = RobotSelectionBridge.GetSelectedRobotId();
            if (string.IsNullOrEmpty(selectedRobotId))
            {
                selectedRobotId = "2DOF_RR";
            }

            showroomManager.Configure(new RobotShowroomContext(
                robotIds: RobotCatalog.GetAllRobotIds(),
                maxVisiblePods: 3,
                showLabels: true,
                showCtaButtons: true,
                allowOrbit: false,
                podSpacing: UIDesignTokens.Size.PodSpacing,
                heroRobotId: selectedRobotId,
                enablePaging: true,
                primaryCtaKind: RobotShowroomCtaKind.SelectOnly,
                secondaryCtaKind: RobotShowroomCtaKind.None));
        }

        private void UpdateSelectionLabel(string robotId)
        {
            if (subtitleText == null)
            {
                return;
            }

            if (RobotCatalog.TryGet(robotId, out var entry))
            {
                subtitleText.text = $"{entry.Metadata.DisplayName} · {entry.Metadata.Dof}DOF · {entry.Metadata.RobotType}";
            }
            else
            {
                subtitleText.text = string.Empty;
            }
        }

        private void HandlePageChanged(int currentPage, int totalPages)
        {
            if (pageStatusText != null)
            {
                pageStatusText.text = $"{currentPage}/{totalPages}";
            }

            if (previousPageButton != null)
            {
                previousPageButton.interactable = currentPage > 1;
            }

            if (nextPageButton != null)
            {
                nextPageButton.interactable = currentPage < totalPages;
            }
        }

        private void SelectCurrentRobot()
        {
            var selectedRobotId = showroomManager != null ? showroomManager.GetCurrentHeroId() : string.Empty;
            if (string.IsNullOrEmpty(selectedRobotId))
            {
                return;
            }

            onSelected?.Invoke(selectedRobotId);
            Hide();
        }

        private static void SafeDestroy(UnityEngine.Object target)
        {
            if (target == null)
            {
                return;
            }

            if (Application.isPlaying)
            {
                Destroy(target);
            }
            else
            {
                DestroyImmediate(target);
            }
        }
    }
}
