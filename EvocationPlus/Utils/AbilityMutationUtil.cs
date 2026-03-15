using System;
using Kingmaker.Blueprints;
using Kingmaker.ElementsSystem;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;

namespace EvocationPlus.Utils
{
    internal static class AbilityMutationUtil
    {
        internal static AbilityEffectRunAction EnsureIsolatedRunActions(BlueprintAbility ability)
        {
            if (ability == null) return null;

            var run = ability.GetComponent<AbilityEffectRunAction>();
            if (run == null) return null;

            // If we've already isolated it, do nothing.
            // (Cheap sentinel: name marker or a private bool isn't available; name marker is OK for modded assets.)
            if (run.name.Contains("_EVP_Isolated"))
                return run;

            var runClone = UnityEngine.Object.Instantiate(run);
            runClone.name = (runClone.name ?? "AbilityEffectRunAction") + "_EVP_Isolated";

            var src = run.Actions;
            if (src.Actions != null)
            {
                var clonedActions = new GameAction[src.Actions.Length];
                for (int i = 0; i < src.Actions.Length; i++)
                {
                    var a = src.Actions[i];
                    clonedActions[i] = a == null ? null : UnityEngine.Object.Instantiate(a);
                }

                runClone.Actions = new ActionList { Actions = clonedActions };
            }

            // Replace component on the ability
            var comps = ability.ComponentsArray ?? Array.Empty<BlueprintComponent>();
            for (int i = 0; i < comps.Length; i++)
            {
                if (!ReferenceEquals(comps[i], run)) continue;
                comps[i] = runClone;
                ability.ComponentsArray = comps;
                return runClone;
            }

            return runClone;
        }
    }
}