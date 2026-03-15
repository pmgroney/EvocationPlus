using Kingmaker.Blueprints;
using Kingmaker.UnitLogic.Abilities.Blueprints;

namespace EvocationPlus.Spells.Infrastructure
{
    public interface ISpellModifier
    {
        void Apply(BlueprintAbility spell, LibraryScriptableObject library);
    }

    public sealed class SpellDefinition
    {
        public readonly string BaseSpellAssetId;
        public readonly string InternalName;
        public readonly ISpellModifier Modifier;
        public readonly string NewSpellAssetId;
        public readonly int SpellLevel;

        public SpellDefinition(string baseSpellAssetId, string newSpellAssetId, string internalName, int spellLevel,
            ISpellModifier modifier)
        {
            BaseSpellAssetId = baseSpellAssetId;
            NewSpellAssetId = newSpellAssetId;
            InternalName = internalName;
            SpellLevel = spellLevel;
            Modifier = modifier;
        }
    }
}