using Newtonsoft.Json.Linq;
using SAM.Core;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Geometry.Spatial
{
    public class Plane : SAMGeometry, IPlanar3D
    {
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
            normal = Query.Normal(axisX.Unit, this.axisY); //this.axisY.CrossProduct(axisX).Unit;
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

        public double Distance(Plane plane, double tolerance = Tolerance.Distance)
        {
            if (!Coplanar(plane, tolerance))
                return 0;

            return Distance(plane.origin);
        }

        public bool On(Point3D point3D, double tolerance = Tolerance.Distance)
        {
            return System.Math.Abs((normal.X * (point3D.X - origin.X)) + (normal.Y * (point3D.Y - origin.Y)) + (normal.Z * (point3D.Z - origin.Z))) < tolerance;
        }

        public Point3D Closest(Point3D point3D)
        {
            double factor = point3D.ToVector3D().DotProduct(normal) - K;
            return new Point3D(point3D.X - (normal.X * factor), point3D.Y - (normal.Y * factor), point3D.Z - (normal.Z * factor));
        }

        public Point3D Closest(Point3D point3D, Vector3D vector3D, double tolerance = Tolerance.Distance)
        {
            PlanarIntersectionResult planarIntersectionResult = PlanarIntersectionResult.Create(this, point3D, vector3D, tolerance);
            if (planarIntersectionResult == null || !planarIntersectionResult.Intersecting)
                return null;

            return planarIntersectionResult.GetGeometry3Ds<Point3D>()?.FirstOrDefault();
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
            if (!flipY)
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

        public static Plane WorldXY
        {
            get
            {
                return new Plane(Point3D.Zero, Vector3D.WorldZ);
            }
        }

        public static Plane WorldYZ
        {
            get
            {
                return new Plane(Point3D.Zero, Vector3D.WorldX);
            }
        }

        public static Plane WorldXZ
        {
            get
            {
                return new Plane(Point3D.Zero, Vector3D.WorldY);
            }
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