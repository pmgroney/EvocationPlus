using Kingmaker.Blueprints;
using Kingmaker.UnitLogic.Abilities.Blueprints;

namespace EvocationPlus.Patches
{
    public abstract class AllowDeadTargetingComponentBase : BlueprintComponent
    {
        public virtual bool Allow(BlueprintAbility ability)
        {
            return true;
        }
    }
}