using System;
using System.Linq;
using Kingmaker.Blueprints;
using UnityEngine;

namespace EvocationPlus.Utils
{
    public class ComponentSwap
    {
        public static void RemoveComponent<T>(BlueprintScriptableObject bp) where T : BlueprintComponent
        {
            var old = bp.ComponentsArray ?? Array.Empty<BlueprintComponent>();
            bp.ComponentsArray = old.Where(c => !(c is T)).ToArray();
        }

        public static T AddComponent<T>(BlueprintScriptableObject bp) where T : BlueprintComponent
        {
            var c = ScriptableObject.CreateInstance<T>();
            c.name = typeof(T).Name;

            var old = bp.ComponentsArray ?? Array.Empty<BlueprintComponent>();
            var arr = new BlueprintComponent[old.Length + 1];
            Array.Copy(old, arr, old.Length);
            arr[old.Length] = c;
            bp.ComponentsArray = arr;

            return c;
        }
    }
}