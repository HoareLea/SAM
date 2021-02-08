namespace SAM.Analytical
{
    public static partial class Query
    {
        public static double SpecificOccupancySensibleGain(this Space space)
        {
            if (space == null)
                return double.NaN;

            double occupancySensibleGain = OccupancySensibleGain(space);
            if (double.IsNaN(occupancySensibleGain))
                return double.NaN;

            double area = double.NaN;
            if (!space.TryGetValue(SpaceParameter.Area, out area) || double.IsNaN(area))
                return double.NaN;

            if (area == 0)
                return 0;

            return occupancySensibleGain / area;
        }
    }
}