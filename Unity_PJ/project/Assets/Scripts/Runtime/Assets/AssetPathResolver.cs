using System;
using System.IO;
using System.Linq;
using MascotDesktop.Runtime.Diagnostics;

namespace MascotDesktop.Runtime.Assets
{
    public sealed class AssetPathResolverOptions
    {
        public string CanonicalAssetsRoot;
        public string StreamingAssetsRoot;
        public bool ForbidLegacyRoot = true;
        public bool WarnOnNonAscii = true;
    }

    public sealed class AssetPathResolveResult
    {
        public bool Success;
        public string ResolvedPath;
        public string SourceTier;
        public string NormalizedRelativePath;
        public string ErrorCode;
        public string WarningCode;
        public string Message;
    }

    public sealed class AssetPathResolver
    {
        private readonly AssetPathResolverOptions _options;

        public AssetPathResolver(AssetPathResolverOptions options)
        {
            _options = options ?? new AssetPathResolverOptions();
        }

        public AssetPathResolveResult ResolveRelative(string relativePath, string requestId = null)
        {
            var req = string.IsNullOrWhiteSpace(requestId) ? RuntimeLog.NewRequestId() : requestId;

            if (string.IsNullOrWhiteSpace(relativePath))
            {
                return Fail(req, "ASSET.PATH.EMPTY", "relative path is empty", relativePath);
            }

            if (Path.IsPathRooted(relativePath))
            {
                return Fail(req, "ASSET.PATH.ABSOLUTE_FORBIDDEN", "absolute path is forbidden", relativePath);
            }

            var normalized = NormalizeRelative(relativePath);
            if (ContainsTraversal(normalized))
            {
                return Fail(req, "ASSET.PATH.TRAVERSAL_FORBIDDEN", "path traversal is forbidden", normalized);
            }

            if (_options.ForbidLegacyRoot && IsLegacyLikePath(normalized))
            {
                return Fail(req, "ASSET.PATH.LEGACY_FORBIDDEN", "legacy path is forbidden in this mode", normalized);
            }

            string warningCode = null;
            if (_options.WarnOnNonAscii && normalized.Any(ch => ch > 127))
            {
                warningCode = "ASSET.PATH.NON_ASCII_WARN";
                RuntimeLog.Warn(
                    "assets",
                    "assets.path.non_ascii",
                    req,
                    warningCode,
                    "non-ascii path segment detected",
                    normalized,
                    null);
            }

            var canonicalTry = CombineSafe(_options.CanonicalAssetsRoot, normalized);
            if (canonicalTry.Success && File.Exists(canonicalTry.FullPath))
            {
                RuntimeLog.Info(
                    "assets",
                    "assets.path.resolved",
                    req,
                    "resolved from canonical assets root",
                    canonicalTry.FullPath,
                    "assets_user");

                return new AssetPathResolveResult
                {
                    Success = true,
                    ResolvedPath = canonicalTry.FullPath,
                    SourceTier = "assets_user",
                    NormalizedRelativePath = normalized,
                    WarningCode = warningCode,
                    Message = "resolved from assets_user"
                };
            }

            var streamingTry = CombineSafe(_options.StreamingAssetsRoot, normalized);
            if (streamingTry.Success && File.Exists(streamingTry.FullPath))
            {
                RuntimeLog.Info(
                    "assets",
                    "assets.path.resolved",
                    req,
                    "resolved from streaming assets root",
                    streamingTry.FullPath,
                    "streaming_assets");

                return new AssetPathResolveResult
                {
                    Success = true,
                    ResolvedPath = streamingTry.FullPath,
                    SourceTier = "streaming_assets",
                    NormalizedRelativePath = normalized,
                    WarningCode = warningCode,
                    Message = "resolved from streaming_assets"
                };
            }

            return Fail(req, "ASSET.PATH.NOT_FOUND", "asset path not found in allowed roots", normalized, warningCode);
        }

        private AssetPathResolveResult Fail(
            string requestId,
            string errorCode,
            string message,
            string path,
            string warningCode = null)
        {
            RuntimeLog.Error(
                "assets",
                "assets.path.resolve_failed",
                requestId,
                errorCode,
                message,
                path);

            return new AssetPathResolveResult
            {
                Success = false,
                ErrorCode = errorCode,
                WarningCode = warningCode,
                NormalizedRelativePath = path,
                Message = message
            };
        }

        private static string NormalizeRelative(string relativePath)
        {
            var value = relativePath.Replace('\\', '/').Trim();
            while (value.StartsWith("./", StringComparison.Ordinal))
            {
                value = value.Substring(2);
            }

            return value;
        }

        private static bool ContainsTraversal(string normalizedPath)
        {
            return normalizedPath
                .Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries)
                .Any(p => p == "..");
        }

        private static bool IsLegacyLikePath(string normalizedPath)
        {
            var lower = normalizedPath.ToLowerInvariant();
            return lower.StartsWith("workspace/data/assets_user", StringComparison.Ordinal) ||
                   lower.Contains("/workspace/data/assets_user/");
        }

        private static (bool Success, string FullPath) CombineSafe(string root, string normalizedRelative)
        {
            if (string.IsNullOrWhiteSpace(root))
            {
                return (false, null);
            }

            try
            {
                var rootFull = Path.GetFullPath(root);
                var combined = Path.GetFullPath(Path.Combine(rootFull, normalizedRelative.Replace('/', Path.DirectorySeparatorChar)));

                if (!combined.StartsWith(rootFull, StringComparison.OrdinalIgnoreCase))
                {
                    return (false, null);
                }

                return (true, combined);
            }
            catch
            {
                return (false, null);
            }
        }
    }
}
