// Folder: App - Application controllers and services; single UnityEngine entry point.
using System;
using KineTutor3D.UI.Data;
using UnityEngine;

namespace KineTutor3D.App
{
    /// <summary>
    /// Beginner Lesson L0~L3 런타임 TutorStepConfig를 생성합니다.
    /// </summary>
    public static class BeginnerLessonFactory
    {
        public const int LessonCount = 4;

        public static TutorStepConfig[] CreateLessons()
        {
            return new[]
            {
                BuildLesson(0, "로봇 팔은 무엇을 움직이는가",
                    "관절 슬라이더를 움직여 로봇 팔이 어떻게 반응하는지 관찰합니다.",
                    "관절1 슬라이더를 2회, 관절2 슬라이더를 1회 이상 움직여 보세요.",
                    "로봇 팔의 움직임을 관찰했습니다!",
                    BeginnerLeftContent.ObserveGuide,
                    true, true, true, false,
                    new[] { Cond(InteractionType.SliderChange, "joint_slider_1", 2), Cond(InteractionType.SliderChange, "joint_slider_2", 1) }),

                BuildLesson(1, "회전하면 끝점이 어떻게 움직이는가",
                    "관절1만 회전시켜 끝점의 호(arc) 궤적을 관찰합니다.",
                    "관절1 슬라이더를 3회 이상 움직여 호 궤적을 확인하세요.",
                    "관절 회전과 끝점 호 궤적의 관계를 이해했습니다!",
                    BeginnerLeftContent.ArcCompareGuide,
                    true, true, true, false,
                    new[] { Cond(InteractionType.SliderChange, "joint_slider_1", 3) }),

                BuildLesson(2, "두 관절이 같이 움직이면 왜 경로가 바뀌는가",
                    "J1만, J2만, 둘 다 움직여서 끝점 궤적이 어떻게 달라지는지 비교합니다.",
                    "비교 모드 3가지(J1만, J2만, 둘다)를 각각 한 번씩 실행하세요.",
                    "관절 조합에 따른 궤적 차이를 이해했습니다!",
                    BeginnerLeftContent.CombinationGuide,
                    true, true, true, false,
                    new[] { Cond(InteractionType.StepAction, "compare_j1_only", 1), Cond(InteractionType.StepAction, "compare_j2_only", 1), Cond(InteractionType.StepAction, "compare_both", 1) }),

                BuildLesson(3, "목표점을 맞추려면 왜 거꾸로 생각해야 하는가",
                    "타깃 마커를 보고 관절 각도를 조정해 끝점을 맞춰 봅니다.",
                    "타깃 2개를 맞추세요.",
                    "타깃을 맞추며 역기구학적 사고를 경험했습니다!",
                    BeginnerLeftContent.TargetHintGuide,
                    true, true, true, true,
                    new[] { Cond(InteractionType.StepAction, "target_reached", 2) })
            };
        }

        private static TutorStepConfig BuildLesson(
            int lessonIndex,
            string title,
            string objective,
            string hint,
            string toast,
            BeginnerLeftContent leftContent,
            bool showWhyItMoved,
            bool showJointHighlight,
            bool showEndEffectorTrail,
            bool showTargetMarkers,
            GateCondition[] conditions)
        {
            var config = ScriptableObject.CreateInstance<TutorStepConfig>();
            config.name = $"L{lessonIndex:00}";
            config.beginnerMode = true;
            config.showLeftPanel = true;
            config.showRightPanel = true;
            config.showBottomBar = true;
            config.leftContent = LeftPanelContent.Hidden;
            config.rightContent = RightPanelContent.Hidden;
            config.beginnerLeftContent = leftContent;
            config.focusTarget = FocusTarget.Viewport3D;
            config.stepTitleKo = title;
            config.objectiveKo = objective;
            config.hintKo = hint;
            config.gateMeetToastKo = toast;
            config.conditions = conditions ?? Array.Empty<GateCondition>();
            config.showWhyItMoved = showWhyItMoved;
            config.showJointHighlight = showJointHighlight;
            config.showEndEffectorTrail = showEndEffectorTrail;
            config.showTargetMarkers = showTargetMarkers;
            config.showJointInputRail = true;
            config.showPlainLanguage = true;
            config.showFormula = false;
            config.showSliders = true;
            config.showFrameGizmos = true;
            config.showMatrices = false;
            config.showDHTable = false;
            config.showAnimation = true;
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
