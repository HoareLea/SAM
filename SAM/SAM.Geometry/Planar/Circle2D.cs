using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAM.Geometry.Planar
{
    public class Circle2D
    {
        private Point2D center;
        private double radious;

        public Circle2D(Point2D center, double radious)
        {
            this.center = center;
            this.radious = radious;
        }

        public Point2D Center
        {
            get
            {
                return new Point2D(center);
            }
            set
            {
                center = value;
            }
        }

        public double Radious
        {
            get
            {
                return radious;
            }
            set
            {
                radious = value;
            }
        }

        public double GetArea()
        {
            return Math.PI * radious * radious;
        }

        public double Diameter
        {
            get
            {
                return radious * 2;
            }
            set
            {
                radious = value / 2;
            }
        }

        public BoundingBox2D GetBoundingBox()
        {
            return new BoundingBox2D(new Point2D(center[0] - radious, center[1] - radious), new Point2D(center[0] + radious, center[1] + radious));
        }

        public Point2D GetCentroid()
        {
            return new Point2D(center);
        }

        public Circle2D GetMoved(Vector2D vector2D)
        {
            return new Circle2D((Point2D)center.GetMoved(vector2D), radious);
        }

        public bool Inside(Point2D point2D)
        {
            return center.Distance(point2D) < radious;
        }

        public void Move(Vector2D vector2D)
        {
            center.Move(vector2D);
        }

        public double GetPerimeter()
        {
            return 2 * Math.PI * radious;
        }
    }
}
