using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic;
using UnityEngine.Serialization;

namespace EvocationPlus.Archetypes
{
    public class MasterOfDeathArcanaClassSpells :
        OwnedGameLogicComponent<UnitDescriptor>,
        IInitiatorRulebookHandler<RuleCalculateDamage>,
        IRulebookHandler<RuleCalculateDamage>,
        IInitiatorRulebookSubscriber
    {
        // Which base classes' spellbooks should qualify (e.g., Sorcerer only)
        [FormerlySerializedAs("Classes")] public BlueprintCharacterClass[] classes;

        public void OnEventAboutToTrigger(RuleCalculateDamage evt)
        {
            var context = evt.Reason.Context;

            // Must be a spell ability
            if (context?.SourceAbility == null || !context.SourceAbility.IsSpell)
                return;

            // Must be Necromancy school
            if (context.SourceAbility.School != SpellSchool.Necromancy)
                return;

            // Must be coming from one of the allowed class spellbooks
            var spellbook = context.SourceAbilityContext?.Ability?.Spellbook;
            if (spellbook == null)
                return;

            var classSpellbook = GetClassSpellbook(spellbook, Owner);

            var ok = false;
            foreach (var characterClass in classes)
                if (Owner.GetSpellbook(characterClass) == classSpellbook)
                {
                    ok = true;
                    break;
                }

            if (!ok)
                return;

            // +1 damage per die 
            foreach (var baseDamage in evt.DamageBundle)
                baseDamage.AddBonus(baseDamage.Dice.Rolls);
        }

        public void OnEventDidTrigger(RuleCalculateDamage evt)
        {
        }

        public static Spellbook GetClassSpellbook(Spellbook spellbook, UnitDescriptor unit)
        {
            var spellbook1 = spellbook != null
                ? spellbook.Blueprint.GetComponent<GetKnownSpellsFromMemorizationSpellbook>()?.spellbook
                : null;
            return spellbook1 != null ? unit.GetSpellbook(spellbook1) : spellbook;
        }
    }

    public class GetKnownSpellsFromMemorizationSpellbook : BlueprintComponent
    {
        public BlueprintSpellbook spellbook;
    }
}