using EvocationPlus.BlueprintUtils;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.UnitLogic.Abilities.Blueprints;

namespace EvocationPlus.Core
{
    internal static class ContentFixes
    {
        internal static void ApplyAll(LibraryScriptableObject library)
        {
            // Feature icon should match spell icon
            MatchFeatureIconToSpellIcon(
                library,
                Guids.Spells.BoneSpike, // Bone Spike spell
                Guids.Spells.BoneSpikeFeature // Bone Spike feature
            );

            MatchFeatureIconToSpellIcon(
                library,
                Guids.Spells.CorpseExplosion, // Corpse Explosion spell
                Guids.Spells.CorpseExplosionFeature // Corpse Explosion feature
            );
            
            MatchFeatureIconToSpellIcon(
                library,
                Guids.Spells.ForceRay, 
                Guids.Spells.ForceRayFeature
            );
        }

        private static void MatchFeatureIconToSpellIcon(
            LibraryScriptableObject library,
            string spellId,
            string featureId)
        {
            var spell = BlueprintLibrary.GetBlueprint(library, spellId) as BlueprintAbility;
            var feature = BlueprintLibrary.GetBlueprint(library, featureId) as BlueprintFeature;

            if (spell?.Icon != null && feature != null)
                BlueprintUnitFactUI.SetIcon(feature, spell.Icon);
        }
    }
}