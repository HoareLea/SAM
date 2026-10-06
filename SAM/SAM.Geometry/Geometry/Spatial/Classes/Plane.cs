// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using SAM.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;

namespace SAM.Geometry.Spatial
{
    public class Plane : SAMGeometry, IPlanar3D
    {
        private Vector3D normal;
        private Point3D origin;
        private Vector3D axisY;

        internal Vector3D InternalNormal => normal;
        internal Point3D InternalOrigin => origin;
        internal Vector3D InternalAxisY => axisY;

        public Plane()
        {
            normal = Vector3D.WorldZ;
            origin = Point3D.Zero;
            axisY = normal.AxisY();
        }

        public Plane(JsonObject jsonObject)
            : base(jsonObject)
        {
        }

        public Plane(Plane plane)
        {
            if (plane == null)
                throw new ArgumentNullException(nameof(plane));

            normal = new Vector3D(plane.normal);
            origin = new Point3D(plane.origin);
            axisY = new Vector3D(plane.axisY);
        }

        public Plane(Plane plane, Point3D origin)
        {
            if (plane == null)
                throw new ArgumentNullException(nameof(plane));

            normal = new Vector3D(plane.normal);
            this.origin = origin != null ? new Point3D(origin) : Point3D.Zero;
            axisY = new Vector3D(plane.axisY);
        }

        public Plane(Point3D point3D_1, Point3D point3D_2, Point3D point3D_3)
        {
            origin = new Point3D(point3D_1);
            normal = Query.Normal(point3D_1, point3D_2, point3D_3);
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
            normal = Query.Normal(axisX.Unit, this.axisY);
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
                return normal.X * origin.X + normal.Y * origin.Y + normal.Z * origin.Z;
            }
        }

        public double Distance(Point3D point3D)
        {
            if (point3D == null)
                return double.NaN;

            return System.Math.Abs((normal.X * (point3D.X - origin.X)) + (normal.Y * (point3D.Y - origin.Y)) + (normal.Z * (point3D.Z - origin.Z)));
        }

        public double Distance(Segment3D segment3D)
        {
            if (segment3D == null)
                return double.NaN;

            Point3D p0 = segment3D[0];
            Point3D p1 = segment3D[1];
            if (p0 == null || p1 == null)
                return double.NaN;

            double d0 = (normal.X * (p0.X - origin.X)) + (normal.Y * (p0.Y - origin.Y)) + (normal.Z * (p0.Z - origin.Z));
            double d1 = (normal.X * (p1.X - origin.X)) + (normal.Y * (p1.Y - origin.Y)) + (normal.Z * (p1.Z - origin.Z));

            if (d0 * d1 <= 0)
                return 0;

            return System.Math.Min(System.Math.Abs(d0), System.Math.Abs(d1));
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
            if (plane == null)
                return double.NaN;

            if (!Coplanar(plane, tolerance))
                return 0;

            return Distance(plane.origin);
        }

        public bool On(Point3D point3D, double tolerance = Tolerance.Distance)
        {
            if (point3D == null)
            {
                return false;
            }

            return System.Math.Abs((normal.X * (point3D.X - origin.X)) + (normal.Y * (point3D.Y - origin.Y)) + (normal.Z * (point3D.Z - origin.Z))) < tolerance;
        }

        public Point3D Closest(Point3D point3D)
        {
            if (point3D == null)
            {
                return null;
            }

            double factor = (normal.X * (point3D.X - origin.X)) + (normal.Y * (point3D.Y - origin.Y)) + (normal.Z * (point3D.Z - origin.Z));
            return new Point3D(point3D.X - (normal.X * factor), point3D.Y - (normal.Y * factor), point3D.Z - (normal.Z * factor));
        }

        public Point3D Closest(Point3D point3D, Vector3D vector3D, double tolerance = Tolerance.Distance)
        {
            PlanarIntersectionResult planarIntersectionResult = Create.PlanarIntersectionResult(this, point3D, vector3D, tolerance);
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

        public ISAMGeometry3D GetMoved(Vector3D vector3D)
        {
            if (vector3D == null)
                return new Plane(this);

            Point3D movedOrigin = new Point3D(origin.X + vector3D.X, origin.Y + vector3D.Y, origin.Z + vector3D.Z);
            Plane plane = new Plane(movedOrigin, normal);
            plane.axisY = new Vector3D(axisY);

            return plane;
        }

        public ISAMGeometry3D GetTransformed(Transform3D transform3D)
        {
            if (transform3D == null)
            {
                return null;
            }

            return Query.Transform(this, transform3D);
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

        public bool Coplanar(Plane plane, double tolerance = Tolerance.Distance)
        {
            if (plane == null)
                return false;

            return normal.AlmostEqual(plane.normal, tolerance) || normal.AlmostEqual(-plane.normal, tolerance);
        }

        public void Reverse()
        {
            normal.Negate();
            axisY.Negate();
        }

        public override bool FromJsonObject(JsonObject jsonObject)
        {
            if (jsonObject == null)
                return false;

            if (jsonObject["Origin"] is JsonObject jsonObject_Origin)
                origin = new Point3D((JsonObject)jsonObject_Origin.DeepClone());

            if (jsonObject["Normal"] is JsonObject jsonObject_Normal)
                normal = new Vector3D((JsonObject)jsonObject_Normal.DeepClone());

            if (jsonObject["AxisY"] is JsonObject jsonObject_AxisY)
                axisY = new Vector3D((JsonObject)jsonObject_AxisY.DeepClone());
            else
                axisY = normal?.AxisY();

            return true;
        }

        public override JsonObject ToJsonObject()
        {
            JsonObject jsonObject = base.ToJsonObject();
            if (jsonObject == null)
                return null;

            if (origin?.ToJsonObject() is JsonObject originJson)
                jsonObject["Origin"] = originJson.DeepClone();

            if (normal?.ToJsonObject() is JsonObject normalJson)
                jsonObject["Normal"] = normalJson.DeepClone();

            if (axisY?.ToJsonObject() is JsonObject axisYJson)
                jsonObject["AxisY"] = axisYJson.DeepClone();

            return jsonObject;
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

        public override bool Equals(object obj)
        {
            Plane plane = obj as Plane;
            if (plane == null)
            {
                return false;
            }

            return plane.normal == normal && plane.origin == origin && plane.axisY == axisY;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = (hash * 23) + (normal != null ? normal.GetHashCode() : 0);
                hash = (hash * 23) + (origin != null ? origin.GetHashCode() : 0);
                hash = (hash * 23) + (axisY != null ? axisY.GetHashCode() : 0);
                return hash;
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
