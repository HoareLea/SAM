using System.Collections.Generic;
using System.Linq;

namespace SAM.Geometry.Spatial
{
    public static partial class Query
    {
        public static Point3D ClosestPoint3D(this IClosedPlanar3D closedPlanar3D, Point3D point3D)
        {
            if (closedPlanar3D == null || point3D == null)
                return null;

            Plane plane = closedPlanar3D.GetPlane();

            Planar.Point2D point2D_Converted = plane.Convert(plane.Project(point3D));

            Planar.IClosed2D externalEdge = null;
            if (closedPlanar3D is Face3D)
                externalEdge = plane.Convert(((Face3D)closedPlanar3D).GetExternalEdge3D());
            else
                externalEdge = plane.Convert(closedPlanar3D);

            if (externalEdge.Inside(point2D_Converted))
                return plane.Convert(point2D_Converted);

            if (externalEdge is Planar.ISegmentable2D)
                return plane.Convert(Planar.Query.Closest((Planar.ISegmentable2D)externalEdge, point2D_Converted));

            return null;
        }

        public static Point3D ClosestPoint3D(this IEnumerable<Point3D> point3Ds, Point3D point3D)
        {
            if (point3Ds == null || point3Ds.Count() == 0 || point3D == null)
                return null;

            Point3D result = null;
            double distance = double.MaxValue;
            foreach (Point3D point3D_Temp in point3Ds)
            {
                double distance_Temp = point3D_Temp.Distance(point3D);

                if (distance > distance_Temp)
                {
                    distance = distance_Temp;
                    result = point3D_Temp;
                }

                if (distance == 0)
                    return result;
            }

            return result;
        }
    }
}