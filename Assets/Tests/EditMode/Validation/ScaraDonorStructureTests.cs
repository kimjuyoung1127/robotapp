// Folder: Tests/EditMode - EditMode tests for runtime, math, and tooling behaviors.
// ScaraDonorStructure 동작을 검증하는 EditMode 테스트입니다.
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace KineTutor3D.Tests.EditMode
{
    /// <summary>
    /// Validates the donor hierarchy used by the visualization pipeline.
    /// </summary>
    public class ScaraDonorStructureTests
    {
        [Test]
        public void ScaraRobotPrefab_ContainsExpectedDonorPath()
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Runtime/Resources/Robots/ScaraRobot.prefab");

            Assert.That(prefab, Is.Not.Null, "ScaraRobot prefab is required.");
            Assert.That(prefab.transform.Find("Base"), Is.Not.Null, "Base donor is required.");
            Assert.That(prefab.transform.Find("Base/Axis1"), Is.Not.Null, "Axis1 donor is required.");
            Assert.That(prefab.transform.Find("Base/Axis1/Axis2"), Is.Not.Null, "Axis2 donor is required.");
            Assert.That(prefab.transform.Find("Base/Axis1/Axis2/Axis3"), Is.Not.Null, "Axis3 donor is required.");
            Assert.That(prefab.transform.Find("Base/Axis1/Axis2/Axis3/Gripper"), Is.Not.Null, "Gripper donor is required.");
        }

        [Test]
        public void Pick_IsHelperPoint_NotMeshDonor()
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Runtime/Resources/Robots/ScaraRobot.prefab");
            var pick = prefab != null ? prefab.transform.Find("Pick") : null;

            Assert.That(pick, Is.Not.Null, "Pick helper is expected to exist.");
            Assert.That(pick.GetComponent<MeshFilter>(), Is.Null, "Pick should not be used as a mesh donor.");
            Assert.That(pick.GetComponent<MeshRenderer>(), Is.Null, "Pick should not be used as a mesh donor.");
        }
    }
}
