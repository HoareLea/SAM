using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAM.Geometry.Spatial
{
    public class Point3D : IGeometry3D
    {
        public static Point3D Zero { get; } = new Point3D(0, 0, 0);

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

        public Point3D GetMoved(Vector3D vector3D)
        {
            return new Point3D(coordinates[0] + vector3D[0], coordinates[1] + vector3D[1], coordinates[2] + vector3D[2]);
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

        public static Vector3D GetNormal(Point3D point3D_1, Point3D point3D_2, Point3D point3D_3)
        {
            return new Vector3D(point3D_1, point3D_2).CrossProduct(new Vector3D(point3D_1, point3D_3));
        }

        public static List<Point3D> Clone(IEnumerable<Point3D> point3Ds)
        {
            List<Point3D> result = new List<Point3D>();
            foreach (Point3D point3D in point3Ds)
                result.Add(new Point3D(point3D));

            return result;
        }
            
        public IGeometry Clone()
        {
            return new Point3D(this);
        }
    }
}
