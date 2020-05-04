using System;

namespace SAM.Analytical
{
    public static partial class Query
    {
        public static ApertureType ApertureType(this object @object)
        {
            if (@object is ApertureType)
                return (ApertureType)@object;

            ApertureType result;
            if (@object is string)
            {
                string value = (string)@object;

                if (Enum.TryParse(value, out result))
                    return result;

                value = value.Replace(" ", string.Empty).ToUpper();
                foreach (ApertureType apertureType in Enum.GetValues(typeof(ApertureType)))
                {
                    string value_Type = apertureType.ToString().ToUpper();
                    if (value_Type.Equals(value))
                        return result;
                }

                return Analytical.ApertureType.Undefined;
            }

            if (@object is int)
                return (ApertureType)(int)(@object);

            return Analytical.ApertureType.Undefined;
        }
    }
}