// Folder: StatusSafety - live gate detail formatting and unlock guidance for V3 status surfaces.
// Serves diagnostics summaries and next-step guidance for blocked live commands.
// Snapshot assembly and mode/session labels live in sibling StatusSafety partials.
using System;
using System.Collections.Generic;

namespace KineTutor3D.App.Fairino
{
    public sealed partial class RobotControlV3RuntimeController
    {
        private string BuildReadbackOnlyGateDetail()
        {
            return $"지금은 실제 이동 없이 상태만 확인하는 단계입니다. {snapshot.StatusTool}, {snapshot.StatusUser}, 좌표 기준 {FormatCoordSystemDisplay(snapshot.CoordSystem)}으로 현재 위치가 맞는지 먼저 확인하세요.";
        }

        private string BuildMotionGateWhyLocked(LiveCommandSafetyGateResult gate)
        {
            if (gate == null)
            {
                return "잠금 이유: 게이트 상태를 아직 계산하지 못했습니다.";
            }

            if (gate.Status == LiveCommandGateStatus.Allowed)
            {
                return "잠금 이유: 없음. 이번 연결의 실기 live session 승인과 evidence 기준을 모두 통과했습니다.";
            }

            if (gate.Status == LiveCommandGateStatus.RequiresConfirm)
            {
                return "잠금 이유: 이번 연결에서 아직 첫 실기 시작 승인이 끝나지 않았습니다.";
            }

            if (gate.BlockReasons.Count > 0)
            {
                return $"잠금 이유: {string.Join(" / ", gate.BlockReasons.ConvertAll(TranslateGateReason))}";
            }

            return $"잠금 이유: {FormatMotionGateDetail(gate)}";
        }

        private string BuildMotionGateUnlockWhen(LiveCommandSafetyGateResult gate)
        {
            if (gate == null)
            {
                return "언제 풀리는지: 게이트 상태 확인 후 갱신됩니다.";
            }

            if (gate.Status == LiveCommandGateStatus.Allowed)
            {
                return "언제 풀리는지: 지금 실기 live session이 열려 있어 관절, 포인트, 그리퍼 live 제어를 계속 실행할 수 있습니다.";
            }

            if (gate.Status == LiveCommandGateStatus.RequiresConfirm)
            {
                return "언제 풀리는지: 첫 실기 시작 승인만 끝나면 이번 연결 동안 재확인 없이 live 제어가 열립니다.";
            }

            var remaining = BuildRemainingGateChecks(gate);
            return remaining.Count > 0
                ? $"언제 풀리는지: {string.Join(", ", remaining)} 준비 후 실기 live session이 열립니다."
                : "언제 풀리는지: 현재 위치 읽기, 최신 기록, 첫 실기 시작 승인이 모두 준비되면 실기 live session이 열립니다.";
        }

        private string BuildMotionGateNextStep(LiveCommandSafetyGateResult gate)
        {
            if (!hasCurrentPositionReadComplete)
            {
                return "다음 행동: 현재 위치 읽기를 먼저 완료한다.";
            }

            if (gate == null)
            {
                return "다음 행동: 게이트 상태를 다시 계산한다.";
            }

            if (gate.Status == LiveCommandGateStatus.Allowed)
            {
                return "다음 행동: 실기 live 제어를 실행하고 post-sync evidence를 확인한다.";
            }

            if (gate.Status == LiveCommandGateStatus.RequiresConfirm)
            {
                return "다음 행동: 승인 팝업에서 이번 연결의 첫 실기 시작 승인만 마친다.";
            }

            foreach (var reason in gate.BlockReasons)
            {
                switch (reason)
                {
                    case "toolId missing":
                        return "다음 행동: 도구 설정 번호를 먼저 확인한다.";
                    case "userId missing":
                        return "다음 행동: 작업 기준 번호를 먼저 확인한다.";
                    case "coordSystem unresolved":
                        return "다음 행동: 좌표 기준을 로봇 기준, 툴 기준, 작업 기준 중 하나로 확정한다.";
                    case "latest-state freshness failed":
                    case "state readback failed":
                        return "다음 행동: 현재 위치 읽기를 다시 실행해 latest-state를 갱신한다.";
                    case "latest-drift freshness failed":
                        return "다음 행동: latest-drift를 다시 만들어 최신 비교 기록을 확보한다.";
                    case "drift threshold failed":
                        return "다음 행동: 실기 위치와 화면 위치 차이를 먼저 줄인 뒤 다시 확인한다.";
                    case "tiny MoveJ range exceeded":
                        return $"다음 행동: 각 관절 변화량을 {RobotControlMotionRuntime.TinyMoveJMaxJointDeltaDeg:0.#}도 이내로 줄여 다시 미리보기한다.";
                    case "prepared target mismatch":
                        return "다음 행동: 실기 제어 대상을 다시 준비해 preview와 실행 대상을 맞춘다.";
                    case "dry-run preview artifact missing":
                        return "다음 행동: 실행 전에 preview context를 다시 준비한다.";
                    case "production IK guard not cleared":
                        return "다음 행동: tiny MoveJ 자세 계산 안전 확인을 먼저 통과시킨다.";
                    case "boundary data missing or target outside workspace":
                        return "다음 행동: 작은 범위 목표가 작업 범위 안인지 먼저 확인한다.";
                    case "collision data missing or predicted path unsafe":
                        return "다음 행동: tiny MoveJ 경로 충돌 확인을 먼저 끝낸다.";
                    case "operator approval target mismatch":
                        return "다음 행동: 연결 세션이 바뀌었거나 승인 뒤 준비 대상이 달라졌다. 현재 세션 기준으로 다시 준비한다.";
                    case "servo disabled":
                        return "다음 행동: 서보 상태를 확인하고 이동 가능 상태로 맞춘다.";
                    case "not connected":
                        return "다음 행동: 실제 로봇 연결을 다시 확인한다.";
                    case "operator confirm token required":
                        return "다음 행동: 이번 연결의 첫 실기 시작 승인만 마친다.";
                }

                if (reason.StartsWith("fault active", StringComparison.OrdinalIgnoreCase))
                {
                    return "다음 행동: 오류를 초기화하고 현재 위치를 다시 확인한다.";
                }

                if (reason.StartsWith("motion queue not empty", StringComparison.OrdinalIgnoreCase))
                {
                    return "다음 행동: 이전 동작이 끝날 때까지 기다린다.";
                }
            }

            return "다음 행동: 잠금 이유를 확인하고 가장 먼저 막는 조건부터 해소한다.";
        }

        private string BuildMotionGateConfirmTarget(LiveCommandSafetyGateResult gate)
        {
            var label = "승인 대상: 이번 연결의 실기 live session";
            var now = DateTime.UtcNow;
            var pendingActive = pendingLiveApprovalUntilUtc > now && pendingLiveApprovalRequired && pendingLiveApprovalKind == LiveCommandKind.MoveJ;
            var approvedActive = HasActiveLiveSessionApprovalForProduct();

            if (approvedActive)
            {
                return $"{label} · 승인 유지 중";
            }

            if (pendingActive || gate?.Status == LiveCommandGateStatus.RequiresConfirm)
            {
                return $"{label} · 시작 승인 대기";
            }

            return label;
        }

        private string BuildMotionGateConfirmNote(LiveCommandSafetyGateResult gate)
        {
            var now = DateTime.UtcNow;
            var pendingActive = pendingLiveApprovalUntilUtc > now && pendingLiveApprovalRequired && pendingLiveApprovalKind == LiveCommandKind.MoveJ;
            var approvedActive = HasActiveLiveSessionApprovalForProduct();

            if (approvedActive)
            {
                return "첫 실기 시작 승인 후에는 연결이 유지되는 동안 관절, 포인트, 그리퍼 live 제어를 계속 허용합니다.";
            }

            if (pendingActive || gate?.Status == LiveCommandGateStatus.RequiresConfirm)
            {
                return "이 승인은 이번 연결의 첫 실기 시작에만 필요하며, 통과 후에는 재확인 없이 live 세션을 유지합니다.";
            }

            return "첫 실기 제어 전에만 session 승인 토큰을 발급합니다.";
        }

        private List<string> BuildRemainingGateChecks(LiveCommandSafetyGateResult gate)
        {
            var remaining = new List<string>();
            if (gate == null)
            {
                return remaining;
            }

            foreach (var reason in gate.BlockReasons)
            {
                var label = reason switch
                {
                    "toolId missing" => "도구 설정 번호",
                    "userId missing" => "작업 기준 번호",
                    "coordSystem unresolved" => "좌표 기준",
                    "latest-state freshness failed" => "latest-state 최신성",
                    "latest-drift freshness failed" => "latest-drift 최신성",
                    "drift threshold failed" => "drift 기준 통과",
                    "tiny MoveJ range exceeded" => "작은 범위 기준",
                    "prepared target mismatch" => "동일 대상 미리보기",
                    "dry-run preview artifact missing" => "tiny MoveJ 미리보기",
                    "production IK guard not cleared" => "자세 계산 확인",
                    "boundary data missing or target outside workspace" => "작업 범위 확인",
                    "collision data missing or predicted path unsafe" => "충돌 확인",
                    "operator approval target mismatch" => "대상 재승인",
                    "operator confirm token required" => "승인 토큰 확인",
                    "servo disabled" => "서보 상태",
                    "not connected" => "실기 연결",
                    _ => string.Empty,
                };

                if (string.IsNullOrWhiteSpace(label))
                {
                    if (reason.StartsWith("state readback failed", StringComparison.OrdinalIgnoreCase))
                    {
                        label = "현재 위치 읽기";
                    }
                    else if (reason.StartsWith("fault active", StringComparison.OrdinalIgnoreCase))
                    {
                        label = "오류 초기화";
                    }
                    else if (reason.StartsWith("motion queue not empty", StringComparison.OrdinalIgnoreCase))
                    {
                        label = "이전 동작 종료";
                    }
                }

                if (!string.IsNullOrWhiteSpace(label) && !remaining.Contains(label))
                {
                    remaining.Add(label);
                }
            }

            return remaining;
        }

        private string FormatMotionGateDetail(LiveCommandSafetyGateResult gate)
        {
            if (gate.Status == LiveCommandGateStatus.ReadbackOnly)
            {
                return BuildReadbackOnlyGateDetail();
            }

            if (gate.BlockReasons.Count > 0)
            {
                return string.Join(" / ", gate.BlockReasons.ConvertAll(TranslateGateReason));
            }

            if (gate.ClearedReasons.Count > 0)
            {
                return string.Join(" / ", gate.ClearedReasons.ConvertAll(TranslateGateReason));
            }

            return $"현재 기준: {snapshot.StatusTool}, {snapshot.StatusUser}, 좌표 기준 {FormatCoordSystemDisplay(snapshot.CoordSystem)}";
        }

        private string TranslateGateReason(string reason)
        {
            if (string.IsNullOrWhiteSpace(reason))
            {
                return string.Empty;
            }

            if (reason.StartsWith("speed ", StringComparison.OrdinalIgnoreCase))
            {
                return "현재 속도 설정이 안전 확인 기준보다 높습니다.";
            }

            if (reason.StartsWith("fault active", StringComparison.OrdinalIgnoreCase))
            {
                return "오류 코드가 남아 있어 먼저 초기화가 필요합니다.";
            }

            if (reason.StartsWith("motion queue not empty", StringComparison.OrdinalIgnoreCase))
            {
                return "이전 동작이 아직 끝나지 않았습니다.";
            }

            if (reason.StartsWith("state readback failed", StringComparison.OrdinalIgnoreCase))
            {
                return "현재 위치를 다시 읽지 못했습니다.";
            }

            return reason switch
            {
                "live client is readback-only" => "지금은 실제 로봇을 움직이지 않는 확인 단계입니다.",
                "actual motion/IO/gripper commands remain locked on macOS live readback" => "맥북 실기 연결은 현재 읽기 전용으로 잠겨 있습니다.",
                "toolId missing" => "도구 설정 번호를 먼저 확인해야 합니다.",
                "userId missing" => "작업 기준 번호를 먼저 확인해야 합니다.",
                "coordSystem unresolved" => "좌표 기준을 먼저 확정해야 합니다.",
                "latest-state freshness failed" => "최신 위치 증빙이 오래되어 현재 위치를 다시 읽어야 합니다.",
                "latest-drift freshness failed" => "최신 비교 증빙이 오래되어 다시 확인해야 합니다.",
                "drift threshold failed" => string.IsNullOrWhiteSpace(snapshot.LiveBlockedReason)
                    ? "실제 위치와 화면 위치 차이가 커서 이동이 잠겨 있습니다."
                    : snapshot.LiveBlockedReason,
                "tiny MoveJ range exceeded" => $"tiny MoveJ는 각 관절 변화량을 {RobotControlMotionRuntime.TinyMoveJMaxJointDeltaDeg:0.#}도 이내로 줄여야 합니다.",
                "prepared target mismatch" => "미리보기했던 tiny MoveJ 대상과 지금 실행 대상이 달라 다시 확인해야 합니다.",
                "operator approval target mismatch" => "승인 후 tiny MoveJ 대상이 바뀌어 새 승인 토큰이 필요합니다.",
                "operator confirm token required" => "실제 이동 전 마지막 확인이 필요합니다.",
                "operator confirm token accepted" => "실제 이동 전 마지막 확인이 끝났습니다.",
                "live preflight readback clear" => "현재 위치 읽기와 기본 점검이 끝났습니다.",
                "tiny MoveJ dedicated live path enabled" => "tiny MoveJ 전용 실기 통로가 열려 있습니다.",
                "tiny MoveJ range guard within 2.0deg" => $"tiny MoveJ 범위가 {RobotControlMotionRuntime.TinyMoveJMaxJointDeltaDeg:0.#}도 이내로 확인됐습니다.",
                "dry-run simulation" => "실제 로봇 대신 화면에서만 미리보기 중입니다.",
                "mock client" => "실기 연결이 아니라 화면 프리뷰 세션입니다.",
                "not connected" => "먼저 로봇 연결이 필요합니다.",
                "servo disabled" => "실제 이동 전에는 서보를 켜야 합니다.",
                "emergency stop active" => "비상 정지 상태입니다.",
                "safety stop active" => "안전 정지 상태입니다.",
                "controller collision flag active" => "로봇이 충돌 위험 상태로 보고됐습니다.",
                "dry-run preview artifact missing" => "미리보기 확인이 아직 없습니다.",
                "production IK guard not cleared" => "자세 계산 확인이 아직 끝나지 않았습니다.",
                "boundary data missing or target outside workspace" => "이동 가능 범위 확인이 아직 끝나지 않았습니다.",
                "collision data missing or predicted path unsafe" => "충돌 위험 확인이 아직 끝나지 않았습니다.",
                "gripper readback missing" => "그리퍼 상태 확인이 아직 없습니다.",
                _ => reason,
            };
        }
    }
}
