namespace SAM.Analytical
{
    public static partial class Query
    {
        public static string Description(this Construction construction)
        {
            if (construction == null)
                return null;

            string result = null;
            if (!Core.Query.TryGetValue(construction, ParameterName_Description(), out result))
                return null;

            return result;
        }

        public static string Description(this ApertureConstruction apertureConstruction)
        {
            if (apertureConstruction == null)
                return null;

            string result = null;
            if (!Core.Query.TryGetValue(apertureConstruction, ParameterName_Description(), out result))
                return null;

            return result;
        }
    }
}