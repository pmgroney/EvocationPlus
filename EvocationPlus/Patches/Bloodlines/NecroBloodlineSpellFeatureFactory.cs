using System;
using System.Linq;
using EvocationPlus.BlueprintUtils;
using EvocationPlus.Core;
using EvocationPlus.Utils;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.FactLogic;
using Object = UnityEngine.Object;

namespace EvocationPlus.Patches.Bloodlines
{
    internal static class NecroBloodlineSpellFeatureFactory
    {
        internal static BlueprintFeature EnsureBoneSpikeBloodlineSpellFeature(
            LibraryScriptableObject library,
            BlueprintCharacterClass sorcererClass)
        {
            var existing = BlueprintLibrary.GetBlueprint(library, Guids.Spells.BoneSpikeFeature) as BlueprintFeature;
            if (existing != null) return existing;

            var donor = BlueprintLibrary.GetBlueprint(library,
                BloodlineGuids.DonorUndeadBloodlineSpellLevel3FeatureGuid) as BlueprintFeature;
            if (donor == null)
            {
                Main.Mod.Logger.Log("EVP: donor bloodline spell feature (Cause Fear) not found: " +
                                    BloodlineGuids.DonorUndeadBloodlineSpellLevel3FeatureGuid);
                return null;
            }

            var boneSpike = BlueprintLibrary.GetBlueprint(library, Guids.Spells.BoneSpike) as BlueprintAbility;
            if (boneSpike == null)
            {
                Main.Mod.Logger.Log("EVP: Bone Spike spell blueprint not found: " + Guids.Spells.BoneSpike);
                return null;
            }

            return CloneKnownSpellFeature(
                library,
                donor,
                sorcererClass,
                boneSpike,
                Guids.Spells.BoneSpikeFeature,
                "EvocationPlus_BloodlineSpell_BoneSpike",
                "EVP_BoneSpike_Name",
                "EVP_BoneSpike_Desc");
        }

        internal static BlueprintFeature EnsureCorpseExplosionBloodlineSpellFeature(
            LibraryScriptableObject library,
            BlueprintCharacterClass sorcererClass)
        {
            var existing =
                BlueprintLibrary.GetBlueprint(library, Guids.Spells.CorpseExplosionFeature) as BlueprintFeature;
            if (existing != null) return existing;

            var donor = BlueprintLibrary.GetBlueprint(library,
                BloodlineGuids.DonorUndeadBloodlineSpellLevel5FeatureGuid) as BlueprintFeature;
            if (donor == null)
            {
                Main.Mod.Logger.Log("EVP: donor L5 bloodline spell feature not found: " +
                                    BloodlineGuids.DonorUndeadBloodlineSpellLevel5FeatureGuid);
                return null;
            }

            var spell = BlueprintLibrary.GetBlueprint(library, Guids.Spells.CorpseExplosion) as BlueprintAbility;
            if (spell == null)
            {
                Main.Mod.Logger.Log("EVP: Corpse Explosion spell blueprint not found: " + Guids.Spells.CorpseExplosion);
                return null;
            }

            return CloneKnownSpellFeature(
                library,
                donor,
                sorcererClass,
                spell,
                Guids.Spells.CorpseExplosionFeature,
                "EvocationPlus_BloodlineSpell_CorpseExplosion",
                "EVP_CorpseExplosion_Name",
                "EVP_CorpseExplosion_Desc");
        }

        internal static BlueprintFeature EnsureEldritchHorrorBloodlineSpellFeature(
            LibraryScriptableObject library,
            BlueprintCharacterClass sorcererClass)
        {
            var existing =
                BlueprintLibrary.GetBlueprint(library, Guids.Spells.EldritchHorrorFeature) as BlueprintFeature;
            if (existing != null) return existing;

            var donor = BlueprintLibrary.GetBlueprint(library,
                BloodlineGuids.DonorUndeadBloodlineSpellLevel7FeatureGuid) as BlueprintFeature;
            if (donor == null)
            {
                Main.Mod.Logger.Log("EVP: donor L7 bloodline spell feature not found: " +
                                    BloodlineGuids.DonorUndeadBloodlineSpellLevel7FeatureGuid);
                return null;
            }

            var spell = BlueprintLibrary.GetBlueprint(library, Guids.Spells.EldritchHorror) as BlueprintAbility;
            if (spell == null)
            {
                Main.Mod.Logger.Log("EVP: Eldritch Horror spell blueprint not found: " + Guids.Spells.EldritchHorror);
                return null;
            }

            return CloneKnownSpellFeature(
                library,
                donor,
                sorcererClass,
                spell,
                Guids.Spells.EldritchHorrorFeature,
                "EvocationPlus_BloodlineSpell_EldritchHorror",
                "EVP_EldritchHorror_Name",
                "EVP_EldritchHorror_Desc");
        }

        internal static BlueprintFeature EnsureHellOnEarthBloodlineSpellFeature(
            LibraryScriptableObject library,
            BlueprintCharacterClass sorcererClass)
        {
            var existing = BlueprintLibrary.GetBlueprint(library, Guids.Spells.HellOnEarthFeature) as BlueprintFeature;
            if (existing != null) return existing;

            var donor = BlueprintLibrary.GetBlueprint(library,
                BloodlineGuids.DonorArcaneBloodlineSpellLevel19FeatureGuid) as BlueprintFeature;
            if (donor == null)
            {
                Main.Mod.Logger.Log("EVP: donor L19 bloodline spell feature not found: " +
                                    BloodlineGuids.DonorArcaneBloodlineSpellLevel19FeatureGuid);
                return null;
            }

            var spell = BlueprintLibrary.GetBlueprint(library, Guids.Spells.HellOnEarth) as BlueprintAbility;
            if (spell == null)
            {
                Main.Mod.Logger.Log("EVP: Hell on Earth spell blueprint not found: " + Guids.Spells.HellOnEarth);
                return null;
            }

            return CloneKnownSpellFeature(
                library,
                donor,
                sorcererClass,
                spell,
                Guids.Spells.HellOnEarthFeature,
                "EvocationPlus_BloodlineSpell_HellOnEarth",
                "EVP_HELL_ON_EARTH_NAME",
                "EVP_HELL_ON_EARTH_DESC");
        }

        private static BlueprintFeature CloneKnownSpellFeature(
            LibraryScriptableObject library,
            BlueprintFeature donor,
            BlueprintCharacterClass sorcererClass,
            BlueprintAbility spell,
            string newFeatureGuid,
            string newName,
            string nameKey,
            string descKey)
        {
            var clone = Object.Instantiate(donor);
            clone.name = newName;
            clone.AssetGuid = BlueprintLibrary.NormalizeGuid(newFeatureGuid);

            var aksOld = clone.ComponentsArray != null
                ? clone.ComponentsArray.OfType<AddKnownSpell>().ToArray()
                : Array.Empty<AddKnownSpell>();
            if (aksOld.Length == 0)
            {
                Main.Mod.Logger.Log("EVP: donor bloodline spell feature has no AddKnownSpell component(s).");
                return null;
            }

            foreach (var oldComp in aksOld)
            {
                var newComp = Object.Instantiate(oldComp);

                var comps = clone.ComponentsArray;
                for (var i = 0; i < comps.Length; i++)
                {
                    if (ReferenceEquals(comps[i], oldComp))
                    {
                        comps[i] = newComp;
                        break;
                    }
                }

                clone.ComponentsArray = comps;

                newComp.CharacterClass = sorcererClass;
                newComp.SpellLevel = oldComp.SpellLevel;
                newComp.Spell = spell;
            }

            EvocationPlusUnitFactText.SetNameKey(clone, nameKey);
            EvocationPlusUnitFactText.SetDescriptionKey(clone, descKey);

            if (spell != null && spell.Icon != null)
                ReflectionUtils.SetPrivateField(clone, "m_Icon", spell.Icon);

            BlueprintLibrary.Register(library, newFeatureGuid, clone);
            return clone;
        }
    }
}