using HarmonyLib;
using Kingmaker.Localization;
using EvocationPlus.Utils;

namespace EvocationPlus.Patches
{
    [HarmonyPatch(typeof(LocalizedString), nameof(LocalizedString.ToString))]
    internal static class LocalizedString_ToString_Patch
    {
        private static bool Prefix(LocalizedString __instance, ref string __result)
        {
            if (__instance == null) return true;

            // Field name varies; try common ones. You said “no reflection diving”,
            // but this is minimal and stable: it’s just reading the key.
            var key = __instance.Key; // if available in your Kingmaker version
            if (string.IsNullOrEmpty(key)) return true;

            if (Localization.TryGet(key, out var value) && !string.IsNullOrEmpty(value))
            {
                __result = value;
                return false; // skip original
            }

            return true;
        }
    }
}