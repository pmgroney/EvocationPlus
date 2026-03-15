using System.Collections.Generic;
using EvocationPlus.BlueprintUtils;
using EvocationPlus.Patches.Bloodlines;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Object = UnityEngine.Object;

namespace EvocationPlus.Patches.Spellbooks
{
    internal static class EvokerSpellListInstaller
    {
        internal static BlueprintSpellList EnsureEvocationOnlySpellList(LibraryScriptableObject library)
        {
            var existing = BlueprintLibrary.GetBlueprint(library, BloodlineGuids.EvokerSpellListGuid) as BlueprintSpellList;
            if (existing != null) return existing;

            var sorcBook = BlueprintLibrary.GetBlueprint(library, BloodlineGuids.SorcererSpellbookGuid) as BlueprintSpellbook;
            if (sorcBook == null)
            {
                Main.Mod.Logger.Log("EVP: EvokerSpellList: Sorcerer spellbook not found: " + BloodlineGuids.SorcererSpellbookGuid);
                return null;
            }

            var donorList = sorcBook.SpellList;
            if (donorList == null)
            {
                Main.Mod.Logger.Log("EVP: EvokerSpellList: Sorcerer spellbook has null SpellList.");
                return null;
            }

            var clone = Object.Instantiate(donorList);
            clone.name = "EvocationPlus_EvokerSpellList";
            clone.AssetGuid = BlueprintLibrary.NormalizeGuid(BloodlineGuids.EvokerSpellListGuid);

            // Filter spells by school
            if (clone.SpellsByLevel == null || clone.SpellsByLevel.Length == 0)
            {
                Main.Mod.Logger.Log("EVP: EvokerSpellList: donor list has no SpellsByLevel.");
                return null;
            }

            int removed = 0, kept = 0;

            for (int level = 0; level < clone.SpellsByLevel.Length; level++)
            {
                var lvl = clone.SpellsByLevel[level];
                if (lvl?.Spells == null) continue;

                var newList = new List<BlueprintAbility>(lvl.Spells.Count);

                foreach (var a in lvl.Spells)
                {
                    if (a == null) continue;

                    // Kingmaker uses Kingmaker.Enums.SpellSchool on BlueprintAbility.School
                    if (IsAllowedEvokerSchool(a.School))
                    {
                        newList.Add(a);
                        kept++;
                    }
                    else
                    {
                        removed++;
                    }
                }

                lvl.Spells = newList;
            }

            BlueprintLibrary.Register(library, BloodlineGuids.EvokerSpellListGuid, clone);
            return clone;
        }
        private static bool IsAllowedEvokerSchool(SpellSchool school)
        {
            return school == SpellSchool.Evocation || school == SpellSchool.Conjuration;
        }
    }
}