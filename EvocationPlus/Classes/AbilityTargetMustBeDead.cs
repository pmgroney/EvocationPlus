using Kingmaker.Blueprints;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.Utility;

namespace EvocationPlus.Classes
{
    public sealed class AbilityTargetMustBeDead : BlueprintComponent, IAbilityTargetChecker
    {
        public bool CanTarget(UnitEntityData caster, TargetWrapper target)
        {
            var unit = target.Unit;
            return unit != null && unit.Descriptor != null && unit.Descriptor.State.IsDead;
        }
    }
}