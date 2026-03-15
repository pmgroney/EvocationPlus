using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Object = UnityEngine.Object;

namespace EvocationPlus.IconUtils
{
    internal static class IconCreator
    {
        private static readonly Dictionary<string, Sprite> SpriteCache =
            new Dictionary<string, Sprite>(StringComparer.OrdinalIgnoreCase);

        private static readonly Dictionary<string, Texture2D> TextureCache =
            new Dictionary<string, Texture2D>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        ///     Load a PNG from disk into a Sprite. Caches by absolute path.
        /// </summary>
        public static Sprite LoadSpriteFromPng(string path, float pixelsPerUnit = 100f)
        {
            if (string.IsNullOrWhiteSpace(path))
                return null;

            var fullPath = Path.GetFullPath(path);

            if (SpriteCache.TryGetValue(fullPath, out var cached))
                return cached;

            if (!File.Exists(fullPath))
            {
                Main.Mod.Logger.Log($"EvocationPlus: PNG not found: {fullPath}");
                return null;
            }

            try
            {
                var bytes = File.ReadAllBytes(fullPath);

                var tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
                tex.name = $"EvocationPlusIconTex:{Path.GetFileNameWithoutExtension(fullPath)}";

                // Kingmaker-friendly: explicit ImageConversion call
                if (!tex.LoadImage(bytes, false))
                {
                    Main.Mod.Logger.Log($"EvocationPlus: LoadImage failed: {fullPath}");
                    Object.Destroy(tex);
                    return null;
                }

                tex.filterMode = FilterMode.Bilinear;
                tex.wrapMode = TextureWrapMode.Clamp;

                var rect = new Rect(0, 0, tex.width, tex.height);
                var pivot = new Vector2(0.5f, 0.5f);
                var sprite = Sprite.Create(tex, rect, pivot, pixelsPerUnit);
                sprite.name = $"EvocationPlusIconSprite:{Path.GetFileNameWithoutExtension(fullPath)}";

                TextureCache[fullPath] = tex;
                SpriteCache[fullPath] = sprite;

                return sprite;
            }
            catch (Exception ex)
            {
                Main.Mod.Logger.Log($"EvocationPlus: Exception loading icon {fullPath}: {ex}");
                return null;
            }
        }

        /// <summary>
        ///     Dev helper: clears cached textures/sprites.
        /// </summary>
        public static void ClearCache()
        {
            foreach (var s in SpriteCache.Values)
                Object.Destroy(s);

            foreach (var t in TextureCache.Values)
                Object.Destroy(t);

            SpriteCache.Clear();
            TextureCache.Clear();
        }
    }
}