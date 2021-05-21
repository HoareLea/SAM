using System.Collections.Generic;

namespace SAM.Geometry.Spatial
{
    public static partial class Query
    {
        public static List<Point3D> SimplifyByAngle(IEnumerable<Point3D> point3Ds, bool close = false, double tolerane = Core.Tolerance.Angle)
        {
            if (point3Ds == null)
                return null;

            List<Point3D> result = new List<Point3D>(point3Ds);

            int start = 0;
            int end = close ? result.Count : result.Count - 2;
            while (start < end)
            {
                Point3D first = result[start];
                Point3D second = result[(start + 1) % result.Count];
                Point3D third = result[(start + 2) % result.Count];

                if (second.SmallestAngle(first, third) <= tolerane)
                {
                    result.RemoveAt((start + 1) % result.Count);
                    end--;
                }
                else
                {
                    start++;
                }
            }
            return result;
        }
    }
}