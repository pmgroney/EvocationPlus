using EvocationPlus.Archetypes;
using EvocationPlus.Core;
using EvocationPlus.IconUtils;
using EvocationPlus.Utils;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Object = UnityEngine.Object;

namespace EvocationPlus.BlueprintUtils
{
    internal static class EnsureHarbingerOfBoomUtil
    {
        internal static BlueprintFeature EnsureHarbingerOfBoom(
            LibraryScriptableObject library,
            BlueprintCharacterClass parentClass)
        {
            var existing = BlueprintLibrary.GetBlueprint(library, Guids.BlueprintGuids.HarbingerOfBoomGuid) as BlueprintFeature;
            if (existing != null) return existing;

            if (parentClass == null)
            {
                Main.Mod.Logger.Log("HarbingerOfBoom: parent class is null.");
                return null;
            }

            var proto = BlueprintLibrary.GetBlueprint(library, Guids.BlueprintGuids.RedDragonArcanaGuid) as BlueprintFeature;
            if (proto == null)
            {
                Main.Mod.Logger.Log("HarbingerOfBoom: could not find red dragon arcana prototype.");
                return null;
            }

            // Copy prototype (keeps icon/visual defaults)
            var f = Object.Instantiate(proto);
            f.name = "EvocationPlus_HarbingerOfBoom";
            f.AssetGuid = BlueprintLibrary.NormalizeGuid(Guids.BlueprintGuids.HarbingerOfBoomGuid);
            f.IsClassFeature = true;
            f.Ranks = 1;

            UnitFactStringUtils.SetUnitFactStrings(
                f,
                "EVP_HARBINGER_OF_BOOM_NAME",
                "EVP_HARBINGER_OF_BOOM_DESC");

            // Override icon (otherwise it keeps proto icon)
            var icon = DiskIconLoader.LoadSprite("harbinger_of_boom.png");
            if (icon != null)
                BlueprintUnitFactUI.SetIcon(f, icon);

            // ✅ Behavior component (Evocation + CHA once per spell total)
            var comp = f.AddComponent<HarbingerOfBoomArcanaClassSpells>();
            comp.classes = new[] { parentClass };

            BlueprintLibrary.Register(library, Guids.BlueprintGuids.HarbingerOfBoomGuid, f);
            return f;
        }

        // Archetype/class feature placement (matches MasterOfDeath behavior)
        internal static void AddAsClassDeterminator(BlueprintCharacterClass parentClass, BlueprintFeature feature)
        {
            DeterminatorUtil.AddToClassProgression(parentClass, feature);
        }

        // Progression-specific placement (keep for future bloodline/progression UI needs)
        internal static void AddAsProgressionDeterminator(BlueprintProgression progression, BlueprintFeature feature)
        {
            DeterminatorUtil.AddToProgression(progression, feature);
        }
    }
}