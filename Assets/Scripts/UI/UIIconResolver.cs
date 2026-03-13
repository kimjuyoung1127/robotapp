// Folder: UI - HUD/view components only; no kinematics logic.
using UnityEngine;
using UnityEngine.UI;

namespace KineTutor3D.UI
{
    /// <summary>
    /// 아이콘 로딩을 중앙화합니다. Assets/Runtime/Resources/UI/Icons/ 경로의 Sprite를 로드합니다.
    /// </summary>
    public static class UIIconResolver
    {
        private const string IconBasePath = "UI/Icons/";

        /// <summary>
        /// Resources/UI/Icons/ 내 아이콘을 이름으로 로드합니다.
        /// 확장자 없이 파일명만 전달합니다.
        /// </summary>
        public static Sprite Load(string iconName)
        {
            if (string.IsNullOrEmpty(iconName))
            {
                return null;
            }

            return Resources.Load<Sprite>(IconBasePath + iconName);
        }

        /// <summary>
        /// 부모 아래에 아이콘 Image 컴포넌트를 생성합니다.
        /// </summary>
        public static Image CreateIcon(
            Transform parent,
            string name,
            string iconName,
            float size = UIDesignTokens.Size.IconMd,
            Color? tint = null)
        {
            var rect = UiRuntimeStyle.EnsureRectChild(parent, name);
            rect.sizeDelta = new Vector2(size, size);

            var image = rect.GetComponent<Image>();
            if (image == null)
            {
                image = rect.gameObject.AddComponent<Image>();
            }

            var sprite = Load(iconName);
            if (sprite != null)
            {
                image.sprite = sprite;
                image.type = Image.Type.Simple;
                image.preserveAspect = true;
            }

            image.color = tint ?? Color.white;
            image.raycastTarget = false;

            return image;
        }

        /// <summary>
        /// 기존 Image 컴포넌트에 아이콘을 설정합니다.
        /// </summary>
        public static void SetIcon(Image image, string iconName, Color? tint = null)
        {
            if (image == null)
            {
                return;
            }

            var sprite = Load(iconName);
            if (sprite != null)
            {
                image.sprite = sprite;
                image.type = Image.Type.Simple;
                image.preserveAspect = true;
            }

            if (tint.HasValue)
            {
                image.color = tint.Value;
            }
        }
    }
}
