using System.Collections.Generic;
using EvocationPlus.BlueprintUtils;
using EvocationPlus.Core;
using EvocationPlus.Patches.Bloodlines;
using EvocationPlus.Patches.Spellbooks;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Selection;

namespace EvocationPlus.Archetypes
{
    public sealed class EvokerArchetypeBuilder : ArchetypeBuilderBase
    {
        public override void EnsurePrerequisites(ArchetypeBuildContext ctx)
        {
            EvokerBloodlineSelectionInstaller.EnsureSelection(ctx.Library);
            EvokerSpellbookInstaller.EnsureEvokerSpellbook(ctx.Library);
            EnsureHarbingerOfBoomUtil.EnsureHarbingerOfBoom(ctx.Library, ctx.ParentClass);
        }

        public override ArchetypeBuildResult Build(ArchetypeBuildContext ctx)
        {
            var library = ctx.Library;
            var def = ctx.Def;

            var vanillaSelection =
                BlueprintLibrary.GetBlueprint(library, BloodlineGuids.SorcererBloodlineSelectionGuid) as BlueprintFeatureSelection;

            if (vanillaSelection == null)
            {
                Main.Mod.Logger.Log($"EVP: EvokerBuilder: vanilla bloodline selection not found (guid={BloodlineGuids.SorcererBloodlineSelectionGuid}).");
                return null;
            }

            var evokerSelection =
                BlueprintLibrary.GetBlueprint(library, def.AddFeatureGuidLevel1) as BlueprintFeatureSelection;

            if (evokerSelection == null)
            {
                Main.Mod.Logger.Log($"EVP: EvokerBuilder: evoker bloodline selection not found (guid={def.AddFeatureGuidLevel1}).");
                return null;
            }

            var evokerSpellbook = EvokerSpellbookInstaller.EnsureEvokerSpellbook(library);
            if (evokerSpellbook != null)
                ArchetypeSpellbookUtil.SetReplaceSpellbook(ctx.Archetype, evokerSpellbook);
            else
                Main.Mod.Logger.Log("EVP: EvokerBuilder: evoker spellbook missing; spells not restricted.");

            // Archetype feature (not bloodline-related)
            var harbinger = EnsureHarbingerOfBoomUtil.EnsureHarbingerOfBoom(ctx.Library, ctx.ParentClass);
            EnsureHarbingerOfBoomUtil.AddAsClassDeterminator(ctx.ParentClass, harbinger);

            var addLevel1 = new List<BlueprintFeatureBase> { evokerSelection };
            if (harbinger != null) addLevel1.Add(harbinger);

            return new ArchetypeBuildResult
            {
                RemoveFeatures = new[]
                {
                    new LevelEntry
                    {
                        Level = 1,
                        Features = new List<BlueprintFeatureBase> { vanillaSelection }
                    }
                },
                AddFeatures = new[]
                {
                    new LevelEntry
                    {
                        Level = 1,
                        Features = addLevel1
                    }
                }
            };
        }
    }
}
