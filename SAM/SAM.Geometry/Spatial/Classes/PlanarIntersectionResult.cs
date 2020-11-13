using SAM.Geometry.Planar;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Geometry.Spatial
{
    public class PlanarIntersectionResult : IIntersectionResult3D
    {
        private List<ISAMGeometry2D> geometry2Ds;
        private Plane plane;

        public PlanarIntersectionResult(Plane plane, IEnumerable<ISAMGeometry3D> sAMGeometry3Ds)
        {
            this.plane = plane;

            if (plane != null && sAMGeometry3Ds != null)
                geometry2Ds = sAMGeometry3Ds.ToList().ConvertAll(x => plane.Convert(x as dynamic) as ISAMGeometry2D);
        }

        public PlanarIntersectionResult(Plane plane, Point3D point3D)
        {
            this.plane = plane;

            if (point3D != null && plane != null)
            {
                geometry2Ds = new List<ISAMGeometry2D>();
                geometry2Ds.Add(plane.Convert(plane.Project(point3D)));
            }
        }

        public PlanarIntersectionResult(Plane plane, Line3D line3D)
        {
            this.plane = plane;

            if (line3D != null && plane != null)
            {
                geometry2Ds = new List<ISAMGeometry2D>();
                geometry2Ds.Add(plane.Convert(plane.Project(line3D)));
            }
        }

        public PlanarIntersectionResult(Plane plane, Segment3D segment3D)
        {
            this.plane = plane;

            if (segment3D != null && plane != null)
            {
                geometry2Ds = new List<ISAMGeometry2D>();
                geometry2Ds.Add(plane.Convert(plane.Project(segment3D)));
            }
        }

        public PlanarIntersectionResult(Plane plane, Face3D face3D)
        {
            this.plane = plane;

            if (face3D != null && plane != null)
            {
                geometry2Ds = new List<ISAMGeometry2D>();
                geometry2Ds.Add(plane.Convert(plane.Project(face3D)));
            }
        }

        public PlanarIntersectionResult(Plane plane)
        {
            this.plane = plane;
        }

        public bool Intersecting
        {
            get
            {
                return geometry2Ds != null && geometry2Ds.Count > 0;
            }
        }

        public List<ISAMGeometry2D> Geometry2Ds
        {
            get
            {
                return geometry2Ds?.ConvertAll(x => x.Clone() as ISAMGeometry2D);
            }
        }

        public ISAMGeometry3D Geometry3D
        {
            get
            {
                if (geometry2Ds == null || geometry2Ds.Count == 0)
                    return null;

                return plane.Convert(geometry2Ds[0]);
            }
        }

        public List<ISAMGeometry3D> Geometry3Ds
        {
            get
            {
                return geometry2Ds?.ConvertAll(x => plane?.Convert(x) as ISAMGeometry3D);
            }
        }

        public T GetGeometry3D<T>() where T : SAMGeometry, ISAMGeometry3D
        {
            ISAMGeometry3D result = Geometry3Ds?.Find(x => x is T);
            if (result == null)
                return null;

            return (T)result;
        }

        public List<T> GetGeometry3Ds<T>() where T : SAMGeometry, ISAMGeometry3D
        {
            return Geometry3Ds?.FindAll(x => x is T).ConvertAll(x => (T)x);
        }

        public List<T> GetGeometry2Ds<T>() where T : SAMGeometry, ISAMGeometry2D
        {
            return geometry2Ds?.FindAll(x => x is T).ConvertAll(x => (T)x);
        }

        public static PlanarIntersectionResult Create(Plane plane, Point3D point3D)
        {
            if (plane == null || point3D == null)
                return null;

            return new PlanarIntersectionResult(plane, point3D);
        }

        public static PlanarIntersectionResult Create(Plane plane, Point3D point3D, Vector3D vector3D, double tolerance = Core.Tolerance.Distance)
        {
            return Create(plane, new Line3D(point3D, vector3D), tolerance);
        }

        public static PlanarIntersectionResult Create(Face3D face3D, Point3D point3D, Vector3D vector3D, double tolerance = Core.Tolerance.Distance)
        {
            Plane plane = face3D?.GetPlane();
            if (plane == null)
                return null;

            PlanarIntersectionResult planarIntersectionResult = Create(plane, point3D, vector3D, tolerance);
            if (planarIntersectionResult == null)
                return null;

            if (!planarIntersectionResult.Intersecting)
                return planarIntersectionResult;

            Point3D point3D_Plane = planarIntersectionResult.GetGeometry3D<Point3D>();
            
            if(point3D_Plane != null && face3D.InRange(point3D_Plane, tolerance))
                return new PlanarIntersectionResult(plane, point3D_Plane);

            return new PlanarIntersectionResult(plane);
        }

        public static PlanarIntersectionResult Create(Plane plane, Segment3D segment3D, double tolerance = Core.Tolerance.Distance)
        {
            if (plane == null || segment3D == null)
                return null;

            Vector3D direction = segment3D.Direction;

            Vector3D normal = plane.Normal;

            double d = normal.DotProduct(direction);
            if (System.Math.Abs(d) < tolerance)
            {
                if (plane.Distance(segment3D[0]) < tolerance)
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

        public static PlanarIntersectionResult Create(Face3D face3D, Segment3D segment3D, double tolerance = Core.Tolerance.Distance)
        {
            if (face3D == null || segment3D == null)
                return null;

            Plane plane = face3D.GetPlane();
            if (plane == null)
                return null;

            PlanarIntersectionResult planarIntersectionResult = Create(plane, segment3D, tolerance);
            if (planarIntersectionResult == null)
                return null;

            if (!planarIntersectionResult.Intersecting)
                return planarIntersectionResult;

            List<ISAMGeometry3D> geometry3Ds = new List<ISAMGeometry3D>();

            List<Point3D> point3Ds = planarIntersectionResult.GetGeometry3Ds<Point3D>();
            if(point3Ds != null && point3Ds.Count > 0)
            {
                point3Ds = point3Ds.FindAll(x => face3D.Inside(x));
                geometry3Ds.AddRange(point3Ds);
            }

            return new PlanarIntersectionResult(plane, geometry3Ds);
        }

        public static PlanarIntersectionResult Create(Plane plane, Line3D line3D, double tolerance = Core.Tolerance.Distance)
        {
            if (plane == null || line3D == null)
                return null;

            Vector3D direction = line3D.Direction;

            Vector3D normal = plane.Normal;

            Point3D origin = line3D.Origin;

            double d = normal.DotProduct(direction);
            if (System.Math.Abs(d) < tolerance)
            {
                if (plane.Distance(origin) < tolerance)
                    return new PlanarIntersectionResult(plane, plane.Project(line3D));
                else
                    return new PlanarIntersectionResult(plane);
            }

            double u = (plane.K - normal.DotProduct(origin.ToVector3D())) / d;

            Point3D point3D_Intersection = new Point3D(origin.X + u * direction.X, origin.Y + u * direction.Y, origin.Z + u * direction.Z);

            return new PlanarIntersectionResult(plane, point3D_Intersection);
        }

        public static PlanarIntersectionResult Create(Plane plane_1, Plane plane_2, double tolerance = Core.Tolerance.Angle)
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

        public static PlanarIntersectionResult Create(Plane plane, IClosedPlanar3D closedPlanar3D, double tolerance_Angle = Core.Tolerance.Angle, double tolerance_Distance = Core.Tolerance.Distance)
        {
            if (plane == null || closedPlanar3D == null)
                return null;

            if (closedPlanar3D is Face3D)
                return Create(plane, (Face3D)closedPlanar3D, tolerance_Angle, tolerance_Distance);

            Plane plane_ClosedPlanar3D = closedPlanar3D.GetPlane();

            PlanarIntersectionResult planarIntersectionResult = Create(plane, plane_ClosedPlanar3D, tolerance_Angle);
            if (planarIntersectionResult == null || !planarIntersectionResult.Intersecting)
                return new PlanarIntersectionResult(plane);

            Line3D line3D = planarIntersectionResult.GetGeometry3D<Line3D>();
            if (line3D == null)
                return new PlanarIntersectionResult(plane);

            ISegmentable3D segmentable3D = closedPlanar3D as ISegmentable3D;
            if (segmentable3D == null)
                throw new NotImplementedException();

            List<Segment3D> segment3Ds_Segmentable3D = segmentable3D.GetSegments();

            List<PlanarIntersectionResult> planarIntersectionResults_All = segment3Ds_Segmentable3D?.ConvertAll(x => Create(plane, x)).FindAll(x => x.Intersecting);
            if (planarIntersectionResults_All == null || planarIntersectionResults_All.Count == 0)
                return new PlanarIntersectionResult(plane);

            List<Point3D> point3Ds = new List<Point3D>();
            foreach (PlanarIntersectionResult planarIntersectionResult_Temp in planarIntersectionResults_All)
            {
                Point3D point3D = planarIntersectionResult_Temp.GetGeometry3D<Point3D>();
                if(point3D != null)
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

            if(point3Ds == null || point3Ds.Count == 0)
                return new PlanarIntersectionResult(plane);

            if(point3Ds.Count == 1)
                return new PlanarIntersectionResult(plane, point3Ds[0]);

            List<Point2D> point2Ds = point3Ds.ConvertAll(x => plane_ClosedPlanar3D.Convert(x));
            IClosed2D closed2D = plane_ClosedPlanar3D.Convert(closedPlanar3D);

            List<Segment2D> segment2Ds_Result = new List<Segment2D>();
            List<Point2D> point2Ds_Result = new List<Point2D>();
            for(int i = 0; i < point2Ds.Count - 1; i++)
            {
                Point2D point2D_Mid = point2Ds[i].Mid(point2Ds[i + 1]);

                if (closed2D.Inside(point2D_Mid, tolerance_Distance) || closed2D.On(point2D_Mid, tolerance_Distance))
                {
                    segment2Ds_Result.Add(new Segment2D(point2Ds[i], point2Ds[i + 1]));
                    continue;
                }

                if(segment2Ds_Result.Count == 0)
                {
                    point2Ds.Add(point2Ds[i]);
                }

                if(i == point2Ds.Count - 2)
                {
                    point2Ds.Add(point2Ds[i]);
                }

                if (segment2Ds_Result.Count == 0)
                    continue;

                Segment2D segment2D = segment2Ds_Result.Last();
                if (!segment2D[1].AlmostEquals(point2Ds[i], tolerance_Distance))
                    point2Ds.Add(point2Ds[i]);
            }

            List<ISAMGeometry3D> geometry3Ds = new List<ISAMGeometry3D>();
            geometry3Ds.AddRange(segment2Ds_Result.ConvertAll(x => plane_ClosedPlanar3D.Convert(x)));
            geometry3Ds.AddRange(point2Ds_Result.ConvertAll(x => plane_ClosedPlanar3D.Convert(x)));

            return new PlanarIntersectionResult(plane, geometry3Ds);
        }

        public static PlanarIntersectionResult Create(Plane plane, Face3D face3D, double tolerance_Angle = Core.Tolerance.Angle, double tolerance_Distance = Core.Tolerance.Distance)
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
            PlanarIntersectionResult planarIntersectionResult_externaEdge = Create(plane, externaEdge, tolerance_Angle, tolerance_Distance);
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
                PlanarIntersectionResult planarIntersectionResult_InternalEdge = Create(plane, internalEdge, tolerance_Angle, tolerance_Distance);
                if (planarIntersectionResult_InternalEdge == null || !planarIntersectionResult_InternalEdge.Intersecting)
                    continue;

                PlanarIntersectionResults_InternalEdges.Add(planarIntersectionResult_InternalEdge);
            }

            if (PlanarIntersectionResults_InternalEdges == null || PlanarIntersectionResults_InternalEdges.Count == 0)
                return planarIntersectionResult_externaEdge;

            List<Segment2D> segment2Ds = planarIntersectionResult_externaEdge.geometry2Ds.FindAll(x => x is Segment2D).Cast<Segment2D>().ToList();
            List<Point2D> point2Ds = planarIntersectionResult_externaEdge.geometry2Ds.FindAll(x => x is Point2D).Cast<Point2D>().ToList();

            List<ISAMGeometry2D> result = new List<ISAMGeometry2D>();
            foreach (PlanarIntersectionResult planarIntersectionResult_InternalEdge in PlanarIntersectionResults_InternalEdges)
            {
                List<ISAMGeometry2D> geometry2Ds_internalEdge = planarIntersectionResult_InternalEdge.geometry2Ds;

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

        public static PlanarIntersectionResult Create(Face3D face3D_1, Face3D face3D_2, double tolerance_Angle = Core.Tolerance.Angle, double tolerance_Distance = Core.Tolerance.Distance)
        {
            if (face3D_1 == null || face3D_2 == null)
                return null;

            Plane plane_1 = face3D_1.GetPlane();
            if (plane_1 == null)
                return null;

            Plane plane_2 = face3D_2.GetPlane();
            if (plane_2 == null)
                return null;

            if (plane_1.Coplanar(plane_2, tolerance_Distance))
            {
                if (plane_1.Distance(plane_2) > tolerance_Distance)
                    return new PlanarIntersectionResult(plane_1);

                face3D_2 = plane_1.Project(face3D_2);
                return new PlanarIntersectionResult(plane_1, Planar.Query.Intersection(plane_1.Convert(face3D_1), plane_1.Convert(face3D_2))?.ConvertAll(x => plane_1.Convert(x)));
            }

            PlanarIntersectionResult planarIntersectionResult_1 = Create(plane_1, face3D_2, tolerance_Angle, tolerance_Distance);
            if (planarIntersectionResult_1 == null || !planarIntersectionResult_1.Intersecting)
                return new PlanarIntersectionResult(plane_1);

            PlanarIntersectionResult planarIntersectionResult_2 = Create(plane_2, face3D_1, tolerance_Angle, tolerance_Distance);
            if (planarIntersectionResult_2 == null || !planarIntersectionResult_2.Intersecting)
                return new PlanarIntersectionResult(plane_1);

            //TODO: Check and process intersection results
            List<ISAMGeometry3D> geometry3Ds = new List<ISAMGeometry3D>();

            List<Point3D> point3Ds;

            point3Ds = planarIntersectionResult_1.GetGeometry3Ds<Point3D>();
            if (point3Ds != null && point3Ds.Count > 0)
                point3Ds.FindAll(x => face3D_1.Inside(x)).ForEach(x => geometry3Ds.Add(x));

            point3Ds = planarIntersectionResult_2.GetGeometry3Ds<Point3D>();
            if (point3Ds != null && point3Ds.Count > 0)
                point3Ds.FindAll(x => face3D_2.Inside(x)).ForEach(x => geometry3Ds.Add(x));

            throw new NotImplementedException();
        }
    }
}