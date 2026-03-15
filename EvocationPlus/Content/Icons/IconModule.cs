using EvocationPlus.Core;
using EvocationPlus.IconUtils;
using Kingmaker.Blueprints;

namespace EvocationPlus.Content.Icons
{
    public class IconModule : IContentModule
    {
        public string Name => "Icons";

        public void Install(LibraryScriptableObject __instance)
        {
            // Syncs feature icons from ability icons (selection UI, etc.)
            FeatureIconSync.Apply(__instance);
        }
    }
}