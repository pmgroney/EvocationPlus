using System.Collections.Generic;
using EvocationPlus.Core;
using EvocationPlus.Spells.Implementation.Modifiers;

namespace EvocationPlus.Spells.Infrastructure
{
    public static class SpellRegistry
    {
        // Add new spells here.
        public static List<SpellDefinition> GetAll()
        {
            var spells = new List<SpellDefinition>();

            spells.Add(new SpellDefinition(
                Guids.Spells.ScorchingRay, // base spell asset id
                Guids.Spells.EmperorsWrath, // new spell asset id
                "EvocationPlus_EmperorsWrath", // internal blueprint name
                2, // spell level
                new EmperorsWrathModifier()
            ));

            spells.Add(new SpellDefinition(
                Guids.Spells.ScorchingRay, // base spell asset id
                Guids.Spells.FrostBlast, // new spell asset id
                "EvocationPlus_FrostBlast", // internal blueprint name
                2, // spell level
                new FrostBlastModifier()
            ));
            
            spells.Add(new SpellDefinition(
                Guids.Spells.ScorchingRay, // base spell asset id
                Guids.Spells.CausticBeam, // new spell asset id
                "EvocationPlus_CausticBeam", // internal blueprint name
                2, // spell level
                new CausticBeamModifier()
            ));
            spells.Add(new SpellDefinition(
                Guids.Spells.ScorchingRay, // base spell asset id
                Guids.Spells.ForceRay, // new spell asset id
                "EvocationPlus_ForceRay", // internal blueprint name
                2, // spell level
                new ForceRayModifier()
            ));
            spells.Add(new SpellDefinition(
                Guids.Spells.MagicMissile, // base spell asset id
                Guids.Spells.BoneSpike, // new spell asset id
                "EvocationPlus_BoneSpike", // internal blueprint name
                1, // spell level
                new BoneSpikeModifier()
            ));

            spells.Add(new SpellDefinition(
                Guids.Spells.MagicMissile, // base spell asset id
                Guids.Spells.AcidMissile, // new spell asset id
                "EvocationPlus_AcidMissile", // internal blueprint name
                1, // spell level
                new AcidMissileModifier()
            ));
            
            spells.Add(new SpellDefinition(
                Guids.Spells.MagicMissile, // base spell asset id
                Guids.Spells.FireMissile, // new spell asset id
                "EvocationPlus_FireMissile", // internal blueprint name
                1, // spell level
                new FireMissileModifier()));

            spells.Add(new SpellDefinition(
                Guids.Spells.MagicMissile, // base spell asset id
                Guids.Spells.ElectricMissile, // new spell asset id
                "EvocationPlus_ElectricMissile", // internal blueprint name
                1, // spell level
                new ElectricMissileModifier()));

            spells.Add(new SpellDefinition(
                Guids.Spells.MagicMissile, // base spell asset id
                Guids.Spells.IceMissile, // new spell asset id
                "EvocationPlus_IceMissile", // internal blueprint name
                1, // spell level
                new IceMissileModifier()));
            
            spells.Add(new SpellDefinition(
                Guids.Spells.Fireball, // base spell asset id
                Guids.Spells.VitriolicBlast, // new spell asset id
                "EvocationPlus_VitriolicBurst", // internal blueprint name
                3, // spell level
                new VitriolicBurstModifier()
            ));

            spells.Add(new SpellDefinition(
                Guids.Spells.Fireball, // base spell asset id
                Guids.Spells.CorpseExplosion, // new spell asset id
                "EvocationPlus_CorpseExplosion", // internal blueprint name
                2, // spell level
                new CorpseExplosionModifier()
            ));
            spells.Add(new SpellDefinition(
                Guids.Spells.Entangle, // base spell asset id
                Guids.Spells.EldritchHorror, // new spell asset id
                "EvocationPlus_EldritchHorror", // internal blueprint name
                3, // spell level
                new EldritchHorrorModifier()
            ));
            spells.Add(new SpellDefinition(
                Guids.Spells.Entangle, // base spell asset id
                Guids.Spells.HellOnEarth, // new spell asset id
                "EvocationPlus_HellOnEarth", // internal blueprint name
                9, // spell level
                new HellOnEarthModifier()
            ));
            spells.Add(new SpellDefinition(
                Guids.Spells.ScorchingRay, // base spell asset id
                Guids.Spells.DeathRay, // new spell asset id
                "EvocationPlus_DeathRay", // internal blueprint name
                2, // spell level
                new DeathRayModifier()
            ));

            return spells;
        }
    }
}