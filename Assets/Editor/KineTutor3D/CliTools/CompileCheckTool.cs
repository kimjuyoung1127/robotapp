// Folder: Editor/CliTools - unity-cli 커스텀 도구: 컴파일 상태 확인
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEditor.Compilation;
using UnityCliConnector;

namespace KineTutor3D.Editor.CliTools
{
    /// <summary>
    /// Unity 프로젝트 컴파일 에러/경고 카운트를 반환합니다.
    /// </summary>
    [UnityCliTool(Description = "Check compilation status: errors and warnings count")]
    public static class CompileCheckTool
    {
        private static readonly List<CompilerMessage> collectedMessages = new List<CompilerMessage>();
        private static bool hookRegistered;

        private static void EnsureHook()
        {
            if (hookRegistered) return;
            CompilationPipeline.assemblyCompilationFinished += OnAssemblyCompiled;
            hookRegistered = true;
        }

        private static void OnAssemblyCompiled(string assemblyPath, CompilerMessage[] messages)
        {
            if (messages != null)
            {
                collectedMessages.AddRange(messages);
            }
        }

        public static object HandleCommand(JObject @params)
        {
            EnsureHook();

            bool isCompiling = EditorApplication.isCompiling;
            int errors = 0;
            int warnings = 0;
            var errorDetails = new List<string>();

            foreach (var msg in collectedMessages)
            {
                if (msg.type == CompilerMessageType.Error)
                {
                    errors++;
                    errorDetails.Add(msg.message);
                }
                else if (msg.type == CompilerMessageType.Warning)
                {
                    warnings++;
                }
            }

            var assemblies = CompilationPipeline.GetAssemblies();
            bool success = errors == 0 && !isCompiling;
            string status = isCompiling
                ? "Compilation in progress..."
                : success
                    ? $"Compilation clean. {assemblies.Length} assemblies."
                    : $"Compilation has {errors} error(s).";

            return new SuccessResponse(status, new
            {
                success,
                is_compiling = isCompiling,
                errors,
                warnings,
                assembly_count = assemblies.Length,
                error_details = errors > 0 ? errorDetails : null
            });
        }
    }
}
