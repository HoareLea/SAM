using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAM.Geometry.Spatial
{
    public class Point3D : IGeometry3D
    {
        private double[] coordinates = new double[3] { 0, 0, 0 };

        public Point3D()
        {
            coordinates = new double[3] { 0, 0, 0 };
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

        public double Distance(Point3D point3D)
        {
            return new Vector3D(this, point3D).Length;
        }

        public override string ToString()
        {
            return string.Format("{0}(X={1},Y={2},Z={3})", GetType().Name, coordinates[0], coordinates[1], coordinates[2]);
        }
    }
}
