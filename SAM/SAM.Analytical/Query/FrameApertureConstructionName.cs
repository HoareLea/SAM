namespace SAM.Analytical
{
    public static partial class Query
    {
        public static string FrameApertureConstructionName(this ApertureConstruction apertureConstruction)
        {
            return FrameApertureConstructionName(apertureConstruction?.Name);
        }

        public static string FrameApertureConstructionName(string name)
        {
            if (string.IsNullOrEmpty(name))
                return name;

            return string.Format("{0} -frame", name);
        }
    }
}