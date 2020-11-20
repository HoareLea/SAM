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

                if (System.Enum.TryParse(value, out result))
                    return result;

                value = value.Replace(" ", string.Empty).ToUpper();
                foreach (ApertureType apertureType in System.Enum.GetValues(typeof(ApertureType)))
                {
                    string value_Type = null;

                    value_Type = Query.Text(apertureType);
                    if (!string.IsNullOrWhiteSpace(value_Type) && value_Type.ToUpper().Equals(value))
                        return result;

                    value_Type = apertureType.ToString().ToUpper();
                    if (value_Type.Equals(value))
                        return result;
                }

                return Analytical.ApertureType.Undefined;
            }

            if (@object is int)
                return (ApertureType)(int)(@object);

            if (@object is Aperture)
            {
                ApertureConstruction apertureConstruction = ((Aperture)@object).ApertureConstruction;
                if (apertureConstruction != null)
                    return apertureConstruction.ApertureType;
            }

            if (@object is ApertureConstruction)
                return ((ApertureConstruction)@object).ApertureType;

            return Analytical.ApertureType.Undefined;
        }
    }
}