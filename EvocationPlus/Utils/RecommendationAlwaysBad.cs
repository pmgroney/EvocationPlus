using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Designers.Mechanics.Recommendations;
using Kingmaker.UnitLogic.Class.LevelUp;

namespace EvocationPlus.Utils
{
    // Always marks a feature as "Not Recommended" in level-up UI.
    // Intended for automatic granted spells where recommendation should always be thumbs-down.
    [AllowedOn(typeof(BlueprintFeature))]
    public class RecommendationAlwaysBad : LevelUpRecommendationComponent
    {
        public override RecommendationPriority GetPriority([CanBeNull] LevelUpState levelUpState)
            => RecommendationPriority.Bad;
    }
}