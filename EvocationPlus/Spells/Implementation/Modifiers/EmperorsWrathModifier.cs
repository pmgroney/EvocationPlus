using System;
using EvocationPlus.BlueprintUtils;
using EvocationPlus.Core;
using EvocationPlus.Spells.Infrastructure;
using EvocationPlus.Utils;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.ElementsSystem;
using Kingmaker.Enums.Damage;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Object = UnityEngine.Object;

namespace EvocationPlus.Spells.Implementation.Modifiers
{
    public sealed class EmperorsWrathModifier : ISpellModifier
    {
        
        public void Apply(BlueprintAbility spell, LibraryScriptableObject library)
        {
            if (spell == null) return;
            EvocationPlusUnitFactText.SetNameKey(spell, "EVP_EmperorsWrath_NAME");
            EvocationPlusUnitFactText.SetDescriptionKey(spell, "EVP_EmperorsWrath_DESC");
            var sdcOld = spell.GetComponent<SpellDescriptorComponent>();
            if (sdcOld != null)
            {
                var sdcNew = Object.Instantiate(sdcOld);
                ReplaceComponentUtil.ReplaceComponent(spell, sdcOld, sdcNew);

                sdcNew.Descriptor &= ~SpellDescriptor.Fire;
                sdcNew.Descriptor |= SpellDescriptor.Electricity;
            }

            var vfxSource = BlueprintLibrary.GetBlueprint(library, Guids.Spells.ElectricVfxSpell) as BlueprintAbility;
            VfxUtil.TryCopyProjectileVisualOnly(spell, vfxSource, out var reason, true);

            var runAction = spell.GetComponent<AbilityEffectRunAction>();
            if (runAction?.Actions == null)
            {
                Main.Mod.Logger.Log("EmperorsWrath: AbilityEffectRunAction/Actions missing; skipping damage patch.");
                return; // still last step
            }

            // Central recursive walker handles ConditionalSaved + other wrappers
            ActionListUtil.Patch(runAction.Actions, PatchAction);
        }

        private int PatchAction(GameAction action)
        {
            if (!(action is ContextActionDealDamage deal)) return 0;
            // Only convert FIRE damage to Electricity (avoid mutating other damage riders).
            if (deal.DamageType.Type != DamageType.Energy ||
                deal.DamageType.Energy != DamageEnergyType.Fire)
            {
                return 0;
            }

            var dt = (DamageTypeDescription)Activator.CreateInstance(typeof(DamageTypeDescription));
            dt.Type = DamageType.Energy;
            dt.Energy = DamageEnergyType.Electricity;

            deal.DamageType = dt;
            return 1;

        }
    }
}