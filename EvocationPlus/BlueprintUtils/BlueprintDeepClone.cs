using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.ElementsSystem;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Object = UnityEngine.Object;

// GameAction, Condition, ActionList

namespace EvocationPlus.BlueprintUtils
{
    public static class BlueprintDeepClone
    {
        // Entry point: clone ability + isolate all components & action graphs
        public static BlueprintAbility CloneAbilityIsolated(BlueprintAbility src, string newAssetGuid)
        {
            if (src == null) throw new ArgumentNullException(nameof(src));

            // Clone the blueprint object itself (this is still shallow for ComponentsArray)
            var clone = Object.Instantiate(src);
            clone.name = $"{src.name}_Clone";
            clone.AssetGuid = newAssetGuid; // IMPORTANT: must be unique
            clone.ComponentsArray = src.ComponentsArray
                .Select(c => CloneComponentIsolated(c))
                .ToArray();

            return clone;
        }
        public static BlueprintFeatureSelection CloneFeatureSelectionIsolated(
            BlueprintFeatureSelection src,
            string newAssetGuid)
        {
            if (src == null) throw new ArgumentNullException(nameof(src));

            var clone = Object.Instantiate(src);
            clone.name = $"{src.name}_Clone";
            clone.AssetGuid = newAssetGuid;

            // Deep-clone components (same approach as CloneFeatureIsolated)
            clone.ComponentsArray = src.ComponentsArray
                .Select(c => CloneComponentFully(c))
                .ToArray();

            // Preserve selection option arrays (these are NOT components)
            clone.Features = (src.Features ?? Array.Empty<BlueprintFeature>())
                .ToArray();

            clone.AllFeatures = (src.AllFeatures ?? Array.Empty<BlueprintFeature>())
                .ToArray();

            return clone;
        }
        public static BlueprintFeature CloneFeatureIsolated(BlueprintFeature src, string newAssetGuid)
        {
            if (src == null) throw new ArgumentNullException(nameof(src));

            var clone = Object.Instantiate(src);
            clone.name = $"{src.name}_Clone";
            clone.AssetGuid = newAssetGuid;

            clone.ComponentsArray = src.ComponentsArray
                .Select(c => CloneComponentFully(c))
                .ToArray();

            return clone;
        }

        public static T CloneComponentFully<T>(T src) where T : BlueprintComponent
        {
            if (src == null) return null;
            var c = Object.Instantiate(src);
            // call the internal deep field clone
            var method = typeof(BlueprintDeepClone).GetMethod("DeepCloneFieldsInPlace",
                BindingFlags.NonPublic | BindingFlags.Static);
            method.Invoke(null, new object[] { c });
            return c;
        }

        private static BlueprintComponent CloneComponentIsolated(BlueprintComponent srcComponent)
        {
            if (srcComponent == null) return null;

            // Clone the ScriptableObject
            var c = Object.Instantiate(srcComponent);

            // Now deep-clone any nested action/condition graphs inside the component
            // (Many components store ActionList / Conditions in fields.)
            DeepCloneFieldsInPlace(c);

            return c;
        }

        private static void DeepCloneFieldsInPlace(object obj)
        {
            var visited = new Dictionary<object, object>(ReferenceEqualityComparer.Instance);
            DeepCloneObjectGraphInPlace(obj, visited);
        }

        private static void DeepCloneObjectGraphInPlace(object obj, Dictionary<object, object> visited)
        {
            if (obj == null) return;

            var t = obj.GetType();

            // Don’t descend into UnityEngine.Objects or Blueprint references; keep those as shared refs
            if (obj is BlueprintScriptableObject) return;

            if (obj is Object && !(obj is BlueprintComponent))
                return;

            // Avoid cycles
            if (visited.ContainsKey(obj)) return;
            visited[obj] = obj;

            var flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            foreach (var f in t.GetFields(flags))
            {
                if (f.IsNotSerialized) continue;
                var ft = f.FieldType;
                var val = f.GetValue(obj);
                if (val == null) continue;

                // ActionList: clone its Actions array deeply
                if (ft == typeof(ActionList))
                {
                    var al = (ActionList)val;
                    var cloned = CloneActionList(al, visited);
                    f.SetValue(obj, cloned);
                    continue;
                }

                // GameAction / Condition: deep clone the node
                if (typeof(GameAction).IsAssignableFrom(ft))
                {
                    var cloned = (GameAction)CloneManagedNode((GameAction)val, visited);
                    f.SetValue(obj, cloned);
                    continue;
                }

                if (typeof(Condition).IsAssignableFrom(ft))
                {
                    var cloned = (Condition)CloneManagedNode((Condition)val, visited);
                    f.SetValue(obj, cloned);
                    continue;
                }

                // Arrays / Lists: clone elements recursively
                if (ft.IsArray)
                {
                    var arr = (Array)val;
                    var elemType = ft.GetElementType();
                    var newArr = Array.CreateInstance(elemType, arr.Length);

                    for (var i = 0; i < arr.Length; i++)
                    {
                        var e = arr.GetValue(i);
                        newArr.SetValue(CloneValue(e, visited), i);
                    }

                    f.SetValue(obj, newArr);
                    continue;
                }

                if (typeof(IList).IsAssignableFrom(ft))
                {
                    var list = (IList)val;
                    var newList = (IList)Activator.CreateInstance(ft);
                    foreach (var e in list) newList.Add(CloneValue(e, visited));
                    f.SetValue(obj, newList);
                    continue;
                }

                // For other reference types, recurse
                if (!ft.IsValueType && ft != typeof(string))
                {
                    // If it’s an Owlcat node type, clone it; otherwise recurse in place
                    // (Most nested config objects are safe to clone.)
                    var cloned = CloneValue(val, visited);
                    if (!ReferenceEquals(cloned, val))
                        f.SetValue(obj, cloned);
                    else
                        DeepCloneObjectGraphInPlace(val, visited);
                }
            }
        }

        private static object CloneValue(object val, Dictionary<object, object> visited)
        {
            if (val == null) return null;

            // Keep UnityEngine.Object / Blueprint refs shared (they are assets)
            if (val is Object) return val;

            var t = val.GetType();

            if (t.IsValueType || t == typeof(string)) return val;

            // Special cases: Action/Condition nodes
            if (val is GameAction ga) return CloneManagedNode(ga, visited);
            if (val is Condition c) return CloneManagedNode(c, visited);
            if (val is ActionList al) return CloneActionList(al, visited);

            // Collections handled at field level, but handle here too if encountered directly
            if (t.IsArray)
            {
                var arr = (Array)val;
                var elemType = t.GetElementType();
                var newArr = Array.CreateInstance(elemType, arr.Length);
                for (var i = 0; i < arr.Length; i++)
                    newArr.SetValue(CloneValue(arr.GetValue(i), visited), i);
                return newArr;
            }

            // Generic clone for managed config objects
            return CloneManagedObject(val, visited);
        }

        private static ActionList CloneActionList(ActionList src, Dictionary<object, object> visited)
        {
            // ActionList is a struct in some Owlcat versions; treat carefully:
            // In Kingmaker it’s typically a struct with GameAction[] Actions
            var cloned = src;
            if (src.Actions != null)
                cloned.Actions = src.Actions
                    .Select(a => (GameAction)CloneManagedNode(a, visited))
                    .ToArray();
            return cloned;
        }

        private static object CloneManagedNode(object node, Dictionary<object, object> visited)
        {
            if (node == null) return null;
            if (visited.TryGetValue(node, out var existing)) return existing;

            var clone = CloneManagedObject(node, visited);
            visited[node] = clone;

            // Deep-clone children inside the clone
            DeepCloneObjectGraphInPlace(clone, visited);
            return clone;
        }

        private static object CloneManagedObject(object src, Dictionary<object, object> visited)
        {
            if (src == null) return null;
            if (visited.TryGetValue(src, out var existing)) return existing;

            var t = src.GetType();

            // MemberwiseClone is perfect for Owlcat action/condition nodes (shallow copy, then we deep-fix fields)
            var m = t.GetMethod("MemberwiseClone", BindingFlags.Instance | BindingFlags.NonPublic);
            var clone = m.Invoke(src, null);

            visited[src] = clone;
            return clone;
        }

        private sealed class ReferenceEqualityComparer : IEqualityComparer<object>
        {
            public static readonly ReferenceEqualityComparer Instance = new ReferenceEqualityComparer();

            public new bool Equals(object x, object y)
            {
                return ReferenceEquals(x, y);
            }

            public int GetHashCode(object obj)
            {
                return RuntimeHelpers.GetHashCode(obj);
            }
        }
    }
}