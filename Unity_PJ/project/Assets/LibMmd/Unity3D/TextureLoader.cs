using System;
using System.Collections.Generic;
using System.IO;
using LibMMD.Material;
using LibMMD.Unity3D.ImageLoader;
using UnityEngine;
using Object = UnityEngine.Object;

namespace LibMMD.Unity3D
{
    public class TextureLoader : IDisposable
    {
        private const string SysToonPath = "LibMmd/SysToon/";
        private const bool EnableVerboseLogs = false;
        private static readonly bool EnableTgaPngFallback = ReadTgaPngFallbackFlag();
        private static readonly bool EnableTextureTraceLogs = EnableVerboseLogs || ReadTextureTraceFlag();

        private readonly string _relativePath;

        private readonly int _maxTextureSize;

        private static readonly HashSet<string> SysToonNames = new HashSet<string>();
        private static readonly Dictionary<string, string> TextureResolveCache = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        private static readonly object TextureResolveCacheLock = new object();
        private const string MissingPathSentinel = "<missing>";
        
        private readonly Dictionary<string, Texture> _textureMap = new Dictionary<string, Texture>();

        static TextureLoader()
        {
            SysToonNames.Add("toon0.bmp");
            for (var i = 1; i < 9; i++)
            {
                SysToonNames.Add("toon0" + i + ".bmp");
            }
            SysToonNames.Add("toon10.bmp");
        }

        private static bool ReadTextureTraceFlag()
        {
            var flag = Environment.GetEnvironmentVariable("MASCOTDESKTOP_PMX_TEXTURE_TRACE");
            return string.Equals(flag, "1", StringComparison.OrdinalIgnoreCase)
                   || string.Equals(flag, "true", StringComparison.OrdinalIgnoreCase);
        }

        private static bool ReadTgaPngFallbackFlag()
        {
            var flag = Environment.GetEnvironmentVariable("MASCOTDESKTOP_PMX_TGA_PNG_FALLBACK");
            return string.Equals(flag, "1", StringComparison.OrdinalIgnoreCase)
                   || string.Equals(flag, "true", StringComparison.OrdinalIgnoreCase);
        }

        public TextureLoader(string relativePath, int maxTextureSize = 0)
        {
            _relativePath = relativePath + Path.DirectorySeparatorChar;
            _maxTextureSize = maxTextureSize;
        }

        public Texture LoadTexture(MmdTexture mmdTexture)
        {
            return mmdTexture == null ? null : LoadTexture(mmdTexture.TexturePath);
        }
        
        private Texture LoadTexture(string path)
        {
            if (EnableTextureTraceLogs)
            {
                Debug.LogFormat("load texture {0}", path);
            }
            if (string.IsNullOrEmpty(path))
            {
                return null;
            }
            path = ReplaceFileSeperator(path);
            Texture ret;
            if (_textureMap.TryGetValue(path, out ret))
            {
                return ret;
            }
            ret = DoLoadTexture(path);
            if (ret != null)
            {
                _textureMap.Add(path, ret);
            }
            return ret;
        }

        public void Dispose()
        {
            foreach (var entry in _textureMap)
            {
                if (SysToonNames.Contains(entry.Key))
                {
                    continue;
                }
                var tex2D = entry.Value as Texture2D;
                if (tex2D == null)
                {
                    continue;
                }
                Object.Destroy(tex2D);
            }
        }

        private string ReplaceFileSeperator(string path)
        {
            if (Path.DirectorySeparatorChar.Equals('/'))
            {
                path = path.Replace('\\', '/');
            }
            else if (Path.DirectorySeparatorChar.Equals('\\'))
            {
                path = path.Replace('/', '\\');
            }
            return path;
        }

        private Texture DoLoadTexture(string path)
        {
            if (SysToonNames.Contains(path))
            {
                var filename = Path.GetFileNameWithoutExtension(path);
                return Resources.Load(SysToonPath + filename) as Texture;
            }
            var resolvedPath = ResolveTexturePath(path);
            if (string.IsNullOrEmpty(resolvedPath))
            {
                Debug.LogWarningFormat("texture file not exists {0}", path);
                return null;
            }
            path = resolvedPath;
            try
            {
                var extension = Path.GetExtension(path);
                if (extension != null)
                {
                    var ext = extension.ToLower();
                    Texture ret = null;
                    if (".jpg".Equals(ext) || ".jpeg".Equals(ext))
                    {
                        ret = LoadJpg(path);
                    }
                    else if (".png".Equals(ext))
                    {
                        ret = LoadPng(path);
                    }
                    else if (".bmp".Equals(ext))
                    {
                        ret = LoadBmp(path);
                    }
                    else if (".tga".Equals(ext))
                    {
                        var pngPath = Path.ChangeExtension(path, ".png");
                        var hasSiblingPng = !string.IsNullOrWhiteSpace(pngPath) && File.Exists(pngPath);
                        if (EnableTgaPngFallback && hasSiblingPng)
                        {
                            if (EnableTextureTraceLogs)
                            {
                                Debug.LogFormat("TGA->PNG fallback: using {0} instead of {1}", pngPath, path);
                            }
                            ret = LoadPng(pngPath);
                            if (ret == null && EnableTextureTraceLogs)
                            {
                                Debug.LogWarningFormat("TGA->PNG fallback failed to decode: {0}; trying TGA", pngPath);
                            }
                        }

                        if (ret == null)
                        {
                            ret = LoadTga(path);
                        }
                        if (ret == null && EnableTextureTraceLogs)
                        {
                            Debug.LogWarningFormat("TGA loader returned null for: {0}", path);
                        }
                        else if (EnableTextureTraceLogs)
                        {
                            var tex = ret as Texture2D;
                            if (tex != null)
                            {
                                Debug.LogFormat("TGA loaded: {0} size={1}x{2} format={3}", path, ret.width, ret.height, tex.format);
                            }
                            else
                            {
                                Debug.LogFormat("TGA loaded: {0} size={1}x{2}", path, ret.width, ret.height);
                            }
                        }
                    }
                    else if (".dds".Equals(ext))
                    {
                        ret = LoadDds(path);
                    }
                    else
                    {
                        ret = TryLoadWithAllFormats(path);
                    }
                    var tex2D = ret as Texture2D;
                    if (tex2D != null)
                    {
                        RescaleLargeTexture(tex2D);
                        //tex2D.Compress(false);
                    }
                    return ret;
                }
            }
            catch (Exception e)
            {
                Debug.LogWarningFormat("failed to load texture {0}, {1}", path, e);
            }
            return null;
        }

        private string ResolveTexturePath(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return null;
            }

            if (File.Exists(path))
            {
                if (EnableTextureTraceLogs)
                {
                    Debug.LogFormat("[TextureLoader] resolve hit direct path: request={0}", path);
                }
                return path;
            }

            var baseDir = _relativePath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            var fileName = Path.GetFileName(path);
            var candidates = new List<KeyValuePair<string, string>>();
            if (EnableTextureTraceLogs)
            {
                Debug.LogFormat("[TextureLoader] resolve start: request={0}, base={1}", path, baseDir);
            }

            // First candidate: direct combination with baseDir (handles ../texture/... paths)
            try
            {
                var directPath = Path.GetFullPath(Path.Combine(baseDir, path));
                candidates.Add(new KeyValuePair<string, string>("model_relative", directPath));
            }
            catch (Exception ex)
            {
                Debug.LogWarningFormat(
                    "[TextureLoader] resolve candidate build failed: request={0}, base={1}, reason={2}",
                    path,
                    baseDir,
                    ex.Message);
            }

            if (!string.IsNullOrEmpty(fileName))
            {
                candidates.Add(new KeyValuePair<string, string>("model_dir_filename", Path.Combine(baseDir, fileName)));
            }

            var textureDir = Path.Combine(baseDir, "texture");
            var texturesDir = Path.Combine(baseDir, "textures");
            var textureDirExists = Directory.Exists(textureDir);
            var texturesDirExists = Directory.Exists(texturesDir);
            if (!string.IsNullOrEmpty(fileName))
            {
                candidates.Add(new KeyValuePair<string, string>("model_texture_dir_filename", Path.Combine(textureDir, fileName)));
                candidates.Add(new KeyValuePair<string, string>("model_textures_dir_filename", Path.Combine(texturesDir, fileName)));
            }
            candidates.Add(new KeyValuePair<string, string>("model_texture_dir_requested", Path.Combine(textureDir, path)));
            candidates.Add(new KeyValuePair<string, string>("model_textures_dir_requested", Path.Combine(texturesDir, path)));

            var parentDir = Directory.GetParent(baseDir)?.FullName;
            var parentTextureExists = false;
            var parentTexturesExists = false;
            if (!string.IsNullOrEmpty(parentDir))
            {
                var parentTexture = Path.Combine(parentDir, "texture");
                var parentTextures = Path.Combine(parentDir, "textures");
                parentTextureExists = Directory.Exists(parentTexture);
                parentTexturesExists = Directory.Exists(parentTextures);
                if (!string.IsNullOrEmpty(fileName))
                {
                    candidates.Add(new KeyValuePair<string, string>("parent_texture_dir_filename", Path.Combine(parentTexture, fileName)));
                    candidates.Add(new KeyValuePair<string, string>("parent_textures_dir_filename", Path.Combine(parentTextures, fileName)));
                }
                candidates.Add(new KeyValuePair<string, string>("parent_texture_dir_requested", Path.Combine(parentTexture, path)));
                candidates.Add(new KeyValuePair<string, string>("parent_textures_dir_requested", Path.Combine(parentTextures, path)));
            }

            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var entry in candidates)
            {
                var strategy = entry.Key;
                var rawCandidate = entry.Value;
                if (string.IsNullOrWhiteSpace(rawCandidate))
                {
                    continue;
                }
                // Normalize path to resolve .. and . segments
                string candidate;
                try
                {
                    candidate = Path.GetFullPath(rawCandidate);
                }
                catch (Exception ex)
                {
                    candidate = rawCandidate;
                    Debug.LogWarningFormat(
                        "[TextureLoader] resolve candidate normalize failed: raw={0}, strategy={1}, reason={2}",
                        rawCandidate,
                        strategy,
                        ex.Message);
                }
                if (!seen.Add(candidate))
                {
                    continue;
                }
                if (File.Exists(candidate))
                {
                    if (!string.Equals(strategy, "model_relative", StringComparison.OrdinalIgnoreCase))
                    {
                        Debug.LogFormat(
                            "[TextureLoader] resolve fallback: request={0}, resolved={1}, strategy={2}",
                            path,
                            candidate,
                            strategy);
                    }
                    else if (EnableTextureTraceLogs)
                    {
                        Debug.LogFormat("[TextureLoader] resolve model-relative: request={0}, resolved={1}", path, candidate);
                    }
                    return candidate;
                }
            }

            if (!string.IsNullOrEmpty(fileName))
            {
                var recursiveRoots = new[]
                {
                    baseDir,
                    parentDir
                };
                var searchedRoots = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                string bestResolvedPath = null;
                var bestResolvedScore = int.MaxValue;
                foreach (var root in recursiveRoots)
                {
                    if (string.IsNullOrWhiteSpace(root) || !searchedRoots.Add(root))
                    {
                        continue;
                    }

                    var resolved = FindTextureByFileNameWithCache(root, fileName, baseDir, path);
                    if (string.IsNullOrEmpty(resolved))
                    {
                        continue;
                    }

                    var score = ScoreTexturePathForRequest(resolved, baseDir, path);
                    if (bestResolvedPath == null ||
                        score < bestResolvedScore ||
                        (score == bestResolvedScore && string.Compare(resolved, bestResolvedPath, StringComparison.OrdinalIgnoreCase) < 0))
                    {
                        bestResolvedPath = resolved;
                        bestResolvedScore = score;
                    }
                }

                if (!string.IsNullOrEmpty(bestResolvedPath))
                {
                    Debug.LogFormat(
                        "[TextureLoader] resolve recursive: request={0}, resolved={1}, score={2}",
                        path,
                        bestResolvedPath,
                        bestResolvedScore);

                    return bestResolvedPath;
                }
            }

            Debug.LogWarningFormat(
                "[TextureLoader] resolve failed: request={0}, base={1}, fileName={2}, textureDirExists={3}, texturesDirExists={4}, parentTextureDirExists={5}, parentTexturesDirExists={6}",
                path,
                baseDir,
                string.IsNullOrEmpty(fileName) ? "(none)" : fileName,
                textureDirExists,
                texturesDirExists,
                parentTextureExists,
                parentTexturesExists);

            return null;
        }

        private static string FindTextureByFileNameWithCache(string rootDir, string fileName, string modelBaseDir, string requestedPath)
        {
            if (string.IsNullOrWhiteSpace(rootDir) || string.IsNullOrWhiteSpace(fileName) || !Directory.Exists(rootDir))
            {
                return null;
            }

            var cacheKey = string.Concat(
                rootDir, "|",
                fileName, "|",
                modelBaseDir ?? string.Empty, "|",
                NormalizePathForLookup(requestedPath));
            lock (TextureResolveCacheLock)
            {
                string cachedPath;
                if (TextureResolveCache.TryGetValue(cacheKey, out cachedPath))
                {
                    return cachedPath == MissingPathSentinel ? null : cachedPath;
                }
            }

            var found = FindTextureByFileName(rootDir, fileName, modelBaseDir, requestedPath);
            lock (TextureResolveCacheLock)
            {
                TextureResolveCache[cacheKey] = string.IsNullOrEmpty(found) ? MissingPathSentinel : found;
            }

            return found;
        }

        private static string FindTextureByFileName(string rootDir, string fileName, string modelBaseDir, string requestedPath)
        {
            try
            {
                string selectedPath = null;
                var selectedScore = int.MaxValue;
                foreach (var candidate in Directory.EnumerateFiles(rootDir, fileName, SearchOption.AllDirectories))
                {
                    var score = ScoreTexturePathForRequest(candidate, modelBaseDir, requestedPath);
                    if (selectedPath == null ||
                        score < selectedScore ||
                        (score == selectedScore && string.Compare(candidate, selectedPath, StringComparison.OrdinalIgnoreCase) < 0))
                    {
                        selectedPath = candidate;
                        selectedScore = score;
                    }
                }

                return selectedPath;
            }
            catch (Exception ex)
            {
                Debug.LogWarningFormat(
                    "[TextureLoader] recursive texture scan failed: root={0}, file={1}, reason={2}",
                    rootDir,
                    fileName,
                    ex.Message);
                return null;
            }
        }

        private static int ScoreTexturePathForRequest(string candidatePath, string modelBaseDir, string requestedPath)
        {
            var folderPreferenceScore = ScoreTexturePath(candidatePath);
            var proximityScore = ScoreModelProximity(candidatePath, modelBaseDir);
            var hintScore = ScoreRequestedPathHint(candidatePath, requestedPath);
            return proximityScore * 100 + hintScore * 10 + folderPreferenceScore;
        }

        private static int ScoreModelProximity(string candidatePath, string modelBaseDir)
        {
            if (string.IsNullOrWhiteSpace(candidatePath) || string.IsNullOrWhiteSpace(modelBaseDir))
            {
                return 0;
            }

            var candidateDirectory = Path.GetDirectoryName(candidatePath);
            if (string.IsNullOrWhiteSpace(candidateDirectory))
            {
                return 0;
            }

            var candidateSegments = SplitPathSegments(candidateDirectory);
            var modelSegments = SplitPathSegments(modelBaseDir);
            if (candidateSegments.Length == 0 || modelSegments.Length == 0)
            {
                return 0;
            }

            var commonPrefixLength = 0;
            var compareLength = Math.Min(candidateSegments.Length, modelSegments.Length);
            while (commonPrefixLength < compareLength &&
                   string.Equals(candidateSegments[commonPrefixLength], modelSegments[commonPrefixLength], StringComparison.OrdinalIgnoreCase))
            {
                commonPrefixLength++;
            }

            return (candidateSegments.Length - commonPrefixLength) + (modelSegments.Length - commonPrefixLength);
        }

        private static int ScoreRequestedPathHint(string candidatePath, string requestedPath)
        {
            if (string.IsNullOrWhiteSpace(candidatePath) || string.IsNullOrWhiteSpace(requestedPath))
            {
                return 0;
            }

            var requestedDirectory = Path.GetDirectoryName(requestedPath);
            if (string.IsNullOrWhiteSpace(requestedDirectory))
            {
                return 0;
            }

            var hintSegments = SplitPathSegments(requestedDirectory);
            if (hintSegments.Length == 0)
            {
                return 0;
            }

            var normalizedCandidatePath = NormalizePathForLookup(candidatePath);
            var matchedHintCount = 0;
            for (var i = 0; i < hintSegments.Length; i++)
            {
                var segment = hintSegments[i];
                if (segment == "." || segment == "..")
                {
                    continue;
                }

                if (normalizedCandidatePath.IndexOf("/" + segment + "/", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    matchedHintCount++;
                }
            }

            if (matchedHintCount >= hintSegments.Length)
            {
                return 0;
            }

            if (matchedHintCount > 0)
            {
                return 1;
            }

            return 3;
        }

        private static string[] SplitPathSegments(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return new string[0];
            }

            var normalized = NormalizePathForLookup(path);
            return normalized.Split(new[] {'/'}, StringSplitOptions.RemoveEmptyEntries);
        }

        private static string NormalizePathForLookup(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return string.Empty;
            }

            return path.Replace('\\', '/');
        }

        private static int ScoreTexturePath(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return int.MaxValue;
            }

            var normalized = path.Replace('\\', '/');
            if (normalized.IndexOf("/textures/", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return 0;
            }

            if (normalized.IndexOf("/texture/", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return 1;
            }

            return 10;
        }

        private void RescaleLargeTexture(Texture2D tex)
        {
            if (_maxTextureSize <= 0)
            {
                return;
            }
            if (tex.width <= _maxTextureSize && tex.height <= _maxTextureSize)
            {
                return;
            }
            try
            {
                var scaleX = (float)_maxTextureSize / tex.width;
                var scaleY = (float)_maxTextureSize / tex.height;
                var scale = Math.Min(scaleX, scaleY);
                var targetWidth = Math.Max(1, (int)Math.Round(tex.width * scale));
                var targetHeight = Math.Max(1, (int)Math.Round(tex.height * scale));
                TextureScale.Bilinear(tex, targetWidth, targetHeight);
            }
            catch (Exception e)
            {
                Debug.LogWarningFormat("Resize texture failed. {0}", e);
            }
        }

        private Texture DoLoadCubemap(string path)
        {
            var tex2D = DoLoadTexture(path) as Texture2D;
            return tex2D == null ? null : Texture2DToCubeMap(tex2D);
        }

        private static Cubemap Texture2DToCubeMap(Texture2D texture2D)
        {
            if (texture2D.width != texture2D.height)
            {
                Debug.LogWarning("Can't convert a Texture2D to Cubemap when width and height are different");
                return null;
            }
            var ret = new Cubemap(texture2D.width, texture2D.format, false);
            var texPixels = texture2D.GetPixels();
            ret.SetPixels(texPixels, CubemapFace.NegativeX);
            ret.SetPixels(texPixels, CubemapFace.NegativeY);
            ret.SetPixels(texPixels, CubemapFace.NegativeZ);
            ret.SetPixels(texPixels, CubemapFace.PositiveX);
            ret.SetPixels(texPixels, CubemapFace.PositiveY);
            ret.SetPixels(texPixels, CubemapFace.PositiveZ);
            ret.Apply();
            return ret;
        }

        private static Texture TryLoadWithAllFormats(string path)
        {
            var ret = LoadBmp(path);
            if (ret != null)
            {
                return ret;
            }
            ret = LoadPng(path);
            if (ret != null)
            {
                return ret;
            }
            ret = LoadJpg(path);
            return ret;
        }

        private static Texture LoadJpg(string path)
        {
            return LoadWithUnity(path);
        }

        private static Texture LoadPng(string path)
        {
            return LoadWithUnity(path);
        }

        private static Texture LoadBmp(string path)
        {
            var img = BitmapLoader.LoadFromFile(path);
            if (img == null)
            {
                // BitmapLoader does not support some compressed BMP variants.
                // Fallback to Unity decoder before giving up.
                var fallback = LoadWithUnity(path);
                if (fallback != null)
                {
                    return fallback;
                }

                // Some packs include side-by-side png alternatives.
                var pngPath = Path.ChangeExtension(path, ".png");
                if (!string.IsNullOrWhiteSpace(pngPath) && File.Exists(pngPath))
                {
                    return LoadPng(pngPath);
                }

                return null;
            }
            var ret = new Texture2D(img.Width, img.Height, TextureFormat.ARGB32, false);
            ret.SetPixels(img.Pixels);
            ret.Apply();
            return ret;
        }

        private static Texture LoadTga(string path)
        {
            return TargaImage.LoadTargaImage(path);
        }

        private static Texture LoadDds(string path)
        {
            var bytes = File.ReadAllBytes(path);
            var width = DdsLoader.DdsGetWidth(bytes);
            var height = DdsLoader.DdsGetHeight(bytes);
            var nMipmap = DdsLoader.DdsGetMipmap(bytes);
            var hasMipmap = nMipmap > 1;
            var ret = new Texture2D(width, height, TextureFormat.ARGB32, hasMipmap);
            if (hasMipmap)
            {
                for (var i = 0; i < nMipmap; i++)
                {
                    var intColors = DdsLoader.DdsRead(bytes, DdsLoader.DdsReaderArgb, i);
                    ret.SetPixels(IntsArgbToColorUpsideDown(intColors, width / (1 << i), height / (1 << i)), i);
                }
            }
            else
            {
                var intColors = DdsLoader.DdsRead(bytes, DdsLoader.DdsReaderArgb, 0);
                ret.SetPixels(IntsArgbToColorUpsideDown(intColors, width, height));
            }
            ret.Apply();
            return ret;
        }

        public static Color[] IntsArgbToColorUpsideDown(int[] ints, int width, int height)
        {
            var ret = new Color[ints.Length];
            for (var i = 0; i < height; i++)
            {
                for (var j = 0; j < width; j++)
                {
                    var intColor = ints[i * width + j];
                    var dstIndex = (height - 1 - i) * width + j;
                    ret[dstIndex].b = (intColor & 0xFF) / 255.0f;
                    ret[dstIndex].g = ((intColor >> 8) & 0xFF) / 255.0f;
                    ret[dstIndex].r = ((intColor >> 16) & 0xFF) / 255.0f;
                    ret[dstIndex].a = ((intColor >> 24) & 0xFF) / 255.0f;
                }
            }
            return ret;
        }

        private static Texture LoadWithUnity(string path)
        {
            if (!File.Exists(path)) return null;
            var fileData = File.ReadAllBytes(path);
            var tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            if (!tex.LoadImage(fileData))
            {
                Object.Destroy(tex);
                return null;
            }
            return tex;
        }
    }
}
