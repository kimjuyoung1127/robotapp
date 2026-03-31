// Folder: Editor - Unity Editor authoring, QA, and packaging tools.
// UX 테스트용 샘플 데이터를 시드하는 에디터 도구입니다.
using System.Collections.Generic;
using KineTutor3D.UI.Data;
using UnityEditor;
using UnityEngine;

namespace KineTutor3D.Editor
{
    /// <summary>
    /// 학생 친화 UX용 ScriptableObject 데이터를 생성/갱신합니다.
    /// </summary>
    public static class UxDataSeeder
    {
        private const string TutorStepFolder = "Assets/Runtime/Resources/TutorSteps";
        private const string GlossaryFolder = "Assets/Runtime/Resources/Glossary";
        private const string OnboardingFolder = "Assets/Runtime/Resources/Onboarding";

        [MenuItem("KineTutor3D/Seed UX Data")]
        public static void Seed()
        {
            EnsureFolder("Assets/Runtime/Resources");
            EnsureFolder(TutorStepFolder);
            EnsureFolder(GlossaryFolder);
            EnsureFolder(OnboardingFolder);

            CreateTutorSteps();
            CreateGlossary();
            CreateOnboardingSequence();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[KineTutor3D] UX data seeding complete.");
        }

        private static void CreateTutorSteps()
        {
            CreateOrUpdateStep(
                1,
                true,
                false,
                false,
                LeftPanelContent.DHTable,
                RightPanelContent.Hidden,
                FocusTarget.DHTable,
                "DH 파라미터 소개",
                "DH 표의 주요 기호(θ, d, a, α)를 이해합니다.",
                "θ/α 헤더를 각각 확인해 보세요.",
                "DH 파라미터의 의미를 확인했습니다!",
                new[]
                {
                    Cond(InteractionType.Hover, "dh_theta_header", 1),
                    Cond(InteractionType.Hover, "dh_alpha_header", 1)
                });

            CreateOrUpdateStep(
                2,
                false,
                true,
                false,
                LeftPanelContent.Hidden,
                RightPanelContent.FrameInfoOverlay,
                FocusTarget.Viewport3D,
                "좌표 프레임 이해",
                "기저 프레임과 링크 프레임 배치를 확인합니다.",
                "프레임 2개를 클릭하세요.",
                "모든 프레임의 배치 규칙을 확인했습니다!",
                new[]
                {
                    Cond(InteractionType.Click, "frame_0", 1),
                    Cond(InteractionType.Click, "frame_1", 1)
                });

            CreateOrUpdateStep(
                3,
                true,
                false,
                true,
                LeftPanelContent.FourMatrices,
                RightPanelContent.Hidden,
                FocusTarget.MatrixPanel,
                "기본 변환 4종",
                "Rz/Tz/Tx/Rx 행렬 역할을 확인합니다.",
                "행렬 4개를 클릭하고 슬라이더를 조작하세요.",
                "4가지 기본 변환을 모두 확인했습니다!",
                new[]
                {
                    Cond(InteractionType.Click, "rz_panel", 1),
                    Cond(InteractionType.Click, "tz_panel", 1),
                    Cond(InteractionType.Click, "tx_panel", 1),
                    Cond(InteractionType.Click, "rx_panel", 1),
                    Cond(InteractionType.SliderChange, "joint_slider_1", 1)
                });

            CreateOrUpdateStep(
                4,
                true,
                false,
                false,
                LeftPanelContent.MultiplicationProgress,
                RightPanelContent.Hidden,
                FocusTarget.MatrixPanel,
                "Aᵢ 행렬 구성",
                "Rz·Tz·Tx·Rx 순서로 곱셈을 진행합니다.",
                "곱셈 진행 버튼을 4번 클릭하세요.",
                "Aᵢ = Rz·Tz·Tx·Rx 완성!",
                new[]
                {
                    Cond(InteractionType.StepAction, "mul_progress", 4)
                });

            CreateOrUpdateStep(
                5,
                true,
                true,
                true,
                LeftPanelContent.DHReference,
                RightPanelContent.AiColorCoding,
                FocusTarget.RightPanel,
                "회전/위치 분리",
                "행렬의 R(회전), p(위치) 영역을 구분합니다.",
                "슬라이더와 R/p 영역을 확인하세요.",
                "회전(R)과 위치(p)를 구분할 수 있게 되었습니다!",
                new[]
                {
                    Cond(InteractionType.SliderChange, "joint_slider_1", 1),
                    Cond(InteractionType.Hover, "matrix_r", 1),
                    Cond(InteractionType.Hover, "matrix_p", 1)
                });

            CreateOrUpdateStep(
                6,
                true,
                true,
                true,
                LeftPanelContent.CumulativeProduct,
                RightPanelContent.A1A2Reference,
                FocusTarget.Viewport3D,
                "누적곱 체인",
                "A1, A2...를 누적해 T0n으로 연결합니다.",
                "체인 확인 후 슬라이더를 조작하세요.",
                "순기구학 체인이 완성되었습니다!",
                new[]
                {
                    Cond(InteractionType.Click, "chain_complete", 1),
                    Cond(InteractionType.SliderChange, "joint_slider_2", 1)
                });

            CreateOrUpdateStep(
                7,
                true,
                true,
                true,
                LeftPanelContent.T0nAndExtract,
                RightPanelContent.PoseExtract,
                FocusTarget.EndEffectorFrame,
                "EE 추출",
                "T0n에서 위치 벡터 p와 회전 행렬 R을 추출합니다.",
                "위치 열/회전 열을 각각 클릭하세요.",
                "EE 위치와 방향을 추출할 수 있게 되었습니다!",
                new[]
                {
                    Cond(InteractionType.Click, "pose_position_col", 1),
                    Cond(InteractionType.Click, "pose_rotation_col", 1)
                });

            CreateOrUpdateStep(
                8,
                true,
                true,
                true,
                LeftPanelContent.FullDH,
                RightPanelContent.FullMatrices,
                FocusTarget.None,
                "자유 탐색",
                "전체 UI를 자유롭게 조작하며 복습합니다.",
                "관절 파라미터를 변경해 결과를 비교해 보세요.",
                string.Empty,
                new GateCondition[0]);
        }

        private static void CreateGlossary()
        {
            var entries = new List<GlossaryEntryConfig>
            {
                CreateOrUpdateGlossaryEntry("Theta", "θ", "세타, 관절 각도", "관절이 얼마나 돌아갔는지 나타내는 각도입니다.", "회전 관절에서 변수로 쓰이며 보통 Rz(θ)에 반영됩니다.", new[] {1, 3, 5, 8}),
                CreateOrUpdateGlossaryEntry("D", "d", "링크 오프셋", "Z축 방향 거리입니다.", "현재 프레임 원점에서 다음 기준까지의 z축 이동(Tz) 값입니다.", new[] {1, 3, 5, 8}),
                CreateOrUpdateGlossaryEntry("A", "a", "링크 길이", "관절 사이의 길이입니다.", "공통 법선 길이이며 x축 이동(Tx)으로 표현됩니다.", new[] {1, 3, 5, 8}),
                CreateOrUpdateGlossaryEntry("Alpha", "α", "알파, 링크 비틀림", "링크가 얼마나 비틀렸는지 나타냅니다.", "x축 기준 프레임 간 비틀림 각도로 Rx(α)에 반영됩니다.", new[] {1, 3, 5, 8}),
                CreateOrUpdateGlossaryEntry("Rz", "Rz", "Z축 회전", "Z축 기준 회전입니다.", "회전 행렬의 2x2 블록에 cos/sin이 배치됩니다.", new[] {3, 4}),
                CreateOrUpdateGlossaryEntry("Tz", "Tz", "Z축 이동", "Z축 방향 이동입니다.", "동차행렬 [2,3] 항목에 이동량 d가 반영됩니다.", new[] {3, 4}),
                CreateOrUpdateGlossaryEntry("Tx", "Tx", "X축 이동", "X축 방향 이동입니다.", "동차행렬 [0,3] 항목에 이동량 a가 반영됩니다.", new[] {3, 4}),
                CreateOrUpdateGlossaryEntry("Rx", "Rx", "X축 회전", "X축 기준 회전입니다.", "회전 블록의 y/z 성분에 cos/sin이 반영됩니다.", new[] {3, 4}),
                CreateOrUpdateGlossaryEntry("Ai", "Aᵢ", "변환 행렬", "한 링크의 변환을 나타냅니다.", "Aᵢ = Rz(θ)·Tz(d)·Tx(a)·Rx(α) 입니다.", new[] {4, 5, 6}),
                CreateOrUpdateGlossaryEntry("T0n", "T₀ₙ", "누적 변환", "처음부터 n번째까지 전체 변환입니다.", "T₀ₙ = A₁A₂...Aₙ 형태의 누적 곱입니다.", new[] {6, 7, 8}),
                CreateOrUpdateGlossaryEntry("R", "R", "회전 행렬", "방향을 나타내는 3x3 블록입니다.", "T의 좌상단 3x3 블록으로 좌표축 방향을 정의합니다.", new[] {5, 7}),
                CreateOrUpdateGlossaryEntry("P", "p", "위치 벡터", "위치를 나타내는 3x1 벡터입니다.", "T의 마지막 열 상단 3개 항목이 엔드이펙터 위치입니다.", new[] {5, 7}),
                CreateOrUpdateGlossaryEntry("EE", "EE", "엔드이펙터", "로봇 팔 끝 작업점입니다.", "순기구학 결과로 EE의 위치/방향을 계산합니다.", new[] {7, 8}),
                CreateOrUpdateGlossaryEntry("FK", "FK", "순기구학", "관절값에서 자세를 계산합니다.", "관절 변수 입력 -> 누적 행렬 -> EE pose 산출 절차입니다.", new[] {6, 7, 8})
            };

            var dbPath = $"{GlossaryFolder}/GlossaryDatabase.asset";
            var database = LoadOrCreateAsset<GlossaryDatabase>(dbPath);
            database.entries = entries.ToArray();
            EditorUtility.SetDirty(database);
        }

        private static void CreateOnboardingSequence()
        {
            var path = $"{OnboardingFolder}/OnboardingSequenceConfig.asset";
            var sequence = LoadOrCreateAsset<OnboardingSequenceConfig>(path);
            sequence.events = new[]
            {
                new OnboardingStepEvent
                {
                    delaySeconds = 0.5f,
                    target = OnboardingTarget.RobotViewport,
                    messageKo = "이것은 2자유도(2DOF) 로봇입니다. 두 개의 회전 관절이 있습니다."
                },
                new OnboardingStepEvent
                {
                    delaySeconds = 3.0f,
                    target = OnboardingTarget.DHTable,
                    messageKo = "DH 테이블에는 각 관절의 파라미터가 정리되어 있습니다."
                },
                new OnboardingStepEvent
                {
                    delaySeconds = 3.0f,
                    target = OnboardingTarget.DHCell,
                    messageKo = "셀 위에 마우스를 올려보세요. 3D 모델에서 해당 파라미터가 강조됩니다."
                },
                new OnboardingStepEvent
                {
                    delaySeconds = 3.0f,
                    target = OnboardingTarget.None,
                    messageKo = "이제 직접 조작을 시작해보세요."
                }
            };
            EditorUtility.SetDirty(sequence);
        }

        private static void CreateOrUpdateStep(
            int step,
            bool showLeft,
            bool showRight,
            bool showBottom,
            LeftPanelContent leftContent,
            RightPanelContent rightContent,
            FocusTarget focusTarget,
            string title,
            string objective,
            string hint,
            string toast,
            GateCondition[] conditions)
        {
            var path = $"{TutorStepFolder}/S{step:00}.asset";
            var stepAsset = LoadOrCreateAsset<TutorStepConfig>(path);
            stepAsset.showLeftPanel = showLeft;
            stepAsset.showRightPanel = showRight;
            stepAsset.showBottomBar = showBottom;
            stepAsset.leftContent = leftContent;
            stepAsset.rightContent = rightContent;
            stepAsset.focusTarget = focusTarget;
            stepAsset.stepTitleKo = title;
            stepAsset.objectiveKo = objective;
            stepAsset.hintKo = hint;
            stepAsset.gateMeetToastKo = toast;
            stepAsset.conditions = conditions;
            stepAsset.showDHTable = showLeft;
            stepAsset.showMatrices = showRight || leftContent == LeftPanelContent.FourMatrices || leftContent == LeftPanelContent.CumulativeProduct || leftContent == LeftPanelContent.T0nAndExtract || leftContent == LeftPanelContent.FullDH;
            stepAsset.showSliders = showBottom;
            stepAsset.showFrameGizmos = true;
            stepAsset.showAnimation = true;
            stepAsset.showEEHighlight = focusTarget == FocusTarget.EndEffectorFrame;
            stepAsset.tooltips = CreateStepTooltips(step);
            EditorUtility.SetDirty(stepAsset);
        }

        private static TooltipEntry[] CreateStepTooltips(int step)
        {
            switch (step)
            {
                case 1:
                    return new[]
                    {
                        Tip("dh_theta_header", "θ (세타) — 관절 각도", "Z축 기준 회전 각도입니다. 회전 관절의 변수입니다.", true, 1),
                        Tip("dh_alpha_header", "α (알파) — 링크 비틀림", "X축 기준 두 Z축 사이 회전 각도입니다.", false, 1)
                    };
                case 2:
                    return new[]
                    {
                        Tip("frame_0", "기저 프레임 {0}", "모든 변환의 시작 기준 좌표계입니다.", true, 2),
                        Tip("frame_1", "프레임 {1}", "관절 1에 부착된 좌표 프레임입니다.", false, 2)
                    };
                case 3:
                    return new[]
                    {
                        Tip("rz_panel", "Rz(θ)", "Z축 회전 행렬입니다.", true, 3),
                        Tip("tz_panel", "Tz(d)", "Z축 이동 행렬입니다.", false, 3),
                        Tip("tx_panel", "Tx(a)", "X축 이동 행렬입니다.", false, 3),
                        Tip("rx_panel", "Rx(α)", "X축 회전 행렬입니다.", false, 3)
                    };
                default:
                    return new TooltipEntry[0];
            }
        }

        private static TooltipEntry Tip(string anchor, string title, string body, bool autoShow, int step)
        {
            return new TooltipEntry
            {
                anchorId = anchor,
                titleKo = title,
                bodyKo = body,
                autoShow = autoShow,
                step = step
            };
        }

        private static GateCondition Cond(InteractionType interactionType, string targetId, int requiredCount)
        {
            return new GateCondition
            {
                interactionType = interactionType,
                targetId = targetId,
                requiredCount = requiredCount
            };
        }

        private static GlossaryEntryConfig CreateOrUpdateGlossaryEntry(
            string assetName,
            string symbol,
            string koreanName,
            string easyDescription,
            string mathDescription,
            int[] relatedSteps)
        {
            var path = $"{GlossaryFolder}/{assetName}.asset";
            var entry = LoadOrCreateAsset<GlossaryEntryConfig>(path);
            entry.symbol = symbol;
            entry.koreanName = koreanName;
            entry.easyDescription = easyDescription;
            entry.mathDescription = mathDescription;
            entry.relatedSteps = relatedSteps;
            EditorUtility.SetDirty(entry);
            return entry;
        }

        private static T LoadOrCreateAsset<T>(string path) where T : ScriptableObject
        {
            var asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset != null)
            {
                return asset;
            }

            asset = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(asset, path);
            return asset;
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path))
            {
                return;
            }

            var parent = path.Substring(0, path.LastIndexOf('/'));
            var folderName = path.Substring(path.LastIndexOf('/') + 1);
            if (!AssetDatabase.IsValidFolder(parent))
            {
                EnsureFolder(parent);
            }

            AssetDatabase.CreateFolder(parent, folderName);
        }
    }
}
