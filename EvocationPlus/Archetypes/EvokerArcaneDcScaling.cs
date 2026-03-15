using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem.Rules.Abilities;
using Kingmaker.UnitLogic;
using UnityEngine.Serialization;

namespace EvocationPlus.Archetypes
{
    /// <summary>
    /// Adds +Rank to spell DC (untyped) so it stacks with Spell Focus.
    /// Rank comes from the owning feature's Rank (granted at 1/5/9/13/17).
    /// </summary>
    public class EvokerArcaneDcScaling :
        OwnedGameLogicComponent<UnitDescriptor>,
        IInitiatorRulebookHandler<RuleCalculateAbilityParams>
    {
        [FormerlySerializedAs("Classes")] public BlueprintCharacterClass[] classes;
        
        private static int GetMilestoneCount(int level)
        {
            int count = 0;
            if (level >= 1)  count++;
            if (level >= 5)  count++;
            if (level >= 9)  count++;
            if (level >= 13) count++;
            if (level >= 17) count++;
            return count;
        }
        public void OnEventAboutToTrigger(RuleCalculateAbilityParams evt)
        {
            if (evt == null) return;

            var spell = evt.Spell;
            if (spell == null || !spell.IsSpell) return;
            if (spell.School != SpellSchool.Evocation) return;

            var spellbook = evt.Spellbook;
            if (spellbook == null) return;

            var classSpellbook = MasterOfDeathArcanaClassSpells.GetClassSpellbook(spellbook, Owner);

            bool ok = false;
            foreach (var characterClass in classes)
            {
                if (Owner.GetSpellbook(characterClass) == classSpellbook)
                {
                    ok = true;
                    break;
                }
            }
            if (!ok) return;

            int milestones = GetMilestoneCount(Owner.Progression.CharacterLevel);
            int bonus = milestones * 2;
            if (bonus <= 0) return;

            evt.AddBonusDC(bonus);
        }

        public void OnEventDidTrigger(RuleCalculateAbilityParams evt) { }

    }
}