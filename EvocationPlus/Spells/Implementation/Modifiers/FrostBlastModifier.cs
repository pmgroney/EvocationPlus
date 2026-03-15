using System;
using EvocationPlus.BlueprintUtils;
using EvocationPlus.Core;
using EvocationPlus.IconUtils;
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
using Kingmaker.UnitLogic.Mechanics.Actions;

namespace EvocationPlus.Spells.Implementation.Modifiers
{
    public sealed class FrostBlastModifier : ISpellModifier
    {
        public void Apply(BlueprintAbility spell, LibraryScriptableObject library)
        {
            if (spell == null || library == null)
                return;

            EvocationPlusUnitFactText.SetNameKey(spell, "EVP_FrostBlast_Name");
            EvocationPlusUnitFactText.SetDescriptionKey(spell, "EVP_FrostBlast_Desc");

            // Descriptor: remove Fire, add Cold
            SpellSchoolUtil.ReplaceDescriptor(spell, SpellDescriptor.Fire, SpellDescriptor.Cold);

            // VFX + Icon donor
            var vfxSource = BlueprintLibrary.GetBlueprint(library, Guids.Spells.RayOfFrost) as BlueprintAbility;
            if (vfxSource == null)
            {
                Main.Mod.Logger.Log("FrostBlast: donor spell not found for VFX/Icon (" + Guids.Spells.RayOfFrost + ")");
            }
            else
            {
                if (!VfxUtil.TryCopyProjectileVisualOnly(spell, vfxSource, out var reason))
                    Main.Mod.Logger.Log("FrostBlast: VFX copy failed: " + reason);

                CopyFromIcon.CopyIconFrom(spell, library, Guids.Spells.RayOfFrost);
            }

            // Patch damage actions (central recursive walker)
            var runAction = spell.GetComponent<AbilityEffectRunAction>();
            if (runAction?.Actions == null)
            {
                Main.Mod.Logger.Log("FrostBlast: AbilityEffectRunAction/Actions missing; skipping damage patch.");
                return; // last step
            }

            ActionListUtil.Patch(runAction.Actions, PatchAction);
        }

        private static int PatchAction(GameAction action)
        {
            if (action is ContextActionDealDamage deal)
            {
                // Only convert FIRE energy damage to Cold (avoid mutating other damage riders).
                if (deal.DamageType.Type != DamageType.Energy ||
                    deal.DamageType.Energy != DamageEnergyType.Fire)
                {
                    return 0;
                }

                // Replace the DamageTypeDescription rather than mutating it in-place
                // (prevents collateral mutation if the underlying object is shared)
                var dt = (DamageTypeDescription)Activator.CreateInstance(typeof(DamageTypeDescription));
                dt.Type = DamageType.Energy;
                dt.Energy = DamageEnergyType.Cold;
                deal.DamageType = dt;
                return 1;
            }

            // Let ActionListUtil handle wrapper recursion (ConditionalSaved, etc.)
            return 0;
        }
    }
}