using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Mechanics;
using UnityEngine;

namespace EvocationPlus.Utils
{
    internal static class ComponentFactory
    {
        public static AddContextStatBonus NaturalArmorRankBonus(AbilityRankType rankType)
        {
            var c = ScriptableObject.CreateInstance<AddContextStatBonus>();

            c.Stat = StatType.AC;
            c.Descriptor = ModifierDescriptor.NaturalArmor;
            c.Multiplier = 1;
            c.Value = new ContextValue
            {
                ValueType = ContextValueType.Rank,
                ValueRank = rankType
            };

            return c;
        }
    }
}