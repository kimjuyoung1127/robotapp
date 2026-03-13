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
        /// 버전 정보를 생성합니다.
        /// </summary>
        public FairinoVersionInfo(string firmwareVersion, string sdkVersion)
        {
            FirmwareVersion = firmwareVersion ?? string.Empty;
            SdkVersion = sdkVersion ?? string.Empty;
        }
    }
}
