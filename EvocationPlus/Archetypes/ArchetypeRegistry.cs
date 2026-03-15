using System.Collections.Generic;
using EvocationPlus.Core;
using EvocationPlus.Patches.Bloodlines;

namespace EvocationPlus.Archetypes
{
    public sealed class ArchetypeRegistration
    {
        public ArchetypeDefinition Def { get; }
        public IArchetypeBuilder Builder { get; }

        public ArchetypeRegistration(ArchetypeDefinition def, IArchetypeBuilder builder)
        {
            Def = def;
            Builder = builder;
        }
    }

    public static class ArchetypeRegistry
    {
        public static List<ArchetypeRegistration> GetAll()
        {
            return new List<ArchetypeRegistration>
            {
                new ArchetypeRegistration(
                    new ArchetypeDefinition(
                        Guids.Features.SorcererClassGuid,
                        BloodlineGuids.NecromancerArchetypeGuid,
                        "EvocationPlus_NecromancerArchetype",
                        "EVP_NECROMANCER_NAME",
                        "EVP_NECROMANCER_DESC",
                        BloodlineGuids.SorcererBloodlineSelectionGuid,
                        BloodlineGuids.NecromancerBloodlineSelectionGuid
                    ),
                    new NecromancerArchetypeBuilder()
                ),
                new ArchetypeRegistration(
                    new ArchetypeDefinition(
                        Guids.Features.SorcererClassGuid, 
                        BloodlineGuids.EvokerArchetypeGuid,
                        "EvocationPlus_EvokerArchetype",
                        "EVP_EVOKER_NAME",
                        "EVP_EVOKER_DESC",
                        BloodlineGuids.SorcererBloodlineSelectionGuid, 
                        BloodlineGuids.EvokerBloodlineSelectionGuid 
                    ),
                    new EvokerArchetypeBuilder()
                ),
            };
        }
    }
}