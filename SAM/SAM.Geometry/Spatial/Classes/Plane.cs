using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace SAM.Geometry.Spatial
{
    public class Plane : SAMGeometry, IPlanar3D
    {
        public static Plane WorldXY { get; } = new Plane(Point3D.Zero, Vector3D.WorldZ);

        public static Plane WorldYZ { get; } = new Plane(Point3D.Zero, Vector3D.WorldX);

        public static Plane WorldXZ { get; } = new Plane(Point3D.Zero, Vector3D.WorldY);

        private Vector3D normal;
        private Point3D origin;
        private Vector3D axisY;

        public Plane()
        {
            normal = Vector3D.WorldZ; //new Vector3D(0, 0, 1);
            origin = Point3D.Zero;
            axisY = normal.AxisY();
        }

        public Plane(JObject jObject)
            : base(jObject)
        {
        }

        public Plane(Plane plane)
        {
            normal = new Vector3D(plane.normal);
            origin = new Point3D(plane.origin);
            axisY = new Vector3D(plane.axisY);
        }

        public Plane(Plane plane, Point3D origin)
        {
            normal = new Vector3D(plane.normal);
            this.origin = new Point3D(origin);
            axisY = new Vector3D(plane.axisY);
        }

        public Plane(Point3D point3D_1, Point3D point3D_2, Point3D point3D_3)
        {
            origin = new Point3D(point3D_1);
            normal = Query.Normal(point3D_1, point3D_2, point3D_3); //new Vector3D(point3D_1, point3D_2).CrossProduct(new Vector3D(point3D_1, point3D_3)).Unit;
            axisY = normal.AxisY();
        }

        public Plane(Point3D origin, Vector3D normal)
        {
            this.normal = normal.Unit;
            this.origin = new Point3D(origin);
            axisY = normal.AxisY();
        }

        public Plane(Point3D origin, Vector3D axisX, Vector3D axisY)
        {
            this.origin = new Point3D(origin);
            this.axisY = axisY.Unit;
            normal = Query.Normal(axisX, axisY); //this.axisY.CrossProduct(axisX).Unit;
        }

        public Vector3D Normal
        {
            get
            {
                return new Vector3D(normal);
            }
        }

        public Point3D Origin
        {
            get
            {
                return new Point3D(origin);
            }
            set
            {
                origin = value;
            }
        }

        public Vector3D AxisX
        {
            get
            {
                //return axisY.CrossProduct(normal);
                //return normal.CrossProduct(axisY);
                return Query.AxisX(normal, axisY);
            }
        }

        public Vector3D AxisY
        {
            get
            {
                return new Vector3D(axisY);
            }
        }

        public Vector3D AxisZ
        {
            get
            {
                return new Vector3D(normal);
            }
        }

        /// <summary>
        /// A factor for point-normal equation A(x−a)+B(y−b)+C(z−c) = 0 where origin(a,b,c), normal(A,B,C)
        /// </summary>
        /// <value>A value for point-normal equation</value>
        public double A
        {
            get
            {
                return normal.X;
            }
        }

        /// <summary>
        /// B factor for point-normal equation A(x−a)+B(y−b)+C(z−c) = 0 where origin(a,b,c), normal(A,B,C)
        /// </summary>
        /// <value>B value for point-normal equation</value>
        public double B
        {
            get
            {
                return normal.Y;
            }
        }

        /// <summary>
        /// C factor for point-normal equation A(x−a)+B(y−b)+C(z−c) = 0 where origin(a,b,c), normal(A,B,C)
        /// </summary>
        /// <value>C value for point-normal equation</value>
        public double C
        {
            get
            {
                return normal.Z;
            }
        }

        /// <summary>
        /// D factor for point-normal equation Ax+By+Cz = D where origin(a,b,c), normal(A,B,C)
        /// </summary>
        /// <value>D value for point-normal equation</value>
        public double D
        {
            get
            {
                return -(normal.X * origin.X + normal.Y * origin.Y + normal.Z * origin.Z);
            }
        }

        /// <summary>
        /// Scalar constant relating origin point to normal vector.
        /// </summary>
        public double K
        {
            get
            {
                return normal.DotProduct(origin.ToVector3D());
            }
        }

        public Planar.ISAMGeometry2D Convert(IBoundable3D geometry)
        {
            return Convert(geometry as dynamic);
        }

        public ISAMGeometry3D Convert(Planar.ISAMGeometry2D geometry)
        {
            return Convert(geometry as dynamic);
        }

        public PolycurveLoop3D Convert(Planar.PolycurveLoop2D polycurveLoop2D)
        {
            if (polycurveLoop2D == null)
                return null;

            List<Planar.ICurve2D> curve2Ds = polycurveLoop2D.GetCurves();

            if (curve2Ds.TrueForAll(x => x is Planar.Segment2D))
                return new PolycurveLoop3D(curve2Ds.ConvertAll(x => Convert((Planar.Segment2D)x)));

            throw new NotImplementedException();
        }

        public ICurve3D Convert(Planar.ICurve2D curve2D)
        {
            return Convert(curve2D as dynamic);
        }

        public Point3D Convert(Planar.Point2D point2D)
        {
            Vector3D axisX = AxisX;

            Vector3D u = new Vector3D(axisY.X * point2D.Y, axisY.Y * point2D.Y, axisY.Z * point2D.Y);
            Vector3D v = new Vector3D(axisX.X * point2D.X, axisX.Y * point2D.X, axisX.Z * point2D.X);

            return new Point3D(Origin.X + u.X + v.X, Origin.Y + u.Y + v.Y, Origin.Z + u.Z + v.Z);
        }

        public Planar.Point2D Convert(Point3D point3D)
        {
            Vector3D vector3D = new Vector3D(point3D.X - origin.X, point3D.Y - origin.Y, point3D.Z - origin.Z);
            return new Planar.Point2D(AxisX.DotProduct(vector3D), axisY.DotProduct(vector3D));
        }

        public Planar.Vector2D Convert(Vector3D vector3D)
        {
            //Vector3D vector3D_Result = new Vector3D(vector3D.X - origin.X, vector3D.Y - origin.Y, vector3D.Z - origin.Z);
            return new Planar.Vector2D(AxisX.DotProduct(vector3D), axisY.DotProduct(vector3D));
        }

        public Vector3D Convert(Planar.Vector2D vector2D)
        {
            Vector3D axisX = AxisX;

            Vector3D u = new Vector3D(axisY.X * vector2D.Y, axisY.Y * vector2D.Y, axisY.Z * vector2D.Y);
            Vector3D v = new Vector3D(axisX.X * vector2D.X, axisX.Y * vector2D.X, axisX.Z * vector2D.X);

            return new Vector3D(u.X + v.X, u.Y + v.Y, u.Z + v.Z);
        }

        public Polygon3D Convert(Planar.Polygon2D polygon2D)
        {
            //return new Polygon3D(Convert(polygon2D.Points));
            return new Polygon3D(this, polygon2D.Points);
        }

        public Polygon3D Convert(Planar.Rectangle2D rectangle2D)
        {
            //return new Polygon3D(Convert(rectangle2D.GetPoints()));
            return new Polygon3D(this, rectangle2D.GetPoints());
        }

        public Planar.Polygon2D Convert(Polygon3D polygon3D)
        {
            return new Planar.Polygon2D(Convert(polygon3D.GetPoints()));
        }

        public Planar.Polyline2D Convert(Polyline3D polyline3D)
        {
            return new Planar.Polyline2D(Convert(polyline3D.GetPoints()));
        }

        public Polyline3D Convert(Planar.Polyline2D polyline2D)
        {
            return new Polyline3D(Convert(polyline2D.GetPoints()));
        }

        public Planar.Triangle2D Convert(Triangle3D triangle3D)
        {
            List<Point3D> point3Ds = triangle3D.GetPoints();
            return new Planar.Triangle2D(Convert(point3Ds[0]), Convert(point3Ds[1]), Convert(point3Ds[2]));
        }

        public Planar.Segment2D Convert(Segment3D segment3D)
        {
            return new Planar.Segment2D(Convert(segment3D[0]), Convert(segment3D[1]));
        }

        public Segment3D Convert(Planar.Segment2D segment2D)
        {
            return new Segment3D(Convert(segment2D.Start), Convert(segment2D.End));
        }

        public Planar.Line2D Convert(Line3D line3D)
        {
            return new Planar.Line2D(Convert(line3D.Origin), Convert(line3D.Direction));
        }

        public Line3D Convert(Planar.Line2D line2D)
        {
            return new Line3D(Convert(line2D.Origin), Convert(line2D.Direction));
        }

        public IClosedPlanar3D Convert(Planar.IClosed2D closed2D)
        {
            return Convert(closed2D as dynamic);
        }

        public Planar.Face2D Convert(Face3D face3D)
        {
            IClosedPlanar3D closedPlanar3D_External = face3D.GetExternalEdge();
            Planar.IClosed2D closed2D_external = Convert(closedPlanar3D_External);

            List<Planar.IClosed2D> closed2Ds_internal = new List<Planar.IClosed2D>();
            List<IClosedPlanar3D> closedPlanar3Ds_Internal = face3D.GetInternalEdges();
            if (closedPlanar3Ds_Internal != null && closedPlanar3Ds_Internal.Count > 0)
                closedPlanar3Ds_Internal.ForEach(x => closed2Ds_internal.Add(Convert(x)));

            return Planar.Face2D.Create(closed2D_external, closed2Ds_internal);
        }

        public Face3D Convert(Planar.Face2D face2D)
        {
            return new Face3D(this, face2D);
        }

        public Planar.IClosed2D Convert(IClosed3D closed3D)
        {
            return Convert(closed3D as dynamic);
        }

        public List<Planar.Point2D> Convert(IEnumerable<Point3D> point3Ds)
        {
            List<Planar.Point2D> point2ds = new List<Planar.Point2D>();
            foreach (Point3D point3D in point3Ds)
                point2ds.Add(Convert(point3D));

            return point2ds;
        }

        public List<Point3D> Convert(IEnumerable<Planar.Point2D> point2Ds)
        {
            List<Point3D> point3ds = new List<Point3D>();
            foreach (Planar.Point2D point2D in point2Ds)
                point3ds.Add(Convert(point2D));

            return point3ds;
        }

        public double Distance(Point3D point3D)
        {
            return Closest(point3D).Distance(point3D);
        }

        public double Distance(Segment3D segment3D)
        {
            PlanarIntersectionResult planarIntersectionResult = Intersection(segment3D);
            if (planarIntersectionResult == null)
                return double.NaN;

            if (!planarIntersectionResult.Intersecting)
                return System.Math.Min(Distance(segment3D[0]), Distance(segment3D[1]));

            return 0;
        }

        public double Distance(ISegmentable3D segmentable3D)
        {
            List<Segment3D> segment3Ds = segmentable3D?.GetSegments();
            if (segment3Ds == null || segment3Ds.Count == 0)
                return double.MinValue;

            double result = double.MaxValue;
            foreach (Segment3D segment3D in segment3Ds)
            {
                result = System.Math.Min(Distance(segment3D), result);
                if (result == 0)
                    return result;
            }

            return result;
        }

        public double Distance(Plane plane)
        {
            if (!Coplanar(plane))
                return 0;

            return Distance(plane.origin);
        }

        public bool On(Point3D point3D, double tolerance = Core.Tolerance.Distance)
        {
            return System.Math.Abs((normal.X * (point3D.X - origin.X)) + (normal.Y * (point3D.Y - origin.Y)) + (normal.Z * (point3D.Z - origin.Z))) < tolerance;
        }

        public Point3D Closest(Point3D point3D)
        {
            double factor = point3D.ToVector3D().DotProduct(normal) - K;
            return new Point3D(point3D.X - (normal.X * factor), point3D.Y - (normal.Y * factor), point3D.Z - (normal.Z * factor));
        }

        public Vector3D Project(Vector3D vector3D)
        {
            double factor = vector3D.DotProduct(normal) - K;
            return new Vector3D(vector3D.X - (normal.X * factor), vector3D.Y - (normal.Y * factor), vector3D.Z - (normal.Z * factor));
        }

        public Point3D Project(Point3D point3D)
        {
            return Closest(point3D);
        }

        public Segment3D Project(Segment3D segment3D)
        {
            return new Segment3D(Closest(segment3D[0]), Closest(segment3D[1]));
        }

        public Triangle3D Project(Triangle3D triangle3D)
        {
            List<Point3D> point3Ds = triangle3D.GetPoints();
            return new Triangle3D(Closest(point3Ds[0]), point3Ds[1], point3Ds[2]);
        }

        public Polyline3D Project(Polyline3D polyline3D)
        {
            return new Polyline3D(polyline3D.Points.ConvertAll(x => Closest(x)));
        }

        public Polygon3D Project(Polygon3D polygon3D)
        {
            List<Point3D> point3Ds = polygon3D.GetPoints();
            if (point3Ds == null)
                return null;

            List<Planar.Point2D> point2Ds = point3Ds.ConvertAll(x => Convert(x));

            return new Polygon3D(this, point2Ds);
        }

        public ICurve3D Project(ICurve3D curve)
        {
            return Project(curve as dynamic);
        }

        public Face3D Project(Face3D face3D)
        {
            Planar.IClosed2D externalEdge = Convert(Project(face3D.GetExternalEdge()));

            List<IClosedPlanar3D> internalEdges = face3D.GetInternalEdges();
            if (internalEdges != null && internalEdges.Count > 0)
                internalEdges = internalEdges.ConvertAll(x => Project(x));

            return Face3D.Create(this, externalEdge, internalEdges?.ConvertAll(x => Convert(x)));
        }

        public Line3D Project(Line3D line3D)
        {
            return new Line3D(Project(line3D.Origin), Project(line3D.Direction));
        }

        public IClosedPlanar3D Project(IClosedPlanar3D closedPlanar3D)
        {
            if (closedPlanar3D == null)
                return null;

            return Project(closedPlanar3D as dynamic);
        }

        public void FlipZ(bool flipX = true)
        {
            Vector3D axisZ = normal.GetNegated();

            if (!flipX)
                axisY = Query.AxisY(axisZ, AxisX);

            normal = axisZ;
        }

        public void FlipX(bool flipY = true)
        {
            Vector3D axisX = AxisX.GetNegated();
            if(!flipY)
                normal = Query.Normal(axisX, axisY);
            else
                axisY = Query.AxisY(normal, axisX);
        }

        public PlanarIntersectionResult Intersection(Segment3D segment3D, double tolerance = Core.Tolerance.Distance)
        {
            return PlanarIntersectionResult.Create(this, segment3D, tolerance);
        }

        public PlanarIntersectionResult Intersection(IClosedPlanar3D closedPlanar3D, double tolerance = Core.Tolerance.Distance)
        {
            if (closedPlanar3D is Face3D)
                return Intersection((Face3D)closedPlanar3D);

            return PlanarIntersectionResult.Create(this, closedPlanar3D, tolerance);
        }

        public PlanarIntersectionResult Intersection(Face3D face3D, double tolerance = Core.Tolerance.Distance)
        {
            return PlanarIntersectionResult.Create(this, face3D, tolerance);
        }

        public PlanarIntersectionResult Intersection(Plane plane, double tolerance = Core.Tolerance.Distance)
        {
            return PlanarIntersectionResult.Create(this, plane, tolerance);
        }

        public ISAMGeometry3D GetMoved(Vector3D vector3D)
        {
            Plane plane = new Plane((Point3D)origin.GetMoved(vector3D), (Vector3D)normal.Clone());
            plane.axisY = axisY;

            return plane;
        }

        public Plane GetPlane()
        {
            return new Plane(this);
        }

        public void Move(Vector3D vector3D)
        {
            if (vector3D == null)
                return;

            origin.Move(vector3D);
        }

        public override ISAMGeometry Clone()
        {
            return new Plane(this);
        }

        public bool Coplanar(Plane plane, double tolerance = Core.Tolerance.Distance)
        {
            return normal.AlmostEqual(plane.normal, tolerance) || normal.AlmostEqual(-plane.normal, tolerance);
        }

        public void Reverse()
        {
            normal.Negate();
            axisY.Negate();
        }

        public override bool FromJObject(JObject jObject)
        {
            origin = new Point3D(jObject.Value<JObject>("Origin"));
            normal = new Vector3D(jObject.Value<JObject>("Normal"));

            if (jObject.ContainsKey("AxisY"))
                axisY = new Vector3D(jObject.Value<JObject>("AxisY"));
            else
                axisY = normal.AxisY();

            return true;
        }

        public override JObject ToJObject()
        {
            JObject jObject = base.ToJObject();
            if (jObject == null)
                return null;

            jObject.Add("Origin", origin.ToJObject());
            jObject.Add("Normal", normal.ToJObject());
            jObject.Add("AxisY", axisY.ToJObject());

            return jObject;
        }

        public static bool operator ==(Plane plane_1, Plane plane_2)
        {
            if (ReferenceEquals(plane_1, null) && ReferenceEquals(plane_2, null))
                return true;

            if (ReferenceEquals(plane_1, null))
                return false;

            if (ReferenceEquals(plane_2, null))
                return false;

            return plane_1.origin == plane_2.origin && plane_1.normal == plane_2.normal && plane_1.axisY == plane_2.axisY;
        }

        public static bool operator !=(Plane plane_1, Plane plane_2)
        {
            if (ReferenceEquals(plane_1, null) && ReferenceEquals(plane_2, null))
                return false;

            if (ReferenceEquals(plane_1, null))
                return true;

            if (ReferenceEquals(plane_2, null))
                return true;

            return plane_1.origin != plane_2.origin || plane_1.normal != plane_2.normal || plane_1.axisY != plane_2.axisY;
        }
    }
}