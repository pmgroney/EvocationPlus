using EvocationPlus.BlueprintUtils;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Selection;

namespace EvocationPlus.Patches.Bloodlines
{
    internal static class BloodlineSelectionReplacer
    {
        internal static void ReplaceBloodlineInSelection(LibraryScriptableObject library)
        {
            var sel = BlueprintLibrary.GetBlueprint(library, BloodlineGuids.SorcererBloodlineSelectionGuid) as BlueprintFeatureSelection;
            if (sel == null)
            {
                Main.Mod.Logger.Log("EVP: Sorcerer bloodline selection not found.");
                return;
            }

            var baseGuid = BlueprintLibrary.NormalizeGuid(BloodlineGuids.BaseUndeadProgressionGuid);
            var cloneGuid = BlueprintLibrary.NormalizeGuid(BloodlineGuids.ClonedNecroProgressionGuid);

            var all = sel.AllFeatures;
            if (all == null || all.Length == 0)
            {
                Main.Mod.Logger.Log("EVP: Sorcerer bloodline selection AllFeatures empty.");
                return;
            }

            var replaced = false;
            for (var i = 0; i < all.Length; i++)
            {
                var f = all[i];
                if (f == null) continue;

                if (f.AssetGuid == baseGuid)
                {
                    var cloned = BlueprintLibrary.GetBlueprint(library, cloneGuid) as BlueprintProgression;
                    if (cloned == null)
                    {
                        Main.Mod.Logger.Log("EVP: cloned undead progression not found at replace time.");
                        return;
                    }

                    all[i] = cloned;
                    replaced = true;
                    break;
                }
            }

            sel.AllFeatures = all;

            if (!replaced)
                Main.Mod.Logger.Log("EVP: Did not find base undead progression inside bloodline selection AllFeatures.");
        }
    }
}