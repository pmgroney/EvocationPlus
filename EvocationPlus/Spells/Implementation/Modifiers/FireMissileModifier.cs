using System;
using EvocationPlus.BlueprintUtils;
using EvocationPlus.Core;
using EvocationPlus.IconUtils;
using EvocationPlus.Spells.Infrastructure;
using EvocationPlus.Utils;
using Kingmaker.Blueprints;
using Kingmaker.ElementsSystem;
using Kingmaker.Enums.Damage;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Mechanics.Actions;

namespace EvocationPlus.Spells.Implementation.Modifiers
{
    public class FireMissileModifier : ISpellModifier
    {
        public void Apply(BlueprintAbility spell, LibraryScriptableObject library)
        {
            if (spell == null || library == null)
                return;

            EvocationPlusUnitFactText.SetNameKey(spell, "EVP_FIRE_MISSILE_NAME");
            EvocationPlusUnitFactText.SetDescriptionKey(spell, "EVP_FIRE_MISSILE_DESC");
            ColorIcon(spell);
            // // VFX donor swap 
            var donorSpell = BlueprintLibrary.GetBlueprint(library, Guids.Spells.ScorchingRay) as BlueprintAbility;
            if (donorSpell == null)
            {
                Main.Mod.Logger.Log("FireMissile: donor spell not found for VFX (" + Guids.Spells.ScorchingRay + ")");
            }
            else
            {
                if (!VfxUtil.TryCopyProjectileViewAssetIdPreserveCount(spell, donorSpell, out var reason))
                    Main.Mod.Logger.Log("FireMissile: VFX swap failed: " + reason);
            }

            // Patch damage actions (central recursive walker)
            var runAction = spell.GetComponent<AbilityEffectRunAction>();
            if (runAction?.Actions != null)
                ActionListUtil.Patch(runAction.Actions, PatchAction);
        }
        private static void ColorIcon(BlueprintAbility spell)
        {
            var transform = IconStyles.FireRed(
                1.10f,  // slightly higher gamma = deeper shadows
                1.20f,  // moderate brightness
                0.68f,  // highlight starts later
                0.20f   // tighter highlight ramp
            );

            var tinted = IconShader.CreateTransformedCopy(spell.Icon, transform);
            BlueprintUnitFactUI.SetIcon(spell, tinted);
        }
        private static int PatchAction(GameAction action)
        {
            if (action is ContextActionDealDamage deal)
            {
                if (deal.DamageType.Type != DamageType.Force)
                    return 0;

                var dt = (DamageTypeDescription)Activator.CreateInstance(typeof(DamageTypeDescription));
                dt.Type = DamageType.Energy;
                dt.Energy = DamageEnergyType.Fire;
                deal.DamageType = dt;
                return 1;
            }

            return 0;
        }
    }
}