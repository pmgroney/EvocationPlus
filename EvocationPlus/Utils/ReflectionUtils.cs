using System;
using System.Reflection;

namespace EvocationPlus.Utils
{
    internal static class ReflectionUtils
    {
        public static void SetPrivateField(object obj, string fieldName, object value)
        {
            if (obj == null) throw new ArgumentNullException(nameof(obj));

            var t = obj.GetType();
            FieldInfo field = null;

            while (t != null)
            {
                field = t.GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
                if (field != null) break;
                t = t.BaseType;
            }

            if (field == null)
                throw new MissingFieldException(obj.GetType().FullName, fieldName);

            field.SetValue(obj, value);
        }

        public static void SetField(object obj, string fieldName, object value)
        {
            var field = obj.GetType().GetField(fieldName,
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

            if (field == null)
                throw new MissingFieldException(obj.GetType().FullName, fieldName);

            field.SetValue(obj, value);
        }

        public static void SetFieldAny(object obj, string[] fieldNames, object value)
        {
            if (obj == null) throw new ArgumentNullException(nameof(obj));

            foreach (var name in fieldNames)
            {
                try
                {
                    SetPrivateField(obj, name, value);
                    return;
                }
                catch
                {
                    /* try next */
                }

                // also try public field if present
                var t = obj.GetType();
                while (t != null)
                {
                    var f = t.GetField(name, BindingFlags.Instance | BindingFlags.Public);
                    if (f != null)
                    {
                        f.SetValue(obj, value);
                        return;
                    }

                    t = t.BaseType;
                }
            }

            throw new MissingFieldException(obj.GetType().FullName, string.Join("/", fieldNames));
        }
    }
}