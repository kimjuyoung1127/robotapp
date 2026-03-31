// Folder: UI - HUD/view components only; no kinematics logic.
using System.Text;
using KineTutor3D.UI.Data;
using UnityEngine;
using UnityEngine.UI;

namespace KineTutor3D.UI
{
    /// <summary>
    /// 용어 사전 패널 표시와 쉬운/수학 설명 모드 전환을 담당합니다.
    /// </summary>
    [ExecuteAlways]
    public class GlossaryPanelController : MonoBehaviour, IVisibilityControllable
    {
        [SerializeField] private GameObject panelRoot;
        [SerializeField] private Button openButton;
        [SerializeField] private Button closeButton;
        [SerializeField] private Button easyModeButton;
        [SerializeField] private Button mathModeButton;
        [SerializeField] private InputField searchField;
        [SerializeField] private Text contentText;
        [SerializeField] private GlossaryDatabase glossaryDatabase;

        private bool mathMode;

        private void Awake()
        {
            AutoWire();
            LoadGlossaryIfNeeded();

            if (openButton != null) openButton.onClick.AddListener(Open);
            if (closeButton != null) closeButton.onClick.AddListener(Close);
            if (easyModeButton != null) easyModeButton.onClick.AddListener(() => SetMode(false));
            if (mathModeButton != null) mathModeButton.onClick.AddListener(() => SetMode(true));
            if (searchField != null) searchField.onValueChanged.AddListener(_ => Refresh());

            Close();
        }

        private void OnEnable()
        {
            AutoWire();
            Close();
        }

        public void Open()
        {
            if (panelRoot != null) panelRoot.SetActive(true);
            Refresh();
        }

        public void Close()
        {
            if (panelRoot != null) panelRoot.SetActive(false);
        }

        /// <summary>
        /// 패널 가시성을 설정합니다.
        /// </summary>
        public void SetVisible(bool visible)
        {
            if (visible) Open();
            else Close();
        }

        private void SetMode(bool useMath)
        {
            mathMode = useMath;
            Refresh();
        }

        private void Refresh()
        {
            if (contentText == null || glossaryDatabase == null || glossaryDatabase.entries == null)
            {
                return;
            }

            var keyword = searchField != null ? (searchField.text ?? string.Empty).Trim() : string.Empty;
            var keywordLower = keyword.ToLowerInvariant();
            var sb = new StringBuilder();

            foreach (var entry in glossaryDatabase.entries)
            {
                if (entry == null) continue;

                var symbol = entry.symbol ?? string.Empty;
                var koreanName = entry.koreanName ?? string.Empty;

                if (!string.IsNullOrEmpty(keywordLower) &&
                    !symbol.ToLowerInvariant().Contains(keywordLower) &&
                    !koreanName.ToLowerInvariant().Contains(keywordLower))
                {
                    continue;
                }

                sb.Append(symbol).Append(" (").Append(koreanName).AppendLine(")");
                sb.AppendLine(mathMode ? (entry.mathDescription ?? string.Empty) : (entry.easyDescription ?? string.Empty));
                sb.AppendLine();
            }

            contentText.text = sb.Length == 0 ? "검색 결과가 없습니다." : sb.ToString();
        }

        private void AutoWire()
        {
            if (panelRoot == null)
            {
                panelRoot = FindByName("GlossaryPanel");
            }

            if (openButton == null)
            {
                var go = FindByName("BtnGlossaryOpen");
                if (go != null) openButton = go.GetComponent<Button>();
            }

            if (closeButton == null)
            {
                var go = FindByName("BtnGlossaryClose");
                if (go != null) closeButton = go.GetComponent<Button>();
            }

            if (easyModeButton == null)
            {
                var go = FindByName("BtnGlossaryEasy");
                if (go != null) easyModeButton = go.GetComponent<Button>();
            }

            if (mathModeButton == null)
            {
                var go = FindByName("BtnGlossaryMath");
                if (go != null) mathModeButton = go.GetComponent<Button>();
            }

            if (searchField == null)
            {
                var go = FindByName("GlossarySearchInput");
                if (go != null) searchField = go.GetComponent<InputField>();
            }

            if (contentText == null)
            {
                var go = FindByName("GlossaryContentText");
                if (go != null) contentText = go.GetComponent<Text>();
            }
        }

        private void LoadGlossaryIfNeeded()
        {
            if (glossaryDatabase != null)
            {
                return;
            }

            glossaryDatabase = Resources.Load<GlossaryDatabase>("Glossary/GlossaryDatabase");
            if (glossaryDatabase != null)
            {
                return;
            }

            var fallback = ScriptableObject.CreateInstance<GlossaryDatabase>();
            fallback.entries = new GlossaryEntryConfig[0];
            glossaryDatabase = fallback;
        }

        private static GameObject FindByName(string objectName)
        {
            foreach (var candidate in Resources.FindObjectsOfTypeAll<Transform>())
            {
                if (candidate == null || candidate.gameObject.hideFlags != HideFlags.None)
                {
                    continue;
                }

                if (candidate.gameObject.scene.IsValid() && candidate.name == objectName)
                {
                    return candidate.gameObject;
                }
            }

            return null;
        }
    }
}

