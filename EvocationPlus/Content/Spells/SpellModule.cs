using EvocationPlus.Core;
using EvocationPlus.Spells.Infrastructure;
using Kingmaker.Blueprints;

namespace EvocationPlus.Content.Spells
{
    public class SpellModule : IContentModule
    {
        public string Name => "Spells";

        public void Install(LibraryScriptableObject library)
        {
            SpellInstaller.InstallAll(library, SpellRegistry.GetAll());
            SpellIconApplier.ApplyAbilityIcons(library, Main.Mod.Path);
        }
    }
}