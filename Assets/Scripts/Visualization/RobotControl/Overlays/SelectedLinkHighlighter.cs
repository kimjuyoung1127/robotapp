// Folder: Visualization - RobotControl-specific rendering and overlay drivers.
using System.Collections.Generic;
using UnityEngine;

namespace KineTutor3D.Visualization
{
    /// <summary>
    /// 선택한 링크의 renderer 재질을 잠시 강조해 메인 로봇 위에서 선택 상태를 보이게 합니다.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SelectedLinkHighlighter : MonoBehaviour
    {
        [SerializeField] private Color tintColor = new(1f, 0.88f, 0.32f, 1f);
        [SerializeField] private Color emissionColor = new(0.95f, 0.72f, 0.18f, 1f);
        [SerializeField] private float tintStrength = 0.35f;

        private readonly Dictionary<Renderer, Material[]> originalMaterials = new();
        private readonly List<Material> runtimeMaterials = new();
        private Transform selectedTarget;

        public Transform SelectedTarget => selectedTarget;

        public void Select(Transform target)
        {
            if (selectedTarget == target)
            {
                return;
            }

            Clear();
            selectedTarget = target;
            if (selectedTarget == null)
            {
                return;
            }

            var renderers = selectedTarget.GetComponentsInChildren<Renderer>(true);
            for (var index = 0; index < renderers.Length; index++)
            {
                var renderer = renderers[index];
                if (renderer == null)
                {
                    continue;
                }

                var originals = renderer.sharedMaterials;
                if (originals == null || originals.Length == 0)
                {
                    continue;
                }

                originalMaterials[renderer] = originals;
                var highlighted = new Material[originals.Length];
                for (var materialIndex = 0; materialIndex < originals.Length; materialIndex++)
                {
                    var source = originals[materialIndex];
                    if (source == null)
                    {
                        continue;
                    }

                    var clone = new Material(source)
                    {
                        name = source.name + "_Selected"
                    };
                    ApplyHighlight(clone);
                    highlighted[materialIndex] = clone;
                    runtimeMaterials.Add(clone);
                }

                renderer.materials = highlighted;
            }
        }

        public void Clear()
        {
            foreach (var pair in originalMaterials)
            {
                if (pair.Key == null)
                {
                    continue;
                }

                pair.Key.sharedMaterials = pair.Value;
            }

            for (var index = 0; index < runtimeMaterials.Count; index++)
            {
                var material = runtimeMaterials[index];
                if (material == null)
                {
                    continue;
                }

                if (Application.isPlaying)
                {
                    Destroy(material);
                }
                else
                {
                    DestroyImmediate(material);
                }
            }

            originalMaterials.Clear();
            runtimeMaterials.Clear();
            selectedTarget = null;
        }

        private void OnDisable()
        {
            Clear();
        }

        private void ApplyHighlight(Material material)
        {
            if (material == null)
            {
                return;
            }

            if (material.HasProperty("_BaseColor"))
            {
                var baseColor = material.GetColor("_BaseColor");
                material.SetColor("_BaseColor", Color.Lerp(baseColor, tintColor, tintStrength));
            }
            else if (material.HasProperty("_Color"))
            {
                var color = material.GetColor("_Color");
                material.SetColor("_Color", Color.Lerp(color, tintColor, tintStrength));
            }

            if (material.HasProperty("_EmissionColor"))
            {
                material.EnableKeyword("_EMISSION");
                material.SetColor("_EmissionColor", emissionColor);
            }
        }
    }
}
