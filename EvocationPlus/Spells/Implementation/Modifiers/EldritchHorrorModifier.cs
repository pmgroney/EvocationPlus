using EvocationPlus.BlueprintUtils;
using EvocationPlus.Patches;
using EvocationPlus.Spells.Infrastructure;
using EvocationPlus.Utils;
using HarmonyLib;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Enums;
using Kingmaker.RuleSystem;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Abilities.Components.AreaEffects;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.UnitLogic.Mechanics.Components;
using Kingmaker.Utility;

namespace EvocationPlus.Spells.Implementation.Modifiers
{
    public sealed class EldritchHorrorModifier : ISpellModifier
    {
        private static readonly System.Reflection.FieldInfo AbilityAoERadiusMRadius =
            AccessTools.Field(typeof(AbilityAoERadius), "m_Radius");
        private static readonly System.Reflection.FieldInfo ContextRankConfigMType =
            AccessTools.Field(typeof(ContextRankConfig), "m_Type");
        private static readonly System.Reflection.FieldInfo ContextRankConfigMBaseValueType =
            AccessTools.Field(typeof(ContextRankConfig), "m_BaseValueType");
        private static readonly System.Reflection.FieldInfo ContextRankConfigMProgression =
            AccessTools.Field(typeof(ContextRankConfig), "m_Progression");
        private static readonly System.Reflection.FieldInfo ContextRankConfigMStartLevel =
            AccessTools.Field(typeof(ContextRankConfig), "m_StartLevel");
        private static readonly System.Reflection.FieldInfo ContextRankConfigMStepLevel =
            AccessTools.Field(typeof(ContextRankConfig), "m_StepLevel");
        
        public void Apply(BlueprintAbility spell, LibraryScriptableObject library)
        {
            if (spell == null) return;

            EvocationPlusUnitFactText.SetNameKey(spell, "EVP_EldritchHorror_Name");
            EvocationPlusUnitFactText.SetDescriptionKey(spell, "EVP_EldritchHorror_Desc");

            // School
            var sc = spell.GetComponent<SpellComponent>();
            if (sc != null)
                sc.School = SpellSchool.Necromancy;

            if (!TryFindSpawnAreaEffect(spell, out var spawn, out var reason))
            {
                Main.Mod.Logger.Log("EldritchHorror: " + reason);
                return;
            }

            // Area setup
            spawn.AreaEffect.Size = 40.Feet();
            spawn.OnUnit = false;

            spawn.DurationValue = new ContextDurationValue
            {
                Rate = DurationRate.Rounds,
                DiceType = DiceType.D4, // ignored since DiceCountValue=0
                DiceCountValue = new ContextValue { ValueType = ContextValueType.Simple, Value = 0 },
                BonusValue = new ContextValue { ValueType = ContextValueType.Simple, Value = 10 }
            };

            EnsureEldritchHorrorRank(spawn.AreaEffect);

            // Visual scaling
            var scaleComp = spawn.AreaEffect.GetComponent<AreaEffectScaleFxOnSpawn>();
            if (scaleComp == null)
                scaleComp = spawn.AreaEffect.AddComponent<AreaEffectScaleFxOnSpawn>();
            scaleComp.scale = 0.5f;

            // Reticle radius (UI)
            SetAoERadiusFeet(spell, 20);

            var areaRun = spawn.AreaEffect.GetComponent<AbilityAreaEffectRunAction>();
            if (areaRun == null)
            {
                Main.Mod.Logger.Log("EldritchHorror: AbilityAreaEffectRunAction missing on AreaEffect.");
            }
            else
            {
                EnsureAreaDealsBaseDamagePatch.EnsureAreaDealsBaseDamage(areaRun);
            }
        }

        private static bool TryFindSpawnAreaEffect(
            BlueprintAbility spell,
            out ContextActionSpawnAreaEffect spawn,
            out string reason)
        {
            spawn = null;
            reason = null;

            var effectRun = spell.GetComponent<AbilityEffectRunAction>();
            var actions = effectRun?.Actions;
            if (actions == null)
            {
                reason = "AbilityEffectRunAction / Actions missing";
                return false;
            }

            ContextActionSpawnAreaEffect found = null;

            // Walk the entire action tree (Conditionals, Saved branches, wrappers, etc.)
            ActionListUtil.Patch(actions, action =>
            {
                if (found == null && action is ContextActionSpawnAreaEffect s && s.AreaEffect != null)
                    found = s;

                // Search only; don't mutate.
                return 0;
            });

            if (found != null)
            {
                spawn = found;
                return true;
            }

            reason = "No ContextActionSpawnAreaEffect with AreaEffect found (recursive search).";
            return false;
        }

        private static void EnsureEldritchHorrorRank(BlueprintScriptableObject bpOwner)
        {
            // This config drives extra dice: first bonus at CL 7, then every 2 CL.
            var rank = bpOwner.GetComponent<ContextRankConfig>();
            if (rank == null)
                rank = bpOwner.AddComponent<ContextRankConfig>();

            if (ContextRankConfigMType == null ||
                ContextRankConfigMBaseValueType == null ||
                ContextRankConfigMProgression == null ||
                ContextRankConfigMStartLevel == null ||
                ContextRankConfigMStepLevel == null)
            {
                Main.Mod.Logger.Log("EldritchHorror: ContextRankConfig field layout mismatch; rank not configured.");
                return;
            }

            ContextRankConfigMType.SetValue(rank, AbilityRankType.Default);
            ContextRankConfigMBaseValueType.SetValue(rank, ContextRankBaseValueType.CasterLevel);
            ContextRankConfigMProgression.SetValue(rank, ContextRankProgression.DelayedStartPlusDivStep);
            ContextRankConfigMStartLevel.SetValue(rank, 7);
            ContextRankConfigMStepLevel.SetValue(rank, 2);
        }

        private static void SetAoERadiusFeet(BlueprintAbility spell, int feet)
        {
            var aoeRadiusComp = spell.GetComponent<AbilityAoERadius>();
            if (aoeRadiusComp == null)
            {
                Main.Mod.Logger.Log("EldritchHorror: AbilityAoERadius component missing; reticle radius not set.");
                return;
            }

            if (AbilityAoERadiusMRadius == null)
            {
                Main.Mod.Logger.Log("EldritchHorror: AbilityAoERadius.m_Radius field not found; reticle radius not set.");
                return;
            }

            AbilityAoERadiusMRadius.SetValue(aoeRadiusComp, feet.Feet());
        }

    }
}
