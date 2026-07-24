using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.EntitySystem.Stats;

namespace EvocationPlus.Archetypes
{
    public class HarbingerOfBoomArcanaClassSpells :
        OwnedGameLogicComponent<UnitDescriptor>,
        IInitiatorRulebookHandler<RuleCalculateDamage>
    {
        public void OnEventAboutToTrigger(RuleCalculateDamage evt)
        {
            var context = evt.Reason.Context;

            // Must be a spell ability
            if (context?.SourceAbility == null || !context.SourceAbility.IsSpell)
                return;

            // Must be Evocation school
            if (context.SourceAbility.School != SpellSchool.Evocation)
                return;

            // Add CHA bonus to total damage (per damage instance in bundle)
            var cha = Owner.Stats?.GetStat<ModifiableValueAttributeStat>(StatType.Charisma)?.Bonus ?? 0;
            if (cha <= 0)
                return;

            var firstDamage = evt.DamageBundle.FirstOrDefault();
            firstDamage?.AddBonus(cha);
        }

        public void OnEventDidTrigger(RuleCalculateDamage evt) { }
    }
}