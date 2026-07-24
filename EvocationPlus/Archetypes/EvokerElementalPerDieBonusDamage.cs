using Kingmaker.Blueprints;
using Kingmaker.Enums.Damage;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic;

namespace EvocationPlus.Archetypes
{
    /// <summary>
    /// Adds +Rank bonus damage per die, but only to matching energy damage instances (DamageEnergyType).
    /// Rank comes from the owning fact (feature) rank. Applies to any spell dealing the matching
    /// energy type, regardless of which class's spellbook it was cast from.
    /// </summary>
    public class EvokerElementalPerDieBonusDamage :
        OwnedGameLogicComponent<UnitDescriptor>,
        IInitiatorRulebookHandler<RuleCalculateDamage>
    {
        public DamageEnergyType EnergyType;

        public void OnEventAboutToTrigger(RuleCalculateDamage evt)
        {
            var context = evt.Reason.Context;

            // Must be a spell ability
            if (context?.SourceAbility == null || !context.SourceAbility.IsSpell)
                return;

            // Rank drives scaling (1..5)
            var rank = Fact?.GetRank() ?? 0;
            if (rank <= 0) return;

            // Apply +rank per die ONLY to matching energy damages
            foreach (var dmg in evt.DamageBundle)
            {
                var energy = dmg as EnergyDamage;
                if (energy == null) continue;
                if (energy.EnergyType != EnergyType) continue;

                // +rank per die: rolls * rank
                dmg.AddBonus(dmg.Dice.Rolls * rank);
            }
        }

        public void OnEventDidTrigger(RuleCalculateDamage evt) { }
    }
}