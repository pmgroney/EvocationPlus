using EvocationPlus.BlueprintUtils;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.UnitLogic.Abilities.Blueprints;

namespace EvocationPlus.IconUtils
{
    internal static class FeatureIconSync
    {
        // abilityGuid -> featureGuid
        private static readonly (string ability, string feature)[] Pairs =
        {
            ("b311c4d368bd4257916dd15b2438e804", "08fd4c579c7b429cb0bdce4c9a58ba4c"), // BoneSpike
            ("78133a1f0218401a8fd254e0923014f3", "2444632e058986248b9f9e2d57a9dfee") // CorpseExplosion
        };

        public static void Apply(LibraryScriptableObject library)
        {
            if (library == null) return;

            for (var i = 0; i < Pairs.Length; i++)
                SyncFeatureIconFromAbility(library, Pairs[i].ability, Pairs[i].feature);
        }

        private static void SyncFeatureIconFromAbility(LibraryScriptableObject library, string abilityGuid,
            string featureGuid)
        {
            var ability = BlueprintLibrary.GetBlueprint(library, abilityGuid) as BlueprintAbility;
            if (ability?.Icon == null) return;

            var feature = BlueprintLibrary.GetBlueprint(library, featureGuid) as BlueprintFeature;
            if (feature == null) return;

            BlueprintUnitFactUI.SetIcon(feature, ability.Icon);
        }
    }
}