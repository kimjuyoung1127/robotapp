// Folder: Visualization - Unity-side rendering and FK binding.
using UnityEngine;

namespace KineTutor3D.Visualization
{
    internal static class ScaraDonorMapper
    {
        public static Transform ResolveDonorSource(Transform visualRoot, string donorSourceName, string donorFallbackName)
        {
            var donor = RobotRigBinder.FindSceneTransform(donorSourceName) ?? RobotRigBinder.FindSceneTransform(donorFallbackName);
            if (donor == null)
            {
                return null;
            }

            donor.SetParent(visualRoot, false);
            donor.localPosition = Vector3.zero;
            donor.localRotation = Quaternion.identity;
            donor.localScale = Vector3.one;
            DonorMeshCopier.DisableRuntimeComponents(donor);
            donor.gameObject.SetActive(false);
            return donor;
        }

        public static void CacheDonorParts(Transform donorSourceRoot, ref Transform donorBaseSource, ref Transform donorLink0Source, ref Transform donorLink1Source, ref Transform donorAxis3Source, ref Transform donorEndEffectorSource, ref Transform donorPickSource)
        {
            if (donorSourceRoot == null)
            {
                return;
            }

            donorBaseSource ??= donorSourceRoot.Find("Base");
            donorLink0Source ??= donorSourceRoot.Find("Base/Axis1");
            donorLink1Source ??= donorSourceRoot.Find("Base/Axis1/Axis2");
            donorAxis3Source ??= donorSourceRoot.Find("Base/Axis1/Axis2/Axis3");
            donorEndEffectorSource ??= donorAxis3Source != null ? donorAxis3Source.Find("Gripper") : donorSourceRoot.Find("Base/Axis1/Axis2/Axis3/Gripper");
            donorPickSource ??= donorSourceRoot.Find("Pick");
        }
    }
}
