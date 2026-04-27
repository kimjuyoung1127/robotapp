// Folder: App - Application controllers and services; single UnityEngine entry point.
namespace KineTutor3D.App.Fairino
{
    /// <summary>
    /// FAIRINO SDK gripper 설정과 명령 파라미터입니다.
    /// </summary>
    public readonly struct FairinoGripperProfile
    {
        public FairinoGripperProfile(int company, int device, int softVersion, int bus, int index)
        {
            Company = company;
            Device = device;
            SoftVersion = softVersion;
            Bus = bus;
            Index = index;
        }

        public int Company { get; }
        public int Device { get; }
        public int SoftVersion { get; }
        public int Bus { get; }
        public int Index { get; }

        public static FairinoGripperProfile Pgea10040Default => new(4, 0, 0, 2, 2);

        public override string ToString()
        {
            return $"company={Company}; device={Device}; soft={SoftVersion}; bus={Bus}; index={Index}";
        }
    }

    /// <summary>
    /// Pendant 사용자 개도율, FAIRINO SDK raw percent, Unity visual pose를 분리하는 그리퍼 보정값입니다.
    /// </summary>
    internal readonly struct GripperCalibrationProfile
    {
        public GripperCalibrationProfile(int closedRawPercent, int openRawPercent, int objectStopRawPercent)
        {
            ClosedRawPercent = ClampPercent(closedRawPercent);
            OpenRawPercent = ClampPercent(openRawPercent);
            ObjectStopRawPercent = ClampPercent(objectStopRawPercent);
        }

        public int ClosedRawPercent { get; }
        public int OpenRawPercent { get; }
        public int ObjectStopRawPercent { get; }

        public static GripperCalibrationProfile Pgea10040Observed => new(60, 100, 70);

        public int UserToRawPercent(int userPercent)
        {
            var user = ClampPercent(userPercent) / 100f;
            return ClampPercent(UnityEngine.Mathf.RoundToInt(UnityEngine.Mathf.Lerp(ClosedRawPercent, OpenRawPercent, user)));
        }

        public int RawToUserPercent(int rawPercent)
        {
            var raw = ClampPercent(rawPercent);
            if (OpenRawPercent == ClosedRawPercent)
            {
                return raw >= OpenRawPercent ? 100 : 0;
            }

            var user = UnityEngine.Mathf.InverseLerp(ClosedRawPercent, OpenRawPercent, raw);
            return ClampPercent(UnityEngine.Mathf.RoundToInt(user * 100f));
        }

        public float UserToVisualOpenRatio(int userPercent)
        {
            return ClampPercent(userPercent) / 100f;
        }

        public override string ToString()
        {
            return $"closedRaw={ClosedRawPercent}; openRaw={OpenRawPercent}; objectStopRaw={ObjectStopRawPercent}";
        }

        private static int ClampPercent(int value)
        {
            return value < 0 ? 0 : value > 100 ? 100 : value;
        }
    }

    /// <summary>
    /// FAIRINO SDK MoveGripper 명령 파라미터입니다.
    /// </summary>
    public readonly struct FairinoGripperCommand
    {
        public FairinoGripperCommand(
            FairinoGripperProfile profile,
            int positionPercent,
            int speedPercent,
            int forcePercent,
            int maxTimeMs,
            bool blocking,
            int gripperType = 0,
            double rotateTurns = 0,
            int rotateSpeedPercent = 0,
            int rotateTorquePercent = 0)
        {
            Profile = profile;
            PositionPercent = ClampPercent(positionPercent);
            SpeedPercent = ClampPercent(speedPercent);
            ForcePercent = ClampPercent(forcePercent);
            MaxTimeMs = maxTimeMs < 0 ? 0 : maxTimeMs > 30000 ? 30000 : maxTimeMs;
            Blocking = blocking;
            GripperType = gripperType;
            RotateTurns = rotateTurns;
            RotateSpeedPercent = ClampPercent(rotateSpeedPercent);
            RotateTorquePercent = ClampPercent(rotateTorquePercent);
        }

        public FairinoGripperProfile Profile { get; }
        public int PositionPercent { get; }
        public int SpeedPercent { get; }
        public int ForcePercent { get; }
        public int MaxTimeMs { get; }
        public bool Blocking { get; }
        public int GripperType { get; }
        public double RotateTurns { get; }
        public int RotateSpeedPercent { get; }
        public int RotateTorquePercent { get; }

        public static FairinoGripperCommand ForOpen(bool open)
        {
            return ForPosition(open ? 100 : 0);
        }

        public static FairinoGripperCommand ForPosition(int positionPercent, int speedPercent = 50, int forcePercent = 50)
        {
            return new FairinoGripperCommand(
                FairinoGripperProfile.Pgea10040Default,
                positionPercent,
                speedPercent,
                forcePercent,
                30000,
                blocking: true);
        }

        public override string ToString()
        {
            return $"{Profile}; pos={PositionPercent}; vel={SpeedPercent}; force={ForcePercent}; max={MaxTimeMs}; block={Blocking}; type={GripperType}";
        }

        private static int ClampPercent(int value)
        {
            return value < 0 ? 0 : value > 100 ? 100 : value;
        }
    }

    /// <summary>
    /// SDK gripper method 존재 여부와 readback 가능성을 표시합니다.
    /// </summary>
    public readonly struct FairinoGripperCapability
    {
        public FairinoGripperCapability(
            bool canConfigure,
            bool canActivate,
            bool canMove,
            bool canReadMotion,
            bool canReadActivation,
            bool canReadPosition,
            bool canReadSpeed,
            bool canReadCurrent,
            bool canReadVoltage,
            bool canReadTemperature)
        {
            CanConfigure = canConfigure;
            CanActivate = canActivate;
            CanMove = canMove;
            CanReadMotion = canReadMotion;
            CanReadActivation = canReadActivation;
            CanReadPosition = canReadPosition;
            CanReadSpeed = canReadSpeed;
            CanReadCurrent = canReadCurrent;
            CanReadVoltage = canReadVoltage;
            CanReadTemperature = canReadTemperature;
        }

        public bool CanConfigure { get; }
        public bool CanActivate { get; }
        public bool CanMove { get; }
        public bool CanReadMotion { get; }
        public bool CanReadActivation { get; }
        public bool CanReadPosition { get; }
        public bool CanReadSpeed { get; }
        public bool CanReadCurrent { get; }
        public bool CanReadVoltage { get; }
        public bool CanReadTemperature { get; }
        public bool CanUseLiveGripper => CanConfigure && CanActivate && CanMove;

        public override string ToString()
        {
            return $"configure={CanConfigure}; activate={CanActivate}; move={CanMove}; motion={CanReadMotion}; active={CanReadActivation}; pos={CanReadPosition}; speed={CanReadSpeed}; current={CanReadCurrent}; voltage={CanReadVoltage}; temp={CanReadTemperature}";
        }
    }

    /// <summary>
    /// FAIRINO SDK gripper 상태 readback입니다.
    /// </summary>
    public readonly struct FairinoGripperStatus
    {
        public FairinoGripperStatus(
            int motionFault,
            int motionDone,
            int activationFault,
            int activationMask,
            int positionFault,
            int positionPercent,
            int speedFault,
            int speedPercent,
            int currentFault,
            int currentPercent,
            int voltageFault,
            int voltage,
            int temperatureFault,
            int temperature)
        {
            MotionFault = motionFault;
            MotionDone = motionDone;
            ActivationFault = activationFault;
            ActivationMask = activationMask;
            PositionFault = positionFault;
            PositionPercent = positionPercent;
            SpeedFault = speedFault;
            SpeedPercent = speedPercent;
            CurrentFault = currentFault;
            CurrentPercent = currentPercent;
            VoltageFault = voltageFault;
            Voltage = voltage;
            TemperatureFault = temperatureFault;
            Temperature = temperature;
        }

        public int MotionFault { get; }
        public int MotionDone { get; }
        public int ActivationFault { get; }
        public int ActivationMask { get; }
        public int PositionFault { get; }
        public int PositionPercent { get; }
        public int SpeedFault { get; }
        public int SpeedPercent { get; }
        public int CurrentFault { get; }
        public int CurrentPercent { get; }
        public int VoltageFault { get; }
        public int Voltage { get; }
        public int TemperatureFault { get; }
        public int Temperature { get; }

        public override string ToString()
        {
            return $"motionFault={MotionFault}; done={MotionDone}; activationFault={ActivationFault}; activationMask={ActivationMask}; positionFault={PositionFault}; position={PositionPercent}; speedFault={SpeedFault}; speed={SpeedPercent}; currentFault={CurrentFault}; current={CurrentPercent}; voltageFault={VoltageFault}; voltage={Voltage}; tempFault={TemperatureFault}; temp={Temperature}";
        }
    }
}
