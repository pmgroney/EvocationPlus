namespace EvocationPlus.Archetypes
{
    public interface IArchetypeBuilder
    {
        void EnsurePrerequisites(ArchetypeBuildContext ctx);
        ArchetypeBuildResult Build(ArchetypeBuildContext ctx);
    }
}