// Folder: App - Application controllers and services; single UnityEngine entry point.
using System;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace KineTutor3D.App.Fairino
{
    /// <summary>
    /// FAIRINO C# SDK(libfairino.dll)를 래핑하는 실기 연동 클라이언트입니다.
    /// SDK DLL이 Assets/Plugins/Fairino/에 배치되어야 동작합니다.
    /// </summary>
    public sealed class LiveFairinoClient : IFairinoRobotClient
    {
        private const byte CurrentStateFlag = 0;
        private const int DefaultRealtimeStatePeriodMs = 100;
        private const int DefaultLiveMoveLSpeedPercent = 10;

        private readonly FairinoErrorTranslator errorTranslator;
        private bool connected;
        private bool enabled;
        private object sdkRobot;
        private FairinoCoordContext lastCoordContext = FairinoCoordContext.Default();
        private FairinoControllerFault lastControllerFault = FairinoControllerFault.None();
        private bool lastDragTeachActive;
        private int lastRobotMode;
        private int lastSafetyCode;
        private int lastRealtimeStatePeriodMs = DefaultRealtimeStatePeriodMs;

        public bool IsConnected => connected;
        public bool IsEnabled => enabled;

        public LiveFairinoClient(FairinoErrorTranslator translator = null)
        {
            errorTranslator = translator ?? new FairinoErrorTranslator();
        }

        public FairinoResult Connect(string ip, int port)
        {
            if (string.IsNullOrEmpty(ip))
            {
                return FairinoResult.Fail(-1, "IP 주소가 비어 있습니다.");
            }

            try
            {
                var robotType = FindSdkType("fairino.Robot");
                if (robotType == null)
                {
                    Debug.LogWarning("[LiveFairinoClient] libfairino.dll을 찾을 수 없습니다. MockFairinoClient를 사용하세요.");
                    return FairinoResult.Fail(-1, "SDK DLL이 로드되지 않았습니다. Assets/Plugins/Fairino/에 libfairino.dll을 배치하세요.");
                }

                sdkRobot = Activator.CreateInstance(robotType);
                var result = InvokeSdk("RPC", ip);
                if (!result.IsSuccess)
                {
                    sdkRobot = null;
                    connected = false;
                    return FairinoResult.Fail(
                        result.ErrorCode,
                        $"RPC 연결 실패 (ip={ip}, port={port}, code={result.ErrorCode}). 컨트롤러 전원, 네트워크, SDK 호환 버전을 확인하세요.");
                }

                connected = true;
                enabled = false;
                lastCoordContext = FairinoCoordContext.Default();
                lastControllerFault = FairinoControllerFault.None();
                lastDragTeachActive = false;
                lastRobotMode = 0;
                lastSafetyCode = 0;
                lastRealtimeStatePeriodMs = DefaultRealtimeStatePeriodMs;
                return FairinoResult.Ok($"연결 성공: {ip}:{port}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[LiveFairinoClient] 연결 실패: {ex.Message}");
                return FairinoResult.Fail(-6, $"연결 실패: {ex.Message}");
            }
        }

        public FairinoResult Disconnect()
        {
            connected = false;
            enabled = false;

            if (sdkRobot != null && HasMethod("CloseRPC"))
            {
                try
                {
                    InvokeSdk("CloseRPC");
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"[LiveFairinoClient] CloseRPC 실패: {ex.Message}");
                }
            }

            sdkRobot = null;
            lastCoordContext = FairinoCoordContext.Default();
            lastControllerFault = FairinoControllerFault.None();
            lastDragTeachActive = false;
            lastRobotMode = 0;
            lastSafetyCode = 0;
            lastRealtimeStatePeriodMs = DefaultRealtimeStatePeriodMs;
            return FairinoResult.Ok("연결 해제");
        }

        public FairinoResult Enable()
        {
            if (!connected)
            {
                return errorTranslator.ToResult(-1);
            }

            var preflight = BestEffortPrepareForLiveMotion();
            if (!preflight.IsSuccess)
            {
                return preflight;
            }

            var result = InvokeSdk("RobotEnable", (byte)1);
            if (result.IsSuccess)
            {
                enabled = true;
            }

            return result;
        }

        public FairinoResult Disable()
        {
            if (!connected)
            {
                return errorTranslator.ToResult(-1);
            }

            var result = InvokeSdk("RobotEnable", (byte)0);
            if (result.IsSuccess)
            {
                enabled = false;
            }

            return result;
        }

        public FairinoResult MoveJ(double[] jointPosDeg, int speedPercent, int accPercent)
        {
            var preflight = EnsureReadyForLiveMotion();
            if (!preflight.IsSuccess) return preflight;
            if (jointPosDeg == null || jointPosDeg.Length != 6)
            {
                return FairinoResult.Fail(-3, "6축 관절 값이 필요합니다.");
            }

            var context = EnsureCoordContext();
            return InvokeSdk(
                "MoveJ",
                CreateSdkJointPos(jointPosDeg),
                context.ToolId,
                context.UserId,
                (float)speedPercent,
                (float)accPercent,
                100f,
                CreateSdkExaxisPos(),
                0f,
                (byte)0,
                CreateSdkDescPose(new double[6]));
        }

        public FairinoResult ServoJ(double[] jointPosDeg)
        {
            return FairinoResult.Fail(-6, "Live ServoJ는 v1 하드웨어 bring-up 범위에서 비활성화되어 있습니다.");
        }

        public FairinoResult<FairinoRobotState> ReadState()
        {
            if (!connected)
            {
                return FairinoResult<FairinoRobotState>.Fail(-1, "연결되지 않은 상태입니다.");
            }

            try
            {
                if (TryReadRealtimeState(out var realtimeState))
                {
                    return FairinoResult<FairinoRobotState>.Ok(realtimeState, "실시간 상태 읽기 성공");
                }

                var joints = ReadJointPositionsFallback();
                var tcpPose = ReadTcpPoseFallback();
                return FairinoResult<FairinoRobotState>.Ok(CreateState(joints, tcpPose), "기본 상태 읽기 성공");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[LiveFairinoClient] 상태 읽기 실패: {ex.Message}");
                return FairinoResult<FairinoRobotState>.Fail(-6, ex.Message);
            }
        }

        public FairinoResult MoveL(double[] tcpPose, int speedPercent, int accPercent)
        {
            var preflight = EnsureReadyForLiveMotion();
            if (!preflight.IsSuccess) return preflight;
            if (tcpPose == null || tcpPose.Length != 6)
            {
                return FairinoResult.Fail(-3, "6축 TCP 포즈가 필요합니다.");
            }

            var context = EnsureCoordContext();
            return InvokeSdk(
                "MoveL",
                CreateSdkDescPose(tcpPose),
                context.ToolId,
                context.UserId,
                (float)speedPercent,
                (float)accPercent,
                100f,
                0f,
                0,
                CreateSdkExaxisPos(),
                (byte)0,
                (byte)0,
                CreateSdkDescPose(new double[6]),
                -1,
                0,
                0,
                DefaultLiveMoveLSpeedPercent);
        }

        public FairinoResult StopMotion()
        {
            if (!connected)
            {
                return errorTranslator.ToResult(-1);
            }

            if (HasMethod("StopMotion"))
            {
                return InvokeSdk("StopMotion");
            }

            if (HasMethod("MoveStopJ"))
            {
                return InvokeSdk("MoveStopJ");
            }

            return FairinoResult.Fail(-6, "정지 명령 메서드를 찾을 수 없습니다.");
        }

        public FairinoResult<FairinoVersionInfo> GetVersion()
        {
            if (!connected)
            {
                return FairinoResult<FairinoVersionInfo>.Fail(-1, "연결되지 않은 상태입니다.");
            }

            try
            {
                var sdkVersion = ReadSingleStringByRef("GetSDKVersion");
                var softwareVersion = ReadStringsByRef("GetSoftwareVersion", string.Empty, string.Empty, string.Empty);
                var hardwareVersion = ReadStringsByRef(
                    "GetFirmwareVersion",
                    string.Empty,
                    string.Empty,
                    string.Empty,
                    string.Empty,
                    string.Empty,
                    string.Empty,
                    string.Empty,
                    string.Empty);

                var firmwareSummary = JoinNonEmpty(hardwareVersion);
                var softwareSummary = JoinNonEmpty(softwareVersion);
                var controllerVersion = softwareVersion.Length > 2 ? softwareVersion[2] : string.Empty;

                if (string.IsNullOrWhiteSpace(firmwareSummary))
                {
                    firmwareSummary = "Live";
                }

                if (string.IsNullOrWhiteSpace(sdkVersion))
                {
                    sdkVersion = "Unknown SDK";
                }

                return FairinoResult<FairinoVersionInfo>.Ok(
                    new FairinoVersionInfo(
                        firmwareSummary,
                        sdkVersion,
                        softwareSummary,
                        controllerVersion,
                        firmwareSummary));
            }
            catch (Exception ex)
            {
                Debug.LogError($"[LiveFairinoClient] 버전 읽기 실패: {ex.Message}");
                return FairinoResult<FairinoVersionInfo>.Fail(-6, ex.Message);
            }
        }

        public FairinoResult<int> GetSafetyCode()
        {
            if (!connected)
            {
                return FairinoResult<int>.Fail(-1, "연결되지 않은 상태입니다.");
            }

            try
            {
                var code = InvokeSdkRaw("GetSafetyCode", Array.Empty<object>());
            var safetyCode = ConvertSdkReturnCode(code, "GetSafetyCode");
            lastSafetyCode = safetyCode;
            return FairinoResult<int>.Ok(safetyCode);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[LiveFairinoClient] safety code 읽기 실패: {ex.Message}");
                return FairinoResult<int>.Fail(-6, ex.Message);
            }
        }

        public FairinoResult<int> GetRealtimeStateSamplePeriod()
        {
            if (!connected)
            {
                return FairinoResult<int>.Fail(-1, "연결되지 않은 상태입니다.");
            }

            try
            {
                var args = new object[] { 0 };
                var code = InvokeSdkRaw("GetRobotRealtimeStateSamplePeriod", args);
                var errCode = ConvertSdkReturnCode(code, "GetRobotRealtimeStateSamplePeriod");
                if (errCode != 0)
                {
                    return FairinoResult<int>.Fail(errCode, errorTranslator.Translate(errCode));
                }

                var period = Convert.ToInt32(args[0]);
                lastRealtimeStatePeriodMs = period;
                return FairinoResult<int>.Ok(period);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[LiveFairinoClient] 상태 주기 읽기 실패: {ex.Message}");
                return FairinoResult<int>.Fail(-6, ex.Message);
            }
        }

        public FairinoResult SetRealtimeStateSamplePeriod(int periodMs)
        {
            if (!connected)
            {
                return FairinoResult.Fail(-1, "연결되지 않은 상태입니다.");
            }

            var result = InvokeSdk("SetRobotRealtimeStateSamplePeriod", periodMs);
            if (result.IsSuccess)
            {
                lastRealtimeStatePeriodMs = periodMs;
            }

            return result;
        }

        public FairinoResult ClearMotionQueue()
        {
            if (!connected)
            {
                return FairinoResult.Fail(-1, "연결되지 않은 상태입니다.");
            }

            return InvokeSdk("MotionQueueClear");
        }

        public FairinoResult SetMode(int mode)
        {
            if (!connected)
            {
                return FairinoResult.Fail(-1, "연결되지 않은 상태입니다.");
            }

            return InvokeSdk("Mode", mode);
        }

        public FairinoResult SetReconnect(bool enable, int timeoutMs, int periodMs)
        {
            if (!connected)
            {
                return FairinoResult.Fail(-1, "연결되지 않은 상태입니다.");
            }

            if (HasMethod("SetReConnectParam"))
            {
                return InvokeSdk("SetReConnectParam", enable, timeoutMs, periodMs);
            }

            if (HasMethod("SetReconnectParam"))
            {
                return InvokeSdk("SetReconnectParam", enable, timeoutMs, periodMs);
            }

            return FairinoResult.Ok("SDK reconnect API unavailable");
        }

        public FairinoResult ExitDragTeach()
        {
            if (!connected)
            {
                return FairinoResult.Fail(-1, "연결되지 않은 상태입니다.");
            }

            var result = InvokeSdk("DragTeachSwitch", (byte)0);
            if (result.IsSuccess)
            {
                lastDragTeachActive = false;
            }

            return result;
        }

        public FairinoResult EnsureAutoMode()
        {
            var result = SetMode(0);
            if (result.IsSuccess)
            {
                lastRobotMode = 0;
            }

            return result;
        }

        public FairinoResult<FairinoCoordContext> ReadCoordContext()
        {
            if (!connected)
            {
                return FairinoResult<FairinoCoordContext>.Fail(-1, "연결되지 않은 상태입니다.");
            }

            try
            {
                var context = ReadCoordContextCore();
                lastCoordContext = context;
                return FairinoResult<FairinoCoordContext>.Ok(context);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[LiveFairinoClient] 좌표 문맥 읽기 실패: {ex.Message}");
                return FairinoResult<FairinoCoordContext>.Fail(-6, ex.Message);
            }
        }

        public FairinoResult<FairinoControllerFault> ReadControllerFault()
        {
            if (!connected)
            {
                return FairinoResult<FairinoControllerFault>.Fail(-1, "연결되지 않은 상태입니다.");
            }

            try
            {
                var fault = ReadControllerFaultCore();
                lastControllerFault = fault;
                return FairinoResult<FairinoControllerFault>.Ok(fault);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[LiveFairinoClient] fault 읽기 실패: {ex.Message}");
                return FairinoResult<FairinoControllerFault>.Fail(-6, ex.Message);
            }
        }

        public FairinoResult ResetErrors()
        {
            if (!connected)
            {
                return FairinoResult.Fail(-1, "연결되지 않은 상태입니다.");
            }

            var result = InvokeSdk("ResetAllError");
            if (result.IsSuccess)
            {
                lastControllerFault = FairinoControllerFault.None();
            }

            return result;
        }

        public FairinoResult<FairinoGripperCapability> ProbeGripperCapability()
        {
            if (!connected)
            {
                return FairinoResult<FairinoGripperCapability>.Fail(-1, "연결되지 않은 상태입니다.");
            }

            var capability = new FairinoGripperCapability(
                HasMethod("SetGripperConfig"),
                HasMethod("ActGripper"),
                HasMethod("MoveGripper"),
                HasMethod("GetGripperMotionDone"),
                HasMethod("GetGripperActivateStatus"),
                HasMethod("GetGripperCurPosition"),
                HasMethod("GetGripperCurSpeed"),
                HasMethod("GetGripperCurCurrent"),
                HasMethod("GetGripperVoltage"),
                HasMethod("GetGripperTemp"));
            return FairinoResult<FairinoGripperCapability>.Ok(capability, capability.ToString());
        }

        public FairinoResult<FairinoGripperStatus> ReadGripperStatus()
        {
            if (!connected)
            {
                return FairinoResult<FairinoGripperStatus>.Fail(-1, "연결되지 않은 상태입니다.");
            }

            try
            {
                var motion = ReadPairByRef("GetGripperMotionDone");
                var activation = ReadPairByRef("GetGripperActivateStatus");
                var position = ReadPairByRef("GetGripperCurPosition");
                var speed = ReadPairByRef("GetGripperCurSpeed");
                var current = ReadPairByRef("GetGripperCurCurrent");
                var voltage = ReadPairByRef("GetGripperVoltage");
                var temperature = ReadPairByRef("GetGripperTemp");
                var status = new FairinoGripperStatus(
                    motion.fault,
                    motion.value,
                    activation.fault,
                    activation.value,
                    position.fault,
                    position.value,
                    speed.fault,
                    speed.value,
                    current.fault,
                    current.value,
                    voltage.fault,
                    voltage.value,
                    temperature.fault,
                    temperature.value);
                return FairinoResult<FairinoGripperStatus>.Ok(status, status.ToString());
            }
            catch (Exception ex)
            {
                Debug.LogError($"[LiveFairinoClient] gripper status 읽기 실패: {ex.Message}");
                return FairinoResult<FairinoGripperStatus>.Fail(-6, ex.Message);
            }
        }

        public FairinoResult ConfigureGripper(FairinoGripperProfile profile)
        {
            if (!connected)
            {
                return FairinoResult.Fail(-1, "연결되지 않은 상태입니다.");
            }

            return InvokeSdk("SetGripperConfig", profile.Company, profile.Device, profile.SoftVersion, profile.Bus);
        }

        public FairinoResult ActivateGripper(FairinoGripperProfile profile, bool activate)
        {
            if (!connected)
            {
                return FairinoResult.Fail(-1, "연결되지 않은 상태입니다.");
            }

            return InvokeSdk("ActGripper", profile.Index, activate ? 1 : 0);
        }

        public FairinoResult MoveGripper(FairinoGripperCommand command)
        {
            if (!connected)
            {
                return FairinoResult.Fail(-1, "연결되지 않은 상태입니다.");
            }

            return InvokeSdk(
                "MoveGripper",
                command.Profile.Index,
                command.PositionPercent,
                command.SpeedPercent,
                command.ForcePercent,
                command.MaxTimeMs,
                command.Blocking ? 1 : 0,
                command.GripperType,
                command.RotateTurns,
                command.RotateSpeedPercent,
                command.RotateTorquePercent);
        }

        private FairinoResult InvokeSdk(string methodName, params object[] args)
        {
            try
            {
                var code = InvokeSdkRaw(methodName, args);
                return errorTranslator.ToResult(ConvertSdkReturnCode(code, methodName));
            }
            catch (Exception ex)
            {
                Debug.LogError($"[LiveFairinoClient] {methodName} 실패: {ex.Message}");
                return FairinoResult.Fail(-6, ex.Message);
            }
        }

        private object InvokeSdkRaw(string methodName, object[] args)
        {
            if (sdkRobot == null)
            {
                throw new InvalidOperationException("SDK 인스턴스가 없습니다.");
            }

            var method = ResolveBestMethod(methodName, args);
            if (method == null)
            {
                throw new MissingMethodException($"SDK에 {methodName} 메서드가 없습니다.");
            }

            return method.Invoke(sdkRobot, args);
        }

        private MethodInfo ResolveBestMethod(string methodName, object[] args)
        {
            return sdkRobot.GetType()
                .GetMethods()
                .Where(m => m.Name == methodName)
                .OrderBy(m => ScoreMethod(m, args))
                .FirstOrDefault();
        }

        private static int ScoreMethod(MethodInfo method, object[] args)
        {
            var parameters = method.GetParameters();
            if (parameters.Length != args.Length)
            {
                return int.MaxValue;
            }

            var score = 0;
            for (var i = 0; i < parameters.Length; i++)
            {
                var parameterType = parameters[i].ParameterType;
                if (parameterType.IsByRef)
                {
                    parameterType = parameterType.GetElementType();
                }

                var arg = args[i];
                if (arg == null)
                {
                    score += 1;
                    continue;
                }

                var argType = arg.GetType();
                if (parameterType == argType)
                {
                    continue;
                }

                if (parameterType.IsAssignableFrom(argType))
                {
                    score += 1;
                    continue;
                }

                if (parameterType == typeof(float) && argType == typeof(int))
                {
                    score += 2;
                    continue;
                }

                if (parameterType == typeof(int) && argType == typeof(byte))
                {
                    score += 2;
                    continue;
                }

                if (parameterType == typeof(byte) && argType == typeof(int))
                {
                    score += 3;
                    continue;
                }

                score += 10;
            }

            return score;
        }

        private bool HasMethod(string methodName)
        {
            return sdkRobot != null && sdkRobot.GetType().GetMethods().Any(m => m.Name == methodName);
        }

        private (int fault, int value) ReadPairByRef(string methodName)
        {
            if (!HasMethod(methodName))
            {
                return (-1, 0);
            }

            var args = new object[] { 0, 0 };
            var code = InvokeSdkRaw(methodName, args);
            var errCode = ConvertSdkReturnCode(code, methodName);
            if (errCode != 0)
            {
                return (errCode, 0);
            }

            return (Convert.ToInt32(args[0]), Convert.ToInt32(args[1]));
        }

        private FairinoResult EnsureReadyForLiveMotion()
        {
            if (!connected)
            {
                return errorTranslator.ToResult(-1);
            }

            if (!enabled)
            {
                return errorTranslator.ToResult(-2);
            }

            var stateResult = ReadState();
            if (!stateResult.IsSuccess)
            {
                return new FairinoResult(stateResult.ErrorCode, stateResult.Message);
            }

            var state = stateResult.Value;
            if (state.IsInDragTeach)
            {
                return errorTranslator.ToResult(-8);
            }

            if (state.RobotMode != 0)
            {
                return errorTranslator.ToResult(-7);
            }

            if (state.IsEmergencyStop || state.IsSafetyStop || state.SafetyCode != 0)
            {
                return errorTranslator.ToResult(-4);
            }

            if (state.MainErrorCode != 0 || state.SubErrorCode != 0)
            {
                return errorTranslator.ToResult(-9);
            }

            return FairinoResult.Ok();
        }

        private FairinoResult BestEffortPrepareForLiveMotion()
        {
            var dragResult = ExitDragTeach();
            if (!dragResult.IsSuccess && HasMethod("IsInDragTeach"))
            {
                return dragResult;
            }

            var modeResult = EnsureAutoMode();
            if (!modeResult.IsSuccess)
            {
                return modeResult;
            }

            return FairinoResult.Ok();
        }

        private FairinoCoordContext EnsureCoordContext()
        {
            var result = ReadCoordContext();
            return result.IsSuccess ? result.Value : lastCoordContext;
        }

        private bool TryReadRealtimeState(out FairinoRobotState state)
        {
            state = FairinoRobotState.Zero();
            if (!HasMethod("GetRobotRealTimeState"))
            {
                return false;
            }

            var pkgType = FindSdkType("fairino.ROBOT_STATE_PKG");
            if (pkgType == null)
            {
                return false;
            }

            var pkg = Activator.CreateInstance(pkgType);
            var args = new[] { pkg };
            var result = InvokeSdkRaw("GetRobotRealTimeState", args);
            var errCode = ConvertSdkReturnCode(result, "GetRobotRealTimeState");
            if (errCode != 0)
            {
                throw new InvalidOperationException(errorTranslator.Translate(errCode));
            }

            var payload = args[0];
            var joints = ReadDoubleArrayField(payload, "jt_cur_pos", 6);
            var tcp = ReadDoubleArrayField(payload, "tl_cur_pos", 6);
            lastRobotMode = Convert.ToInt32(GetFieldValue(payload, "robot_mode"));
            lastDragTeachActive = ReadDragTeachStateOrDefault();
            state = CreateState(
                joints,
                tcp,
                robotMode: lastRobotMode,
                motionQueueLength: Convert.ToInt32(GetFieldValue(payload, "mc_queue_len")),
                isEmergencyStop: Convert.ToByte(GetFieldValue(payload, "EmergencyStop")) != 0,
                isCollisionDetected: Convert.ToByte(GetFieldValue(payload, "collisionState")) != 0,
                isRobotEnabled: Convert.ToInt32(GetFieldValue(payload, "rbtEnableState")) != 0,
                isInDragTeach: lastDragTeachActive);
            return true;
        }

        private bool ReadDragTeachStateOrDefault()
        {
            if (!HasMethod("IsInDragTeach"))
            {
                return lastDragTeachActive;
            }

            try
            {
                var args = new object[] { 0 };
                var code = InvokeSdkRaw("IsInDragTeach", args);
                var errCode = ConvertSdkReturnCode(code, "IsInDragTeach");
                if (errCode != 0)
                {
                    return lastDragTeachActive;
                }

                return Convert.ToInt32(args[0]) != 0;
            }
            catch
            {
                return lastDragTeachActive;
            }
        }

        private double[] ReadJointPositionsFallback()
        {
            var jointPos = CreateSdkJointPos(new double[6]);
            var args = new[] { (object)CurrentStateFlag, jointPos };
            var result = InvokeSdkRaw("GetActualJointPosDegree", args);
            var errCode = ConvertSdkReturnCode(result, "GetActualJointPosDegree");
            if (errCode != 0)
            {
                throw new InvalidOperationException(errorTranslator.Translate(errCode));
            }

            return ReadDoubleArrayField(args[1], "jPos", 6);
        }

        private double[] ReadTcpPoseFallback()
        {
            var descPose = CreateSdkDescPose(new double[6]);
            var args = new[] { (object)CurrentStateFlag, descPose };
            var result = InvokeSdkRaw("GetActualTCPPose", args);
            var errCode = ConvertSdkReturnCode(result, "GetActualTCPPose");
            if (errCode != 0)
            {
                throw new InvalidOperationException(errorTranslator.Translate(errCode));
            }

            return ReadPoseFromDescPose(args[1]);
        }

        private FairinoCoordContext ReadCoordContextCore()
        {
            var toolId = ReadIntByRef("GetActualTCPNum", CurrentStateFlag, lastCoordContext.ToolId);
            var userId = ReadIntByRef("GetActualWObjNum", CurrentStateFlag, lastCoordContext.UserId);
            var toolPose = ReadPoseByRef("GetCurToolCoord");
            var wObjPose = ReadPoseByRef("GetCurWObjCoord");
            return new FairinoCoordContext(toolId, userId, toolPose, wObjPose);
        }

        private FairinoControllerFault ReadControllerFaultCore()
        {
            var args = new object[] { 0, 0 };
            var result = InvokeSdkRaw("GetRobotErrorCode", args);
            var errCode = ConvertSdkReturnCode(result, "GetRobotErrorCode");
            if (errCode != 0)
            {
                throw new InvalidOperationException(errorTranslator.Translate(errCode));
            }

            var safetyArgs = new object[] { (byte)0, (byte)0 };
            var safetyResult = InvokeSdkRaw("GetSafetyStopState", safetyArgs);
            var safetyErr = ConvertSdkReturnCode(safetyResult, "GetSafetyStopState");
            var isSafetyStop = false;
            if (safetyErr == 0)
            {
                isSafetyStop = Convert.ToByte(safetyArgs[0]) != 0 || Convert.ToByte(safetyArgs[1]) != 0;
            }

            return new FairinoControllerFault(Convert.ToInt32(args[0]), Convert.ToInt32(args[1]), isSafetyStop);
        }

        private int ReadIntByRef(string methodName, object firstArg, int defaultValue)
        {
            var args = new[] { firstArg, (object)defaultValue };
            var result = InvokeSdkRaw(methodName, args);
            var errCode = ConvertSdkReturnCode(result, methodName);
            if (errCode != 0)
            {
                return defaultValue;
            }

            return Convert.ToInt32(args[1]);
        }

        private double[] ReadPoseByRef(string methodName)
        {
            var pose = CreateSdkDescPose(new double[6]);
            var args = new[] { pose };
            var result = InvokeSdkRaw(methodName, args);
            var errCode = ConvertSdkReturnCode(result, methodName);
            if (errCode != 0)
            {
                return new double[6];
            }

            return ReadPoseFromDescPose(args[0]);
        }

        private string ReadSingleStringByRef(string methodName)
        {
            var args = new object[] { string.Empty };
            var result = InvokeSdkRaw(methodName, args);
            var errCode = ConvertSdkReturnCode(result, methodName);
            if (errCode != 0)
            {
                throw new InvalidOperationException(errorTranslator.Translate(errCode));
            }

            return args[0]?.ToString() ?? string.Empty;
        }

        private string[] ReadStringsByRef(string methodName, params string[] initialValues)
        {
            var args = initialValues.Cast<object>().ToArray();
            var result = InvokeSdkRaw(methodName, args);
            var errCode = ConvertSdkReturnCode(result, methodName);
            if (errCode != 0)
            {
                throw new InvalidOperationException(errorTranslator.Translate(errCode));
            }

            return args.Select(a => a?.ToString() ?? string.Empty).ToArray();
        }

        private static object CreateSdkJointPos(double[] joints)
        {
            var jointPosType = FindSdkType("fairino.JointPos")
                ?? throw new InvalidOperationException("fairino.JointPos 타입을 찾을 수 없습니다.");
            var instance = Activator.CreateInstance(jointPosType);
            SetField(instance, "jPos", (double[])joints.Clone());
            return instance;
        }

        private static object CreateSdkExaxisPos()
        {
            var exaxisType = FindSdkType("fairino.ExaxisPos")
                ?? throw new InvalidOperationException("fairino.ExaxisPos 타입을 찾을 수 없습니다.");
            var instance = Activator.CreateInstance(exaxisType);
            SetField(instance, "ePos", new double[4]);
            return instance;
        }

        private static object CreateSdkDescPose(double[] tcpPose)
        {
            var descPoseType = FindSdkType("fairino.DescPose")
                ?? throw new InvalidOperationException("fairino.DescPose 타입을 찾을 수 없습니다.");
            var descTranType = FindSdkType("fairino.DescTran")
                ?? throw new InvalidOperationException("fairino.DescTran 타입을 찾을 수 없습니다.");
            var rpyType = FindSdkType("fairino.Rpy")
                ?? throw new InvalidOperationException("fairino.Rpy 타입을 찾을 수 없습니다.");

            var descPose = Activator.CreateInstance(descPoseType);
            var tran = Activator.CreateInstance(descTranType);
            var rpy = Activator.CreateInstance(rpyType);

            SetField(tran, "x", tcpPose[0]);
            SetField(tran, "y", tcpPose[1]);
            SetField(tran, "z", tcpPose[2]);
            SetField(rpy, "rx", tcpPose[3]);
            SetField(rpy, "ry", tcpPose[4]);
            SetField(rpy, "rz", tcpPose[5]);

            SetField(descPose, "tran", tran);
            SetField(descPose, "rpy", rpy);
            return descPose;
        }

        private static double[] ReadPoseFromDescPose(object descPose)
        {
            var tran = GetFieldValue(descPose, "tran");
            var rpy = GetFieldValue(descPose, "rpy");
            return new[]
            {
                Convert.ToDouble(GetFieldValue(tran, "x")),
                Convert.ToDouble(GetFieldValue(tran, "y")),
                Convert.ToDouble(GetFieldValue(tran, "z")),
                Convert.ToDouble(GetFieldValue(rpy, "rx")),
                Convert.ToDouble(GetFieldValue(rpy, "ry")),
                Convert.ToDouble(GetFieldValue(rpy, "rz"))
            };
        }

        private static double[] ReadDoubleArrayField(object instance, string fieldName, int expectedLength)
        {
            var raw = GetFieldValue(instance, fieldName) as double[];
            if (raw == null)
            {
                return new double[expectedLength];
            }

            if (raw.Length == expectedLength)
            {
                return (double[])raw.Clone();
            }

            var copy = new double[expectedLength];
            Array.Copy(raw, copy, System.Math.Min(raw.Length, expectedLength));
            return copy;
        }

        private static void SetField(object instance, string fieldName, object value)
        {
            var field = instance.GetType().GetField(fieldName);
            if (field == null)
            {
                throw new MissingFieldException(instance.GetType().FullName, fieldName);
            }

            field.SetValue(instance, value);
        }

        private static object GetFieldValue(object instance, string fieldName)
        {
            var field = instance.GetType().GetField(fieldName);
            if (field == null)
            {
                throw new MissingFieldException(instance.GetType().FullName, fieldName);
            }

            return field.GetValue(instance);
        }

        private static int ConvertSdkReturnCode(object sdkResult, string methodName)
        {
            if (sdkResult is int intCode)
            {
                return intCode;
            }

            if (sdkResult is byte byteCode)
            {
                return byteCode;
            }

            throw new InvalidOperationException($"{methodName} returned unsupported result type '{sdkResult?.GetType().FullName ?? "null"}'.");
        }

        private FairinoRobotState CreateState(
            double[] joints,
            double[] tcp,
            int robotMode = 0,
            int motionQueueLength = 0,
            bool isEmergencyStop = false,
            bool isCollisionDetected = false,
            bool isRobotEnabled = false,
            bool isInDragTeach = false)
        {
            var coordContext = lastCoordContext;
            var controllerFault = lastControllerFault;
            return new FairinoRobotState(
                joints,
                tcp,
                robotMode: robotMode,
                motionQueueLength: motionQueueLength,
                safetyCode: lastSafetyCode,
                realtimeStateSamplePeriodMs: lastRealtimeStatePeriodMs,
                mainErrorCode: controllerFault.MainCode,
                subErrorCode: controllerFault.SubCode,
                toolId: coordContext.ToolId,
                userId: coordContext.UserId,
                isEmergencyStop: isEmergencyStop,
                isCollisionDetected: isCollisionDetected,
                isRobotEnabled: isRobotEnabled,
                isInDragTeach: isInDragTeach,
                isSafetyStop: controllerFault.IsSafetyStop);
        }

        private static string JoinNonEmpty(params string[] values)
        {
            return string.Join(" | ", values.Where(v => !string.IsNullOrWhiteSpace(v)));
        }

        private static Type FindSdkType(string fullName)
        {
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                var type = asm.GetType(fullName);
                if (type != null)
                {
                    return type;
                }
            }

            return null;
        }
    }
}
