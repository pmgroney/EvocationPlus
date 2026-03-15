using System.Linq;
using EvocationPlus.BlueprintUtils;
using EvocationPlus.Core;
using EvocationPlus.Patches;
using EvocationPlus.Spells.Infrastructure;
using EvocationPlus.Utils;
using HarmonyLib;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.ElementsSystem;
using Kingmaker.Enums;
using Kingmaker.RuleSystem;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Abilities.Components.AreaEffects;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.UnitLogic.Mechanics.Components;

namespace EvocationPlus.Spells.Implementation.Modifiers
{
    public sealed class HellOnEarthModifier : ISpellModifier
    {
        private static readonly System.Reflection.FieldInfo ContextRankConfigMType =
            AccessTools.Field(typeof(ContextRankConfig), "m_Type");
        private static readonly System.Reflection.FieldInfo ContextRankConfigMBaseValueType =
            AccessTools.Field(typeof(ContextRankConfig), "m_BaseValueType");
        private static readonly System.Reflection.FieldInfo ContextRankConfigMProgression =
            AccessTools.Field(typeof(ContextRankConfig), "m_Progression");
        private static readonly System.Reflection.FieldInfo ContextRankConfigMStartLevel =
            AccessTools.Field(typeof(ContextRankConfig), "m_StartLevel");
        private static readonly System.Reflection.FieldInfo ContextRankConfigMStepLevel =
            AccessTools.Field(typeof(ContextRankConfig), "m_StepLevel");

        public void Apply(BlueprintAbility spell, LibraryScriptableObject library)
        {
            if (spell == null || library == null) return;

            EvocationPlusUnitFactText.SetNameKey(spell, "EVP_HELL_ON_EARTH_NAME");
            EvocationPlusUnitFactText.SetDescriptionKey(spell, "EVP_HELL_ON_EARTH_DESC");

            var sc = spell.GetComponent<SpellComponent>();
            if (sc != null)
                sc.School = SpellSchool.Necromancy;

            if (!TryFindSpawnAreaEffect(spell, out var spawn, out var reason))
            {
                Main.Mod.Logger.Log("HellOnEarth: " + reason);
                return;
            }

            // IMPORTANT: never mutate shared EntangleArea directly
            var clonedAreaGuid = Guids.Spells.HellOnEarthAreaEffectGuid;
            var clonedArea = BlueprintLibrary.GetBlueprint(library, clonedAreaGuid) as BlueprintAbilityAreaEffect;

            if (clonedArea == null)
            {
                clonedArea = CloneAreaEffectIsolated(
                    spawn.AreaEffect,
                    clonedAreaGuid);

                if (clonedArea == null)
                {
                    Main.Mod.Logger.Log("HellOnEarth: failed to clone area effect.");
                    return;
                }

                BlueprintLibrary.Register(library, clonedAreaGuid, clonedArea);
            }

            spawn.AreaEffect = clonedArea;
            spawn.OnUnit = false;

            spawn.DurationValue = new ContextDurationValue
            {
                Rate = DurationRate.Rounds,
                DiceType = DiceType.D12,
                DiceCountValue = new ContextValue { ValueType = ContextValueType.Simple, Value = 0 },
                BonusValue = new ContextValue { ValueType = ContextValueType.Simple, Value = 10 }
            };

            EnsureHellOnEarthRank(clonedArea);
            PatchHellOnEarthDamage(clonedArea);
            
        }

        private static void PatchHellOnEarthDamage(BlueprintAbilityAreaEffect areaEffect)
        {
            if (areaEffect == null) return;

            var areaRun = areaEffect.GetComponent<AbilityAreaEffectRunAction>();
            if (areaRun == null)
            {
                Main.Mod.Logger.Log("HellOnEarth: AbilityAreaEffectRunAction missing on AreaEffect.");
                return;
            }

            // First ensure there IS a damage action to patch.
            EnsureAreaDealsBaseDamagePatch.EnsureAreaDealsBaseDamage(areaRun);

            var patched = 0;

            ActionListUtil.Patch(areaRun.UnitEnter, action => PatchDamageAction(action, ref patched));
            ActionListUtil.Patch(areaRun.Round, action => PatchDamageAction(action, ref patched));
            ActionListUtil.Patch(areaRun.UnitExit, action => PatchDamageAction(action, ref patched));

            Main.Mod.Logger.Log("HellOnEarth: patched damage actions = " + patched);
        }

        private static int PatchDamageAction(GameAction action, ref int patched)
        {
            var deal = action as ContextActionDealDamage;
            if (deal == null)
                return 0;

            deal.Value = new ContextDiceValue
            {
                DiceType = DiceType.D12,
                DiceCountValue = new ContextValue
                {
                    ValueType = ContextValueType.Rank,
                    ValueRank = AbilityRankType.Default
                },
                BonusValue = new ContextValue
                {
                    ValueType = ContextValueType.Simple,
                    Value = 0
                }
            };

            patched++;
            return 1;
        }
        private static bool TryFindSpawnAreaEffect(
            BlueprintAbility spell,
            out ContextActionSpawnAreaEffect spawn,
            out string reason)
        {
            spawn = null;
            reason = null;

            var effectRun = spell.GetComponent<AbilityEffectRunAction>();
            var actions = effectRun?.Actions;
            if (actions == null)
            {
                reason = "AbilityEffectRunAction / Actions missing";
                return false;
            }

            ContextActionSpawnAreaEffect found = null;

            ActionListUtil.Patch(actions, action =>
            {
                if (found == null && action is ContextActionSpawnAreaEffect s && s.AreaEffect != null)
                    found = s;

                return 0;
            });

            if (found != null)
            {
                spawn = found;
                return true;
            }

            reason = "No ContextActionSpawnAreaEffect with AreaEffect found (recursive search).";
            return false;
        }

        private static void EnsureHellOnEarthRank(BlueprintScriptableObject bpOwner)
        {
            var rank = bpOwner.GetComponent<ContextRankConfig>();
            if (rank == null)
                rank = bpOwner.AddComponent<ContextRankConfig>();

            if (ContextRankConfigMType == null ||
                ContextRankConfigMBaseValueType == null ||
                ContextRankConfigMProgression == null ||
                ContextRankConfigMStartLevel == null ||
                ContextRankConfigMStepLevel == null)
            {
                Main.Mod.Logger.Log("HellOnEarth: ContextRankConfig field layout mismatch; rank not configured.");
                return;
            }

            ContextRankConfigMType.SetValue(rank, AbilityRankType.Default);
            ContextRankConfigMBaseValueType.SetValue(rank, ContextRankBaseValueType.CasterLevel);
            ContextRankConfigMProgression.SetValue(rank, ContextRankProgression.AsIs);
            ContextRankConfigMStartLevel.SetValue(rank, 0);
            ContextRankConfigMStepLevel.SetValue(rank, 0);
        }
        private static BlueprintAbilityAreaEffect CloneAreaEffectIsolated(
            BlueprintAbilityAreaEffect src,
            string newGuid)
        {
            if (src == null) return null;

            var clone = UnityEngine.Object.Instantiate(src);
            clone.name = src.name + "_EvocationPlus_HellOnEarth";
            clone.AssetGuid = BlueprintLibrary.NormalizeGuid(newGuid);
            clone.ComponentsArray = (src.ComponentsArray ?? System.Array.Empty<BlueprintComponent>())
                .Select(BlueprintDeepClone.CloneComponentFully)
                .ToArray();

            return clone;
        }
    }
}