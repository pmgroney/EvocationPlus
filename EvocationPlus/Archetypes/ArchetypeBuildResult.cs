using Kingmaker.Blueprints.Classes;

namespace EvocationPlus.Archetypes
{
    public sealed class ArchetypeBuildResult
    {
        public LevelEntry[] RemoveFeatures { get; set; }
        public LevelEntry[] AddFeatures { get; set; }

        // Optional: post-build patches (timeline/progression edits, etc.)
        public System.Action ApplyPatches { get; set; }
    }
}