using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Geometry.Spatial
{
    public class Point3D : SAMGeometry, ISAMGeometry3D
    {
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
            if (point3D == null)
            {
                coordinates[0] = double.NaN;
                coordinates[1] = double.NaN;
                coordinates[2] = double.NaN;
            }
            else
            {
                coordinates[0] = point3D.coordinates[0];
                coordinates[1] = point3D.coordinates[1];
                coordinates[2] = point3D.coordinates[2];
            }
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
                return new Vector3D(coordinates[0], coordinates[1], coordinates[2]);
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

            double value = vector3D_1.DotProduct(vector3D_2) / (vector3D_1.Length * vector3D_2.Length);

            double result =  System.Math.Acos(value);
            if (!double.IsNaN(result))
                return result;

            return value > 0 ? 0 : System.Math.PI;
        }

        public bool IsValid()
        {
            return !double.IsNaN(coordinates[0]) && !double.IsNaN(coordinates[1]) && !double.IsNaN(coordinates[2]);
        }

        public bool IsZero()
        {
            return coordinates[0] == 0 && coordinates[1] == 0 && coordinates[2] == 0;
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

        public static Point3D Zero 
        {
            get
            {
                return new Point3D(0, 0, 0);
            }
        }

        public ISAMGeometry3D GetTransformed(Transform3D transform3D)
        {
            if (transform3D == null)
            {
                return null;
            }

            return Query.Transform(this, transform3D);
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

        public static Point3D operator -(Point3D point3D, Vector3D vector3D)
        {
            return new Point3D(point3D.coordinates[0] - vector3D[0], point3D.coordinates[1] - vector3D[1], point3D.coordinates[2] - vector3D[2]);
        }
    }
}