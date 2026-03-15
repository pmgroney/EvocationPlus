using System;
using Kingmaker.ElementsSystem;
using Kingmaker.UnitLogic.Mechanics.Actions;

namespace EvocationPlus.Spells
{
    internal static class SpellActionPatcher
    {
        public static int PatchAllDealDamage(GameAction[] actions, Action<ContextActionDealDamage> patch)
        {
            if (actions == null || patch == null) return 0;

            var changed = 0;
            for (var i = 0; i < actions.Length; i++)
                changed += PatchAction(actions[i], patch);

            return changed;
        }

        private static int PatchAction(GameAction action, Action<ContextActionDealDamage> patch)
        {
            if (action == null) return 0;

            var deal = action as ContextActionDealDamage;
            if (deal != null)
            {
                patch(deal);
                return 1;
            }

            var saved = action as ContextActionConditionalSaved;
            if (saved != null)
            {
                var c = 0;

                if (saved.Succeed?.Actions != null)
                    for (var i = 0; i < saved.Succeed.Actions.Length; i++)
                        c += PatchAction(saved.Succeed.Actions[i], patch);

                if (saved.Failed?.Actions != null)
                    for (var i = 0; i < saved.Failed.Actions.Length; i++)
                        c += PatchAction(saved.Failed.Actions[i], patch);

                return c;
            }

            return 0;
        }
    }
}