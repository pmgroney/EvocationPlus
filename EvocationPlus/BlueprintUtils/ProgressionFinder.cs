using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using UnityModManagerNet;

namespace EvocationPlus.BlueprintUtils
{
    internal static class ProgressionFinder
    {
        public static UnityModManager.ModEntry.ModLogger Log;

        public static void DumpProgressionsContainingFeature(string featureGuid, string fileName = null)
        {
            var feature = ResourcesLibrary.TryGetBlueprint<BlueprintFeature>(BlueprintLibrary.NormalizeGuid(featureGuid));
            if (feature == null)
            {
                Log?.Warning($"[ProgressionFinder] Feature not found: {featureGuid}");
                return;
            }

            var all = BlueprintLibrary.GetAllBlueprints<BlueprintProgression>(); // use your existing enumerator
            var hits = new List<BlueprintProgression>();

            foreach (var prog in all)
            {
                if (prog?.LevelEntries == null) continue;

                bool contains = prog.LevelEntries.Any(le =>
                    le?.Features != null && le.Features.Any(f => f == feature));

                if (contains) hits.Add(prog);
            }

            var lines = hits
                .OrderBy(p => p.name)
                .Select(p => $"{p.AssetGuid} | {p.name}");

            BlueprintDumper.WriteLinesToFile(
                header: $"Progressions containing feature {feature.name} ({feature.AssetGuid}) count={hits.Count}",
                lines: lines,
                fileName: fileName
            );
        }
    }
}