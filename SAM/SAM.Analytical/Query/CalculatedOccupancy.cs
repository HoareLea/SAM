namespace SAM.Analytical
{
    public static partial class Query
    {
        public static double CalculatedOccupancy(this Space space)
        {
            if (space == null)
                return double.NaN;

            double occupancy = double.NaN;
            if (space.TryGetValue(SpaceParameter.Occupancy, out occupancy) && !double.IsNaN(occupancy))
                return occupancy;

            double area = double.NaN;
            if (!space.TryGetValue(SpaceParameter.Area, out area) || double.IsNaN(area))
                return double.NaN;

            InternalCondition internalCondition = space.InternalCondition;
            if (internalCondition == null)
                return double.NaN;

            double areaPerPerson;
            if (!internalCondition.TryGetValue(InternalConditionParameter.AreaPerPerson, out areaPerPerson) || double.IsNaN(areaPerPerson))
                return double.NaN;

            if (areaPerPerson == 0)
                return 0;

            return area / areaPerPerson;
        }
    }
}