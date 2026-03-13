// Folder: App - Application controllers and services; single UnityEngine entry point.
using System;
using KineTutor3D.UI.Data;
using UnityEngine;

namespace KineTutor3D.App
{
    /// <summary>
    /// SO 로드 실패 시 사용할 런타임 기본 Step 설정을 생성합니다.
    /// </summary>
    public static class TutorStepRuntimeFactory
    {
        public static TutorStepConfig[] CreateDefaults()
        {
            return new[]
            {
                BuildStep(1, LeftPanelContent.DHTable, RightPanelContent.Hidden, true, false, false, FocusTarget.DHTable,
                    "DH 파라미터 소개", "DH 표의 기호 의미를 확인합니다.", "DH 헤더를 2개 이상 호버해 보세요.",
                    "DH 파라미터의 의미를 확인했습니다!",
                    Cond(InteractionType.Hover, "dh_theta_header", 1), Cond(InteractionType.Hover, "dh_alpha_header", 1)),

                BuildStep(2, LeftPanelContent.Hidden, RightPanelContent.FrameInfoOverlay, false, true, false, FocusTarget.Viewport3D,
                    "좌표 프레임", "프레임 배치 규칙을 확인합니다.", "프레임 2개를 클릭하세요.",
                    "모든 프레임의 배치 규칙을 확인했습니다!",
                    Cond(InteractionType.Click, "frame_0", 1), Cond(InteractionType.Click, "frame_1", 1)),

                BuildStep(3, LeftPanelContent.FourMatrices, RightPanelContent.Hidden, true, false, true, FocusTarget.MatrixPanel,
                    "기본 변환", "Rz, Tz, Tx, Rx를 확인합니다.", "4개 패널 클릭 후 슬라이더를 조작하세요.",
                    "4가지 기본 변환을 모두 확인했습니다!",
                    Cond(InteractionType.Click, "rz_panel", 1), Cond(InteractionType.Click, "tz_panel", 1),
                    Cond(InteractionType.Click, "tx_panel", 1), Cond(InteractionType.Click, "rx_panel", 1),
                    Cond(InteractionType.SliderChange, "joint_slider_1", 1)),

                BuildStep(4, LeftPanelContent.MultiplicationProgress, RightPanelContent.Hidden, true, false, false, FocusTarget.MatrixPanel,
                    "A_i 구성", "곱셈 단계를 순서대로 확인합니다.", "곱셈 진행 버튼을 4회 실행하세요.",
                    "Aᵢ = Rz·Tz·Tx·Rx 완성!",
                    Cond(InteractionType.StepAction, "mul_progress", 4)),

                BuildStep(5, LeftPanelContent.DHReference, RightPanelContent.AiColorCoding, true, true, true, FocusTarget.RightPanel,
                    "회전/위치 구분", "행렬의 R/p 영역을 구분합니다.", "슬라이더와 R/p 영역을 확인하세요.",
                    "회전(R)과 위치(p)를 구분할 수 있게 되었습니다!",
                    Cond(InteractionType.SliderChange, "joint_slider_1", 1), Cond(InteractionType.Hover, "matrix_r", 1), Cond(InteractionType.Hover, "matrix_p", 1)),

                BuildStep(6, LeftPanelContent.CumulativeProduct, RightPanelContent.A1A2Reference, true, true, true, FocusTarget.Viewport3D,
                    "누적곱 체인", "누적 변환 체인을 확인합니다.", "체인 완료 후 슬라이더를 조작하세요.",
                    "순기구학 체인이 완성되었습니다!",
                    Cond(InteractionType.Click, "chain_complete", 1), Cond(InteractionType.SliderChange, "joint_slider_2", 1)),

                BuildStep(7, LeftPanelContent.T0nAndExtract, RightPanelContent.PoseExtract, true, true, true, FocusTarget.EndEffectorFrame,
                    "EE 추출", "EE 위치와 방향 추출을 확인합니다.", "위치 열과 회전 열을 각각 클릭하세요.",
                    "EE 위치와 방향을 추출할 수 있게 되었습니다!",
                    Cond(InteractionType.Click, "pose_position_col", 1), Cond(InteractionType.Click, "pose_rotation_col", 1)),

                BuildStep(8, LeftPanelContent.FullDH, RightPanelContent.FullMatrices, true, true, true, FocusTarget.None,
                    "자유 탐색", "전체 요소를 자유롭게 조작합니다.", "원하는 파라미터를 조작해 보세요.",
                    string.Empty)
            };
        }

        private static TutorStepConfig BuildStep(
            int step,
            LeftPanelContent left,
            RightPanelContent right,
            bool showLeft,
            bool showRight,
            bool showBottom,
            FocusTarget focus,
            string title,
            string objective,
            string hint,
            string toast,
            params GateCondition[] conditions)
        {
            var config = ScriptableObject.CreateInstance<TutorStepConfig>();
            config.name = $"S{step:00}";
            config.showLeftPanel = showLeft;
            config.showRightPanel = showRight;
            config.showBottomBar = showBottom;
            config.leftContent = left;
            config.rightContent = right;
            config.focusTarget = focus;
            config.stepTitleKo = title;
            config.objectiveKo = objective;
            config.hintKo = hint;
            config.gateMeetToastKo = toast;
            config.conditions = conditions ?? Array.Empty<GateCondition>();
            config.showDHTable = showLeft;
            config.showMatrices = showRight || left == LeftPanelContent.FourMatrices || left == LeftPanelContent.CumulativeProduct || left == LeftPanelContent.T0nAndExtract || left == LeftPanelContent.FullDH;
            config.showSliders = showBottom;
            config.showFrameGizmos = true;
            config.showAnimation = true;
            config.showEEHighlight = focus == FocusTarget.EndEffectorFrame;
            return config;
        }

        private static GateCondition Cond(InteractionType type, string targetId, int count)
        {
            return new GateCondition
            {
                interactionType = type,
                targetId = targetId,
                requiredCount = count
            };
        }
    }
}

