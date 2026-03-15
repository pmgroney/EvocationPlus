using EvocationPlus.Core;
using HarmonyLib;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.View.MapObjects;
using UnityEngine;

namespace EvocationPlus.Patches
{
    [HarmonyPatch(typeof(AreaEffectView), "SpawnFxs")]
    public static class EldritchHorrorAreaFxScalePatch
    {
        private static readonly AccessTools.FieldRef<AreaEffectView, GameObject> AreaEffectViewMSpawnedFx =
            AccessTools.FieldRefAccess<AreaEffectView, GameObject>("m_SpawnedFx");

        [HarmonyPostfix]
        public static void Postfix(AreaEffectView __instance)
        {
            if (__instance == null) return;

            var data = __instance.Data as AreaEffectEntityData;
            if (data == null) return;

            var bp = data.Blueprint;
            if (bp == null) return;

            var isEldritchHorror = bp.AssetGuid == Guids.Spells.EldritchHorrorAreaEffectGuid;
            var isHellOnEarth = bp.AssetGuid == Guids.Spells.HellOnEarthAreaEffectGuid;

            if (!isEldritchHorror && !isHellOnEarth)
                return;

            var fx = AreaEffectViewMSpawnedFx(__instance);
            if (fx == null) return;

            if (isEldritchHorror)
            {
                const float scale = 0.45f;
                fx.transform.localScale *= scale;
            }

            var tint = isEldritchHorror
                ? new Color(0.6f, 0.1f, 0.8f, 1f)
                : new Color(0.8f, 0.15f, 0.15f, 1f);

            ApplyTint(fx, tint);
        }

        private static void ApplyTint(GameObject fx, Color tint)
        {
            var particles = fx.GetComponentsInChildren<ParticleSystem>(true);
            foreach (var ps in particles)
            {
                var main = ps.main;
                main.startColor = tint;
            }

            var renderers = fx.GetComponentsInChildren<Renderer>(true);
            foreach (var renderer in renderers)
            {
                foreach (var mat in renderer.materials)
                {
                    if (mat == null) continue;

                    if (mat.HasProperty("_Color"))
                        mat.color = tint;

                    if (mat.HasProperty("_TintColor"))
                        mat.SetColor("_TintColor", tint);
                }
            }
        }
    }
}