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
    public class AcidMissileModifier : ISpellModifier
    {
          public void Apply(BlueprintAbility spell, LibraryScriptableObject library)
        {
            if (spell == null || library == null)
                return;

            EvocationPlusUnitFactText.SetNameKey(spell, "EVP_ACID_MISSILE_NAME");
            EvocationPlusUnitFactText.SetDescriptionKey(spell, "EVP_ACID_MISSILE_DESC");
            IconTintUtil.ApplyTheme(spell, IconTheme.Acid);
            // VFX donor swap (optional, but if donor missing just log once)
            var donorSpell = BlueprintLibrary.GetBlueprint(library, Guids.Spells.DeathVfxSpell) as BlueprintAbility;
            if (donorSpell == null)
            {
                Main.Mod.Logger.Log("AcidMissile: donor spell not found for VFX (" + Guids.Spells.DeathVfxSpell + ")");
            }
            else
            {
                if (!VfxUtil.TryCopyProjectileViewAssetIdPreserveCount(spell, donorSpell, out var reason))
                    Main.Mod.Logger.Log("AcidMissile: VFX swap failed: " + reason);
            }

            // Patch damage actions (central recursive walker)
            var runAction = spell.GetComponent<AbilityEffectRunAction>();
            if (runAction?.Actions != null)
                ActionListUtil.Patch(runAction.Actions, PatchAction);
        }

        private static int PatchAction(GameAction action)
        {
            if (action is ContextActionDealDamage deal)
            {
                if (deal.DamageType.Type != DamageType.Force)
                    return 0;

                var dt = (DamageTypeDescription)Activator.CreateInstance(typeof(DamageTypeDescription));
                dt.Type = DamageType.Energy;
                dt.Energy = DamageEnergyType.Acid;
                deal.DamageType = dt;
                return 1;
            }

            return 0;
        }
    }
}