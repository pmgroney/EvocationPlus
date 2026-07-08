using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.UnitLogic.FactLogic;

namespace EvocationPlus.Utils
{
    internal static class BloodlineUiGroupUtil
    {
        /// <summary>
        /// Ensures all AddKnownSpell features show in the same "spells row" on the progression timeline.
        /// Call this ONLY on your cloned progressions (not donor / not vanilla).
        /// </summary>
        internal static void NormalizeSpellRow(BlueprintProgression prog)
        {
            if (prog == null) return;
            // Collect spell-grant features in stable progression order (by LevelEntries).
            var spellFeaturesOrdered = GetSpellGrantFeaturesInProgressionOrder(prog);
            if (spellFeaturesOrdered.Count == 0) return;

            if (prog.UIGroups == null || prog.UIGroups.Length == 0)
            {
                prog.UIGroups = new[]
                {
                    new UIGroup
                    {
                        Features = new List<BlueprintFeatureBase>(spellFeaturesOrdered)
                    }
                };
                return;
            }

            // Pick a spells row:
            // Prefer the first UIGroup that already contains any spell-grant features.
            // Otherwise, create a dedicated spell row instead of merging into an unrelated chain.
            var groups = prog.UIGroups;
            var spellsRowIndex = FindExistingSpellsRowIndex(groups, spellFeaturesOrdered);
            if (!spellsRowIndex.HasValue)
            {
                prog.UIGroups = groups
                    .Concat(new[]
                    {
                        new UIGroup
                        {
                            Features = new List<BlueprintFeatureBase>(spellFeaturesOrdered)
                        }
                    })
                    .ToArray();
                return;
            }

            // Remove spell features from ALL rows first (avoid duplicates / split rows).
            for (int gi = 0; gi < groups.Length; gi++)
            {
                var g = groups[gi];
                if (g?.Features == null) continue;

                g.Features.RemoveAll(f => f != null && spellFeaturesOrdered.Contains(f));
            }

            // Add them back to the chosen spells row in correct order.
            var spellsRow = groups[spellsRowIndex.Value];
            if (spellsRow.Features == null)
                spellsRow.Features = new List<BlueprintFeatureBase>();

            foreach (var f in spellFeaturesOrdered)
            {
                if (f == null) continue;
                if (!spellsRow.Features.Contains(f))
                    spellsRow.Features.Add(f);
            }
        }

        private static List<BlueprintFeatureBase> GetSpellGrantFeaturesInProgressionOrder(BlueprintProgression prog)
        {
            var result = new List<BlueprintFeatureBase>();

            if (prog.LevelEntries == null) return result;

            foreach (var le in prog.LevelEntries.OrderBy(e => e?.Level ?? 0))
            {
                if (le?.Features == null) continue;

                foreach (var fb in le.Features)
                {
                    var bf = fb as BlueprintFeature;
                    if (bf == null) continue;

                    // AddKnownSpell is the bloodline-granted-spell marker
                    if (bf.GetComponents<AddKnownSpell>()?.Any() == true)
                    {
                        if (!result.Contains(fb))
                            result.Add(fb);
                    }
                }
            }

            return result;
        }

        private static int? FindExistingSpellsRowIndex(UIGroup[] groups, List<BlueprintFeatureBase> spellFeatures)
        {
            for (int i = 0; i < groups.Length; i++)
            {
                var g = groups[i];
                if (g?.Features == null) continue;

                for (int j = 0; j < g.Features.Count; j++)
                {
                    var f = g.Features[j];
                    if (f != null && spellFeatures.Contains(f))
                        return i;
                }
            }
            return null;
        }
    }
}
