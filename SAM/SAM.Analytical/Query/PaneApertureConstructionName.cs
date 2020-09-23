namespace SAM.Analytical
{
    public static partial class Query
    {
        public static string PaneApertureConstructionName(this ApertureConstruction apertureConstruction)
        {
            return PaneApertureConstructionName(apertureConstruction?.Name);
        }

        public static string PaneApertureConstructionName(string name)
        {
            if (string.IsNullOrEmpty(name))
                return name;

            return string.Format("{0} -pane", name);
        }
    }
}