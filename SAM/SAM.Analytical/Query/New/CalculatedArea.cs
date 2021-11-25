namespace SAM.Analytical
{
    public static partial class Query
    {
        public static double CalculatedArea(this Space space, BuildingModel buildingModel = null, double offset = 0.1, double tolerance_Angle = Core.Tolerance.Angle, double tolerance_Distance = Core.Tolerance.Distance, double tolerance_Snap = Core.Tolerance.MacroDistance)
        {
            if (space.TryGetValue(SpaceParameter.Area, out double result) && !double.IsNaN(result))
                return result;

            if (buildingModel == null)
                return double.NaN;

            return buildingModel.GetArea(space, offset, tolerance_Angle, tolerance_Distance, tolerance_Snap);
        }
    }
}