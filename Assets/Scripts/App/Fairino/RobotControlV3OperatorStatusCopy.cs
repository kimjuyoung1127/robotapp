// Folder: App - Application controllers and services; single UnityEngine entry point.
namespace KineTutor3D.App.Fairino
{
    /// <summary>
    /// Pendant V3 운영자 노출용 상태 문구를 한 곳에서 정리합니다.
    /// </summary>
    internal static class RobotControlV3OperatorStatusCopy
    {
        internal static string BuildRepresentativeStatus(
            bool isConnected,
            bool hasCurrentPositionReadComplete,
            bool actualMoveLocked)
        {
            if (!isConnected)
            {
                return "미연결";
            }

            if (!hasCurrentPositionReadComplete)
            {
                return "연결됨 · 위치 확인 전";
            }

            if (actualMoveLocked)
            {
                return "실제 이동: 잠겨 있음";
            }

            return "연결됨 · 위치 확인 완료";
        }

        internal static string BuildConnectionStatusValue(bool isConnected, bool hasCurrentPositionReadComplete)
        {
            if (!isConnected)
            {
                return "미연결";
            }

            return hasCurrentPositionReadComplete
                ? "연결됨 · 위치 확인 완료"
                : "연결됨 · 위치 확인 전";
        }

        internal static string BuildConnectionChip(bool isConnected, bool hasCurrentPositionReadComplete)
        {
            return $"연결: {BuildConnectionStatusValue(isConnected, hasCurrentPositionReadComplete)}";
        }

        internal static string BuildConnectionCardStatus(
            bool isConnected,
            bool hasCurrentPositionReadComplete,
            bool actualMoveLocked)
        {
            return $"대표 상태: {BuildRepresentativeStatus(isConnected, hasCurrentPositionReadComplete, actualMoveLocked)}";
        }

        internal static string BuildCurrentPositionReadStatus(bool isConnected, bool hasCurrentPositionReadComplete)
        {
            if (!isConnected)
            {
                return "현재 위치 읽음: 아직 안 함";
            }

            return hasCurrentPositionReadComplete
                ? "현재 위치 읽음: 완료"
                : "현재 위치 읽음: 아직 안 함";
        }

        internal static string BuildLiveTrackingStatus(
            bool isConnected,
            bool hasCurrentPositionReadComplete,
            bool prioritizeLiveReadback,
            bool hasPendingPreview,
            bool readbackOnlyLive)
        {
            if (!isConnected)
            {
                return "실시간 추적 상태: 중지됨";
            }

            if (!hasCurrentPositionReadComplete)
            {
                return readbackOnlyLive
                    ? "실시간 추적 상태: 위치 확인을 기다리는 중"
                    : "실시간 추적 상태: 현재 위치 확인 전";
            }

            if (prioritizeLiveReadback)
            {
                return hasPendingPreview
                    ? "실시간 추적 상태: 실제 값과 미리보기를 함께 보는 중"
                    : "실시간 추적 상태: 실제 로봇 값을 반영 중";
            }

            return hasPendingPreview
                ? "실시간 추적 상태: 미리보기 후보를 확인 중"
                : "실시간 추적 상태: 최신 값을 유지 중";
        }
    }
}
