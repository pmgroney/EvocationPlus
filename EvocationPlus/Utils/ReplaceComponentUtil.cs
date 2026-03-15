using Kingmaker.Blueprints;

namespace EvocationPlus.Utils
{
    internal static class ReplaceComponentUtil
    {
        public static bool ReplaceComponent(BlueprintScriptableObject bp, BlueprintComponent oldC,
            BlueprintComponent newC)
        {
            if (bp == null || oldC == null || newC == null) return false;

            var arr = bp.ComponentsArray;
            for (var i = 0; i < arr.Length; i++)
                if (ReferenceEquals(arr[i], oldC))
                {
                    arr[i] = newC;
                    bp.ComponentsArray = arr;
                    return true;
                }

            return false;
        }
    }
}