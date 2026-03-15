using System;
using System.Collections.Generic;
using EvocationPlus.BlueprintUtils;
using EvocationPlus.IconUtils;
using EvocationPlus.Patches.Bloodlines;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.UnitLogic.FactLogic;
using UnityEngine;

namespace EvocationPlus.Archetypes
{
    internal static class BoneArmorInstaller
    {
        internal static BlueprintProgression EnsureBoneArmorProgression(
            LibraryScriptableObject library,
            BlueprintCharacterClass sorcererClass,
            Action<BlueprintUnitFact, string, string> setUnitFactStrings)
        {
            if (library == null) throw new ArgumentNullException(nameof(library));
            if (sorcererClass == null) throw new ArgumentNullException(nameof(sorcererClass));
            if (setUnitFactStrings == null) throw new ArgumentNullException(nameof(setUnitFactStrings));

            var existing = BlueprintLibrary.GetBlueprint(library, BloodlineGuids.BoneArmorProgressionGuid) as BlueprintProgression;
            if (existing != null) return existing;

            var bonusFeature = EnsureBoneArmorBonusFeature(library, setUnitFactStrings);
            if (bonusFeature == null)
                throw new Exception("EnsureBoneArmorBonusFeature returned null.");
            var progression = ScriptableObject.CreateInstance<BlueprintProgression>();
            progression.name = "EvocationPlus_BoneArmorProgression";
            progression.AssetGuid = BlueprintLibrary.NormalizeGuid(BloodlineGuids.BoneArmorProgressionGuid);
            progression.IsClassFeature = true;
            progression.Ranks = 1;

            var icon = DiskIconLoader.LoadSprite("bone_armor.png");
            // In EnsureBoneArmorBonusFeature (after create)
            if (icon != null)
            {
                BlueprintUnitFactUI.SetIcon(bonusFeature, icon);
                BlueprintUnitFactUI.SetIcon(progression, icon);
            }

            // IMPORTANT: Use the keys you already have in the resx for option A.
            setUnitFactStrings(progression, "EVP_NECRO_BONE_ARMOR_NAME", "EVP_NECRO_BONE_ARMOR_DESC");

            progression.Classes = new[] { sorcererClass };
            progression.Archetypes = Array.Empty<BlueprintArchetype>();

            progression.LevelEntries = new[]
            {
                new LevelEntry
                {
                    Level = 1,
                    Features = new List<BlueprintFeatureBase> { bonusFeature }
                },
                new LevelEntry
                {
                    Level = 5,
                    Features = new List<BlueprintFeatureBase> { bonusFeature }
                },
                new LevelEntry
                {
                    Level = 9,
                    Features = new List<BlueprintFeatureBase> { bonusFeature }
                },
                new LevelEntry
                {
                    Level = 13,
                    Features = new List<BlueprintFeatureBase> { bonusFeature }
                },
                new LevelEntry
                {
                    Level = 17,
                    Features = new List<BlueprintFeatureBase> { bonusFeature }
                }
            };

            progression.UIDeterminatorsGroup = Array.Empty<BlueprintFeatureBase>();
            progression.UIGroups = Array.Empty<UIGroup>();
            progression.ExclusiveProgression = null;

            BlueprintLibrary.Register(library, BloodlineGuids.BoneArmorProgressionGuid, progression);
            return progression;
        }

        internal static BlueprintFeature EnsureBoneArmorBonusFeature(
            LibraryScriptableObject library,
            Action<BlueprintUnitFact, string, string> setUnitFactStrings)
        {
            var existing = BlueprintLibrary.GetBlueprint(library, BloodlineGuids.BoneArmorBonusGuid) as BlueprintFeature;
            if (existing != null) return existing;

            var feature = ScriptableObject.CreateInstance<BlueprintFeature>();
            feature.name = "EvocationPlus_BoneArmorBonus";
            feature.AssetGuid = BlueprintLibrary.NormalizeGuid(BloodlineGuids.BoneArmorBonusGuid);
            feature.IsClassFeature = true;

            // This is the scaling mechanism:
            feature.Ranks = 5;
            feature.HideInUI = false;

            // Not required, but safe:
            setUnitFactStrings(feature, "EVP_NECRO_BONEARMOR_BONUS_NAME", "EVP_NECRO_BONEARMOR_BONUS_DESC");

            var addStatBonus = ScriptableObject.CreateInstance<AddStatBonus>();
            addStatBonus.Stat = StatType.AC;
            addStatBonus.Descriptor = ModifierDescriptor.NaturalArmor;
            addStatBonus.Value = 1;
            addStatBonus.ScaleByBasicAttackBonus = false;

            feature.ComponentsArray = new BlueprintComponent[] { addStatBonus };

            BlueprintLibrary.Register(library, BloodlineGuids.BoneArmorBonusGuid, feature);

            return feature;
        }
    }
}