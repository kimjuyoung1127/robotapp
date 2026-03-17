// Folder: Editor/CliTools - unity-cli 커스텀 도구: Resources 에셋 크기 분석
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json.Linq;
using UnityCliConnector;
using UnityEngine;

namespace KineTutor3D.Editor.CliTools
{
    /// <summary>
    /// Resources 폴더의 에셋 크기를 폴더별/파일별로 분석합니다.
    /// </summary>
    [UnityCliTool(Description = "Analyze asset sizes in Resources folder by subfolder and file")]
    public static class AssetSizeTool
    {
        public class Parameters
        {
            [ToolParameter("Number of largest files to show", Required = false)]
            public int Top { get; set; }
        }

        public static object HandleCommand(JObject @params)
        {
            var p = new ToolParams(@params);
            int top = p.GetInt("top", 10) ?? 10;

            string resourcesRoot = Path.Combine(Application.dataPath, "Runtime", "Resources");
            if (!Directory.Exists(resourcesRoot))
                return new ErrorResponse("Resources folder not found: Assets/Runtime/Resources/");

            var folderInfos = new List<object>();
            var allFiles = new List<FileEntry>();
            long totalSize = 0;

            foreach (string dir in Directory.GetDirectories(resourcesRoot))
            {
                string folderName = Path.GetFileName(dir);
                var dirInfo = new DirectoryInfo(dir);
                var files = dirInfo.GetFiles("*", SearchOption.AllDirectories)
                    .Where(f => !f.Name.EndsWith(".meta"))
                    .ToArray();

                long folderSize = 0;
                foreach (var file in files)
                {
                    folderSize += file.Length;
                    allFiles.Add(new FileEntry
                    {
                        relativePath = folderName + "/" + file.Name,
                        sizeBytes = file.Length
                    });
                }

                totalSize += folderSize;
                folderInfos.Add(new
                {
                    name = folderName,
                    file_count = files.Length,
                    size_kb = System.Math.Round(folderSize / 1024.0, 1)
                });
            }

            // 상위 N개 파일
            var largest = allFiles
                .OrderByDescending(f => f.sizeBytes)
                .Take(top)
                .Select(f => new
                {
                    path = f.relativePath,
                    size_kb = System.Math.Round(f.sizeBytes / 1024.0, 1)
                })
                .ToArray();

            return new SuccessResponse(
                $"Asset size analysis: {System.Math.Round(totalSize / 1024.0, 1)} KB total.",
                new
                {
                    total_size_kb = System.Math.Round(totalSize / 1024.0, 1),
                    total_files = allFiles.Count,
                    folders = folderInfos,
                    largest_files = largest
                });
        }

        private struct FileEntry
        {
            public string relativePath;
            public long sizeBytes;
        }
    }
}
