// ...same usings...

using System;
using System.Linq;
using EvocationPlus.BlueprintUtils;
using EvocationPlus.Core;
using EvocationPlus.IconUtils;
using EvocationPlus.Patches.Bloodlines;
using EvocationPlus.Utils;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Selection;

namespace EvocationPlus.Archetypes
{
    internal static class NecroFamiliarSelectionBuilder
    {
        internal static BlueprintFeatureSelection EnsureNecroFamiliarSelection(LibraryScriptableObject library)
        {
            var existing = BlueprintLibrary.GetBlueprint(library, BloodlineGuids.NewNecroFamiliarSelectionGuid) as BlueprintFeatureSelection;
            if (existing != null) return existing;

            var donor = BlueprintLibrary.GetBlueprint(library, BloodlineGuids.DonorSylvanAnimalCompanionSelectionGuid) as BlueprintFeatureSelection;
            if (donor == null)
            {
                Main.Mod.Logger.Log("EVP: Sylvan animal companion selection donor not found.");
                return null;
            }

            var sel = BlueprintDeepClone.CloneFeatureSelectionIsolated(
                donor,
                BlueprintLibrary.NormalizeGuid(BloodlineGuids.NewNecroFamiliarSelectionGuid));

            sel.name = "EvocationPlus_NecroFamiliarSelection";

            UnitFactStringUtils.SetUnitFactStrings(sel, "EVP_NECRO_FAMILIAR_NAME", "EVP_NECRO_FAMILIAR_DESC");

            var icon = DiskIconLoader.LoadSprite("familiar_leopard.png");
            if (icon != null) BlueprintUnitFactUI.SetIcon(sel, icon);
            else Main.Mod.Logger.Log("EVP: familiar_leopard.png could not be loaded.");

            sel.IsClassFeature = true;
            sel.ReapplyOnLevelUp = false;
            sel.HideNotAvailibleInUI = true;

            // Ensure Maximus exists + injected everywhere (including Necro clone source issues)
            MaximusCompanionBuilder.EnsureMaximusCompanionEverywhere(library);

            // IMPORTANT: also inject directly into THIS cloned selection, so it's not order-dependent
            InjectMaximusIntoSelection(library, sel);

            BlueprintLibrary.Register(library, BloodlineGuids.NewNecroFamiliarSelectionGuid, sel);
            return sel;
        }

        private static void InjectMaximusIntoSelection(LibraryScriptableObject library, BlueprintFeatureSelection sel)
        {
            var maximus = BlueprintLibrary.GetBlueprint(library, Guids.BlueprintGuids.MaximusLeopardFeatureGuid) as BlueprintFeature;
            if (maximus == null)
            {
                Main.Mod.Logger.Log("EVP: FAILED: Maximus feature not found when patching Necro familiar selection.");
                return;
            }

            var all = sel.AllFeatures ?? Array.Empty<BlueprintFeature>();

            bool hasMaximus = all.Any(f =>
                f != null &&
                string.Equals(f.AssetGuid.ToString(), maximus.AssetGuid.ToString(), StringComparison.OrdinalIgnoreCase));

            if (!hasMaximus)
                sel.AllFeatures = all.Concat(new[] { maximus }).ToArray();

            if (sel.Features != null && sel.Features.Length > 0)
            {
                bool hasInFeatures = sel.Features.Any(f =>
                    f != null &&
                    string.Equals(f.AssetGuid.ToString(), maximus.AssetGuid.ToString(), StringComparison.OrdinalIgnoreCase));

                if (!hasInFeatures)
                    sel.Features = sel.Features.Concat(new[] { maximus }).ToArray();
            }
        }
    }
}