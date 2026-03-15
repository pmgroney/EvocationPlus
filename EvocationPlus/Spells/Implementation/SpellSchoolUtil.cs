using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using UnityEngine;

namespace EvocationPlus.Spells.Implementation
{
    internal static class SpellSchoolUtil
    {
        public static bool ReplaceDescriptor(
            BlueprintAbility spell,
            SpellDescriptor remove,
            SpellDescriptor add)
        {
            if (spell == null)
                return false;

            var old = spell.GetComponent<SpellDescriptorComponent>();
            if (old == null)
                return false;

            var clone = Object.Instantiate(old);

            clone.Descriptor &= ~remove;
            clone.Descriptor |= add;

            ReplaceComponent(spell, old, clone);
            return true;
        }

        private static void ReplaceComponent(
            BlueprintAbility bp,
            BlueprintComponent oldC,
            BlueprintComponent newC)
        {
            var arr = bp.ComponentsArray;
            for (int i = 0; i < arr.Length; i++)
            {
                if (ReferenceEquals(arr[i], oldC))
                {
                    arr[i] = newC;
                    bp.ComponentsArray = arr;
                    return;
                }
            }
        }
    }
}