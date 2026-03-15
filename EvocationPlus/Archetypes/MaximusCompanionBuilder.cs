using System;
using System.Linq;
using System.Reflection;
using EvocationPlus.BlueprintUtils;
using EvocationPlus.Core;
using EvocationPlus.IconUtils;
using EvocationPlus.Utils;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Blueprints.Root;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.Localization;
using Kingmaker.UnitLogic.FactLogic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace EvocationPlus.Archetypes
{
    internal static class MaximusCompanionBuilder
    {
        private static bool _built;

        internal static void EnsureMaximusCompanionEverywhere(LibraryScriptableObject library)
        {
            if (_built) return;
            _built = true;

            var maximusUnit = EnsureMaximusUnit(library);
            if (maximusUnit == null) return;

            var maximusFeature = EnsureMaximusFeature(library, maximusUnit);
            if (maximusFeature == null) return;

            AddMaximusToAllSelectionsThatContainLeopard(library, maximusFeature);
        }

        private static BlueprintUnit EnsureMaximusUnit(LibraryScriptableObject library)
        {
            var existing =
                BlueprintLibrary.GetBlueprint(library, Guids.BlueprintGuids.MaximusLeopardUnitGuid) as BlueprintUnit;
            if (existing != null) return existing;

            var donor = BlueprintLibrary.GetBlueprint(library, Guids.BlueprintGuids.LeopardUnitGuid) as BlueprintUnit;
            if (donor == null)
            {
                Main.Mod.Logger.Log("EVP: FAILED: donor leopard unit not found (Maximus).");
                return null;
            }

            var unit = Object.Instantiate(donor);
            unit.name = "EvocationPlus_AnimalCompanionUnitMaximus";
            unit.AssetGuid = BlueprintLibrary.NormalizeGuid(Guids.BlueprintGuids.MaximusLeopardUnitGuid);

            // Overtune
            unit.Dexterity += 6;
            unit.Constitution += 10;
            unit.Charisma += 14;

            // Keep (even though runtime tint is what visually sticks)
            unit.Color = new Color(0.70f, 0.72f, 0.75f, 1f);

            ApplyMaximusPortrait(library, unit);
            ApplyMaximusName(unit);

            BlueprintLibrary.Register(library, Guids.BlueprintGuids.MaximusLeopardUnitGuid, unit);
            return unit;
        }

        private static BlueprintFeature EnsureMaximusFeature(LibraryScriptableObject library, BlueprintUnit maximusUnit)
        {
            var existing = BlueprintLibrary.GetBlueprint(library, Guids.BlueprintGuids.MaximusLeopardFeatureGuid) as BlueprintFeature;
            if (existing != null) return existing;

            var donor = BlueprintLibrary.GetBlueprint(library, Guids.BlueprintGuids.LeopardFeatureGuid) as BlueprintFeature;
            if (donor == null)
            {
                Main.Mod.Logger.Log("EVP: FAILED: donor leopard feature not found.");
                return null;
            }

            var feature = BlueprintDeepClone.CloneFeatureIsolated(
                donor,
                BlueprintLibrary.NormalizeGuid(Guids.BlueprintGuids.MaximusLeopardFeatureGuid));

            feature.name = "EvocationPlus_AnimalCompanionFeatureMaximus";

            // ✅ Selector text + tooltip
            UnitFactStringUtils.SetUnitFactStrings(
                feature,
                "EVP_MAXIMUS_COMPANION_NAME",
                "EVP_MAXIMUS_COMPANION_DESC"
            );

            // ✅ Optional but recommended: distinct icon in the list
            var icon = DiskIconLoader.LoadSprite("max_portrait.png"); // or reuse familiar_leopard.png
            if (icon != null)
                BlueprintUnitFactUI.SetIcon(feature, icon);
            else
                Main.Mod.Logger.Log("EVP: FAILED: maximus_companion.png could not be loaded.");

            // ✅ Make sure AddPet points to the new unit
            var addPet = feature.GetComponent<Kingmaker.UnitLogic.FactLogic.AddPet>();
            if (addPet == null)
            {
                Main.Mod.Logger.Log("EVP: FAILED: Maximus feature clone has no AddPet component.");
                return null;
            }

            addPet.Pet = maximusUnit;
            // Keep vanilla growth behavior, but add +6 DEX on top by cloning the UpgradeFeature.
            if (addPet.UpgradeFeature == null)
            {
                Main.Mod.Logger.Log("EVP: FAILED: donor leopard AddPet has no UpgradeFeature.");
                return null;
            }

            var maximusUpgrade = EnsureMaximusUpgradeFeatureWithDex(library, addPet.UpgradeFeature);
            if (maximusUpgrade == null) return null;

            addPet.UpgradeFeature = maximusUpgrade;

            addPet.UpgradeFeature = maximusUpgrade;
            // keep the same UpgradeLevel from donor (do NOT change unless you want a different trigger level)
            BlueprintLibrary.Register(library, Guids.BlueprintGuids.MaximusLeopardFeatureGuid, feature);
            return feature;
        }
      
        private static BlueprintFeature EnsureMaximusUpgradeFeatureWithDex(
            LibraryScriptableObject library,
            BlueprintFeature donorUpgrade)
        {
            var existing =
                BlueprintLibrary.GetBlueprint(library, Guids.BlueprintGuids.MaximusLeopardUpgradeFeatureGuid) as BlueprintFeature;
            if (existing != null) return existing;

            var upgrade = BlueprintDeepClone.CloneFeatureIsolated(
                donorUpgrade,
                BlueprintLibrary.NormalizeGuid(Guids.BlueprintGuids.MaximusLeopardUpgradeFeatureGuid));

            upgrade.name = "EvocationPlus_MaximusLeopardUpgradeFeature";

            // Add +6 DEX on top of vanilla growth (size change etc.)
            var dexBonus = upgrade.AddComponent<AddStatBonus>(); 
            dexBonus.Stat = StatType.Dexterity;
            dexBonus.Value = 6;
            dexBonus.Descriptor = ModifierDescriptor.UntypedStackable;

            BlueprintLibrary.Register(library, Guids.BlueprintGuids.MaximusLeopardUpgradeFeatureGuid, upgrade);
            return upgrade;
        }
        
        private static void AddMaximusToAllSelectionsThatContainLeopard(LibraryScriptableObject library,
            BlueprintFeature maximusFeature)
        {
            var allSelections = ResourcesLibrary.LibraryObject?.GetAllBlueprints()?.OfType<BlueprintFeatureSelection>();
            if (allSelections == null) return;

            foreach (var sel in allSelections)
            {
                if (sel == null) continue;

                var all = sel.AllFeatures ?? Array.Empty<BlueprintFeature>();

                bool hasLeopard = all.Any(f =>
                    f != null &&
                    string.Equals(f.AssetGuid.ToString(), Guids.BlueprintGuids.LeopardFeatureGuid,
                        StringComparison.OrdinalIgnoreCase));

                if (!hasLeopard) continue;

                bool hasMaximus = all.Any(f =>
                    f != null &&
                    string.Equals(f.AssetGuid.ToString(), maximusFeature.AssetGuid.ToString(),
                        StringComparison.OrdinalIgnoreCase));

                if (hasMaximus) continue;

                sel.AllFeatures = all.Concat(new[] { maximusFeature }).ToArray();

                if (sel.Features != null && sel.Features.Length > 0)
                    sel.Features = sel.Features.Concat(new[] { maximusFeature }).ToArray();
            }
        }

        private static void ApplyMaximusPortrait(LibraryScriptableObject library, BlueprintUnit unit)
        {
            var sprite = DiskIconLoader.LoadSprite("maximus.png");
            if (sprite == null)
            {
                Main.Mod.Logger.Log("EVP: FAILED: maximus.png could not be loaded.");
                return;
            }

            var portraitField =
                typeof(BlueprintUnit).GetField("m_Portrait", BindingFlags.Instance | BindingFlags.NonPublic);
            if (portraitField == null)
            {
                Main.Mod.Logger.Log("EVP: FAILED: Could not find BlueprintUnit.m_Portrait field via reflection.");
                return;
            }

            var currentPortrait = portraitField.GetValue(unit) as BlueprintPortrait;
            var proto = currentPortrait
                        ?? BlueprintRoot.Instance?.UIRoot?.MalePlaceholderPortrait
                        ?? BlueprintRoot.Instance?.UIRoot?.FemalePlaceholderPortrait;

            if (proto == null)
            {
                Main.Mod.Logger.Log("EVP: FAILED: Could not find portrait prototype.");
                return;
            }

            var existing =
                BlueprintLibrary.GetBlueprint(library, Guids.BlueprintGuids.MaximusLeopardPortraitGuid) as
                    BlueprintPortrait;
            var newPortrait = existing;
            if (newPortrait == null)
            {
                newPortrait = Object.Instantiate(proto);
                newPortrait.name = "EvocationPlus_MaximusLeopardPortrait";
                newPortrait.AssetGuid = BlueprintLibrary.NormalizeGuid(Guids.BlueprintGuids.MaximusLeopardPortraitGuid);

                newPortrait.Data = new PortraitData(null, sprite, sprite, sprite);
                BlueprintLibrary.Register(library, Guids.BlueprintGuids.MaximusLeopardPortraitGuid, newPortrait);
            }

            portraitField.SetValue(unit, newPortrait);
        }

        private static void ApplyMaximusName(BlueprintUnit unit)
        {
            const string key = "EVP_MAXIMUS_NAME";

            var keyField = typeof(LocalizedString).GetField("m_Key", BindingFlags.Instance | BindingFlags.NonPublic);
            if (keyField == null)
            {
                Main.Mod.Logger.Log("EVP: FAILED: Could not reflect LocalizedString.m_Key for Maximus name.");
                return;
            }

            var shared = ScriptableObject.CreateInstance<SharedStringAsset>();
            var ls = new LocalizedString();
            keyField.SetValue(ls, key);
            ls.ShouldProcess = false;

            shared.String = ls;
            unit.LocalizedName = shared;
        }
    }
}