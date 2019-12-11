using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAM.Geometry.Spatial
{
    public class Vector3D : IGeometry3D
    {
        private double[] coordinates = new double[] { 0, 0, 0 };

        public Vector3D(double x, double y, double z)
        {
            coordinates = new double[] { x, y, z};
        }

        public Vector3D(Point3D start, Point3D end)
        {
            coordinates = new double[] { end[0] - start[0], end[1] - start[1], end[2] - start[2] };
        }

        public Vector3D()
        {
            coordinates = new double[] { 0, 0, 0 };
        }

        public Vector3D(Vector3D vector3D)
        {
            coordinates[0] = vector3D.coordinates[0];
            coordinates[1] = vector3D.coordinates[1];
            coordinates[2] = vector3D.coordinates[2];
        }

        public Vector3D(double[] coordinates)
        {
            coordinates[0] = this.coordinates[0];
            coordinates[1] = this.coordinates[1];
            coordinates[2] = this.coordinates[2];
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

        public double Length
        {
            get
            {
                return Math.Sqrt((coordinates[0] * coordinates[0]) + (coordinates[1] * coordinates[1]) + (coordinates[2] * coordinates[2]));
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

        /// <summary>
        /// The cross product takes two vectors and produces a third vector that is orthogonal to both. For example, if you have two vectors lying on the World xy-plane, then their cross product is a vector perpendicular to the xy-plane going either in the positive or negative World z-axis direction. Sample: a​<a1, a2, a3> and ​b​<b1, b2, b3> then a​ ​×​ ​b ​=​ ​<​a2​ ​*​ ​b3 – a3​ ​*​ ​b2​,​ ​a3​ ​*​ ​b1 - a1​ ​*​ ​b3, a1​ ​*​ ​b2 - a2​ ​*​ ​b1​ ​>
        /// </summary>
        /// <returns>
        /// Cross Product Vector3D
        /// </returns>
        /// <param name="vector3D">A Vector3D</param>
        public Vector3D CrossProduct(Vector3D vector3D)
        {
            return new Vector3D((coordinates[1] * vector3D.coordinates[2]) - (coordinates[2] * vector3D.coordinates[1]), (coordinates[2] * vector3D.coordinates[0]) - (coordinates[0] * vector3D.coordinates[2]), (coordinates[0] * vector3D.coordinates[1]) - (coordinates[1] * vector3D.coordinates[0]));
        }

        public double DotProduct(Vector3D vector3D)
        {
            return (coordinates[0] * vector3D.coordinates[0]) + (coordinates[1] * vector3D.coordinates[1]) + (coordinates[2] * vector3D.coordinates[2]);
        }

        public static Vector3D operator +(Vector3D vector3D_1, Vector3D vector3D_2)
        {
            return new Vector3D(vector3D_1.coordinates[0] + vector3D_2.coordinates[0], vector3D_1.coordinates[1] + vector3D_2.coordinates[1], vector3D_1.coordinates[2] + vector3D_2.coordinates[2]);
        }

        public static Vector3D operator -(Vector3D vector3D_1, Vector3D vector3D_2)
        {
            return new Vector3D(vector3D_1.coordinates[0] - vector3D_2.coordinates[0], vector3D_1.coordinates[1] - vector3D_2.coordinates[1], vector3D_1.coordinates[2] - vector3D_2.coordinates[2]);
        }

        public static double operator *(Vector3D vector3D_1, Vector3D vector3D_2)
        {
            return vector3D_1.coordinates[0] * vector3D_2.coordinates[0] + vector3D_1.coordinates[1] * vector3D_2.coordinates[1] + vector3D_1.coordinates[2] * vector3D_2.coordinates[2];
        }

        public static Vector3D operator *(Vector3D vector3D_1, double factor)
        {
            return new Vector3D(vector3D_1.coordinates[0] * factor, vector3D_1.coordinates[1] * factor, vector3D_1.coordinates[2] * factor);
        }

        public static Vector3D operator *(double factor, Vector3D vector3D_1)
        {
            return new Vector3D(vector3D_1.coordinates[0] * factor, vector3D_1.coordinates[1] * factor, vector3D_1.coordinates[2] * factor);
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
            return string.Format("{0}(X={1},Y={2},Z={2})", GetType().Name, coordinates[0], coordinates[1], coordinates[2]);
        }
    }
}
