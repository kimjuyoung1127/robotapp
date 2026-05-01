// Folder: StatusSafety - operator-facing quick actions and failure copy for V3 status surfaces.
// Serves header/status-card wording and failure category summaries.
// Session labels and gate detail formatting live in sibling StatusSafety partials.
namespace KineTutor3D.App.Fairino
{
    public sealed partial class RobotControlV3RuntimeController
    {
        private string ResolveQuickActionLabel()
        {
            if (snapshot.StatusKind == RobotControlV3RuntimeStatusKind.ConnectedServoOff && IsReadbackOnlyLiveClient())
            {
                return hasCurrentPositionReadComplete ? "연결 완료" : "현재 위치 다시 읽기";
            }

            return snapshot.StatusKind switch
            {
                RobotControlV3RuntimeStatusKind.Disconnected => "연결 + 위치 읽기",
                RobotControlV3RuntimeStatusKind.ConnectedServoOff => "서보 켜기",
                RobotControlV3RuntimeStatusKind.ConnectedUnsynced => "동기화",
                RobotControlV3RuntimeStatusKind.Fault => "오류 초기화",
                _ => "조작 시작",
            };
        }

        private bool ResolveQuickActionEnabled()
        {
            if (snapshot.StatusKind == RobotControlV3RuntimeStatusKind.ConnectedServoOff &&
                IsReadbackOnlyLiveClient() &&
                hasCurrentPositionReadComplete)
            {
                return false;
            }

            return snapshot.StatusKind != RobotControlV3RuntimeStatusKind.AutoReconnect;
        }

        private string BuildActionNow()
        {
            if (snapshot.StatusKind == RobotControlV3RuntimeStatusKind.ConnectedServoOff && IsReadbackOnlyLiveClient())
            {
                return hasCurrentPositionReadComplete
                    ? "지금 상태: 현재 위치 확인이 끝났습니다."
                    : "지금 상태: 연결은 됐고, 현재 위치 확인 전입니다.";
            }

            return snapshot.StatusKind switch
            {
                RobotControlV3RuntimeStatusKind.Disconnected => "지금 상태: 아직 미연결",
                RobotControlV3RuntimeStatusKind.ConnectedServoOff => "지금 상태: 연결됨 / 서보 OFF",
                RobotControlV3RuntimeStatusKind.ConnectedUnsynced => "지금 상태: 서보 ON / 아직 미동기화",
                RobotControlV3RuntimeStatusKind.Fault => "지금 상태: Fault 발생",
                _ => snapshot.DryRunEnabled ? "지금 상태: DryRun 시뮬레이션 가능" : "지금 상태: 조작 가능",
            };
        }

        private string BuildActionPrimary()
        {
            if (snapshot.StatusKind == RobotControlV3RuntimeStatusKind.ConnectedServoOff && IsReadbackOnlyLiveClient())
            {
                return hasCurrentPositionReadComplete
                    ? "다음 행동: 연결 완료"
                    : "다음 행동: 현재 위치 다시 읽기";
            }

            return snapshot.StatusKind switch
            {
                RobotControlV3RuntimeStatusKind.Disconnected => "다음 행동: 연결하고 현재 위치 읽기",
                RobotControlV3RuntimeStatusKind.ConnectedServoOff => "다음 행동: 서보를 먼저 켜기",
                RobotControlV3RuntimeStatusKind.ConnectedUnsynced => "다음 행동: 동기화 먼저",
                RobotControlV3RuntimeStatusKind.Fault => "다음 행동: 오류 초기화부터",
                _ => snapshot.PendingCommandSummary,
            };
        }

        private string BuildActionWhy()
        {
            if (snapshot.StatusKind == RobotControlV3RuntimeStatusKind.ConnectedServoOff && IsReadbackOnlyLiveClient())
            {
                return hasCurrentPositionReadComplete
                    ? "연결과 현재 위치 읽기가 함께 끝나서 화면과 실제 로봇 위치를 바로 비교할 수 있습니다."
                    : "지금은 실제로 움직이지 않고, 화면 위치와 실제 로봇 위치가 맞는지부터 확인하는 단계입니다.";
            }

            return snapshot.StatusKind switch
            {
                RobotControlV3RuntimeStatusKind.Disconnected => "현재 상태를 읽으려면 연결부터 살아 있어야 한다.",
                RobotControlV3RuntimeStatusKind.ConnectedServoOff => "실제 이동을 보내려면 서보가 먼저 살아 있어야 한다.",
                RobotControlV3RuntimeStatusKind.ConnectedUnsynced => "첫 조작 전에 현재 자세를 읽는 게 덜 위험하다.",
                RobotControlV3RuntimeStatusKind.Fault => "초기화부터 누르면 같은 Fault를 다시 밟을 수 있다.",
                _ => snapshot.DryRunEnabled ? "지금은 실제 로봇 대신 화면 안에서만 미리보기 중입니다." : "지금 화면의 적용 버튼은 실제 로봇 동작으로 이어질 수 있습니다.",
            };
        }

        private string BuildOperatorNextAction()
        {
            return BuildOperatorNextAction(BuildFailureCategory(), snapshot.MotionGateNextStep);
        }

        private static string BuildOperatorNextAction(string failureCategory, string fallbackAction)
        {
            return failureCategory switch
            {
                "network/SDK unavailable" => "8080 연결과 현재 위치 읽기를 다시 확인",
                "mode != 0" => "헤더 자동 버튼으로 자동 모드 전환",
                "drag/teach still on" => "티칭/드래그를 끄고 자동 모드를 다시 확인",
                "servo not ready" => "서보 ON 후 다시 미리보기/적용",
                "controller fault present" => "오류 초기화 후 현재 위치를 다시 읽기",
                "tool/user/coord missing" => "tool/user/coord를 다시 읽어 기준을 확정",
                "evidence stale" => "현재 위치 읽기와 latest-state/latest-drift를 다시 갱신",
                "gripper activation not ready" => "그리퍼 warm-up 뒤 다시 적용",
                "tiny range exceeded" => $"각 관절 변화량을 {RobotControlMotionRuntime.TinyMoveJMaxJointDeltaDeg:0.#}도 이내로 줄여 다시 적용",
                "sequence loop still locked" => "반복 대신 1회 실행만 사용",
                _ => StripActionPrefix(fallbackAction),
            };
        }

        private string BuildFailureCategory()
        {
            var liveBlocked = ResolveEffectiveOperatorBlockedReason();
            var lastFeedbackText = snapshot.LastFeedback ?? string.Empty;
            return ClassifyFailureCategory(liveBlocked, lastFeedbackText);
        }

        private string ClassifyFailureCategory(string liveBlocked, string lastFeedbackText)
        {
            if (!connectionService.Client.IsConnected
                || liveBlocked.Contains("8080")
                || liveBlocked.Contains("포트 확인 실패")
                || liveBlocked.Contains("not connected"))
            {
                return "network/SDK unavailable";
            }

            if (currentState.IsInDragTeach)
            {
                return "drag/teach still on";
            }

            if (!connectionService.IsMockMode && currentState.RobotMode != 0)
            {
                return "mode != 0";
            }

            if (connectionService.LastControllerFault.HasBlockingFault || liveBlocked.Contains("fault active"))
            {
                return "controller fault present";
            }

            if (!IsReadbackOnlyLiveClient() && !connectionService.Client.IsEnabled)
            {
                return "servo not ready";
            }

            if (liveBlocked.Contains("gripper activation not ready")
                || lastFeedbackText.Contains("gripper activation not ready"))
            {
                return "gripper activation not ready";
            }

            if (liveBlocked.Contains("tiny MoveJ range exceeded")
                || lastFeedbackText.Contains("tiny MoveJ range exceeded"))
            {
                return "tiny range exceeded";
            }

            if (liveBlocked.Contains("latest-state freshness failed")
                || liveBlocked.Contains("latest-drift freshness failed")
                || liveBlocked.Contains("state readback failed")
                || liveBlocked.Contains("drift threshold failed"))
            {
                return "evidence stale";
            }

            if (liveBlocked.Contains("toolId missing")
                || liveBlocked.Contains("userId missing")
                || liveBlocked.Contains("coordSystem unresolved"))
            {
                return "tool/user/coord missing";
            }

            if (liveBlocked.Contains("반복 live 실행은 아직 잠겨 있다")
                || lastFeedbackText.Contains("반복 live 실행은 아직 잠겨 있다"))
            {
                return "sequence loop still locked";
            }

            return "ready";
        }

        private static string StripActionPrefix(string action)
        {
            if (string.IsNullOrWhiteSpace(action))
            {
                return "먼저 연결";
            }

            const string prefix = "다음 행동: ";
            return action.StartsWith(prefix, System.StringComparison.Ordinal)
                ? action.Substring(prefix.Length)
                : action;
        }
    }
}
