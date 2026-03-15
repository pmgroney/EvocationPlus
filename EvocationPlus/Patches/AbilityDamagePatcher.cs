using Kingmaker.Blueprints;
using Kingmaker.ElementsSystem;
using Kingmaker.Enums.Damage;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Mechanics.Actions;

namespace EvocationPlus.Patches
{
    /// <summary>
    ///     Utility to patch a BlueprintAbility's damage actions from Acid energy to Negative Energy.
    ///     Safe when used ONLY on your cloned ability, not on donor blueprints.
    /// </summary>
    internal static class AbilityDamagePatcher
    {
        /// <summary>
        ///     Walks AbilityEffectRunAction and converts Acid energy damage to Negative Energy.
        /// </summary>
        public static void PatchAbilityDamageToNegativeEnergy(BlueprintAbility ability)
        {
            if (ability == null) return;

            var runAction = ability.GetComponent<AbilityEffectRunAction>();
            var actions = runAction?.Actions?.Actions;
            if (actions == null || actions.Length == 0) return;

            for (var i = 0; i < actions.Length; i++)
                PatchAction(actions[i]);
        }

        private static void PatchAction(GameAction action)
        {
            if (action == null) return;

            // Direct damage action
            if (action is ContextActionDealDamage deal)
            {
                // dt is a struct in Kingmaker; read-modify-write
                var dt = deal.DamageType;

                // Be precise: only convert Acid energy damage
                if (dt.Type == DamageType.Energy && dt.Energy == DamageEnergyType.Acid)
                {
                    dt.Energy = DamageEnergyType.NegativeEnergy;
                    deal.DamageType = dt;
                }

                return;
            }

            // Wrapper: conditional saved
            if (action is ContextActionConditionalSaved saved)
            {
                var succ = saved.Succeed?.Actions;
                if (succ != null)
                    for (var i = 0; i < succ.Length; i++)
                        PatchAction(succ[i]);

                var fail = saved.Failed?.Actions;
                if (fail != null)
                    for (var i = 0; i < fail.Length; i++)
                        PatchAction(fail[i]);
            }

            // Add more wrappers here if needed (e.g., ContextActionOnContextCaster, ContextActionOnTargets, etc.)
        }
    }
}