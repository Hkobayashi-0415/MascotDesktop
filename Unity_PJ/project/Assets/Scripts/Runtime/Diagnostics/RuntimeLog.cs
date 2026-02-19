using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
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
        private static readonly object StateLock = new object();
        private static readonly Queue<string> PendingLines = new Queue<string>();
        private static readonly List<RuntimeLogEntry> RecentEntries = new List<RuntimeLogEntry>();
        private static readonly AutoResetEvent FlushSignal = new AutoResetEvent(false);

        private const string DefaultComponent = "runtime";
        private const string DefaultEventName = "runtime.event";
        private const string DefaultWarnErrorCode = "RUNTIME.WARN.UNSPECIFIED";
        private const string DefaultErrorErrorCode = "RUNTIME.ERROR.UNSPECIFIED";
        private const int RecentEntryLimit = 512;
        private const int DefaultFlushIntervalMs = 250;
        private const int DefaultMaxBatchSize = 128;
        private const int DefaultMaxQueueSize = 4096;
        private const long DefaultMaxFileBytes = 1024 * 1024;
        private const int DefaultMaxRetainedFiles = 10;

        private const string EnvMinLevel = "MASCOTDESKTOP_RUNTIMELOG_MIN_LEVEL";
        private const string EnvIncludeComponents = "MASCOTDESKTOP_RUNTIMELOG_INCLUDE_COMPONENTS";
        private const string EnvExcludeComponents = "MASCOTDESKTOP_RUNTIMELOG_EXCLUDE_COMPONENTS";
        private const string EnvIncludeEvents = "MASCOTDESKTOP_RUNTIMELOG_INCLUDE_EVENTS";
        private const string EnvExcludeEvents = "MASCOTDESKTOP_RUNTIMELOG_EXCLUDE_EVENTS";
        private const string EnvMaxFileBytes = "MASCOTDESKTOP_RUNTIMELOG_MAX_FILE_BYTES";
        private const string EnvMaxFiles = "MASCOTDESKTOP_RUNTIMELOG_MAX_FILES";
        private const string EnvFlushIntervalMs = "MASCOTDESKTOP_RUNTIMELOG_FLUSH_INTERVAL_MS";
        private const string EnvMaxBatchSize = "MASCOTDESKTOP_RUNTIMELOG_MAX_BATCH_SIZE";
        private const string EnvMaxQueueSize = "MASCOTDESKTOP_RUNTIMELOG_MAX_QUEUE_SIZE";
        private const string EnvFilePrefix = "MASCOTDESKTOP_RUNTIMELOG_FILE_PREFIX";

        private static Thread writerThread;
        private static bool writerRunning;
        private static bool writerIsFlushing;
        private static bool writerShutdownRequested;
        private static string activeDateToken = string.Empty;
        private static string activeFilePrefix = string.Empty;
        private static int activeFileIndex;
        private static string activeLogFilePath = string.Empty;
        private static string cachedLogDir;

        static RuntimeLog()
        {
            AppDomain.CurrentDomain.ProcessExit += (_, __) => ShutdownWriter();
            AppDomain.CurrentDomain.DomainUnload += (_, __) => ShutdownWriter();
            try 
            {
                // Optimistically try to cache on main thread initialization
                cachedLogDir = Path.Combine(Application.persistentDataPath, "logs");
            }
            catch
            {
                // Ignored if called from background thread
            }
        }

        public static string NewRequestId()
        {
            return $"req-{Guid.NewGuid():N}";
        }

        public static RuntimeLogEntry[] SnapshotRecentEntries(int maxCount = 128)
        {
            if (maxCount <= 0)
            {
                return Array.Empty<RuntimeLogEntry>();
            }

            lock (StateLock)
            {
                var count = Math.Min(maxCount, RecentEntries.Count);
                if (count <= 0)
                {
                    return Array.Empty<RuntimeLogEntry>();
                }

                var startIndex = RecentEntries.Count - count;
                return RecentEntries
                    .Skip(startIndex)
                    .Select(CloneEntry)
                    .ToArray();
            }
        }

        public static void ClearRecentEntries()
        {
            lock (StateLock)
            {
                RecentEntries.Clear();
            }
        }

        public static void Flush(int timeoutMs = 2000)
        {
            if (timeoutMs < 0)
            {
                timeoutMs = 0;
            }

            FlushSignal.Set();
            var deadline = DateTime.UtcNow.AddMilliseconds(timeoutMs);
            while (DateTime.UtcNow <= deadline)
            {
                lock (StateLock)
                {
                    if (PendingLines.Count == 0 && !writerIsFlushing)
                    {
                        return;
                    }
                }

                Thread.Sleep(20);
            }
        }

        public static void Info(
            string component,
            string eventName,
            string requestId,
            string message = null,
            string path = null,
            string sourceTier = null)
        {
            Write("INFO", component, eventName, requestId, null, message, path, sourceTier, null, null, null);
        }

        public static void Warn(
            string component,
            string eventName,
            string requestId,
            string errorCode = null,
            string message = null,
            string path = null,
            string sourceTier = null,
            Exception ex = null,
            string exceptionType = null,
            string exceptionMessage = null)
        {
            Write("WARN", component, eventName, requestId, errorCode, message, path, sourceTier, ex, exceptionType, exceptionMessage);
        }

        public static void Error(
            string component,
            string eventName,
            string requestId,
            string errorCode = null,
            string message = null,
            string path = null,
            string sourceTier = null,
            Exception ex = null,
            string exceptionType = null,
            string exceptionMessage = null)
        {
            Write("ERROR", component, eventName, requestId, errorCode, message, path, sourceTier, ex, exceptionType, exceptionMessage);
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
            Exception ex,
            string exceptionType,
            string exceptionMessage)
        {
            var normalizedLevel = NormalizeLevel(level);
            var safeComponent = string.IsNullOrWhiteSpace(component) ? DefaultComponent : component;
            var safeEventName = string.IsNullOrWhiteSpace(eventName) ? DefaultEventName : eventName;
            if (!ShouldEmit(normalizedLevel, safeComponent, safeEventName))
            {
                return;
            }

            var exceptionForEntry = UnwrapException(ex);
            var entry = new RuntimeLogEntry
            {
                timestamp = DateTimeOffset.UtcNow.ToString("o"),
                level = normalizedLevel,
                component = safeComponent,
                event_name = safeEventName,
                request_id = string.IsNullOrWhiteSpace(requestId) ? NewRequestId() : requestId,
                error_code = NormalizeErrorCode(normalizedLevel, errorCode),
                message = message ?? string.Empty,
                path = path ?? string.Empty,
                source_tier = sourceTier ?? string.Empty,
                exception_type = !string.IsNullOrWhiteSpace(exceptionType)
                    ? exceptionType
                    : exceptionForEntry?.GetType().FullName ?? string.Empty,
                exception_message = !string.IsNullOrWhiteSpace(exceptionMessage)
                    ? exceptionMessage
                    : exceptionForEntry?.Message ?? string.Empty
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

            EnqueueJsonLine(entry, json);
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

        private static bool ShouldEmit(string level, string component, string eventName)
        {
            if (GetLevelPriority(level) < GetLevelPriority(GetMinimumLevel()))
            {
                return false;
            }

            var includeComponents = ParseFilterValues(Environment.GetEnvironmentVariable(EnvIncludeComponents));
            var includeEvents = ParseFilterValues(Environment.GetEnvironmentVariable(EnvIncludeEvents));
            var excludeComponents = ParseFilterValues(Environment.GetEnvironmentVariable(EnvExcludeComponents));
            var excludeEvents = ParseFilterValues(Environment.GetEnvironmentVariable(EnvExcludeEvents));

            if (includeComponents.Length > 0 && !MatchesAnyFilter(component, includeComponents))
            {
                return false;
            }

            if (includeEvents.Length > 0 && !MatchesAnyFilter(eventName, includeEvents))
            {
                return false;
            }

            if (excludeComponents.Length > 0 && MatchesAnyFilter(component, excludeComponents))
            {
                return false;
            }

            if (excludeEvents.Length > 0 && MatchesAnyFilter(eventName, excludeEvents))
            {
                return false;
            }

            return true;
        }

        private static string[] ParseFilterValues(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw))
            {
                return Array.Empty<string>();
            }

            return raw
                .Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(v => v.Trim())
                .Where(v => !string.IsNullOrWhiteSpace(v))
                .ToArray();
        }

        private static bool MatchesAnyFilter(string value, string[] filters)
        {
            if (filters == null || filters.Length == 0)
            {
                return false;
            }

            var safeValue = value ?? string.Empty;
            for (var i = 0; i < filters.Length; i++)
            {
                var filter = filters[i];
                if (filter == "*")
                {
                    return true;
                }

                if (safeValue.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return true;
                }
            }

            return false;
        }

        private static string GetMinimumLevel()
        {
            var minLevel = Environment.GetEnvironmentVariable(EnvMinLevel);
            return string.IsNullOrWhiteSpace(minLevel) ? "INFO" : NormalizeLevel(minLevel);
        }

        private static int GetLevelPriority(string level)
        {
            if (string.Equals(level, "ERROR", StringComparison.Ordinal))
            {
                return 2;
            }

            if (string.Equals(level, "WARN", StringComparison.Ordinal))
            {
                return 1;
            }

            return 0;
        }

        private static void EnqueueJsonLine(RuntimeLogEntry entry, string json)
        {
            try
            {
                lock (StateLock)
                {
                    EnsureWriterThreadLocked();

                    var maxQueueSize = ParsePositiveInt(Environment.GetEnvironmentVariable(EnvMaxQueueSize), DefaultMaxQueueSize);
                    if (PendingLines.Count >= maxQueueSize)
                    {
                        PendingLines.Dequeue();
                    }

                    PendingLines.Enqueue(json);
                    RecentEntries.Add(CloneEntry(entry));
                    if (RecentEntries.Count > RecentEntryLimit)
                    {
                        RecentEntries.RemoveAt(0);
                    }
                }

                FlushSignal.Set();
            }
            catch
            {
                // Avoid recursive logging failure.
            }
        }

        private static void EnsureWriterThreadLocked()
        {
            if (writerRunning)
            {
                return;
            }

            writerShutdownRequested = false;
            writerThread = new Thread(WriterLoop)
            {
                IsBackground = true,
                Name = "MascotRuntimeLogWriter"
            };
            writerRunning = true;
            writerThread.Start();
        }

        private static void WriterLoop()
        {
            while (true)
            {
                var flushIntervalMs = ParsePositiveInt(Environment.GetEnvironmentVariable(EnvFlushIntervalMs), DefaultFlushIntervalMs);
                FlushSignal.WaitOne(flushIntervalMs);

                List<string> batch = null;
                lock (StateLock)
                {
                    if (PendingLines.Count == 0)
                    {
                        if (writerShutdownRequested)
                        {
                            writerRunning = false;
                            return;
                        }

                        continue;
                    }

                    writerIsFlushing = true;
                    var maxBatchSize = ParsePositiveInt(Environment.GetEnvironmentVariable(EnvMaxBatchSize), DefaultMaxBatchSize);
                    var takeCount = Math.Min(maxBatchSize, PendingLines.Count);
                    batch = new List<string>(takeCount);
                    for (var i = 0; i < takeCount; i++)
                    {
                        batch.Add(PendingLines.Dequeue());
                    }
                }

                bool shouldExit = false;
                try
                {
                    WriteBatch(batch);
                }
                catch
                {
                    // Avoid recursive logging failure.
                }
                finally
                {
                    lock (StateLock)
                    {
                        writerIsFlushing = false;
                        if (writerShutdownRequested && PendingLines.Count == 0)
                        {
                            writerRunning = false;
                            shouldExit = true;
                        }
                    }
                }

                if (shouldExit)
                {
                    return;
                }
        }
    }

        private static void WriteBatch(List<string> lines)
        {
            if (lines == null || lines.Count == 0)
            {
                return;
            }

            var logDir = cachedLogDir;
            // Fallback if initialization failed (unlikely if triggered from main thread first)
            if (string.IsNullOrEmpty(logDir))
            {
                // This will throw if on background thread, but we have no choice
                 logDir = Path.Combine(Application.persistentDataPath, "logs");
            }

            Directory.CreateDirectory(logDir);

            for (var i = 0; i < lines.Count; i++)
            {
                var line = lines[i] ?? string.Empty;
                var payload = line + Environment.NewLine;
                var bytes = Encoding.UTF8.GetByteCount(payload);
                var targetFile = ResolveActiveLogFile(logDir, bytes);
                File.AppendAllText(targetFile, payload, Encoding.UTF8);
            }

            EnforceRetention(logDir);
        }

        private static string ResolveActiveLogFile(string logDir, int incomingBytes)
        {
            var dateToken = DateTime.UtcNow.ToString("yyyyMMdd");
            var prefix = GetFilePrefix();
            if (!string.Equals(activeDateToken, dateToken, StringComparison.Ordinal) ||
                !string.Equals(activeFilePrefix, prefix, StringComparison.Ordinal) ||
                string.IsNullOrWhiteSpace(activeLogFilePath))
            {
                activeDateToken = dateToken;
                activeFilePrefix = prefix;
                activeFileIndex = 0;
                activeLogFilePath = BuildLogFilePath(logDir, prefix, dateToken, activeFileIndex);
            }

            var maxBytes = ParsePositiveLong(Environment.GetEnvironmentVariable(EnvMaxFileBytes), DefaultMaxFileBytes);
            var currentLength = File.Exists(activeLogFilePath) ? new FileInfo(activeLogFilePath).Length : 0L;
            if (currentLength + incomingBytes > maxBytes)
            {
                activeFileIndex++;
                activeLogFilePath = BuildLogFilePath(logDir, prefix, dateToken, activeFileIndex);
            }

            return activeLogFilePath;
        }

        private static void EnforceRetention(string logDir)
        {
            var maxFiles = ParsePositiveInt(Environment.GetEnvironmentVariable(EnvMaxFiles), DefaultMaxRetainedFiles);
            if (maxFiles <= 0)
            {
                return;
            }

            var prefix = GetFilePrefix();
            var pattern = $"{prefix}-*.jsonl";
            var files = new DirectoryInfo(logDir)
                .GetFiles(pattern, SearchOption.TopDirectoryOnly)
                .OrderByDescending(file => file.LastWriteTimeUtc)
                .ToArray();

            for (var i = maxFiles; i < files.Length; i++)
            {
                try
                {
                    files[i].Delete();
                }
                catch
                {
                    // Ignore retention failure.
                }
            }
        }

        private static string BuildLogFilePath(string logDir, string prefix, string dateToken, int index)
        {
            return Path.Combine(logDir, $"{prefix}-{dateToken}-{index:D2}.jsonl");
        }

        private static string GetFilePrefix()
        {
            var prefix = Environment.GetEnvironmentVariable(EnvFilePrefix);
            return string.IsNullOrWhiteSpace(prefix) ? "runtime" : prefix.Trim();
        }

        private static int ParsePositiveInt(string raw, int fallback)
        {
            if (int.TryParse(raw, out var parsed) && parsed > 0)
            {
                return parsed;
            }

            return fallback;
        }

        private static long ParsePositiveLong(string raw, long fallback)
        {
            if (long.TryParse(raw, out var parsed) && parsed > 0)
            {
                return parsed;
            }

            return fallback;
        }

        private static RuntimeLogEntry CloneEntry(RuntimeLogEntry source)
        {
            return new RuntimeLogEntry
            {
                timestamp = source?.timestamp ?? string.Empty,
                level = source?.level ?? string.Empty,
                component = source?.component ?? string.Empty,
                event_name = source?.event_name ?? string.Empty,
                request_id = source?.request_id ?? string.Empty,
                error_code = source?.error_code ?? string.Empty,
                message = source?.message ?? string.Empty,
                path = source?.path ?? string.Empty,
                source_tier = source?.source_tier ?? string.Empty,
                exception_type = source?.exception_type ?? string.Empty,
                exception_message = source?.exception_message ?? string.Empty
            };
        }

        private static Exception UnwrapException(Exception ex)
        {
            var current = ex;
            while (current is System.Reflection.TargetInvocationException invocation && invocation.InnerException != null)
            {
                current = invocation.InnerException;
            }

            return current;
        }

        private static void ShutdownWriter()
        {
            lock (StateLock)
            {
                writerShutdownRequested = true;
            }

            FlushSignal.Set();
            var thread = writerThread;
            if (thread != null && thread.IsAlive)
            {
                try
                {
                    thread.Join(500);
                }
                catch
                {
                    // Ignore shutdown failures.
                }
            }
        }
    }
}
