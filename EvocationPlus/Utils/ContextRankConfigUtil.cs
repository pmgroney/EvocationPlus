using HarmonyLib;
using Kingmaker.UnitLogic.Mechanics.Components;

namespace EvocationPlus.Utils
{
    internal static class ContextRankConfigUtil
    {
        public static bool ForceMinMax(ContextRankConfig rank, int min, int max)
        {
            var useMin = AccessTools.Field(typeof(ContextRankConfig), "m_UseMin");
            var minF = AccessTools.Field(typeof(ContextRankConfig), "m_Min");
            var useMax = AccessTools.Field(typeof(ContextRankConfig), "m_UseMax");
            var maxF = AccessTools.Field(typeof(ContextRankConfig), "m_Max");

            if (useMin == null || minF == null || useMax == null || maxF == null)
                return false;

            useMin.SetValue(rank, true);
            minF.SetValue(rank, min);
            useMax.SetValue(rank, true);
            maxF.SetValue(rank, max);
            return true;
        }
    }
}