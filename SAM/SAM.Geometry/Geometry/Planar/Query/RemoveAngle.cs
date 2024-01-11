using System.Collections.Generic;
using System.Linq;

namespace SAM.Geometry.Planar
{
    public static partial class Query
    {
        public static Polygon2D RemoveAngle(this Polygon2D polygon2D, double length, double minAngle = 1.5708, double tolerance = Core.Tolerance.MacroDistance)
        {
            List<Point2D> point2Ds = polygon2D?.Points;

            if (point2Ds == null)
                return null;

            if (point2Ds.Count < 3)
                return null;

            point2Ds.Insert(0, point2Ds.Last());
            point2Ds.Add(point2Ds[1]);

            int count = point2Ds.Count;

            List<Vector2D> directions = Directions(polygon2D.GetSegments());

            List<Point2D> point2Ds_New = new List<Point2D>();
            for (int i = 1; i < count - 1; i++)
            {
                Point2D point2D_Previous = point2Ds[i - 1];
                Point2D point2D = point2Ds[i];
                Point2D point2D_Next = point2Ds[i + 1];

                List<Point2D> point2Ds_Temp = RemoveAngle(point2D_Previous, point2D, point2D_Next, length, minAngle, tolerance);
                if (point2Ds_Temp == null)
                {
                    point2Ds_New.Add(point2D);
                }
                else if (point2Ds_Temp.Count == 3)
                {
                    point2Ds_New.Add(point2D);
                }
                else if (point2Ds_Temp.Count == 4)
                {
                    //TODO: To find better way to determine which points to choose
                    List<Point2D> point2Ds_Temp_2 = RemoveAngle(point2D_Next, point2D, point2D_Previous, length, minAngle, tolerance);
                    if (point2Ds_Temp_2 != null && point2Ds_Temp_2.Count == 4)
                    {
                        Vector2D vector2D_1 = new Vector2D(point2Ds_Temp[1], point2Ds_Temp[2]);
                        Vector2D vector2D_2 = new Vector2D(point2Ds_Temp_2[1], point2Ds_Temp_2[2]);

                        Vector2D vector2D_direction_1 = SmallestAngleVector(directions, vector2D_1);
                        Vector2D vector2D_direction_2 = SmallestAngleVector(directions, vector2D_2);

                        if (vector2D_direction_1.SmallestAngle(vector2D_1) < vector2D_direction_2.SmallestAngle(vector2D_2))
                        {
                            point2Ds_New.Add(point2Ds_Temp[1]);
                            point2Ds_New.Add(point2Ds_Temp[2]);
                        }
                        else
                        {
                            point2Ds_New.Add(point2Ds_Temp_2[2]);
                            point2Ds_New.Add(point2Ds_Temp_2[1]);
                        }
                    }
                    else
                    {
                        point2Ds_New.Add(point2Ds_Temp[1]);
                        point2Ds_New.Add(point2Ds_Temp[2]);
                    }
                }
                else
                {
                    point2Ds_New.Add(point2D);
                }
            }

            if (point2Ds_New == null || point2Ds_New.Count < 3)
                return null;

            return new Polygon2D(point2Ds_New);
        }

        public static List<Point2D> RemoveAngle(Point2D point2D_Previous, Point2D point2D, Point2D point2D_Next, double length, double minAngle = 1.5708, double tolerance = Core.Tolerance.MacroDistance)
        {
            if (point2D_Previous == null || point2D == null || point2D_Next == null)
                return null;

            double angle = Angle(point2D_Previous, point2D, point2D_Next);
            if (angle - minAngle + tolerance > 0)
                return new List<Point2D>() { point2D_Previous, point2D, point2D_Next };

            double b = Math.Query.Cotan(angle) * length;
            Vector2D vector2D_b = new Vector2D(point2D, point2D_Previous);
            if (vector2D_b.Length <= length)
                return null;

            double c = Math.Query.Cosec(angle) * length;
            Vector2D vector2D_c = new Vector2D(point2D, point2D_Next);
            if (vector2D_c.Length <= length)
                return null;

            vector2D_b = vector2D_b.Unit * b;
            vector2D_c = vector2D_c.Unit * c;

            Point2D point2D_b = point2D.GetMoved(vector2D_b);
            if (point2D_b.Distance(point2D_Previous) < tolerance)
                return new List<Point2D>() { point2D_Previous, point2D, point2D_Next };

            Point2D point2D_c = point2D.GetMoved(vector2D_c);
            if (point2D_c.Distance(point2D_Next) < tolerance)
                return new List<Point2D>() { point2D_Previous, point2D, point2D_Next };

            return new List<Point2D>() { point2D_Previous, point2D.GetMoved(vector2D_b), point2D.GetMoved(vector2D_c), point2D_Next };
        }
    }
}