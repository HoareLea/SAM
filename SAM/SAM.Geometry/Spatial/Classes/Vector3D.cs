using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Geometry.Spatial
{
    public class Vector3D : SAMGeometry, IBoundable3D
    {
        public static Vector3D WorldX { get; } = new Vector3D(1, 0, 0);

        public static Vector3D WorldY { get; } = new Vector3D(0, 1, 0);

        public static Vector3D WorldZ { get; } = new Vector3D(0, 0, 1);

        private double[] coordinates = new double[] { 0, 0, 0 };

        public Vector3D(double x, double y, double z)
        {
            coordinates = new double[] { x, y, z };
        }

        public Vector3D(Point3D start, Point3D end)
        {
            coordinates = new double[] { end[0] - start[0], end[1] - start[1], end[2] - start[2] };
        }

        public Vector3D()
        {
            coordinates = new double[] { 0, 0, 0 };
        }

        public Vector3D(JObject jObject)
        {
            FromJObject(jObject);
        }

        public Vector3D(Vector3D vector3D)
        {
            coordinates[0] = vector3D.coordinates[0];
            coordinates[1] = vector3D.coordinates[1];
            coordinates[2] = vector3D.coordinates[2];
        }

        public Vector3D(double[] coordinates)
        {
            this.coordinates[0] = coordinates[0];
            this.coordinates[1] = coordinates[1];
            this.coordinates[2] = coordinates[2];
        }

        public Vector3D(IEnumerable<double> coordinates)
        {
            this.coordinates[0] = coordinates.ElementAt(0);
            this.coordinates[1] = coordinates.ElementAt(1);
            this.coordinates[2] = coordinates.ElementAt(2);
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

        public void Add(Vector3D vector3D)
        {
            coordinates[0] += vector3D.coordinates[0];
            coordinates[1] += vector3D.coordinates[1];
            coordinates[2] += vector3D.coordinates[2];
        }

        public void Scale(double value)
        {
            coordinates[0] = coordinates[0] * value;
            coordinates[1] = coordinates[1] * value;
            coordinates[2] = coordinates[2] * value;
        }

        public void Normalize()
        {
            double aLength = Length;
            coordinates[0] = coordinates[0] / aLength;
            coordinates[1] = coordinates[1] / aLength;
            coordinates[2] = coordinates[2] / aLength;
        }

        public bool Collinear(Vector3D vector3D, double tolerance = Core.Tolerance.Distance)
        {
            if(vector3D == null)
                return false;

            return System.Math.Abs(System.Math.Abs(this * vector3D) - (Length * vector3D.Length)) <= tolerance;
        }

        public bool SameHalf(Vector3D vector3D)
        {
            if (vector3D == null)
                return false;

            Vector3D vector3D_Temp = new Vector3D(vector3D);
            vector3D_Temp.Negate();

            return Angle(vector3D) <= Angle(vector3D_Temp);
        }

        public bool InRange(Vector3D direction, double angleDifference)
        {
            if (direction == null || double.IsNaN(angleDifference))
                return false;

            return Angle(direction) < angleDifference;
        }

        public double Length
        {
            get
            {
                return System.Math.Sqrt((coordinates[0] * coordinates[0]) + (coordinates[1] * coordinates[1]) + (coordinates[2] * coordinates[2]));
            }
            set
            {
                Vector3D vector3D = Unit;
                vector3D.Scale(value);
                coordinates = new double[] { vector3D[0], vector3D[1], vector3D[2] };
            }
        }

        public Vector3D Unit
        {
            get
            {
                double aLength = Length;
                return new Vector3D(coordinates[0] / aLength, coordinates[1] / aLength, coordinates[2] / aLength);
            }
        }

        /// <summary> The cross product takes two vectors and produces a third vector that is
        /// orthogonal to both. For example, if you have two vectors lying on the World xy-plane,
        /// then their cross product is a vector perpendicular to the xy-plane going either in the
        /// positive or negative World z-axis direction. Sample: a​<a1, a2, a3> and ​b​<b1, b2, b3>
        /// then a​ ​×​ ​b ​=​ ​<​a2​ ​*​ ​b3 – a3​ ​*​ ​b2​,​ ​a3​ ​*​ ​b1 - a1​ ​*​ ​b3, a1​ ​*​
        /// ​b2 - a2​ ​*​ ​b1​ ​> </summary> <returns> Cross Product Vector3D </returns> <param
        /// name="vector3D">A Vector3D</param>
        public Vector3D CrossProduct(Vector3D vector3D)
        {
            return new Vector3D((coordinates[1] * vector3D.coordinates[2]) - (coordinates[2] * vector3D.coordinates[1]), (coordinates[2] * vector3D.coordinates[0]) - (coordinates[0] * vector3D.coordinates[2]), (coordinates[0] * vector3D.coordinates[1]) - (coordinates[1] * vector3D.coordinates[0]));
        }

        public double DotProduct(Vector3D vector3D)
        {
            return (coordinates[0] * vector3D.coordinates[0]) + (coordinates[1] * vector3D.coordinates[1]) + (coordinates[2] * vector3D.coordinates[2]);
        }

        public void Negate()
        {
            coordinates[0] = -coordinates[0];
            coordinates[1] = -coordinates[1];
            coordinates[2] = -coordinates[2];
        }

        public Vector3D GetNegated()
        {
            return new Vector3D(-coordinates[0], -coordinates[1], -coordinates[2]);
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

        public override string ToString()
        {
            return string.Format("{0}(X={1},Y={2},Z={3})", GetType().Name, coordinates[0], coordinates[1], coordinates[2]);
        }

        public override ISAMGeometry Clone()
        {
            return new Vector3D(this);
        }

        //Calculate the dot product as an angle
        //Source: https://wiki.unity3d.com/index.php/3d_Math_functions
        public double Angle(Vector3D vector3D)
        {
            double dotProduct = DotProduct(vector3D);
            double length = Length * vector3D.Length;

            return (System.Math.Abs(dotProduct) < length) ? System.Math.Acos(dotProduct / length) : (dotProduct < 0) ? System.Math.PI : 0;

            //Version 2

            ////get the dot product
            //double dotProduct = DotProduct(vector3D);

            ////Clamp to prevent NaN error. Shouldn't need this in the first place, but there could be a rounding error issue.
            //if (dotProduct < -1)
            //    dotProduct = -1;
            //else if (dotProduct > 1)
            //    dotProduct = 1;

            ////Calculate the angle. The output is in radians
            //return System.Math.Acos(dotProduct);


            //Version 1

            //double value = System.Math.Acos(DotProduct(vector3D) / (Length * vector3D.Length));
            //if (double.IsNaN(value))
            //    return 0;

            //return value;
        }

        //Calculate the dot product of plane normal as an angle
        //Source: https://wiki.unity3d.com/index.php/3d_Math_functions
        public double Angle(Plane plane)
        {
            return (System.Math.PI / 2) - Angle(plane.Normal);
        }

        public Point3D Project(Point3D point3D)
        {
            if (point3D == null)
                return null;

            return (this * (DotProduct(point3D.ToVector3D()))).ToPoint3D();
        }

        public Point3D ToPoint3D()
        {
            return new Point3D(coordinates[0], coordinates[1], coordinates[2]);
        }

        public Math.Matrix GetArgumentedMatrix()
        {
            return new Math.Matrix(new double[,] { { coordinates[0] }, { coordinates[1] }, { coordinates[2] }, { 0 } });
        }

        public double SmallestAngle(Vector3D vector3D)
        {
            double value = System.Math.Abs(Angle(vector3D));
            if (value == 0)
                return value;

            return System.Math.PI - value;
        }

        public BoundingBox3D GetBoundingBox(double offset = 0)
        {
            return new BoundingBox3D(Point3D.Zero, new Point3D(coordinates[0], coordinates[1], coordinates[2]), offset);
        }

        public ISAMGeometry3D GetMoved(Vector3D vector3D)
        {
            return new Vector3D(coordinates[0] + vector3D.coordinates[0], coordinates[1] + vector3D.coordinates[1], coordinates[2] + vector3D.coordinates[2]);
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
            if ((@object == null) || !(@object is Vector3D))
                return false;

            Vector3D vector3D = (Vector3D)@object;
            return coordinates[0] == vector3D.coordinates[0] && coordinates[1] == vector3D.coordinates[1] && coordinates[2] == vector3D.coordinates[2];
        }

        public bool AlmostEqual(Vector3D vector3D, double tolerance = Core.Tolerance.Distance)
        {
            if (this == vector3D)
                return true;

            if (vector3D == null)
                return false;

            double x_1 = Core.Query.Round(vector3D.coordinates[0], tolerance);
            double y_1 = Core.Query.Round(vector3D.coordinates[1], tolerance);
            double z_1 = Core.Query.Round(vector3D.coordinates[2], tolerance);

            double x_2 = Core.Query.Round(coordinates[0], tolerance);
            double y_2 = Core.Query.Round(coordinates[1], tolerance);
            double z_2 = Core.Query.Round(coordinates[2], tolerance);

            return System.Math.Abs(x_1 - x_2) < tolerance && System.Math.Abs(y_1 - y_2) < tolerance && (System.Math.Abs(z_1 - z_2) < tolerance);
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
            if (decimals == Core.Rounding.NoRounding)
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

        public static Vector3D operator +(Vector3D vector3D_1, Vector3D vector3D_2)
        {
            return new Vector3D(vector3D_1.coordinates[0] + vector3D_2.coordinates[0], vector3D_1.coordinates[1] + vector3D_2.coordinates[1], vector3D_1.coordinates[2] + vector3D_2.coordinates[2]);
        }

        public static Vector3D operator -(Vector3D vector3D_1, Vector3D vector3D_2)
        {
            return new Vector3D(vector3D_1.coordinates[0] - vector3D_2.coordinates[0], vector3D_1.coordinates[1] - vector3D_2.coordinates[1], vector3D_1.coordinates[2] - vector3D_2.coordinates[2]);
        }

        public static Vector3D operator -(Vector3D vector3D)
        {
            Vector3D result = new Vector3D(vector3D);
            result.Negate();
            return result;
        }

        public static double operator *(Vector3D vector3D_1, Vector3D vector3D_2)
        {
            return vector3D_1.coordinates[0] * vector3D_2.coordinates[0] + vector3D_1.coordinates[1] * vector3D_2.coordinates[1] + vector3D_1.coordinates[2] * vector3D_2.coordinates[2];
        }

        public static Vector3D operator *(Vector3D vector3D, double factor)
        {
            return new Vector3D(vector3D.coordinates[0] * factor, vector3D.coordinates[1] * factor, vector3D.coordinates[2] * factor);
        }

        public static Vector3D operator /(Vector3D vector3D, double factor)
        {
            return new Vector3D(vector3D.coordinates[0] / factor, vector3D.coordinates[1] / factor, vector3D.coordinates[2] / factor);
        }

        public static Vector3D operator *(double factor, Vector3D vector3D)
        {
            return new Vector3D(vector3D.coordinates[0] * factor, vector3D.coordinates[1] * factor, vector3D.coordinates[2] * factor);
        }

        public static bool operator ==(Vector3D vector3D_1, Vector3D vector3D_2)
        {
            if (ReferenceEquals(vector3D_1, null) && ReferenceEquals(vector3D_2, null))
                return true;

            if (ReferenceEquals(vector3D_1, null))
                return false;

            if (ReferenceEquals(vector3D_2, null))
                return false;

            return vector3D_1?.coordinates[0] == vector3D_2?.coordinates[0] && vector3D_1?.coordinates[1] == vector3D_2?.coordinates[1] && vector3D_1?.coordinates[2] == vector3D_2?.coordinates[2];
        }

        public static bool operator !=(Vector3D vector3D_1, Vector3D vector3D_2)
        {
            if (ReferenceEquals(vector3D_1, null) && ReferenceEquals(vector3D_2, null))
                return false;

            if (ReferenceEquals(vector3D_1, null))
                return true;

            if (ReferenceEquals(vector3D_2, null))
                return true;

            return vector3D_1?.coordinates[0] != vector3D_2?.coordinates[0] || vector3D_1?.coordinates[1] != vector3D_2?.coordinates[1] || vector3D_1?.coordinates[2] != vector3D_2?.coordinates[2];
        }
    }
}