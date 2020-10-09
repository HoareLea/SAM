using System;

namespace SAM.Core.Attributes
{
    public class ParameterTypes : Attribute
    {
        private Type[] values;

        public ParameterTypes(params Type[] values)
        {
            this.values = values;
        }

        public static Type[] Get(Enum @enum)
        {
            if (@enum == null)
                return null;

            ParameterTypes types_Temp = GetCustomAttribute(@enum.GetType(), typeof(ParameterTypes)) as ParameterTypes;
            return types_Temp?.values;
        }

        public static ParameterTypes Get(Type type)
        {
            if (type == null)
                return null;

            if (!type.IsEnum)
                return null;

            object[] objects = type.GetCustomAttributes(typeof(ParameterTypes), true);
            if (objects == null || objects.Length == 0)
                return null;

            foreach (object @object in objects)
            {
                ParameterTypes parameterTypes = @object as ParameterTypes;
                if (parameterTypes != null)
                    return parameterTypes;
            }

            return null;
        }

        public virtual bool IsValid(Type type)
        {
            if (values == null || values.Length == 0)
                return false;

            foreach (Type type_Temp in values)
            {
                if (type_Temp == null)
                    continue;

                if (type_Temp.IsAssignableFrom(type))
                    return true;
            }

            return false;
        }
    }
}
