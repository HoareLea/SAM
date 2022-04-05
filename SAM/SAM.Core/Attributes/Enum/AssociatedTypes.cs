using System;
using System.Collections;
using System.Linq;

namespace SAM.Core.Attributes
{
    [AttributeUsage(AttributeTargets.Enum, AllowMultiple = false)]
    public class AssociatedTypes : Attribute, IEnumerable
    {
        private Type[] types;

        public AssociatedTypes(params Type[] values)
        {
            this.types = values;
        }

        public virtual bool IsValid(Type type)
        {
            if (types == null || types.Length == 0)
                return false;

            foreach (Type type_Temp in types)
            {
                if (type_Temp == null)
                    continue;

                if (type.Equals(type_Temp))
                    return true;

                if (type.IsAssignableFrom(type_Temp))
                    return true;
            }

            return false;
        }

        public Type[] Types
        {
            get
            {
                return types?.ToArray();
            }
        }

        public static Type[] Get(Enum @enum)
        {
            if (@enum == null)
                return null;

            AssociatedTypes types_Temp = GetCustomAttribute(@enum.GetType(), typeof(AssociatedTypes)) as AssociatedTypes;
            return types_Temp?.types;
        }

        public static AssociatedTypes Get(Type type)
        {
            if (type == null)
                return null;

            if (!type.IsEnum)
                return null;

            object[] objects = type.GetCustomAttributes(typeof(AssociatedTypes), true);
            if (objects == null || objects.Length == 0)
                return null;

            foreach (object @object in objects)
            {
                AssociatedTypes parameterTypes = @object as AssociatedTypes;
                if (parameterTypes != null)
                    return parameterTypes;
            }

            return null;
        }

        public IEnumerator GetEnumerator()
        {
            return types?.GetEnumerator();
        }
    }
}
