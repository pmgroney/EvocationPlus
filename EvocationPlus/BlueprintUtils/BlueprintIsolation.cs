using System;
using System.Linq;
using System.Reflection;
using Kingmaker.Blueprints;
using Object = UnityEngine.Object;

namespace EvocationPlus.BlueprintUtils
{
    public static class BlueprintIsolation
    {
        // Finds the private field that actually stores BlueprintComponent[] and replaces an element in it.
        private static FieldInfo FindComponentsField(Type t)
        {
            const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
            while (t != null)
            {
                var f = t.GetFields(flags).FirstOrDefault(fi => fi.FieldType == typeof(BlueprintComponent[]));
                if (f != null) return f;
                t = t.BaseType;
            }

            return null;
        }

        public static T EnsureUniqueComponent<T>(BlueprintScriptableObject bp) where T : BlueprintComponent
        {
            if (bp == null) return null;

            // Get current component instance
            var existing = bp.GetComponent<T>();
            if (existing == null) return null;

            // Clone the component instance
            var cloned = Object.Instantiate(existing);

            // Replace in the blueprint's component array
            var field = FindComponentsField(bp.GetType());
            if (field == null)
                throw new Exception("Could not locate BlueprintComponent[] backing field on " + bp.GetType().FullName);

            var arr = (BlueprintComponent[])field.GetValue(bp);
            if (arr == null)
                throw new Exception("Component array field was null on " + bp.name);

            for (var i = 0; i < arr.Length; i++)
                if (ReferenceEquals(arr[i], existing))
                {
                    arr[i] = cloned;
                    field.SetValue(bp, arr);
                    return cloned;
                }

            // If not found, something is off (component came from elsewhere)
            throw new Exception("Component instance not found in backing array for " + typeof(T).Name);
        }
    }
}