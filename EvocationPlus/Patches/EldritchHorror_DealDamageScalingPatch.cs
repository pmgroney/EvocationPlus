using System;
using EvocationPlus.Core;
using HarmonyLib;
using Kingmaker.ElementsSystem;
using Kingmaker.RuleSystem;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Actions;

namespace EvocationPlus.Patches
{
    [HarmonyPatch(typeof(ContextActionDealDamage), "RunAction")]
    public static class EldritchHorrorDealDamageScalingPatch
    {
        [HarmonyPrefix]
        public static void Prefix(ContextActionDealDamage __instance)
        {
            if (__instance == null) return;

            // TEMP DEBUG: log whenever ANY deal-damage runs with our name OR in our area
            // (keep it limited so logs don't explode)
            try
            {
                var data = ElementsContext.GetData<MechanicsContext.Data>();
                var ctx = data?.Context;
                if (ctx == null) return;

                var area = ctx.AssociatedBlueprint as BlueprintAbilityAreaEffect;
                if (area == null) return;

                // Use ordinal-ignore-case and trim to avoid hidden mismatch
                var guid = (area.AssetGuid ?? "").Trim();
                if (!string.Equals(guid, Guids.Spells.EldritchHorrorAreaEffectGuid, StringComparison.OrdinalIgnoreCase))
                    return;

                // now apply scaling (and log it)
                var cl = ctx.Params?.CasterLevel ?? 0;
                var extra = cl < 7 ? 0 : 1 + (cl - 7) / 2;
                var diceCount = 2 + extra;

                __instance.Value.DiceType = DiceType.D4;
                __instance.Value.DiceCountValue = new ContextValue
                    { ValueType = ContextValueType.Simple, Value = diceCount };
                __instance.Value.BonusValue = new ContextValue { ValueType = ContextValueType.Simple, Value = 0 };
                __instance.IsAoE = true;

                Main.Mod.Logger.Log(
                    $"EH ScalePatch: applied diceCount={diceCount} (extra={extra}) name='{__instance.name ?? "<null>"}' guid='{guid}' CL={cl}");
            }
            catch (Exception e)
            {
                Main.Mod.Logger.Log($"EH ScalePatch ERROR: {e}");
            }
        }
    }
}