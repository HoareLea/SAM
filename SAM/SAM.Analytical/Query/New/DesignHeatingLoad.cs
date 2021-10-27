using System.Collections.Generic;

namespace SAM.Analytical
{
    public static partial class Query
    {

        public static double DesignHeatingLoad(this ArchitecturalModel architecturalModel, Zone zone)
        {
            List<Space> spaces = architecturalModel?.GetSpaces(zone);
            if (spaces == null || spaces.Count == 0)
                return double.NaN;

            double result = 0;
            foreach(Space space in spaces)
            {
                if (space == null)
                    continue;

                if (!space.TryGetValue(SpaceParameter.DesignHeatingLoad, out double load) || double.IsNaN(load))
                    continue;

                result += load;
            }

            return result;
        }
    }
}