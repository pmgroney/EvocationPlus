using System.Reflection;
using EvocationPlus.Utils;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Localization;
using UnityEngine;

namespace EvocationPlus.BlueprintUtils
{
    public static class EvocationPlusUnitFactText
    {
        private static readonly FieldInfo DisplayNameField =
            typeof(BlueprintUnitFact).GetField("m_DisplayName", BindingFlags.Instance | BindingFlags.NonPublic);

        private static readonly FieldInfo DescriptionField =
            typeof(BlueprintUnitFact).GetField("m_Description", BindingFlags.Instance | BindingFlags.NonPublic);

        private static readonly FieldInfo IconField =
            typeof(BlueprintUnitFact).GetField("m_Icon", BindingFlags.Instance | BindingFlags.NonPublic);

        private static readonly FieldInfo LocalizedStringKeyField =
            typeof(LocalizedString).GetField("m_Key", BindingFlags.Instance | BindingFlags.NonPublic);

        public static void SetNameKey(BlueprintUnitFact fact, string key)
        {
            if (fact == null || string.IsNullOrEmpty(key) || DisplayNameField == null) return;

            var ls = (LocalizedString)DisplayNameField.GetValue(fact);
            ls = EnsureLocalizedStringWithKey(ls, key);
            DisplayNameField.SetValue(fact, ls);
        }

        public static void SetDescriptionKey(BlueprintUnitFact fact, string key)
        {
            if (fact == null || string.IsNullOrEmpty(key) || DescriptionField == null) return;

            var ls = (LocalizedString)DescriptionField.GetValue(fact);
            ls = EnsureLocalizedStringWithKey(ls, key);
            DescriptionField.SetValue(fact, ls);
        }
        public static string GetNameKey(BlueprintUnitFact fact)
        {
            if (fact == null || DisplayNameField == null || LocalizedStringKeyField == null) return null;
            var ls = (LocalizedString)DisplayNameField.GetValue(fact);
            return ls != null ? (string)LocalizedStringKeyField.GetValue(ls) : null;
        }

        public static string GetDescriptionKey(BlueprintUnitFact fact)
        {
            if (fact == null || DescriptionField == null || LocalizedStringKeyField == null) return null;
            var ls = (LocalizedString)DescriptionField.GetValue(fact);
            return ls != null ? (string)LocalizedStringKeyField.GetValue(ls) : null;
        }
        public static void SetIcon(BlueprintUnitFact fact, Sprite icon)
        {
            if (fact == null || IconField == null) return;
            IconField.SetValue(fact, icon);
        }

        private static LocalizedString EnsureLocalizedStringWithKey(LocalizedString ls, string key)
        {
            if (ls == null)
                ls = new LocalizedString();

            // Prefer your existing util (single source of truth)
            LocalizedStringUtils.SetKey(ls, key);

            // If you want to avoid LocalizedStringUtils reflection, you could do:
            // if (LocalizedStringKeyField != null) LocalizedStringKeyField.SetValue(ls, key);

            return ls;
        }
    }
}