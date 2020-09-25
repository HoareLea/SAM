namespace SAM.Analytical
{
    public static partial class Query
    {
        public static string PaneApertureConstructionUniqueName(this ApertureConstruction apertureConstruction)
        {
            return PaneApertureConstructionUniqueName(apertureConstruction?.UniqueName());
        }

        public static string PaneApertureConstructionUniqueName(string name)
        {
            if (string.IsNullOrEmpty(name))
                return name;

            return string.Format("{0} -pane", name);
        }
    }
}