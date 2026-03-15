namespace EvocationPlus.Archetypes
{
    public abstract class ArchetypeBuilderBase : IArchetypeBuilder
    {
        public virtual void EnsurePrerequisites(ArchetypeBuildContext ctx) { }
        public abstract ArchetypeBuildResult Build(ArchetypeBuildContext ctx);
    }
}