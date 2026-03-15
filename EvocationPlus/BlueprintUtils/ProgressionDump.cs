using System;
using System.Linq;
using Kingmaker.Blueprints.Classes;
using UnityModManagerNet;

namespace EvocationPlus.BlueprintUtils
{
    internal static class ProgressionDump
    {
        public static UnityModManager.ModEntry.ModLogger Log;

        public static void DumpProgression(string progressionGuid, string fileName)
        {
            var prog = BlueprintLibrary.TryGet<BlueprintProgression>(progressionGuid);
            if (prog == null)
            {
                Log?.Error($"[ProgressionDump] Progression not found: {progressionGuid}");
                return;
            }

            var lines =
                prog.LevelEntries
                    .OrderBy(le => le.Level)
                    .SelectMany(le => le.Features.Select(f =>
                        $"Level {le.Level:00} | {f.AssetGuid} | {f.name}"
                    ));

            BlueprintDumper.WriteLinesToFile(
                $"Progression dump: {prog.name} ({prog.AssetGuid})",
                lines,
                fileName
            );
        }
    }
}