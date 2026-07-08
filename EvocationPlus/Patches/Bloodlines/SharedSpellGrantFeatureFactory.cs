using System.Linq;
using EvocationPlus.BlueprintUtils;
using EvocationPlus.Core;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.FactLogic;
using UnityEngine;

namespace EvocationPlus.Patches.Bloodlines
{
    internal static class SharedSpellGrantFeatureFactory
    {
        internal static BlueprintFeature EnsureProtectionFromEnergyCommunalSpellFeature(
            LibraryScriptableObject library,
            BlueprintCharacterClass sorc)
        {
            if (library == null || sorc == null)
                return null;

            var existing =
                BlueprintLibrary.GetBlueprint(library, Guids.Spells.ProtectionFromEnergyCommunalFeature) as
                    BlueprintFeature;
            if (existing != null)
                return existing;

            var spell = FindProtectionFromEnergyCommunal(library);
            if (spell == null)
            {
                Main.Mod.Logger.Log("EVP: ProtectionFromEnergyCommunal spell not found.");
                return null;
            }

            var feature = ScriptableObject.CreateInstance<BlueprintFeature>();
            feature.name = "EvocationPlus_ProtectionFromEnergyCommunal_Level8";
            feature.AssetGuid = BlueprintLibrary.NormalizeGuid(Guids.Spells.ProtectionFromEnergyCommunalFeature);
            feature.Ranks = 1;
            feature.IsClassFeature = true;
            feature.Groups = new FeatureGroup[0];

            EvocationPlusUnitFactText.SetNameKey(feature, "EVP_PROTECTION_FROM_ENERGY_COMMUNAL_NAME");
            EvocationPlusUnitFactText.SetDescriptionKey(feature, "EVP_PROTECTION_FROM_ENERGY_COMMUNAL_DESC");
            BlueprintUnitFactUI.SetIcon(feature, spell.Icon);

            var addKnown = ScriptableObject.CreateInstance<AddKnownSpell>();
            addKnown.CharacterClass = sorc;
            addKnown.SpellLevel = 4;
            addKnown.Spell = spell;
            addKnown.Archetype = null;

            feature.ComponentsArray = new BlueprintComponent[] { addKnown };

            BlueprintLibrary.Register(library, Guids.Spells.ProtectionFromEnergyCommunalFeature, feature);
            return feature;
        }

        private static BlueprintAbility FindProtectionFromEnergyCommunal(LibraryScriptableObject library)
        {
            if (library?.BlueprintsByAssetId == null) return null;

            return library.BlueprintsByAssetId.Values
                .OfType<BlueprintAbility>()
                .FirstOrDefault(a =>
                {
                    var name = NormalizeBlueprintName(a.name);
                    return name == "protectionfromenergycommunal" ||
                           name == "protectionfromenergycommunalability" ||
                           name == "protectionfromenergycommunalmass";
                });
        }

        private static string NormalizeBlueprintName(string name)
        {
            return string.IsNullOrEmpty(name)
                ? string.Empty
                : new string(name.Where(char.IsLetterOrDigit).Select(char.ToLowerInvariant).ToArray());
        }
    }
}
