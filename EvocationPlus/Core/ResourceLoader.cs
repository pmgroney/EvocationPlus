using System;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace EvocationPlus.Core
{
    internal static class ResourceLoader
    {
        // resourcePath example: "EvocationPlus.Icons.corpse_explosion.png"
        internal static Sprite LoadSprite(string resourcePath, float pixelsPerUnit = 100f)
        {
            var asm = Assembly.GetExecutingAssembly();

            var stream = asm.GetManifestResourceStream(resourcePath);
            if (stream == null)
                throw new FileNotFoundException("Embedded resource not found: " + resourcePath);

            byte[] bytes;
            try
            {
                using (stream)
                using (var ms = new MemoryStream())
                {
                    stream.CopyTo(ms);
                    bytes = ms.ToArray();
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Failed reading embedded resource: " + resourcePath, ex);
            }

            var tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            if (!tex.LoadImage(bytes))
                throw new Exception("Failed to decode PNG for: " + resourcePath);

            tex.name = resourcePath;
            return Sprite.Create(
                tex,
                new Rect(0, 0, tex.width, tex.height),
                new Vector2(0.5f, 0.5f),
                pixelsPerUnit);
        }

        internal static string[] ListResources()
        {
            return Assembly.GetExecutingAssembly().GetManifestResourceNames();
        }
    }
}