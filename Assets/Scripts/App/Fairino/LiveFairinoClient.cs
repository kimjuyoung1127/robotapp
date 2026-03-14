// Folder: App - Application controllers and services; single UnityEngine entry point.
using UnityEngine;

namespace KineTutor3D.App.Fairino
{
    /// <summary>
    /// FAIRINO C# SDK(fairino.dll)를 래핑하는 실기 연동 클라이언트입니다.
    /// SDK DLL이 Assets/Plugins/Fairino/에 배치되어야 동작합니다.
    /// </summary>
    public sealed class LiveFairinoClient : IFairinoRobotClient
    {
        private readonly FairinoErrorTranslator errorTranslator;
        private bool connected;
        private bool enabled;

        // fairino.Robot SDK 인스턴스 (DLL 존재 시 리플렉션으로 로드)
        private object sdkRobot;

        /// <summary>
        /// 현재 연결 상태입니다.
        /// </summary>
        public bool IsConnected => connected;

        /// <summary>
        /// 로봇 활성화(서보 ON) 상태입니다.
        /// </summary>
        public bool IsEnabled => enabled;

        /// <summary>
        /// Live 클라이언트를 생성합니다.
        /// </summary>
        public LiveFairinoClient(FairinoErrorTranslator translator = null)
        {
            errorTranslator = translator ?? new FairinoErrorTranslator();
        }

        /// <summary>
        /// 로봇에 연결합니다.
        /// </summary>
        public FairinoResult Connect(string ip, int port)
        {
            if (string.IsNullOrEmpty(ip))
            {
                return FairinoResult.Fail(-1, "IP 주소가 비어 있습니다.");
            }

            try
            {
                // SDK가 로드되지 않은 경우 리플렉션으로 시도
                var robotType = FindSdkType("fairino.Robot");
                if (robotType == null)
                {
                    Debug.LogWarning("[LiveFairinoClient] fairino.dll을 찾을 수 없습니다. MockFairinoClient를 사용하세요.");
                    return FairinoResult.Fail(-1, "SDK DLL이 로드되지 않았습니다. Assets/Plugins/Fairino/에 fairino.dll을 배치하세요.");
                }

                sdkRobot = System.Activator.CreateInstance(robotType);
                var connectMethod = robotType.GetMethod("RPC");
                if (connectMethod != null)
                {
                    var result = connectMethod.Invoke(sdkRobot, new object[] { ip });
                    connected = true;
                    return FairinoResult.Ok("연결 성공");
                }

                return FairinoResult.Fail(-1, "SDK Connect 메서드를 찾을 수 없습니다.");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[LiveFairinoClient] 연결 실패: {ex.Message}");
                return FairinoResult.Fail(-1, $"연결 실패: {ex.Message}");
            }
        }

        /// <summary>
        /// 연결을 해제합니다.
        /// </summary>
        public FairinoResult Disconnect()
        {
            connected = false;
            enabled = false;
            sdkRobot = null;
            return FairinoResult.Ok("연결 해제");
        }

        /// <summary>
        /// 로봇을 활성화(서보 ON)합니다.
        /// </summary>
        public FairinoResult Enable()
        {
            if (!connected) return errorTranslator.ToResult(-1);

            var result = InvokeSdk("RobotEnable", 1);
            if (result.IsSuccess) enabled = true;
            return result;
        }

        /// <summary>
        /// 로봇을 비활성화(서보 OFF)합니다.
        /// </summary>
        public FairinoResult Disable()
        {
            if (!connected) return errorTranslator.ToResult(-1);

            var result = InvokeSdk("RobotEnable", 0);
            if (result.IsSuccess) enabled = false;
            return result;
        }

        /// <summary>
        /// 관절 공간 이동 명령을 전송합니다 (MoveJ).
        /// </summary>
        public FairinoResult MoveJ(double[] jointPosDeg, int speedPercent, int accPercent)
        {
            if (!connected) return errorTranslator.ToResult(-1);
            if (!enabled) return errorTranslator.ToResult(-2);
            if (jointPosDeg == null || jointPosDeg.Length != 6)
                return FairinoResult.Fail(-3, "6축 관절 값이 필요합니다.");

            return InvokeSdk("MoveJ", jointPosDeg, speedPercent, accPercent);
        }

        /// <summary>
        /// 서보 관절 이동 명령을 전송합니다 (ServoJ).
        /// </summary>
        public FairinoResult ServoJ(double[] jointPosDeg)
        {
            if (!connected) return errorTranslator.ToResult(-1);
            if (!enabled) return errorTranslator.ToResult(-2);
            if (jointPosDeg == null || jointPosDeg.Length != 6)
                return FairinoResult.Fail(-3, "6축 관절 값이 필요합니다.");

            return InvokeSdk("ServoJ", jointPosDeg);
        }

        /// <summary>
        /// 현재 로봇 상태를 읽어옵니다.
        /// </summary>
        public FairinoResult<FairinoRobotState> ReadState()
        {
            if (!connected)
                return FairinoResult<FairinoRobotState>.Fail(-1, "연결되지 않은 상태입니다.");

            try
            {
                var jointPos = new double[6];
                var tcpPose = new double[6];

                InvokeSdkRaw("GetActualJointPosDegree", new object[] { jointPos });
                InvokeSdkRaw("GetActualTCPPose", new object[] { tcpPose });

                var state = new FairinoRobotState(jointPos, tcpPose);
                return FairinoResult<FairinoRobotState>.Ok(state);
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[LiveFairinoClient] 상태 읽기 실패: {ex.Message}");
                return FairinoResult<FairinoRobotState>.Fail(-6, ex.Message);
            }
        }

        /// <summary>
        /// 직교 공간 직선 이동 명령을 전송합니다 (MoveL).
        /// </summary>
        public FairinoResult MoveL(double[] tcpPose, int speedPercent, int accPercent)
        {
            if (!connected) return errorTranslator.ToResult(-1);
            if (!enabled) return errorTranslator.ToResult(-2);
            if (tcpPose == null || tcpPose.Length != 6)
                return FairinoResult.Fail(-3, "6축 TCP 포즈가 필요합니다.");

            return InvokeSdk("MoveL", tcpPose, speedPercent, accPercent);
        }

        /// <summary>
        /// 모든 동작을 즉시 정지합니다.
        /// </summary>
        public FairinoResult StopMotion()
        {
            if (!connected) return errorTranslator.ToResult(-1);
            return InvokeSdk("MoveStopJ");
        }

        /// <summary>
        /// 로봇 버전 정보를 읽어옵니다.
        /// </summary>
        public FairinoResult<FairinoVersionInfo> GetVersion()
        {
            if (!connected)
                return FairinoResult<FairinoVersionInfo>.Fail(-1, "연결되지 않은 상태입니다.");

            var version = new FairinoVersionInfo("Live", "fairino-csharp-sdk");
            return FairinoResult<FairinoVersionInfo>.Ok(version);
        }

        private FairinoResult InvokeSdk(string methodName, params object[] args)
        {
            try
            {
                var code = InvokeSdkRaw(methodName, args);
                int errCode = code is int i ? i : 0;
                return errorTranslator.ToResult(errCode);
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[LiveFairinoClient] {methodName} 실패: {ex.Message}");
                return FairinoResult.Fail(-6, ex.Message);
            }
        }

        private object InvokeSdkRaw(string methodName, object[] args)
        {
            if (sdkRobot == null)
                throw new System.InvalidOperationException("SDK 인스턴스가 없습니다.");

            var method = sdkRobot.GetType().GetMethod(methodName);
            if (method == null)
                throw new System.MissingMethodException($"SDK에 {methodName} 메서드가 없습니다.");

            return method.Invoke(sdkRobot, args);
        }

        private static System.Type FindSdkType(string fullName)
        {
            foreach (var asm in System.AppDomain.CurrentDomain.GetAssemblies())
            {
                var type = asm.GetType(fullName);
                if (type != null) return type;
            }

            return null;
        }
    }
}
