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

            var sourceFilter = source.GetComponent<MeshFilter>();
            var sourceRenderer = source.GetComponent<MeshRenderer>();

            if (sourceFilter != null)
            {
                var targetFilter = target.GetComponent<MeshFilter>() ?? target.AddComponent<MeshFilter>();
                targetFilter.sharedMesh = sourceFilter.sharedMesh;
            }

            if (sourceRenderer != null)
            {
                var targetRenderer = target.GetComponent<MeshRenderer>() ?? target.AddComponent<MeshRenderer>();
                targetRenderer.sharedMaterials = sourceRenderer.sharedMaterials;
                targetRenderer.shadowCastingMode = sourceRenderer.shadowCastingMode;
                targetRenderer.receiveShadows = sourceRenderer.receiveShadows;
            }
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
