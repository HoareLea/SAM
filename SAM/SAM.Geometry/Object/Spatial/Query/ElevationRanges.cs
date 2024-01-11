using SAM.Core;
using System.Collections.Generic;

namespace SAM.Geometry.Object.Spatial
{
    public static partial class Query
    {
        public static List<Range<double>> ElevationRanges<T>(this IEnumerable<T> face3DObjects, double tolerance = Tolerance.Distance) where T : IFace3DObject
        {
            Dictionary<double, List<T>> dictionary = ElevationDictionary(face3DObjects, out double maxElevation, tolerance);
            if (dictionary == null)
            {
                return null;
            }

            List<double> elevations = new List<double>(dictionary.Keys);
            elevations.Add(maxElevation);
            elevations.Sort();

            List<Range<double>> result = new List<Range<double>>();
            for (int i = 1; i < elevations.Count; i++)
            {
                Range<double> range = new Range<double>(elevations[i - 1], elevations[i]);
                result.Add(range);
            }

            return result;
        }
    }
}