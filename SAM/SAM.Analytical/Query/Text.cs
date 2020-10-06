namespace SAM.Analytical
{
    public static partial class Query
    {
        public static string Text(this PanelType panelType)
        {
            return Core.Query.Description(panelType);
        }
    }
}