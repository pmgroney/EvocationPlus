using EvocationPlus.BlueprintUtils;
using EvocationPlus.Patches.Bloodlines;
using HarmonyLib;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Selection;

namespace EvocationPlus.Patches
{
    [HarmonyPatch(typeof(LibraryScriptableObject), nameof(LibraryScriptableObject.LoadDictionary))]
    internal static class LibraryScriptableObject_LoadDictionary_Patch
    {
        internal static LibraryScriptableObject Library;
        private static bool _installed;

        private static void Postfix(LibraryScriptableObject __instance)
        {
            // Always capture
            Library = __instance;

            // Install only once, only when ready
            if (_installed) return;
            if (__instance?.BlueprintsByAssetId == null || __instance.BlueprintsByAssetId.Count == 0) return;
            if (__instance.ResourceNamesByAssetId == null) return;

            _installed = true;

            new EvocationPlus.Content.Spells.SpellModule().Install(__instance);
            new EvocationPlus.Content.Archetypes.ArchetypeModule().Install(__instance);

            
            // // Dump to get guids for full progression of abilities and spells
            // var prog = ResourcesLibrary.TryGetBlueprint<BlueprintProgression>(BloodlineGuids.NecromancerBloodlineSelectionGuid);
            // BlueprintDumper.DumpProgressionDetailed(prog, "necro_prog_detailed.txt");
            
            // -----------------------------
            // KEEP: quick sanity / debugging
            // -----------------------------
            // BlueprintDumper.DumpCommonFinders("Sorcerer", "sorcerer");
            // BlueprintDumper.DumpCommonFinders("Necromancer", "necromancer");
            // BlueprintDumper.DumpCommonFinders("Evoker", "evoker");

            // ---------------------------------------------------------
            // KEEP (toggle): dump ALL evocation spells (big but useful)
            // ---------------------------------------------------------
            // var evocationSpells = BlueprintLibrary
            //     .GetAllBlueprints<BlueprintAbility>()
            //     .Where(a => a != null && a.IsSpell && a.School == SpellSchool.Evocation)
            //     .OrderBy(a => a.name)
            //     .Select(a => $"{a.AssetGuid} | {a.name}");
            //
            // BlueprintDumper.WriteLinesToFile(
            //     "Evocation Spells (IsSpell && School == Evocation)",
            //     evocationSpells,
            //     "evocation_spells.txt"
            // );

            // -------------------------------------------------------------------
            // KEEP (toggle): dump full donor progression structure (best for swaps)
            // -------------------------------------------------------------------
            //var donorFire = BlueprintLibrary.GetBlueprint(__instance, BloodlineGuids.DonorArcaneProgressionGuid) as BlueprintProgression;
            //BlueprintDumper.DumpFullProgressionStructure(donorFire, "donor_arcane_full.txt");

        }
    }
}