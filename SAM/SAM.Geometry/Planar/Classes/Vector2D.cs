using Newtonsoft.Json.Linq;

using SAM.Geometry.Interfaces;

namespace SAM.Geometry.Planar
{
    public class Vector2D : SAMGeometry, ISAMGeometry2D, IReversible
    {
        public static Vector2D WorldX { get; } = new Vector2D(1, 0);
        public static Vector2D WorldY { get; } = new Vector2D(0, 1);

        private double[] coordinates = new double[2] { 0, 0 };

        public Vector2D(double x, double y)
        {
            coordinates = new double[2] { x, y };
        }

        public Vector2D(Point2D start, Point2D end)
        {
            coordinates = new double[2] { end[0] - start[0], end[1] - start[1] };
        }

        public Vector2D()
        {
            coordinates = new double[2] { 0, 0 };
        }

        public Vector2D(Vector2D vector)
        {
            coordinates[0] = vector.coordinates[0];
            coordinates[1] = vector.coordinates[1];
        }

        public Vector2D(double[] coordinates)
        {
            coordinates[0] = this.coordinates[0];
            coordinates[1] = this.coordinates[1];
        }

        public Vector2D(JObject jObject)
            : base(jObject)
        {
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

        public void Add(Vector2D vector2D)
        {
            coordinates[0] += vector2D.coordinates[0];
            coordinates[1] += vector2D.coordinates[1];
        }

        public void Scale(double value)
        {
            coordinates[0] = coordinates[0] * value;
            coordinates[1] = coordinates[1] * value;
        }

        public double Length
        {
            get
            {
                return System.Math.Sqrt((coordinates[0] * coordinates[0]) + (coordinates[1] * coordinates[1]));
            }
            set
            {
                Vector2D vector2D = Unit;
                vector2D.Scale(value);
                coordinates = new double[2] { vector2D[0], vector2D[1] };
            }
        }

        public Vector2D Unit
        {
            get
            {
                double aLength = Length;
                return new Vector2D(coordinates[0] / aLength, coordinates[1] / aLength);
            }
        }

        public void Reverse()
        {
            Negate();
        }

        public void Negate()
        {
            coordinates[0] = -coordinates[0];
            coordinates[1] = -coordinates[1];
        }

        public Vector2D GetNegated()
        {
            return new Vector2D(-coordinates[0], -coordinates[1]);
        }

        public bool AlmostEqual(Vector2D vector2D, double tolerance = Core.Tolerance.Distance)
        {
            return ((System.Math.Abs(coordinates[0] - vector2D.coordinates[0]) <= tolerance) && (System.Math.Abs(coordinates[1] - vector2D.coordinates[1]) <= tolerance));
        }

        public override bool Equals(object obj)
        {
            if (!(obj is Vector2D))
                return false;

            Vector2D point2D = (Vector2D)obj;
            return point2D.coordinates[0].Equals(coordinates[0]) && point2D.coordinates[1].Equals(coordinates[1]);
        }

        public override int GetHashCode()
        {
            int hash = 13;
            hash = (hash * 7) + coordinates[0].GetHashCode();
            hash = (hash * 7) + coordinates[1].GetHashCode();
            return hash;
        }

        public bool IsPerpedicular(Vector2D vector2D, double tolerance = Core.Tolerance.Distance)
        {
            return (vector2D.coordinates[0] * coordinates[0]) + (vector2D.coordinates[1] * coordinates[1]) <= tolerance;
        }

        public bool IsParallel(Vector2D vector2D, double tolerance = Core.Tolerance.Distance)
        {
            Vector2D vector2D_1 = vector2D.Unit;
            Vector2D vector2D_2 = Unit;

            return vector2D_1.AlmostEqual(vector2D_2, tolerance) || vector2D_1.AlmostEqual(vector2D_2.GetNegated(), tolerance);
        }

        public bool IsValid
        {
            get
            {
                return !double.IsNaN(coordinates[0]) && !double.IsNaN(coordinates[1]);
            }
        }

        public bool IsZero
        {
            get
            {
                return coordinates[0] == 0 && coordinates[1] == 0;
            }
        }

        public void Round(double tolerance = Core.Tolerance.Distance)
        {
            coordinates[0] = Core.Query.Round(coordinates[0], tolerance);
            coordinates[1] = Core.Query.Round(coordinates[1], tolerance);
        }

        public BoundingBox2D GetBoundingBox()
        {
            return new BoundingBox2D(Point2D.Zero, new Point2D(coordinates[0], coordinates[1]));
        }

        //Calculate the dot product as an angle
        //Source: https://wiki.unity3d.com/index.php/3d_Math_functions
        public double Angle(Vector2D vector2D)
        {
            //Get the dot product
            double dotProduct = Unit.DotProduct(vector2D.Unit);

            //Clamp to prevent NaN error. Shouldn't need this in the first place, but there could be a rounding error issue.
            if (dotProduct < -1)
                dotProduct = -1;
            else if (dotProduct > 1)
                dotProduct = 1;

            //Calculate the angle. The output is in radians
            return System.Math.Acos(dotProduct);

            //double result = System.Math.Acos(DotProduct(vector2D) / (Length * vector2D.Length));
            //if (double.IsNaN(result))
            //    result = 0;

            //return result;
        }

        public bool Collinear(Vector2D vector2D, double tolerance = Core.Tolerance.Angle)
        {
            if (vector2D == null)
                return false;

            return System.Math.Abs(System.Math.Abs(this * vector2D) - (Length * vector2D.Length)) <= tolerance;
        }

        public double SmallestAngle(Vector2D vector2D)
        {
            double value = System.Math.Abs(Angle(vector2D));
            if (value == 0)
                return value;

            return System.Math.PI - value;
        }

        public bool SameHalf(Vector2D vector2D)
        {
            if (vector2D == null)
                return false;

            Vector2D vector2D_Temp = new Vector2D(vector2D);
            vector2D_Temp.Negate();

            return Angle(vector2D) <= Angle(vector2D_Temp);
        }

        public double DotProduct(Vector2D vector2D)
        {
            return (coordinates[0] * vector2D.coordinates[0]) + (coordinates[1] * vector2D.coordinates[1]);
        }

        public Point2D Project(Point2D point2D)
        {
            if (point2D == null)
                return null;

            if (coordinates[0] == 0)
                return new Point2D(0, point2D.Y);

            if (coordinates[1] == 0)
                return new Point2D(point2D.X, 0);

            double m = coordinates[1] / coordinates[0];

            double X = (m * point2D.Y + point2D.X - m * 0) / (m * m + 1);
            double Y = (m * m * point2D.Y + m * point2D.X) / (m * m + 1);

            return new Point2D(X, Y);
        }

        /// <summary>
        /// Project input Vector onto this Vector
        /// </summary>
        /// <param name="vector2D">Vector to be projected</param>
        /// <returns>Projection vector</returns>
        public Vector2D Project(Vector2D vector2D)
        {
            return GetScaled(vector2D.DotProduct(this) / DotProduct(this));
        }

        public Vector2D GetScaled(double value)
        {
            Vector2D vector2D = new Vector2D(this);
            vector2D.Scale(value);
            return vector2D;
        }

        public Vector2D GetPerpendicular(Orientation orientation = Orientation.Clockwise)
        {
            switch (orientation)
            {
                case Orientation.Clockwise:
                    return new Vector2D(coordinates[1], -coordinates[0]);

                case Orientation.CounterClockwise:
                    return new Vector2D(-coordinates[1], coordinates[0]);

                default:
                    return null;
            }
        }

        public static Vector2D operator +(Vector2D vector2D_1, Vector2D vector2D_2)
        {
            return new Vector2D(vector2D_1.coordinates[0] + vector2D_2.coordinates[0], vector2D_1.coordinates[1] + vector2D_2.coordinates[1]);
        }

        public static Vector2D operator -(Vector2D vector2D_1, Vector2D vector2D_2)
        {
            return new Vector2D(vector2D_1.coordinates[0] - vector2D_2.coordinates[0], vector2D_1.coordinates[1] - vector2D_2.coordinates[1]);
        }

        public static double operator *(Vector2D vector2D_1, Vector2D vector2D_2)
        {
            return vector2D_1.coordinates[0] * vector2D_2.coordinates[0] + vector2D_1.coordinates[1] * vector2D_2.coordinates[1];
        }

        public static Vector2D operator /(Vector2D vector2D, double factor)
        {
            return new Vector2D(vector2D.X / factor, vector2D.Y / factor);
        }

        public static Vector2D operator *(Vector2D vector2D_1, double factor)
        {
            return new Vector2D(vector2D_1.coordinates[0] * factor, vector2D_1.coordinates[1] * factor);
        }

        public static Vector2D operator *(double factor, Vector2D vector2D_1)
        {
            return new Vector2D(vector2D_1.coordinates[0] * factor, vector2D_1.coordinates[1] * factor);
        }

        public static bool operator ==(Vector2D vector2D_1, Vector2D vector2D_2)
        {
            return vector2D_1?.coordinates[0] == vector2D_2?.coordinates[0] && vector2D_1?.coordinates[1] == vector2D_2?.coordinates[1];
        }

        public static bool operator !=(Vector2D vector2D_1, Vector2D vector2D_2)
        {
            return vector2D_1?.coordinates[0] != vector2D_2?.coordinates[0] || vector2D_1?.coordinates[1] != vector2D_2?.coordinates[1];
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
                    coordinates = new double[2];

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
                    coordinates = new double[2];

                coordinates[1] = value;
            }
        }

        public override string ToString()
        {
            return string.Format("{0}(X={1},Y={2})", GetType().Name, coordinates[0], coordinates[1]);
        }

        public override ISAMGeometry Clone()
        {
            return new Vector2D(this);
        }

        public override bool FromJObject(JObject jObject)
        {
            coordinates[0] = jObject.Value<double>("X");
            coordinates[1] = jObject.Value<double>("Y");

            return true;
        }

        public override JObject ToJObject()
        {
            JObject jObject = base.ToJObject();
            if (jObject == null)
                return null;

            jObject.Add("X", coordinates[0]);
            jObject.Add("Y", coordinates[1]);
            return jObject;
        }
    }
}