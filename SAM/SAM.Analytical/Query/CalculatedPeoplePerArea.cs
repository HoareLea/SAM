namespace SAM.Analytical
{
    public static partial class Query
    {
        public static double CalculatedPeoplePerArea(this Space space)
        {
            if (space == null)
                return double.NaN;

            double occupancy = CalculatedOccupancy(space);
            if (double.IsNaN(occupancy))
                return double.NaN;

            double area = double.NaN;
            if (!space.TryGetValue(SpaceParameter.Area, out area) || double.IsNaN(area))
                return double.NaN;

            if (area == 0)
                return 0;

            return occupancy / area;
        }
    }
}