// Folder: UI - HUD/view components only; no kinematics logic.
namespace KineTutor3D.UI
{
    /// <summary>
    /// HandInputDiagnosticsState를 RobotControl 진단 서랍용 문자열로 변환합니다.
    /// </summary>
    internal static class HandInputDiagnosticsFormatter
    {
        private const string SuccessHintText = "Step 1 성공 기준: 여기 상태가 Fresh로 바뀌면 최소 연결 확인입니다.";

        public static string FormatBody(HandInputDiagnosticsState state)
        {
            if (state == null || !state.IsConfigured)
            {
                return "Hand Input: 미구성\n현재 씬에 hand input source가 연결되지 않았습니다.";
            }

            switch (state.Mode)
            {
                case "Fresh":
                    return
                        "Hand Input: Fresh\n" +
                        FormatEndpoint(state) +
                        $"Seq: {state.Sequence}\n" +
                        $"Tracked: {FormatYesNo(state.IsTracked)}\n" +
                        $"X/Y: {state.HandX:F2} / {state.HandY:F2}\n" +
                        $"Pinch: {state.Pinch:F2}\n" +
                        $"Source: {state.SourceId}\n\n" +
                        SuccessHintText;
                case "Stale":
                    return
                        "Hand Input: Stale\n" +
                        FormatEndpoint(state) +
                        $"Seq: {state.Sequence}\n" +
                        $"Tracked: {FormatYesNo(state.IsTracked)}\n" +
                        $"X/Y: {state.HandX:F2} / {state.HandY:F2}\n" +
                        $"Pinch: {state.Pinch:F2}\n" +
                        $"Source: {state.SourceId}\n" +
                        $"Timeout: {state.TimeoutSeconds:F2} sec\n\n" +
                        "최근 샘플은 있었지만 현재는 fresh 상태가 아닙니다.";
                case "Listening":
                    return
                        "Hand Input: Listening\n" +
                        $"Port: {state.ListenPort}\n" +
                        $"Allow: {state.AllowedSenderIp}\n" +
                        $"Listening: {FormatYesNo(state.IsListening)}\n\n" +
                        "패킷이 아직 들어오지 않았습니다.";
                default:
                    return "Hand Input: 대기 중";
            }
        }

        private static string FormatEndpoint(HandInputDiagnosticsState state)
        {
            var sender = string.IsNullOrWhiteSpace(state.SenderIp) ? "-" : state.SenderIp;
            return $"Port: {state.ListenPort}\nAllow: {state.AllowedSenderIp}\nSender: {sender}\n";
        }

        private static string FormatYesNo(bool value)
        {
            return value ? "Yes" : "No";
        }
    }
}
