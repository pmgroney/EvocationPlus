using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using UnityModManagerNet;

namespace EvocationPlus.BlueprintUtils
{
    /// <summary>
    /// Kingmaker-safe blueprint dumper.
    /// - Enumerates via ResourcesLibrary.LibraryObject.GetAllBlueprints()
    /// - Avoids private display-name fields (logs type + guid + internal name)
    /// - Writes either to UMM log or to a file in the mod folder
    /// </summary>
    internal static class BlueprintDumper
    {
        public static UnityModManager.ModEntry.ModLogger Log;
        public static string ModFolder;

        private static bool _didDump;

        /// <summary>
        /// Ensures a dump block only runs once per session.
        /// Call this from your blueprints-loaded hook (e.g., BlueprintsCache.Init Postfix).
        /// </summary>
        public static void DumpOnce(Action dumpAction)
        {
            if (_didDump) return;
            _didDump = true;

            try
            {
                dumpAction?.Invoke();
            }
            catch (Exception ex)
            {
                Log?.Error($"[BlueprintDumper] Dump failed: {ex}");
            }
        }

        /// <summary>
        /// Dump blueprints of type T whose INTERNAL NAME (bp.name) contains the substring (case-insensitive).
        /// </summary>
        public static void DumpByNameContains<T>(string contains, string fileName = null)
            where T : BlueprintScriptableObject
        {
            var needle = (contains ?? "").Trim();

            var items = GetAllBlueprints<T>()
                .Where(bp =>
                {
                    var n = bp?.name ?? "";
                    return n.IndexOf(needle, StringComparison.OrdinalIgnoreCase) >= 0;
                })
                .ToList();

            WriteDump(
                header: $"DUMP: {typeof(T).Name} name contains \"{needle}\" (count={items.Count})",
                lines: items.Select(FormatBlueprintLine),
                fileName: fileName
            );
        }

        /// <summary>
        /// Dump ANY blueprint whose internal name contains the substring (case-insensitive), regardless of type.
        /// Useful for discovering your mod-owned blueprints if you have a naming prefix.
        /// </summary>
        public static void DumpAnyByNameContains(string contains, string fileName)
        {
            var needle = (contains ?? "").Trim();

            var items = GetAllBlueprints()
                .Where(bp =>
                {
                    var n = bp?.name ?? "";
                    return n.IndexOf(needle, StringComparison.OrdinalIgnoreCase) >= 0;
                })
                .ToList();

            WriteDump(
                header: $"DUMP: ANY BlueprintScriptableObject name contains \"{needle}\" (count={items.Count})",
                lines: items.Select(FormatBlueprintLine),
                fileName: fileName
            );
        }

        /// <summary>
        /// Convenience: dump common blueprint types by a shared substring.
        /// </summary>
        public static void DumpCommonFinders(string contains, string filePrefix = "dump")
        {
            DumpByNameContains<BlueprintCharacterClass>(contains, $"{filePrefix}_classes.txt");
            DumpByNameContains<BlueprintArchetype>(contains, $"{filePrefix}_archetypes.txt");
            DumpByNameContains<BlueprintFeature>(contains, $"{filePrefix}_features.txt");
            DumpByNameContains<BlueprintFeatureSelection>(contains, $"{filePrefix}_featureSelections.txt");
            DumpByNameContains<BlueprintAbility>(contains, $"{filePrefix}_abilities.txt");
            DumpByNameContains<BlueprintBuff>(contains, $"{filePrefix}_buffs.txt");
            DumpByNameContains<BlueprintSpellbook>(contains, $"{filePrefix}_spellbooks.txt");
        }

        /// <summary>
        /// Compatibility wrapper for existing callers in your project.
        /// </summary>
        public static void WriteLinesToFile(string header, IEnumerable<string> lines, string fileName)
        {
            WriteDump(header, lines ?? Enumerable.Empty<string>(), fileName);
        }

        /// <summary>
        /// Utility for other dump tools: write arbitrary lines to a file (or log if fileName is null/empty).
        /// </summary>
        public static void WriteLines(string header, IEnumerable<string> lines, string fileName = null)
        {
            WriteDump(header, lines ?? Enumerable.Empty<string>(), fileName);
        }

        // --------------------
        // Internals
        // --------------------

        private static List<BlueprintScriptableObject> GetAllBlueprints()
        {
            // Kingmaker: this is the canonical blueprint library root.
            var library = ResourcesLibrary.LibraryObject;
            if (library == null)
            {
                Log?.Warning("[BlueprintDumper] ResourcesLibrary.LibraryObject is null (blueprints not loaded yet).");
                return new List<BlueprintScriptableObject>();
            }

            // Most KM builds expose: IEnumerable<BlueprintScriptableObject> GetAllBlueprints()
            var all = library.GetAllBlueprints();
            return (all ?? Enumerable.Empty<BlueprintScriptableObject>()).Where(bp => bp != null).ToList();
        }

        private static List<T> GetAllBlueprints<T>() where T : BlueprintScriptableObject
        {
            return GetAllBlueprints().OfType<T>().ToList();
        }

        private static string FormatBlueprintLine(BlueprintScriptableObject bp)
        {
            var typeName = bp.GetType().Name;
            var guid = bp.AssetGuid.ToString();
            var internalName = bp.name ?? "";
            return $"{typeName} | {guid} | {internalName}";
        }

        private static void WriteDump(string header, IEnumerable<string> lines, string fileName)
        {
            Log?.Log($"[BlueprintDumper] {header}");

            var content = new StringBuilder()
                .AppendLine($"========== {header} ==========")
                .AppendLine(string.Join(Environment.NewLine, lines ?? Enumerable.Empty<string>()))
                .ToString();

            if (string.IsNullOrWhiteSpace(fileName))
            {
                // No file requested: log each line (can be noisy)
                foreach (var line in lines ?? Enumerable.Empty<string>())
                    Log?.Log(line);
                return;
            }

            try
            {
                var folder = string.IsNullOrWhiteSpace(ModFolder) ? Environment.CurrentDirectory : ModFolder;
                var fullPath = Path.Combine(folder, fileName);
                File.WriteAllText(fullPath, content, Encoding.UTF8);
                Log?.Log($"[BlueprintDumper] Wrote: {fullPath}");
            }
            catch (Exception ex)
            {
                Log?.Error($"[BlueprintDumper] Failed writing file \"{fileName}\": {ex}");
            }
        }

        internal static void DumpProgressionDetailed(BlueprintProgression prog, string fileName)
        {
            if (prog == null)
            {
                Main.Mod.Logger.Log("[BlueprintDumper] DumpProgressionDetailed: prog is null.");
                return;
            }

            // Local helper ONLY inside this method (no new shared methods)
            string DisplayName(BlueprintFeatureBase f)
            {
                if (f == null) return "<null>";

                try
                {
                    // BlueprintFeatureBase ultimately derives from BlueprintUnitFact, which has Name
                    // In Kingmaker, Name usually resolves localization if available.
                    var n = f.Name;
                    if (!string.IsNullOrWhiteSpace(n)) return n.Trim();
                }
                catch
                {
                    // ignore and fall back
                }

                return f.name ?? "<no-internal-name>";
            }

            var lines = new List<string>();
            lines.Add($"=== PROGRESSION DETAILED: {prog.name} ({prog.AssetGuid}) ===");
            lines.Add("");

            lines.Add("== LevelEntries ==");
            foreach (var le in (prog.LevelEntries ?? Array.Empty<LevelEntry>()).OrderBy(e => e?.Level ?? 0))
            {
                if (le?.Features == null) continue;

                lines.Add($"-- Level {le.Level:00} --");
                for (int i = 0; i < le.Features.Count; i++)
                {
                    var f = le.Features[i];
                    if (f == null)
                    {
                        lines.Add($"  [{i}] <null>");
                        continue;
                    }

                    lines.Add(
                        $"  [{i}] {DisplayName(f)} | {f.AssetGuid} | internal={f.name} | type={f.GetType().Name}");
                }
            }

            lines.Add("");
            lines.Add("== UIGroups ==");
            var ugs = prog.UIGroups ?? Array.Empty<UIGroup>();
            for (int gi = 0; gi < ugs.Length; gi++)
            {
                var g = ugs[gi];
                lines.Add($"-- UIGroups[{gi}] --");
                if (g?.Features == null)
                {
                    lines.Add("  <null>");
                    continue;
                }

                for (int i = 0; i < g.Features.Count; i++)
                {
                    var f = g.Features[i];
                    if (f == null)
                    {
                        lines.Add($"  [{i}] <null>");
                        continue;
                    }

                    lines.Add(
                        $"  [{i}] {DisplayName(f)} | {f.AssetGuid} | internal={f.name} | type={f.GetType().Name}");
                }
            }

            Main.Mod.Logger.Log("[BlueprintDumper] !!DumpProgressionDetailed: " + lines);
            WriteLinesToFile($"Progression Detailed Dump: {prog.name}", lines, fileName);
        }

        internal static void DumpFeatureSelectionComponents(BlueprintFeatureSelection sel, string fileName)
        {
            if (sel == null)
            {
                WriteLinesToFile("FeatureSelection Component Dump", new[] { "Selection was null." }, fileName);
                return;
            }

            var lines = new List<string>();
            lines.Add("========== FEATURE SELECTION COMPONENT DUMP ==========");
            lines.Add($"Selection: {sel.name} | {sel.AssetGuid} | type={sel.GetType().Name}");
            lines.Add($"AllFeatures: {(sel.AllFeatures?.Length ?? 0)}");
            if (sel.AllFeatures != null)
                for (int i = 0; i < sel.AllFeatures.Length; i++)
                    lines.Add(
                        $"  AllFeatures[{i}] = {sel.AllFeatures[i]?.name ?? "<null>"} | {sel.AllFeatures[i]?.AssetGuid.ToString() ?? "<null>"}");

            lines.Add($"Components: {(sel.ComponentsArray?.Length ?? 0)}");
            lines.Add("");

            var flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

            if (sel.ComponentsArray != null)
            {
                for (int ci = 0; ci < sel.ComponentsArray.Length; ci++)
                {
                    var c = sel.ComponentsArray[ci];
                    if (c == null)
                    {
                        lines.Add($"Component[{ci}]: <null>");
                        continue;
                    }

                    var ct = c.GetType();
                    lines.Add($"Component[{ci}] Type: {ct.FullName}");

                    foreach (var f in ct.GetFields(flags))
                    {
                        if (f.IsNotSerialized) continue;
                        object v = null;
                        try
                        {
                            v = f.GetValue(c);
                        }
                        catch
                        {
                            /* ignore */
                        }

                        string s;
                        if (v == null) s = "<null>";
                        else if (v is BlueprintScriptableObject bso) s = $"{bso.name} | {bso.AssetGuid}";
                        else if (v is Array arr) s = $"Array(len={arr.Length})";
                        else s = v.ToString();

                        lines.Add($"    Field: {f.Name} = {s}");
                    }

                    lines.Add("");
                }
            }

            WriteLinesToFile($"FeatureSelection Component Dump: {sel.name}", lines, fileName);
        }
        public static void DumpAllFeatureSelections(string fileName = "feature_selections.txt")
        {
            var selections = BlueprintLibrary
                .GetAllBlueprints<BlueprintFeatureSelection>()
                .OrderBy(s => s.name);

            var lines = new List<string>();

            foreach (var sel in selections)
            {
                if (sel == null) continue;

                lines.Add("--------------------------------------------------");
                lines.Add($"Selection: {sel.name}");
                lines.Add($"Guid: {sel.AssetGuid}");
                lines.Add($"Group: {sel.Group}");
                lines.Add($"IgnorePrereq: {sel.IgnorePrerequisites}");
                lines.Add($"Obligatory: {sel.Obligatory}");

                var features = sel.AllFeatures;

                if (features == null || features.Length == 0)
                {
                    lines.Add("Features: <EMPTY>");
                    continue;
                }

                lines.Add($"Features ({features.Length}):");

                foreach (var f in features)
                {
                    if (f == null)
                    {
                        lines.Add("   <null>");
                        continue;
                    }

                    lines.Add($"   {f.AssetGuid} | {f.name}");
                }
            }

            WriteLinesToFile("Feature Selections Dump", lines, fileName);
        }
    }
}