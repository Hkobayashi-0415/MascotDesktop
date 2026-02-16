using System;
using System.IO;
using UnityEngine;

namespace MascotDesktop.Runtime.Diagnostics
{
    [Serializable]
    public sealed class RuntimeLogEntry
    {
        public string timestamp;
        public string level;
        public string component;
        public string event_name;
        public string request_id;
        public string error_code;
        public string message;
        public string path;
        public string source_tier;
        public string exception_type;
        public string exception_message;
    }

    public static class RuntimeLog
    {
        private static readonly object FileLock = new object();
        private const string DefaultComponent = "runtime";
        private const string DefaultEventName = "runtime.event";
        private const string DefaultWarnErrorCode = "RUNTIME.WARN.UNSPECIFIED";
        private const string DefaultErrorErrorCode = "RUNTIME.ERROR.UNSPECIFIED";

        public static string NewRequestId()
        {
            return $"req-{Guid.NewGuid():N}";
        }

        public static void Info(
            string component,
            string eventName,
            string requestId,
            string message = null,
            string path = null,
            string sourceTier = null)
        {
            Write("INFO", component, eventName, requestId, null, message, path, sourceTier, null);
        }

        public static void Warn(
            string component,
            string eventName,
            string requestId,
            string errorCode = null,
            string message = null,
            string path = null,
            string sourceTier = null)
        {
            Write("WARN", component, eventName, requestId, errorCode, message, path, sourceTier, null);
        }

        public static void Error(
            string component,
            string eventName,
            string requestId,
            string errorCode = null,
            string message = null,
            string path = null,
            string sourceTier = null,
            Exception ex = null)
        {
            Write("ERROR", component, eventName, requestId, errorCode, message, path, sourceTier, ex);
        }

        private static void Write(
            string level,
            string component,
            string eventName,
            string requestId,
            string errorCode,
            string message,
            string path,
            string sourceTier,
            Exception ex)
        {
            var normalizedLevel = NormalizeLevel(level);
            var entry = new RuntimeLogEntry
            {
                timestamp = DateTimeOffset.UtcNow.ToString("o"),
                level = normalizedLevel,
                component = string.IsNullOrWhiteSpace(component) ? DefaultComponent : component,
                event_name = string.IsNullOrWhiteSpace(eventName) ? DefaultEventName : eventName,
                request_id = string.IsNullOrWhiteSpace(requestId) ? NewRequestId() : requestId,
                error_code = NormalizeErrorCode(normalizedLevel, errorCode),
                message = message ?? string.Empty,
                path = path ?? string.Empty,
                source_tier = sourceTier ?? string.Empty,
                exception_type = ex?.GetType().FullName ?? string.Empty,
                exception_message = ex?.Message ?? string.Empty
            };

            var json = JsonUtility.ToJson(entry);
            if (entry.level == "ERROR")
            {
                Debug.LogError(json);
            }
            else if (entry.level == "WARN")
            {
                Debug.LogWarning(json);
            }
            else
            {
                Debug.Log(json);
            }

            TryAppendJsonLine(json);
        }

        private static string NormalizeLevel(string level)
        {
            if (string.Equals(level, "ERROR", StringComparison.OrdinalIgnoreCase))
            {
                return "ERROR";
            }

            if (string.Equals(level, "WARN", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(level, "WARNING", StringComparison.OrdinalIgnoreCase))
            {
                return "WARN";
            }

            return "INFO";
        }

        private static string NormalizeErrorCode(string level, string errorCode)
        {
            if (!string.IsNullOrWhiteSpace(errorCode))
            {
                return errorCode;
            }

            if (level == "ERROR")
            {
                return DefaultErrorErrorCode;
            }

            if (level == "WARN")
            {
                return DefaultWarnErrorCode;
            }

            return string.Empty;
        }

        private static void TryAppendJsonLine(string json)
        {
            try
            {
                var logDir = Path.Combine(Application.persistentDataPath, "logs");
                Directory.CreateDirectory(logDir);
                var logFile = Path.Combine(logDir, "runtime.jsonl");

                lock (FileLock)
                {
                    File.AppendAllText(logFile, json + Environment.NewLine);
                }
            }
            catch
            {
                // Avoid recursive logging failure.
            }
        }
    }
}
