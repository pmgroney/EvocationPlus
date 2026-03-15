using HarmonyLib;
using Kingmaker.Blueprints;
using Kingmaker.UnitLogic.Abilities.Blueprints;

namespace EvocationPlus.Patches
{
    [HarmonyPatch(typeof(BlueprintAbility), nameof(BlueprintAbility.CanCastToDeadTarget), MethodType.Getter)]
    internal static class BlueprintAbilityCanCastToDeadTargetPatch
    {
        private static void Postfix(BlueprintAbility __instance, ref bool __result)
        {
            if (__result) return;

            var comp = __instance.GetComponent<AllowDeadTargetingComponentBase>();
            if (comp != null && comp.Allow(__instance))
                __result = true;
        }
    }
}