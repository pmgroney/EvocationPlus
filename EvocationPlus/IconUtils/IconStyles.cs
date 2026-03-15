using UnityEngine;

namespace EvocationPlus.IconUtils
{
    public static class IconStyles
    {
        public static IconShader.PixelTransform Theme(
            IconTheme theme,
            float gamma,
            float brightness,
            float whiteStart,
            float whiteRange)
        {
            switch (theme)
            {
                case IconTheme.Lightning:
                    return LightningWhite(gamma, brightness, whiteStart, whiteRange);
                case IconTheme.Acid:
                    return AcidGreen(gamma, brightness, whiteStart, whiteRange);
                case IconTheme.Water:
                    return IceFrost(gamma, brightness, whiteStart, whiteRange);
                case IconTheme.Fire:
                    return FireRed(gamma, brightness, whiteStart, whiteRange);
                default:
                    return LightningWhite(gamma, brightness, whiteStart, whiteRange);
            }
        }
        public static IconShader.PixelTransform LumaTint(LumaTintStyle s)
        {
            return src =>
            {
                // luminance from source pixel
                var lum = src.r * 0.299f + src.g * 0.587f + src.b * 0.114f;

                // brightness curve
                var b = Mathf.Clamp01(Mathf.Pow(lum, s.Gamma) * s.Brightness);

                // highlight ramp (avoid div-by-zero)
                var t = s.WhiteRange <= 0.0001f
                    ? b >= s.WhiteStart ? 1f : 0f
                    : Mathf.Clamp01((b - s.WhiteStart) / s.WhiteRange);

                // tint scaling
                var body = new Color(s.BodyTint.r * b, s.BodyTint.g * b, s.BodyTint.b * b, src.a);
                var hi = new Color(s.HighlightTint.r * b, s.HighlightTint.g * b, s.HighlightTint.b * b, src.a);

                return Color.Lerp(body, hi, t);
            };
        }

        // ---- Optional convenience wrappers / presets ----
        public static IconShader.PixelTransform IceFrost(
            float gamma,
            float brightness,
            float whiteStart,
            float whiteRange)
        {
            return LumaTint(new LumaTintStyle
            {
                Gamma = gamma,
                Brightness = brightness,
                WhiteStart = whiteStart,
                WhiteRange = whiteRange,

                // Warmer, slightly teal-leaning blue
                BodyTint = new Color(0.30f, 0.55f, 0.90f),

                // Frosty white with slight blue tint (not pure white)
                HighlightTint = new Color(0.85f, 0.95f, 1.00f)
            });
        }

        public static IconShader.PixelTransform FireRed(
            float gamma,
            float brightness,
            float whiteStart,
            float whiteRange)
        {
            return LumaTint(new LumaTintStyle
            {
                Gamma = gamma,
                Brightness = brightness,
                WhiteStart = whiteStart,
                WhiteRange = whiteRange,

                // darker ember reds in body
                BodyTint = new Color(0.85f, 0.18f, 0.02f),

                // strong yellow/white flame core
                HighlightTint = new Color(1.00f, 0.95f, 0.60f)
            });
        }
        public static IconShader.PixelTransform LightningWhite(
            float gamma,
            float brightness,
            float whiteStart,
            float whiteRange)
        {
            return LumaTint(new LumaTintStyle
            {
                Gamma = gamma,
                Brightness = brightness,
                WhiteStart = whiteStart,
                WhiteRange = whiteRange,
                BodyTint = new Color(0.15f, 0.45f, 1.00f),   // deeper electric blue
                HighlightTint = new Color(1.00f, 1.00f, 1.00f) // pure white core
            });
        }

        public static IconShader.PixelTransform AcidGreen(
            float gamma,
            float brightness,
            float whiteStart,
            float whiteRange)
        {
            return LumaTint(new LumaTintStyle
            {
                Gamma = gamma,
                Brightness = brightness,
                WhiteStart = whiteStart,
                WhiteRange = whiteRange,
                BodyTint = new Color(0.10f, 0.95f, 0.15f),
                HighlightTint = new Color(0.70f, 1.00f, 0.65f)
            });
        }

        /// <summary>
        ///     Generic “luminance -> tinted body + highlight” style.
        ///     Works for lightning, acid, frost, fire, etc.
        /// </summary>
        public struct LumaTintStyle
        {
            public float Gamma; // how much to lift/darken
            public float Brightness; // overall brightness multiplier
            public float WhiteStart; // threshold where whitening begins (0..1)
            public float WhiteRange; // ramp range for whitening (0..1)

            public Color BodyTint; // base tint (RGB multipliers)
            public Color HighlightTint; // highlight tint (RGB multipliers)
        }
    }
}