// Folder: Visualization - Unity-side rendering and FK binding.
using UnityEngine;

namespace KineTutor3D.Visualization
{
    internal static class RobotVisibilityProbe
    {
        public static Bounds GetAggregateVisualBounds(Component root)
        {
            var renderers = root.GetComponentsInChildren<MeshRenderer>(true);
            var hasBounds = false;
            var aggregate = new Bounds(root.transform.position, Vector3.zero);

            foreach (var renderer in renderers)
            {
                if (!renderer.enabled || !renderer.gameObject.activeInHierarchy)
                {
                    continue;
                }

                if (!hasBounds)
                {
                    aggregate = renderer.bounds;
                    hasBounds = true;
                }
                else
                {
                    aggregate.Encapsulate(renderer.bounds);
                }
            }

            return aggregate;
        }

        public static bool IsVisibleFrom(Component root, Camera camera)
        {
            if (camera == null)
            {
                return false;
            }

            var bounds = GetAggregateVisualBounds(root);
            if (bounds.size.sqrMagnitude < 1e-6f)
            {
                return false;
            }

            var planes = GeometryUtility.CalculateFrustumPlanes(camera);
            return GeometryUtility.TestPlanesAABB(planes, bounds);
        }
    }
}
