// Folder: App - Application controllers and services; single UnityEngine entry point.
namespace KineTutor3D.App.Fairino
{
    /// <summary>
    /// FAIRINO 로봇 버전 정보입니다.
    /// </summary>
    public readonly struct FairinoVersionInfo
    {
        /// <summary>
        /// 펌웨어 버전입니다.
        /// </summary>
        public string FirmwareVersion { get; }

        /// <summary>
        /// SDK 버전입니다.
        /// </summary>
        public string SdkVersion { get; }

        /// <summary>
        /// 소프트웨어 버전 요약입니다.
        /// </summary>
        public string SoftwareVersion { get; }

        /// <summary>
        /// 컨트롤러 버전 요약입니다.
        /// </summary>
        public string ControllerVersion { get; }

        /// <summary>
        /// 하드웨어 버전 요약입니다.
        /// </summary>
        public string HardwareVersion { get; }

        /// <summary>
        /// 버전 정보를 생성합니다.
        /// </summary>
        public FairinoVersionInfo(string firmwareVersion, string sdkVersion)
            : this(firmwareVersion, sdkVersion, string.Empty, string.Empty, string.Empty)
        {
        }

        /// <summary>
        /// 확장 버전 정보를 생성합니다.
        /// </summary>
        public FairinoVersionInfo(
            string firmwareVersion,
            string sdkVersion,
            string softwareVersion,
            string controllerVersion,
            string hardwareVersion)
        {
            FirmwareVersion = firmwareVersion ?? string.Empty;
            SdkVersion = sdkVersion ?? string.Empty;
            SoftwareVersion = softwareVersion ?? string.Empty;
            ControllerVersion = controllerVersion ?? string.Empty;
            HardwareVersion = hardwareVersion ?? string.Empty;
        }
    }
}
