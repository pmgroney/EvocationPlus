using System;
using System.Linq;
using EvocationPlus.BlueprintUtils;
using EvocationPlus.Core;
using EvocationPlus.Spells.Implementation;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Designers.EventConditionActionSystem.Actions;
using Kingmaker.ElementsSystem;
using Kingmaker.Enums.Damage;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Mechanics.Actions;
using UnityEngine;

namespace EvocationPlus.Patches.Bloodlines
{
    internal static class ArcaneBloodlineSpellFeatureFactory
    {
        public static BlueprintFeature EnsureForceRayBloodlineSpellFeature(
            LibraryScriptableObject library,
            BlueprintCharacterClass sorc)
        {
            if (library == null || sorc == null)
                return null;

            var featureGuid = Guids.Spells.ForceRayFeature;

            var existing = BlueprintLibrary.GetBlueprint(library, featureGuid) as BlueprintFeature;
            if (existing != null)
                return existing;

            var forceRay = BlueprintLibrary.GetBlueprint(library, Guids.Spells.ForceRay) as BlueprintAbility;
            if (forceRay == null)
            {
                Main.Mod.Logger.Log("EVP: ForceRay spell not found: " + Guids.Spells.ForceRay);
                return null;
            }

            var f = ScriptableObject.CreateInstance<BlueprintFeature>();
            f.name = "EvocationPlus_ForceRay_BloodlineSpellLevel2";
            f.AssetGuid = BlueprintLibrary.NormalizeGuid(featureGuid);

            f.Ranks = 1;
            f.IsClassFeature = true;
            f.Groups = new FeatureGroup[0];

            // UI
            EvocationPlusUnitFactText.SetNameKey(f, "EVP_FORCE_RAY_NAME");
            EvocationPlusUnitFactText.SetDescriptionKey(f, "EVP_FORCE_RAY_DESC");
            BlueprintUnitFactUI.SetIcon(f, forceRay.Icon);

            // ---- AddKnownSpell component ----
            var addKnown = ScriptableObject.CreateInstance<AddKnownSpell>();
            addKnown.CharacterClass = sorc;
            addKnown.SpellLevel = 2; // Level 5 bloodline slot = 2nd level spell
            addKnown.Spell = forceRay;
            addKnown.Archetype = null;

            f.ComponentsArray = f.ComponentsArray == null
                ? new BlueprintComponent[] { addKnown }
                : f.ComponentsArray.Concat(new BlueprintComponent[] { addKnown }).ToArray();

            BlueprintLibrary.Register(library, featureGuid, f);
            return f;
        }

        public static BlueprintFeature EnsureElementalForceBlastFeature(
            LibraryScriptableObject library,
            BlueprintCharacterClass sorc,
            string donorSlotFeatureGuid,
            string donorTemplateFeatureGuid,
            string newAbilityGuid,
            string nameKey,
            string descKey)
        {
            if (library == null || sorc == null) return null;

            // Guard: newAbilityGuid must belong to a BlueprintAbility (or be unused)
            var existingObj = BlueprintLibrary.GetBlueprint(library, newAbilityGuid);
            if (existingObj != null && !(existingObj is BlueprintAbility))
            {
                Main.Mod.Logger.Log(
                    $"EVP: ForceBlast newAbilityGuid collision! {newAbilityGuid} is {existingObj.GetType().Name} ({existingObj.name})");
                return null;
            }

            // Slot feature: what we're replacing (Dimension Door etc). Keep for logging only.
            var slotFeat = BlueprintLibrary.GetBlueprint(library, donorSlotFeatureGuid) as BlueprintFeature;
            if (slotFeat == null)
            {
                Main.Mod.Logger.Log("EVP: donor slot feature not found: " + donorSlotFeatureGuid);
                return null;
            }

            // Template feature (Elemental Blast feature)
            var template = BlueprintLibrary.GetBlueprint(library, donorTemplateFeatureGuid) as BlueprintFeature;
            if (template == null)
            {
                Main.Mod.Logger.Log("EVP: Template feature not found: " + donorTemplateFeatureGuid);
                return null;
            }

            // Template grants an ABILITY via AddFacts (not AddKnownSpell)
            var templateAbility = FindFirstGrantedAbility(template);
            if (templateAbility == null)
            {
                Main.Mod.Logger.Log("EVP: Template feature has no granted BlueprintAbility via AddFacts: " +
                                    template.name);
                return null;
            }

            // Ensure cloned ability exists (clone from templateAbility, NOT from Dimension Door)
            var clonedAbility = BlueprintLibrary.GetBlueprint(library, newAbilityGuid) as BlueprintAbility;
            if (clonedAbility == null)
            {
                clonedAbility = BlueprintDeepClone.CloneAbilityIsolated(
                    templateAbility,
                    BlueprintLibrary.NormalizeGuid(newAbilityGuid));

                clonedAbility.name = "EvocationPlus_Arcane_ForceBlast_Ability_Level4";
                BlueprintLibrary.Register(library, newAbilityGuid, clonedAbility);
            }

            // Apply UI + force conversion every load
            EvocationPlusUnitFactText.SetNameKey(clonedAbility, nameKey);
            EvocationPlusUnitFactText.SetDescriptionKey(clonedAbility, descKey);
            SpellSchoolUtil.ReplaceDescriptor(clonedAbility, SpellDescriptor.Fire, SpellDescriptor.Force);
            ConvertAbilityDamageToForce(clonedAbility);
            // Feature GUID used in progression (your mod-owned feature)
            var newFeatureGuid = BloodlineGuids.EvpArcaneBloodlineSpellLevel9FeatureGuid;

            var existing = BlueprintLibrary.GetBlueprint(library, newFeatureGuid) as BlueprintFeature;
            if (existing != null)
            {
                EvocationPlusUnitFactText.SetNameKey(existing, nameKey);
                EvocationPlusUnitFactText.SetDescriptionKey(existing, descKey);

                // IMPORTANT: grant the ability via AddFacts (and remove any old granted ability)
                ReplaceGrantedAbilityInAddFacts(existing, templateAbility, clonedAbility);

                return existing;
            }

            // Clone feature isolated
            var clone = ScriptableObject.CreateInstance<BlueprintFeature>();
            clone.name = "EvocationPlus_Arcane_ForceBlast_BloodlineSpellLevel9";
            clone.AssetGuid = BlueprintLibrary.NormalizeGuid(newFeatureGuid);
            clone.IsClassFeature = true;
            clone.Ranks = 1;

            EvocationPlusUnitFactText.SetNameKey(clone, nameKey);
            EvocationPlusUnitFactText.SetDescriptionKey(clone, descKey);
            BlueprintUnitFactUI.SetIcon(clone, template.Icon);

            // clean AddFacts only
            var addFacts = ScriptableObject.CreateInstance<AddFacts>();
            addFacts.name = "EVP_ForceBlast_AddFacts";
            addFacts.Facts = new BlueprintUnitFact[] { clonedAbility };

            clone.ComponentsArray = new BlueprintComponent[] { addFacts };

            BlueprintLibrary.Register(library, newFeatureGuid, clone);
            return clone;
        }

        private static void ReplaceGrantedAbilityInAddFacts(
            BlueprintFeature feature,
            BlueprintAbility donorAbility,
            BlueprintAbility newAbility)
        {
            if (feature == null || donorAbility == null || newAbility == null) return;

            // Materialize once so we can test emptiness + iterate safely without .Length
            var addFactsList = feature.GetComponents<AddFacts>()?.ToList() ??
                               new System.Collections.Generic.List<AddFacts>();

            if (!addFactsList.Any())
            {
                // If the template didn’t have AddFacts (unexpected), add one cleanly
                var af = ScriptableObject.CreateInstance<AddFacts>();
                af.name = "EVP_AddFacts";
                af.Facts = new BlueprintUnitFact[] { newAbility };

                feature.ComponentsArray = (feature.ComponentsArray ?? Array.Empty<BlueprintComponent>())
                    .Concat(new BlueprintComponent[] { af })
                    .ToArray();

                return;
            }

            var donorGuid = donorAbility.AssetGuid;

            foreach (var af in addFactsList)
            {
                if (af?.Facts == null) continue;

                for (int j = 0;
                     j < af.Facts.Length;
                     j++) // <- this is an array; if you also want no .Length here, see note below
                {
                    var ab = af.Facts[j] as BlueprintAbility;
                    if (ab == null) continue;

                    if (ab.AssetGuid == donorGuid)
                        af.Facts[j] = newAbility;
                }
            }
        }

        private static BlueprintAbility FindFirstGrantedAbility(BlueprintFeature feature)
        {
            if (feature == null) return null;

            var addFacts = feature.GetComponents<AddFacts>();
            if (addFacts == null) return null;

            foreach (var af in addFacts)
            {
                if (af?.Facts == null) continue;
                foreach (var f in af.Facts)
                {
                    var ab = f as BlueprintAbility;
                    if (ab != null) return ab;
                }
            }

            return null;
        }

        private static void ConvertAbilityDamageToForce(BlueprintAbility ability)
        {
            try
            {
                var run = ability.GetComponent<AbilityEffectRunAction>();
                if (run?.Actions?.Actions == null) return;

                foreach (var a in run.Actions.Actions)
                {
                    ConvertActionToForce(a);
                }
            }
            catch (Exception ex)
            {
                Main.Mod.Logger.Log("EVP: ConvertAbilityDamageToForce failed for " + ability?.name + " : " + ex);
            }
        }

      
        private static void ConvertActionToForce(GameAction action)
        {
            if (action == null) return;

            var deal = action as ContextActionDealDamage;
            if (deal != null)
            {
                if (deal.DamageType.Type == DamageType.Energy &&
                    deal.DamageType.Energy == DamageEnergyType.Fire)
                {
                    var newType = new DamageTypeDescription
                    {
                        Type = DamageType.Force,
                        Energy = 0
                    };

                    deal.DamageType = newType;
                }

                return;
            }

            var conditional = action as Conditional;
            if (conditional != null)
            {
                if (conditional.IfTrue?.Actions != null)
                    foreach (var a in conditional.IfTrue.Actions)
                        ConvertActionToForce(a);

                if (conditional.IfFalse?.Actions != null)
                    foreach (var a in conditional.IfFalse.Actions)
                        ConvertActionToForce(a);

                return;
            }

            var saved = action as ContextActionSavingThrow;
            if (saved != null)
            {
                if (saved.Actions?.Actions != null)
                    foreach (var a in saved.Actions.Actions)
                        ConvertActionToForce(a);

                return;
            }

            var onCtx = action as ContextActionOnContextCaster;
            if (onCtx != null)
            {
                if (onCtx.Actions?.Actions != null)
                    foreach (var a in onCtx.Actions.Actions)
                        ConvertActionToForce(a);

                return;
            }
        }
    }
}