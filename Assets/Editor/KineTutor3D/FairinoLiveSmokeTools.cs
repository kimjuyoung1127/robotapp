// Folder: Editor - Unity Editor authoring, QA, and packaging tools.
// FAIRINO 라이브 연결 스모크 점검용 에디터 도구입니다.
#if UNITY_EDITOR
using KineTutor3D.App.Fairino;
using UnityEditor;
using UnityEngine;

namespace KineTutor3D.Editor
{
    public static class FairinoLiveSmokeTools
    {
        [MenuItem("KineTutor3D/RobotControl/Run FAIRINO Live Smoke Test", priority = 170)]
        public static void RunMenu()
        {
            Debug.Log(RunSmoke());
        }

        public static string RunSmoke()
        {
            return FairinoLiveSmokeRunner.RunFromEnvironment();
        }
    }
}
#endif
