using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;

namespace EvocationPlus.BlueprintUtils
{
    public static class BlueprintLibrary
    {
        public static string NormalizeGuid(string s)
        {
            if (string.IsNullOrEmpty(s)) return s;

            s = s.Trim();

            // Strip braces if present: "{...}"
            if (s.Length >= 2 && s[0] == '{' && s[s.Length - 1] == '}')
                s = s.Substring(1, s.Length - 2);

            // Owlcat blueprint GUIDs are typically stored without dashes and lowercased
            return s.Replace("-", "").ToLowerInvariant();
        }

        public static BlueprintScriptableObject GetBlueprint(LibraryScriptableObject library, string assetId)
        {
            if (library?.BlueprintsByAssetId == null) return null;
            if (string.IsNullOrEmpty(assetId)) return null;

            var key = NormalizeGuid(assetId);
            assetId = assetId?.Trim();
            BlueprintScriptableObject bp;
            return library.BlueprintsByAssetId.TryGetValue(key, out bp) ? bp : null;
        }

        public static bool Contains(LibraryScriptableObject library, string assetId)
        {
            if (library?.BlueprintsByAssetId == null) return false;
            if (string.IsNullOrEmpty(assetId)) return false;

            return library.BlueprintsByAssetId.ContainsKey(NormalizeGuid(assetId));
        }

        /// <summary>
        ///     Registers a blueprint into the Library's authoritative lookup map.
        ///     Avoids relying on GetAllBlueprints() mutability (often returns a copy/iterator).
        ///     Also defensively sets bp.AssetGuid to match the normalized key.
        /// </summary>
        public static void Register(LibraryScriptableObject library, string assetId, BlueprintScriptableObject bp)
        {
            if (library?.BlueprintsByAssetId == null) return;
            if (bp == null) return;
            if (string.IsNullOrEmpty(assetId)) return;

            var key = NormalizeGuid(assetId);

            // Keep the blueprint object consistent with the library key.
            bp.AssetGuid = key;

            // The dictionary is the authoritative registry.
            library.BlueprintsByAssetId[key] = bp;
        }
        internal static IEnumerable<BlueprintScriptableObject> GetAllBlueprints()
        {
            // Kingmaker: this is the canonical library root once blueprints are loaded.
            var library = ResourcesLibrary.LibraryObject;
            if (library == null)
                return Enumerable.Empty<BlueprintScriptableObject>();

            // Most KM builds expose this method publicly.
            return library.GetAllBlueprints() ?? Enumerable.Empty<BlueprintScriptableObject>();
        }

        /// <summary>
        /// Enumerate every loaded blueprint of type T.
        /// </summary>
        internal static IEnumerable<T> GetAllBlueprints<T>() where T : BlueprintScriptableObject
        {
            return GetAllBlueprints().OfType<T>();
        }

        /// <summary>
        /// Resolve a blueprint by GUID (returns null if missing).
        /// </summary>
        internal static T TryGet<T>(string guid) where T : BlueprintScriptableObject
        {
            if (string.IsNullOrWhiteSpace(guid)) return null;
            guid = NormalizeGuid(guid);

            // KM typically exposes this generic.
            return ResourcesLibrary.TryGetBlueprint<T>(guid);
        }
    }
}