// Folder: Visualization - Unity-side rendering and FK binding.
using UnityEngine;

namespace KineTutor3D.Visualization
{
    internal static class DonorMeshCopier
    {
        public static void CopyMeshOnly(GameObject target, Transform source)
        {
            if (target == null || source == null)
            {
                return;
            }

            try
            {
                source.TryGetComponent<MeshFilter>(out var sourceFilter);
                source.TryGetComponent<MeshRenderer>(out var sourceRenderer);
                source.TryGetComponent<SkinnedMeshRenderer>(out var sourceSkinnedRenderer);

                if (sourceFilter != null)
                {
                    var sourceMesh = sourceFilter.sharedMesh;
                    if (sourceMesh != null)
                    {
                        var targetFilter = EnsureMeshFilter(target);
                        if (targetFilter != null)
                        {
                            targetFilter.sharedMesh = sourceMesh;
                        }
                    }
                }

                if (sourceRenderer != null)
                {
                    var targetRenderer = EnsureMeshRenderer(target);
                    if (targetRenderer != null)
                    {
                        targetRenderer.sharedMaterials = sourceRenderer.sharedMaterials;
                        targetRenderer.shadowCastingMode = sourceRenderer.shadowCastingMode;
                        targetRenderer.receiveShadows = sourceRenderer.receiveShadows;
                    }
                }

                if (sourceSkinnedRenderer != null && sourceSkinnedRenderer.sharedMesh != null)
                {
                    var targetFilter = EnsureMeshFilter(target);
                    var targetRenderer = EnsureMeshRenderer(target);
                    if (targetFilter != null)
                    {
                        targetFilter.sharedMesh = sourceSkinnedRenderer.sharedMesh;
                    }

                    if (targetRenderer != null)
                    {
                        targetRenderer.sharedMaterials = sourceSkinnedRenderer.sharedMaterials;
                        targetRenderer.shadowCastingMode = sourceSkinnedRenderer.shadowCastingMode;
                        targetRenderer.receiveShadows = sourceSkinnedRenderer.receiveShadows;
                    }
                }
            }
            catch (MissingComponentException)
            {
                // 일부 donor prefab 노드는 editor/runtime 컨텍스트에 따라 불완전할 수 있다.
                // preview에서는 해당 노드만 건너뛰고 나머지 계층 복사를 계속 진행한다.
            }
        }

        private static MeshFilter EnsureMeshFilter(GameObject target)
        {
            var filter = target.GetComponent<MeshFilter>();
            if (filter != null)
            {
                return filter;
            }

            return target.AddComponent<MeshFilter>();
        }

        private static MeshRenderer EnsureMeshRenderer(GameObject target)
        {
            var renderer = target.GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                return renderer;
            }

            return target.AddComponent<MeshRenderer>();
        }

        public static void DisableRuntimeComponents(Transform donorRoot)
        {
            foreach (var behaviour in donorRoot.GetComponentsInChildren<MonoBehaviour>(true))
            {
                behaviour.enabled = false;
            }

            foreach (var rigidbody in donorRoot.GetComponentsInChildren<Rigidbody>(true))
            {
                rigidbody.isKinematic = true;
                rigidbody.useGravity = false;
                rigidbody.detectCollisions = false;
            }

            foreach (var collider in donorRoot.GetComponentsInChildren<Collider>(true))
            {
                collider.enabled = false;
            }
        }
    }
}
