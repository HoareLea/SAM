using SAM.Geometry.Planar;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Geometry.Spatial
{
    public static partial class Create
    {
        public static PlanarIntersectionResult PlanarIntersectionResult(Plane plane, Point3D point3D)
        {
            if (plane == null || point3D == null)
                return null;

            return new PlanarIntersectionResult(plane, point3D);
        }

        public static PlanarIntersectionResult PlanarIntersectionResult(Face3D face3D, Point3D point3D, Vector3D vector3D, double tolerance = Core.Tolerance.Distance)
        {
            Plane plane = face3D?.GetPlane();
            if (plane == null)
                return null;

            PlanarIntersectionResult planarIntersectionResult = PlanarIntersectionResult(plane, point3D, vector3D, tolerance);
            if (planarIntersectionResult == null)
                return null;

            if (!planarIntersectionResult.Intersecting)
                return planarIntersectionResult;

            Point3D point3D_Plane = planarIntersectionResult.GetGeometry3D<Point3D>();

            if (point3D_Plane != null && face3D.InRange(point3D_Plane, tolerance))
                return new PlanarIntersectionResult(plane, point3D_Plane);

            return new PlanarIntersectionResult(plane);
        }

        public static PlanarIntersectionResult PlanarIntersectionResult(Plane plane, Segment3D segment3D, double tolerance = Core.Tolerance.Distance)
        {
            if (plane == null || segment3D == null)
                return null;

            Vector3D direction = segment3D.Direction;

            Vector3D normal = plane.Normal;

            double d = normal.DotProduct(direction);
            if (System.Math.Abs(d) < tolerance)
            {
                if (System.Math.Min(plane.Distance(segment3D[0]), plane.Distance(segment3D[1])) <= tolerance)
                    return new PlanarIntersectionResult(plane, plane.Project(segment3D));
                else
                    return new PlanarIntersectionResult(plane);
            }

            double u = (plane.K - normal.DotProduct(segment3D[0].ToVector3D())) / d;
            Point3D point3D = segment3D[0];

            Point3D point3D_Intersection = new Point3D(point3D.X + u * direction.X, point3D.Y + u * direction.Y, point3D.Z + u * direction.Z);
            if (!segment3D.On(point3D_Intersection, tolerance))
                return new PlanarIntersectionResult(plane);

            return new PlanarIntersectionResult(plane, point3D_Intersection);
        }

        public static PlanarIntersectionResult PlanarIntersectionResult(Face3D face3D, Segment3D segment3D, double tolerance = Core.Tolerance.Distance)
        {
            if (face3D == null || segment3D == null)
                return null;

            Plane plane = face3D.GetPlane();
            if (plane == null)
                return null;

            if (!face3D.GetBoundingBox(tolerance).InRange(segment3D.GetBoundingBox(tolerance)))
                return new PlanarIntersectionResult(plane);

            PlanarIntersectionResult planarIntersectionResult = PlanarIntersectionResult(plane, segment3D, tolerance);
            if (planarIntersectionResult == null)
                return null;

            if (!planarIntersectionResult.Intersecting)
                return planarIntersectionResult;

            List<ISAMGeometry3D> geometry3Ds = new List<ISAMGeometry3D>();

            List<Point3D> point3Ds = planarIntersectionResult.GetGeometry3Ds<Point3D>();
            if (point3Ds != null && point3Ds.Count > 0)
            {
                point3Ds = point3Ds.FindAll(x => face3D.Inside(x, tolerance) || face3D.OnEdge(x, tolerance));
                geometry3Ds.AddRange(point3Ds);
            }

            List<Segment2D> segment2Ds = planarIntersectionResult.GetGeometry2Ds<Segment2D>();
            if (segment2Ds != null && segment2Ds.Count > 0)
            {
                Face2D face2D = plane.Convert(face3D);
                foreach (Segment2D segment2D in segment2Ds)
                {
                    List<ISAMGeometry2D> geometry2Ds_Intersection = Planar.Query.Intersection<ISAMGeometry2D>(face2D, segment2D, tolerance);
                    if (geometry2Ds_Intersection != null && geometry2Ds_Intersection.Count != 0)
                        geometry3Ds.AddRange(geometry2Ds_Intersection.ConvertAll(x => plane.Convert(x)));
                }
            }

            return new PlanarIntersectionResult(plane, geometry3Ds);
        }

        public static PlanarIntersectionResult PlanarIntersectionResult(Face3D face3D, ISegmentable3D segmentable3D, double tolerance = Core.Tolerance.Distance)
        {
            if (face3D == null || segmentable3D == null)
            {
                return null;
            }

            Plane plane = face3D.GetPlane();
            if(plane == null)
            {
                return null;
            }

            List<Segment3D> segment3Ds = segmentable3D.GetSegments();
            if(segment3Ds == null)
            {
                return null;
            }

            if (!face3D.GetBoundingBox(tolerance).InRange(segmentable3D.GetBoundingBox(tolerance)))
            {
                return new PlanarIntersectionResult(plane);
            }

            List<ISAMGeometry3D> geometry3Ds = new List<ISAMGeometry3D>();
            foreach (Segment3D segment3D in segment3Ds)
            {
                PlanarIntersectionResult planarIntersectionResult_Segment3D = PlanarIntersectionResult(face3D, segment3D, tolerance);
                if(planarIntersectionResult_Segment3D == null || !planarIntersectionResult_Segment3D.Intersecting)
                {
                    continue;
                }

                List<ISAMGeometry3D> geometry3Ds_Segment3D = planarIntersectionResult_Segment3D.Geometry3Ds;
                if(geometry3Ds_Segment3D != null && geometry3Ds_Segment3D.Count != 0)
                {
                    geometry3Ds.AddRange(geometry3Ds_Segment3D);
                }
            }

            return new PlanarIntersectionResult(plane, geometry3Ds);
        }

        public static PlanarIntersectionResult PlanarIntersectionResult(Plane plane, Point3D point3D, Vector3D vector3D, double tolerance = Core.Tolerance.Distance)
        {
            if (plane == null || point3D == null || vector3D == null)
                return null;

            Vector3D normal = plane.Normal;

            double d = normal.DotProduct(vector3D);
            if (System.Math.Abs(d) < tolerance)
            {
                if (plane.Distance(point3D) < tolerance)
                    return new PlanarIntersectionResult(plane, plane.Project(new Line3D(point3D, vector3D)));

                return new PlanarIntersectionResult(plane);
            }

            double u = (plane.K - normal.DotProduct(point3D.ToVector3D())) / d;

            Point3D point3D_Intersection = new Point3D(point3D.X + u * vector3D.X, point3D.Y + u * vector3D.Y, point3D.Z + u * vector3D.Z);

            return new PlanarIntersectionResult(plane, point3D_Intersection);
        }

        public static PlanarIntersectionResult PlanarIntersectionResult(Plane plane, Line3D line3D, double tolerance = Core.Tolerance.Distance)
        {
            return PlanarIntersectionResult(plane, line3D.Origin, line3D.Direction, tolerance);
        }

        public static PlanarIntersectionResult PlanarIntersectionResult(Plane plane_1, Plane plane_2, double tolerance = Core.Tolerance.Angle)
        {
            if (plane_1.Normal.Parallel(plane_2.Normal, tolerance))
                return new PlanarIntersectionResult(plane_1);

            //Calculate tangent of line perpendicular to the normal of the two planes
            Vector3D tangent = plane_1.Normal.CrossProduct(plane_2.Normal).Unit;

            //d-values from plane equation: ax+by+cz+d=0
            double d1 = -plane_1.Normal.DotProduct(new Vector3D(plane_1.Origin.X, plane_1.Origin.Y, plane_1.Origin.Z));
            double d2 = -plane_2.Normal.DotProduct(new Vector3D(plane_2.Origin.X, plane_2.Origin.Y, plane_2.Origin.Z));

            Point3D orgin;

            Vector3D n1 = plane_1.Normal;
            Vector3D n2 = plane_2.Normal;

            if (System.Math.Abs(tangent.Z) >= tolerance)
            {
                double x0 = (n1.Y * d2 - n2.Y * d1) / (n1.X * n2.Y - n2.X * n1.Y);
                double y0 = (n2.X * d1 - n1.X * d2) / (n1.X * n2.Y - n2.X * n1.Y);

                orgin = new Point3D { X = x0, Y = y0, Z = 0 };
            }
            else if (System.Math.Abs(tangent.Y) >= tolerance)
            {
                double x0 = (n1.Z * d2 - n2.Z * d1) / (n1.X * n2.Z - n2.X * n1.Z);
                double z0 = (n2.X * d1 - n1.X * d2) / (n1.X * n2.Z - n2.X * n1.Z);
                orgin = new Point3D { X = x0, Y = 0, Z = z0 };
            }
            else
            {
                double y0 = (n1.Z * d2 - n2.Z * d1) / (n1.Y * n2.Z - n2.Y * n1.Z);
                double z0 = (n2.Y * d1 - n1.Y * d2) / (n1.Y * n2.Z - n2.Y * n1.Z);
                orgin = new Point3D { X = 0, Y = y0, Z = z0 };
            }

            return new PlanarIntersectionResult(plane_1, new Line3D(orgin, tangent));
        }

        public static PlanarIntersectionResult PlanarIntersectionResult(Plane plane, ISegmentable3D segmentable3D, double tolerance_Angle = Core.Tolerance.Angle, double tolerance_Distance = Core.Tolerance.Distance)
        {
            if (segmentable3D is IClosedPlanar3D)
                return PlanarIntersectionResult(plane, (IClosedPlanar3D)segmentable3D, tolerance_Angle, tolerance_Distance);

            List<PlanarIntersectionResult> planarIntersectionResults = segmentable3D.GetSegments()?.ConvertAll(x => PlanarIntersectionResult(plane, x)).FindAll(x => x.Intersecting);
            if (planarIntersectionResults == null || planarIntersectionResults.Count == 0)
                return new PlanarIntersectionResult(plane);

            List<ISAMGeometry3D> geometry3Ds = new List<ISAMGeometry3D>();
            foreach (PlanarIntersectionResult planarIntersectionResult in planarIntersectionResults)
                geometry3Ds.AddRange(planarIntersectionResult.Geometry3Ds);

            return new PlanarIntersectionResult(plane, geometry3Ds);
        }

        public static PlanarIntersectionResult PlanarIntersectionResult(Plane plane, IClosedPlanar3D closedPlanar3D, double tolerance_Angle = Core.Tolerance.Angle, double tolerance_Distance = Core.Tolerance.Distance)
        {
            if (plane == null || closedPlanar3D == null)
                return null;

            if (closedPlanar3D is Face3D)
                return PlanarIntersectionResult(plane, (Face3D)closedPlanar3D, tolerance_Angle, tolerance_Distance);

            Plane plane_ClosedPlanar3D = closedPlanar3D.GetPlane();

            PlanarIntersectionResult planarIntersectionResult = PlanarIntersectionResult(plane, plane_ClosedPlanar3D, tolerance_Angle);
            if (planarIntersectionResult == null || !planarIntersectionResult.Intersecting)
                return new PlanarIntersectionResult(plane);

            Line3D line3D = planarIntersectionResult.GetGeometry3D<Line3D>();
            if (line3D == null)
                return new PlanarIntersectionResult(plane);

            ISegmentable3D segmentable3D = closedPlanar3D as ISegmentable3D;
            if (segmentable3D == null)
                throw new NotImplementedException();

            List<Segment3D> segment3Ds_Segmentable3D = segmentable3D.GetSegments();

            List<Tuple<Segment3D, PlanarIntersectionResult>> tuples = new List<Tuple<Segment3D, PlanarIntersectionResult>>();
            foreach(Segment3D segment3D_Temp in segment3Ds_Segmentable3D)
            {
                PlanarIntersectionResult planarIntersectionResult_Temp = PlanarIntersectionResult(plane, segment3D_Temp, tolerance_Distance);
                if(planarIntersectionResult_Temp == null || !planarIntersectionResult_Temp.Intersecting)
                {
                    continue;
                }

                tuples.Add(new Tuple<Segment3D, PlanarIntersectionResult>(segment3D_Temp, planarIntersectionResult_Temp));
            }

            if (tuples == null || tuples.Count == 0)
                return new PlanarIntersectionResult(plane);

            List<Point3D> point3Ds = new List<Point3D>();
            foreach (PlanarIntersectionResult planarIntersectionResult_Temp in tuples.ConvertAll(x => x.Item2))
            {
                Point3D point3D = planarIntersectionResult_Temp.GetGeometry3D<Point3D>();
                if (point3D != null)
                {
                    Modify.Add(point3Ds, point3D, tolerance_Distance);
                    continue;
                }

                Segment3D segment3D = planarIntersectionResult_Temp.GetGeometry3D<Segment3D>();
                if (segment3D != null)
                {
                    Modify.Add(point3Ds, segment3D[0], tolerance_Distance);
                    Modify.Add(point3Ds, segment3D[1], tolerance_Distance);
                    continue;
                }
            }

            if (point3Ds == null || point3Ds.Count == 0)
                return new PlanarIntersectionResult(plane);

            if (point3Ds.Count == 1)
                return new PlanarIntersectionResult(plane, point3Ds[0]);

            if (point3Ds.Count == 2)
                return new PlanarIntersectionResult(plane, new Segment3D(point3Ds[0], point3Ds[1]));

            List<Point2D> point2Ds = point3Ds.ConvertAll(x => plane_ClosedPlanar3D.Convert(x));
            Planar.Query.ExtremePoints(point2Ds, out Point2D point2D_1, out Point2D point2D_2);
            point2Ds.SortByDistance(point2D_1);
            
            IClosed2D closed2D = plane_ClosedPlanar3D.Convert(closedPlanar3D);
            ISegmentable2D segmentable2D = closed2D as ISegmentable2D;

            point2Ds = point2Ds.ConvertAll(x => segmentable2D.Snap(x));

            List<Segment2D> segment2Ds_Result = new List<Segment2D>();
            List<Point2D> point2Ds_Result = new List<Point2D>();
            for (int i = 0; i < point2Ds.Count - 1; i++)
            {
                Point2D point2D_Mid = point2Ds[i].Mid(point2Ds[i + 1]);

                if (closed2D.Inside(point2D_Mid, tolerance_Distance) || closed2D.On(point2D_Mid, tolerance_Distance))
                {
                    segment2Ds_Result.Add(new Segment2D(point2Ds[i], point2Ds[i + 1]));
                    continue;
                }

                if (segment2Ds_Result.Count == 0)
                {
                    point2Ds_Result.Add(point2Ds[i]);
                }

                if (i == point2Ds.Count - 2)
                {
                    point2Ds_Result.Add(point2Ds[i]);
                }

                if (segment2Ds_Result.Count == 0)
                    continue;

                Segment2D segment2D = segment2Ds_Result.Last();
                if (!segment2D[1].AlmostEquals(point2Ds[i], tolerance_Distance))
                    point2Ds_Result.Add(point2Ds[i]);
            }

            List<ISAMGeometry3D> geometry3Ds = new List<ISAMGeometry3D>();
            geometry3Ds.AddRange(segment2Ds_Result.ConvertAll(x => plane_ClosedPlanar3D.Convert(x)));
            geometry3Ds.AddRange(point2Ds_Result.ConvertAll(x => plane_ClosedPlanar3D.Convert(x)));

            return new PlanarIntersectionResult(plane, geometry3Ds);
        }

        public static PlanarIntersectionResult PlanarIntersectionResult(Plane plane, Face3D face3D, double tolerance_Angle = Core.Tolerance.Angle, double tolerance_Distance = Core.Tolerance.Distance)
        {
            if (plane == null || face3D == null)
                return null;

            Plane plane_Face3D = face3D.GetPlane();
            if (plane_Face3D == null)
                return null;

            if (plane.Coplanar(plane_Face3D, tolerance_Distance))
            {
                if (plane.Distance(plane_Face3D) < tolerance_Distance)
                    return new PlanarIntersectionResult(plane, face3D);
                else
                    return new PlanarIntersectionResult(plane);
            }

            IClosedPlanar3D externaEdge = face3D.GetExternalEdge3D();
            PlanarIntersectionResult planarIntersectionResult_externaEdge = PlanarIntersectionResult(plane, externaEdge, tolerance_Angle, tolerance_Distance);
            if (planarIntersectionResult_externaEdge == null)
                return null;

            if (!planarIntersectionResult_externaEdge.Intersecting)
                return planarIntersectionResult_externaEdge;

            List<IClosedPlanar3D> internalEdges = face3D.GetInternalEdge3Ds();
            if (internalEdges == null || internalEdges.Count == 0)
                return planarIntersectionResult_externaEdge;

            List<PlanarIntersectionResult> PlanarIntersectionResults_InternalEdges = new List<PlanarIntersectionResult>();
            foreach (IClosedPlanar3D internalEdge in internalEdges)
            {
                PlanarIntersectionResult planarIntersectionResult_InternalEdge = PlanarIntersectionResult(plane, internalEdge, tolerance_Angle, tolerance_Distance);
                if (planarIntersectionResult_InternalEdge == null || !planarIntersectionResult_InternalEdge.Intersecting)
                    continue;

                PlanarIntersectionResults_InternalEdges.Add(planarIntersectionResult_InternalEdge);
            }

            if (PlanarIntersectionResults_InternalEdges == null || PlanarIntersectionResults_InternalEdges.Count == 0)
                return planarIntersectionResult_externaEdge;

            List<ISAMGeometry2D> geometry2Ds = planarIntersectionResult_externaEdge.Geometry2Ds;

            List<Segment2D> segment2Ds = geometry2Ds.FindAll(x => x is Segment2D).Cast<Segment2D>().ToList();
            List<Point2D> point2Ds = geometry2Ds.FindAll(x => x is Point2D).Cast<Point2D>().ToList();

            List<ISAMGeometry2D> result = new List<ISAMGeometry2D>();
            foreach (PlanarIntersectionResult planarIntersectionResult_InternalEdge in PlanarIntersectionResults_InternalEdges)
            {
                List<ISAMGeometry2D> geometry2Ds_internalEdge = planarIntersectionResult_InternalEdge.Geometry2Ds;

                List<Segment2D> segment2Ds_internalEdge = geometry2Ds_internalEdge.FindAll(x => x is Segment2D).Cast<Segment2D>().ToList();
                List<Point2D> point2Ds_internalEdge = geometry2Ds_internalEdge.FindAll(x => x is Point2D).Cast<Point2D>().ToList();

                foreach (Point2D point2D in point2Ds_internalEdge)
                {
                    if (point2Ds.Find(x => x.AlmostEquals(point2D)) == null)
                        point2Ds.Add(point2D);
                }

                bool @continue = true;
                while (@continue)
                {
                    @continue = false;

                    int count = 0;
                    foreach (Segment2D segment2D in segment2Ds_internalEdge)
                    {
                        count++;
                        List<Segment2D> segment2Ds_On = segment2Ds.FindAll(x => x.On(segment2D[0], tolerance_Distance) || x.On(segment2D[1], tolerance_Distance));
                        if (segment2Ds_On == null || segment2Ds_On.Count == 0)
                            continue;

                        foreach (Segment2D segment2D_On in segment2Ds_On)
                        {
                            List<Segment2D> segment2Ds_Temp = Planar.Query.Difference(segment2D_On, segment2D, tolerance_Distance);
                            if (segment2Ds_Temp == null || segment2Ds_Temp.Count == 0)
                                continue;

                            segment2Ds.Remove(segment2D_On);
                            segment2Ds.AddRange(segment2Ds_Temp);
                            @continue = true;
                            break;
                        }

                        if (@continue)
                        {
                            segment2Ds_internalEdge.RemoveRange(0, count);
                            break;
                        }
                    }
                }
            }

            Planar.Modify.RemoveAlmostSimilar(segment2Ds, tolerance_Distance);

            point2Ds = point2Ds.Distinct().ToList();
            point2Ds.RemoveAll(x => segment2Ds.Find(y => y.On(x, tolerance_Distance)) != null);

            List<ISAMGeometry3D> geometry3Ds = new List<ISAMGeometry3D>();
            geometry3Ds.AddRange(segment2Ds.ConvertAll(x => plane.Convert(x)));
            geometry3Ds.AddRange(point2Ds.ConvertAll(x => plane.Convert(x)));

            return new PlanarIntersectionResult(plane, geometry3Ds);
        }

        public static PlanarIntersectionResult PlanarIntersectionResult(Face3D face3D_1, Face3D face3D_2, double tolerance_Angle = Core.Tolerance.Angle, double tolerance_Distance = Core.Tolerance.Distance)
        {
            Plane plane_1 = face3D_1?.GetPlane();
            if (plane_1 == null)
                return null;

            Plane plane_2 = face3D_2?.GetPlane();
            if (plane_2 == null)
                return null;

            if(!face3D_1.GetBoundingBox(tolerance_Distance).InRange(face3D_2.GetBoundingBox(tolerance_Distance)))
                return new PlanarIntersectionResult(plane_1);

            if (plane_1.Coplanar(plane_2, tolerance_Distance))
            {
                if (plane_1.Distance(plane_2) > tolerance_Distance)
                    return new PlanarIntersectionResult(plane_1);

                Face3D face3D_Temp = plane_1.Project(face3D_2);
                return new PlanarIntersectionResult(plane_1, Planar.Query.Intersection<ISAMGeometry2D>(plane_1.Convert(face3D_1), plane_1.Convert(face3D_Temp), tolerance_Distance)?.ConvertAll(x => plane_1.Convert(x)));
            }

            PlanarIntersectionResult planarIntersectionResult_1 = PlanarIntersectionResult(plane_1, face3D_2, tolerance_Angle, tolerance_Distance);
            if (planarIntersectionResult_1 == null || !planarIntersectionResult_1.Intersecting)
                return new PlanarIntersectionResult(plane_1);

            PlanarIntersectionResult planarIntersectionResult_2 = PlanarIntersectionResult(plane_2, face3D_1, tolerance_Angle, tolerance_Distance);
            if (planarIntersectionResult_2 == null || !planarIntersectionResult_2.Intersecting)
                return new PlanarIntersectionResult(plane_1);

            List<ISAMGeometry3D> geometry3Ds = new List<ISAMGeometry3D>();

            List<Point3D> point3Ds;

            point3Ds = planarIntersectionResult_1.GetGeometry3Ds<Point3D>();
            if (point3Ds != null && point3Ds.Count > 0)
                point3Ds.FindAll(x => face3D_1.Inside(x)).ForEach(x => geometry3Ds.Add(x));

            point3Ds = planarIntersectionResult_2.GetGeometry3Ds<Point3D>();
            if (point3Ds != null && point3Ds.Count > 0)
                point3Ds.FindAll(x => face3D_2.Inside(x)).ForEach(x => geometry3Ds.Add(x));

            List<Segment3D> segment3Ds = planarIntersectionResult_1.GetGeometry3Ds<Segment3D>();
            if (segment3Ds != null && segment3Ds.Count > 0)
            {
                foreach (Segment3D segment3D_Plane in segment3Ds)
                {
                    PlanarIntersectionResult planarIntersectionResult_Plane = PlanarIntersectionResult(face3D_1, segment3D_Plane, tolerance_Distance);
                    if (planarIntersectionResult_Plane == null || !planarIntersectionResult_Plane.Intersecting)
                        continue;

                    List<Point3D> point3Ds_Plane = planarIntersectionResult_Plane.GetGeometry3Ds<Point3D>();
                    if (point3Ds_Plane != null && point3Ds_Plane.Count > 0)
                        point3Ds_Plane.ForEach(x => geometry3Ds.Add(x));

                    List<Segment3D> segment3Ds_Plane = planarIntersectionResult_Plane.GetGeometry3Ds<Segment3D>();
                    if (segment3Ds_Plane != null && segment3Ds_Plane.Count > 0)
                        segment3Ds_Plane.ForEach(x => geometry3Ds.Add(x));
                }
            }


            return new PlanarIntersectionResult(plane_1, geometry3Ds);
        }
    
        public static PlanarIntersectionResult PlanarIntersectionResult(IClosedPlanar3D closedPlanar3D_1, IClosedPlanar3D closedPlanar3D_2, double tolerance_Angle = Core.Tolerance.Angle, double tolerance_Distance = Core.Tolerance.Distance)
        {
            if(closedPlanar3D_1 == null || closedPlanar3D_2 == null)
            {
                return null;
            }
            
            Face3D face3D_1 = null;
            if(closedPlanar3D_1 is Face3D)
            {
                face3D_1 = (Face3D)closedPlanar3D_1;
            }
            else
            {
                face3D_1 = new Face3D(closedPlanar3D_1);
            }

            Face3D face3D_2 = null;
            if (closedPlanar3D_2 is Face3D)
            {
                face3D_2 = (Face3D)closedPlanar3D_2;
            }
            else
            {
                face3D_2 = new Face3D(closedPlanar3D_2);
            }

            return PlanarIntersectionResult(face3D_1, face3D_2, tolerance_Angle, tolerance_Distance);
        }

        public static PlanarIntersectionResult PlanarIntersectionResult(Plane plane, Mesh3D mesh3D, double tolerance_Angle = Core.Tolerance.Angle, double tolerance_Distance = Core.Tolerance.Distance)
        {
            if(plane == null || mesh3D == null)
            {
                return null;
            }

            BoundingBox3D boundingBox3D = mesh3D.GetBoundingBox();
            if(!plane.Intersect(boundingBox3D, tolerance_Distance))
            {
                return new PlanarIntersectionResult(plane);
            }

            List<ISAMGeometry3D> sAMGeometry3Ds = new List<ISAMGeometry3D>();
            List<Triangle3D> triangle3Ds = mesh3D.GetTriangles();
            if(triangle3Ds != null && triangle3Ds.Count != 0)
            {
                foreach(Triangle3D triangle3D in triangle3Ds)
                {
                    if(triangle3D == null)
                    {
                        continue;
                    }

                    PlanarIntersectionResult planarIntersectionResult = PlanarIntersectionResult(plane, new Face3D(triangle3D), tolerance_Angle, tolerance_Distance);
                    if(planarIntersectionResult != null && planarIntersectionResult.Intersecting)
                    {
                        sAMGeometry3Ds.AddRange(planarIntersectionResult.GetGeometry3Ds<ISAMGeometry3D>());
                    }
                }
            }

            return new PlanarIntersectionResult(plane, sAMGeometry3Ds);
        }

        public static PlanarIntersectionResult PlanarIntersectionResult(Face3D face3D, Shell shell, double tolerance_Angle = Core.Tolerance.Angle, double tolerance_Distance = Core.Tolerance.Distance)
        {
            if(face3D == null || shell == null)
            {
                return null;
            }

            BoundingBox3D boundingBox3D = face3D.GetBoundingBox();
            if(boundingBox3D == null)
            {
                return null;
            }

            BoundingBox3D boundingBox3D_Shell = shell.GetBoundingBox();
            if(boundingBox3D_Shell == null)
            {
                return null;
            }

            Plane plane = face3D.GetPlane();
            if(plane == null)
            {
                return null;
            }

            if(!boundingBox3D.InRange(boundingBox3D_Shell, tolerance_Distance))
            {
                return new PlanarIntersectionResult(plane);
            }

            List<Face3D> face3Ds = shell.Face3Ds;
            if(face3Ds == null || face3Ds.Count == 0)
            {
                return new PlanarIntersectionResult(plane);
            }

            List<ISAMGeometry3D> geometry3Ds = new List<ISAMGeometry3D>();
            foreach (Face3D face3D_Shell in face3Ds)
            {
                PlanarIntersectionResult planarIntersectionResult = PlanarIntersectionResult(face3D, face3D_Shell,tolerance_Angle, tolerance_Distance);
                if(planarIntersectionResult == null || !planarIntersectionResult.Intersecting)
                {
                    continue;
                }

                geometry3Ds.AddRange(planarIntersectionResult.Geometry3Ds);
            }

            return new PlanarIntersectionResult(plane, geometry3Ds);
        }

        public static PlanarIntersectionResult PlanarIntersectionResult(Face3D face3D, IEnumerable<Face3D> face3Ds, double tolerance_Angle = Core.Tolerance.Angle, double tolerance_Distance = Core.Tolerance.Distance)
        {
            if(face3Ds == null)
            {
                return null;
            }

            Plane plane = face3D.GetPlane();
            if(plane == null)
            {
                return null;
            }

            List<ISAMGeometry3D> geometry3Ds = new List<ISAMGeometry3D>();
            foreach (Face3D face3D_Temp in face3Ds)
            {
                PlanarIntersectionResult planarIntersectionResult = PlanarIntersectionResult(face3D, face3D_Temp, tolerance_Angle, tolerance_Distance);
                if (planarIntersectionResult == null || !planarIntersectionResult.Intersecting)
                {
                    continue;
                }

                geometry3Ds.AddRange(planarIntersectionResult.Geometry3Ds);
            }

            return new PlanarIntersectionResult(plane, geometry3Ds);
        }

        public static PlanarIntersectionResult PlanarIntersectionResult(Face3D face3D, Plane plane, double tolerance_Angle = Core.Tolerance.Angle, double tolerance_Distance = Core.Tolerance.Distance)
        {
            PlanarIntersectionResult planarIntersectionResult = PlanarIntersectionResult(plane, face3D, tolerance_Angle, tolerance_Distance);
            if(planarIntersectionResult == null || !planarIntersectionResult.Intersecting)
            {
                return planarIntersectionResult;
            }

            return new PlanarIntersectionResult(face3D.GetPlane(), planarIntersectionResult.GetGeometry3Ds<ISAMGeometry3D>());
        }

        public static PlanarIntersectionResult PlanarIntersectionResult(Plane plane, IFace3DObject face3DObject, double tolerance_Angle = Core.Tolerance.Angle, double tolerance_Distance = Core.Tolerance.Distance)
        {
            return PlanarIntersectionResult(plane, face3DObject?.Face3D, tolerance_Angle, tolerance_Distance);
        }

        public static PlanarIntersectionResult PlanarIntersectionResult<T>(Face3D face3D, IEnumerable<T> face3DObjects, double tolerance_Angle = Core.Tolerance.Angle, double tolerance_Distance = Core.Tolerance.Distance) where T: IFace3DObject
        {
            return PlanarIntersectionResult(face3D, face3DObjects?.ToList().ConvertAll(x => x.Face3D), tolerance_Angle, tolerance_Distance);
        }
    }
}