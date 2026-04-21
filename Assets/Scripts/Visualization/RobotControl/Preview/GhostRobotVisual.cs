// Folder: Visualization - 3D rendering helpers for robot joint/link display.
using UnityEngine;

namespace KineTutor3D.Visualization
{
    /// <summary>
    /// 메인 로봇과 분리된 반투명 고스트 로봇을 유지합니다.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class GhostRobotVisual : MonoBehaviour
    {
        [SerializeField] private GameObject ghostInstance;
        [SerializeField] private FairinoUrdfJointDriver ghostDriver;
        [SerializeField] private string baseLinkName = "base_link";

        public bool HasGhost => ghostInstance != null && ghostInstance.activeSelf;

        public void EnsureGhost(GameObject sourceRoot, string sourceBaseLinkName)
        {
            if (sourceRoot == null)
            {
                return;
            }

            baseLinkName = string.IsNullOrWhiteSpace(sourceBaseLinkName) ? "base_link" : sourceBaseLinkName;
            if (ghostInstance == null)
            {
                ghostInstance = Instantiate(sourceRoot, transform);
                ghostInstance.name = "GhostRobot";
                ghostInstance.transform.localPosition = Vector3.zero;
                ghostInstance.transform.localRotation = Quaternion.identity;
                ghostInstance.transform.localScale = Vector3.one;
                PrepareGhostHierarchy(ghostInstance);
                ghostDriver = ghostInstance.GetComponent<FairinoUrdfJointDriver>();
                if (ghostDriver == null)
                {
                    ghostDriver = ghostInstance.AddComponent<FairinoUrdfJointDriver>();
                }
            }

            var baseLink = FindChildRecursive(ghostInstance.transform, baseLinkName);
            if (ghostDriver != null && baseLink != null)
            {
                ghostDriver.Inject(baseLink);
            }

            SetVisible(false);
        }

        public void ApplyJointAngles(double[] jointAnglesDeg)
        {
            ghostDriver?.ApplyJointAngles(jointAnglesDeg);
        }

        public void SetVisible(bool visible)
        {
            if (ghostInstance != null)
            {
                ghostInstance.SetActive(visible);
            }
        }

        private static void PrepareGhostHierarchy(GameObject root)
        {
            var renderers = root.GetComponentsInChildren<Renderer>(true);
            for (var index = 0; index < renderers.Length; index++)
            {
                var renderer = renderers[index];
                if (renderer == null)
                {
                    continue;
                }

                renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                renderer.receiveShadows = false;

                var materials = renderer.sharedMaterials;
                for (var materialIndex = 0; materialIndex < materials.Length; materialIndex++)
                {
                    var sourceMaterial = materials[materialIndex];
                    if (sourceMaterial == null)
                    {
                        continue;
                    }

                    var material = new Material(sourceMaterial)
                    {
                        name = $"{sourceMaterial.name}_Ghost",
                    };
                    if (material.HasProperty("_BaseColor"))
                    {
                        var color = material.GetColor("_BaseColor");
                        color.a = 0.28f;
                        material.SetColor("_BaseColor", color);
                    }

                    if (material.HasProperty("_Color"))
                    {
                        var color = material.color;
                        color.a = 0.28f;
                        material.color = color;
                    }

                    materials[materialIndex] = material;
                }

                renderer.sharedMaterials = materials;
            }

            var colliders = root.GetComponentsInChildren<Collider>(true);
            for (var index = 0; index < colliders.Length; index++)
            {
                if (colliders[index] != null)
                {
                    colliders[index].enabled = false;
                }
            }

            var articulationBodies = root.GetComponentsInChildren<ArticulationBody>(true);
            for (var index = 0; index < articulationBodies.Length; index++)
            {
                var body = articulationBodies[index];
                if (body == null)
                {
                    continue;
                }

                body.useGravity = false;
                body.linearVelocity = Vector3.zero;
                body.angularVelocity = Vector3.zero;
                if (!body.isRoot)
                {
                    body.enabled = false;
                }
            }

            var controllers = root.GetComponentsInChildren<MonoBehaviour>(true);
            for (var index = 0; index < controllers.Length; index++)
            {
                var controller = controllers[index];
                if (controller == null)
                {
                    continue;
                }

                if (controller is FairinoUrdfJointDriver)
                {
                    continue;
                }

                controller.enabled = false;
            }
        }

        private static Transform FindChildRecursive(Transform parent, string childName)
        {
            if (parent == null)
            {
                return null;
            }

            if (parent.name == childName)
            {
                return parent;
            }

            for (var index = 0; index < parent.childCount; index++)
            {
                var found = FindChildRecursive(parent.GetChild(index), childName);
                if (found != null)
                {
                    return found;
                }
            }

            return null;
        }
    }
}
