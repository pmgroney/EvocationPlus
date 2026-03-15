using System;
using Kingmaker.Blueprints;
using UnityEngine;

namespace EvocationPlus.BlueprintUtils
{
    public static class BlueprintComponentExtensions
    {
        public static T AddComponent<T>(this BlueprintScriptableObject blueprint)
            where T : BlueprintComponent
        {
            if (blueprint == null) return null;

            var component = ScriptableObject.CreateInstance<T>();
            component.name = typeof(T).Name;

            var old = blueprint.ComponentsArray ?? Array.Empty<BlueprintComponent>();
            var newArr = new BlueprintComponent[old.Length + 1];
            Array.Copy(old, newArr, old.Length);
            newArr[old.Length] = component;
            blueprint.ComponentsArray = newArr;

            return component;
        }
    }
}