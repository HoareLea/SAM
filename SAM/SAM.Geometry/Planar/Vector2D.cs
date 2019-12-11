using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAM.Geometry.Planar
{
    public class Vector2D : IGeometry2D
    {
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
    }
}
