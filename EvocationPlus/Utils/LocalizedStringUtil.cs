using System;
using System.Reflection;
using Kingmaker.Localization;

namespace EvocationPlus.Utils
{
    internal static class LocalizedStringUtils
    {
        private static readonly FieldInfo KeyField =
            typeof(LocalizedString).GetField("m_Key", BindingFlags.Instance | BindingFlags.NonPublic);

        public static LocalizedString Create(string key)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentException("Localization key is null/empty.", nameof(key));

            if (KeyField == null)
                throw new MissingFieldException(typeof(LocalizedString).FullName, "m_Key");

            var ls = new LocalizedString();
            KeyField.SetValue(ls, key);
            return ls;
        }

        // Optional helpers (useful for debugging / logging)
        public static string GetKey(LocalizedString ls)
        {
            if (ls == null) return null;
            if (KeyField == null) return null;
            return (string)KeyField.GetValue(ls);
        }

        public static void SetKey(LocalizedString ls, string key)
        {
            if (ls == null)
                throw new ArgumentNullException(nameof(ls));
            if (string.IsNullOrEmpty(key))
                throw new ArgumentException("Localization key is null/empty.", nameof(key));
            if (KeyField == null)
                throw new MissingFieldException(typeof(LocalizedString).FullName, "m_Key");

            KeyField.SetValue(ls, key);
        }
    }
}