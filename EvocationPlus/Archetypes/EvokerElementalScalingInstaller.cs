using System;
using System.Collections.Generic;
using System.Linq;
using EvocationPlus.BlueprintUtils;
using EvocationPlus.Core;
using EvocationPlus.IconUtils;
using EvocationPlus.Patches.Bloodlines;
using EvocationPlus.Utils;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Enums.Damage;
using UnityEngine;

namespace EvocationPlus.Archetypes
{
    internal static class EvokerElementalScalingInstaller
    {
        // Levels: every 4 levels (pattern you chose)
        private static readonly int[] ScalingLevels = { 1, 5, 9, 13, 17 };
      
        internal static void ApplyFire(LibraryScriptableObject library, BlueprintProgression bloodlineProg,
            BlueprintCharacterClass sorcererClass)
            => ApplyElement(library, bloodlineProg, sorcererClass,
                Guids.Features.EvokerScalingFireGuid,
                "EvocationPlus_EvokerScaling_Fire",
                "EVP_EVOKER_SCALING_FIRE_NAME",
                "EVP_EVOKER_SCALING_FIRE_DESC",
                "infernal_potency.png", // ✅
                DamageEnergyType.Fire);

        internal static void ApplyWater(LibraryScriptableObject library, BlueprintProgression bloodlineProg,
            BlueprintCharacterClass sorcererClass)
            => ApplyElement(library, bloodlineProg, sorcererClass,
                Guids.Features.EvokerScalingWaterGuid,
                "EvocationPlus_EvokerScaling_Water",
                "EVP_EVOKER_SCALING_WATER_NAME",
                "EVP_EVOKER_SCALING_WATER_DESC",
                "glacial_dominion.png", // ✅
                DamageEnergyType.Cold);

        internal static void ApplyAir(LibraryScriptableObject library, BlueprintProgression bloodlineProg,
            BlueprintCharacterClass sorcererClass)
            => ApplyElement(library, bloodlineProg, sorcererClass,
                Guids.Features.EvokerScalingAirGuid,
                "EvocationPlus_EvokerScaling_Air",
                "EVP_EVOKER_SCALING_AIR_NAME",
                "EVP_EVOKER_SCALING_AIR_DESC",
                "tempest_surge.png", // ✅
                DamageEnergyType.Electricity);

        internal static void ApplyEarth(LibraryScriptableObject library, BlueprintProgression bloodlineProg,
            BlueprintCharacterClass sorcererClass)
            => ApplyElement(library, bloodlineProg, sorcererClass,
                Guids.Features.EvokerScalingEarthGuid,
                "EvocationPlus_EvokerScaling_Earth",
                "EVP_EVOKER_SCALING_EARTH_NAME",
                "EVP_EVOKER_SCALING_EARTH_DESC",
                "corrosive_mastery.png", // ✅
                DamageEnergyType.Acid);

        internal static void ApplyArcane(LibraryScriptableObject library, BlueprintProgression bloodlineProg,
            BlueprintCharacterClass sorcererClass)
        {
            ApplyArcaneInternal(
                library,
                bloodlineProg,
                sorcererClass,
                Guids.Features.EvokerScalingArcaneGuid,
                "EvocationPlus_EvokerScaling_Arcane",
                "EVP_EVOKER_SCALING_ARCANE_NAME",
                "EVP_EVOKER_SCALING_ARCANE_DESC",
                "arcane_supremacy.png"); // ✅
        }

        private static void ApplyElement(
            LibraryScriptableObject library,
            BlueprintProgression prog,
            BlueprintCharacterClass sorcererClass,
            string featureGuid,
            string internalName,
            string nameKey,
            string descKey,
            string iconPng,
            DamageEnergyType energyType)
        {
            if (prog == null || sorcererClass == null) return;

            var feature = EnsureElementFeature(library, sorcererClass, featureGuid, internalName, nameKey, descKey,
                iconPng, energyType);
            if (feature == null) return;

            // Grant rank scaling via repeated adds at 1/5/9/13/17
            foreach (var lvl in ScalingLevels)
                EnsureFeatureAtLevel(prog, lvl, feature);
        }

        private static void EnsureFeatureAtLevel(BlueprintProgression prog, int level, BlueprintFeatureBase feature)
        {
            var entries = (prog.LevelEntries ?? Array.Empty<LevelEntry>()).ToList();
            var entry = entries.FirstOrDefault(e => e != null && e.Level == level);
            if (entry == null)
            {
                entry = new LevelEntry { Level = level, Features = new List<BlueprintFeatureBase>() };
                entries.Add(entry);
            }

            if (!entry.Features.Contains(feature))
                entry.Features.Add(feature);

            prog.LevelEntries = entries.OrderBy(e => e.Level).ToArray();
        }

        private static void ApplyArcaneInternal(
            LibraryScriptableObject library,
            BlueprintProgression prog,
            BlueprintCharacterClass sorcererClass,
            string featureGuid,
            string internalName,
            string nameKey,
            string descKey,
            string iconPng)
        {
            if (prog == null || sorcererClass == null) return;

            var feature = EnsureArcaneDcFeature(library, sorcererClass, featureGuid, internalName, nameKey, descKey,
                iconPng);
            if (feature == null) return;
            feature.Ranks = 5; 
            feature.HideInUI = false;
            feature.HideInCharacterSheetAndLevelUp = false;
            feature.HideNotAvailibleInUI = false;
            // Keep the REAL scaling feature for mechanics (ranks granted by repeated adds).
            foreach (var lvl in ScalingLevels)
                EnsureFeatureAtLevel(prog, lvl, feature);
            
        }

        private static BlueprintFeature EnsureElementFeature(
            LibraryScriptableObject library,
            BlueprintCharacterClass sorcererClass,
            string featureGuid,
            string internalName,
            string nameKey,
            string descKey,
            string iconPng,
            DamageEnergyType energyType)
        {
            var existing = BlueprintLibrary.GetBlueprint(library, featureGuid) as BlueprintFeature;
            if (existing != null) return existing;

            // ✅ Build from scratch (no proto)
            var f = ScriptableObject.CreateInstance<BlueprintFeature>();
            f.name = internalName;
            f.AssetGuid = BlueprintLibrary.NormalizeGuid(featureGuid);

            f.IsClassFeature = true;

            // ✅ This is the scaling mechanism: ranks are granted by repeated adds at 1/5/9/13/17
            f.Ranks = 5;
            f.HideInUI = false;
            f.HideInCharacterSheetAndLevelUp = false;
            f.HideNotAvailibleInUI = false;

            UnitFactStringUtils.SetUnitFactStrings(f, nameKey, descKey);

            var icon = DiskIconLoader.LoadSprite(iconPng);
            if (icon != null)
                BlueprintUnitFactUI.SetIcon(f, icon);

            // Add ONLY your logic component(s)
            var comp = ScriptableObject.CreateInstance<EvokerElementalPerDieBonusDamage>();
            comp.EnergyType = energyType;

            f.ComponentsArray = new BlueprintComponent[] { comp };

            BlueprintLibrary.Register(library, featureGuid, f);
            return f;
        }

        private static BlueprintFeature EnsureArcaneDcFeature(
            LibraryScriptableObject library,
            BlueprintCharacterClass sorcererClass,
            string featureGuid,
            string internalName,
            string nameKey,
            string descKey,
            string iconPng)
        {
            var existing = BlueprintLibrary.GetBlueprint(library, featureGuid) as BlueprintFeature;
            if (existing != null) return existing;

            // ✅ Build from scratch (no proto)
            var f = ScriptableObject.CreateInstance<BlueprintFeature>();
            f.name = internalName;
            f.AssetGuid = BlueprintLibrary.NormalizeGuid(featureGuid);

            f.IsClassFeature = true;

            // ✅ Ranks come from repeated adds at 1/5/9/13/17
            f.Ranks = 5;
            f.HideInUI = false;
            f.HideInCharacterSheetAndLevelUp = false;
            f.HideNotAvailibleInUI = false;

            UnitFactStringUtils.SetUnitFactStrings(f, nameKey, descKey);

            var icon = DiskIconLoader.LoadSprite(iconPng);
            if (icon != null)
                BlueprintUnitFactUI.SetIcon(f, icon);

            // Add ONLY your logic component(s)
            var comp = ScriptableObject.CreateInstance<EvokerArcaneDcScaling>();

            f.ComponentsArray = new BlueprintComponent[] { comp };

            BlueprintLibrary.Register(library, featureGuid, f);
            return f;
        }
    }
}