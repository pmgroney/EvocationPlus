using System.IO;
using EvocationPlus.BlueprintUtils;
using EvocationPlus.IconUtils;
using HarmonyLib;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Facts;
using Kingmaker.UnitLogic.Abilities.Blueprints;

namespace EvocationPlus.Spells.Infrastructure
{
    internal static class SpellIconApplier
    {
        public static void ApplyAbilityIcons(LibraryScriptableObject library, string modRootPath)
        {
            var all = SpellIconRegistry.GetAllAbilityIcons();
            var iconField = AccessTools.Field(typeof(BlueprintAbility), "m_Icon");
            if (iconField == null)
            {
                Main.Mod.Logger.Log("EvocationPlus: Could not find BlueprintAbility.m_Icon field.");
                return;
            }

            foreach (var kv in all)
            {
                var guid = kv.Key;
                var fullPath = Path.Combine(modRootPath, kv.Value);

                var sprite = IconCreator.LoadSpriteFromPng(fullPath);
                if (sprite == null)
                {
                    Main.Mod.Logger.Log($"EvocationPlus: Icon sprite null for ability {guid} -> {fullPath}");
                    continue;
                }

                var bp = BlueprintLibrary.GetBlueprint(library, guid) as BlueprintAbility;
                if (bp == null)
                {
                    Main.Mod.Logger.Log($"EvocationPlus: BlueprintAbility not found for GUID: {guid}");
                    continue;
                }

                iconField.SetValue(bp, sprite);
            }
        }

        public static void ApplyUnitFactIcons(LibraryScriptableObject library, string modRootPath)
        {
            var all = SpellIconRegistry.GetAllUnitFactIcons();
            var iconField = AccessTools.Field(typeof(BlueprintUnitFact), "m_Icon");
            if (iconField == null)
            {
                Main.Mod.Logger.Log("EvocationPlus: Could not find BlueprintUnitFact.m_Icon field.");
                return;
            }

            foreach (var kv in all)
            {
                var guid = kv.Key;
                var fullPath = Path.Combine(modRootPath, kv.Value);

                var sprite = IconCreator.LoadSpriteFromPng(fullPath);
                if (sprite == null)
                {
                    Main.Mod.Logger.Log($"EvocationPlus: Icon sprite null for unitfact {guid} -> {fullPath}");
                    continue;
                }

                var bp = BlueprintLibrary.GetBlueprint(library, guid) as BlueprintUnitFact;
                if (bp == null)
                {
                    Main.Mod.Logger.Log($"EvocationPlus: BlueprintUnitFact not found for GUID: {guid}");
                    continue;
                }

                iconField.SetValue(bp, sprite);
            }
        }
    }
}