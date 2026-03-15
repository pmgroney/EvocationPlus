using System;
using System.Collections.Generic;
using EvocationPlus.BlueprintUtils;
using EvocationPlus.Core;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using UnityEngine;

namespace EvocationPlus.IconUtils
{
    public static class IconTintUtil
    {
        // Keyed by: source sprite + theme + packed float params
        private static readonly Dictionary<long, Sprite> Cache = new Dictionary<long, Sprite>();

        public static void ApplyTheme(
            BlueprintAbility spell,
            IconTheme theme,
            float gamma = 0.85f,
            float brightness = 1.6f,
            float whiteStart = 0.55f,
            float whiteRange = 0.35f,
            bool useCache = true)
        {
            if (spell == null || spell.Icon == null) return;

            var transform = IconStyles.Theme(theme, gamma, brightness, whiteStart, whiteRange);

            try
            {
                if (!useCache)
                {
                    var tintedNoCache = IconShader.CreateTransformedCopy(spell.Icon, transform);
                    BlueprintUnitFactUI.SetIcon(spell, tintedNoCache);
                    return;
                }

                var key = MakeKey(spell.Icon, theme, gamma, brightness, whiteStart, whiteRange);

                if (!Cache.TryGetValue(key, out var tinted) || tinted == null)
                {
                    tinted = IconShader.CreateTransformedCopy(spell.Icon, transform);
                    Cache[key] = tinted;
                }

                BlueprintUnitFactUI.SetIcon(spell, tinted);
            }
            catch (Exception ex)
            {
                Main.Mod.Logger.Log("IconTintUtil.ApplyTheme failed: " + ex);
            }
        }

        public static void ApplyTheme(
            BlueprintFeature feature,
            IconTheme theme,
            float gamma = 0.85f,
            float brightness = 1.6f,
            float whiteStart = 0.55f,
            float whiteRange = 0.35f,
            bool useCache = true)
        {
            if (feature == null || feature.Icon == null) return;

            var transform = IconStyles.Theme(theme, gamma, brightness, whiteStart, whiteRange);

            try
            {
                if (!useCache)
                {
                    var tintedNoCache = IconShader.CreateTransformedCopy(feature.Icon, transform);
                    BlueprintUnitFactUI.SetIcon(feature, tintedNoCache);
                    return;
                }

                var key = MakeKey(feature.Icon, theme, gamma, brightness, whiteStart, whiteRange);

                if (!Cache.TryGetValue(key, out var tinted) || tinted == null)
                {
                    tinted = IconShader.CreateTransformedCopy(feature.Icon, transform);
                    Cache[key] = tinted;
                }

                BlueprintUnitFactUI.SetIcon(feature, tinted);
            }
            catch (Exception ex)
            {
                Main.Mod.Logger.Log("IconTintUtil.ApplyTheme(feature) failed: " + ex);
            }
        }

        private static long MakeKey(
            Sprite src,
            IconTheme theme,
            float gamma,
            float brightness,
            float whiteStart,
            float whiteRange)
        {
            unchecked
            {
                // Start with stable-ish identity for the source sprite
                long h = (long)src.GetInstanceID();
                h = (h * 397) ^ src.texture.GetInstanceID();

                // Mix in theme + params (bitwise exact)
                h = (h * 397) ^ (int)theme;
                h = (h * 397) ^ FloatBits(gamma);
                h = (h * 397) ^ FloatBits(brightness);
                h = (h * 397) ^ FloatBits(whiteStart);
                h = (h * 397) ^ FloatBits(whiteRange);

                return h;
            }
        }
        private static int FloatBits(float f)
        {
            // .NET Framework-friendly: exact bit pattern, no numeric conversion
            return BitConverter.ToInt32(BitConverter.GetBytes(f), 0);
        }
    }
}