// Folder: UI - HUD/view components only; no kinematics logic.
using KineTutor3D.App;
using KineTutor3D.Templates;
using KineTutor3D.Types;
using System;
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
        private RobotLibrarySelectionPanel selectionPanel;
        private RawImage showroomOutput;
        private RobotShowroomManager showroomManager;
        private Button previousPageButton;
        private Button nextPageButton;
        private Text pageStatusText;
        private Camera showroomCamera;
        private Light showroomLight;
        private RenderTexture showroomTexture;
        private GameObject showroomRuntimeRoot;
        private readonly Dictionary<string, double[]> previewPoseByRobotId = new Dictionary<string, double[]>();
        private RobotCatalogEntry selectedEntry;
        private bool useSceneAuthoredGridLayout;
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

                EnsureSelectionPanel();
                uiBuilt = true;
            }
            else
            {
                EnsureSelectionPanel();
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
            libraryScrollRect = null;
            gridContainer = null;
            detailDrawer = GetComponentInChildren<RobotDetailDrawer>(true);
            selectionPanel = GetComponentInChildren<RobotLibrarySelectionPanel>(true);

            if (topBar == null || showroomArea == null || showroomOutput == null || detailDrawer == null)
            {
                return false;
            }

            var backBtn = topBar.Find("BtnBack")?.GetComponent<Button>();
            if (backBtn != null)
            {
                backBtn.onClick.RemoveAllListeners();
                backBtn.onClick.AddListener(OnBackClicked);
            }

            CleanupLegacyGridUi(scrollArea);

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

            const float verticalFov = 36f;
            float halfVerticalFovRad = verticalFov * 0.5f * Mathf.Deg2Rad;
            float halfHorizontalFovRad = Mathf.Atan(Mathf.Tan(halfVerticalFovRad) * aspect);

            float halfGroupWidth = ((visibleCount - 1) * UIDesignTokens.Size.PodSpacing * 0.5f) + 0.65f;
            float halfGroupHeight = 1.1f;
            float distanceForWidth = halfGroupWidth / Mathf.Max(0.1f, Mathf.Tan(halfHorizontalFovRad));
            float distanceForHeight = halfGroupHeight / Mathf.Max(0.1f, Mathf.Tan(halfVerticalFovRad));
            float distance = Mathf.Max(distanceForWidth, distanceForHeight) + 0.7f;
            Vector3 focusPoint = new Vector3(0f, 0.82f, 0f);
            Vector3 cameraPosition = focusPoint + new Vector3(0f, 0.42f, -distance);

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

            var allIds = RobotCatalog.GetRobotLibraryIds();
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

            showroomManager.OnRobotSelected -= OnShowroomRobotSelected;
            showroomManager.OnRobotSelected += OnShowroomRobotSelected;
            showroomManager.OnPageChanged -= OnShowroomPageChanged;
            showroomManager.OnPageChanged += OnShowroomPageChanged;
            showroomManager.Configure(ctx);
            if (!string.IsNullOrEmpty(showroomManager.GetCurrentHeroId()))
            {
                OnShowroomRobotSelected(showroomManager.GetCurrentHeroId());
            }
            UpdateShowroomFraming();
            showroomConfigured = true;
        }

        private void OnShowroomRobotSelected(string robotId)
        {
            if (!RobotCatalog.TryGet(robotId, out var entry))
            {
                return;
            }

            selectedEntry = entry;
            RefreshSelectionPanel();
            RebuildGrid();
            if (detailDrawer != null && detailDrawer.IsVisible)
            {
                if (entry.LibraryInteractionMode == LibraryInteractionMode.SelectOnly)
                {
                    detailDrawer.Hide();
                }
                else
                {
                    detailDrawer.Show(entry);
                }
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
            CleanupLegacyGridUi();
        }

        private void CleanupLegacyGridUi(RectTransform scrollArea = null)
        {
            libraryScrollRect = null;
            gridContainer = null;
            useSceneAuthoredGridLayout = false;

            if (canvasRoot == null)
            {
                return;
            }

            scrollArea ??= canvasRoot.Find("ScrollArea") as RectTransform;
            if (scrollArea != null)
            {
                scrollArea.SetParent(null, false);
                SafeDestroy(scrollArea.gameObject);
            }

            var viewport = canvasRoot.Find("Viewport");
            if (viewport != null)
            {
                viewport.SetParent(null, false);
                SafeDestroy(viewport.gameObject);
            }

            var grid = canvasRoot.Find("GridContent");
            if (grid != null)
            {
                grid.SetParent(null, false);
                SafeDestroy(grid.gameObject);
            }
        }

        private void ConfigureScrollViewport(RectTransform viewport)
        {
            if (viewport == null)
            {
                return;
            }

            UiRuntimeStyle.Stretch(viewport, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

            var stencilMask = viewport.GetComponent<Mask>();
            if (stencilMask != null)
            {
                SafeDestroy(stencilMask);
            }

            var rectMask = viewport.GetComponent<RectMask2D>();
            if (rectMask == null)
            {
                rectMask = viewport.gameObject.AddComponent<RectMask2D>();
            }

            rectMask.padding = Vector4.zero;
            rectMask.softness = Vector2Int.zero;

            var viewportImage = viewport.GetComponent<Image>();
            if (viewportImage == null)
            {
                viewportImage = viewport.gameObject.AddComponent<Image>();
            }

            viewportImage.color = Color.clear;
            viewportImage.raycastTarget = true;
        }

        private RectTransform EnsureGridContainer(RectTransform viewport)
        {
            if (viewport == null)
            {
                return null;
            }

            var existing = viewport.Find("GridContent") as RectTransform;
            if (existing != null)
            {
                return existing;
            }

            var go = new GameObject("GridContent", typeof(RectTransform));
            go.transform.SetParent(viewport, false);
            return go.GetComponent<RectTransform>();
        }

        private RectTransform RecreateGridContainer(RectTransform viewport)
        {
            if (viewport == null)
            {
                return null;
            }

            var existing = viewport.Find("GridContent") as RectTransform;
            if (existing != null)
            {
                existing.name = "GridContent_Deleted";
                existing.SetParent(null, false);
                SafeDestroy(existing.gameObject);
            }

            return EnsureGridContainer(viewport);
        }

        private void ConfigureGridContainer(RectTransform content)
        {
            if (content == null)
            {
                return;
            }

            content.anchorMin = new Vector2(0.5f, 0.5f);
            content.anchorMax = new Vector2(0.5f, 0.5f);
            content.pivot = new Vector2(0.5f, 0.5f);
            content.anchoredPosition = Vector2.zero;
            content.sizeDelta = Vector2.zero;
            content.localScale = Vector3.one;
        }

        private Vector2 ConfigureGridLayout(RectTransform content, int itemCount)
        {
            if (content == null)
            {
                return new Vector2(UIDesignTokens.Size.CardWidth, UIDesignTokens.Size.CardHeight);
            }

            var gridLayout = content.GetComponent<GridLayoutGroup>();
            if (gridLayout == null)
            {
                gridLayout = content.gameObject.AddComponent<GridLayoutGroup>();
            }

            Canvas.ForceUpdateCanvases();

            Rect viewportRect = libraryScrollRect != null && libraryScrollRect.viewport != null
                ? libraryScrollRect.viewport.rect
                : content.parent is RectTransform parentRect
                    ? parentRect.rect
                    : new Rect(0f, 0f, UIDesignTokens.Size.CardWidth * 2f, UIDesignTokens.Size.CardHeight * 3f);

            int columnCount = Mathf.Max(1, Mathf.Min(3, itemCount));
            int totalRows = Mathf.Max(1, Mathf.CeilToInt(itemCount / (float)columnCount));
            int visibleRows = Mathf.Min(2, totalRows);
            float horizontalPadding = Mathf.Clamp(viewportRect.width * 0.014f, 10f, 16f);
            float verticalPadding = Mathf.Clamp(viewportRect.height * 0.014f, 8f, 14f);
            float spacingX = Mathf.Clamp(viewportRect.width * 0.014f, 12f, 18f);
            float spacingY = Mathf.Clamp(viewportRect.height * 0.014f, 10f, 16f);
            float availableWidth = Mathf.Max(UIDesignTokens.Size.CardWidth, viewportRect.width - horizontalPadding * 2f - spacingX * (columnCount - 1));
            float availableHeight = Mathf.Max(UIDesignTokens.Size.CardHeight, viewportRect.height - verticalPadding * 2f - spacingY * (visibleRows - 1));
            float cellWidth = Mathf.Floor(availableWidth / columnCount);
            float cellHeight = Mathf.Floor(availableHeight / visibleRows);
            cellHeight = Mathf.Clamp(cellHeight, 220f, 340f);
            cellWidth = Mathf.Clamp(cellWidth, 170f, 360f);

            gridLayout.cellSize = new Vector2(cellWidth, cellHeight);
            gridLayout.spacing = new Vector2(spacingX, spacingY);
            gridLayout.padding = new RectOffset(
                Mathf.RoundToInt(horizontalPadding),
                Mathf.RoundToInt(horizontalPadding),
                Mathf.RoundToInt(verticalPadding),
                Mathf.RoundToInt(verticalPadding));
            gridLayout.startCorner = GridLayoutGroup.Corner.UpperLeft;
            gridLayout.startAxis = GridLayoutGroup.Axis.Horizontal;
            gridLayout.childAlignment = TextAnchor.UpperLeft;
            gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            gridLayout.constraintCount = columnCount;

            var fitter = content.GetComponent<ContentSizeFitter>();
            if (fitter != null)
            {
                SafeDestroy(fitter);
            }

            int rowCount = Mathf.Max(1, Mathf.CeilToInt(itemCount / (float)columnCount));
            float contentWidth = gridLayout.padding.left + gridLayout.padding.right + (cellWidth * columnCount) + (spacingX * Mathf.Max(0, columnCount - 1));
            float contentHeight = gridLayout.padding.top + gridLayout.padding.bottom + (cellHeight * rowCount) + (spacingY * Mathf.Max(0, rowCount - 1));
            content.sizeDelta = new Vector2(contentWidth, contentHeight);
            return gridLayout.cellSize;
        }

        private void ApplyBottomLayout(RectTransform scrollArea)
        {
            if (scrollArea == null)
            {
                return;
            }

            float leftWidth = UILayoutProfile.IsTablet ? 0.58f : 0.66f;
            scrollArea.anchorMin = Vector2.zero;
            scrollArea.anchorMax = new Vector2(leftWidth, 1f - UIDesignTokens.Size.ShowroomViewportRatio);
            scrollArea.offsetMin = new Vector2(UIDesignTokens.Space.Md, UIDesignTokens.Space.Md);
            scrollArea.offsetMax = new Vector2(-UIDesignTokens.Space.Xs, -UIDesignTokens.Space.Xs);
        }

        private void ApplySelectionPanelLayout(RectTransform panelRect)
        {
            if (panelRect == null)
            {
                return;
            }

            var canvasWidth = canvasRoot != null && canvasRoot.rect.width > 1f ? canvasRoot.rect.width : 1920f;
            float panelWidthRatio = UILayoutProfile.IsTablet ? 0.42f : 0.34f;
            float panelWidth = Mathf.Clamp(canvasWidth * panelWidthRatio, 420f, UILayoutProfile.IsTablet ? 640f : 720f);
            float lowerBandTop = 1f - UIDesignTokens.Size.ShowroomViewportRatio;

            panelRect.anchorMin = new Vector2(0.5f, 0f);
            panelRect.anchorMax = new Vector2(0.5f, lowerBandTop);
            panelRect.offsetMin = new Vector2(-panelWidth * 0.5f, UIDesignTokens.Space.Md);
            panelRect.offsetMax = new Vector2(panelWidth * 0.5f, -UIDesignTokens.Space.Xs);
            panelRect.anchoredPosition = Vector2.zero;
            panelRect.pivot = new Vector2(0.5f, 0.5f);
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

        private void EnsureSelectionPanel()
        {
            selectionPanel = GetComponentInChildren<RobotLibrarySelectionPanel>(true);
            if (selectionPanel == null)
            {
                var go = new GameObject("RobotLibrarySelectionPanel", typeof(RectTransform), typeof(Image));
                go.transform.SetParent(canvasRoot, false);
                selectionPanel = go.AddComponent<RobotLibrarySelectionPanel>();
            }

            var panelRect = selectionPanel.transform as RectTransform;
            if (panelRect != null)
            {
                ApplySelectionPanelLayout(panelRect);
            }

            selectionPanel.Initialize(canvasRoot, fallbackFont);
            selectionPanel.OnGuidedLessonRequested -= OnSelectionGuidedLessonRequested;
            selectionPanel.OnGuidedLessonRequested += OnSelectionGuidedLessonRequested;
            selectionPanel.OnSandboxRequested -= OnSelectionSandboxRequested;
            selectionPanel.OnSandboxRequested += OnSelectionSandboxRequested;
            selectionPanel.OnRobotControlRequested -= OnSelectionRobotControlRequested;
            selectionPanel.OnRobotControlRequested += OnSelectionRobotControlRequested;

            if (selectedEntry != null)
            {
                RefreshSelectionPanel();
            }
            else
            {
                EnsureDefaultSelectionEntry();
                RefreshSelectionPanel();
            }
        }

        private void RebuildGrid()
        {
            CleanupLegacyGridUi();
        }

        private static Vector2 ReadCurrentCardSize(RectTransform content)
        {
            if (content == null)
            {
                return new Vector2(UIDesignTokens.Size.CardWidth, UIDesignTokens.Size.CardHeight);
            }

            var gridLayout = content.GetComponent<GridLayoutGroup>();
            return gridLayout != null ? gridLayout.cellSize : new Vector2(UIDesignTokens.Size.CardWidth, UIDesignTokens.Size.CardHeight);
        }

        private static bool IsGridContainerInvalid(RectTransform content)
        {
            if (content == null)
            {
                return true;
            }

            if (content.anchorMin != new Vector2(0.5f, 0.5f) || content.anchorMax != new Vector2(0.5f, 0.5f))
            {
                return true;
            }

            if (content.pivot != new Vector2(0.5f, 0.5f))
            {
                return true;
            }

            if (content.rect.width <= 1f || content.rect.height <= 1f)
            {
                return true;
            }

            var fitter = content.GetComponent<ContentSizeFitter>();
            if (fitter != null && fitter.enabled)
            {
                return true;
            }

            return false;
        }

        private void OnCardSelected(RobotCatalogEntry entry)
        {
            SelectRobot(entry, true);
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

        private void SelectRobot(RobotCatalogEntry entry, bool syncShowroom)
        {
            if (entry == null)
            {
                return;
            }

            selectedEntry = entry;
            if (syncShowroom && showroomManager != null && Application.isPlaying)
            {
                showroomManager.SelectRobot(entry.Metadata.RobotId);
            }

            RefreshSelectionPanel();
            RebuildGrid();
        }

        private void RefreshSelectionPanel()
        {
            if (selectionPanel == null)
            {
                return;
            }

            EnsureDefaultSelectionEntry();

            if (selectedEntry == null)
            {
                selectionPanel.ShowRobot(null);
                return;
            }

            selectionPanel.ShowRobot(selectedEntry);
            ApplyPreviewPose(selectedEntry.Metadata.RobotId);
        }

        private void EnsureDefaultSelectionEntry()
        {
            if (selectedEntry != null)
            {
                return;
            }

            if (RobotCatalog.TryGet("SCARA_RV", out var scaraEntry))
            {
                selectedEntry = scaraEntry;
                return;
            }

            var entries = RobotCatalog.GetRobotLibraryEntries();
            if (entries != null && entries.Length > 0)
            {
                selectedEntry = entries[0];
            }
        }

        private double[] GetOrCreatePreviewPose(RobotCatalogEntry entry)
        {
            if (entry == null)
            {
                return System.Array.Empty<double>();
            }

            var robotId = entry.Metadata.RobotId;
            if (!previewPoseByRobotId.TryGetValue(robotId, out var pose) || pose == null || pose.Length == 0)
            {
                pose = CreateInitialPreviewPose(entry.Metadata);
                previewPoseByRobotId[robotId] = pose;
            }

            return (double[])pose.Clone();
        }

        private static double[] CreateInitialPreviewPose(RobotMetadataInfo metadata)
        {
            if (metadata.DemoPoseDeg != null && metadata.DemoPoseDeg.Length > 0)
            {
                return (double[])metadata.DemoPoseDeg.Clone();
            }

            if (metadata.HomePoseDeg != null && metadata.HomePoseDeg.Length > 0)
            {
                return (double[])metadata.HomePoseDeg.Clone();
            }

            return new double[Mathf.Max(1, metadata.Dof)];
        }

        private void ApplyPreviewPose(string robotId)
        {
            if (string.IsNullOrWhiteSpace(robotId) || showroomManager == null || !showroomManager.TryGetPod(robotId, out var pod))
            {
                return;
            }

            if (previewPoseByRobotId.TryGetValue(robotId, out var pose) && pose != null)
            {
                pod.SetPose(pose);
            }
        }

        private void OnStartLesson(RobotCatalogEntry entry)
        {
            if (!RobotCatalog.HasTemplate(entry.Metadata.RobotId))
            {
                return;
            }

            RobotSelectionBridge.SetSelection(entry.Metadata.RobotId, RobotSelectionBridge.GuidedLessonMode);
            SceneNavigator.Load(SceneId.Sandbox);
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
            if (entry == null
                || entry.LibraryInteractionMode == LibraryInteractionMode.SelectOnly
                || !RobotCatalog.HasTemplate(entry.Metadata.RobotId)
                || !SupportsRobotControl(entry))
            {
                return;
            }

            var targetScene = RobotControlScenePreference.GetPreferredSceneId();
            RobotControlEntryPolicy.Apply(targetScene, RobotControlEntryPolicy.Intent.ResumeLastSession);
            RobotSelectionBridge.SetSelection(entry.Metadata.RobotId, RobotSelectionBridge.RobotControlMode);
            SceneNavigator.Load(targetScene);
        }

        private void LaunchPrimaryExperience(RobotCatalogEntry entry)
        {
            if (entry == null || entry.LibraryInteractionMode == LibraryInteractionMode.SelectOnly)
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
            if (entry == null || entry.LibraryInteractionMode == LibraryInteractionMode.SelectOnly)
            {
                detailDrawer?.Hide();
                return;
            }

            if (detailDrawer != null)
            {
                detailDrawer.Show(entry);
            }
        }

        private void OnBackClicked()
        {
            SceneNavigator.Load(SceneId.Onboarding);
        }

        private void OnSelectionGuidedLessonRequested()
        {
            if (selectedEntry != null)
            {
                OnStartLesson(selectedEntry);
            }
        }

        private void OnSelectionSandboxRequested()
        {
            if (selectedEntry != null)
            {
                OnOpenSandbox(selectedEntry);
            }
        }

        private void OnSelectionRobotControlRequested()
        {
            if (selectedEntry != null)
            {
                OnOpenRobotControl(selectedEntry);
            }
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
                SelectRobot(entry, false);
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
