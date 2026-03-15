using System.Linq;
using EvocationPlus.BlueprintUtils;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Prerequisites;
using Kingmaker.Blueprints.Classes.Selection;
using UnityEngine;

namespace EvocationPlus.Patches.Bloodlines
{
    internal static class EvokerBloodlineSelectionInstaller
    {
        internal static void EnsureSelection(LibraryScriptableObject library)
        {
            var existing =
                BlueprintLibrary.GetBlueprint(library, BloodlineGuids.EvokerBloodlineSelectionGuid) as
                    BlueprintFeatureSelection;
            if (existing != null) return;

            var sel = ScriptableObject.CreateInstance<BlueprintFeatureSelection>();
            sel.name = "EvocationPlus_EvokerBloodlineSelection";
            sel.AssetGuid = BlueprintLibrary.NormalizeGuid(BloodlineGuids.EvokerBloodlineSelectionGuid);

            EvocationPlusUnitFactText.SetNameKey(sel, "EVP_EVOKER_BLOODLINE_NAME");
            EvocationPlusUnitFactText.SetDescriptionKey(sel, "EVP_EVOKER_BLOODLINE_DESC");

            sel.IsClassFeature = true;
            sel.Obligatory = true;
            sel.ReapplyOnLevelUp = true;
            sel.Mode = SelectionMode.Default;

            sel.Group = FeatureGroup.BloodLine;
            sel.Groups = new[] { FeatureGroup.BloodLine };

            sel.HideInUI = false;
            sel.HideInCharacterSheetAndLevelUp = false;
            sel.HideNotAvailibleInUI = false;
            sel.IgnorePrerequisites = false;

            var arcane = EvokerProgressions.EnsureArcane(library);
            var air = EvokerProgressions.EnsureAir(library);
            var earth = EvokerProgressions.EnsureEarth(library);
            var fire = EvokerProgressions.EnsureFire(library);
            var water = EvokerProgressions.EnsureWater(library);
            sel.AllFeatures = new BlueprintFeature[] { arcane, air, earth, fire, water };
            BlueprintLibrary.Register(library, BloodlineGuids.EvokerBloodlineSelectionGuid, sel);
        }

        public static void StripArchetypePrereqs(BlueprintProgression prog)
        {
            if (prog == null) return;

            // Remove prereqs that require/forbid specific archetypes (e.g., "Seeker level 1")
            prog.ComponentsArray = prog.ComponentsArray
                .Where(c =>
                    !(c is PrerequisiteArchetypeLevel) &&
                    !(c is PrerequisiteNoArchetype))
                .ToArray();
        }
    }
}