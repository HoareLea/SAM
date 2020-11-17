namespace SAM.Analytical
{
    public static partial class Query
    {
        public static string Text(this PanelType panelType)
        {
            return Core.Query.Description(panelType);
        }

        public static string Text(this ApertureType apertureType)
        {
            return Core.Query.Description(apertureType);
        }

        public static string Text(this ProfileType profileType)
        {
            return Core.Query.Description(profileType);
        }
    }
}