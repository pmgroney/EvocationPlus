using EvocationPlus.BlueprintUtils;
using Kingmaker.Blueprints.Classes;

namespace EvocationPlus.Patches.Bloodlines
{
    internal static class ProgressionSwapUtil
    {
        public static int ReplaceInProgression(
            BlueprintProgression prog,
            string oldGuid,
            BlueprintFeatureBase replacement)
        {
            if (prog == null || replacement == null || string.IsNullOrEmpty(oldGuid))
                return 0;

            var oldNorm = BlueprintLibrary.NormalizeGuid(oldGuid);
            var replaced = 0;

            var entries = prog.LevelEntries;
            if (entries == null) return 0;

            foreach (var entry in entries)
            {
                if (entry?.Features == null) continue;

                for (var i = 0; i < entry.Features.Count; i++)
                {
                    var f = entry.Features[i];
                    if (f == null) continue;

                    if (BlueprintLibrary.NormalizeGuid(f.AssetGuid) != oldNorm) continue;

                    entry.Features[i] = replacement;
                    replaced++;
                }
            }

            return replaced;
        }

        public static int ReplaceInUiGroups(
            BlueprintProgression prog,
            string oldGuid,
            BlueprintFeatureBase replacement)
        {
            if (prog == null || replacement == null || string.IsNullOrEmpty(oldGuid))
                return 0;

            var oldNorm = BlueprintLibrary.NormalizeGuid(oldGuid);
            var replaced = 0;

            var groups = prog.UIGroups;
            if (groups == null) return 0;

            foreach (var g in groups)
            {
                if (g?.Features == null) continue;

                for (var i = 0; i < g.Features.Count; i++)
                {
                    var f = g.Features[i];
                    if (f == null) continue;

                    if (BlueprintLibrary.NormalizeGuid(f.AssetGuid) != oldNorm) continue;

                    g.Features[i] = replacement;
                    replaced++;
                }
            }

            return replaced;
        }

        public static int ReplaceInDeterminators(
            BlueprintProgression prog,
            string oldGuid,
            BlueprintFeatureBase replacement)
        {
            if (prog == null || replacement == null || string.IsNullOrEmpty(oldGuid))
                return 0;

            var det = prog.UIDeterminatorsGroup;
            if (det == null || det.Length == 0)
                return 0;

            var oldNorm = BlueprintLibrary.NormalizeGuid(oldGuid);
            var replaced = 0;

            for (var i = 0; i < det.Length; i++)
            {
                var f = det[i];
                if (f == null) continue;
                if (BlueprintLibrary.NormalizeGuid(f.AssetGuid) != oldNorm) continue;

                det[i] = replacement;
                replaced++;
            }

            prog.UIDeterminatorsGroup = det;
            return replaced;
        }
    }
}