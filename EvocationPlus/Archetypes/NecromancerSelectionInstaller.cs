using System;
using System.Linq;
using EvocationPlus.BlueprintUtils;
using EvocationPlus.Core;
using EvocationPlus.Patches.Bloodlines;
using EvocationPlus.Utils;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Blueprints.Facts;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Components;
using UnityEngine;

// AddFacts

namespace EvocationPlus.Archetypes
{
    internal static class NecromancerSelectionInstaller
    {
        // You need a GUID for the wrapper feature (new blueprint)
        // Put this in your ArchetypeDefinition/registry or keep as a const here for now.
        private const string OptBGuid = "2b3c4d5e6f708192a3b4c5d6e7f8091a";
        private const string OptAGuid = "3c4d5e6f708192a3b4c5d6e7f8091a2b";


        internal static BlueprintFeatureSelection EnsureSelection(LibraryScriptableObject library)
        {
            var existing = BlueprintLibrary.GetBlueprint(library, Guids.Features.NecromancerSelection) as BlueprintFeatureSelection;
            if (existing != null) return existing;

            var sel = ScriptableObject.CreateInstance<BlueprintFeatureSelection>();
            sel.name = "EvocationPlus_NecromancerFocusSelection";
            sel.AssetGuid = BlueprintLibrary.NormalizeGuid(Guids.Features.NecromancerSelection);

            UnitFactStringUtils.SetUnitFactStrings(sel, "EVP_NECRO_FOCUS_NAME", "EVP_NECRO_FOCUS_DESC");
            sel.ReapplyOnLevelUp = true;
            sel.IsClassFeature = true;
            sel.Obligatory = true;
            sel.IgnorePrerequisites = false;
            sel.Mode = SelectionMode.Default;
            sel.Group = FeatureGroup.None;

            var sorcererClass = BlueprintLibrary.GetBlueprint(
                library,
                Guids.Features.SorcererClassGuid) as BlueprintCharacterClass;

            if (sorcererClass == null)
            {
                Main.Mod.Logger.Log(
                    "EvocationPlus: Sorcerer class blueprint not found; cannot build Necromancer selection.");
                return null;
            }

            // Build/get the progression
            var boneArmorProgression = BoneArmorInstaller.EnsureBoneArmorProgression(
                library,
                sorcererClass,
                UnitFactStringUtils.SetUnitFactStrings);

            if (boneArmorProgression == null)
            {
                Main.Mod.Logger.Log("EvocationPlus: BoneArmorProgression is null.");
                return null;
            }
            var optionA = EnsureOption(
                library,
                OptAGuid,
                "EVP_NECRO_OPT_A_NAME",
                "EVP_NECRO_OPT_A_DESC",
                "EvocationPlus_NecroOptionA");
            
            var optionB = EnsureOption(
                library,
                OptBGuid,
                "EVP_NECRO_OPT_B_NAME",
                "EVP_NECRO_OPT_B_DESC",
                "EvocationPlus_NecroOptionB");

            sel.AllFeatures = new[]
                {
                    optionA,
                    optionB
                }
                .Where(f => f != null)
                .ToArray();

            foreach (var f in sel.AllFeatures)
                if (f == null)
                {
                    Main.Mod.Logger.Log("Necromancer selection item: <null>");
                }

            BlueprintLibrary.Register(library, Guids.Features.NecromancerSelection, sel);
            return sel;
        }

        public static BlueprintFeature EnsureBoneArmorRankUp(
            LibraryScriptableObject library,
            string guid,
            string internalName,
            BlueprintUnitFact markerFact, // your OptionA wrapper feature
            BlueprintUnitFact bonusFeature) // the stacking AC feature
        {
            var existing = BlueprintLibrary.GetBlueprint(library, guid) as BlueprintFeature;
            if (existing != null) return existing;

            var f = ScriptableObject.CreateInstance<BlueprintFeature>();
            f.name = internalName;
            f.AssetGuid = BlueprintLibrary.NormalizeGuid(guid);
            f.IsClassFeature = true;
            f.Ranks = 1;

            // Hide these helpers from UI
            f.HideInUI = false;
            f.HideInCharacterSheetAndLevelUp = false;
            f.HideNotAvailibleInUI = false;
            UnitFactStringUtils.SetUnitFactStrings(f, "EVP_NECRO_BONE_ARMOR_NAME", "EVP_NECRO_BONE_ARMOR_DESC");
            BlueprintLibrary.Register(library, guid, f);
            return f;
        }

        private static BlueprintFeature EnsureOption(
            LibraryScriptableObject library,
            string guid,
            string nameKey,
            string descKey,
            string internalName)
        {
            if (guid == BloodlineGuids.BoneArmorProgressionGuid)
                throw new Exception(
                    "Option A (Bone Armor) must be created as a BlueprintProgression. Do not call EnsureOption for Option A.");

            var existing = BlueprintLibrary.GetBlueprint(library, guid) as BlueprintFeature;
            if (existing != null) return existing;

            var f = ScriptableObject.CreateInstance<BlueprintFeature>();
            f.name = internalName;
            f.AssetGuid = BlueprintLibrary.NormalizeGuid(guid);
            f.IsClassFeature = true;
            f.Ranks = 1;

            UnitFactStringUtils.SetUnitFactStrings(f, nameKey, descKey);
            if (guid == BloodlineGuids.BoneArmorProgressionGuid) 
                return EnsureBoneArmorProgression(library);

            BlueprintLibrary.Register(library, guid, f);
            return f;
        }

        private static BlueprintFeature EnsureBoneArmorProgression(LibraryScriptableObject library)
        {
            var existing = BlueprintLibrary.GetBlueprint(library, BloodlineGuids.BoneArmorProgressionGuid) as BlueprintProgression;
            if (existing != null) return existing;

            var sorcerer = BlueprintLibrary.GetBlueprint(library, "b3a505fb61437dc4097f43c3f8f9a4cf")
                as BlueprintCharacterClass;

            if (sorcerer == null)
            {
                Main.Mod.Logger.Log("EvocationPlus: Sorcerer class not found for Bone Armor.");
                return null;
            }

            var bonusFeature = EnsureBoneArmorBonusFeature(library);

            var prog = ScriptableObject.CreateInstance<BlueprintProgression>();
            prog.name = "EvocationPlus_BoneArmorProgression";
            prog.AssetGuid = BlueprintLibrary.NormalizeGuid(BloodlineGuids.BoneArmorProgressionGuid);
            prog.IsClassFeature = true;

            // Use the keys that already exist in your .resx for option A
            UnitFactStringUtils.SetUnitFactStrings(prog, "EVP_NECRO_BONE_ARMOR_NAME", "EVP_NECRO_BONE_ARMOR_DESC");

            // Class association
            prog.Classes = new[] { sorcerer };
            prog.Archetypes = new BlueprintArchetype[0];

            // IMPORTANT: helps the game treat this progression as part of Sorcerer progression during level-up preview
            prog.ExclusiveProgression = sorcerer;

            var boneArmorFeature = EnsureBoneArmorBonusFeature(library);

            prog.LevelEntries = new[]
            {
                new LevelEntry { Level = 1, Features = { boneArmorFeature } },
                new LevelEntry { Level = 5, Features = { boneArmorFeature } },
                new LevelEntry { Level = 9, Features = { boneArmorFeature } },
                new LevelEntry { Level = 13, Features = { boneArmorFeature } },
                new LevelEntry { Level = 17, Features = { boneArmorFeature } }
            };
            prog.HideInUI = false;
            prog.HideInCharacterSheetAndLevelUp = false;
            prog.HideNotAvailibleInUI = false;


            prog.UIDeterminatorsGroup = new BlueprintFeatureBase[] { bonusFeature };

            // These two help the class progression window draw the track cleanly
            prog.UIGroups = new[]
            {
                new UIGroup { Features = { bonusFeature } }
            };

            BlueprintLibrary.Register(library, BloodlineGuids.BoneArmorProgressionGuid, prog);
            return prog;
        }

        internal static BlueprintFeature EnsureBoneArmorBonusFeature(LibraryScriptableObject library)
        {
            var boneArmorBonus =
                BoneArmorInstaller.EnsureBoneArmorBonusFeature(library, UnitFactStringUtils.SetUnitFactStrings);
            if (boneArmorBonus == null)
                Main.Mod.Logger.Log("ArchetypeInstaller: Bone Armor bonus feature not created.");

            var feature = ScriptableObject.CreateInstance<BlueprintFeature>();
            feature.name = "EvocationPlus_BoneArmorFeature";
            feature.AssetGuid = BlueprintLibrary.NormalizeGuid(BloodlineGuids.BoneArmorBonusGuid);
            feature.IsClassFeature = true;

            // This is what lets it scale to +5 by being granted 5 times
            feature.Ranks = 5;

            // IMPORTANT: do not hide it, or it will not show in the class progression window
            feature.HideInUI = false;
            feature.HideInCharacterSheetAndLevelUp = false;
            feature.HideNotAvailibleInUI = false;

            UnitFactStringUtils.SetUnitFactStrings(feature, "EVP_NECRO_BONE_ARMOR_NAME", "EVP_NECRO_BONE_ARMOR_DESC");
            var rank = feature.AddComponent<ContextRankConfig>();
            // Field names vary a bit by Owlcat version, so set via your reflection helper.
            // We try the common names used in Kingmaker/WotR.
            ReflectionUtils.SetFieldAny(rank, new[] { "m_BaseValueType", "BaseValueType" },
                ContextRankBaseValueType.FeatureRank);
            ReflectionUtils.SetFieldAny(rank, new[] { "m_Type", "Type" }, AbilityRankType.Default);
            var add = feature.AddComponent<AddContextStatBonus>();
            add.Stat = StatType.AC;
            add.Descriptor = ModifierDescriptor.NaturalArmor;
            add.Value = new ContextValue
            {
                ValueType = ContextValueType.Rank
            };

            //feature.ComponentsArray = new BlueprintComponent[] { add };

            BlueprintLibrary.Register(library, BloodlineGuids.BoneArmorBonusGuid, feature);

            return feature;
        }
    }
}