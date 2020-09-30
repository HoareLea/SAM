namespace SAM.Analytical
{
    public static partial class Query
    {
        public static string DefaultApertureConstructionName(this PanelType panelType, ApertureType apertureType)
        {
            ApertureConstruction apertureConstruction = DefaultApertureConstruction(panelType, apertureType);
            if (apertureConstruction == null)
                return null;

            return apertureConstruction.Name;
        }
    }
}