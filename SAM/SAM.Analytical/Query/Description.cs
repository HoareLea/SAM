using System;

namespace SAM.Analytical
{
    public static partial class Query
    {
        [Obsolete]
        public static string Description(this Construction construction)
        {
            if (construction == null)
                return null;

            //TODO: Description Query to be removed (use Parameter Instead)
            return construction.GetValue<string>(ConstructionParameter.Description);

            string result = null;
            if (!Core.Query.TryGetValue(construction, ParameterName_Description(), out result))
                return null;

            return result;
        }

        [Obsolete]
        public static string Description(this ApertureConstruction apertureConstruction)
        {
            if (apertureConstruction == null)
                return null;

            //TODO: Description Query to be removed (use Parameter Instead)
            return apertureConstruction.GetValue<string>(ApertureConstructionParameter.Description);

            string result = null;
            if (!Core.Query.TryGetValue(apertureConstruction, ParameterName_Description(), out result))
                return null;

            return result;
        }
    }
}