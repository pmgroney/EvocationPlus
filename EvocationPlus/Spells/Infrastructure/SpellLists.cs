using System;
using System.Collections.Generic;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Object = UnityEngine.Object;

namespace EvocationPlus.Spells.Infrastructure
{
    public static class SpellLists
    {
        public static BlueprintSpellList FindWizardSpellList(LibraryScriptableObject library)
        {
            if (library == null || library.BlueprintsByAssetId == null) return null;

            foreach (var kv in library.BlueprintsByAssetId)
            {
                var bp = kv.Value;
                if (bp == null) continue;

                var list = bp as BlueprintSpellList;
                if (list == null) continue;

                // internal name search: keep it simple for now
                var uo = (Object)list;
                var n = uo.name ?? "";
                if (n.IndexOf("Wizard", StringComparison.OrdinalIgnoreCase) >= 0) return list;
            }

            return null;
        }

        public static bool AddToLevel(BlueprintSpellList list, BlueprintAbility ability, int level, string label)
        {
            if (list == null || ability == null) return false;

            if (list.SpellsByLevel == null || level < 0 || level >= list.SpellsByLevel.Length)
            {
                Main.Mod.Logger.Log("EvocationPlus: " + label + " has no SpellsByLevel for level " + level);
                return false;
            }

            var levelList = list.SpellsByLevel[level];
            if (levelList == null)
            {
                Main.Mod.Logger.Log("EvocationPlus: " + label + " level list is null at level " + level);
                return false;
            }

            if (levelList.Spells != null)
                for (var i = 0; i < levelList.Spells.Count; i++)
                {
                    var existing = levelList.Spells[i];
                    if (existing != null && existing.AssetGuid == ability.AssetGuid)
                        return false; // already present
                }
            else
                levelList.Spells = new List<BlueprintAbility>();

            levelList.Spells.Add(ability);
            return true;
        }
    }
}