// Folder: UI - HUD/view components only; no kinematics logic.
using System;
using UnityEngine;
using UnityEngine.UI;

namespace KineTutor3D.UI
{
    /// <summary>
    /// Live 모드에서 MoveJ 실행 전 확인 대화상자입니다.
    /// AccentDanger 색상으로 위험성을 강조합니다.
    /// </summary>
    public class FairinoMoveConfirmDialog : MonoBehaviour, IVisibilityControllable
    {
        [SerializeField] private Text messageLabel;
        [SerializeField] private Button confirmButton;
        [SerializeField] private Button cancelButton;
        [SerializeField] private Image backdrop;
        [SerializeField] private Font fallbackFont;

        private Action onConfirm;
        private Action onCancel;
        private bool listenersBound;

        private void Awake()
        {
            EnsurePresentation();
            BindListeners();
        }

        private void OnEnable()
        {
            EnsurePresentation();
            BindListeners();
        }

        private void OnDisable()
        {
            UnbindListeners();
        }

        /// <summary>
        /// 대화상자를 표시합니다.
        /// </summary>
        public void Show(string message, Action confirmAction, Action cancelAction = null)
        {
            EnsurePresentation();
            onConfirm = confirmAction;
            onCancel = cancelAction;

            if (messageLabel != null)
            {
                messageLabel.text = message;
            }

            gameObject.SetActive(true);
        }

        /// <summary>
        /// 대화상자를 숨깁니다.
        /// </summary>
        public void Hide()
        {
            gameObject.SetActive(false);
            onConfirm = null;
            onCancel = null;
        }

        /// <summary>
        /// 패널 가시성을 설정합니다.
        /// </summary>
        public void SetVisible(bool visible)
        {
            gameObject.SetActive(visible);
        }

        private void EnsurePresentation()
        {
            fallbackFont = UiRuntimeStyle.ResolveFont(fallbackFont);
            var root = transform as RectTransform;
            if (root == null)
            {
                return;
            }

            if (TryBindExistingPresentation(root))
            {
                return;
            }

            backdrop = root.GetComponent<Image>() ?? root.gameObject.AddComponent<Image>();
            backdrop.color = new Color(0f, 0f, 0f, 0.5f);

            var card = UiRuntimeStyle.EnsureRectChild(root, "DialogCard");
            UiRuntimeStyle.Anchor(card, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(UIDesignTokens.Size.ModalWidth, 200f), Vector2.zero);
            var cardBg = card.GetComponent<Image>() ?? card.gameObject.AddComponent<Image>();
            cardBg.color = UIDesignTokens.Colors.SurfaceRaisedAlt;

            var title = UiRuntimeStyle.EnsureText(card, "Title", fallbackFont, UIDesignTokens.Type.HeadingSm, FontStyle.Bold, TextAnchor.UpperCenter, UIDesignTokens.Colors.AccentDanger);
            UiRuntimeStyle.Anchor(title.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(300f, 24f), new Vector2(0f, -16f));
            title.text = "실제 로봇 이동 확인";

            messageLabel = UiRuntimeStyle.EnsureText(card, "Message", fallbackFont, UIDesignTokens.Type.Body, FontStyle.Normal, TextAnchor.UpperCenter, UIDesignTokens.Colors.TextPrimary);
            UiRuntimeStyle.Anchor(messageLabel.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(300f, 60f), new Vector2(0f, -50f));

            confirmButton ??= UIComponentFactory.CreatePrimaryButton(card, "BtnConfirm", "실행", fallbackFont, 120f);
            UiRuntimeStyle.Anchor((RectTransform)confirmButton.transform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(120f, UIDesignTokens.Size.ButtonHeightMd), new Vector2(-70f, 20f));
            var confirmLabel = confirmButton.GetComponentInChildren<Text>();
            if (confirmLabel != null) confirmLabel.color = Color.white;
            var confirmImg = confirmButton.GetComponent<Image>();
            if (confirmImg != null) confirmImg.color = UIDesignTokens.Colors.AccentDanger;

            cancelButton ??= UIComponentFactory.CreateSecondaryButton(card, "BtnCancel", "취소", fallbackFont, 120f);
            UiRuntimeStyle.Anchor((RectTransform)cancelButton.transform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(120f, UIDesignTokens.Size.ButtonHeightMd), new Vector2(70f, 20f));
        }

        private bool TryBindExistingPresentation(RectTransform root)
        {
            backdrop = root.GetComponent<Image>();
            var card = root.Find("DialogCard") as RectTransform;
            messageLabel = card?.Find("Message")?.GetComponent<Text>();
            confirmButton = card?.Find("BtnConfirm")?.GetComponent<Button>();
            cancelButton = card?.Find("BtnCancel")?.GetComponent<Button>();

            if (backdrop == null || card == null || messageLabel == null || confirmButton == null || cancelButton == null)
            {
                return false;
            }

            backdrop.color = new Color(0f, 0f, 0f, 0.5f);

            var title = card.Find("Title")?.GetComponent<Text>();
            if (title != null)
            {
                title.text = "실제 로봇 이동 확인";
            }

            var confirmLabel = confirmButton.GetComponentInChildren<Text>();
            if (confirmLabel != null)
            {
                confirmLabel.color = Color.white;
            }

            var confirmImg = confirmButton.GetComponent<Image>();
            if (confirmImg != null)
            {
                confirmImg.color = UIDesignTokens.Colors.AccentDanger;
            }

            return true;
        }

        private void BindListeners()
        {
            if (listenersBound)
            {
                return;
            }

            confirmButton?.onClick.AddListener(OnConfirmClicked);
            cancelButton?.onClick.AddListener(OnCancelClicked);
            listenersBound = true;
        }

        private void UnbindListeners()
        {
            if (!listenersBound)
            {
                return;
            }

            confirmButton?.onClick.RemoveListener(OnConfirmClicked);
            cancelButton?.onClick.RemoveListener(OnCancelClicked);
            listenersBound = false;
        }

        private void OnConfirmClicked()
        {
            var callback = onConfirm;
            Hide();
            callback?.Invoke();
        }

        private void OnCancelClicked()
        {
            var callback = onCancel;
            Hide();
            callback?.Invoke();
        }
    }
}
