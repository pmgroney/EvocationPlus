using System;
using Kingmaker.ElementsSystem;
using Kingmaker.Enums.Damage;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic.Abilities.Components.AreaEffects;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Actions;
using UnityEngine;

namespace EvocationPlus.Patches
{
    public class EnsureAreaDealsBaseDamagePatch
    {
        public static void EnsureAreaDealsBaseDamage(AbilityAreaEffectRunAction areaRun)
        {
            if (areaRun == null) return;

            if (areaRun.Round == null)
                areaRun.Round = new ActionList();

            var actions = areaRun.Round.Actions ?? Array.Empty<GameAction>();

            // Only skip if OUR named action already exists
            for (var i = 0; i < actions.Length; i++)
                if (actions[i] is ContextActionDealDamage dd &&
                    string.Equals(dd.name, "$EldritchHorror_DealDamage", StringComparison.Ordinal))
                    return;

            var deal = ScriptableObject.CreateInstance<ContextActionDealDamage>();
            deal.DamageType = new DamageTypeDescription
            {
                Type = DamageType.Physical,
                Energy = DamageEnergyType.Unholy
            };
            deal.Value = new ContextDiceValue
            {
                DiceType = DiceType.D4,
                DiceCountValue = new ContextValue { ValueType = ContextValueType.Simple, Value = 2 },
                BonusValue = new ContextValue { ValueType = ContextValueType.Simple, Value = 0 }
            };

            deal.name = "$EldritchHorror_DealDamage";
            deal.hideFlags = HideFlags.HideInHierarchy;
            deal.IsAoE = true;
            deal.HalfIfSaved = false;
            deal.Half = false;

            var newArr = new GameAction[actions.Length + 1];
            Array.Copy(actions, newArr, actions.Length);
            newArr[newArr.Length - 1] = deal;
            areaRun.Round.Actions = newArr;
        }
    }
}