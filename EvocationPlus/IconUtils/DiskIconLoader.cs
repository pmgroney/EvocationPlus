using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace EvocationPlus.IconUtils
{
    public static class DiskIconLoader
    {
        private static readonly Dictionary<string, Sprite> Cache =
            new Dictionary<string, Sprite>(StringComparer.OrdinalIgnoreCase);

        // Set this once at init: Path.Combine(ModEntry.Path, "Icons")
        public static string IconsRootDir { get; set; }

        public static Sprite LoadSprite(string fileNameOrRelativePath, float pixelsPerUnit = 100f)
        {
            if (string.IsNullOrWhiteSpace(fileNameOrRelativePath))
                throw new ArgumentNullException(nameof(fileNameOrRelativePath));

            var fullPath = Path.IsPathRooted(fileNameOrRelativePath)
                ? fileNameOrRelativePath
                : Path.Combine(IconsRootDir ?? "", fileNameOrRelativePath);

            fullPath = Path.GetFullPath(fullPath);

            if (Cache.TryGetValue(fullPath, out var cached))
                return cached;

            if (!File.Exists(fullPath))
            {
                Main.Mod.Logger.Log($"DiskIconLoader: icon file not found: {fullPath}");
                return null;
            }

            var bytes = File.ReadAllBytes(fullPath);

            var tex = new Texture2D(2, 2, TextureFormat.RGBA32, false, false);
            if (!tex.LoadImage(bytes, true))
            {
                Main.Mod.Logger.Log($"DiskIconLoader: failed to decode PNG: {fullPath}");
                return null;
            }

            tex.wrapMode = TextureWrapMode.Clamp;
            tex.filterMode = FilterMode.Bilinear;

            var sprite = Sprite.Create(
                tex,
                new Rect(0, 0, tex.width, tex.height),
                new Vector2(0.5f, 0.5f),
                pixelsPerUnit);

            Cache[fullPath] = sprite;
            return sprite;
        }
    }
}