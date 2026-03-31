// Folder: App - Application controllers and services; single UnityEngine entry point.
using System;
using KineTutor3D.UI.Data;
using UnityEngine;

namespace KineTutor3D.App
{
    public static class MathReadinessLessonFactory
    {
        public const int LessonCount = 4;

        public static TutorStepConfig[] CreateLessons()
        {
            return new[]
            {
                BuildLesson(
                    0,
                    "각도는 방향이다",
                    "먼저 슬라이더를 움직여 팔이 향하는 방향을 직접 봅니다.",
                    "0°/90°/180° 기준선을 보고 목표 각도로 먼저 움직여 보세요.",
                    "방향 감각을 먼저 만들면 이후 위치와 좌표도 훨씬 쉬워져요.",
                    "0°는 오른쪽, 90°는 위쪽, 180°는 왼쪽이라는 기준을 같이 보세요.",
                    "학생이 먼저 움직여 보고 나서 방향 이름을 붙이게 해주세요.",
                    string.Empty,
                    Array.Empty<string>(),
                    string.Empty,
                    "좋아요! 이제 90° 방향을 구분할 수 있어요.",
                    MathReadinessContent.AngleDirection,
                    1,
                    new[]
                    {
                        Question(
                            "방금 팔이 향한 방향은 어디에 가장 가까웠나요?",
                            new[] { "오른쪽(0°)", "위쪽(90°)", "왼쪽(180°)" },
                            1,
                            "math_m0_correct",
                            new[]
                            {
                                "조금만 다시 볼게요. 0°는 오른쪽 방향이에요.",
                                string.Empty,
                                "조금만 다시 볼게요. 180°는 왼쪽 방향이에요."
                            },
                            true,
                            90f,
                            0,
                            "math_m0_pose_ready",
                            "J1 슬라이더를 90°로 옮겨보세요.")
                    },
                    new[]
                    {
                        Cond(InteractionType.SliderReachTarget, "math_m0_pose_ready", 1),
                        Cond(InteractionType.StepAction, "math_m0_correct", 1),
                    }),
                BuildLesson(
                    1,
                    "길이와 각도로 위치를 짐작해요",
                    "실제 링크 길이 1.0을 기준으로 끝점이 어디쯤 가는지 봅니다.",
                    "슬라이더를 목표 각도로 옮기고 끝점 위치를 먼저 본 뒤 질문을 확인하세요.",
                    "실제 로봇 길이를 기준으로 보면 좌표 감각이 훨씬 자연스럽게 잡혀요.",
                    "0°면 오른쪽, 90°면 위쪽으로 길이 1만큼 간다고 느끼게 해주세요.",
                    "정답보다 먼저, 끝점이 어느 쪽으로 이동했는지 말로 설명하게 해주세요.",
                    string.Empty,
                    Array.Empty<string>(),
                    string.Empty,
                    "좋아요! 이제 방향과 길이로 위치를 짐작할 수 있어요.",
                    MathReadinessContent.LengthAngleToPoint,
                    1,
                    new[]
                    {
                        Question(
                            "끝점이 어디에 가장 가까웠나요?",
                            new[] { "(1, 0) 근처", "(0, 1) 근처", "(0.7, 0.7) 근처" },
                            0,
                            "math_m1_zero_correct",
                            new[]
                            {
                                string.Empty,
                                "조금만 다시 볼게요. 90°로 올리면 (0, 1) 쪽에 가까워져요.",
                                "거의 맞았어요. 대각선은 45°일 때 더 가까워요."
                            },
                            true,
                            0f,
                            0,
                            "math_m1_zero_pose_ready",
                            "J1을 0°로 놓아보세요."),
                        Question(
                            "이번에는 끝점이 어디로 갔나요?",
                            new[] { "(1, 0) 근처", "(0, 1) 근처", "(-1, 0) 근처" },
                            1,
                            "math_m1_ninety_correct",
                            new[]
                            {
                                "조금만 다시 볼게요. 0°는 오른쪽으로 더 가까워요.",
                                string.Empty,
                                "조금만 다시 볼게요. 왼쪽 방향은 180°일 때 더 가까워요."
                            },
                            true,
                            90f,
                            0,
                            "math_m1_ninety_pose_ready",
                            "이번엔 J1을 90°로 옮겨보세요.")
                    },
                    new[]
                    {
                        Cond(InteractionType.SliderReachTarget, "math_m1_zero_pose_ready", 1),
                        Cond(InteractionType.StepAction, "math_m1_zero_correct", 1),
                        Cond(InteractionType.SliderReachTarget, "math_m1_ninety_pose_ready", 1),
                        Cond(InteractionType.StepAction, "math_m1_ninety_correct", 1),
                    }),
                BuildLesson(
                    2,
                    "45°는 대각선이다",
                    "슬라이더를 45°로 옮기고 끝점이 대각선 쪽으로 가는 걸 봅니다.",
                    "0°와 90° 사이의 중간 방향이 어디인지 기준선과 함께 확인해 보세요.",
                    "정확한 삼각함수보다 먼저, 대각선이라는 감각을 만드는 단계예요.",
                    "45°는 오른쪽과 위쪽이 함께 섞인 방향이라는 점을 연결해 주세요.",
                    "숫자를 외우게 하기보다 대각선 느낌을 먼저 말하게 해주세요.",
                    string.Empty,
                    Array.Empty<string>(),
                    string.Empty,
                    "좋아요! 이제 45°를 대각선 감각으로 볼 수 있어요.",
                    MathReadinessContent.DiagonalIntuition,
                    1,
                    new[]
                    {
                        Question(
                            "끝점이 어느 점에 가장 가까웠나요?",
                            new[] { "(1, 0) 근처", "(0.7, 0.7) 근처", "(0, 1) 근처" },
                            1,
                            "math_m2_correct",
                            new[]
                            {
                                "조금만 다시 볼게요. 그건 0°에 더 가까운 위치예요.",
                                string.Empty,
                                "조금만 다시 볼게요. 그건 90°에 더 가까운 위치예요."
                            },
                            true,
                            45f,
                            0,
                            "math_m2_pose_ready",
                            "J1을 45°로 옮겨보세요.")
                    },
                    new[]
                    {
                        Cond(InteractionType.SliderReachTarget, "math_m2_pose_ready", 1),
                        Cond(InteractionType.StepAction, "math_m2_correct", 1),
                    }),
                BuildLesson(
                    3,
                    "두 링크를 합치면 더 멀리 간다",
                    "두 관절을 함께 움직여 끝점이 더 다양한 곳으로 가는 걸 직접 봅니다.",
                    "먼저 J1은 유지하고 J2를 -45° 근처로 옮겨 보며 두 링크가 만든 변화를 확인하세요.",
                    "관절이 하나 더 생기면 끝점이 만들 수 있는 위치가 훨씬 다양해져요.",
                    "두 번째 관절은 첫 번째를 덮어쓰는 게 아니라 새로운 조합을 만든다는 점을 강조해 주세요.",
                    "정답보다 먼저, 끝점이 전보다 다른 곳까지 간다는 느낌을 말하게 해주세요.",
                    string.Empty,
                    Array.Empty<string>(),
                    string.Empty,
                    "좋아요! 방금 한 게 기구학의 시작이에요.",
                    MathReadinessContent.TwoLinkComposition,
                    2,
                    new[]
                    {
                        Question(
                            "관절 하나만 있을 때와 비교하면 무엇이 달라졌나요?",
                            new[] { "거의 같은 곳만 갈 수 있어요", "끝점이 더 다양한 곳으로 가요", "오히려 움직임이 줄었어요" },
                            1,
                            "math_m3_correct",
                            new[]
                            {
                                "조금만 다시 볼게요. 관절이 두 개면 가능한 조합이 더 많아져요.",
                                string.Empty,
                                "조금만 다시 볼게요. 관절을 하나 더 쓰면 끝점은 더 다양한 위치에 갈 수 있어요."
                            },
                            true,
                            -45f,
                            1,
                            "math_m3_pose_ready",
                            "J1은 유지하고 J2를 -45°로 옮겨보세요.",
                            45f,
                            0)
                    },
                    new[]
                    {
                        Cond(InteractionType.SliderReachTarget, "math_m3_pose_ready", 1),
                        Cond(InteractionType.StepAction, "math_m3_correct", 1),
                    })
            };
        }

        private static TutorStepConfig BuildLesson(
            int lessonIndex,
            string title,
            string objective,
            string hint,
            string rationale,
            string commonMistake,
            string coachHint,
            string warmupPrompt,
            string[] warmupChoices,
            string warmupFollowup,
            string successToast,
            MathReadinessContent content,
            int interactiveJointCount,
            MathReadinessQuestion[] questions,
            GateCondition[] conditions)
        {
            var config = ScriptableObject.CreateInstance<TutorStepConfig>();
            config.name = $"M{lessonIndex:00}";
            config.mathReadinessMode = true;
            config.beginnerMode = false;
            config.showMathReadinessPanel = true;
            config.showLeftPanel = false;
            config.showRightPanel = false;
            config.showBottomBar = false;
            config.leftContent = LeftPanelContent.Hidden;
            config.rightContent = RightPanelContent.Hidden;
            config.focusTarget = FocusTarget.Viewport3D;
            config.stepTitleKo = title;
            config.objectiveKo = objective;
            config.hintKo = hint;
            config.rationaleKo = rationale;
            config.commonMistakeKo = commonMistake;
            config.coachHintKo = coachHint;
            config.warmupPromptKo = warmupPrompt;
            config.warmupChoicesKo = warmupChoices ?? Array.Empty<string>();
            config.warmupFollowupKo = warmupFollowup;
            config.successToastKo = successToast;
            config.gateMeetToastKo = successToast;
            config.conditions = conditions ?? Array.Empty<GateCondition>();
            config.readinessQuestions = questions ?? Array.Empty<MathReadinessQuestion>();
            config.mathReadinessContent = content;
            config.interactiveJointCount = interactiveJointCount;
            config.showMathVisualHints = true;
            config.showWhyItMoved = true;
            config.showJointHighlight = true;
            config.showEndEffectorTrail = true;
            config.showTargetMarkers = false;
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

        private static MathReadinessQuestion Question(
            string prompt,
            string[] choices,
            int correctIndex,
            string targetId,
            string[] corrections,
            bool requiresManipulationFirst = false,
            float targetAngleDeg = float.NaN,
            int targetJointIndex = 0,
            string targetReachGateId = "",
            string manipulationInstructionKo = "",
            float secondaryTargetAngleDeg = float.NaN,
            int secondaryTargetJointIndex = -1)
        {
            return new MathReadinessQuestion
            {
                promptKo = prompt,
                choicesKo = choices ?? Array.Empty<string>(),
                correctChoiceIndex = correctIndex,
                correctTargetId = targetId,
                correctionMessagesKo = corrections ?? Array.Empty<string>(),
                requiresManipulationFirst = requiresManipulationFirst,
                targetAngleDeg = targetAngleDeg,
                targetJointIndex = targetJointIndex,
                targetReachGateId = targetReachGateId,
                manipulationInstructionKo = manipulationInstructionKo,
                secondaryTargetAngleDeg = secondaryTargetAngleDeg,
                secondaryTargetJointIndex = secondaryTargetJointIndex
            };
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
