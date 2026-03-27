// Folder: App - External hand-tracking input payloads and adapters for RobotControl.
using System;

namespace KineTutor3D.App.HandTracking
{
    /// <summary>
    /// 손 추적 입력 공급원의 최소 인터페이스입니다.
    /// UDP, WebSocket, XR 등 소스 교체를 쉽게 하기 위해 사용합니다.
    /// </summary>
    public interface IHandPoseSource
    {
        event Action<HandPoseSample> OnSampleReceived;

        bool HasFreshSample { get; }

        HandPoseSample LatestSample { get; }

        float SampleTimeoutSeconds { get; }

        bool TryGetLatestSample(out HandPoseSample sample);
    }
}
