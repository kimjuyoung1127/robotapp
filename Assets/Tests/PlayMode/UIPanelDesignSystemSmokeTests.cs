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
        public IEnumerator HomeScene_ShowsContinueAndMathWarmupButtons()
        {
            yield return LoadScene("Home");

            Assert.That(GameObject.Find("BtnContinueLatestContext")?.GetComponent<Button>(), Is.Not.Null);
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
