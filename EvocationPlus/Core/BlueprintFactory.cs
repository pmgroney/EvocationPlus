using EvocationPlus.BlueprintUtils;
using EvocationPlus.IconUtils;

namespace EvocationPlus.Core
{
    internal static class BlueprintFactory
    {
        internal static void RegisterAbilityIcon(string blueprintGuid, string relativePngPath)
        {
            SpellIconRegistry.RegisterAbility(
                BlueprintLibrary.NormalizeGuid(blueprintGuid),
                relativePngPath);
        }

        internal static void RegisterUnitFactIcon(string blueprintGuid, string relativePngPath)
        {
            SpellIconRegistry.RegisterUnitFact(
                BlueprintLibrary.NormalizeGuid(blueprintGuid),
                relativePngPath);
        }
    }
}