using System;
using System.IO;

namespace MascotDesktop.Runtime.Avatar
{
    public enum ModelAssetKind
    {
        Image,
        Vrm,
        Pmx,
        Unsupported
    }

    public static class ModelFormatRouter
    {
        public static ModelAssetKind Classify(string absolutePath)
        {
            var ext = Path.GetExtension(absolutePath)?.ToLowerInvariant();
            switch (ext)
            {
                case ".png":
                case ".jpg":
                case ".jpeg":
                case ".bmp":
                    return ModelAssetKind.Image;
                case ".vrm":
                    return ModelAssetKind.Vrm;
                case ".pmx":
                case ".pmd":
                    return ModelAssetKind.Pmx;
                default:
                    return ModelAssetKind.Unsupported;
            }
        }

        public static string UnsupportedExtensionErrorCode(string absolutePath)
        {
            var ext = Path.GetExtension(absolutePath)?.ToLowerInvariant();
            if (string.IsNullOrWhiteSpace(ext))
            {
                return "ASSET.READ.EXTENSION_MISSING";
            }

            return "ASSET.READ.UNSUPPORTED_EXTENSION";
        }
    }
}
