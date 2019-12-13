using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAM.Geometry.Planar
{
    public class Vector2D : IGeometry2D
    {
        public static Vector2D BaseX { get; } = new Vector2D(1, 0);
        public static Vector2D BaseY { get; } = new Vector2D(0, 1);

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
                return Math.Sqrt((coordinates[0] * coordinates[0]) + (coordinates[1] * coordinates[1]));
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

        public void Negate()
        {
            coordinates[0] = -coordinates[0];
            coordinates[1] = -coordinates[1];
        }

        public bool AlmostEqual(Vector2D vector2D, double tolerance = Tolerance.MicroDistance)
        {
            return ((Math.Abs(coordinates[0] - vector2D.coordinates[0]) <= tolerance) && (Math.Abs(coordinates[1] - vector2D.coordinates[1]) <= tolerance));
        }

        public bool IsPerpedicular(Vector2D vector2D, double tolerance = Tolerance.MicroDistance)
        {
            return (vector2D.coordinates[0] * coordinates[0]) + (vector2D.coordinates[1] * coordinates[1]) <= tolerance;
        }

        public bool IsParallel(Vector2D vector2D, double tolerance = Tolerance.MicroDistance)
        {
            Vector2D vector2D_1 = vector2D.Unit;
            Vector2D vector2D_2 = Unit;

            return vector2D_1.AlmostEqual(vector2D_2, tolerance);
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

        public BoundingBox2D GetBoundingBox()
        {
            return new BoundingBox2D(Point2D.Zero, new Point2D(coordinates[0], coordinates[1]));
        }

        public double Angle(Vector2D vector2D)
        {
            return Math.Acos(DotProduct(vector2D) / (Length * vector2D.Length));
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

        public IGeometry Clone()
        {
            return new Vector2D(this);
        }
    }
}
