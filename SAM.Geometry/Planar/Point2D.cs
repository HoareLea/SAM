using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAM.Geometry.Planar
{
    public class Point2D
    {
        private double[] coordinates = new double[2] { 0, 0 };

        public Point2D()
        {
            coordinates = new double[2] { 0, 0 };
        }

        public Point2D(double x, double y)
        {
            coordinates = new double[2] { x, y };
        }

        public Point2D(double[] coordinates)
        {
            coordinates[0] = this.coordinates[0];
            coordinates[1] = this.coordinates[1];
        }

        public Point2D(Point2D point2D)
        {
            coordinates = new double[2] { point2D[0], point2D[1] };
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
            return string.Format("Point2D(X={0},Y={1})", coordinates[0], coordinates[1]);
        }

        public string ToString(int decimals)
        {
            return string.Format("Point2D(X={0},Y={1})", Math.Round(coordinates[0], decimals), Math.Round(coordinates[1], decimals));
        }

        public bool AlmostEqual(Point2D point2D, double tolerance = Tolerance.MicroDistance)
        {
            return ((Math.Abs(coordinates[0] - point2D.coordinates[0]) < tolerance) && (Math.Abs(coordinates[1] - point2D.coordinates[1]) < tolerance));
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

        public Point2D Duplicate()
        {
            return new Point2D(this);
        }

        public void Move(Vector2D vector2D)
        {
            coordinates[0] += vector2D[0];
            coordinates[1] += vector2D[1];
        }

        public void Mirror(Point2D point2D)
        {
            this.Move(new Vector2D(point2D, this));
        }

        public void Scale(Point2D point2D, double factor)
        {
            if (point2D == null)
                return;

            if (factor == 0)
                return;

            if (factor == 1)
            {
                coordinates[0] = point2D.coordinates[0];
                coordinates[1] = point2D.coordinates[1];
                return;
            }

            Vector2D vector = Vector2D(point2D);
            vector.Length = vector.Length * factor;

            coordinates[0] = point2D.coordinates[0] + vector[0];
            coordinates[1] = point2D.coordinates[1] + vector[1];
        }

        public Vector2D Vector2D(Point2D point2D)
        {
            return new Vector2D(coordinates[0] - point2D[0], coordinates[1] - point2D[1]);
        }

        public Vector2D AsVector2D()
        {
            return new Vector2D(coordinates[0], coordinates[1]);
        }

    }
}
