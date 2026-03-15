using System;
using Kingmaker.ElementsSystem;
using Kingmaker.UnitLogic.Mechanics.Actions;

namespace EvocationPlus.Utils
{
    public static class ActionListUtil
    {
        public static int Patch(ActionList list, Func<GameAction, int> patchFunc)
        {
            if (list?.Actions == null) return 0;

            var changed = 0;
            for (var i = 0; i < list.Actions.Length; i++)
                changed += PatchRecursive(list.Actions[i], patchFunc);

            return changed;
        }

        private static int PatchRecursive(GameAction action, Func<GameAction, int> patchFunc)
        {
            if (action == null) return 0;

            var changed = patchFunc(action);

            // Handle common wrappers once, centrally
            if (action is ContextActionConditionalSaved saved)
            {
                changed += Patch(saved.Succeed, patchFunc);
                changed += Patch(saved.Failed, patchFunc);
            }

            return changed;
        }

        public static int PatchDealDamage(ActionList list, Action<ContextActionDealDamage> patch)
        {
            if (patch == null) return 0;

            return Patch(list, a =>
            {
                if (a is ContextActionDealDamage deal)
                {
                    patch(deal);
                    return 1;
                }

                return 0;
            });
        }
    }
}