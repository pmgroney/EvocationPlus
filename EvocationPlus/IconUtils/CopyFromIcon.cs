using EvocationPlus.BlueprintUtils;
using Kingmaker.Blueprints;
using Kingmaker.UnitLogic.Abilities.Blueprints;

namespace EvocationPlus.IconUtils
{
    public static class CopyFromIcon
    {
        public static void CopyIconFrom(BlueprintAbility target, LibraryScriptableObject library, string iconSpellId)
        {
            // Normalize if your BlueprintLibrary expects normalized IDs
            var id = BlueprintLibrary.NormalizeGuid(iconSpellId);
            var source = BlueprintLibrary.GetBlueprint(library, id) as BlueprintAbility;
            if (source == null)
            {
                Main.Mod.Logger.Log("CopyIconFrom: source blueprint not a BlueprintAbility (id=" + id + ")");
                return;
            }

            if (source.Icon == null)
            {
                Main.Mod.Logger.Log("CopyIconFrom: source.Icon is null (source=" + source.name + ")");
                return;
            }

            // This is the Kingmaker-safe way (you already used it successfully)
            BlueprintUnitFactUI.SetIcon(target, source.Icon);
        }
    }
}