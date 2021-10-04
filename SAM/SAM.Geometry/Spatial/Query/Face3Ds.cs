using System.Collections.Generic;

namespace SAM.Geometry.Spatial
{
    public static partial class Query
    {
        public static List<Face3D> Face3Ds(this IEnumerable<ISAMGeometry3D> geometry3Ds, double tolerance = Core.Tolerance.Distance)
        {
            if (geometry3Ds == null)
                return null;

            List<Face3D> result = new List<Face3D>();
            foreach (ISAMGeometry3D geometry3D in geometry3Ds)
            {
                if (geometry3D is Segment3D)
                    continue;

                if(geometry3D is Shell)
                {
                    List<Face3D> face3Ds = ((Shell)geometry3D).Face3Ds;
                    if(face3Ds != null && face3Ds.Count > 0)
                    {
                        result.AddRange(face3Ds);
                        continue;
                    }
                }

                if(geometry3D is Mesh3D)
                {
                    List<Triangle3D> triangle3Ds = ((Mesh3D)geometry3D).GetTriangles();
                    if(triangle3Ds != null && triangle3Ds.Count != 0)
                    {
                        foreach(Triangle3D triangle3D in triangle3Ds)
                        {
                            if(triangle3D == null || !triangle3D.IsValid() || triangle3D.GetArea() < tolerance)
                            {
                                continue;
                            }

                            result.Add(new Face3D(triangle3D));
                        }

                        continue;
                    }
                }

                if (geometry3D is Face3D)
                {
                    result.Add((Face3D)geometry3D);
                    continue;
                }

                if (geometry3D is IClosedPlanar3D)
                {
                    IClosedPlanar3D closedPlanar3D = (IClosedPlanar3D)geometry3D;
                    Plane plane = closedPlanar3D.GetPlane();
                    if (plane == null)
                        continue;

                    result.Add(new Face3D(closedPlanar3D));
                    continue;
                }

                if (geometry3D is ICurvable3D)
                {
                    List<Point3D> point3Ds = ((ICurvable3D)geometry3D).GetCurves().ConvertAll(x => x.GetStart());
                    result.Add(new Face3D(new Polygon3D(point3Ds, tolerance)));
                }
            }

            return result;
        }

        public static List<Face3D> Face3Ds(this Extrusion extrusion, double tolerance = Core.Tolerance.Distance)
        {
            return Create.Shell(extrusion, tolerance)?.Face3Ds;
        }

        public static List<Face3D> Face3Ds(this PlanarIntersectionResult planarIntersectionResult, double snapTolerance = Core.Tolerance.MacroDistance, double tolerance = Core.Tolerance.Distance)
        {
            if(planarIntersectionResult == null || !planarIntersectionResult.Intersecting)
            {
                return null;
            }

            Plane plane = planarIntersectionResult.Plane;
            if(plane == null)
            {
                return null;
            }

            List<Face3D> result = new List<Face3D>();

            List<Planar.ISegmentable2D> segmentable2Ds = planarIntersectionResult.GetGeometry2Ds<Planar.ISegmentable2D>();
            if(segmentable2Ds == null || segmentable2Ds.Count < 3)
            {
                return result;
            }

            List<Planar.Segment2D> segment2Ds = Planar.Query.Split(segmentable2Ds, tolerance);
            segment2Ds = Planar.Query.Snap(segment2Ds, true, snapTolerance);

            List<Planar.Face2D> face2Ds = Planar.Create.Face2Ds(segmentable2Ds, tolerance);
            if(face2Ds == null || face2Ds.Count == 0)
            {
                return result;
            }

            result.AddRange(face2Ds.ConvertAll(x => plane.Convert(x)));
            return result;
        }
    }
}