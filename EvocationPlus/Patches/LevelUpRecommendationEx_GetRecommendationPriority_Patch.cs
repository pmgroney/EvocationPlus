using System;
using System.Linq;
using HarmonyLib;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Class.LevelUp;
using Kingmaker.UnitLogic.FactLogic;

namespace EvocationPlus.Patches.UI
{
    [HarmonyPatch(typeof(LevelUpRecommendationEx), nameof(LevelUpRecommendationEx.GetRecommendationPriority))]
    internal static class LevelUpRecommendationEx_GetRecommendationPriority_Patch
    {
        private static void Postfix(
            BlueprintScriptableObject blueprint,
            LevelUpState levelUpState,
            ref int __result)
        {
            if (blueprint == null || levelUpState == null) return;

            var spell = blueprint as BlueprintAbility;
            if (spell == null) return;
            if (IsAutoGrantedByProgression(levelUpState, spell))
                __result = -1;
        }

        private static bool IsAutoGrantedByProgression(LevelUpState state, BlueprintAbility spell)
        {
            var selectedClass = state.SelectedClass ?? state.Unit?.Progression?.GetMaxClass();
            if (selectedClass == null) return false;

            var unit = state.Unit;
            if (unit == null || unit.Progression == null || unit.Progression.Features == null) return false;

            foreach (var fact in unit.Progression.Features)
            {
                if (fact == null) continue;

                var prog = fact.Blueprint as BlueprintProgression;
                if (prog == null || prog.LevelEntries == null) continue;

                // Only progressions that apply to the class currently being leveled
                if (prog.Classes == null || !prog.Classes.Contains(selectedClass))
                    continue;

                // Scan ALL levels of the progression (this is the key change)
                foreach (var entry in prog.LevelEntries)
                {
                    if (entry == null || entry.Features == null) continue;

                    foreach (var bf in entry.Features)
                    {
                        var feature = bf as BlueprintFeature;
                        if (feature == null) continue;

                        var adds = feature.GetComponents<AddKnownSpell>();
                        if (adds == null) continue;

                        var addsList = adds as AddKnownSpell[] ?? adds.ToArray();
                        for (int i = 0; i < addsList.Length; i++)
                        {
                            var a = addsList[i];
                            if (a == null) continue;

                            if (a.CharacterClass == selectedClass && a.Spell == spell)
                                return true;
                        }
                    }
                }
            }

            return false;
        }
    }
}