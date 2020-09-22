namespace SAM.Analytical
{
    public static partial class Query
    {
        public static string ConstructionName(this PanelType panelType)
        {
            Construction construction = DefaultConstruction(panelType);
            if (construction == null)
                return null;

            return construction.Name;
        }
    }
}