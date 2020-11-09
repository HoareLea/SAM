using SAM.Core;
using SAM.Geometry.Planar;
using System.Collections.Generic;

namespace SAM.Geometry.Spatial
{
    public static partial class Query
    {
        public static List<Segment3D> Grid(this IEnumerable<IClosedPlanar3D> closedPlanar3Ds, double x, double y, Plane plane = null, Point3D point3D = null, double tolerance_Angle = Tolerance.Angle, double tolerance_Distance = Tolerance.Distance, bool keepFull = false)
        {
            if (closedPlanar3Ds == null || double.IsNaN(x) || double.IsNaN(y))
                return null;

            if(plane == null)
            {
                plane = Plane.WorldXY;
                double z = double.MaxValue;
                foreach (IClosedPlanar3D closedPlanar3D in closedPlanar3Ds)
                {
                    BoundingBox3D boundingBox3D = closedPlanar3D?.GetBoundingBox();
                    if (boundingBox3D == null)
                        continue;

                    if (z > boundingBox3D.Min.Z)
                        z = boundingBox3D.Min.Z;
                }

                if (z == double.MaxValue)
                    return null;

                plane = plane.GetMoved(new Vector3D(0, 0, z)) as Plane;
            }

            List<IBoundable2D> boundable2Ds = new List<IBoundable2D>();
            foreach(IClosedPlanar3D closedPlanar3D in closedPlanar3Ds)
            {
                PlanarIntersectionResult planarIntersectionResult = PlanarIntersectionResult.Create(plane, closedPlanar3D, tolerance_Angle, tolerance_Distance);
                if (planarIntersectionResult == null || !planarIntersectionResult.Intersecting)
                    continue;

                foreach(ISAMGeometry3D sAMGeometry3D in planarIntersectionResult.Geometry3Ds)
                {
                    IBoundable3D boundable3D = sAMGeometry3D as IBoundable3D;
                    if (boundable3D == null)
                        continue;

                    IBoundable2D boundable2D = plane.Convert(boundable3D) as IBoundable2D;
                    if (boundable2D == null)
                        continue;

                    boundable2Ds.Add(boundable2D);
                }
            }

            List<Segment3D> result = new List<Segment3D>();
            if (boundable2Ds == null || boundable2Ds.Count == 0)
                return result;

            Point2D point2D = null;
            if (point3D != null)
                point2D = plane.Convert(plane.Project(point3D));

            if (point2D == null)
                point2D = plane.Convert(plane.Origin);

            List<Segment2D> segment2Ds = Planar.Query.Grid(point2D, boundable2Ds, x, y, keepFull);
            if (segment2Ds == null || segment2Ds.Count == 0)
                return result;

            return segment2Ds.ConvertAll(segment2D => plane.Convert(segment2D));
        }
    }
}