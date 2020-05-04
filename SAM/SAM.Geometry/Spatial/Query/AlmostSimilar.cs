using System.Collections.Generic;

namespace SAM.Geometry.Spatial
{
    public static partial class Query
    {
        public static bool AlmostSimilar(this Vector3D vector3D_1, Vector3D vector3D_2, double tolerance = Core.Tolerance.Distance)
        {
            if (vector3D_1 == vector3D_2)
                return true;

            if (vector3D_1 == null || vector3D_2 == null)
                return false;

            Vector3D vector3D_3 = new Vector3D(vector3D_2);
            vector3D_3.Negate();

            return vector3D_1.AlmostEqual(vector3D_2, tolerance) || vector3D_1.AlmostEqual(vector3D_3, tolerance);
        }

        public static bool AlmostSimilar(this ISegmentable3D segmentable3D_1, ISegmentable3D segmentable3D_2, double tolerance = Core.Tolerance.Distance)
        {
            if (segmentable3D_1 == segmentable3D_2)
                return true;

            if (segmentable3D_1 == null || segmentable3D_2 == null)
                return false;

            List<Point3D> point3Ds = null;

            point3Ds = segmentable3D_1.GetPoints();
            foreach (Point3D point3D in point3Ds)
                if (!segmentable3D_2.On(point3D, tolerance))
                    return false;

            point3Ds = segmentable3D_2.GetPoints();
            foreach (Point3D point3D in point3Ds)
                if (!segmentable3D_1.On(point3D, tolerance))
                    return false;

            return true;
        }
    }
}