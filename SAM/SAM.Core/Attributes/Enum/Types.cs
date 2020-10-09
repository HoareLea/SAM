using System;

namespace SAM.Core.Attributes
{
    public class Types : Attribute
    {
        private Type[] values;

        public Types(params Type[] values)
        {
            this.values = values;
        }

        public static Type[] Get(Enum @enum)
        {
            if (@enum == null)
                return null;

            Types types_Temp = GetCustomAttribute(@enum.GetType(), typeof(Types)) as Types;
            return types_Temp?.values;
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
