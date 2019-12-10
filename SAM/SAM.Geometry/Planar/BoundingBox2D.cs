using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAM.Geometry.Planar
{
    public class BoundingBox2D
    {
        private Point2D min;
        private Point2D max;

        public BoundingBox2D(IEnumerable<Point2D> point2Ds)
        {
            double aX_Min = double.MaxValue;
            double aX_Max = double.MinValue;
            double aY_Min = double.MaxValue;
            double aY_Max = double.MinValue;
            foreach (Point2D point2D in point2Ds)
            {
                if (point2D.X > aX_Max)
                    aX_Max = point2D.X;
                if (point2D.X < aX_Min)
                    aX_Min = point2D.X;
                if (point2D.Y > aY_Max)
                    aY_Max = point2D.Y;
                if (point2D.Y < aY_Min)
                    aY_Min = point2D.Y;
            }

            min = new Point2D(aX_Min, aY_Min);
            max = new Point2D(aX_Max, aY_Max);
        }

        public BoundingBox2D(Point2D point2D_1, Point2D point2D_2)
        {
            max = Point2D.Max(point2D_1, point2D_2);
            min = Point2D.Min(point2D_1, point2D_2);
        }

        public BoundingBox2D(Point2D point2D, double offset)
        {
            min = new Point2D(point2D.X - offset, point2D.Y - offset);
            max = new Point2D(point2D.X + offset, point2D.Y + offset);
        }

        public BoundingBox2D(IEnumerable<Point2D> point2Ds, double offset)
        {
            double aX_Min = double.MaxValue;
            double aX_Max = double.MinValue;
            double aY_Min = double.MaxValue;
            double aY_Max = double.MinValue;
            foreach (Point2D point2D in point2Ds)
            {
                if (point2D.X > aX_Max)
                    aX_Max = point2D.X;
                if (point2D.X < aX_Min)
                    aX_Min = point2D.X;
                if (point2D.Y > aY_Max)
                    aY_Max = point2D.Y;
                if (point2D.Y < aY_Min)
                    aY_Min = point2D.Y;
            }

            min = new Point2D(aX_Min - offset, aY_Min - offset);
            max = new Point2D(aX_Max + offset, aY_Max + offset);
        }
    }
}
