// Folder: App - Application controllers and services; single UnityEngine entry point.
using System;
using KineTutor3D.Types;
using KineTutor3D.UI.Data;
using UnityEngine;

namespace KineTutor3D.App
{
    /// <summary>
    /// Main 탭형 학습 콘텐츠 JSON을 로드하고 기본 문서로 폴백합니다.
    /// </summary>
    public static class MainLearningTabsLoader
    {
        private const string DefaultRobotId = "2DOF_RR";
        private const string ResourceRoot = "LearningTabs";

        public static MainLearningTabsDocument LoadForRobot(string robotId, RobotMetadataInfo metadata)
        {
            var requestedRobotId = string.IsNullOrWhiteSpace(robotId) ? DefaultRobotId : robotId;

            if (TryLoadResourceDocument(requestedRobotId, metadata, out var document))
            {
                return document;
            }

            if (!string.Equals(requestedRobotId, DefaultRobotId, StringComparison.Ordinal) &&
                TryLoadResourceDocument(DefaultRobotId, metadata, out document))
            {
                return document;
            }

            Debug.LogWarning($"[MainLearningTabsLoader] JSON resource not found for '{requestedRobotId}'. Using code fallback.");
            return BuildFallbackDocument(requestedRobotId, metadata);
        }

        public static MainLearningTabsDocument ParseOrFallback(string rawJson, string requestedRobotId, RobotMetadataInfo metadata)
        {
            if (string.IsNullOrWhiteSpace(rawJson))
            {
                Debug.LogWarning($"[MainLearningTabsLoader] Empty JSON for '{requestedRobotId}'. Using code fallback.");
                return BuildFallbackDocument(requestedRobotId, metadata);
            }

            try
            {
                var document = JsonUtility.FromJson<MainLearningTabsDocument>(rawJson);
                if (!IsUsable(document))
                {
                    Debug.LogWarning($"[MainLearningTabsLoader] Invalid JSON shape for '{requestedRobotId}'. Using code fallback.");
                    return BuildFallbackDocument(requestedRobotId, metadata);
                }

                document.robotId = string.IsNullOrWhiteSpace(document.robotId) ? requestedRobotId : document.robotId;
                if (string.IsNullOrWhiteSpace(document.displayTitle))
                {
                    document.displayTitle = metadata.DisplayName;
                }

                if (string.IsNullOrWhiteSpace(document.heroSummary))
                {
                    document.heroSummary = metadata.Description;
                }

                document.Sanitize();
                return document;
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"[MainLearningTabsLoader] Failed to parse JSON for '{requestedRobotId}': {exception.Message}");
                return BuildFallbackDocument(requestedRobotId, metadata);
            }
        }

        public static MainLearningTabsDocument BuildFallbackDocument(string requestedRobotId, RobotMetadataInfo metadata)
        {
            var displayTitle = string.IsNullOrWhiteSpace(metadata.DisplayName) ? requestedRobotId : metadata.DisplayName;
            var heroSummary = string.IsNullOrWhiteSpace(metadata.Description)
                ? "현재 로봇을 3D와 2D 다이어그램으로 함께 보며 조인트와 끝점 변화를 빠르게 익힙니다."
                : metadata.Description;

            var document = new MainLearningTabsDocument
            {
                robotId = requestedRobotId,
                displayTitle = displayTitle,
                heroSummary = heroSummary,
                legend = BuildLegend(metadata.Dof),
                tabs = new[]
                {
                    new MainTabContentDto
                    {
                        id = "overview",
                        labelKo = "개요",
                        headlineKo = $"{displayTitle} 빠르게 이해하기",
                        bodyKo = "왼쪽 3D 로봇과 오른쪽 2D 다이어그램을 함께 보며 관절과 끝점 관계를 먼저 익힙니다.",
                        cards = new[]
                        {
                            new InfoCardDto
                            {
                                tagKo = "ROBOT",
                                titleKo = $"{metadata.Dof} DOF · {metadata.RobotType}",
                                bodyKo = $"난이도 {metadata.Difficulty} · {metadata.Convention}"
                            },
                            new InfoCardDto
                            {
                                tagKo = "FLOW",
                                titleKo = "3초 시작 흐름",
                                bodyKo = "개요를 보고, 움직임 탭으로 넘어가 슬라이더를 조작한 뒤, FK 탭에서 결과를 읽습니다."
                            }
                        },
                        callouts = new[]
                        {
                            new CalloutDto
                            {
                                tone = "accent",
                                titleKo = "차별점",
                                bodyKo = "텍스트만 읽는 학습이 아니라 3D 로봇과 2D FK를 동시에 보며 변화가 즉시 반응합니다."
                            }
                        }
                    },
                    new MainTabContentDto
                    {
                        id = "motion",
                        labelKo = "움직임",
                        headlineKo = "직접 움직이며 감각 만들기",
                        bodyKo = "슬라이더를 조작하면 3D 로봇, 2D FK 다이어그램, 끝점 결과가 함께 갱신됩니다."
                    },
                    new MainTabContentDto
                    {
                        id = "fk",
                        labelKo = "FK",
                        headlineKo = "결과를 수식과 행렬로 읽기",
                        bodyKo = "현재 포즈의 누적 변환과 끝점 위치를 compact 카드로 확인합니다."
                    }
                },
                motion = new MotionPanelDto
                {
                    primaryCtaLabelKo = "움직여보기",
                    resultTitleKo = "실시간 결과",
                    overlaySummaryKo = metadata.Dof <= 2
                        ? "이 로봇은 각도 아크, 링크 길이, 그리드 오버레이를 함께 보여줍니다."
                        : "이 로봇은 기본 결과 모드로 3D와 2D 다이어그램을 함께 보여줍니다.",
                    metrics = new[]
                    {
                        new MetricLabelDto { id = "ee_x", labelKo = "EE X", unitKo = "m" },
                        new MetricLabelDto { id = "ee_y", labelKo = "EE Y", unitKo = "m" },
                        new MetricLabelDto { id = "ee_theta", labelKo = "EE 각도", unitKo = "deg" },
                        new MetricLabelDto { id = "link_lengths", labelKo = "링크 길이", unitKo = string.Empty }
                    }
                },
                forwardKinematics = new ForwardKinematicsPanelDto
                {
                    compactTitleKo = "현재 누적 변환",
                    advancedDrawerLabelKo = "자세히 보기",
                    formulaLinesKo = new[]
                    {
                        "0Tn = A1 × A2 × ... × An",
                        "마지막 열은 끝점 위치를 나타냅니다.",
                        "회전 블록은 현재 끝점 방향을 나타냅니다."
                    },
                    advancedCards = new[]
                    {
                        new InfoCardDto
                        {
                            tagKo = "READ",
                            titleKo = "먼저 볼 것",
                            bodyKo = "행렬 전체를 외우기보다 끝점 위치와 누적 각도 변화를 먼저 읽습니다."
                        },
                        new InfoCardDto
                        {
                            tagKo = "LINK",
                            titleKo = "링크 합성",
                            bodyKo = "각 링크는 이전 관절까지의 누적 각도를 따라 이어집니다."
                        }
                    }
                }
            };

            document.Sanitize();
            return document;
        }

        private static bool TryLoadResourceDocument(string robotId, RobotMetadataInfo metadata, out MainLearningTabsDocument document)
        {
            var asset = Resources.Load<TextAsset>($"{ResourceRoot}/{robotId}");
            if (asset == null)
            {
                document = null;
                return false;
            }

            document = ParseOrFallback(asset.text, robotId, metadata);
            return document != null;
        }

        private static bool IsUsable(MainLearningTabsDocument document)
        {
            return document != null &&
                document.tabs != null &&
                document.tabs.Length >= 3 &&
                document.motion != null &&
                document.forwardKinematics != null;
        }

        private static LegendItemDto[] BuildLegend(int dof)
        {
            var safeDof = Mathf.Max(2, dof);
            var legend = new LegendItemDto[safeDof + 1];
            for (var i = 0; i < safeDof; i++)
            {
                legend[i] = new LegendItemDto
                {
                    accentKey = $"joint{i + 1}",
                    labelKo = $"Joint {i + 1}"
                };
            }

            legend[safeDof] = new LegendItemDto
            {
                accentKey = "ee",
                labelKo = "End Effector"
            };

            return legend;
        }
    }
}
