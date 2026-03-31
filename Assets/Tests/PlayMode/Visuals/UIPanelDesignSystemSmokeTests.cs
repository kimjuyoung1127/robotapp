// Folder: Tests/PlayMode - PlayMode smoke and scene flow tests.
// UIPanelDesignSystemSmoke 흐름을 검증하는 PlayMode 테스트입니다.
using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace KineTutor3D.Tests.PlayMode
{
    public class UIPanelDesignSystemSmokeTests
    {
        [UnityTest]
        public IEnumerator RobotLibraryScene_ShowsMathWarmupButton()
        {
            yield return LoadScene("RobotLibrary");

            Assert.That(GameObject.Find("BtnStartMathReadiness")?.GetComponent<Button>(), Is.Not.Null);
        }

        [UnityTest]
        public IEnumerator SandboxScene_ShowsActionButtons_AndSnapshotLite()
        {
            yield return LoadScene("Sandbox");

            Assert.That(GameObject.Find("BtnZeroPose")?.GetComponent<Button>(), Is.Not.Null);
            Assert.That(GameObject.Find("BtnHomePose")?.GetComponent<Button>(), Is.Not.Null);
            Assert.That(GameObject.Find("BtnDemoPose")?.GetComponent<Button>(), Is.Not.Null);
            Assert.That(GameObject.Find("BtnResetPose")?.GetComponent<Button>(), Is.Not.Null);
            Assert.That(GameObject.Find("BtnSaveSnapshot")?.GetComponent<Button>(), Is.Not.Null);
            Assert.That(GameObject.Find("BtnCompareSnapshot")?.GetComponent<Button>(), Is.Not.Null);
        }

        private static IEnumerator LoadScene(string sceneName)
        {
            var op = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
            Assert.That(op, Is.Not.Null);
            while (!op.isDone)
            {
                yield return null;
            }

            yield return null;
        }
    }
}
