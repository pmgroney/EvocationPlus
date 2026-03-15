using System;
using System.Collections.Generic;

namespace EvocationPlus.IconUtils
{
    internal static class SpellIconRegistry
    {
        // Ability GUID -> relative PNG path
        private static readonly Dictionary<string, string> AbilityIconByGuid =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        // UnitFact GUID (BlueprintFeature/Progression/etc.) -> relative PNG path
        private static readonly Dictionary<string, string> UnitFactIconByGuid =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        public static void RegisterAbility(string abilityGuid, string relativePngPath)
        {
            if (string.IsNullOrWhiteSpace(abilityGuid))
                throw new ArgumentException("abilityGuid is null/empty", nameof(abilityGuid));
            if (string.IsNullOrWhiteSpace(relativePngPath))
                throw new ArgumentException("relativePngPath is null/empty", nameof(relativePngPath));

            AbilityIconByGuid[abilityGuid] = relativePngPath;
        }

        public static void RegisterUnitFact(string unitFactGuid, string relativePngPath)
        {
            if (string.IsNullOrWhiteSpace(unitFactGuid))
                throw new ArgumentException("unitFactGuid is null/empty", nameof(unitFactGuid));
            if (string.IsNullOrWhiteSpace(relativePngPath))
                throw new ArgumentException("relativePngPath is null/empty", nameof(relativePngPath));

            UnitFactIconByGuid[unitFactGuid] = relativePngPath;
        }

        public static IReadOnlyDictionary<string, string> GetAllAbilityIcons() => AbilityIconByGuid;
        public static IReadOnlyDictionary<string, string> GetAllUnitFactIcons() => UnitFactIconByGuid;
    }
}