// Folder: UI - HUD/view components only; no kinematics logic.
using System.Collections.Generic;
using UnityEngine;

namespace KineTutor3D.UI
{
    /// <summary>
    /// Play 진입 시 authored RectTransform 값을 캡처하고, runtime 정규화 후 복원합니다.
    /// 누락 오브젝트는 binder가 만들되, 이미 authored에 있던 위치는 코드가 덮어쓰지 않게 합니다.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class RobotControlAuthoredLayoutLock : MonoBehaviour
    {
        private readonly Dictionary<string, RectSnapshot> snapshots = new();
        private bool captured;

        public void CaptureIfNeeded(RectTransform root)
        {
            if (!Application.isPlaying || captured || root == null)
            {
                return;
            }

            snapshots.Clear();
            CaptureRecursive(root, root, string.Empty);
            captured = true;
        }

        public void RestoreCapturedLayout(RectTransform root)
        {
            if (!Application.isPlaying || !captured || root == null)
            {
                return;
            }

            foreach (var pair in snapshots)
            {
                var target = string.IsNullOrEmpty(pair.Key) ? root : root.Find(pair.Key) as RectTransform;
                if (target == null)
                {
                    continue;
                }

                pair.Value.ApplyTo(target);
            }
        }

        private void CaptureRecursive(RectTransform current, RectTransform root, string path)
        {
            snapshots[path] = RectSnapshot.From(current);

            for (var i = 0; i < current.childCount; i++)
            {
                if (current.GetChild(i) is not RectTransform child)
                {
                    continue;
                }

                var childPath = string.IsNullOrEmpty(path) ? child.name : $"{path}/{child.name}";
                CaptureRecursive(child, root, childPath);
            }
        }

        private readonly struct RectSnapshot
        {
            public readonly Vector2 AnchorMin;
            public readonly Vector2 AnchorMax;
            public readonly Vector2 Pivot;
            public readonly Vector2 AnchoredPosition;
            public readonly Vector2 SizeDelta;
            public readonly Vector2 OffsetMin;
            public readonly Vector2 OffsetMax;
            public readonly Vector3 LocalScale;

            private RectSnapshot(
                Vector2 anchorMin,
                Vector2 anchorMax,
                Vector2 pivot,
                Vector2 anchoredPosition,
                Vector2 sizeDelta,
                Vector2 offsetMin,
                Vector2 offsetMax,
                Vector3 localScale)
            {
                AnchorMin = anchorMin;
                AnchorMax = anchorMax;
                Pivot = pivot;
                AnchoredPosition = anchoredPosition;
                SizeDelta = sizeDelta;
                OffsetMin = offsetMin;
                OffsetMax = offsetMax;
                LocalScale = localScale;
            }

            public static RectSnapshot From(RectTransform rect)
            {
                return new RectSnapshot(
                    rect.anchorMin,
                    rect.anchorMax,
                    rect.pivot,
                    rect.anchoredPosition,
                    rect.sizeDelta,
                    rect.offsetMin,
                    rect.offsetMax,
                    rect.localScale);
            }

            public void ApplyTo(RectTransform rect)
            {
                rect.anchorMin = AnchorMin;
                rect.anchorMax = AnchorMax;
                rect.pivot = Pivot;
                rect.anchoredPosition = AnchoredPosition;
                rect.sizeDelta = SizeDelta;
                rect.offsetMin = OffsetMin;
                rect.offsetMax = OffsetMax;
                rect.localScale = LocalScale;
            }
        }
    }
}
