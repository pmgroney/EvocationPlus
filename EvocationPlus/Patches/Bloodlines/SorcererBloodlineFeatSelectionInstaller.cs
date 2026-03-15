using System;
using System.Linq;
using EvocationPlus.BlueprintUtils;
using EvocationPlus.Core;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Prerequisites;
using Kingmaker.Blueprints.Classes.Selection;

namespace EvocationPlus.Patches.Bloodlines
{
    public class SorcererBloodlineFeatSelectionInstaller
    {
        internal static void EnsureArchetypeFeatSelectionAddedToSorcererFeatSelection(LibraryScriptableObject library)
        {
            var sorcererFeatSelection =
                BlueprintLibrary.GetBlueprint(library, BloodlineGuids.SorcererFeatSelectionGuid) as
                    BlueprintFeatureSelection;

            if (sorcererFeatSelection == null)
            {
                Main.Mod.Logger.Log("EVP: SorcererFeatSelection not found.");
                return;
            }

            var sharedSelection = EnsureSharedArchetypeFeatSelection(library);
            if (sharedSelection == null)
            {
                Main.Mod.Logger.Log("EVP: shared archetype feat selection not found/created.");
                return;
            }

            var all = (sorcererFeatSelection.AllFeatures ?? Array.Empty<BlueprintFeature>())
                .Where(f => f != null)
                .ToList();

            if (!all.Any(f => f.AssetGuid == sharedSelection.AssetGuid))
            {
                all.Add(sharedSelection);
                sorcererFeatSelection.AllFeatures = all.ToArray();
            }
        }

        private static BlueprintFeatureSelection EnsureSharedArchetypeFeatSelection(LibraryScriptableObject library)
        {
            var existing =
                BlueprintLibrary.GetBlueprint(library, BloodlineGuids.SorcererArchetypeSharedFeatSelectionGuid) as
                    BlueprintFeatureSelection;
            if (existing != null) return existing;

            var arcane =
                BlueprintLibrary.GetBlueprint(library, BloodlineGuids.BloodlineArcaneFeatSelectionGuid) as
                    BlueprintFeatureSelection;
            var elemental =
                BlueprintLibrary.GetBlueprint(library, BloodlineGuids.BloodlineElementalFeatSelectionGuid) as
                    BlueprintFeatureSelection;

            if (arcane == null)
            {
                Main.Mod.Logger.Log(
                    "EVP: shared archetype feat selection failed: BloodlineArcaneFeatSelection missing.");
                return null;
            }

            if (elemental == null)
            {
                Main.Mod.Logger.Log(
                    "EVP: shared archetype feat selection failed: BloodlineElementalFeatSelection missing.");
                return null;
            }

            var sorcererClass =
                BlueprintLibrary.GetBlueprint(library, Guids.Features.SorcererClassGuid) as BlueprintCharacterClass;

            var evokerArchetype =
                BlueprintLibrary.GetBlueprint(library, BloodlineGuids.EvokerArchetypeGuid) as BlueprintArchetype;

            var necromancerArchetype =
                BlueprintLibrary.GetBlueprint(library, BloodlineGuids.NecromancerArchetypeGuid) as BlueprintArchetype;

            if (sorcererClass == null)
            {
                Main.Mod.Logger.Log("EVP: shared archetype feat selection failed: Sorcerer class missing.");
                return null;
            }

            if (evokerArchetype == null)
            {
                Main.Mod.Logger.Log("EVP: shared archetype feat selection failed: Evoker archetype missing.");
                return null;
            }

            if (necromancerArchetype == null)
            {
                Main.Mod.Logger.Log("EVP: shared archetype feat selection failed: Necromancer archetype missing.");
                return null;
            }

            var sel = UnityEngine.Object.Instantiate(arcane);
            sel.name = "EvocationPlus_SorcererArchetypeSharedFeatSelection";
            sel.AssetGuid = BlueprintLibrary.NormalizeGuid(BloodlineGuids.SorcererArchetypeSharedFeatSelectionGuid);

            sel.ComponentsArray = Array.Empty<BlueprintComponent>();

            sel.Ranks = 1;
            sel.IsClassFeature = true;
            sel.HideInUI = false;
            sel.HideInCharacterSheetAndLevelUp = false;
            sel.HideNotAvailibleInUI = false;
            sel.IgnorePrerequisites = false;

            sel.AllFeatures = arcane.AllFeatures
                .Concat(elemental.AllFeatures)
                .Where(f => f != null)
                .GroupBy(f => f.AssetGuid)
                .Select(g => g.First())
                .ToArray();

            var evokerPrereq = UnityEngine.ScriptableObject
                .CreateInstance<Kingmaker.Blueprints.Classes.Prerequisites.PrerequisiteArchetypeLevel>();
            evokerPrereq.CharacterClass = sorcererClass;
            evokerPrereq.Archetype = evokerArchetype;
            evokerPrereq.Level = 1;
            evokerPrereq.Group = Prerequisite.GroupType.Any;
            
            var necromancerPrereq = UnityEngine.ScriptableObject
                .CreateInstance<Kingmaker.Blueprints.Classes.Prerequisites.PrerequisiteArchetypeLevel>();
            necromancerPrereq.CharacterClass = sorcererClass;
            necromancerPrereq.Archetype = necromancerArchetype;
            necromancerPrereq.Level = 1;
            necromancerPrereq.Group = Prerequisite.GroupType.Any;
            
            sel.ComponentsArray = new BlueprintComponent[]
            {
                evokerPrereq,
                necromancerPrereq
            };

            BlueprintLibrary.Register(library, BloodlineGuids.SorcererArchetypeSharedFeatSelectionGuid, sel);
            return sel;
        }
    }
}