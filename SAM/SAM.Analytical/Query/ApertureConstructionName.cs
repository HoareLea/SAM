namespace SAM.Analytical
{
    public static partial class Query
    {
        public static string ApertureConstructionName(this ApertureType apertureType, bool external = true)
        {
            ApertureConstruction apertureConstruction = ApertureConstruction(apertureType, external);
            if (apertureConstruction == null)
                return null;

            return apertureConstruction.Name;
        }
    }
}