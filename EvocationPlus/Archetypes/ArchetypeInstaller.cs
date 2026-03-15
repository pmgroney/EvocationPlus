using System.Collections.Generic;
using System.Linq;
using EvocationPlus.BlueprintUtils;
using EvocationPlus.Patches.Bloodlines;
using EvocationPlus.Spells.Infrastructure;
using EvocationPlus.Utils;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using UnityEngine;

namespace EvocationPlus.Archetypes
{
    public static class ArchetypeInstaller
    {
        public static void InstallAll(LibraryScriptableObject library)
        {
            if (library == null)
            {
                Main.Mod.Logger.Log("EVP: ArchetypeInstaller.InstallAll: library was null (skipping).");
                return;
            }

            if (library.BlueprintsByAssetId == null)
            {
                Main.Mod.Logger.Log("EVP: ArchetypeInstaller.InstallAll: BlueprintsByAssetId was null (skipping).");
                return;
            }

            var regs = ArchetypeRegistry.GetAll()?.ToList() ?? new List<ArchetypeRegistration>();

            foreach (var reg in regs)
                InstallOne(library, reg);

            SorcererBloodlineFeatSelectionInstaller.EnsureArchetypeFeatSelectionAddedToSorcererFeatSelection(library);
        }

        private static void InstallOne(LibraryScriptableObject library, ArchetypeRegistration reg)
        {
            var def = reg.Def;
            var builder = reg.Builder;

            if (builder == null)
            {
                Main.Mod.Logger.Log("EVP: ArchetypeInstaller: builder was null for " + def?.InternalName);
                return;
            }

            var parent = GetParentClass(library, def);
            if (parent == null) return;

            if (ArchetypeAlreadyExists(library, def))
                return;

            var archetype = CreateArchetype(def, parent);

            // Context includes the freshly-created archetype instance
            var ctx = new ArchetypeBuildContext(library, def, parent, archetype);

            // Builder prereqs first (selections, cloned progressions, spellbooks, etc.)
            builder.EnsurePrerequisites(ctx);

            // Builder returns Add/Remove LevelEntries + optional patch action
            var result = builder.Build(ctx);
            if (result == null) return;

            archetype.RemoveFeatures = result.RemoveFeatures ?? new LevelEntry[0];
            archetype.AddFeatures    = result.AddFeatures    ?? new LevelEntry[0];

            result.ApplyPatches?.Invoke();

            RegisterAndAttach(library, def, parent, archetype);
        }

        private static BlueprintCharacterClass GetParentClass(
            LibraryScriptableObject library,
            ArchetypeDefinition def)
        {
            var parent = BlueprintLibrary.GetBlueprint(library, def.ParentClassGuid)
                as BlueprintCharacterClass;

            if (parent == null)
                Main.Mod.Logger.Log("ArchetypeInstaller: parent class not found.");

            return parent;
        }

        private static bool ArchetypeAlreadyExists(LibraryScriptableObject library, ArchetypeDefinition def)
            => BlueprintLibrary.Contains(library, def.ArchetypeGuid);

        private static BlueprintArchetype CreateArchetype(
            ArchetypeDefinition def,
            BlueprintCharacterClass parent)
        {
            var archetype = ScriptableObject.CreateInstance<BlueprintArchetype>();

            archetype.name = def.InternalName;
            archetype.AssetGuid = BlueprintLibrary.NormalizeGuid(def.ArchetypeGuid);

            ReflectionUtils.SetPrivateField(archetype, "m_ParentClass", parent);

            archetype.LocalizedName =
                LocalizedStringUtils.Create(def.DisplayNameKey);

            archetype.LocalizedDescription =
                LocalizedStringUtils.Create(def.DescriptionKey);

            return archetype;
        }

        private static void RegisterAndAttach(
            LibraryScriptableObject library,
            ArchetypeDefinition def,
            BlueprintCharacterClass parent,
            BlueprintArchetype archetype)
        {
            BlueprintLibrary.Register(library, def.ArchetypeGuid, archetype);

            parent.Archetypes = (parent.Archetypes ?? new BlueprintArchetype[0])
                .Concat(new[] { archetype })
                .ToArray();
        }
        
    }
}