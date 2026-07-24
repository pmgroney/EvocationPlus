using EvocationPlus.BlueprintUtils;
using EvocationPlus.Core;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Spells;
using EvocationPlus.Patches.Bloodlines;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Mechanics;

namespace EvocationPlus.Archetypes
{
    public class MasterOfDeathArcanaClassSpells :
        OwnedGameLogicComponent<UnitDescriptor>,
        IInitiatorRulebookHandler<RuleCalculateDamage>,
        IRulebookHandler<RuleCalculateDamage>,
        IInitiatorRulebookSubscriber
    {
        private static readonly string WitheringRayAbilityGuid =
            BlueprintLibrary.NormalizeGuid(BloodlineGuids.NewNecroRayAbilityGuid);

        public void OnEventAboutToTrigger(RuleCalculateDamage evt)
        {
            var context = evt.Reason.Context;

            if (context?.SourceAbility == null)
                return;

            if (!IsQualifiedNecromancySpell(context) && !IsWitheringRay(context.SourceAbility.AssetGuid))
                return;

            var bonusPerDie = GetBonusPerDie(GetSorcererLevel());

            foreach (var baseDamage in evt.DamageBundle)
                baseDamage.AddBonus(baseDamage.Dice.Rolls * bonusPerDie);
        }

        private static int GetBonusPerDie(int sorcererLevel)
        {
            if (sorcererLevel >= 17) return 3;
            if (sorcererLevel >= 9) return 2;
            return 1;
        }

        private int GetSorcererLevel()
        {
            var sorcererClass = BlueprintLibrary.TryGet<BlueprintCharacterClass>(Guids.Features.SorcererClassGuid);
            return sorcererClass != null ? Owner.Progression.GetClassLevel(sorcererClass) : 0;
        }

        private bool IsQualifiedNecromancySpell(MechanicsContext context)
        {
            if (!context.SourceAbility.IsSpell)
                return false;

            return context.SourceAbility.School == SpellSchool.Necromancy;
        }

        private static bool IsWitheringRay(string guid)
        {
            return string.Equals(guid, WitheringRayAbilityGuid, System.StringComparison.OrdinalIgnoreCase);
        }

        public void OnEventDidTrigger(RuleCalculateDamage evt)
        {
        }
    }
}
