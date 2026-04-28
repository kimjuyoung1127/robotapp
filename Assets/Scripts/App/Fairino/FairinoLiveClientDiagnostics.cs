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
}
