using System.Collections.Generic;
using System.Linq;
using EvocationPlus.Archetypes;
using EvocationPlus.BlueprintUtils;
using EvocationPlus.Core;
using EvocationPlus.Utils;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.UnitLogic.FactLogic;
using Object = UnityEngine.Object;

namespace EvocationPlus.Patches.Bloodlines
{
    internal static class EvokerProgressions
    {
        internal static BlueprintProgression EnsureArcane(LibraryScriptableObject library)
        {
            var prog = EnsureFromDonor(
                library,
                BloodlineGuids.DonorArcaneProgressionGuid,
                BloodlineGuids.EvokerArcaneProgressionGuid,
                "EvocationPlus_EvokerBloodline_Arcane",
                "EVP_EVOKER_ARCANE_NAME",
                "EVP_EVOKER_ARCANE_DESC");

            if (prog == null)
            {
                Main.Mod.Logger.Log("EVP: EnsureArcane failed: prog null (donor missing or clone/register failed).");
                return null;
            }

            var sorc =
                BlueprintLibrary.GetBlueprint(library, Guids.Features.SorcererClassGuid) as BlueprintCharacterClass;
            if (sorc != null)
                EvokerElementalScalingInstaller.ApplyArcane(library, prog, sorc);
            else
                Main.Mod.Logger.Log("EVP: EnsureArcane: Sorcerer class not found (scaling skipped).");
            RemoveArcaneSchoolPowerSelection(prog);

            var forceBeamFeat = ForceBeamFeatureBuilder.EnsureForceBeamFeature(library);
            if (forceBeamFeat != null)
            {
                ProgressionSwapUtil.ReplaceInProgression(prog, Guids.Features.ArcaneArcaneBondFeatureGuid,
                    forceBeamFeat);
                ProgressionSwapUtil.ReplaceInUiGroups(prog, Guids.Features.ArcaneArcaneBondFeatureGuid, forceBeamFeat);
            }

            var forceRayFeat = ArcaneBloodlineSpellFeatureFactory.EnsureForceRayBloodlineSpellFeature(library, sorc);
            if (forceRayFeat != null)
            {
                ProgressionSwapUtil.ReplaceInProgression(prog,
                    BloodlineGuids.DonorArcaneBloodlineSpellLevel5FeatureGuid, forceRayFeat);
                ProgressionSwapUtil.ReplaceInUiGroups(prog, BloodlineGuids.DonorArcaneBloodlineSpellLevel5FeatureGuid,
                    forceRayFeat);
            }
            else
            {
                Main.Mod.Logger.Log("EVP: EnsureArcane: ForceBeam feature null (swap skipped).");
            }

            var forceBlastFeat = ArcaneBloodlineSpellFeatureFactory.EnsureElementalForceBlastFeature(
                library,
                sorc,
                BloodlineGuids.DonorArcaneBloodlineSpellLevel9FeatureGuid,
                BloodlineGuids.ElementalFireElementalBlastFeature,
                BloodlineGuids.EvpArcaneSpellLevel4Guid,
                "EVP_ARCANE_FORCE_BLAST_NAME",
                "EVP_ARCANE_FORCE_BLAST_DESC"
            );

            if (forceBlastFeat != null)
            {
                ProgressionSwapUtil.ReplaceInProgression(prog,
                    BloodlineGuids.DonorArcaneBloodlineSpellLevel9FeatureGuid, forceBlastFeat);
                ProgressionSwapUtil.ReplaceInUiGroups(prog, BloodlineGuids.DonorArcaneBloodlineSpellLevel9FeatureGuid,
                    forceBlastFeat);
            }
            else
            {
                Main.Mod.Logger.Log("Force Blast swap failed!");
            }

            AddProtectionFromEnergyCommunal(library, prog, sorc);

            return prog;
        }

        internal static BlueprintProgression EnsureAir(LibraryScriptableObject library)
        {
            var prog = EnsureFromDonor(
                library,
                BloodlineGuids.DonorAirProgressionGuid,
                BloodlineGuids.EvokerAirProgressionGuid,
                "EvocationPlus_EvokerBloodline_Air",
                "EVP_EVOKER_AIR_NAME",
                "EVP_EVOKER_AIR_DESC");

            var sorc =
                BlueprintLibrary.GetBlueprint(library, Guids.Features.SorcererClassGuid) as BlueprintCharacterClass;
            if (prog != null && sorc != null)
                EvokerElementalScalingInstaller.ApplyAir(library, prog, sorc);

            AddProtectionFromEnergyCommunal(library, prog, sorc);

            return prog;
        }

        internal static BlueprintProgression EnsureEarth(LibraryScriptableObject library)
        {
            var prog = EnsureFromDonor(
                library,
                BloodlineGuids.DonorEarthProgressionGuid,
                BloodlineGuids.EvokerEarthProgressionGuid,
                "EvocationPlus_EvokerBloodline_Earth",
                "EVP_EVOKER_EARTH_NAME",
                "EVP_EVOKER_EARTH_DESC");

            var sorc =
                BlueprintLibrary.GetBlueprint(library, Guids.Features.SorcererClassGuid) as BlueprintCharacterClass;
            if (prog != null && sorc != null)
                EvokerElementalScalingInstaller.ApplyEarth(library, prog, sorc);

            AddProtectionFromEnergyCommunal(library, prog, sorc);

            return prog;
        }

        internal static BlueprintProgression EnsureFire(LibraryScriptableObject library)
        {
            var prog = EnsureFromDonor(
                library,
                BloodlineGuids.DonorFireProgressionGuid,
                BloodlineGuids.EvokerFireProgressionGuid,
                "EvocationPlus_EvokerBloodline_Fire",
                "EVP_EVOKER_FIRE_NAME",
                "EVP_EVOKER_FIRE_DESC");

            var sorc =
                BlueprintLibrary.GetBlueprint(library, Guids.Features.SorcererClassGuid) as BlueprintCharacterClass;
            if (prog != null && sorc != null)
                EvokerElementalScalingInstaller.ApplyFire(library, prog, sorc);

            AddProtectionFromEnergyCommunal(library, prog, sorc);

            return prog;
        }

        internal static BlueprintProgression EnsureWater(LibraryScriptableObject library)
        {
            var prog = EnsureFromDonor(
                library,
                BloodlineGuids.DonorWaterProgressionGuid,
                BloodlineGuids.EvokerWaterProgressionGuid,
                "EvocationPlus_EvokerBloodline_Water",
                "EVP_EVOKER_WATER_NAME",
                "EVP_EVOKER_WATER_DESC");

            var sorc =
                BlueprintLibrary.GetBlueprint(library, Guids.Features.SorcererClassGuid) as BlueprintCharacterClass;
            if (prog != null && sorc != null)
                EvokerElementalScalingInstaller.ApplyWater(library, prog, sorc);

            AddProtectionFromEnergyCommunal(library, prog, sorc);

            return prog;
        }

        private static void AddProtectionFromEnergyCommunal(
            LibraryScriptableObject library,
            BlueprintProgression prog,
            BlueprintCharacterClass sorc)
        {
            if (prog == null || sorc == null) return;

            var feature = SharedSpellGrantFeatureFactory.EnsureProtectionFromEnergyCommunalSpellFeature(library, sorc);
            if (feature == null) return;

            AddFeatureAtLevel(prog, feature, 8);
            NormalizeSpellGrantOrdering(prog);
            BloodlineUiGroupUtil.NormalizeSpellRow(prog);
        }

        private static BlueprintProgression EnsureFromDonor(
            LibraryScriptableObject library,
            string donorGuid,
            string newGuid,
            string internalName,
            string nameKey,
            string descKey)
        {
            var existing = BlueprintLibrary.GetBlueprint(library, newGuid) as BlueprintProgression;
            if (existing != null) return existing;

            var donorObj = BlueprintLibrary.GetBlueprint(library, donorGuid);
            if (donorObj == null)
            {
                Main.Mod.Logger.Log("EVP: Evoker donor bloodline progression not found: " + donorGuid);
                return null;
            }

            var donor = donorObj as BlueprintProgression;
            if (donor == null)
            {
                Main.Mod.Logger.Log("EVP: donorGuid was not a BlueprintProgression: " + donorGuid);
                return null;
            }

            var clone = Object.Instantiate(donor);
            clone.name = internalName;
            clone.AssetGuid = BlueprintLibrary.NormalizeGuid(newGuid);

            // Keep it categorized as a bloodline
            clone.Groups = new[] { FeatureGroup.BloodLine };
            clone.IsClassFeature = true;

            // Deep copy UIGroups (only if donor has them)
            if (donor.UIGroups.Length > 0)
            {
                clone.UIGroups = donor.UIGroups.Select(g => new UIGroup
                {
                    Features = g?.Features == null
                        ? new List<BlueprintFeatureBase>()
                        : new List<BlueprintFeatureBase>(g.Features)
                }).ToArray();
            }

            // Deep copy determinators (only if donor has them)
            if (donor.UIDeterminatorsGroup.Length > 0)
                clone.UIDeterminatorsGroup = donor.UIDeterminatorsGroup.ToArray();

            // Deep copy LevelEntries
            clone.LevelEntries = donor.LevelEntries?
                .Select(e => new LevelEntry
                {
                    Level = e.Level,
                    Features = new List<BlueprintFeatureBase>(e.Features)
                })
                .ToArray();

            NormalizeSpellGrantOrdering(clone);


            // UI strings
            EvocationPlusUnitFactText.SetNameKey(clone, nameKey);
            EvocationPlusUnitFactText.SetDescriptionKey(clone, descKey);

            // Remove prereqs that reference archetypes
            EvokerBloodlineSelectionInstaller.StripArchetypePrereqs(clone);

            BlueprintLibrary.Register(library, newGuid, clone);
            return clone;
        }

        private static void AddFeatureAtLevel(
            BlueprintProgression prog,
            BlueprintFeatureBase feature,
            int level)
        {
            if (prog == null || feature == null) return;

            var entries = (prog.LevelEntries ?? new LevelEntry[0]).ToList();
            var entry = entries.FirstOrDefault(e => e != null && e.Level == level);
            if (entry == null)
            {
                entry = new LevelEntry
                {
                    Level = level,
                    Features = new List<BlueprintFeatureBase>()
                };
                entries.Add(entry);
            }

            if (entry.Features == null)
                entry.Features = new List<BlueprintFeatureBase>();

            var featureGuid = BlueprintLibrary.NormalizeGuid(feature.AssetGuid);
            if (entry.Features.Any(f => f != null && BlueprintLibrary.NormalizeGuid(f.AssetGuid) == featureGuid))
                return;

            entry.Features.Add(feature);
            prog.LevelEntries = entries.OrderBy(e => e?.Level ?? 0).ToArray();
        }

        private static void NormalizeSpellGrantOrdering(BlueprintProgression prog)
        {
            if (prog?.LevelEntries == null) return;

            foreach (var entry in prog.LevelEntries)
            {
                if (entry?.Features == null || entry.Features.Count <= 1) continue;

                entry.Features = entry.Features
                    .OrderByDescending(IsSpellGrantFeature)
                    .ToList();
            }
        }

        private static bool IsSpellGrantFeature(BlueprintFeatureBase f)
        {
            var feature = f as BlueprintFeature;
            if (feature == null) return false;

            var adds = feature.GetComponents<AddKnownSpell>();
            return adds != null && adds.Any();
        }

        private static void RemoveArcaneSchoolPowerSelection(BlueprintProgression prog)
        {
            if (prog == null) return;

            const string
                removeGuid = BloodlineGuids.BloodlineArcaneSchoolPowerSelection; // BloodlineArcaneSchoolPowerSelection

            // LevelEntries
            foreach (var entry in prog.LevelEntries)
            {
                if (entry?.Features == null) continue;
                entry.Features = entry.Features
                    .Where(f => f == null || f.AssetGuid.ToString() != removeGuid)
                    .ToList();
            }

            // UIGroups
            foreach (var group in prog.UIGroups)
            {
                if (group?.Features == null) continue;
                group.Features = group.Features
                    .Where(f => f == null || f.AssetGuid.ToString() != removeGuid)
                    .ToList();
            }

            // UIDeterminatorsGroup
            if (prog.UIDeterminatorsGroup.Length > 0)
            {
                prog.UIDeterminatorsGroup = prog.UIDeterminatorsGroup
                    .Where(f => f == null || f.AssetGuid.ToString() != removeGuid)
                    .ToArray();
            }
        }
    }
}
