using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Geometry.Spatial
{
    public class Point3D : SAMGeometry, ISAMGeometry3D
    {
        public static Point3D Zero { get; } = new Point3D(0, 0, 0);

        private double[] coordinates = new double[3] { 0, 0, 0 };

        public Point3D()
        {
            coordinates = new double[3] { 0, 0, 0 };
        }

        public Point3D(JObject jObject)
        {
            FromJObject(jObject);
        }

        public Point3D(double x, double y, double z)
        {
            coordinates = new double[3] { x, y, z };
        }

        public Point3D(Point3D point3D)
        {
            coordinates = new double[3] { point3D[0], point3D[1], point3D[2] };
        }

        public double this[int index]
        {
            get
            {
                return coordinates[index];
            }
            set
            {
                coordinates[index] = value;
            }
        }

        public double X
        {
            get
            {
                return coordinates[0];
            }
            set
            {
                if (coordinates == null)
                    coordinates = new double[3];

                coordinates[0] = value;
            }
        }

        public double Y
        {
            get
            {
                return coordinates[1];
            }
            set
            {
                if (coordinates == null)
                    coordinates = new double[3];

                coordinates[1] = value;
            }
        }

        public double Z
        {
            get
            {
                return coordinates[2];
            }
            set
            {
                if (coordinates == null)
                    coordinates = new double[3];

                coordinates[2] = value;
            }
        }

        public Vector3D ToVector3D(Point3D point3D = null)
        {
            if (point3D == null)
                return new Vector3D(coordinates);
            else
                return new Vector3D(coordinates[0] - point3D[0], coordinates[1] - point3D[1], coordinates[2] - point3D[2]);
        }

        public Math.Matrix GetArgumentedMatrix()
        {
            return new Math.Matrix(new double[,] { { coordinates[0] }, { coordinates[1] }, { coordinates[2] }, { 1 } });
        }

        public double Distance(Point3D point3D)
        {
            return new Vector3D(this, point3D).Length;
        }

        public ISAMGeometry3D GetMoved(Vector3D vector3D)
        {
            return new Point3D(coordinates[0] + vector3D[0], coordinates[1] + vector3D[1], coordinates[2] + vector3D[2]);
        }

        public double Angle(Point3D point3D_1, Point3D point3D_2)
        {
            Vector3D vector3D_1 = new Vector3D(this, point3D_1);
            Vector3D vector3D_2 = new Vector3D(this, point3D_2);

            return System.Math.Acos(vector3D_1.DotProduct(vector3D_2) / (vector3D_1.Length * vector3D_2.Length));
        }

        public double SmallestAngle(Point3D point3D_1, Point3D point3D_2)
        {
            return System.Math.PI - System.Math.Abs(Angle(point3D_1, point3D_2));
        }

        public void Move(Vector3D vector3D)
        {
            coordinates[0] = coordinates[0] + vector3D[0];
            coordinates[1] = coordinates[1] + vector3D[1];
            coordinates[2] = coordinates[2] + vector3D[2];
        }

        public override string ToString()
        {
            return string.Format("{0}(X={1},Y={2},Z={3})", GetType().Name, coordinates[0], coordinates[1], coordinates[2]);
        }

        public override ISAMGeometry Clone()
        {
            return new Point3D(this);
        }

        public BoundingBox3D GetBoundingBox(double offset)
        {
            return new BoundingBox3D(this, offset);
        }

        public override bool FromJObject(JObject jObject)
        {
            coordinates[0] = jObject.Value<double>("X");
            coordinates[1] = jObject.Value<double>("Y");
            coordinates[2] = jObject.Value<double>("Z");

            return true;
        }

        public override JObject ToJObject()
        {
            JObject jObject = base.ToJObject();
            if (jObject == null)
                return null;

            jObject.Add("X", coordinates[0]);
            jObject.Add("Y", coordinates[1]);
            jObject.Add("Z", coordinates[2]);
            return jObject;
        }

        public override bool Equals(object @object)
        {
            if ((@object == null) || !(@object is Point3D))
                return false;

            Point3D point3D = (Point3D)@object;
            return coordinates[0] == point3D.coordinates[0] && coordinates[1] == point3D.coordinates[1] && coordinates[2] == point3D.coordinates[2];
        }

        public override int GetHashCode()
        {
            int hash = 13;
            hash = (hash * 7) + coordinates[0].GetHashCode();
            hash = (hash * 7) + coordinates[1].GetHashCode();
            hash = (hash * 7) + coordinates[2].GetHashCode();
            return hash;
        }

        public void Round(int decimals = Core.Rounding.Distance)
        {
            if (decimals == -1)
                return;

            coordinates[0] = System.Math.Round(coordinates[0], decimals);
            coordinates[1] = System.Math.Round(coordinates[1], decimals);
            coordinates[2] = System.Math.Round(coordinates[2], decimals);
        }

        public void Round(double tolerance = Core.Tolerance.Distance)
        {
            coordinates[0] = Core.Query.Round(coordinates[0], tolerance);
            coordinates[1] = Core.Query.Round(coordinates[1], tolerance);
            coordinates[2] = Core.Query.Round(coordinates[2], tolerance);
        }

        public bool AlmostEquals(Point3D point3D, double tolerance = Core.Tolerance.Distance)
        {
            return ((System.Math.Abs(coordinates[0] - point3D.coordinates[0]) < tolerance) && (System.Math.Abs(coordinates[1] - point3D.coordinates[1]) < tolerance) && (System.Math.Abs(coordinates[2] - point3D.coordinates[2]) < tolerance));
        }

        public static Point3D Snap(IEnumerable<Point3D> point3Ds, Point3D point3D, double maxDistance = double.NaN)
        {
            Point3D result = Closest(point3Ds, point3D);

            if (point3D.Distance(result) > maxDistance)
                result = new Point3D(point3D);

            return result;
        }

        public static List<Segment3D> GetSegments(IEnumerable<Point3D> point3Ds, bool close = false)
        {
            if (point3Ds == null)
                return null;

            List<Segment3D> result = new List<Segment3D>();
            if (point3Ds.Count() < 2)
                return result;

            int aCount = point3Ds.Count();

            for (int i = 0; i < aCount - 1; i++)
                result.Add(new Segment3D(point3Ds.ElementAt(i), point3Ds.ElementAt(i + 1)));

            if (close)
                result.Add(new Segment3D(point3Ds.Last(), point3Ds.First()));

            return result;
        }

        public static List<Point3D> SimplifyByAngle(IEnumerable<Point3D> point3Ds, bool close = false, double tolerane = Core.Tolerance.Angle)
        {
            if (point3Ds == null)
                return null;

            List<Point3D> result = new List<Point3D>(point3Ds);

            int start = 0;
            int end = close ? result.Count : result.Count - 2;
            while (start < end)
            {
                Point3D first = result[start];
                Point3D second = result[(start + 1) % result.Count];
                Point3D third = result[(start + 2) % result.Count];

                if (second.SmallestAngle(first, third) <= tolerane)
                {
                    result.RemoveAt((start + 1) % result.Count);
                    end--;
                }
                else
                {
                    start++;
                }
            }
            return result;
        }

        public static List<Point3D> SimplifyByDistance(IEnumerable<Point3D> point3Ds, bool close = false, double tolerane = Core.Tolerance.Distance)
        {
            if (point3Ds == null)
                return null;

            List<Point3D> result = new List<Point3D>(point3Ds);
            Point3D last = result.Last();

            int start = 0;
            int end = close ? result.Count : result.Count - 1;
            while (start < end)
            {
                Point3D first = result[start];
                Point3D second = result[(start + 1) % result.Count];

                if (first.Distance(second) <= tolerane)
                {
                    result.RemoveAt((start + 1) % result.Count);
                    end--;
                }
                else
                    start++;
            }

            if (!close)
            {
                result.Remove(result.Last());
                result.Add(last);
            }
            //else
            //{
            //    result.Add(result.First());
            //}

            while (result.Last().Distance(result[result.Count() - 2]) < tolerane)
            {
                result.RemoveAt(result.Count - 2);
            }

            return result;
        }

        public static List<Point3D> Clone(IEnumerable<Point3D> point3Ds)
        {
            List<Point3D> result = new List<Point3D>();
            foreach (Point3D point3D in point3Ds)
                result.Add(new Point3D(point3D));

            return result;
        }

        public static Point3D Closest(IEnumerable<Point3D> point3Ds, Point3D point3D)
        {
            if (point3Ds == null || point3Ds.Count() == 0 || point3D == null)
                return null;

            Point3D result = null;
            double distance = double.MaxValue;
            foreach (Point3D point3D_Temp in point3Ds)
            {
                double distance_Temp = point3D_Temp.Distance(point3D);

                if (distance > distance_Temp)
                {
                    distance = distance_Temp;
                    result = point3D_Temp;
                }

                if (distance == 0)
                    return result;
            }

            return result;
        }

        public static Point3D Max(Point3D point3D_1, Point3D point3D_2)
        {
            return new Point3D(System.Math.Max(point3D_1.X, point3D_2.X), System.Math.Max(point3D_1.Y, point3D_2.Y), System.Math.Max(point3D_1.Z, point3D_2.Z));
        }

        public static Point3D Max(IEnumerable<Point3D> point3Ds)
        {
            if (point3Ds == null || point3Ds.Count() == 0)
                return null;

            double aX = double.MinValue;
            double aY = double.MinValue;
            double aZ = double.MinValue;
            foreach (Point3D point3D in point3Ds)
            {
                if (aX < point3D.X)
                    aX = point3D.X;
                if (aY < point3D.Y)
                    aY = point3D.Y;
                if (aZ < point3D.Y)
                    aZ = point3D.Y;
            }

            return new Point3D(aX, aY, aZ);
        }

        public static Point3D Min(Point3D point3D_1, Point3D point3D_2)
        {
            return new Point3D(System.Math.Min(point3D_1.X, point3D_2.X), System.Math.Min(point3D_1.Y, point3D_2.Y), System.Math.Min(point3D_1.Z, point3D_2.Z));
        }

        public static Point3D Min(IEnumerable<Point3D> point3Ds)
        {
            if (point3Ds == null || point3Ds.Count() == 0)
                return null;

            double x = double.MaxValue;
            double y = double.MaxValue;
            double z = double.MaxValue;
            foreach (Point3D point3D in point3Ds)
            {
                if (x > point3D.X)
                    x = point3D.X;
                if (y > point3D.Y)
                    y = point3D.Y;
                if (z > point3D.Z)
                    z = point3D.Z;
            }

            return new Point3D(x, y, z);
        }

        public static List<Point3D> Generate(BoundingBox3D boundingBox3D, double offset)
        {
            List<Point3D> result = new List<Point3D>();

            double width = boundingBox3D.Width;
            double height = boundingBox3D.Height;
            double depth = boundingBox3D.Depth;

            double distance_Width = 0;
            while (distance_Width <= width)
            {
                double distance_Height = 0;
                while (distance_Height <= height)
                {
                    double distance_Depth = 0;
                    while (distance_Depth <= depth)
                    {
                        result.Add(new Point3D(boundingBox3D.Min.X + distance_Width, boundingBox3D.Min.Y + distance_Depth, boundingBox3D.Min.Z + distance_Height));
                        distance_Depth += offset;
                    }
                    distance_Height += offset;
                }
                distance_Width += offset;
            }

            return result;
        }

        public static Point3D GetCenter(Point3D point3D_1, Point3D point3D_2)
        {
            return new Point3D((point3D_1.X + point3D_2.X) / 2, (point3D_1.Y + point3D_2.Y) / 2, (point3D_1.Z + point3D_2.Z) / 2);
        }

        public static bool Collinear(Point3D point3D_1, Point3D point3D_2, Point3D point3D_3, double tolerance = Core.Tolerance.Angle)
        {
            return new Vector3D(point3D_1, point3D_2).SmallestAngle(new Vector3D(point3D_1, point3D_3)) < tolerance;
        }

        public static bool operator ==(Point3D point3D_1, Point3D point3D_2)
        {
            if (ReferenceEquals(point3D_1, null) && ReferenceEquals(point3D_2, null))
                return true;

            if (ReferenceEquals(point3D_1, null))
                return false;

            if (ReferenceEquals(point3D_2, null))
                return false;

            return point3D_1?.coordinates[0] == point3D_2?.coordinates[0] && point3D_1?.coordinates[1] == point3D_2?.coordinates[1] && point3D_1?.coordinates[2] == point3D_2?.coordinates[2];
        }

        public static bool operator !=(Point3D point3D_1, Point3D point3D_2)
        {
            if (ReferenceEquals(point3D_1, null) && ReferenceEquals(point3D_2, null))
                return false;

            if (ReferenceEquals(point3D_1, null))
                return true;

            if (ReferenceEquals(point3D_2, null))
                return true;

            return point3D_1?.coordinates[0] != point3D_2?.coordinates[0] || point3D_1?.coordinates[1] != point3D_2?.coordinates[1] || point3D_1?.coordinates[2] != point3D_2?.coordinates[2];
        }

        public static Vector3D operator -(Point3D point3D_1, Point3D point3D_2)
        {
            return new Vector3D(point3D_1.coordinates[0] - point3D_2.coordinates[0], point3D_1.coordinates[1] - point3D_2.coordinates[1], point3D_1.coordinates[2] - point3D_2.coordinates[2]);
        }
    }
}