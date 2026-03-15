using EvocationPlus.BlueprintUtils;
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
            if (existing != null) return existing;

            var src = BlueprintLibrary.GetBlueprint(library, BloodlineGuids.BaseUndeadProgressionGuid) as BlueprintProgression;
            if (src == null)
            {
                Main.Mod.Logger.Log("EVP: base undead bloodline progression not found.");
                return null;
            }

            var clone = Object.Instantiate(src);
            clone.name = "EvocationPlus_NecroBloodlineProgression";
            clone.AssetGuid = BlueprintLibrary.NormalizeGuid(BloodlineGuids.ClonedNecroProgressionGuid);

            EvocationPlusUnitFactText.SetDescriptionKey(clone, "EVP_UNDEAD_BLOODLINE_DESC");
            EvocationPlusUnitFactText.SetNameKey(clone, "EVP_UNDEAD_BLOODLINE_NAME");

            // L3 bloodline spell swap (Cause Fear -> Bone Spike)
            var boneSpikeFeature = NecroBloodlineSpellFeatureFactory.EnsureBoneSpikeBloodlineSpellFeature(library, sorcererClass);
            if (boneSpikeFeature != null)
            {
                ProgressionSwapUtil.ReplaceInProgression(clone, BloodlineGuids.DonorUndeadBloodlineSpellLevel3FeatureGuid, boneSpikeFeature);
                ProgressionSwapUtil.ReplaceInUiGroups(clone, BloodlineGuids.DonorUndeadBloodlineSpellLevel3FeatureGuid, boneSpikeFeature);
            }
            else
            {
                Main.Mod.Logger.Log("EVP: Bone Spike bloodline spell feature not created; leaving Cause Fear.");
            }

            // Grave Touch selection -> Necro Ray feature
            var necroRayFeature = NecroRayFeatureBuilder.EnsureNecroRayFeature(library);
            if (necroRayFeature != null)
            {
                ProgressionSwapUtil.ReplaceInProgression(clone, BloodlineGuids.DonorGraveTouchFeatureGuid, necroRayFeature);
                ProgressionSwapUtil.ReplaceInUiGroups(clone, BloodlineGuids.DonorGraveTouchFeatureGuid, necroRayFeature);
            }
            else
            {
                Main.Mod.Logger.Log("EVP: necro ray donor not available; leaving Grave Touch as-is.");
            }

            // L5 swap -> Corpse Explosion
            var corpseExplosionFeature = NecroBloodlineSpellFeatureFactory.EnsureCorpseExplosionBloodlineSpellFeature(library, sorcererClass);
            if (corpseExplosionFeature != null)
            {
                ProgressionSwapUtil.ReplaceInProgression(clone, BloodlineGuids.DonorUndeadBloodlineSpellLevel5FeatureGuid, corpseExplosionFeature);
                ProgressionSwapUtil.ReplaceInUiGroups(clone, BloodlineGuids.DonorUndeadBloodlineSpellLevel5FeatureGuid, corpseExplosionFeature);
            }

            // L7 swap -> Eldritch Horror
            var eldritchHorrorFeature = NecroBloodlineSpellFeatureFactory.EnsureEldritchHorrorBloodlineSpellFeature(library, sorcererClass);
            if (eldritchHorrorFeature != null)
            {
                ProgressionSwapUtil.ReplaceInProgression(clone, BloodlineGuids.DonorUndeadBloodlineSpellLevel7FeatureGuid, eldritchHorrorFeature);
                ProgressionSwapUtil.ReplaceInUiGroups(clone, BloodlineGuids.DonorUndeadBloodlineSpellLevel7FeatureGuid, eldritchHorrorFeature);
            }

            // L19 swap -> Hell on Earth
            var hellOnEarthFeature = NecroBloodlineSpellFeatureFactory.EnsureHellOnEarthBloodlineSpellFeature(library, sorcererClass);
            if (hellOnEarthFeature != null)
            {
                ProgressionSwapUtil.ReplaceInProgression(clone, BloodlineGuids.DonorArcaneBloodlineSpellLevel19FeatureGuid, hellOnEarthFeature);
                ProgressionSwapUtil.ReplaceInUiGroups(clone, BloodlineGuids.DonorArcaneBloodlineSpellLevel19FeatureGuid, hellOnEarthFeature);
            }
            
            BlueprintLibrary.Register(library, BloodlineGuids.ClonedNecroProgressionGuid, clone);

            BloodlineSelectionReplacer.ReplaceBloodlineInSelection(library);

            return clone;
        }
    }
}