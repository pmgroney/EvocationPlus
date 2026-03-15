using EvocationPlus.Core;
using Kingmaker.Blueprints;

namespace EvocationPlus.Content.Fixes
{
    public class FixesModule : IContentModule
    {
        public string Name => "Fixes";

        public void Install(LibraryScriptableObject library)
        {
            // Central place for any one-off blueprint fixes / compatibility patches.
            // If you don't have any yet, keep it empty but present.
            ContentFixes.ApplyAll(library);
        }
    }
}