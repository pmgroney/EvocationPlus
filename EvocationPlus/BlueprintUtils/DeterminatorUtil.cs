using System;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;

namespace EvocationPlus.BlueprintUtils
{
    internal static class DeterminatorUtil
    {
        // Use for archetype/class features (shows on the class progression UI)
        internal static void AddToClassProgression(BlueprintCharacterClass parentClass, BlueprintFeatureBase feature)
        {
            if (parentClass?.Progression == null) return;
            AddToProgression(parentClass.Progression, feature);
        }

        // Use for bloodlines / cloned progressions (shows on that progression’s UI)
        internal static void AddToProgression(BlueprintProgression progression, BlueprintFeatureBase feature)
        {
            if (progression == null || feature == null) return;

            var group = progression.UIDeterminatorsGroup ?? Array.Empty<BlueprintFeatureBase>();
            if (group.Contains(feature)) return;

            progression.UIDeterminatorsGroup = group.Concat(new[] { feature }).ToArray();
        }
    }
}