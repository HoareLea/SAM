using System;


namespace SAM.Core
{
    public static partial class Query
    {
        public static ConvertType ConvertType(this object @object)
        {
            if (@object is ConvertType)
                return (ConvertType)@object;

            ConvertType result;
            if (@object is string)
            {
                string value = (string)@object;

                if (Enum.TryParse(value, out result))
                    return result;

                value = value.Replace(" ", string.Empty).ToUpper();
                foreach (ConvertType panelType in Enum.GetValues(typeof(ConvertType)))
                {
                    string value_Type = panelType.ToString().ToUpper();
                    if (value_Type.Equals(value))
                        return result;
                }

                return Core.ConvertType.Undefined;
            }

            if (@object is int)
                return(ConvertType)(int)(@object);

            return Core.ConvertType.Undefined;
        }
    }
}
