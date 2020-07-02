using System.Collections.Generic;
using System.Linq;

namespace SAM.Geometry.Planar
{
    public static partial class Create
    {
        public static Rectangle2D Rectangle2D(this IEnumerable<Point2D> point2Ds, Vector2D direction)
        {
            if (point2Ds == null || direction == null || point2Ds.Count() < 2)
                return null;

            Vector2D direction_Height = new Vector2D(direction);
            direction_Height = direction_Height.Unit;
            Vector2D direction_Width = direction_Height.GetPerpendicular();

            List<Point2D> point2DList_Height = new List<Point2D>();
            List<Point2D> point2DList_Width = new List<Point2D>();

            foreach (Point2D point2D in point2Ds)
            {
                point2DList_Height.Add(direction_Height.Project(point2D));
                point2DList_Width.Add(direction_Width.Project(point2D));
            }

            Point2D point2D_1_Height = null;
            Point2D point2D_2_Height = null;
            double aHeight = Query.MaxDistance(point2DList_Height, out point2D_1_Height, out point2D_2_Height);
            if (point2D_1_Height == null || point2D_2_Height == null)
                return null;

            Point2D point2D_1_Width = null;
            Point2D point2D_2_Width = null;
            double aWidth = Query.MaxDistance(point2DList_Width, out point2D_1_Width, out point2D_2_Width);
            if (point2D_1_Width == null || point2D_2_Width == null)
                return null;

            Segment2D segment2D_Height = new Segment2D(point2D_1_Height, point2D_2_Height);
            Segment2D segment2D_Width = new Segment2D(point2D_1_Width, point2D_2_Width);

            if (!segment2D_Height.Direction.AlmostEqual(direction_Height))
                segment2D_Height.Reverse();

            if (!segment2D_Width.Direction.AlmostEqual(direction_Width))
                segment2D_Width.Reverse();

            Point2D point2D_Temp = segment2D_Height[0];
            segment2D_Height.MoveTo(segment2D_Width[0]);
            segment2D_Width.MoveTo(point2D_Temp);

            Point2D point2D_Closest1 = null;
            Point2D point2D_Closest2 = null;

            Point2D point2D_Intersection = segment2D_Height.Intersection(segment2D_Width, out point2D_Closest1, out point2D_Closest2);
            if (point2D_Intersection == null)
                return null;

            return new Rectangle2D(point2D_Intersection, aWidth, aHeight, direction_Height);
        }

        public static Rectangle2D Rectangle2D(this IEnumerable<Point2D> point2Ds)
        {
            if (point2Ds == null || point2Ds.Count() < 2)
                return null;

            List<Point2D> point2Ds_ConvexHull = Query.ConvexHull(point2Ds);

            double area = double.MaxValue;
            Rectangle2D rectangle = null;

            Vector2D vector2D = Vector2D.WorldY;

            HashSet<double> angleHashSet = new HashSet<double>();
            for (int i = 0; i < point2Ds_ConvexHull.Count - 1; i++)
            {
                Vector2D direction = new Vector2D(point2Ds_ConvexHull[i], point2Ds_ConvexHull[i + 1]);
                double angle = direction.Angle(vector2D);
                if (!angleHashSet.Contains(angle))
                {
                    angleHashSet.Add(angle);
                    Rectangle2D rectangle_Temp = Rectangle2D(point2Ds_ConvexHull, direction);
                    double aArea_Temp = rectangle_Temp.GetArea();
                    if (aArea_Temp < area)
                    {
                        area = aArea_Temp;
                        rectangle = rectangle_Temp;
                    }
                }
            }

            return rectangle;
        }
    }
}