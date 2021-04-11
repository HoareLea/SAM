using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Geometry.Spatial
{
    public class Segment3D : SAMGeometry, ICurve3D, ISegmentable3D
    {
        private Point3D origin;
        private Vector3D vector;

        public Segment3D(Point3D start, Point3D end)
        {
            origin = new Point3D(start);
            vector = new Vector3D(start, end);
        }

        public Segment3D(Segment3D segment3D)
        {
            origin = new Point3D(segment3D.origin);
            vector = new Vector3D(segment3D.vector);
        }

        public Segment3D(Point3D origin, Vector3D vector)
        {
            this.origin = new Point3D(origin);
            this.vector = new Vector3D(vector);
        }

        public Segment3D(JObject jObject)
            : base(jObject)
        {
        }

        public Point3D this[int index]
        {
            get
            {
                if (index == 0)
                    return origin;
                if (index == 1)
                    return (Point3D)origin.GetMoved(vector);

                return null;
            }
            set
            {
                if (index == 0)
                    origin = value;
                else if (index == 1)
                    vector = new Vector3D(origin, value);
            }
        }

        public Point3D GetStart()
        {
            return new Point3D(origin);
        }

        public Point3D GetEnd()
        {
            return (Point3D)origin.GetMoved(vector);
        }

        public Vector3D Direction
        {
            get
            {
                return vector.Unit;
            }
        }

        public List<Point3D> GetPoints()
        {
            return new List<Point3D>() { origin, this[1] };
        }

        public override ISAMGeometry Clone()
        {
            return new Segment3D(this);
        }

        public List<Segment3D> GetSegments()
        {
            return new List<Segment3D>() { new Segment3D(this) };
        }

        public List<ICurve3D> GetCurves()
        {
            return new List<ICurve3D>() { new Segment3D(this) };
        }

        public BoundingBox3D GetBoundingBox(double offset = 0)
        {
            return new BoundingBox3D(new Point3D(origin), this[1], offset);
        }

        public void Reverse()
        {
            origin = GetEnd();
            vector.Negate();
        }

        public ISAMGeometry3D GetMoved(Vector3D vector3D)
        {
            return new Segment3D((Point3D)origin.GetMoved(vector3D), (Vector3D)vector.Clone());
        }

        public Point3D GetCenter()
        {
            return (Point3D)origin.GetMoved(vector / 2);
        }

        public override bool FromJObject(JObject jObject)
        {
            origin = new Point3D(jObject.Value<JObject>("Origin"));
            vector = new Vector3D(jObject.Value<JObject>("Vector"));

            return true;
        }

        public override JObject ToJObject()
        {
            JObject jObject = base.ToJObject();
            if (jObject == null)
                return null;

            jObject.Add("Origin", origin.ToJObject());
            jObject.Add("Vector", vector.ToJObject());

            return jObject;
        }

        public double GetLength()
        {
            return vector.Length;
        }

        public Point3D Project(Point3D point3D)
        {
            return Closest(point3D, false);
        }

        public Point3D Closest(Point3D point3D, bool bounded = true)
        {
            Point3D start = GetStart();
            Point3D end = GetEnd();

            double a = point3D.X - start.X;
            double b = point3D.Y - start.Y;
            double e = point3D.Z - start.Z;
            double c = end.X - start.X;
            double d = end.Y - start.Y;
            double f = end.Z - start.Z;

            double dot = a * c + b * d + e * f;
            double squareLength = c * c + d * d + f * f;
            double parameter = -1;
            if (squareLength != 0)
                parameter = dot / squareLength;

            if (parameter < 0 && bounded)
                return start;
            else if (parameter > 1 && bounded)
                return end;
            else
                return new Point3D(start.X + parameter * c, start.Y + parameter * d, start.Z + parameter * f);
        }

        public double Distance(Point3D point3D)
        {
            if (point3D == null)
                return double.NaN;

            return point3D.Distance(Closest(point3D));
        }

        //public double Distance(Segment3D segment3D, double tolerance = Core.Tolerance.Distance)
        //{
        //    if (segment3D == null)
        //        return double.NaN;

        //    Vector3D normal = Direction.CrossProduct(segment3D.Direction);
        //    double length = normal.Length;
        //    if(double.IsNaN(length) || normal.Length < tolerance)
        //    {
        //        normal = Query.Normal(origin, GetEnd(), segment3D.origin);
        //        length = normal.Length;
        //        if (double.IsNaN(length) || normal.Length < tolerance)
        //        {
        //            normal = Direction.Rotate90();
        //        }
        //    }

        //    Plane plane = new Plane(origin, normal);
        //    Planar.Segment2D segment2D_1 = plane.Convert(this);
        //    Planar.Segment2D segment2D_2 = plane.Convert(segment3D);
        //    return segment2D_1.Distance(segment2D_2);
        //}

        public double Distance(Segment3D segment3D, double tolerance = Core.Tolerance.Distance)
        {
            if (segment3D == null)
                return double.NaN;

            Point3D point3D_Closest_1 = null;
            Point3D point3D_Closest_2 = null;

            Point3D point3D_Intersection = Intersection(segment3D, out point3D_Closest_1, out point3D_Closest_2, tolerance);

            if (point3D_Intersection == null)
            {
                //Paraller segments
                Segment3D segment3D_Temp = segment3D;
                point3D_Closest_1 = Project(segment3D.origin);
                if (!On(point3D_Closest_1, tolerance))
                {
                    point3D_Closest_1 = Project(segment3D.GetEnd());
                    if (!On(point3D_Closest_1, tolerance))
                    {
                        segment3D_Temp = this;
                        point3D_Closest_1 = segment3D.Project(origin);
                        if (!segment3D.On(point3D_Closest_1, tolerance))
                        {
                            point3D_Closest_1 = segment3D.Project(GetEnd());
                            if (!segment3D.On(point3D_Closest_1, tolerance))
                                return (new double[] { segment3D.origin.Distance(origin), segment3D.GetEnd().Distance(GetEnd()), segment3D.origin.Distance(GetEnd()), segment3D.GetEnd().Distance(origin) }).Min();
                        }
                    }
                }

                point3D_Closest_2 = segment3D_Temp.Project(point3D_Closest_1);
            }
            else if (point3D_Closest_1 == null || point3D_Closest_2 == null)
            {
                return 0;
            }

            return Math.Query.Min(Distance(segment3D[0]), Distance(segment3D[1]), segment3D.Distance(origin), segment3D.Distance(GetEnd()));

            //return point2D_Closest_1.Distance(point2D_Closest_2);
        }

        public Point3D Intersection(Segment3D segment3D, out Point3D point3D_Closest1, out Point3D point3D_Closest2, double tolerance = Core.Tolerance.Distance)
        {
            point3D_Closest1 = null;
            point3D_Closest2 = null;

            Vector3D normal = Direction.CrossProduct(segment3D.Direction);
            double length = normal.Length;
            if (double.IsNaN(length) || normal.Length < tolerance)
            {
                normal = Query.Normal(origin, GetEnd(), segment3D.origin);
                length = normal.Length;
                if (double.IsNaN(length) || normal.Length < tolerance)
                {
                    normal = Direction.Rotate90();
                }

                Plane plane = new Plane(origin, normal);
                Planar.Segment2D segment2D_1 = plane.Convert(this);
                Planar.Segment2D segment2D_2 = plane.Convert(segment3D);

                Planar.Point2D point2D_Intersection = segment2D_1.Intersection(segment2D_2, out Planar.Point2D point2D_Closest_1, out Planar.Point2D point2D_Closest_2, tolerance);
                if (point2D_Closest_1 != null)
                {
                    point3D_Closest1 = plane.Convert(point2D_Closest_1);
                }

                if (point2D_Closest_2 != null)
                {
                    point3D_Closest2 = plane.Convert(point2D_Closest_2);
                }

                return plane.Convert(point2D_Intersection);
            }

            var point0 = origin;
            var u = Direction;
            var point1 = segment3D.origin;
            var v = segment3D.Direction;

            var w0 = point0 - point1;
            var a = u.DotProduct(u);
            var b = u.DotProduct(v);
            var c = v.DotProduct(v);
            var d = u.DotProduct(w0);
            var e = v.DotProduct(w0);

            var sc = ((b * e) - (c * d)) / ((a * c) - (b * b));
            var tc = ((a * e) - (b * d)) / ((a * c) - (b * b));

            point3D_Closest1 = point0.GetMoved(sc * u) as Point3D;
            point3D_Closest2 = point1.GetMoved(tc * v) as Point3D;

            if(!On(point3D_Closest1, tolerance))
            {
                point3D_Closest1 = Closest(point3D_Closest1);
            }

            if (!segment3D.On(point3D_Closest2, tolerance))
            {
                point3D_Closest2 = segment3D.Closest(point3D_Closest2);
            }

            if (!point3D_Closest1.AlmostEquals(point3D_Closest2, tolerance))
                return null;

            Point3D result = point3D_Closest1;
            point3D_Closest1 = null;
            point3D_Closest2 = null;

            return result;
        }

        public Point3D Intersection(Segment3D segment3D, bool bounded = true, double tolerance = Core.Tolerance.Distance)
        {
            if (segment3D == null)
                return null;

            Point3D result = Query.Intersection(origin, vector.GetNormalized(), segment3D.origin, segment3D.vector.GetNormalized(), tolerance);
            if(result == null)
            {
                return result;
            }

            if(!bounded)
            {
                return result;
            }

            if (segment3D.On(result, tolerance) && On(result, tolerance))
            {
                return result;
            }

            return null;
        }

        public bool On(Point3D point3D, double tolerance = Core.Tolerance.Distance)
        {
            return Closest(point3D).Distance(point3D) < tolerance;
        }

        public Line3D GetLine3D()
        {
            return new Line3D(origin, vector);
        }

        public static explicit operator Segment3D(Line3D line3D)
        {
            if (line3D == null)
                return null;

            return new Segment3D(line3D.Origin, line3D.Direction);
        }

        public static List<Point3D> GetPoints(IEnumerable<Segment3D> segment3Ds, bool close = false)
        {
            if (segment3Ds == null)
                return null;

            List<Point3D> result = new List<Point3D>() { segment3Ds.First().GetStart() };
            foreach (Segment3D segment3D in segment3Ds)
                result.Add(segment3D.GetEnd());

            if (close && result.First().Distance(result.Last()) != 0)
                result.Add(result.First());

            return result;
        }

        public static Segment3D Snap(IEnumerable<Point3D> point3Ds, Segment3D segment3D, double maxDistance = double.NaN)
        {
            Point3D point3D_1 = Point3D.Snap(point3Ds, segment3D[0], maxDistance);
            Point3D point3D_2 = Point3D.Snap(point3Ds, segment3D[1], maxDistance);

            return new Segment3D(point3D_1, point3D_2);
        }

        public static bool operator ==(Segment3D segment3D_1, Segment3D segment3D_2)
        {
            if (ReferenceEquals(segment3D_1, null) && ReferenceEquals(segment3D_2, null))
                return true;

            if (ReferenceEquals(segment3D_1, null))
                return false;

            if (ReferenceEquals(segment3D_2, null))
                return false;

            return segment3D_1.origin == segment3D_2.origin && segment3D_1.vector == segment3D_2.vector;
        }

        public static bool operator !=(Segment3D segment3D_1, Segment3D segment3D_2)
        {
            if (ReferenceEquals(segment3D_1, null) && ReferenceEquals(segment3D_2, null))
                return false;

            if (ReferenceEquals(segment3D_1, null))
                return true;

            if (ReferenceEquals(segment3D_2, null))
                return true;

            return segment3D_1.origin != segment3D_2.origin || segment3D_1.vector != segment3D_2.vector;
        }
    }
}