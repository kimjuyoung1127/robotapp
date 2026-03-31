// Folder: Visualization - Unity-side rendering and FK binding.
using UnityEngine;

namespace KineTutor3D.Visualization
{
    internal static class RobotRigBinder
    {
        public static Transform EnsureTransformChild(Transform parent, string childName)
        {
            var child = parent.Find(childName);
            if (child == null)
            {
                var go = new GameObject(childName);
                go.transform.SetParent(parent, false);
                child = go.transform;
            }

            return child;
        }

        public static Transform FindSceneTransform(string objectName)
        {
            var go = GameObject.Find(objectName);
            if (go != null)
            {
                return go.transform;
            }

            foreach (var candidate in Resources.FindObjectsOfTypeAll<GameObject>())
            {
                if (candidate.name == objectName)
                {
                    return candidate.transform;
                }
            }

            return null;
        }

        public static FrameGizmo EnsureFrameGizmo(Transform target)
        {
            if (target == null)
            {
                return null;
            }

            return target.GetComponent<FrameGizmo>() ?? target.gameObject.AddComponent<FrameGizmo>();
        }

        public static void HideLegacyMarker(Transform frameTransform)
        {
            if (frameTransform == null)
            {
                return;
            }

            var renderer = frameTransform.GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                renderer.enabled = false;
            }
        }

        public static void DisableLegacyFrame(Transform root, string childName)
        {
            var legacy = root.Find(childName);
            if (legacy != null && legacy.gameObject.activeSelf)
            {
                legacy.gameObject.SetActive(false);
            }
        }

        public static void DisableLegacyVisual(Transform visualRoot, string childName)
        {
            if (visualRoot == null)
            {
                return;
            }

            var legacy = visualRoot.Find(childName);
            if (legacy != null && legacy.gameObject.activeSelf)
            {
                legacy.gameObject.SetActive(false);
            }
        }

        public static Transform EnsureVisualAnchor(string childName, Transform parent, Transform source)
        {
            if (parent == null)
            {
                return null;
            }

            var anchor = parent.Find(childName);
            if (anchor == null)
            {
                var go = new GameObject(childName);
                go.transform.SetParent(parent, false);
                anchor = go.transform;
            }

            DonorMeshCopier.CopyMeshOnly(anchor.gameObject, source);
            return anchor;
        }

        public static Transform EnsurePivotChild(Transform parent, string childName)
        {
            if (parent == null)
            {
                return null;
            }

            var child = parent.Find(childName);
            if (child == null)
            {
                var go = new GameObject(childName);
                go.transform.SetParent(parent, false);
                child = go.transform;
            }

            return child;
        }

        public static Transform NormalizeVisualReference(Transform current, string expectedName)
        {
            if (current == null)
            {
                return null;
            }

            return current.name == expectedName ? current : null;
        }
    }
}
