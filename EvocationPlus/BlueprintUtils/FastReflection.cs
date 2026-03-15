using System;
using System.Reflection;
using System.Reflection.Emit;

namespace EvocationPlus.BlueprintUtils
{
    // takes (object instance, object value)
    public delegate void FastSetter(object __instance, object value);

    public static class Helpers
    {
        public static FastSetter CreateFieldSetter<T>(string fieldName)
        {
            if (string.IsNullOrWhiteSpace(fieldName))
                throw new ArgumentNullException(nameof(fieldName));

            var flags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
            var field = typeof(T).GetField(fieldName, flags);

            if (field == null)
                throw new MissingFieldException(typeof(T).FullName, fieldName);

            // DynamicMethod hosted against T so it can access non-public members in full .NET Framework
            var dm = new DynamicMethod(
                $"{typeof(T).Name}_set_{fieldName}",
                typeof(void),
                new[] { typeof(object), typeof(object) },
                typeof(T),
                true);

            var il = dm.GetILGenerator();

            // Load instance for instance field
            if (!field.IsStatic)
            {
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Castclass, typeof(T));
            }

            // Load value and cast/unbox to the field type
            il.Emit(OpCodes.Ldarg_1);
            EmitCastOrUnbox(il, field.FieldType);

            // Store
            if (field.IsStatic)
                il.Emit(OpCodes.Stsfld, field);
            else
                il.Emit(OpCodes.Stfld, field);

            il.Emit(OpCodes.Ret);

            return (FastSetter)dm.CreateDelegate(typeof(FastSetter));
        }

        private static void EmitCastOrUnbox(ILGenerator il, Type targetType)
        {
            if (targetType.IsValueType)
                il.Emit(OpCodes.Unbox_Any, targetType);
            else
                il.Emit(OpCodes.Castclass, targetType);
        }
    }
}