// Folder: App - Application controllers and services; single UnityEngine entry point.
namespace KineTutor3D.App.Fairino
{
    /// <summary>
    /// Live/bridge 클라이언트의 SDK 진단 상태를 외부 기록기가 읽을 수 있게 노출합니다.
    /// </summary>
    public interface IFairinoLiveClientDiagnostics
    {
        string ClientMode { get; }
        string SdkLoadStatus { get; }
        string SdkVersion { get; }
        string SdkRuntime { get; }
        bool IsReadbackOnly { get; }
    }

    /// <summary>
    /// readback-only 래퍼가 내부 motion-capable live 세션을 재사용할 수 있을 때 노출하는 계약입니다.
    /// </summary>
    public interface IFairinoMotionSessionProvider
    {
        bool TryGetMotionCapableClient(out IFairinoRobotClient motionClient);
    }
}
