using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;

namespace EvocationPlus.Archetypes
{
    public sealed class ArchetypeBuildContext
    {
        public LibraryScriptableObject Library { get; }
        public ArchetypeDefinition Def { get; }
        public BlueprintCharacterClass ParentClass { get; }
        public BlueprintArchetype Archetype { get; }

        public ArchetypeBuildContext(
            LibraryScriptableObject library,
            ArchetypeDefinition def,
            BlueprintCharacterClass parentClass,
            BlueprintArchetype archetype)
        {
            Library = library;
            Def = def;
            ParentClass = parentClass;
            Archetype = archetype;
        }
    }
}