using System;
using System.Linq;
using HarmonyLib;
using Kingmaker.Blueprints;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Object = UnityEngine.Object;

namespace EvocationPlus.Spells.Implementation
{
    public static class VfxUtilSpawnFx
    {
        public static bool TryCopySpawnFx(BlueprintAbility target, BlueprintAbility donor, out string reason)
        {
            reason = null;

            if (target == null)
            {
                reason = "target null";
                return false;
            }

            if (donor == null)
            {
                reason = "donor null";
                return false;
            }

            var tFx = FindComponentByTypeName(target, "AbilitySpawnFx");
            if (tFx == null)
            {
                reason = "target missing AbilitySpawnFx";
                return false;
            }

            var dFx = FindComponentByTypeName(donor, "AbilitySpawnFx");
            if (dFx == null)
            {
                reason = "donor missing AbilitySpawnFx";
                return false;
            }

            if (!TryGetPrefabAssetId(dFx, out var donorAssetId) || string.IsNullOrEmpty(donorAssetId))
            {
                reason = "donor AbilitySpawnFx prefab AssetId missing/empty";
                return false;
            }

            // Clone target component (avoid mutating shared instance)
            var tFxClone = Object.Instantiate((Object)tFx) as BlueprintComponent;
            if (tFxClone == null)
            {
                reason = "failed to clone target AbilitySpawnFx";
                return false;
            }

            tFxClone.name = $"{tFx.name}_EVP_SpawnFxClone";

            if (!TrySetPrefabAssetId(tFxClone, donorAssetId))
            {
                reason = "failed to set prefab AssetId on cloned AbilitySpawnFx";
                return false;
            }

            ReplaceComponent(target, tFx, tFxClone);
            return true;
        }

        private static BlueprintComponent FindComponentByTypeName(BlueprintScriptableObject bp, string typeName)
        {
            var arr = bp?.ComponentsArray;
            if (arr == null) return null;
            return arr.FirstOrDefault(c => c != null && c.GetType().Name == typeName);
        }

        private static bool TryGetPrefabAssetId(object abilitySpawnFxComponent, out string assetId)
        {
            assetId = null;
            if (abilitySpawnFxComponent == null) return false;

            // AbilitySpawnFx typically has PrefabLink or Prefab
            var prefabField =
                AccessTools.Field(abilitySpawnFxComponent.GetType(), "PrefabLink") ??
                AccessTools.Field(abilitySpawnFxComponent.GetType(), "Prefab");
            if (prefabField == null) return false;

            var link = prefabField.GetValue(abilitySpawnFxComponent);
            if (link == null) return false;

            var assetIdField = AccessTools.Field(link.GetType(), "AssetId");
            if (assetIdField == null) return false;

            assetId = assetIdField.GetValue(link) as string;
            return true;
        }

        private static bool TrySetPrefabAssetId(object abilitySpawnFxComponent, string assetId)
        {
            if (abilitySpawnFxComponent == null) return false;
            if (string.IsNullOrEmpty(assetId)) return false;

            var prefabField =
                AccessTools.Field(abilitySpawnFxComponent.GetType(), "PrefabLink") ??
                AccessTools.Field(abilitySpawnFxComponent.GetType(), "Prefab");
            if (prefabField == null) return false;

            var link = prefabField.GetValue(abilitySpawnFxComponent);
            if (link == null) return false;

            var assetIdField = AccessTools.Field(link.GetType(), "AssetId");
            if (assetIdField == null) return false;

            assetIdField.SetValue(link, assetId);
            return true;
        }

        private static void ReplaceComponent(BlueprintScriptableObject bp, BlueprintComponent oldComp,
            BlueprintComponent newComp)
        {
            var arr = bp.ComponentsArray ?? Array.Empty<BlueprintComponent>();
            for (var i = 0; i < arr.Length; i++)
                if (ReferenceEquals(arr[i], oldComp))
                {
                    arr[i] = newComp;
                    bp.ComponentsArray = arr;
                    return;
                }

            // If not found, append (safe fallback)
            var newArr = new BlueprintComponent[arr.Length + 1];
            Array.Copy(arr, newArr, arr.Length);
            newArr[arr.Length] = newComp;
            bp.ComponentsArray = newArr;
        }
    }
}