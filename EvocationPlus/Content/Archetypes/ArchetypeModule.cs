using EvocationPlus.Archetypes;
using EvocationPlus.Core;
using EvocationPlus.Spells.Infrastructure;
using Kingmaker.Blueprints;

namespace EvocationPlus.Content.Archetypes
{
    public class ArchetypeModule : IContentModule
    {
        public string Name => "Archetypes";

        public void Install(LibraryScriptableObject library)
        {
            ArchetypeInstaller.InstallAll(library);

            // Apply icons for BlueprintUnitFact-derived blueprints created during archetype install
            SpellIconApplier.ApplyUnitFactIcons(library, Main.Mod.Path);
        }
    }
}