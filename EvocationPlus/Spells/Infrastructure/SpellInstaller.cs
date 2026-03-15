using System.Collections.Generic;
using EvocationPlus.BlueprintUtils;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.UnitLogic.Abilities.Blueprints;

namespace EvocationPlus.Spells.Infrastructure
{
    public static class SpellInstaller
    {
        public static void InstallAll(LibraryScriptableObject library, List<SpellDefinition> defs)
        {
            if (library == null || library.BlueprintsByAssetId == null)
            {
                Main.Mod.Logger.Log("EvocationPlus: library not ready.");
                return;
            }

            if (defs == null || defs.Count == 0)
            {
                Main.Mod.Logger.Log("EvocationPlus: no spells configured.");
                return;
            }

            var arcaneList = SpellLists.FindWizardSpellList(library);
            if (arcaneList == null)
            {
                Main.Mod.Logger.Log("EvocationPlus: could not find arcane (wizard) spell list.");
                return;
            }

            for (var i = 0; i < defs.Count; i++)
                InstallOne(library, arcaneList, defs[i]);
        }

        private static void InstallOne(
            LibraryScriptableObject library,
            BlueprintSpellList arcaneList,
            SpellDefinition def)
        {
            if (def == null) return;

            var baseBp = BlueprintLibrary.GetBlueprint(library, def.BaseSpellAssetId) as BlueprintAbility;
            if (baseBp == null)
            {
                Main.Mod.Logger.Log("EvocationPlus: base spell not found (id=" + def.BaseSpellAssetId + ") for " +
                                    def.InternalName);
                return;
            }

            var newBp = BlueprintLibrary.GetBlueprint(library, def.NewSpellAssetId) as BlueprintAbility;
            if (newBp == null)
            {
                newBp = CloneAbility(baseBp, def.NewSpellAssetId, def.InternalName);
                BlueprintLibrary.Register(library, def.NewSpellAssetId, newBp);
            }

            if (def.Modifier != null)
                def.Modifier.Apply(newBp, library);

            SpellLists.AddToLevel(arcaneList, newBp, def.SpellLevel, "Arcane(Wizard)");
        }

        private static BlueprintAbility CloneAbility(BlueprintAbility baseSpell, string newAssetId, string internalName)
        {
            var normalizedGuid = BlueprintLibrary.NormalizeGuid(newAssetId);

            var clone = BlueprintDeepClone.CloneAbilityIsolated(baseSpell, normalizedGuid);
            clone.name = internalName;

            // Needed for Scorching Ray–style scaling rays / cached setup
            clone.OnEnable();

            return clone;
        }
    }
}