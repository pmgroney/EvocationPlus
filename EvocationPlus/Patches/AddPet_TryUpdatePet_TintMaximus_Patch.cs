using System;
using System.Collections.Generic;
using EvocationPlus.Core;
using HarmonyLib;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.FactLogic;
using UnityEngine;

namespace EvocationPlus.Patches
{
    [HarmonyPatch(typeof(AddPet), nameof(AddPet.TryUpdatePet))]
    internal static class AddPet_TryUpdatePet_TintMaximus_Patch
    {
        private static readonly string LeopardUnitGuid = Guids.BlueprintGuids.MaximusLeopardUnitGuid;
        private static readonly Dictionary<int, Texture2D> _maximusLeopardTexBySource = new Dictionary<int, Texture2D>();
        // Adjust this if you want slightly darker/lighter silver
        private static readonly Color TargetTint = new Color(0.90f, 0.92f, 0.95f, 1f);
        private static readonly Dictionary<string, int> PatchedPetViewInstance = new Dictionary<string, int>();

        private static readonly HashSet<string> PatchedPetIds = new HashSet<string>();

        static void Postfix(AddPet __instance)
        {
            var pet = __instance?.SpawnedPet;
            if (pet == null) return;

            var bp = pet.Blueprint;
            if (bp == null) return;
            if (!string.Equals(bp.AssetGuid.ToString(), LeopardUnitGuid, StringComparison.OrdinalIgnoreCase))
                return;

            var id = pet.UniqueId;
            if (string.IsNullOrEmpty(id)) return;

            var view = pet.View;
            if (view == null) return;

            var viewId = view.GetInstanceID();
            if (PatchedPetViewInstance.TryGetValue(id, out var lastViewId) && lastViewId == viewId)
                return; // already applied to this specific view instance

            PatchedPetViewInstance[id] = viewId;

            ApplyTextureTint(pet, TargetTint);
        }

        private static void ApplyTextureTint(UnitEntityData pet, Color tint)
        {
            var view = pet.View;
            if (view == null) return;

            var go = view.gameObject;
            if (go == null) return;

            var renderers = go.GetComponentsInChildren<Renderer>(true);
            if (renderers == null) return;

            foreach (var r in renderers)
            {
                if (r == null) continue;

                var mats = r.materials;
                if (mats == null) continue;

                foreach (var mat in mats)
                {
                    if (mat == null) continue;
                    if (!mat.HasProperty("_MainTex")) continue;

                    TryApplyRecoloredTexture(mat, tint);
                }
            }
        }

        private static void TryApplyRecoloredTexture(Material mat, Color tint)
        {
            var srcTex = mat.GetTexture("_MainTex") as Texture2D;
            if (srcTex == null) return;

            // Cache per source texture so we don't accidentally reuse a recolor built from a different texture.
            var key = srcTex.GetInstanceID();

            if (!_maximusLeopardTexBySource.TryGetValue(key, out var tinted) || tinted == null)
            {
                tinted = MakeReadableCopy(srcTex);
                if (tinted == null) return;

                RecolorToSilverGray(tinted, tint);

                tinted.name = srcTex.name + "_maximus";
                tinted.wrapMode = srcTex.wrapMode;
                tinted.filterMode = srcTex.filterMode;
                tinted.anisoLevel = srcTex.anisoLevel;

                _maximusLeopardTexBySource[key] = tinted;
            }

            mat.SetTexture("_MainTex", tinted);
        }

        private static Texture2D MakeReadableCopy(Texture2D src)
        {
            var rt = RenderTexture.GetTemporary(src.width, src.height, 0,
                RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);

            try
            {
                Graphics.Blit(src, rt);
                var prev = RenderTexture.active;
                RenderTexture.active = rt;

                var readable = new Texture2D(src.width, src.height,
                    TextureFormat.RGBA32, false, true);

                readable.ReadPixels(new Rect(0, 0, src.width, src.height), 0, 0);
                readable.Apply(false, false);

                RenderTexture.active = prev;
                return readable;
            }
            finally
            {
                RenderTexture.ReleaseTemporary(rt);
            }
        }

        private static void RecolorToSilverGray(Texture2D tex, Color tint)
        {
            const float grayAmount = 0.90f;
            const float tintAmount = 0.70f;

            var pixels = tex.GetPixels32();
            for (int i = 0; i < pixels.Length; i++)
            {
                var p = pixels[i];

                float r = p.r / 255f;
                float g = p.g / 255f;
                float b = p.b / 255f;

                float lum = (0.2126f * r) + (0.7152f * g) + (0.0722f * b);

                float gr = Mathf.Lerp(r, lum, grayAmount);
                float gg = Mathf.Lerp(g, lum, grayAmount);
                float gb = Mathf.Lerp(b, lum, grayAmount);

                float tr = Mathf.Lerp(gr, gr * tint.r, tintAmount);
                float tg = Mathf.Lerp(gg, gg * tint.g, tintAmount);
                float tb = Mathf.Lerp(gb, gb * tint.b, tintAmount);

                p.r = (byte)Mathf.Clamp(Mathf.RoundToInt(tr * 255f), 0, 255);
                p.g = (byte)Mathf.Clamp(Mathf.RoundToInt(tg * 255f), 0, 255);
                p.b = (byte)Mathf.Clamp(Mathf.RoundToInt(tb * 255f), 0, 255);

                pixels[i] = p;
            }

            tex.SetPixels32(pixels);
            tex.Apply(false, false);
        }
    }
}