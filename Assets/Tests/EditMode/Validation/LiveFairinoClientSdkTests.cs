// Folder: Tests/EditMode - EditMode tests for runtime, math, and tooling behaviors.
// LiveFairinoClientSdk 동작을 검증하는 EditMode 테스트입니다.
using System.IO;
using System.Linq;
using System.Reflection;
using NUnit.Framework;

namespace KineTutor3D.Tests.EditMode
{
    public class LiveFairinoClientSdkTests
    {
        private const string DllPath = "Assets/Plugins/Fairino/libfairino.dll";

        [Test]
        public void FairinoSdk_RobotTypeExists()
        {
            var assembly = LoadSdkAssembly();
            var robotType = assembly.GetType("fairino.Robot");

            Assert.That(robotType, Is.Not.Null, "fairino.Robot type should exist in libfairino.dll");
        }

        [Test]
        public void FairinoSdk_RequiredMethodsExist()
        {
            var assembly = LoadSdkAssembly();
            var robotType = assembly.GetType("fairino.Robot");
            Assert.That(robotType, Is.Not.Null);

            var requiredMethods = new[]
            {
                "RPC",
                "RobotEnable",
                "MoveJ",
                "MoveL",
                "ServoJ",
                "GetActualJointPosDegree",
                "GetActualTCPPose",
                "GetRobotRealTimeState",
                "GetSDKVersion",
                "GetSoftwareVersion",
                "GetFirmwareVersion",
                "GetSafetyCode",
                "SetRobotRealtimeStateSamplePeriod",
                "GetRobotRealtimeStateSamplePeriod",
                "MotionQueueClear",
                "StopMotion",
                "DragTeachSwitch",
                "IsInDragTeach",
                "GetRobotErrorCode",
                "ResetAllError",
                "GetActualTCPNum",
                "GetActualWObjNum"
            };

            var methodNames = robotType.GetMethods().Select(m => m.Name).ToHashSet();
            foreach (var methodName in requiredMethods)
            {
                Assert.That(methodNames.Contains(methodName), Is.True, $"Method '{methodName}' should exist in fairino.Robot");
            }

            Assert.That(
                methodNames.Contains("SetReConnectParam") || methodNames.Contains("SetReconnectParam") || methodNames.Contains("GetReconnectState"),
                Is.True,
                "Reconnect-related API should exist in fairino.Robot");
        }

        private static Assembly LoadSdkAssembly()
        {
            Assert.That(File.Exists(DllPath), Is.True, $"{DllPath} should exist");
            return Assembly.LoadFrom(DllPath);
        }
    }
}
