namespace SAM.Analytical
{
    public static partial class Query
    {
        public static double CalculatedVolume(this Space space, ArchitecturalModel architecturalModel = null, double silverSpacing = Core.Tolerance.MacroDistance, double tolerance = Core.Tolerance.Distance)
        {
            if (space.TryGetValue(SpaceParameter.Volume, out double result) && !double.IsNaN(result))
                return result;

            if (architecturalModel == null)
                return double.NaN;

            return architecturalModel.GetVolume(space, silverSpacing, tolerance);

        }
    }
}