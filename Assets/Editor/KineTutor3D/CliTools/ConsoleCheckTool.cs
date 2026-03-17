// Folder: Editor/CliTools - unity-cli 커스텀 도구: 콘솔 로그 조회
using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityCliConnector;
using UnityEngine;

namespace KineTutor3D.Editor.CliTools
{
    /// <summary>
    /// Unity 콘솔 로그를 버퍼링하고 에러/경고를 반환합니다.
    /// </summary>
    [UnityCliTool(Description = "Read Unity console logs filtered by type (error, warn, all)")]
    public static class ConsoleCheckTool
    {
        public class Parameters
        {
            [ToolParameter("Filter type: error, warn, all", Required = false)]
            public string Type { get; set; }

            [ToolParameter("Maximum number of entries to return", Required = false)]
            public int Lines { get; set; }
        }

        private static readonly object bufferLock = new object();
        private static readonly List<LogEntry> logBuffer = new List<LogEntry>();
        private static bool registered;

        private struct LogEntry
        {
            public string message;
            public string stackTrace;
            public LogType type;
            public DateTime timestamp;
        }

        static ConsoleCheckTool()
        {
            EnsureRegistered();
        }

        private static void EnsureRegistered()
        {
            if (registered) return;
            Application.logMessageReceived += OnLogMessage;
            registered = true;
        }

        private static void OnLogMessage(string condition, string stackTrace, LogType type)
        {
            lock (bufferLock)
            {
                logBuffer.Add(new LogEntry
                {
                    message = condition,
                    stackTrace = stackTrace,
                    type = type,
                    timestamp = DateTime.Now
                });

                if (logBuffer.Count > 1000)
                    logBuffer.RemoveAt(0);
            }
        }

        public static object HandleCommand(JObject @params)
        {
            EnsureRegistered();
            var p = new ToolParams(@params);
            string filter = p.Get("type", "error");
            int maxLines = p.GetInt("lines", 50) ?? 50;
            bool verbose = p.GetBool("verbose", false);

            var errors = new List<object>();
            var warnings = new List<object>();
            var logs = new List<object>();

            List<LogEntry> snapshot;
            lock (bufferLock)
            {
                snapshot = new List<LogEntry>(logBuffer);
            }

            foreach (var entry in snapshot)
            {
                object item = verbose
                    ? (object)new
                    {
                        message = entry.message,
                        stack_trace = entry.stackTrace,
                        type = entry.type.ToString(),
                        timestamp = entry.timestamp.ToString("HH:mm:ss")
                    }
                    : new
                    {
                        message = entry.message,
                        type = entry.type.ToString(),
                        timestamp = entry.timestamp.ToString("HH:mm:ss")
                    };

                switch (entry.type)
                {
                    case LogType.Error:
                    case LogType.Exception:
                    case LogType.Assert:
                        errors.Add(item);
                        break;
                    case LogType.Warning:
                        warnings.Add(item);
                        break;
                    default:
                        logs.Add(item);
                        break;
                }
            }

            object result;
            switch (filter.ToLowerInvariant())
            {
                case "error":
                    var errorSlice = errors.Count > maxLines ? errors.GetRange(errors.Count - maxLines, maxLines) : errors;
                    result = new { error_count = errors.Count, errors = errorSlice };
                    break;
                case "warn":
                    var warnSlice = warnings.Count > maxLines ? warnings.GetRange(warnings.Count - maxLines, maxLines) : warnings;
                    result = new { warning_count = warnings.Count, warnings = warnSlice };
                    break;
                default:
                    result = new
                    {
                        error_count = errors.Count,
                        warning_count = warnings.Count,
                        log_count = logs.Count,
                        errors,
                        warnings,
                        logs
                    };
                    break;
            }

            return new SuccessResponse($"Console: {errors.Count} errors, {warnings.Count} warnings.", result);
        }
    }
}
