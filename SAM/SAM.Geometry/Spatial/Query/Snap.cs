using System;
using System.Collections.Generic;

namespace SAM.Geometry.Spatial
{
    public static partial class Query
    {
        public static List<Face3D> Snap(this Face3D face3D_1, Face3D face3D_2, double snapDistance, double tolerance = Core.Tolerance.Distance)
        {
            Plane plane_1 = face3D_1?.GetPlane();
            if (plane_1 == null)
                return null;

            Plane plane_2 = face3D_2?.GetPlane();
            if (plane_2 == null)
                return null;

            if (plane_1.Coplanar(plane_2, tolerance))
            {
                Planar.Face2D face2D_1 = plane_1.Convert(face3D_1);
                Planar.Face2D face2D_2 = plane_1.Convert(plane_1.Project(face3D_2));

                List<Planar.Face2D> face2Ds = Planar.Query.Snap(face2D_1, face2D_2, snapDistance, tolerance);
                if (face2Ds == null)
                    return null;

                return face2Ds?.ConvertAll(x => plane_1.Convert(x));
            }

            PlanarIntersectionResult planarIntersectionResult = plane_1.Intersection(plane_2);
            if (planarIntersectionResult == null || !planarIntersectionResult.Intersecting)
                return null;

            List<Segment3D> segment3Ds = planarIntersectionResult.GetGeometry3Ds<Segment3D>();
            if (segment3Ds == null || segment3Ds.Count == 0)
                return null;

            throw new NotImplementedException();
        }

        public static Polygon3D Snap(this Polygon3D polygon3D, IEnumerable<Point3D> point3Ds, double tolerance = Core.Tolerance.Distance)
        {
            if (point3Ds == null)
                return null;

            List<Point3D> point3Ds_Result = polygon3D?.GetPoints();
            if (point3Ds == null || point3Ds_Result.Count == 0)
                return null;

            for (int j = 0; j < point3Ds_Result.Count; j++)
            {
                double distance = double.MaxValue;
                foreach (Point3D point3D_Temp in point3Ds)
                {
                    if (point3D_Temp == null)
                        continue;

                    double distance_Temp = point3D_Temp.Distance(point3Ds_Result[j]);
                    if (distance_Temp > 0 && distance_Temp <= tolerance && distance > distance_Temp)
                    {
                        point3Ds_Result[j] = point3D_Temp;
                        distance = distance_Temp;
                    }
                }
            }

            return new Polygon3D(point3Ds_Result);
        }
    }
}