namespace SAM.Architectural
{
    public static partial class Create
    {
        public static Level Level(double elevation, double tolerance = Core.Tolerance.MacroDistance)
        {
            return new Level(string.Format("Level {0}", Core.Query.Round(elevation, tolerance)), elevation);
        }
    }
}