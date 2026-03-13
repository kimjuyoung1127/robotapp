// Folder: UI - HUD/view components only; no kinematics logic.
using KineTutor3D.App;
using KineTutor3D.Templates;
using KineTutor3D.Types;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace KineTutor3D.UI
{
    /// <summary>
    /// Robot Library 씬의 UI를 구성하고 관리합니다.
    /// 상단 55% 3D 쇼룸 뷰포트 + 하단 45% 카드 스크롤 레이아웃.
    /// </summary>
    [ExecuteAlways]
    public class RobotLibraryManager : MonoBehaviour
    {
        [SerializeField] private RectTransform canvasRoot;
        [SerializeField] private Font fallbackFont;

        private RectTransform topBar;
        private RectTransform showroomArea;
        private RectTransform gridContainer;
        private ScrollRect libraryScrollRect;
        private RobotDetailDrawer detailDrawer;
        private RawImage showroomOutput;
        private RobotShowroomManager showroomManager;
        private Button previousPageButton;
        private Button nextPageButton;
        private Text pageStatusText;
        private Camera showroomCamera;
        private Light showroomLight;
        private RenderTexture showroomTexture;
        private GameObject showroomRuntimeRoot;
        private bool showroomConfigured;
        private bool uiBuilt;

        private void Awake()
        {
            EnsurePresentation();
        }

        private void OnEnable()
        {
            EnsurePresentation();
        }

        private void OnDisable()
        {
            showroomConfigured = false;
            ReleaseShowroomTexture();
            ReleaseShowroomRuntime();
        }

        private void OnDestroy()
        {
            ReleaseShowroomTexture();
            ReleaseShowroomRuntime();
        }

        private void EnsurePresentation()
        {
            fallbackFont = UiRuntimeStyle.ResolveFont(fallbackFont);
            canvasRoot ??= transform as RectTransform;

            if (canvasRoot == null)
            {
                return;
            }

            if (!uiBuilt)
            {
                if (!TryBindStaticLayout())
                {
                    BuildTopBar();
                    BuildShowroomViewport();
                    BuildScrollGrid();
                    EnsureDetailDrawer();
                }
                else
                {
                    EnsureShowroomOutput(showroomArea);
                }

                uiBuilt = true;
            }

            if (Application.isPlaying)
            {
                EnsureShowroomRuntime();
                RebuildGrid();
                ConfigureShowroom();
                UpdateShowroomFraming();
            }
            else
            {
                RebuildGrid();
            }
        }

        private bool TryBindStaticLayout()
        {
            topBar = canvasRoot.Find("TopBar") as RectTransform;
            showroomArea = canvasRoot.Find("ShowroomArea") as RectTransform;
            showroomOutput = showroomArea != null ? showroomArea.Find("ShowroomOutput")?.GetComponent<RawImage>() : null;
            previousPageButton = showroomArea != null ? showroomArea.Find("BtnPrevPage")?.GetComponent<Button>() : null;
            nextPageButton = showroomArea != null ? showroomArea.Find("BtnNextPage")?.GetComponent<Button>() : null;
            pageStatusText = showroomArea != null ? showroomArea.Find("PageStatus")?.GetComponent<Text>() : null;
            var scrollArea = canvasRoot.Find("ScrollArea") as RectTransform;
            libraryScrollRect = scrollArea != null ? scrollArea.GetComponent<ScrollRect>() : null;
            var viewport = scrollArea != null ? scrollArea.Find("Viewport") as RectTransform : null;
            gridContainer = viewport != null ? viewport.Find("GridContent") as RectTransform : null;
            detailDrawer = GetComponentInChildren<RobotDetailDrawer>(true);

            if (topBar == null || showroomArea == null || showroomOutput == null || scrollArea == null || viewport == null || gridContainer == null || detailDrawer == null)
            {
                return false;
            }

            var backBtn = topBar.Find("BtnBack")?.GetComponent<Button>();
            if (backBtn != null)
            {
                backBtn.onClick.RemoveAllListeners();
                backBtn.onClick.AddListener(OnBackClicked);
            }

            detailDrawer.Initialize(canvasRoot, fallbackFont);
            RemoveCompareStrip();
            BuildShowroomOverlay(showroomArea);
            return true;
        }

        private void BuildTopBar()
        {
            topBar = UiRuntimeStyle.EnsureRectChild(canvasRoot, "TopBar");
            UiRuntimeStyle.Anchor(topBar, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, UIDesignTokens.Size.TopBarHeight), Vector2.zero);
            UiRuntimeStyle.Stretch(topBar, new Vector2(0f, 1f), Vector2.one, new Vector2(0f, -UIDesignTokens.Size.TopBarHeight), Vector2.zero);

            var bg = UiRuntimeStyle.EnsureImage(topBar, "TopBarBg", UIDesignTokens.Colors.SurfaceRaised);
            UiRuntimeStyle.Stretch((RectTransform)bg.transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

            var title = UiRuntimeStyle.EnsureText(topBar, "Title", fallbackFont, UIDesignTokens.Type.DisplaySm, FontStyle.Bold, TextAnchor.MiddleLeft, UIDesignTokens.Colors.TextPrimary);
            UiRuntimeStyle.Anchor(title.rectTransform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(300f, 40f), new Vector2(80f, 0f));
            title.text = "Robot Library";

            var backBtn = EnsureButton(topBar, "BtnBack", "< Back", new Vector2(0f, 0.5f), new Vector2(100f, UIDesignTokens.Size.ButtonHeightMd), new Vector2(16f, 0f), UIDesignTokens.Colors.SurfaceCard);
            backBtn.onClick.RemoveAllListeners();
            backBtn.onClick.AddListener(OnBackClicked);
        }

        private void BuildShowroomViewport()
        {
            showroomArea = UiRuntimeStyle.EnsureRectChild(canvasRoot, "ShowroomArea");
            float topBarBottom = UIDesignTokens.Size.TopBarHeight;
            float viewportRatio = UIDesignTokens.Size.ShowroomViewportRatio;

            // 상단 55%: TopBar 아래 ~ 중간
            showroomArea.anchorMin = new Vector2(0f, 1f - viewportRatio);
            showroomArea.anchorMax = Vector2.one;
            showroomArea.offsetMin = new Vector2(0f, 0f);
            showroomArea.offsetMax = new Vector2(0f, -topBarBottom);
            showroomArea.pivot = new Vector2(0.5f, 0.5f);

            var showroomBg = UiRuntimeStyle.EnsureImage(showroomArea, "ShowroomBg", UIDesignTokens.Colors.SurfaceBase);
            UiRuntimeStyle.Stretch((RectTransform)showroomBg.transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

            showroomOutput = EnsureShowroomOutput(showroomArea);
            RemoveCompareStrip();
            BuildShowroomOverlay(showroomArea);
            if (Application.isPlaying)
            {
                EnsureShowroomRuntime();
            }
        }

        private RawImage EnsureShowroomOutput(RectTransform parent)
        {
            var existing = parent.Find("ShowroomOutput");
            var output = existing != null ? existing.GetComponent<RawImage>() : null;
            if (output == null)
            {
                var go = new GameObject("ShowroomOutput", typeof(RectTransform), typeof(RawImage));
                go.transform.SetParent(parent, false);
                output = go.GetComponent<RawImage>();
            }

            UiRuntimeStyle.Stretch((RectTransform)output.transform, Vector2.zero, Vector2.one, UIDesignTokens.Space.Xs * Vector2.one, -UIDesignTokens.Space.Xs * Vector2.one);
            output.color = Color.white;
            output.raycastTarget = true;
            BindShowroomPointerClick(output);
            return output;
        }

        private void EnsureShowroomRuntime()
        {
            if (!Application.isPlaying)
            {
                return;
            }

            ReuseOrCleanupShowroomRuntime();

            if (showroomManager != null)
            {
                EnsureShowroomRig();
                UpdateShowroomFraming();
                EnsureShowroomTexture();
                return;
            }

            showroomRuntimeRoot = new GameObject("RobotShowroomRuntime");
            showroomRuntimeRoot.transform.position = new Vector3(0f, -1000f, 0f);

            showroomManager = showroomRuntimeRoot.AddComponent<RobotShowroomManager>();
            EnsureShowroomRig();
            UpdateShowroomFraming();
            EnsureShowroomTexture();
        }

        private void ReuseOrCleanupShowroomRuntime()
        {
            var runtimeManagers = FindObjectsByType<RobotShowroomManager>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            RobotShowroomManager keeper = showroomManager;
            foreach (var candidate in runtimeManagers)
            {
                if (candidate == null)
                {
                    continue;
                }

                if (keeper == null)
                {
                    keeper = candidate;
                    continue;
                }

                if (candidate == keeper)
                {
                    continue;
                }

                SafeDestroy(candidate.gameObject);
            }

            showroomManager = keeper;
            showroomRuntimeRoot = showroomManager != null ? showroomManager.gameObject : null;
        }

        private void EnsureShowroomRig()
        {
            if (showroomRuntimeRoot == null)
            {
                showroomRuntimeRoot = showroomManager != null ? showroomManager.gameObject : null;
            }

            if (showroomRuntimeRoot == null)
            {
                return;
            }

            var cameraTransform = showroomRuntimeRoot.transform.Find("ShowroomCamera");
            if (cameraTransform == null)
            {
                var cameraGo = new GameObject("ShowroomCamera");
                cameraGo.transform.SetParent(showroomRuntimeRoot.transform, false);
                cameraTransform = cameraGo.transform;
            }

            cameraTransform.localPosition = new Vector3(0f, 1.55f, -7.8f);
            cameraTransform.localRotation = Quaternion.Euler(8f, 0f, 0f);
            showroomCamera = cameraTransform.GetComponent<Camera>();
            if (showroomCamera == null)
            {
                showroomCamera = cameraTransform.gameObject.AddComponent<Camera>();
            }

            showroomCamera.clearFlags = CameraClearFlags.SolidColor;
            showroomCamera.backgroundColor = UIDesignTokens.Colors.SurfaceBase;
            showroomCamera.fieldOfView = 44f;
            showroomCamera.nearClipPlane = 0.1f;
            showroomCamera.farClipPlane = 30f;
            showroomCamera.allowHDR = false;
            showroomCamera.allowMSAA = true;

            var lightTransform = showroomRuntimeRoot.transform.Find("ShowroomLight");
            if (lightTransform == null)
            {
                var lightGo = new GameObject("ShowroomLight");
                lightGo.transform.SetParent(showroomRuntimeRoot.transform, false);
                lightTransform = lightGo.transform;
            }

            lightTransform.localRotation = Quaternion.Euler(36f, -35f, 0f);
            showroomLight = lightTransform.GetComponent<Light>();
            if (showroomLight == null)
            {
                showroomLight = lightTransform.gameObject.AddComponent<Light>();
            }

            showroomLight.type = LightType.Directional;
            showroomLight.intensity = 1.25f;
            showroomLight.color = Color.white;
        }

        private void EnsureShowroomTexture()
        {
            if (showroomOutput == null || showroomCamera == null)
            {
                return;
            }

            var outputRect = (RectTransform)showroomOutput.transform;
            float scaleFactor = showroomOutput.canvas != null ? showroomOutput.canvas.scaleFactor : 1f;
            int width = Mathf.Max(512, Mathf.RoundToInt(outputRect.rect.width * scaleFactor));
            int height = Mathf.Max(256, Mathf.RoundToInt(outputRect.rect.height * scaleFactor));
            bool needsNewTexture = showroomTexture == null || showroomTexture.width != width || showroomTexture.height != height;
            if (!needsNewTexture)
            {
                return;
            }

            ReleaseShowroomTexture();

            showroomTexture = new RenderTexture(width, height, 16, RenderTextureFormat.ARGB32)
            {
                name = "RobotShowroomRT"
            };
            showroomTexture.Create();

            showroomCamera.aspect = (float)width / height;
            showroomCamera.targetTexture = showroomTexture;
            showroomOutput.texture = showroomTexture;
        }

        private void UpdateShowroomFraming()
        {
            if (!Application.isPlaying || showroomCamera == null || showroomOutput == null)
            {
                return;
            }

            var outputRect = (RectTransform)showroomOutput.transform;
            float width = Mathf.Max(1f, outputRect.rect.width);
            float height = Mathf.Max(1f, outputRect.rect.height);
            float aspect = width / height;
            int visibleCount = showroomManager != null
                ? Mathf.Max(1, showroomManager.GetVisibleRobotIds().Length)
                : 3;

            const float verticalFov = 42f;
            float halfVerticalFovRad = verticalFov * 0.5f * Mathf.Deg2Rad;
            float halfHorizontalFovRad = Mathf.Atan(Mathf.Tan(halfVerticalFovRad) * aspect);

            float halfGroupWidth = ((visibleCount - 1) * UIDesignTokens.Size.PodSpacing * 0.5f) + 0.9f;
            float halfGroupHeight = 1.35f;
            float distanceForWidth = halfGroupWidth / Mathf.Max(0.1f, Mathf.Tan(halfHorizontalFovRad));
            float distanceForHeight = halfGroupHeight / Mathf.Max(0.1f, Mathf.Tan(halfVerticalFovRad));
            float distance = Mathf.Max(distanceForWidth, distanceForHeight) + 1.35f;
            Vector3 focusPoint = new Vector3(0f, 0.72f, 0f);
            Vector3 cameraPosition = focusPoint + new Vector3(0f, 0.52f, -distance);

            showroomCamera.fieldOfView = verticalFov;
            showroomCamera.aspect = aspect;
            showroomCamera.transform.localPosition = cameraPosition;
            showroomCamera.transform.localRotation = Quaternion.LookRotation(focusPoint - cameraPosition, Vector3.up);
        }

        private void ReleaseShowroomTexture()
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

        private void ReleaseShowroomRuntime()
        {
            if (showroomRuntimeRoot != null)
            {
                SafeDestroy(showroomRuntimeRoot);
            }

            showroomRuntimeRoot = null;
            showroomManager = null;
            showroomCamera = null;
            showroomLight = null;
            showroomConfigured = false;
        }

        private void ConfigureShowroom()
        {
            if (!Application.isPlaying || showroomManager == null)
            {
                return;
            }

            if (showroomConfigured && !string.IsNullOrEmpty(showroomManager.GetCurrentHeroId()))
            {
                return;
            }

            var allIds = RobotCatalog.GetAllRobotIds();
            var ctx = new RobotShowroomContext(
                robotIds: allIds,
                maxVisiblePods: 3,
                showLabels: true,
                showCtaButtons: true,
                allowOrbit: true,
                podSpacing: UIDesignTokens.Size.PodSpacing,
                enablePaging: true,
                primaryCtaKind: RobotShowroomCtaKind.GuidedLesson,
                secondaryCtaKind: RobotShowroomCtaKind.Sandbox);

            showroomManager.Configure(ctx);

            showroomManager.OnRobotSelected -= OnShowroomRobotSelected;
            showroomManager.OnRobotSelected += OnShowroomRobotSelected;
            showroomManager.OnPageChanged -= OnShowroomPageChanged;
            showroomManager.OnPageChanged += OnShowroomPageChanged;
            UpdateShowroomFraming();
            showroomConfigured = true;
        }

        private void OnShowroomRobotSelected(string robotId)
        {
            if (!RobotCatalog.TryGet(robotId, out var entry))
            {
                return;
            }

            if (detailDrawer != null && detailDrawer.IsVisible)
            {
                detailDrawer.Show(entry);
            }

        }

        private void OnShowroomPageChanged(int currentPage, int totalPages)
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

            UpdateShowroomFraming();
        }

        private void BuildScrollGrid()
        {
            var scrollArea = UiRuntimeStyle.EnsureRectChild(canvasRoot, "ScrollArea");
            float viewportRatio = UIDesignTokens.Size.ShowroomViewportRatio;

            // 하단 45%: 쇼룸 아래 ~ 바닥
            scrollArea.anchorMin = Vector2.zero;
            scrollArea.anchorMax = new Vector2(1f, 1f - viewportRatio);
            scrollArea.offsetMin = new Vector2(UIDesignTokens.Space.Md, UIDesignTokens.Space.Md);
            scrollArea.offsetMax = new Vector2(-UIDesignTokens.Space.Md, -UIDesignTokens.Space.Xs);
            scrollArea.pivot = new Vector2(0.5f, 0.5f);

            libraryScrollRect = scrollArea.GetComponent<ScrollRect>();
            if (libraryScrollRect == null)
            {
                libraryScrollRect = scrollArea.gameObject.AddComponent<ScrollRect>();
            }

            var viewport = UiRuntimeStyle.EnsureRectChild(scrollArea, "Viewport");
            UiRuntimeStyle.Stretch(viewport, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

            var viewportMask = viewport.GetComponent<Mask>();
            if (viewportMask == null)
            {
                viewportMask = viewport.gameObject.AddComponent<Mask>();
                viewportMask.showMaskGraphic = false;
            }

            var viewportImage = viewport.GetComponent<Image>();
            if (viewportImage == null)
            {
                viewportImage = viewport.gameObject.AddComponent<Image>();
                viewportImage.color = Color.clear;
            }

            gridContainer = UiRuntimeStyle.EnsureRectChild(viewport, "GridContent");
            UiRuntimeStyle.Anchor(gridContainer, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 0f), Vector2.zero);
            gridContainer.anchorMin = new Vector2(0f, 1f);
            gridContainer.anchorMax = new Vector2(1f, 1f);
            gridContainer.pivot = new Vector2(0f, 1f);

            var gridLayout = gridContainer.GetComponent<GridLayoutGroup>();
            if (gridLayout == null)
            {
                gridLayout = gridContainer.gameObject.AddComponent<GridLayoutGroup>();
            }

            gridLayout.cellSize = new Vector2(UIDesignTokens.Size.CardWidth, UIDesignTokens.Size.CardHeight);
            gridLayout.spacing = new Vector2(UIDesignTokens.Size.GridSpacing, UIDesignTokens.Size.GridSpacing);
            gridLayout.padding = new RectOffset(10, 10, 10, 10);
            gridLayout.startCorner = GridLayoutGroup.Corner.UpperLeft;
            gridLayout.startAxis = GridLayoutGroup.Axis.Horizontal;
            gridLayout.childAlignment = TextAnchor.UpperLeft;
            gridLayout.constraint = GridLayoutGroup.Constraint.Flexible;

            var fitter = gridContainer.GetComponent<ContentSizeFitter>();
            if (fitter == null)
            {
                fitter = gridContainer.gameObject.AddComponent<ContentSizeFitter>();
            }

            fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            libraryScrollRect.content = gridContainer;
            libraryScrollRect.viewport = viewport;
            libraryScrollRect.horizontal = false;
            libraryScrollRect.vertical = true;
        }

        private void EnsureDetailDrawer()
        {
            detailDrawer = GetComponentInChildren<RobotDetailDrawer>(true);
            if (detailDrawer == null)
            {
                var go = new GameObject("RobotDetailDrawer", typeof(RectTransform));
                go.transform.SetParent(canvasRoot, false);
                detailDrawer = go.AddComponent<RobotDetailDrawer>();
            }

            detailDrawer.Initialize(canvasRoot, fallbackFont);
        }

        private void RebuildGrid()
        {
            if (gridContainer == null)
            {
                return;
            }

            var staleChildren = new List<GameObject>(gridContainer.childCount);
            for (int i = gridContainer.childCount - 1; i >= 0; i--)
            {
                var child = gridContainer.GetChild(i).gameObject;
                staleChildren.Add(child);
                child.transform.SetParent(null, false);
            }

            foreach (var child in staleChildren)
            {
                SafeDestroy(child);
            }

            var entries = RobotCatalog.GetAll();
            foreach (var entry in entries)
            {
                var captured = entry;
                RobotCardBuilder.BuildCard(
                    gridContainer,
                    captured,
                    fallbackFont,
                    () => OnStartLesson(captured),
                    () => OnOpenSandbox(captured),
                    () => OnOpenRobotControl(captured),
                    () => OnCardSelected(captured));
            }

            LayoutRebuilder.ForceRebuildLayoutImmediate(gridContainer);
            Canvas.ForceUpdateCanvases();
            if (libraryScrollRect != null)
            {
                libraryScrollRect.normalizedPosition = new Vector2(0f, 1f);
                libraryScrollRect.velocity = Vector2.zero;
            }
        }

        private void OnCardSelected(RobotCatalogEntry entry)
        {
            if (showroomManager != null && Application.isPlaying)
            {
                showroomManager.SelectRobot(entry.Metadata.RobotId);
            }

            LaunchPrimaryExperience(entry);
        }

        private void BuildShowroomOverlay(RectTransform parent)
        {
            previousPageButton = EnsureButton(parent, "BtnPrevPage", "<", new Vector2(0f, 0.5f), new Vector2(44f, 44f), new Vector2(26f, 36f), UIDesignTokens.Colors.SurfaceRaisedAlt);
            previousPageButton.onClick.RemoveAllListeners();
            previousPageButton.onClick.AddListener(OnPreviousPageClicked);

            nextPageButton = EnsureButton(parent, "BtnNextPage", ">", new Vector2(1f, 0.5f), new Vector2(44f, 44f), new Vector2(-26f, 36f), UIDesignTokens.Colors.SurfaceRaisedAlt);
            nextPageButton.onClick.RemoveAllListeners();
            nextPageButton.onClick.AddListener(OnNextPageClicked);

            pageStatusText = UiRuntimeStyle.EnsureText(parent, "PageStatus", fallbackFont, UIDesignTokens.Type.Caption, FontStyle.Bold, TextAnchor.MiddleCenter, UIDesignTokens.Colors.TextSecondary);
            UiRuntimeStyle.Anchor(pageStatusText.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(100f, 20f), new Vector2(0f, -UIDesignTokens.Space.Sm));
            pageStatusText.text = "1/1";

        }

        private void OnPreviousPageClicked()
        {
            showroomManager?.PreviousPage();
        }

        private void OnNextPageClicked()
        {
            showroomManager?.NextPage();
        }

        private void OnStartLesson(RobotCatalogEntry entry)
        {
            if (!RobotCatalog.HasTemplate(entry.Metadata.RobotId))
            {
                return;
            }

            RobotSelectionBridge.SetSelection(entry.Metadata.RobotId, RobotSelectionBridge.GuidedLessonMode);
            SceneNavigator.Load(SceneId.Main);
        }

        private void OnOpenSandbox(RobotCatalogEntry entry)
        {
            if (!RobotCatalog.HasTemplate(entry.Metadata.RobotId) || !entry.Metadata.SandboxSupported)
            {
                return;
            }

            RobotSelectionBridge.SetSelection(entry.Metadata.RobotId, RobotSelectionBridge.SandboxMode);
            SceneNavigator.Load(SceneId.Sandbox);
        }

        private void OnOpenRobotControl(RobotCatalogEntry entry)
        {
            if (entry == null || !RobotCatalog.HasTemplate(entry.Metadata.RobotId) || !SupportsRobotControl(entry))
            {
                return;
            }

            RobotSelectionBridge.SetSelection(entry.Metadata.RobotId, RobotSelectionBridge.RobotControlMode);
            SceneNavigator.Load(SceneId.RobotControl);
        }

        private void LaunchPrimaryExperience(RobotCatalogEntry entry)
        {
            if (entry == null)
            {
                return;
            }

            if (RobotCatalog.HasTemplate(entry.Metadata.RobotId) && entry.Metadata.GuidedLessonSupported)
            {
                OnStartLesson(entry);
                return;
            }

             if (RobotCatalog.HasTemplate(entry.Metadata.RobotId) && SupportsRobotControl(entry))
            {
                OnOpenRobotControl(entry);
                return;
            }

            if (RobotCatalog.HasTemplate(entry.Metadata.RobotId) && entry.Metadata.SandboxSupported)
            {
                OnOpenSandbox(entry);
            }
        }

        private void OnViewDetails(RobotCatalogEntry entry)
        {
            if (detailDrawer != null)
            {
                detailDrawer.Show(entry);
            }
        }

        private void OnBackClicked()
        {
            SceneNavigator.Load(SceneId.Home);
        }

        private void RemoveCompareStrip()
        {
            if (showroomArea == null)
            {
                return;
            }

            var compareStrip = showroomArea.Find("CompareStrip");
            if (compareStrip != null)
            {
                SafeDestroy(compareStrip.gameObject);
            }
        }

        private void BindShowroomPointerClick(RawImage output)
        {
            if (output == null)
            {
                return;
            }

            var trigger = output.GetComponent<EventTrigger>();
            if (trigger == null)
            {
                trigger = output.gameObject.AddComponent<EventTrigger>();
            }

            trigger.triggers ??= new List<EventTrigger.Entry>();
            trigger.triggers.RemoveAll(entry => entry != null && entry.eventID == EventTriggerType.PointerClick);

            var clickEntry = new EventTrigger.Entry
            {
                eventID = EventTriggerType.PointerClick
            };
            clickEntry.callback.AddListener(OnShowroomPointerClick);
            trigger.triggers.Add(clickEntry);
        }

        private void OnShowroomPointerClick(BaseEventData eventData)
        {
            if (!Application.isPlaying || showroomCamera == null || showroomOutput == null)
            {
                return;
            }

            if (!(eventData is PointerEventData pointerEventData))
            {
                return;
            }

            var outputRect = showroomOutput.rectTransform;
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(outputRect, pointerEventData.position, pointerEventData.pressEventCamera, out var localPoint))
            {
                return;
            }

            var rect = outputRect.rect;
            float normalizedX = Mathf.InverseLerp(rect.xMin, rect.xMax, localPoint.x);
            float normalizedY = Mathf.InverseLerp(rect.yMin, rect.yMax, localPoint.y);
            var ray = showroomCamera.ViewportPointToRay(new Vector3(normalizedX, normalizedY, 0f));
            if (!Physics.Raycast(ray, out var hit, 100f))
            {
                return;
            }

            var pod = hit.collider != null ? hit.collider.GetComponentInParent<Visualization.RobotPreviewPod>() : null;
            if (pod == null || string.IsNullOrWhiteSpace(pod.RobotId))
            {
                return;
            }

            showroomManager?.SelectRobot(pod.RobotId);
            if (RobotCatalog.TryGet(pod.RobotId, out var entry))
            {
                LaunchPrimaryExperience(entry);
            }
        }

        private Button EnsureButton(Transform parent, string name, string label, Vector2 anchor, Vector2 size, Vector2 position, Color background)
        {
            var existing = parent.Find(name);
            var button = existing != null ? existing.GetComponent<Button>() : null;
            if (button == null)
            {
                var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
                go.transform.SetParent(parent, false);
                button = go.GetComponent<Button>();
            }

            var rect = (RectTransform)button.transform;
            UiRuntimeStyle.Anchor(rect, anchor, anchor, size, position);
            UiRuntimeStyle.EnsureButtonLabel(button, fallbackFont, label, background);
            return button;
        }

        private static void SafeDestroy(Object target)
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

        private static bool SupportsRobotControl(RobotCatalogEntry entry)
        {
            if (entry == null || entry.Metadata.SupportedLessons == null)
            {
                return false;
            }

            for (var i = 0; i < entry.Metadata.SupportedLessons.Length; i++)
            {
                if (string.Equals(entry.Metadata.SupportedLessons[i], "RobotControl", System.StringComparison.Ordinal))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
