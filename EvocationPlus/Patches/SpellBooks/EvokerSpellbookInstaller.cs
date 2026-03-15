using EvocationPlus.BlueprintUtils;
using EvocationPlus.Patches.Bloodlines;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes.Spells;
using UnityEngine;
using Object = UnityEngine.Object;

namespace EvocationPlus.Patches.Spellbooks
{
    internal static class EvokerSpellbookInstaller
    {
        internal static BlueprintSpellbook EnsureEvokerSpellbook(LibraryScriptableObject library)
        {
            var existing = BlueprintLibrary.GetBlueprint(library, BloodlineGuids.EvokerSpellbookGuid) as BlueprintSpellbook;
            if (existing != null) return existing;

            var sorcBook = BlueprintLibrary.GetBlueprint(library, BloodlineGuids.SorcererSpellbookGuid) as BlueprintSpellbook;
            if (sorcBook == null)
            {
                Main.Mod.Logger.Log("EVP: EvokerSpellbook: Sorcerer spellbook not found: " + BloodlineGuids.SorcererSpellbookGuid);
                return null;
            }

            var evokerList = EvokerSpellListInstaller.EnsureEvocationOnlySpellList(library);
            if (evokerList == null)
            {
                Main.Mod.Logger.Log("EVP: EvokerSpellbook: Evoker spell list could not be created.");
                return null;
            }

            var clone = Object.Instantiate(sorcBook);
            clone.name = "EvocationPlus_EvokerSpellbook";
            clone.AssetGuid = BlueprintLibrary.NormalizeGuid(BloodlineGuids.EvokerSpellbookGuid);

            // Point to evocation-only list
            clone.SpellList = evokerList;

            BlueprintLibrary.Register(library, BloodlineGuids.EvokerSpellbookGuid, clone);
            return clone;
        }
    }
}