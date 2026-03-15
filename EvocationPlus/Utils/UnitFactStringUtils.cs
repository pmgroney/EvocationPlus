using System;
using System.Reflection;
using Kingmaker.Blueprints.Facts;

namespace EvocationPlus.Utils
{
    internal static class UnitFactStringUtils
    {
        private static readonly FieldInfo DisplayNameField =
            typeof(BlueprintUnitFact).GetField("m_DisplayName", BindingFlags.Instance | BindingFlags.NonPublic);

        private static readonly FieldInfo DescriptionField =
            typeof(BlueprintUnitFact).GetField("m_Description", BindingFlags.Instance | BindingFlags.NonPublic);

        public static void SetUnitFactStrings(BlueprintUnitFact fact, string nameKey, string descKey)
        {
            if (fact == null) throw new ArgumentNullException(nameof(fact));
            if (string.IsNullOrEmpty(nameKey)) throw new ArgumentException("nameKey is null/empty", nameof(nameKey));
            if (string.IsNullOrEmpty(descKey)) throw new ArgumentException("descKey is null/empty", nameof(descKey));

            if (DisplayNameField == null)
                throw new MissingFieldException(typeof(BlueprintUnitFact).FullName, "m_DisplayName");
            if (DescriptionField == null)
                throw new MissingFieldException(typeof(BlueprintUnitFact).FullName, "m_Description");

            DisplayNameField.SetValue(fact, LocalizedStringUtils.Create(nameKey));
            DescriptionField.SetValue(fact, LocalizedStringUtils.Create(descKey));
        }
    }
}