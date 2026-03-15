using System;
using EvocationPlus.BlueprintUtils;
using EvocationPlus.Core;
using EvocationPlus.Utils;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Enums.Damage;
using Kingmaker.ElementsSystem;
using Kingmaker.Enums;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Components;

namespace EvocationPlus.Patches.Bloodlines
{
    internal static class NecroRayFeatureBuilder
    {
        internal static BlueprintFeature EnsureNecroRayFeature(LibraryScriptableObject library)
        {
            var existing = BlueprintLibrary.GetBlueprint(library, BloodlineGuids.NewNecroRayFeatureGuid) as BlueprintFeature;
            if (existing != null) return existing;

            var donor = BlueprintLibrary.GetBlueprint(library, BloodlineGuids.DonorEarthBlastFeatureGuid) as BlueprintFeature;
            if (donor == null)
            {
                Main.Mod.Logger.Log("EVP: donor earth blast feature not found (base game).");
                return null;
            }

            var necroFeat = BlueprintDeepClone.CloneFeatureIsolated(
                donor,
                BlueprintLibrary.NormalizeGuid(BloodlineGuids.NewNecroRayFeatureGuid));

            necroFeat.name = "EvocationPlus_NecromancyRayFeature";

            var normDonorAbility = BlueprintLibrary.NormalizeGuid(BloodlineGuids.DonorEarthBlastAbilityGuid);
            var replaced = false;

            foreach (var af in necroFeat.GetComponents<AddFacts>())
            {
                if (af == null || af.Facts == null) continue;

                for (var i = 0; i < af.Facts.Length; i++)
                {
                    var ability = af.Facts[i] as BlueprintAbility;
                    if (ability == null) continue;
                    if (ability.AssetGuid != normDonorAbility) continue;

                    var existingAbility = BlueprintLibrary.GetBlueprint(library, BloodlineGuids.NewNecroRayAbilityGuid) as BlueprintAbility;
                    var newAbility = existingAbility ?? BlueprintDeepClone.CloneAbilityIsolated(
                        ability,
                        BlueprintLibrary.NormalizeGuid(BloodlineGuids.NewNecroRayAbilityGuid));

                    newAbility.name = "EvocationPlus_NecromancyBlastAbility";

                    EvocationPlusUnitFactText.SetNameKey(newAbility, "EVP_WitheringRay_Name");
                    EvocationPlusUnitFactText.SetDescriptionKey(newAbility, "EVP_WitheringRay_Desc");

                    PatchAbilityDamageToNegativeEnergy(newAbility);
                    var sorcererClass = BlueprintLibrary.GetBlueprint(library, Guids.Features.SorcererClassGuid) as BlueprintCharacterClass;
                    if (sorcererClass != null)
                    {
                        try
                        {
                            PatchAbilityScalingToDicePlusPerDie(newAbility, sorcererClass);
                        }
                        catch (Exception ex)
                        {
                            Main.Mod.Logger.Log("EVP: Necro Ray scaling patch failed: " + ex);
                        }
                    }
                    else
                    {
                        Main.Mod.Logger.Log("EVP: Sorcerer class not found; skipping Necro Ray scaling.");
                    }
                    if (existingAbility == null)
                        BlueprintLibrary.Register(library, BloodlineGuids.NewNecroRayAbilityGuid, newAbility);

                    af.Facts[i] = newAbility;
                    replaced = true;
                    break;
                }

                if (replaced) break;
            }

            EvocationPlusUnitFactText.SetNameKey(necroFeat, "EVP_WitheringRay_Name");
            EvocationPlusUnitFactText.SetDescriptionKey(necroFeat, "EVP_WitheringRay_Desc");

            if (!replaced)
                Main.Mod.Logger.Log("EVP: donor earth blast feature found, but expected blast ability not present on AddFacts.");

            BlueprintLibrary.Register(library, BloodlineGuids.NewNecroRayFeatureGuid, necroFeat);
            return necroFeat;
        }

        private static void PatchAbilityDamageToNegativeEnergy(BlueprintAbility ability)
        {
            var runAction = ability.GetComponent<AbilityEffectRunAction>();
            var actions = runAction != null && runAction.Actions != null ? runAction.Actions.Actions : null;
            if (actions == null) return;

            for (var i = 0; i < actions.Length; i++)
                PatchAction(actions[i]);
        }

        private static void PatchAction(GameAction action)
        {
            if (action == null) return;

            var deal = action as ContextActionDealDamage;
            if (deal != null)
            {
                deal.DamageType = new DamageTypeDescription
                {
                    Type = DamageType.Energy,
                    Energy = DamageEnergyType.NegativeEnergy
                };

                // Nd6 + N where N = Rank(Default)
                if (deal.Value == null) deal.Value = new ContextDiceValue();

                deal.Value.DiceType = DiceType.D6;

                deal.Value.DiceCountValue = new ContextValue
                {
                    ValueType = ContextValueType.Rank,
                    ValueRank = AbilityRankType.Default
                };

                deal.Value.BonusValue = new ContextValue
                {
                    ValueType = ContextValueType.Rank,
                    ValueRank = AbilityRankType.Default
                };

                return;
            }

            var saved = action as ContextActionConditionalSaved;
            if (saved == null) return;

            var succ = saved.Succeed != null ? saved.Succeed.Actions : null;
            if (succ != null)
                for (var i = 0; i < succ.Length; i++)
                    PatchAction(succ[i]);

            var fail = saved.Failed != null ? saved.Failed.Actions : null;
            if (fail != null)
                for (var i = 0; i < fail.Length; i++)
                    PatchAction(fail[i]);
        }
        private static void PatchAbilityScalingToDicePlusPerDie(
            BlueprintAbility ability,
            BlueprintCharacterClass sorcererClass)
        {
            if (ability == null || sorcererClass == null) return;

            var rank = ability.GetComponent<ContextRankConfig>();
            if (rank == null) rank = ability.AddComponent<ContextRankConfig>();

            // These field names come directly from your decompile
            ReflectionUtils.SetFieldAny(rank, new[] { "m_Type" }, AbilityRankType.Default);
            ReflectionUtils.SetFieldAny(rank, new[] { "m_BaseValueType" }, ContextRankBaseValueType.ClassLevel);
            ReflectionUtils.SetFieldAny(rank, new[] { "m_Class" }, new[] { sorcererClass });

            // 1 + floor((level - 1)/2) => StartPlusDivStep with StartLevel=1, StepLevel=2
            ReflectionUtils.SetFieldAny(rank, new[] { "m_Progression" }, ContextRankProgression.StartPlusDivStep);
            ReflectionUtils.SetFieldAny(rank, new[] { "m_StartLevel" }, 1);
            ReflectionUtils.SetFieldAny(rank, new[] { "m_StepLevel" }, 2);

            ReflectionUtils.SetFieldAny(rank, new[] { "m_UseMin" }, true);
            ReflectionUtils.SetFieldAny(rank, new[] { "m_Min" }, 1);
        }
    }
}