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
using Kingmaker.UnitLogic.Mechanics.Components;

namespace EvocationPlus.Spells.Implementation.Modifiers
{
    public sealed class BoneSpikeModifier : ISpellModifier
    {
        public void Apply(BlueprintAbility spell, LibraryScriptableObject library)
        {
            if (spell == null || library == null)
                return;

            EvocationPlusUnitFactText.SetNameKey(spell, "EVP_BoneSpike_Name");
            EvocationPlusUnitFactText.SetDescriptionKey(spell, "EVP_BoneSpike_Desc");

            // Descriptor: remove Force, add Death
            SpellSchoolUtil.ReplaceDescriptor(spell, SpellDescriptor.Force, SpellDescriptor.Death);

            spell.Range = AbilityRange.Long;
            IncreaseProjectileScaling(spell);
            
            // School (simple + safe)
            var sc = spell.GetComponent<SpellComponent>();
            if (sc != null)
                sc.School = SpellSchool.Necromancy;

            // VFX donor swap (optional, but if donor missing just log once)
            var donorSpell = BlueprintLibrary.GetBlueprint(library, Guids.Spells.DeathVfxSpell) as BlueprintAbility;
            if (donorSpell == null)
            {
                Main.Mod.Logger.Log("BoneSpike: donor spell not found for VFX (" + Guids.Spells.DeathVfxSpell + ")");
            }
            else
            {
                if (!VfxUtil.TryCopyProjectileViewAssetIdPreserveCount(spell, donorSpell, out var reason))
                    Main.Mod.Logger.Log("BoneSpike: VFX swap failed: " + reason);
            }

            // Patch damage actions (central recursive walker)
            var runAction = spell.GetComponent<AbilityEffectRunAction>();
            if (runAction?.Actions != null)
                ActionListUtil.Patch(runAction.Actions, PatchAction);
        }

        private static void IncreaseProjectileScaling(BlueprintAbility spell)
        {
            var rank = spell.GetComponent<ContextRankConfig>();
            if (rank == null)
            {
                Main.Mod.Logger.Log("BoneSpike: missing ContextRankConfig; projectile scaling was not changed.");
                return;
            }

            if (!ContextRankConfigUtil.ForceMinMax(rank, 1, 8))
                Main.Mod.Logger.Log("BoneSpike: ContextRankConfig field layout mismatch; projectile scaling was not changed.");
        }

        private static int PatchAction(GameAction action)
        {
            if (action is ContextActionDealDamage deal)
            {
                if (deal.DamageType.Type != DamageType.Force)
                    return 0;

                var dt = (DamageTypeDescription)Activator.CreateInstance(typeof(DamageTypeDescription));
                dt.Type = DamageType.Energy;
                dt.Energy = DamageEnergyType.Unholy;
                deal.DamageType = dt;
                return 1;
            }

            return 0;
        }
    }
}
