using System;
using EvocationPlus.BlueprintUtils;
using EvocationPlus.Core;
using EvocationPlus.IconUtils;
using EvocationPlus.Spells.Implementation;
using EvocationPlus.Utils;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.ElementsSystem;
using Kingmaker.Enums.Damage;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Actions;

namespace EvocationPlus.Patches.Bloodlines
{
    internal static class ForceBeamFeatureBuilder
    {
        internal static BlueprintFeature EnsureForceBeamFeature(LibraryScriptableObject library)
        {
            var existing =
                BlueprintLibrary.GetBlueprint(library, Guids.Spells.ForceBeamFeatureGuid) as BlueprintFeature;
            if (existing != null) return existing;

            var donor =
                BlueprintLibrary.GetBlueprint(library, BloodlineGuids.DonorEarthBlastFeatureGuid) as BlueprintFeature;
            if (donor == null)
            {
                Main.Mod.Logger.Log("EVP: ForceBeam donor feature NOT found (earth blast feature).");
                return null;
            }

            var feat = BlueprintDeepClone.CloneFeatureIsolated(
                donor,
                BlueprintLibrary.NormalizeGuid(Guids.Spells.ForceBeamFeatureGuid));

            feat.name = "EvocationPlus_ForceBeamFeature";

            EvocationPlusUnitFactText.SetNameKey(feat, "EVP_FORCE_BEAM_NAME");
            EvocationPlusUnitFactText.SetDescriptionKey(feat, "EVP_FORCE_BEAM_DESC");

            var normDonorAbility = BlueprintLibrary.NormalizeGuid(BloodlineGuids.DonorEarthBlastAbilityGuid);
            var replaced = false;

            foreach (var af in feat.GetComponents<AddFacts>())
            {
                if (af?.Facts == null) continue;

                for (var i = 0; i < af.Facts.Length; i++)
                {
                    var ability = af.Facts[i] as BlueprintAbility;
                    if (ability == null) continue;
                    if (ability.AssetGuid != normDonorAbility) continue;

                    var existingAbility =
                        BlueprintLibrary.GetBlueprint(library, Guids.Spells.ForceBeamAbilityGuid) as BlueprintAbility;

                    var newAbility = existingAbility ?? BlueprintDeepClone.CloneAbilityIsolated(
                        ability,
                        BlueprintLibrary.NormalizeGuid(Guids.Spells.ForceBeamAbilityGuid));

                    newAbility.name = "EvocationPlus_ForceBeamAbility";

                    EvocationPlusUnitFactText.SetNameKey(newAbility, "EVP_FORCE_BEAM_NAME");
                    EvocationPlusUnitFactText.SetDescriptionKey(newAbility, "EVP_FORCE_BEAM_DESC");
                    ColorIcon(newAbility);
                    BlueprintUnitFactUI.SetIcon(feat, newAbility.Icon);

                    PatchAbilityDamageToForce(newAbility, true);

                    var vfxSource = BlueprintLibrary.GetBlueprint(library, Guids.Spells.RayOfFrost) as BlueprintAbility;
                    if (vfxSource == null)
                    {
                        Main.Mod.Logger.Log("Force Beam: donor spell not found for VFX (" + Guids.Spells.Enervation + ")");
                    }
                    else
                    {
                        if (!VfxUtil.TryCopyProjectileVisualOnly(newAbility, vfxSource, out var reason, true))
                            Main.Mod.Logger.Log("Force Beam: VFX copy failed: " + reason);
                    }

                    if (existingAbility == null)
                    {
                        BlueprintLibrary.Register(library, Guids.Spells.ForceBeamAbilityGuid, newAbility);
                    }

                    af.Facts[i] = newAbility;
                    replaced = true;
                    break;
                }

                if (replaced) break;
            }

            if (!replaced)
            {
                Main.Mod.Logger.Log(
                    "EVP: ForceBeam donor feature found, but donor ability was NOT present on AddFacts.");
            }

            BlueprintLibrary.Register(library, Guids.Spells.ForceBeamFeatureGuid, feat);
            return feat;
        }

        private static void ColorIcon(BlueprintAbility spell)
        {
            var transform = IconStyles.LightningWhite(
                1.15f,
                1.15f,
                0.75f,
                0.15f
            );

            var tinted = IconShader.CreateTransformedCopy(spell.Icon, transform);
            BlueprintUnitFactUI.SetIcon(spell, tinted);
        }

        private static void PatchAbilityDamageToForce(BlueprintAbility ability, bool tweakDice = false)
        {
            var originalRun = ability.GetComponent<AbilityEffectRunAction>();
            if (originalRun?.Actions == null)
            {
                Main.Mod.Logger.Log("EVP: ForceBeam AbilityEffectRunAction/Actions missing; skipping damage patch.");
                return;
            }

            // Re-isolate before mutation, just to be safe.
            var isolatedRun = BlueprintDeepClone.CloneComponentFully(originalRun);
            ReplaceRunActionComponent(ability, originalRun, isolatedRun);

            ActionListUtil.Patch(isolatedRun.Actions, action => PatchAction(action, tweakDice));
        }

        private static int PatchAction(GameAction action, bool tweakDice)
        {
            var deal = action as ContextActionDealDamage;
            if (deal == null)
                return 0;

            // Only convert the intended donor damage payload.
            if (deal.DamageType.Type != DamageType.Energy ||
                deal.DamageType.Energy != DamageEnergyType.Electricity)
            {
                return 0;
            }

            deal.DamageType = new DamageTypeDescription
            {
                Type = DamageType.Force
            };

            if (tweakDice && deal.Value != null)
            {
                var clonedValue = CloneDiceValueWithReducedDie(deal.Value);
                if (clonedValue != null)
                {
                    deal.Value = clonedValue;
                }
            }

            return 1;
        }
        private static ContextDiceValue CloneDiceValueWithReducedDie(ContextDiceValue src)
        {
            if (src == null) return null;

            var clone = new ContextDiceValue
            {
                DiceType = ReduceDiceType(src.DiceType),
                DiceCountValue = src.DiceCountValue,
                BonusValue = src.BonusValue
            };

            return clone;
        }

        private static DiceType ReduceDiceType(DiceType diceType)
        {
            if (diceType == DiceType.D6) return DiceType.D4;
            if (diceType == DiceType.D8) return DiceType.D6;
            return diceType;
        }
        private static void ReplaceRunActionComponent(
            BlueprintAbility ability,
            AbilityEffectRunAction oldComponent,
            AbilityEffectRunAction newComponent)
        {
            var components = ability.ComponentsArray;
            for (var i = 0; i < components.Length; i++)
            {
                if (ReferenceEquals(components[i], oldComponent))
                {
                    components[i] = newComponent;
                    ability.ComponentsArray = components;
                    return;
                }
            }

            Main.Mod.Logger.Log("EVP: ForceBeam failed to replace AbilityEffectRunAction component.");
        }
    }
}