using System;
using EvocationPlus.BlueprintUtils;
using EvocationPlus.Core;
using EvocationPlus.Spells.Infrastructure;
using EvocationPlus.Utils;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Enums.Damage;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;

namespace EvocationPlus.Spells.Implementation.Modifiers
{
    public sealed class DeathRayModifier : ISpellModifier
    {
        public void Apply(BlueprintAbility spell, LibraryScriptableObject library)
        {
            if (spell == null) return;

            EvocationPlusUnitFactText.SetNameKey(spell, "EVP_DeathRay_Name");
            EvocationPlusUnitFactText.SetDescriptionKey(spell, "EVP_DeathRay_Desc");
            // VFX donor
            var vfxSource = BlueprintLibrary.GetBlueprint(library, Guids.Spells.Enervation) as BlueprintAbility;
            if (vfxSource == null)
            {
                Main.Mod.Logger.Log("Umbral Strike: donor spell not found for VFX (" + Guids.Spells.Enervation + ")");
            }
            else
            {
                if (!VfxUtil.TryCopyProjectileVisualOnly(spell, vfxSource, out var reason))
                    Main.Mod.Logger.Log("Umbral Strike: VFX copy failed: " + reason);
            }
            var sc = spell.GetComponent<SpellComponent>();
            if (sc != null)
                sc.School = SpellSchool.Necromancy;
            PatchDescriptor(spell);
            PatchDamageType(spell);
        }

        private static void PatchDescriptor(BlueprintAbility spell)
        {
            // remove elemental descriptors, add Death
            var removed =
                SpellDescriptor.Fire |
                SpellDescriptor.Cold |
                SpellDescriptor.Electricity |
                SpellDescriptor.Acid;

            SpellSchoolUtil.ReplaceDescriptor(spell, removed, SpellDescriptor.Death);
        }

        private static void PatchDamageType(BlueprintAbility spell)
        {
            var run = spell.GetComponent<AbilityEffectRunAction>();
            if (run?.Actions == null)
            {
                Main.Mod.Logger.Log("DeathRay: AbilityEffectRunAction missing; skipping damage patch.");
                return;
            }

            ActionListUtil.Patch(run.Actions, action =>
            {
                var deal = action as Kingmaker.UnitLogic.Mechanics.Actions.ContextActionDealDamage;
                if (deal == null) return 0;

                var dt = (DamageTypeDescription)Activator.CreateInstance(typeof(DamageTypeDescription));
                dt.Type = DamageType.Energy;
                dt.Energy = DamageEnergyType.NegativeEnergy;

                deal.DamageType = dt;
                return 1;
            });
            
        }
    }
}
