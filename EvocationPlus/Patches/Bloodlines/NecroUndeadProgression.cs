using System.Collections.Generic;
using System.Linq;
using EvocationPlus.BlueprintUtils;
using EvocationPlus.Utils;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Object = UnityEngine.Object;

namespace EvocationPlus.Patches.Bloodlines
{
    public static class NecroUndeadProgression
    {
        public static BlueprintProgression EnsureNecroUndeadProgression(
            LibraryScriptableObject library,
            BlueprintCharacterClass sorcererClass)
        {
            var existing = BlueprintLibrary.GetBlueprint(library, BloodlineGuids.ClonedNecroProgressionGuid) as BlueprintProgression;
            if (existing != null)
            {
                PatchNecroProgression(library, existing, sorcererClass);
                BloodlineSelectionReplacer.ReplaceBloodlineInSelection(library);
                return existing;
            }

            var src = BlueprintLibrary.GetBlueprint(library, BloodlineGuids.BaseUndeadProgressionGuid) as BlueprintProgression;
            if (src == null)
            {
                Main.Mod.Logger.Log("EVP: base undead bloodline progression not found.");
                return null;
            }

            var clone = Object.Instantiate(src);
            clone.name = "EvocationPlus_NecroBloodlineProgression";
            clone.AssetGuid = BlueprintLibrary.NormalizeGuid(BloodlineGuids.ClonedNecroProgressionGuid);

            clone.LevelEntries = src.LevelEntries?
                .Select(e => new LevelEntry
                {
                    Level = e.Level,
                    Features = e.Features != null
                        ? new List<BlueprintFeatureBase>(e.Features)
                        : new List<BlueprintFeatureBase>()
                })
                .ToArray();

            clone.UIGroups = src.UIGroups?
                .Select(g => new UIGroup
                {
                    Features = g?.Features != null
                        ? new List<BlueprintFeatureBase>(g.Features)
                        : new List<BlueprintFeatureBase>()
                })
                .ToArray();

            clone.UIDeterminatorsGroup = src.UIDeterminatorsGroup?.ToArray();

            EvocationPlusUnitFactText.SetDescriptionKey(clone, "EVP_UNDEAD_BLOODLINE_DESC");
            EvocationPlusUnitFactText.SetNameKey(clone, "EVP_UNDEAD_BLOODLINE_NAME");

            PatchNecroProgression(library, clone, sorcererClass);

            BlueprintLibrary.Register(library, BloodlineGuids.ClonedNecroProgressionGuid, clone);

            BloodlineSelectionReplacer.ReplaceBloodlineInSelection(library);

            return clone;
        }

        private static void PatchNecroProgression(
            LibraryScriptableObject library,
            BlueprintProgression progression,
            BlueprintCharacterClass sorcererClass)
        {
            if (progression == null) return;

            // L3 bloodline spell swap (Cause Fear -> Bone Spike)
            var boneSpikeFeature = NecroBloodlineSpellFeatureFactory.EnsureBoneSpikeBloodlineSpellFeature(library, sorcererClass);
            if (boneSpikeFeature != null)
            {
                ProgressionSwapUtil.ReplaceInProgression(progression, BloodlineGuids.DonorUndeadBloodlineSpellLevel3FeatureGuid, boneSpikeFeature);
                ProgressionSwapUtil.ReplaceInUiGroups(progression, BloodlineGuids.DonorUndeadBloodlineSpellLevel3FeatureGuid, boneSpikeFeature);
            }
            else
            {
                Main.Mod.Logger.Log("EVP: Bone Spike bloodline spell feature not created; leaving Cause Fear.");
            }

            // Grave Touch selection -> Necro Ray feature
            var necroRayFeature = NecroRayFeatureBuilder.EnsureNecroRayFeature(library);
            if (necroRayFeature != null)
            {
                ProgressionSwapUtil.ReplaceInProgression(progression, BloodlineGuids.DonorGraveTouchFeatureGuid, necroRayFeature);
                ProgressionSwapUtil.ReplaceInUiGroups(progression, BloodlineGuids.DonorGraveTouchFeatureGuid, necroRayFeature);
            }
            else
            {
                Main.Mod.Logger.Log("EVP: necro ray donor not available; leaving Grave Touch as-is.");
            }

            // L5 swap -> Corpse Explosion
            var corpseExplosionFeature = NecroBloodlineSpellFeatureFactory.EnsureCorpseExplosionBloodlineSpellFeature(library, sorcererClass);
            if (corpseExplosionFeature != null)
            {
                ProgressionSwapUtil.ReplaceInProgression(progression, BloodlineGuids.DonorUndeadBloodlineSpellLevel5FeatureGuid, corpseExplosionFeature);
                ProgressionSwapUtil.ReplaceInUiGroups(progression, BloodlineGuids.DonorUndeadBloodlineSpellLevel5FeatureGuid, corpseExplosionFeature);
            }

            // L7 swap -> Eldritch Horror
            var eldritchHorrorFeature = NecroBloodlineSpellFeatureFactory.EnsureEldritchHorrorBloodlineSpellFeature(library, sorcererClass);
            if (eldritchHorrorFeature != null)
            {
                ProgressionSwapUtil.ReplaceInProgression(progression, BloodlineGuids.DonorUndeadBloodlineSpellLevel7FeatureGuid, eldritchHorrorFeature);
                ProgressionSwapUtil.ReplaceInUiGroups(progression, BloodlineGuids.DonorUndeadBloodlineSpellLevel7FeatureGuid, eldritchHorrorFeature);
            }

            // L19 swap -> Hell on Earth
            var hellOnEarthFeature = NecroBloodlineSpellFeatureFactory.EnsureHellOnEarthBloodlineSpellFeature(library, sorcererClass);
            if (hellOnEarthFeature != null)
            {
                ProgressionSwapUtil.ReplaceInProgression(progression, BloodlineGuids.DonorArcaneBloodlineSpellLevel19FeatureGuid, hellOnEarthFeature);
                ProgressionSwapUtil.ReplaceInUiGroups(progression, BloodlineGuids.DonorArcaneBloodlineSpellLevel19FeatureGuid, hellOnEarthFeature);
            }

            AddProtectionFromEnergyCommunal(library, progression, sorcererClass);
            BloodlineUiGroupUtil.NormalizeSpellRow(progression);
        }

        private static void AddProtectionFromEnergyCommunal(
            LibraryScriptableObject library,
            BlueprintProgression progression,
            BlueprintCharacterClass sorcererClass)
        {
            if (progression == null || sorcererClass == null) return;

            var feature =
                SharedSpellGrantFeatureFactory.EnsureProtectionFromEnergyCommunalSpellFeature(library, sorcererClass);
            if (feature == null) return;

            AddFeatureAtLevel(progression, feature, 8);
        }

        private static void AddFeatureAtLevel(
            BlueprintProgression progression,
            BlueprintFeatureBase feature,
            int level)
        {
            if (progression == null || feature == null) return;

            var entries = (progression.LevelEntries ?? new LevelEntry[0]).ToList();
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
            progression.LevelEntries = entries.OrderBy(e => e?.Level ?? 0).ToArray();
        }
    }
}
