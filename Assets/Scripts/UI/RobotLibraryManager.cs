// Folder: UI - HUD/view components only; no kinematics logic.
using KineTutor3D.App;
using KineTutor3D.Templates;
using KineTutor3D.Types;
using UnityEngine;
using UnityEngine.UI;

namespace KineTutor3D.UI
{
    /// <summary>
    /// Robot Library 씬의 UI를 구성하고 관리합니다.
    /// </summary>
    [ExecuteAlways]
    public class RobotLibraryManager : MonoBehaviour
    {
        [SerializeField] private RectTransform canvasRoot;
        [SerializeField] private Font fallbackFont;

        private RectTransform topBar;
        private RectTransform gridContainer;
        private RobotDetailDrawer detailDrawer;
        private bool uiBuilt;

        private void Awake()
        {
            EnsurePresentation();
        }

        private void OnEnable()
        {
            EnsurePresentation();
        }

        private void EnsurePresentation()
        {
            fallbackFont = UiRuntimeStyle.ResolveFont(fallbackFont);
            canvasRoot ??= transform as RectTransform;

            if (canvasRoot == null)
            {
                return;
            }

            if (uiBuilt)
            {
                return;
            }

            BuildTopBar();
            BuildScrollGrid();
            EnsureDetailDrawer();
            RebuildGrid();
            uiBuilt = true;
        }

        private void BuildTopBar()
        {
            topBar = UiRuntimeStyle.EnsureRectChild(canvasRoot, "TopBar");
            UiRuntimeStyle.Anchor(topBar, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, 60f), Vector2.zero);
            UiRuntimeStyle.Stretch(topBar, new Vector2(0f, 1f), Vector2.one, new Vector2(0f, -60f), Vector2.zero);

            var bg = UiRuntimeStyle.EnsureImage(topBar, "TopBarBg", UiRuntimeStyle.PanelBackground);
            UiRuntimeStyle.Stretch((RectTransform)bg.transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

            var title = UiRuntimeStyle.EnsureText(topBar, "Title", fallbackFont, 24, FontStyle.Bold, TextAnchor.MiddleLeft, UiRuntimeStyle.TextPrimary);
            UiRuntimeStyle.Anchor(title.rectTransform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(300f, 40f), new Vector2(80f, 0f));
            title.text = "Robot Library";

            var backBtn = EnsureButton(topBar, "BtnBack", "< Back", new Vector2(0f, 0.5f), new Vector2(100f, 36f), new Vector2(16f, 0f), UiRuntimeStyle.CardBackground);
            backBtn.onClick.RemoveAllListeners();
            backBtn.onClick.AddListener(OnBackClicked);
        }

        private void BuildScrollGrid()
        {
            var scrollArea = UiRuntimeStyle.EnsureRectChild(canvasRoot, "ScrollArea");
            UiRuntimeStyle.Stretch(scrollArea, Vector2.zero, Vector2.one, new Vector2(20f, 20f), new Vector2(-20f, -70f));

            var scrollRect = scrollArea.GetComponent<ScrollRect>();
            if (scrollRect == null)
            {
                scrollRect = scrollArea.gameObject.AddComponent<ScrollRect>();
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

            gridLayout.cellSize = new Vector2(280f, 220f);
            gridLayout.spacing = new Vector2(20f, 20f);
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

            scrollRect.content = gridContainer;
            scrollRect.viewport = viewport;
            scrollRect.horizontal = false;
            scrollRect.vertical = true;
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

            for (int i = gridContainer.childCount - 1; i >= 0; i--)
            {
                var child = gridContainer.GetChild(i).gameObject;
                if (Application.isPlaying)
                {
                    Destroy(child);
                }
                else
                {
                    DestroyImmediate(child);
                }
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
                    () => OnViewDetails(captured));
            }
        }

        private void OnStartLesson(RobotCatalogEntry entry)
        {
            if (!RobotCatalog.HasTemplate(entry.Metadata.RobotId))
            {
                return;
            }

            RobotSelectionBridge.SetSelectedRobot(entry.Metadata.RobotId);
            SceneNavigator.Load(SceneId.Main);
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
            SceneNavigator.Load(SceneId.Onboarding);
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
    }
}
