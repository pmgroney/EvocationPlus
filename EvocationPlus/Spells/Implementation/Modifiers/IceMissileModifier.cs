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
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Mechanics.Actions;

namespace EvocationPlus.Spells.Implementation.Modifiers
{
    public class IceMissileModifier : ISpellModifier
    {
        public void Apply(BlueprintAbility spell, LibraryScriptableObject library)
        {
            if (spell == null || library == null)
                return;

            EvocationPlusUnitFactText.SetNameKey(spell, "EVP_ICE_MISSILE_NAME");
            EvocationPlusUnitFactText.SetDescriptionKey(spell, "EVP_ICE_MISSILE_DESC");
            ColorIcon(spell);
            // VFX donor swap (optional, but if donor missing just log once)
            var donorSpell = BlueprintLibrary.GetBlueprint(library, Guids.Spells.SnowBall) as BlueprintAbility;
            if (donorSpell == null)
            {
                Main.Mod.Logger.Log("IceMissile: donor spell not found for VFX (" + Guids.Spells.SnowBall + ")");
            }
            else
            {
                if (!VfxUtil.TryCopyProjectileViewAssetIdPreserveCount(spell, donorSpell, out var reason))
                    Main.Mod.Logger.Log("IceMissile: VFX swap failed: " + reason);
            }

            // Patch damage actions (central recursive walker)
            var runAction = spell.GetComponent<AbilityEffectRunAction>();
            if (runAction?.Actions != null)
                ActionListUtil.Patch(runAction.Actions, PatchAction);
        }
        private static void ColorIcon(BlueprintAbility spell)
        {
            var transform = IconStyles.IceFrost(
                1.15f,  // moderate gamma
                1.15f,  // balanced brightness
                0.70f,  // highlight slightly later
                0.22f   // softer highlight ramp
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
                dt.Energy = DamageEnergyType.Cold;
                deal.DamageType = dt;
                return 1;
            }

            return 0;
        }
    }
}