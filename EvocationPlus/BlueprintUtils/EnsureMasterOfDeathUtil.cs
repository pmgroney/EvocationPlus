using System;
using System.Linq;
using EvocationPlus.Archetypes;
using EvocationPlus.Core;
using EvocationPlus.IconUtils;
using EvocationPlus.Utils;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Object = UnityEngine.Object;

// where MasterOfDeathArcanaClassSpells lives

namespace EvocationPlus.BlueprintUtils
{
    public class EnsureMasterOfDeathUtil
    {
        public static BlueprintFeature EnsureMasterOfDeath(
            LibraryScriptableObject library,
            BlueprintCharacterClass parent,
            BlueprintFeatureBase removeFeatureForIcon)
        {
            var existing = BlueprintLibrary.GetBlueprint(library, Guids.BlueprintGuids.MasterOfDeathGuid) as BlueprintFeature;
            if (existing != null) return existing;

            if (parent == null)
            {
                Main.Mod.Logger.Log("MasterOfDeath: parent class is null.");
                return null;
            }

            var proto = BlueprintLibrary.GetBlueprint(library, Guids.BlueprintGuids.RedDragonArcanaGuid) as BlueprintFeature;
            if (proto == null)
            {
                Main.Mod.Logger.Log("MasterOfDeath: could not find red dragon arcana prototype.");
                return null;
            }

            // Copy prototype (keeps icon/visual defaults)
            var f = Object.Instantiate(proto);
            f.name = "EvocationPlus_MasterOfDeath";
            f.AssetGuid = BlueprintLibrary.NormalizeGuid(Guids.BlueprintGuids.MasterOfDeathGuid);
            f.IsClassFeature = true;
            f.Ranks = 1;

            UnitFactStringUtils.SetUnitFactStrings(
                f,
                "EVP_MASTER_OF_DEATH_NAME",
                "EVP_MASTER_OF_DEATH_DESC");

            // Ensure icon matches removed bloodline selection (optional)
            var icon = DiskIconLoader.LoadSprite("Necromancer.png");
            if (icon != null)
                BlueprintUnitFactUI.SetIcon(f, icon);


            var comp = f.AddComponent<MasterOfDeathArcanaClassSpells>();
            comp.classes = new[] { parent };

            BlueprintLibrary.Register(library, Guids.BlueprintGuids.MasterOfDeathGuid, f);
            return f;
        }

        public static void AddDeterminator(BlueprintCharacterClass parent, BlueprintFeature feature)
        {
            if (parent?.Progression == null || feature == null) return;

            var prog = parent.Progression;
            if (prog.UIDeterminatorsGroup == null || !prog.UIDeterminatorsGroup.Contains(feature))
                prog.UIDeterminatorsGroup = (prog.UIDeterminatorsGroup ?? Array.Empty<BlueprintFeatureBase>())
                    .Concat(new BlueprintFeatureBase[] { feature })
                    .ToArray();
        }
    }
}