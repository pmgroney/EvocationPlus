using UnityEngine;

namespace EvocationPlus.IconUtils
{
    public static class IconShader
    {
        public delegate Color PixelTransform(Color src);

        public static Sprite CreateTransformedCopy(Sprite original, PixelTransform transform)
        {
            if (original == null || transform == null) return null;

            var readable = ExtractSpriteTextureReadable(original);
            if (readable == null) return null;

            var pixels = readable.GetPixels();
            for (var i = 0; i < pixels.Length; i++)
            {
                var p = pixels[i];
                if (p.a <= 0f) continue;
                pixels[i] = transform(p);
            }

            readable.SetPixels(pixels);
            readable.Apply();

            return Sprite.Create(
                readable,
                new Rect(0, 0, readable.width, readable.height),
                new Vector2(original.pivot.x / original.rect.width, original.pivot.y / original.rect.height),
                original.pixelsPerUnit
            );
        }

        private static Texture2D ExtractSpriteTextureReadable(Sprite sprite)
        {
            var srcTex = sprite.texture;
            if (srcTex == null) return null;

            var r = sprite.rect;
            var w = Mathf.RoundToInt(r.width);
            var h = Mathf.RoundToInt(r.height);
            if (w <= 0 || h <= 0) return null;

            var prev = RenderTexture.active;
            var rt = RenderTexture.GetTemporary(w, h, 0, RenderTextureFormat.ARGB32);

            try
            {
                Graphics.Blit(srcTex, rt);
                RenderTexture.active = rt;

                var dst = new Texture2D(w, h, TextureFormat.RGBA32, false);
                dst.ReadPixels(new Rect(r.x, r.y, r.width, r.height), 0, 0);
                dst.Apply();
                return dst;
            }
            finally
            {
                RenderTexture.active = prev;
                RenderTexture.ReleaseTemporary(rt);
            }
        }
    }
}