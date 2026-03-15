using System;
using System.Reflection;
using EvocationPlus.Utils;
using Kingmaker.Localization;

namespace EvocationPlus.Patches
{
    public static class LocalizedStringImplicitPatch
    {
        private static FieldInfo _keyField;

        // IMPORTANT: static method parameter is the LocalizedString argument
        public static bool Prefix(LocalizedString localizedString, ref string __result)
        {
            if (localizedString == null)
            {
                __result = null;
                return false;
            }

            var key = GetKey(localizedString);
            if (!string.IsNullOrEmpty(key) && Localization.TryGet(key, out var value) && value != null)
            {
                __result = value;
                return false; // skip original op_Implicit
            }

            return true; // fall back to game's localization
        }

        private static string GetKey(LocalizedString ls)
        {
            if (_keyField == null)
            {
                var flags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
                foreach (var f in typeof(LocalizedString).GetFields(flags))
                {
                    if (f.FieldType != typeof(string)) continue;
                    var n = f.Name ?? "";
                    if (n.IndexOf("key", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        _keyField = f;
                        break;
                    }
                }
            }

            return _keyField == null ? null : (string)_keyField.GetValue(ls);
        }
    }
}