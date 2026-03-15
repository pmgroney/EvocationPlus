using System.IO;
using EvocationPlus.BlueprintUtils;
using EvocationPlus.Core;
using EvocationPlus.IconUtils;
using EvocationPlus.Patches;
using EvocationPlus.Patches.Bloodlines;
using EvocationPlus.Utils;
using HarmonyLib;
using Kingmaker;
using Kingmaker.EntitySystem.Persistence;
using Kingmaker.GameModes;
using Kingmaker.Localization;
using UnityModManagerNet;

namespace EvocationPlus
{
    public static class Main
    {
        private static Harmony _harmony;
        public static UnityModManager.ModEntry Mod { get; private set; }
        public static bool Enabled { get; private set; } = true;

        public static bool Load(UnityModManager.ModEntry modEntry)
        {
            Mod = modEntry;
            modEntry.OnToggle = OnToggle;
            DiskIconLoader.IconsRootDir = Path.Combine(Mod.Path, "Icons");
            // Load our localization dictionary
            Localization.LoadFromResx(modEntry);

            // ABILITIES
            SpellIconRegistry.RegisterAbility(Guids.Spells.BoneSpike, @"Icons\bone_spike.png");
            SpellIconRegistry.RegisterAbility(Guids.Spells.CorpseExplosion, @"Icons\corpse_explosion.png");
            SpellIconRegistry.RegisterAbility(Guids.Spells.EldritchHorror, @"Icons\eldritch_horror.png");
            SpellIconRegistry.RegisterAbility(Guids.Spells.VitriolicBlast, @"Icons\vitriolic_blast.png");
            SpellIconRegistry.RegisterAbility(Guids.Spells.DeathRay, @"Icons\death_ray.png");
            SpellIconRegistry.RegisterAbility(Guids.Spells.ForceRay, @"Icons\force_ray_arcane.png");
            SpellIconRegistry.RegisterAbility(Guids.Spells.HellOnEarth, @"Icons\hell_on_earth.png");
            SpellIconRegistry.RegisterAbility(Guids.Spells.EmperorsWrath, @"Icons\emperors_wrath.png");
            // UNIT FACTS (your bloodline spell FEATURES)
            SpellIconRegistry.RegisterUnitFact(Guids.Spells.BoneSpikeFeature, @"Icons\bone_spike.png");
            SpellIconRegistry.RegisterUnitFact(Guids.Spells.CorpseExplosionFeature, @"Icons\corpse_explosion.png");
            SpellIconRegistry.RegisterUnitFact(Guids.Spells.EldritchHorrorFeature, @"Icons\eldritch_horror.png");
            SpellIconRegistry.RegisterUnitFact(Guids.Spells.ForceRayFeature, @"Icons\force_ray_arcane.png");
            SpellIconRegistry.RegisterUnitFact(BloodlineGuids.EvpArcaneSpellLevel4Guid, @"Icons\force_blast.png");
            SpellIconRegistry.RegisterUnitFact(BloodlineGuids.EvpArcaneBloodlineSpellLevel9FeatureGuid, @"Icons\force_blast.png");
            SpellIconRegistry.RegisterUnitFact(Guids.Spells.HellOnEarthFeature, @"Icons\hell_on_earth.png");
            // Harmony
            _harmony = new Harmony(modEntry.Info.Id);
            // Apply ALL attribute-based patches 
            _harmony.PatchAll(typeof(Main).Assembly);
            // Also apply the explicit cast patch
            PatchLocalizedStringCast(_harmony);
            Mod.Logger.Log("EvocationPlus loaded");
            return true;
        }

        private static void PatchLocalizedStringCast(Harmony harmony)
        {
            var original = AccessTools.Method(
                typeof(LocalizedString),
                "op_Implicit",
                new[] { typeof(LocalizedString) });

            if (original == null)
            {
                Mod.Logger.Log("EvocationPlus: LocalizedString.op_Implicit not found.");
                return;
            }

            var prefix = new HarmonyMethod(
                typeof(LocalizedStringImplicitPatch),
                nameof(LocalizedStringImplicitPatch.Prefix));

            harmony.Patch(original, prefix);
        }


        private static bool OnToggle(UnityModManager.ModEntry modEntry, bool value)
        {
            Enabled = value;
            return true;
        }
    }
}