namespace EvocationPlus.Archetypes
{
    public sealed class ArchetypeDefinition
    {
        public readonly string AddFeatureGuidLevel1;
        public readonly string ArchetypeGuid;
        public readonly string DescriptionKey;
        public readonly string DisplayNameKey;
        public readonly string InternalName;
        public readonly string ParentClassGuid;
        public readonly string RemoveFeatureGuidLevel1;
        public readonly bool KeepBaseBloodlineSelection;
        public ArchetypeDefinition(
            string parentClassGuid,
            string archetypeGuid,
            string internalName,
            string displayNameKey,
            string descriptionKey,
            string removeFeatureGuidLevel1,
            string addFeatureGuidLevel1,
            bool keepBaseBloodlineSelection = false)
        {
            ParentClassGuid = parentClassGuid;
            ArchetypeGuid = archetypeGuid;
            InternalName = internalName;
            DisplayNameKey = displayNameKey;
            DescriptionKey = descriptionKey;
            RemoveFeatureGuidLevel1 = removeFeatureGuidLevel1;
            AddFeatureGuidLevel1 = addFeatureGuidLevel1;
            KeepBaseBloodlineSelection = keepBaseBloodlineSelection;
        }
    }
}