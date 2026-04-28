// Folder: Tests - EditMode validator that guards RobotControlV3 runtime controllers against preview/demo hardcoding.
#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using System.IO;
using NUnit.Framework;

namespace KineTutor3D.Tests.EditMode
{
    /// <summary>
    /// RobotControlV3 runtime controller 영역에 preview/demo 샘플값이 다시 박히지 않도록 막습니다.
    /// `FAIRINO_FR5`, live endpoint IP/port 같은 실기 연동 계약 literal은 예외 허용합니다.
    /// preview state/UXML asset은 별도 SSOT가 있으므로 여기서는 runtime controller .cs만 검사합니다.
    /// </summary>
    [TestFixture]
    public class RobotControlV3HardcodingGuardTests
    {
        private const string RuntimeControllerRoot = "Assets/Scripts/UI/RobotControlV3";
        private const string PendantV3RootStyleSheet = "Assets/UI/PendantV3/pendant-v3.uss";
        private const string PendantV3StyleRoot = "Assets/UI/PendantV3";

        private static readonly string[] ExcludedFiles =
        {
            "PendantV3PreviewState.cs",
        };

        private static readonly string[] ForbiddenPreviewSampleLiterals =
        {
            "-497.0",
            "-130.0",
            "477.0",
            "180.0",
            "90.0",
        };

        private static readonly string[] UiCopyTargetFiles =
        {
            "Assets/Scripts/UI/RobotControlV3/PopupCoordinatorV3.cs",
            "Assets/Scripts/UI/RobotControlV3/ViewportToolbarController.cs",
        };

        private static readonly string[] ForbiddenUiCopyLiterals =
        {
            "서보 활성화 확인",
            "오류 초기화 확인",
            "실행 확인",
            "미저장 변경 확인",
            "서보를 켜면 실제 이동 가능 상태로 바뀜.",
            "Fault 원인을 확인했는지 먼저 점검하고 진행해라.",
            "실제 로봇이 움직일 수 있으니 주변 안전부터 확인해라.",
            "저장하지 않은 변경이 있을 수 있음. 이동 전에 확인해라.",
            "서보 켜기",
            "오류 초기화",
            "실행",
            "이동",
            "취소",
            "충돌 예측: 대기",
            "충돌 예측: 위험 구간 감지 (자동 강조)",
            "충돌 예측: 동기화 후 재확인 필요",
            "충돌 예측: 안전",
            "경계 ON",
            "경계 OFF",
            "충돌 ON",
            "충돌 OFF",
            "작업공간 경계: 표시",
            "작업공간 경계: 숨김",
            "카메라 리셋 요청을 받았다. 실제 카메라 프로필 연결은 2C-2 후속에서 붙인다.",
        };

        private static readonly string[] ForbiddenStyleLiteralPatterns =
        {
            "background-color: rgba(",
            "background-color: rgb(",
            "border-color: rgba(",
            "border-top-color: rgba(",
            "color: rgb(",
            "border-radius: 0",
            "border-radius: 1",
            "border-radius: 2",
            "border-radius: 3",
            "border-radius: 4",
            "border-radius: 5",
            "border-radius: 6",
            "border-radius: 7",
            "border-radius: 8",
            "border-radius: 9",
        };

        [Test]
        public void RobotControlV3_RuntimeControllers_DoNotContainForbiddenPreviewSampleLiterals()
        {
            var issues = new List<string>();
            Assert.That(Directory.Exists(RuntimeControllerRoot), Is.True, $"검사 대상 폴더가 없습니다: {RuntimeControllerRoot}");

            var targetFiles = Directory
                .GetFiles(RuntimeControllerRoot, "*.cs", SearchOption.TopDirectoryOnly)
                .Where(path => !ExcludedFiles.Any(excluded => path.EndsWith(excluded)))
                .ToArray();

            Assert.That(targetFiles.Length, Is.GreaterThan(0), "검사 대상 runtime controller 파일이 없습니다.");

            foreach (var path in targetFiles)
            {
                Assert.That(File.Exists(path), Is.True, $"검사 대상 파일이 없습니다: {path}");
                var text = File.ReadAllText(path);
                foreach (var literal in ForbiddenPreviewSampleLiterals)
                {
                    if (text.Contains(literal))
                    {
                        issues.Add($"{path} -> forbidden preview/demo literal: {literal}");
                    }
                }
            }

            if (issues.Count > 0)
            {
                Assert.Fail("RobotControlV3 하드코딩 가드 위반:\n" + string.Join("\n", issues));
            }
        }

        [Test]
        public void RobotControlV3_RuntimeControllers_DoNotContainForbiddenUiCopyLiterals()
        {
            var issues = new List<string>();

            foreach (var path in UiCopyTargetFiles)
            {
                Assert.That(File.Exists(path), Is.True, $"검사 대상 파일이 없습니다: {path}");
                var text = File.ReadAllText(path);
                foreach (var literal in ForbiddenUiCopyLiterals)
                {
                    if (text.Contains(literal))
                    {
                        issues.Add($"{path} -> forbidden UI copy literal: {literal}");
                    }
                }
            }

            if (issues.Count > 0)
            {
                Assert.Fail("RobotControlV3 UI copy 하드코딩 가드 위반:\n" + string.Join("\n", issues));
            }
        }

        [Test]
        public void RobotControlV3_PendantStyleSheet_DefinesTokenBackedGlobalButtonRadius()
        {
            Assert.That(File.Exists(PendantV3RootStyleSheet), Is.True, $"검사 대상 USS가 없습니다: {PendantV3RootStyleSheet}");
            var text = File.ReadAllText(PendantV3RootStyleSheet);

            Assert.That(text, Does.Contain("--rc-button-radius: var(--rc-radius);"), "버튼 반경은 왼쪽 nav 버튼과 같은 루트 radius 토큰을 따라야 합니다.");
            Assert.That(text, Does.Contain("--rc-button-bg: rgba(80, 140, 255, 0.10);"), "기본 버튼 배경은 회색 literal 대신 버튼 배경 토큰으로 관리해야 합니다.");
            Assert.That(text, Does.Contain("--rc-button-text: rgb(205, 216, 232);"), "기본 버튼 텍스트는 순백 대신 읽기 쉬운 버튼 텍스트 토큰을 사용해야 합니다.");
            Assert.That(text, Does.Contain("--rc-button-border-active:"), "선택/포커스 상태는 배경만이 아니라 active border 토큰으로 구분해야 합니다.");
            Assert.That(text, Does.Contain(".rc-root .unity-button:active"), "클릭 중인 버튼은 active pseudo-state로 즉시 색 피드백을 줘야 합니다.");
            Assert.That(text, Does.Contain(".rc-root .unity-button:focus"), "클릭 후 포커스 상태는 border 색으로 남아야 합니다.");
            Assert.That(text, Does.Contain(".rc-root .unity-button"), "모든 Pendant V3 버튼에 전역 버튼 스타일이 적용되어야 합니다.");
            Assert.That(text, Does.Contain("border-radius: var(--rc-button-radius);"), "전역 버튼 스타일은 하드코딩 반경 대신 버튼 radius 토큰을 사용해야 합니다.");
            Assert.That(text, Does.Contain("background-color: var(--rc-button-bg);"), "전역 버튼 스타일은 하드코딩 회색 배경 대신 버튼 배경 토큰을 사용해야 합니다.");
            Assert.That(text, Does.Contain("color: var(--rc-button-text);"), "전역 버튼 스타일은 순백 텍스트 대신 버튼 텍스트 토큰을 사용해야 합니다.");
        }

        [Test]
        public void RobotControlV3_PendantStyleSheets_UseTokensForColorAndRadiusConsumers()
        {
            var issues = new List<string>();
            Assert.That(Directory.Exists(PendantV3StyleRoot), Is.True, $"검사 대상 폴더가 없습니다: {PendantV3StyleRoot}");

            foreach (var path in Directory.GetFiles(PendantV3StyleRoot, "*.uss", SearchOption.TopDirectoryOnly))
            {
                var lines = File.ReadAllLines(path);
                for (var i = 0; i < lines.Length; i++)
                {
                    var trimmed = lines[i].TrimStart();
                    if (path.EndsWith("pendant-v3.uss") && trimmed.StartsWith("--rc-"))
                    {
                        continue;
                    }

                    foreach (var pattern in ForbiddenStyleLiteralPatterns)
                    {
                        if (trimmed.Contains(pattern))
                        {
                            issues.Add($"{path}:{i + 1} -> direct style literal: {pattern}");
                        }
                    }
                }
            }

            if (issues.Count > 0)
            {
                Assert.Fail("RobotControlV3 USS token guard 위반:\n" + string.Join("\n", issues));
            }
        }
    }
}
#endif
