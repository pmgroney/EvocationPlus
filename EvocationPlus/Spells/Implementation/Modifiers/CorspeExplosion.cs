using System;
using EvocationPlus.BlueprintUtils;
using EvocationPlus.Classes;
using EvocationPlus.Patches;
using EvocationPlus.Spells.Infrastructure;
using EvocationPlus.Utils;
using HarmonyLib;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.ElementsSystem;
using Kingmaker.Enums.Damage;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.Utility;

namespace EvocationPlus.Spells.Implementation.Modifiers
{
    public sealed class CorpseExplosionModifier : ISpellModifier
    {
        private static readonly System.Reflection.FieldInfo TargetsAroundRadiusField =
            AccessTools.Field(typeof(AbilityTargetsAround), "m_Radius");

        public void Apply(BlueprintAbility spell, LibraryScriptableObject library)
        {
            if (spell == null) return;

            EvocationPlusUnitFactText.SetNameKey(spell, "EVP_CorpseExplosion_Name");
            EvocationPlusUnitFactText.SetDescriptionKey(spell, "EVP_CorpseExplosion_Desc");

            // Descriptor: remove Fire, add Death
            SpellSchoolUtil.ReplaceDescriptor(spell, SpellDescriptor.Fire, SpellDescriptor.Death);

            spell.Range = AbilityRange.Long;
            
            var sc = spell.GetComponent<SpellComponent>();
            if (sc != null)
                sc.School = SpellSchool.Necromancy;
            
            EnableDeadTargeting(spell);
            EnsureMustBeDead(spell);
            ConfigureTargeting(spell);

            // Remove projectile "travel feel" while preserving delivery/hit.
            if (!VfxUtil.TryMakeProjectilesInvisiblePreserveCount(spell, out var invisReason))
                Main.Mod.Logger.Log("CorpseExplosion: make projectile fast/minimal failed: " + invisReason);

            SetTargetsAroundRadius(spell, 15);

            PatchDamageType(spell);
        }

        private static void EnableDeadTargeting(BlueprintAbility spell)
        {
            if (spell.GetComponent<AllowDeadTargetingComponentBase>() == null)
                spell.AddComponent<AllowDeadTargeting>();
        }

        private static void EnsureMustBeDead(BlueprintAbility spell)
        {
            if (spell.GetComponent<AbilityTargetMustBeDead>() == null)
                spell.AddComponent<AbilityTargetMustBeDead>();
        }

        private static void ConfigureTargeting(BlueprintAbility spell)
        {
            // Unit-only (corpse), centered on target.Unit.Position
            spell.CanTargetPoint = false;
            spell.CanTargetEnemies = true;
            spell.CanTargetFriends = true;
            spell.CanTargetSelf = false;
            spell.Range = AbilityRange.Medium;
        }

        private static void SetTargetsAroundRadius(BlueprintAbility spell, int feet)
        {
            var around = spell.GetComponent<AbilityTargetsAround>();
            if (around == null) return;

            // If Owlcat ever changes the field name, we just skip safely.
            if (TargetsAroundRadiusField == null) return;

            TargetsAroundRadiusField.SetValue(around, feet.Feet());
        }

        private static void PatchDamageType(BlueprintAbility spell)
        {
            var run = spell.GetComponent<AbilityEffectRunAction>();
            if (run?.Actions == null) return;

            ActionListUtil.Patch(run.Actions, PatchAction);
        }

        private static int PatchAction(GameAction action)
        {
            if (action is ContextActionDealDamage deal)
            {
                if (deal.DamageType.Type != DamageType.Energy ||
                    deal.DamageType.Energy != DamageEnergyType.Fire)
                {
                    return 0;
                }

                var dt = (DamageTypeDescription)Activator.CreateInstance(typeof(DamageTypeDescription));
                dt.Type = DamageType.Energy;
                dt.Energy = DamageEnergyType.Unholy;
                // Prefer explicit instance (matches your other modifiers’ “don’t mutate shared” approach)
                deal.DamageType = dt;
                deal.Value.DiceType = DiceType.D8; 
                return 1;
            }

            // recursion handled centrally in ActionListUtil
            return 0;
        }
    }
}
