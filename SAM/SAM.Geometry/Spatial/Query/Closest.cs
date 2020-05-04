using System.Collections.Generic;
using System.Linq;

namespace SAM.Geometry.Spatial
{
    public static partial class Query
    {
        public static Face3D Closest(this IEnumerable<Face3D> face3Ds, Point3D point3D)
        {
            if (face3Ds == null || face3Ds.Count() == 0 || point3D == null)
                return null;

            double min = double.MaxValue;
            Face3D result = null;
            foreach (Face3D face3D in face3Ds)
            {
                Plane plane = face3D.GetPlane();

                Point3D point3D_Projected = plane.Project(point3D);
                double distance = point3D_Projected.Distance(point3D);
                if (min < distance)
                    continue;

                Planar.Point2D point2D_Converted = plane.Convert(point3D_Projected);
                Planar.IClosed2D externalEdge = face3D.ExternalEdge;
                if (!externalEdge.Inside(point2D_Converted))
                {
                    if (externalEdge is Planar.ISegmentable2D)
                    {
                        Planar.Point2D point2D_Closest = Planar.Query.Closest((Planar.ISegmentable2D)externalEdge, point2D_Converted);
                        distance += point2D_Closest.Distance(point2D_Converted);
                    }
                }

                if (min > distance)
                {
                    min = distance;
                    result = face3D;
                }
            }

            return result;
        }

        public static Point3D Closest(this IClosedPlanar3D closedPlanar3D, Point3D point3D)
        {
            if (closedPlanar3D == null || point3D == null)
                return null;

            Plane plane = closedPlanar3D.GetPlane();

            Planar.Point2D point2D_Converted = plane.Convert(plane.Project(point3D));

            Planar.IClosed2D externalEdge = null;
            if (closedPlanar3D is Face3D)
                externalEdge = plane.Convert(((Face3D)closedPlanar3D).GetExternalEdge());
            else
                externalEdge = plane.Convert(closedPlanar3D);

            if (externalEdge.Inside(point2D_Converted))
                return plane.Convert(point2D_Converted);

            if (externalEdge is Planar.ISegmentable2D)
                return plane.Convert(Planar.Query.Closest((Planar.ISegmentable2D)externalEdge, point2D_Converted));

            return null;
        }
    }
}