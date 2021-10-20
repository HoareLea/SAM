using System.Collections.Generic;

namespace SAM.Analytical
{
    public static partial class Query
    {
        public static double Sum(this ArchitecturalModel architecturalModel, Zone zone, string name)
        {
            if (architecturalModel == null || zone == null || string.IsNullOrWhiteSpace(name))
                return double.NaN;

            List<Space> spaces = architecturalModel.GetSpaces(zone);
            if (spaces == null)
                return double.NaN;

            double result = 0;
            bool contains = false;
            foreach (Space space in spaces)
            {
                if (!space.TryGetValue(name, out double value) || double.IsNaN(value))
                    continue;

                result += value;
                contains = true;
            }

            if (!contains)
                return double.NaN;

            return result;
        }

        public static double Sum(this ArchitecturalModel architecturalModel, Zone zone, SpaceParameter spaceParameter)
        {
            if (architecturalModel == null || zone == null)
                return double.NaN;

            List<Space> spaces = architecturalModel.GetSpaces(zone);
            if (spaces == null)
                return double.NaN;

            double result = 0;
            bool contains = false;
            foreach (Space space in spaces)
            {
                if (!space.TryGetValue(spaceParameter, out double value) || double.IsNaN(value))
                    continue;

                result += value;
                contains = true;
            }

            if (!contains)
                return double.NaN;

            return result;
        }
    }
}