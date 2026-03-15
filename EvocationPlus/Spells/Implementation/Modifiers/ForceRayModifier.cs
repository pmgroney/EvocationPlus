using System;
using EvocationPlus.BlueprintUtils;
using EvocationPlus.Core;
using EvocationPlus.IconUtils;
using EvocationPlus.Patches.Bloodlines;
using EvocationPlus.Spells.Infrastructure;
using EvocationPlus.Utils;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.ElementsSystem;
using Kingmaker.Enums.Damage;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Actions;

namespace EvocationPlus.Spells.Implementation.Modifiers
{
    public sealed class ForceRayModifier : ISpellModifier
    {
        public void Apply(BlueprintAbility spell, LibraryScriptableObject library)
        {
            if (spell == null || library == null)
                return;

            EvocationPlusUnitFactText.SetNameKey(spell, "EVP_FORCE_RAY_NAME");
            EvocationPlusUnitFactText.SetDescriptionKey(spell, "EVP_FORCE_RAY_DESC");

            // Descriptor: remove Fire, add Force
            SpellSchoolUtil.ReplaceDescriptor(spell, SpellDescriptor.Fire, SpellDescriptor.Force);
            // VFX donor
            var vfxSource =
                BlueprintLibrary.GetBlueprint(library, BloodlineGuids.DonorBatteringBlastSpellGuid) as BlueprintAbility;
            if (vfxSource == null)
            {
                Main.Mod.Logger.Log("Air Ray: donor spell not found for VFX (" + Guids.Spells.Enervation + ")");
            }
            else
            {
                if (!VfxUtil.TryCopyProjectileVisualOnly(spell, vfxSource, out var reason))
                    Main.Mod.Logger.Log("Air Ray: VFX copy failed: " + reason);
            }

            var originalRun = spell.GetComponent<AbilityEffectRunAction>();
            if (originalRun?.Actions == null)
            {
                Main.Mod.Logger.Log("ForceRay: AbilityEffectRunAction/Actions missing; skipping damage patch.");
                return;
            }

            // Important: clone the run-action component again so we only mutate a private graph.
            var isolatedRun = BlueprintDeepClone.CloneComponentFully(originalRun);
            ReplaceComponent(spell, originalRun, isolatedRun);

            ActionListUtil.Patch(isolatedRun.Actions, PatchAction);
        }

        private static int PatchAction(GameAction action)
        {
            if (!(action is ContextActionDealDamage deal))
                return 0;

            if (deal.DamageType.Type != DamageType.Energy ||
                deal.DamageType.Energy != DamageEnergyType.Fire)
            {
                return 0;
            }

            deal.DamageType = new DamageTypeDescription
            {
                Type = DamageType.Force
            };

            if (deal.Value != null)
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

        private static void ReplaceComponent(
            BlueprintAbility spell,
            AbilityEffectRunAction oldComponent,
            AbilityEffectRunAction newComponent)
        {
            var components = spell.ComponentsArray;
            for (var i = 0; i < components.Length; i++)
            {
                if (ReferenceEquals(components[i], oldComponent))
                {
                    components[i] = newComponent;
                    spell.ComponentsArray = components;
                    return;
                }
            }

            Main.Mod.Logger.Log("ForceRay: failed to replace AbilityEffectRunAction component.");
        }
    }
}