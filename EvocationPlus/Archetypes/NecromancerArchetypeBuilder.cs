using System;
using System.Collections.Generic;
using System.Linq;
using EvocationPlus.BlueprintUtils;
using EvocationPlus.Patches;
using EvocationPlus.Utils;
using Kingmaker.Blueprints.Classes;
using Kingmaker.EntitySystem.Stats;

namespace EvocationPlus.Archetypes
{
    public sealed class NecromancerArchetypeBuilder : ArchetypeBuilderBase
    {
        public override ArchetypeBuildResult Build(ArchetypeBuildContext ctx)
        {
            var library = ctx.Library;
            var def = ctx.Def;
            var parent = ctx.ParentClass;

            // remove original bloodline selection
            var removeFeature = BlueprintLibrary.GetBlueprint(
                library,
                def.RemoveFeatureGuidLevel1) as BlueprintFeatureBase;

            if (removeFeature == null)
            {
                Main.Mod.Logger.Log("NecromancerBuilder: removeFeature is null.");
                return null;
            }

            // build/patch cloned bloodline progression
            var necroBloodlineProg = EvocationPlus.Patches.Bloodlines.NecroUndeadProgression.EnsureNecroUndeadProgression(library, parent);
            if (necroBloodlineProg == null) return null;

            var boneArmorBonus = BoneArmorInstaller.EnsureBoneArmorBonusFeature(
                library,
                UnitFactStringUtils.SetUnitFactStrings);

            if (boneArmorBonus == null)
            {
                Main.Mod.Logger.Log("NecromancerBuilder: Bone Armor bonus feature not created.");
                return null;
            }

            if (removeFeature.Icon != null)
                ReflectionUtils.SetPrivateField(boneArmorBonus, "m_Icon", removeFeature.Icon);

            // Add Stealth as a class skill (keep all existing parent class skills)
            ctx.Archetype.ReplaceClassSkills = true;

            var parentSkills = parent.ClassSkills;
            ctx.Archetype.ClassSkills = parentSkills
                .Concat(new[] { StatType.SkillPerception, StatType.SkillPersuasion })
                .Distinct()
                .ToArray();
            
            
            var masterOfDeath = EnsureMasterOfDeathUtil.EnsureMasterOfDeath(library, parent, removeFeature);
            if (masterOfDeath == null) return null;
            EnsureMasterOfDeathUtil.AddDeterminator(parent, masterOfDeath);
            var familiarSel = NecroFamiliarSelectionBuilder.EnsureNecroFamiliarSelection(library);
            if (familiarSel == null)
                Main.Mod.Logger.Log("EVP: Familiar selection not created; skipping.");
            
            return new ArchetypeBuildResult
            {
                RemoveFeatures = new[]
                {
                    new LevelEntry { Level = 1, Features = new List<BlueprintFeatureBase> { removeFeature } }
                },
                AddFeatures = new[]
                {
                    new LevelEntry
                    {
                        Level = 1,
                        Features = new List<BlueprintFeatureBase>
                        {
                            necroBloodlineProg,
                            masterOfDeath,
                            boneArmorBonus,
                            familiarSel
                        }
                    },
                    new LevelEntry { Level = 5, Features = new List<BlueprintFeatureBase> { boneArmorBonus } },
                    new LevelEntry { Level = 9, Features = new List<BlueprintFeatureBase> { boneArmorBonus } },
                    new LevelEntry { Level = 13, Features = new List<BlueprintFeatureBase> { boneArmorBonus } },
                    new LevelEntry { Level = 17, Features = new List<BlueprintFeatureBase> { boneArmorBonus } }
                },
                ApplyPatches = () =>
                {
                    // Move PatchSorcererTimeline here.
                    // (I strongly recommend changing the "missing only" behavior later)
                    PatchSorcererTimeline(parent, boneArmorBonus);
                }
            };
        }
        public override void EnsurePrerequisites(ArchetypeBuildContext ctx)
        {
            NecromancerSelectionInstaller.EnsureSelection(ctx.Library);
        }
        private static void PatchSorcererTimeline(
            BlueprintCharacterClass parent,
            BlueprintFeature boneArmorBonus)
        {
            var prog = parent.Progression;
            if (prog == null) return;

            // keep your existing AddLevelEntryIfMissingOnly for now
            // but we’ll likely replace it in the next step
        }
    }
}
