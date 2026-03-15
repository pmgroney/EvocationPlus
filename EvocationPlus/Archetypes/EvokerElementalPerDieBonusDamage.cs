using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Enums.Damage;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic;
using UnityEngine.Serialization;

namespace EvocationPlus.Archetypes
{
    /// <summary>
    /// Adds +Rank bonus damage per die, but only to matching energy damage instances (DamageEnergyType).
    /// Rank comes from the owning fact (feature) rank.
    /// </summary>
    public class EvokerElementalPerDieBonusDamage :
        OwnedGameLogicComponent<UnitDescriptor>,
        IInitiatorRulebookHandler<RuleCalculateDamage>
    {
        [FormerlySerializedAs("Classes")]
        public BlueprintCharacterClass[] classes;

        public DamageEnergyType EnergyType;

        public void OnEventAboutToTrigger(RuleCalculateDamage evt)
        {
            var context = evt.Reason.Context;

            // Must be a spell ability
            if (context?.SourceAbility == null || !context.SourceAbility.IsSpell)
                return;

            // Must be coming from one of the allowed class spellbooks (sorcerer)
            var spellbook = context.SourceAbilityContext?.Ability?.Spellbook;
            if (spellbook == null)
                return;

            var classSpellbook = MasterOfDeathArcanaClassSpells.GetClassSpellbook(spellbook, Owner);

            var ok = false;
            foreach (var characterClass in classes)
            {
                if (Owner.GetSpellbook(characterClass) == classSpellbook)
                {
                    ok = true;
                    break;
                }
            }
            if (!ok) return;

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