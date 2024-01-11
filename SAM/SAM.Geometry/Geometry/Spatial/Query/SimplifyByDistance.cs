using System.Collections.Generic;
using System.Linq;

namespace SAM.Geometry.Spatial
{
    public static partial class Query
    {
        public static List<Point3D> SimplifyByDistance(this IEnumerable<Point3D> point3Ds, bool close = false, double tolerane = Core.Tolerance.Distance)
        {
            if (point3Ds == null)
                return null;

            List<Point3D> result = new List<Point3D>(point3Ds);
            Point3D last = result.Last();

            int start = 0;
            int end = close ? result.Count : result.Count - 1;
            while (start < end)
            {
                Point3D first = result[start];
                Point3D second = result[(start + 1) % result.Count];

                if (first.Distance(second) <= tolerane)
                {
                    result.RemoveAt((start + 1) % result.Count);
                    end--;
                }
                else
                    start++;
            }

            if (!close)
            {
                result.Remove(result.Last());
                result.Add(last);
            }
            //else
            //{
            //    result.Add(result.First());
            //}

            while (result.Last().Distance(result[result.Count() - 2]) < tolerane)
            {
                result.RemoveAt(result.Count - 2);
            }

            return result;
        }
    }
}