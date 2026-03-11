// Folder: Visualization - Unity-side rendering and FK binding.
using UnityEngine;

namespace KineTutor3D.Visualization
{
    /// <summary>
    /// 관절별 하이라이트 링을 생성하고 현재 포커스를 전환합니다.
    /// </summary>
    [ExecuteAlways]
    [DisallowMultipleComponent]
    public class LinkHighlighter : MonoBehaviour
    {
        [SerializeField] private JointHighlightRing joint0Ring;
        [SerializeField] private JointHighlightRing joint1Ring;

        public void Configure(Transform joint0Anchor, Transform joint1Anchor)
        {
            joint0Ring ??= EnsureRing("JointHighlightRing_0");
            joint1Ring ??= EnsureRing("JointHighlightRing_1");

            joint0Ring.Bind(joint0Anchor, 0.24f, new Color(0.95f, 0.77f, 0.15f, 1f));
            joint1Ring.Bind(joint1Anchor, 0.18f, new Color(0.29f, 0.56f, 0.85f, 1f));
            ClearHighlight();
        }

        public void HighlightJoint(int jointIndex)
        {
            joint0Ring?.SetVisible(jointIndex == 0);
            joint1Ring?.SetVisible(jointIndex == 1);
        }

        public void ClearHighlight()
        {
            joint0Ring?.SetVisible(false);
            joint1Ring?.SetVisible(false);
        }

        private JointHighlightRing EnsureRing(string name)
        {
            var existing = transform.Find(name);
            if (existing != null)
            {
                return existing.GetComponent<JointHighlightRing>() ?? existing.gameObject.AddComponent<JointHighlightRing>();
            }

            var go = new GameObject(name, typeof(JointHighlightRing));
            go.transform.SetParent(transform, false);
            return go.GetComponent<JointHighlightRing>();
        }
    }
}
