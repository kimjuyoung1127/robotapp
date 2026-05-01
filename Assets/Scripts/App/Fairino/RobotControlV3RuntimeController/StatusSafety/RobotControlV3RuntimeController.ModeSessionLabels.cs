// Folder: StatusSafety - controller mode, session labels, and context formatting for V3 status surfaces.
// Serves connection/status chips and live session wording shared across V3 diagnostics.
// Snapshot assembly and operator action copy live in sibling StatusSafety partials.
namespace KineTutor3D.App.Fairino
{
    public sealed partial class RobotControlV3RuntimeController
    {
        private static string FormatCoordSystemDisplay(string coordSystem)
        {
            return coordSystem switch
            {
                "Tool" => "툴 기준",
                "User" => "작업 기준",
                "Base" => "로봇 기준",
                _ => string.IsNullOrWhiteSpace(coordSystem) ? "--" : coordSystem,
            };
        }

        private string ResolveControllerModeLabel()
        {
            if (connectionService == null)
            {
                return "--";
            }

            if (connectionService.IsMockMode)
            {
                return "프리뷰";
            }

            if (currentState.IsInDragTeach)
            {
                return "티칭";
            }

            return currentState.RobotMode switch
            {
                0 => "자동",
                1 => "수동",
                _ => $"모드 {currentState.RobotMode}",
            };
        }

        private string BuildControllerSessionSummary(bool readbackOnlyLive)
        {
            if (connectionService == null)
            {
                return "컨트롤러: --";
            }

            if (connectionService.IsMockMode)
            {
                return "컨트롤러: 화면 프리뷰 세션";
            }

            var sessionSummary = readbackOnlyLive
                ? "읽기 전용"
                : "실기 쓰기 가능";
            var changedAt = lastControllerTruthChangedUtc == System.DateTime.MinValue
                ? "truth-change=unknown"
                : $"truth-change={lastControllerTruthChangedUtc.ToLocalTime():HH:mm:ss}";
            return $"컨트롤러: {sessionSummary} · {lastControllerTruthSummary} · {lastModeTransitionSummary} · {changedAt}";
        }

        private static string BuildLiveSessionModeDisplay(LiveCommandSessionMode mode)
        {
            return mode switch
            {
                LiveCommandSessionMode.LiveControl => "live-control",
                LiveCommandSessionMode.LoopRunning => "loop-running",
                LiveCommandSessionMode.GripperOnly => "gripper-only",
                LiveCommandSessionMode.TinyMoveJOnly => "tiny-movej-only",
                _ => "readback-only",
            };
        }

        private static string BuildLiveSessionModeSummary(LiveCommandSessionMode mode)
        {
            return mode switch
            {
                LiveCommandSessionMode.LiveControl => "실기 세션: live write 가능",
                LiveCommandSessionMode.LoopRunning => "실기 세션: mixed live loop 실행 중",
                LiveCommandSessionMode.GripperOnly => "실기 세션: gripper-only live write",
                LiveCommandSessionMode.TinyMoveJOnly => "실기 세션: joint-only live write",
                _ => "실기 세션: 읽기 전용",
            };
        }

        private string ResolveControllerModeChipClass()
        {
            if (connectionService == null)
            {
                return "rc-status-chip--muted";
            }

            if (connectionService.IsMockMode || currentState.IsInDragTeach || currentState.RobotMode == 1)
            {
                return "rc-status-chip--warning";
            }

            return "rc-status-chip--success";
        }

        private string ResolveControllerModeValueClass()
        {
            if (connectionService == null)
            {
                return "rc-status-value--muted";
            }

            if (connectionService.IsMockMode || currentState.IsInDragTeach || currentState.RobotMode == 1)
            {
                return "rc-status-value--warning";
            }

            return "rc-status-value--success";
        }

        private bool ResolveAutoModeSwitchEnabled()
        {
            return connectionService != null
                && connectionService.Client.IsConnected
                && !connectionService.IsMockMode
                && (currentState.IsInDragTeach || currentState.RobotMode != 0);
        }

        private bool ResolveManualModeSwitchEnabled()
        {
            return connectionService != null
                && connectionService.Client.IsConnected
                && !connectionService.IsMockMode
                && currentState.RobotMode != 1;
        }

        private static string FormatContextId(int id)
        {
            return id > 0 ? $"{id}번" : "미확인";
        }

        private static LiveCommandSessionMode ParseLiveCommandSessionMode(string sessionMode)
        {
            if (string.IsNullOrWhiteSpace(sessionMode))
            {
                return LiveCommandSessionMode.LiveControl;
            }

            return sessionMode.Trim().ToLowerInvariant() switch
            {
                "live-control" or "live" or "unified" => LiveCommandSessionMode.LiveControl,
                "loop-running" or "loop" or "mixed-live-loop" => LiveCommandSessionMode.LoopRunning,
                "gripper-only" or "gripper" => LiveCommandSessionMode.GripperOnly,
                "tiny-movej-only" or "tiny-movej" or "tinymovej-only" or "tinymovej" => LiveCommandSessionMode.TinyMoveJOnly,
                _ => LiveCommandSessionMode.LiveControl,
            };
        }

        private static string FormatToolDisplay(int toolId)
        {
            return toolId > 0 ? $"도구 {toolId}번" : "도구 미확인";
        }

        private static string FormatUserDisplay(int userId)
        {
            return userId > 0 ? $"작업 기준 {userId}번" : "작업 기준 미확인";
        }
    }
}
