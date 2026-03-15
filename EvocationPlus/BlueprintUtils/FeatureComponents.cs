using System;
using System.Linq;
using System.Reflection;
using EvocationPlus.Utils;
using Kingmaker.Blueprints.Classes;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Components;
using UnityEngine;

namespace EvocationPlus.BlueprintUtils
{
    internal static class FeatureComponents
    {
        private static readonly BindingFlags NonPublicInstance =
            BindingFlags.Instance | BindingFlags.NonPublic;

        // Cache FieldInfos once (fast + clean)
        private static readonly FieldInfo RankType =
            typeof(ContextRankConfig).GetField("m_Type", NonPublicInstance);

        private static readonly FieldInfo RankBaseValueType =
            typeof(ContextRankConfig).GetField("m_BaseValueType", NonPublicInstance);

        private static readonly FieldInfo RankProgression =
            typeof(ContextRankConfig).GetField("m_Progression", NonPublicInstance);

        private static readonly FieldInfo RankStartLevel =
            typeof(ContextRankConfig).GetField("m_StartLevel", NonPublicInstance);

        private static readonly FieldInfo RankStepLevel =
            typeof(ContextRankConfig).GetField("m_StepLevel", NonPublicInstance);

        private static readonly FieldInfo RankClass =
            typeof(ContextRankConfig).GetField("m_Class", NonPublicInstance);

        private static readonly FieldInfo RankUseMin =
            typeof(ContextRankConfig).GetField("m_UseMin", NonPublicInstance);

        private static readonly FieldInfo RankMin =
            typeof(ContextRankConfig).GetField("m_Min", NonPublicInstance);

        private static readonly FieldInfo RankUseMax =
            typeof(ContextRankConfig).GetField("m_UseMax", NonPublicInstance);

        private static readonly FieldInfo RankMax =
            typeof(ContextRankConfig).GetField("m_Max", NonPublicInstance);

        /// <summary>
        ///     Adds a scaling Natural Armor bonus to a feature:
        ///     +1 at level 1–4, +2 at 5–8, +3 at 9–12, +4 at 13–16, +5 at 17–20.
        ///     Scales by the specified class level.
        /// </summary>
        public static void AddScalingNaturalArmorByClass(
            BlueprintFeature boneArmor,
            BlueprintCharacterClass klass,
            int maxBonus = 5,
            int stepLevels = 4,
            AbilityRankType rankType = AbilityRankType.Default)
        {
            if (boneArmor == null) throw new ArgumentNullException(nameof(boneArmor));
            if (klass == null) throw new ArgumentNullException(nameof(klass));

            // Remove any previous fixed stat bonus to avoid stacking if you ran older builds
            boneArmor.ComponentsArray = boneArmor.ComponentsArray
                .Where(c => !(c is AddStatBonus))
                .ToArray();

            // Also avoid duplicates if you reload and recreate (defensive)
            boneArmor.ComponentsArray = boneArmor.ComponentsArray
                .Where(c => !(c is AddContextStatBonus))
                .Where(c => !(c is ContextRankConfig))
                .ToArray();

            var bonus = ScriptableObject.CreateInstance<AddContextStatBonus>();

            bonus.Stat = StatType.AC;
            bonus.Descriptor = ModifierDescriptor.NaturalArmor;
            bonus.Multiplier = 1;
            bonus.Value = new ContextValue
            {
                ValueType = ContextValueType.Rank,
                ValueRank = rankType
            };

            boneArmor.ComponentsArray = boneArmor.ComponentsArray
                .Append(ComponentFactory.NaturalArmorRankBonus(rankType))
                .ToArray();
        }

        private static ContextRankConfig CreateRank_ClassStep(
            AbilityRankType rankType,
            BlueprintCharacterClass klass,
            int startLevel,
            int stepLevel,
            int min,
            int max)
        {
            var cfg = ScriptableObject.CreateInstance<ContextRankConfig>();

            // Validate required fields exist (fail loudly in logs if Owlcat changes things)
            if (RankType == null || RankBaseValueType == null || RankProgression == null || RankClass == null)
                throw new Exception("ContextRankConfig field layout not as expected (required fields missing).");

            RankType.SetValue(cfg, rankType);
            RankBaseValueType.SetValue(cfg, ContextRankBaseValueType.ClassLevel);
            RankProgression.SetValue(cfg, ContextRankProgression.StartPlusDivStep);

            RankStartLevel?.SetValue(cfg, startLevel);
            RankStepLevel?.SetValue(cfg, stepLevel);

            RankClass.SetValue(cfg, new[] { klass });

            RankUseMin?.SetValue(cfg, true);
            RankMin?.SetValue(cfg, min);

            RankUseMax?.SetValue(cfg, true);
            RankMax?.SetValue(cfg, max);

            return cfg;
        }
    }
}