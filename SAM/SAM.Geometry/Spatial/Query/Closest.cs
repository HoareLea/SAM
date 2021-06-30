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
                Planar.IClosed2D externalEdge = face3D.ExternalEdge2D;
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

        public static T Closest<T>(this IEnumerable<T> segmentable3Ds, Point3D point3D, out double distance) where T: ISegmentable3D
        {
            distance = double.NaN;
            T result = default;

            if (segmentable3Ds == null || point3D == null)
            {
                return result;
            }

            double distance_min = double.MaxValue;
            foreach(T segmentable3D in segmentable3Ds)
            {
                double distance_Temp = segmentable3D.Distance(point3D);
                if(distance_Temp < distance_min)
                {
                    distance = distance_Temp;
                    distance_min = distance_Temp;
                    result = segmentable3D;
                }
            }

            return result;
        }

        public static T Closest<T>(this IEnumerable<T> segmentable3Ds, Point3D point3D) where T: ISegmentable3D
        {
            return Closest<T>(segmentable3Ds, point3D, out double distance);
        }
    }
}