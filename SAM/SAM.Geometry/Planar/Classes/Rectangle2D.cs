using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAM.Geometry.Planar
{
    public class Rectangle2D : IClosed2D
    {
        private Point2D origin;
        private double width;
        private double height;
        private Vector2D heightDirection;

        public Rectangle2D(double width, double height)
        {
            origin = Point2D.Zero;
            this.width = width;
            this.height = height;
            heightDirection = Vector2D.BaseY;
        }

        public Rectangle2D(Point2D origin, double width, double height)
        {
            this.origin = origin;
            this.width = width;
            this.height = height;
            heightDirection = Vector2D.BaseY;
        }

        public Rectangle2D(Point2D origin, double width, double height, Vector2D heightDirection)
        {
            this.origin = origin;
            this.width = width;
            this.height = height;
            this.heightDirection = heightDirection.Unit;
        }

        public Rectangle2D(Rectangle2D rectangle2D)
        {
            this.origin = new Point2D(rectangle2D.origin);
            this.width = rectangle2D.width;
            this.height = rectangle2D.height;
            this.heightDirection = new Vector2D(rectangle2D.heightDirection);
        }

        public Point2D[] GetPoints()
        {
            Point2D[] points = new Point2D[4];
            points[0] = origin;

            Point2D point2D = null;

            Vector2D heightVector = height * heightDirection;

            point2D = new Point2D(origin);
            point2D.Move(heightVector);
            points[1] = point2D;

            Vector2D widthDirection = WidthDirection;
            Vector2D widthVector = width * widthDirection;

            point2D = new Point2D(point2D);
            point2D.Move(widthVector);
            points[2] = point2D;

            point2D = new Point2D(point2D);
            heightVector.Negate();
            point2D.Move(heightVector);
            points[3] = point2D;

            return points;
        }

        public Segment2D[] GetSegments()
        {
            Point2D[] aPoints = GetPoints();

            Segment2D[] aSegments = new Segment2D[4];
            aSegments[0] = new Segment2D(aPoints[0], aPoints[1]);
            aSegments[1] = new Segment2D(aPoints[1], aPoints[2]);
            aSegments[2] = new Segment2D(aPoints[2], aPoints[3]);
            aSegments[3] = new Segment2D(aPoints[3], aPoints[0]);
            return aSegments;
        }

        public bool Contains(Point2D point, double offset)
        {
            return Point2D.Contains(GetPoints(), point, offset);
        }

        public Vector2D WidthDirection
        {
            get
            {
                return heightDirection.GetPerpendicular(Orientation.Clockwise);
            }
        }

        public Vector2D HeightDirection
        {
            get
            {
                return heightDirection;
            }
        }

        public double Width
        {
            get
            {
                return width;
            }
            set
            {
                width = value;
            }
        }

        public double Height
        {
            get
            {
                return height;
            }
            set
            {
                height = value;
            }
        }

        public Point2D Origin
        {
            get
            {
                return origin;
            }
            set
            {
                origin = new Point2D(value);
            }
        }

        public double GetArea()
        {
            return height * width;
        }

        public Point2D GetCentroid()
        {
            return Point2D.GetCentroid(GetPoints());
        }

        public BoundingBox2D GetBoundingBox()
        {
            return new BoundingBox2D(GetPoints());
        }

        public void Move(Vector2D vector2D)
        {
            origin.Move(vector2D);
        }

        public Rectangle2D GetMoved(Vector2D Vector2D)
        {
            return new Rectangle2D((Point2D)origin.GetMoved(Vector2D), width, height, heightDirection);
        }

        public double GetPerimeter()
        {
            return width * height;
        }

        public Segment2D[] GetDiagonals()
        {
            Point2D[] points = GetPoints();

            return new Segment2D[] { new Segment2D(points[0], points[2]), new Segment2D(points[1], points[3]) };
        }

        public void Scale(Point2D point2D, double factor)
        {
            origin.Scale(point2D, factor);
            width = width * factor;
            height = height * factor;
        }

        public IEnumerable<Triangle2D> Triangulate()
        {
            Point2D[] points = GetPoints();

            return new Triangle2D[] { new Triangle2D(points[0], points[1], points[2]), new Triangle2D(points[2], points[3], points[0]) };
        }

        public IGeometry Clone()
        {
            return new Rectangle2D(this);
        }
    }
}

