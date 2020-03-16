using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAM.Geometry.Planar
{
    /// <summary>
    /// Planar Point
    /// </summary>
    public class Point2D : SAMGeometry, ISAMGeometry2D
    {
        public static Point2D Invalid { get; } = new Point2D(double.NaN, double.NaN);

        public static Point2D Zero { get; } = new Point2D(0, 0);

        private double[] coordinates = new double[2] { 0, 0 };

        public Point2D()
        {
            coordinates = new double[2] { 0, 0 };
        }

        public Point2D(JObject jObject)
        {
            FromJObject(jObject);
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

        public void Move(Vector2D vector2D)
        {
            coordinates[0] += vector2D[0];
            coordinates[1] += vector2D[1];
        }

        public Point2D GetMoved(Vector2D vector2D)
        {
            return new Point2D(vector2D[0] + coordinates[0], vector2D[1] + coordinates[1]);
        }

        public double Distance(Point2D point2D)
        {
            return Vector(point2D).Length;
        }

        public Vector2D Vector(Point2D point2D)
        {
            return new Vector2D(coordinates[0] - point2D[0], coordinates[1] - point2D[1]);
        }

        public Vector2D AsVector()
        {
            return new Vector2D(coordinates[0], coordinates[1]);
        }

        public override string ToString()
        {
            return string.Format("{0}(X={1},Y={2})", GetType().Name, coordinates[0], coordinates[1]);
        }

        public string ToString(int decimals = Core.Rounding.Distance)
        {
            return string.Format("Point2D(X={0},Y={1})", System.Math.Round(coordinates[0], decimals), System.Math.Round(coordinates[1], decimals));
        }

        public bool AlmostEqual(Point2D point2D, double tolerance = Core.Tolerance.MicroDistance)
        {
            return ((System.Math.Abs(coordinates[0] - point2D.coordinates[0]) < tolerance) && (System.Math.Abs(coordinates[1] - point2D.coordinates[1]) < tolerance));
        }
        
        public bool AlmostOnSegment(Segment2D segment2D, double tolerance = Core.Tolerance.MicroDistance)
        {
            Segment2D segment2D_temp = new Segment2D(new Point2D(0, 0), new Point2D(segment2D[1].X - segment2D[0].X, segment2D[1].Y - segment2D[0].Y));
            Point2D aPoint2D = new Point2D(coordinates[0] - segment2D[0].X, coordinates[1] - segment2D[0].Y);
            return System.Math.Abs(segment2D_temp[1].X * aPoint2D.Y - aPoint2D.X * segment2D_temp[1].Y) < tolerance;
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

        public void Round(int decimals = Core.Rounding.Distance)
        {
            if (decimals == -1)
                return;
            
            coordinates[0] = System.Math.Round(coordinates[0], decimals);
            coordinates[1] = System.Math.Round(coordinates[1], decimals);
        }

        public void Mirror(Point2D point2D)
        {
            this.Move(new Vector2D(point2D, this));
        }

        public void Mirror(Segment2D segment2D)
        {
            this.Move(new Vector2D(segment2D.Project(this), this));
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

            Vector2D vector = ToVector(point2D);
            vector.Length = vector.Length * factor;

            coordinates[0] = point2D.coordinates[0] + vector[0];
            coordinates[1] = point2D.coordinates[1] + vector[1];
        }

        public override bool Equals(object obj)
        {
            if (!(obj is Point2D))
                return false;

            Point2D point2D = (Point2D)obj;
            return point2D.coordinates[0].Equals(coordinates[0]) && point2D.coordinates[1].Equals(coordinates[1]);
        }

        public bool Equals(Point2D point2D, double tolerance)
        {
            if (tolerance == 0)
                return Equals(point2D);

            return Distance(point2D) <= tolerance;
        }

        public bool IsNaN()
        {
            return double.IsNaN(coordinates[0]) || double.IsNaN(coordinates[1]);
        }

        public Point2D Closest(IEnumerable<Point2D> point2Ds)
        {
            return Closest(point2Ds, this);
        }

        public override ISAMGeometry Clone()
        {
            return new Point2D(this);
        }

        public Vector2D ToVector(Point2D point2D = null)
        {
            if (point2D == null)
                return new Vector2D(coordinates);
            else
                return new Vector2D(coordinates[0] - point2D[0], coordinates[1] - point2D[1]);
        }

        public override bool FromJObject(JObject jObject)
        {
            coordinates[0] = jObject.Value<double>("X");
            coordinates[1] = jObject.Value<double>("Y");

            return true;
        }

        public override JObject ToJObject()
        {
            JObject jObject = base.ToJObject();
            if (jObject == null)
                return null;

            jObject.Add("X", coordinates[0]);
            jObject.Add("Y", coordinates[1]);
            return jObject;
        }

        public override int GetHashCode()
        {
            int hash = 13;
            hash = (hash * 7) + coordinates[0].GetHashCode();
            hash = (hash * 7) + coordinates[1].GetHashCode();
            return hash;
        }


        public static Point2D Move(Point2D point, Vector2D vector)
        {
            Point2D point_Temp = new Point2D(point);
            point_Temp.Move(vector);
            return point_Temp;
        }

        public static Point2D Max(Point2D point2D_1, Point2D point2D_2)
        {
            return new Point2D(System.Math.Max(point2D_1.X, point2D_2.X), System.Math.Max(point2D_1.Y, point2D_2.Y));
        }

        public static Point2D Max(IEnumerable<Point2D> point2Ds)
        {
            if (point2Ds == null || point2Ds.Count() == 0)
                return null;

            double aX = double.MinValue;
            double aY = double.MinValue;
            foreach (Point2D point in point2Ds)
            {
                if (aX < point.X)
                    aX = point.X;
                if (aY < point.Y)
                    aY = point.Y;
            }

            return new Point2D(aX, aY);
        }

        public static Point2D Min(Point2D point2D_1, Point2D point2D_2)
        {
            return new Point2D(System.Math.Min(point2D_1.X, point2D_2.X), System.Math.Min(point2D_1.Y, point2D_2.Y));
        }

        public static Point2D Min(IEnumerable<Point2D> point2Ds)
        {
            if (point2Ds == null || point2Ds.Count() == 0)
                return null;

            double aX = double.MaxValue;
            double aY = double.MaxValue;
            foreach (Point2D point2D in point2Ds)
            {
                if (aX > point2D.X)
                    aX = point2D.X;
                if (aY > point2D.Y)
                    aY = point2D.Y;
            }

            return new Point2D(aX, aY);
        }

        public static double Determinant(Point2D point2D_1, Point2D point2D_2, Point2D point2D_3)
        {
            return (point2D_2.Y - point2D_1.Y) * (point2D_3.X - point2D_2.X) - (point2D_2.X - point2D_1.X) * (point2D_3.Y - point2D_2.Y);
            //return ((point2D_2.X * point2D_3.Y) + (point2D_1.X * point2D_2.Y) + (point2D_1.Y * point2D_3.X)) - ((point2D_1.Y * point2D_2.X) + (point2D_2.Y * point2D_3.X) + (point2D_1.X * point2D_3.Y));
        }

        public static Orientation Orientation(Point2D point2D_1, Point2D point2D_2, Point2D point2D_3)
        {
            double aDeterminant = Determinant(point2D_1, point2D_2, point2D_3);

            if (aDeterminant == 0)
                return SAM.Geometry.Orientation.Collinear;

            if (aDeterminant > 0)
                return SAM.Geometry.Orientation.Clockwise;
            else
                return SAM.Geometry.Orientation.CounterClockwise;
        }

        public static Orientation Orientation(IEnumerable<Point2D> point2Ds, bool convexHull = true)
        {
            if (point2Ds == null || point2Ds.Count() == 0)
                return Geometry.Orientation.Undefined;

            List<Point2D> point2Ds_Temp = new List<Point2D>(point2Ds);
            if (point2Ds_Temp == null || point2Ds_Temp.Count < 3)
                return Geometry.Orientation.Undefined;


            if (convexHull)
            {
                List<Point2D> point2Ds_ConvexHull = Query.ConvexHull(point2Ds);

                //ConvexHull may have different orientation so needs to remove unnecessary points from existing point2Ds
                if(point2Ds_ConvexHull != null  && point2Ds_ConvexHull.Count > 0)
                {
                    List<Point2D> point2Ds_ConvexHull_Temp = new List<Point2D>(point2Ds);
                    point2Ds_ConvexHull_Temp.RemoveAll(x => point2Ds_ConvexHull.Contains(x));
                    point2Ds_Temp.RemoveAll(x => point2Ds_ConvexHull_Temp.Contains(x));
                }

            }

            point2Ds_Temp.Add(point2Ds_Temp[0]);
            point2Ds_Temp.Add(point2Ds_Temp[1]);

            for (int i=0; i < point2Ds_Temp.Count - 2; i++)
            {
                Orientation orientation = Orientation(point2Ds_Temp[i], point2Ds_Temp[i + 1], point2Ds_Temp[i + 2]);
                if (orientation != Geometry.Orientation.Collinear && orientation != Geometry.Orientation.Undefined)
                    return orientation;
            }

            return Geometry.Orientation.Undefined;
        }

        public static List<Orientation> Orientations(IEnumerable<Point2D> point2Ds)
        {
            if (point2Ds == null)
                return null;

            int aCount = point2Ds.Count();

            if (aCount < 3)
                return null;

            List<Orientation> aResult = new List<Orientation>();

            aResult.Add(Orientation(point2Ds.ElementAt(aCount - 1), point2Ds.ElementAt(0), point2Ds.ElementAt(1)));

            for (int i = 1; i < aCount - 1; i++)
                aResult.Add(Orientation(point2Ds.ElementAt(i - 1), point2Ds.ElementAt(i), point2Ds.ElementAt(i + 1)));

            aResult.Add(Orientation(point2Ds.ElementAt(aCount - 2), point2Ds.ElementAt(aCount - 1), point2Ds.ElementAt(0)));

            return aResult;
        }

        public static List<Point2D> Orient(IEnumerable<Point2D> point2Ds, Orientation orientation)
        {
            if (point2Ds == null || point2Ds.Count() < 3 || orientation == SAM.Geometry.Orientation.Collinear)
                return null;

            List<Point2D> aResult = new List<Point2D>(point2Ds);

            if (orientation == SAM.Geometry.Orientation.Undefined)
                return aResult;

            List<Orientation> aOrienationList = Orientations(point2Ds);
            if (aOrienationList.Count(x => x == orientation) > (aOrienationList.Count / 2))
                return aResult;

            aResult.Reverse();

            return aResult;
        }

        public static List<Point2D> SortByDistance(Point2D point2D, IEnumerable<Point2D> point2Ds)
        {
            List<double> aDistanceList = point2Ds.ToList().ConvertAll(x => x.Distance(point2D));

            List<double> aDistanceList_Sorted = aDistanceList.Distinct().ToList();
            aDistanceList_Sorted.Sort();

            List<Point2D> aResult = new List<Point2D>();
            foreach (double aDistance in aDistanceList_Sorted)
            {
                for (int i = 0; i < aDistanceList.Count; i++)
                {
                    if (aDistance == aDistanceList[i])
                        aResult.Add(point2Ds.ElementAt(i));
                }
            }

            return aResult;
        }

        public static Point2D GetCentroid(IEnumerable<Point2D> point2Ds)
        {
            double aArea = 0;
            double aX = 0;
            double aY = 0;

            for (int i = 0, j = point2Ds.Count() - 1; i < point2Ds.Count(); j = i++)
            {
                Point2D aPoint2D_1 = point2Ds.ElementAt(i);
                Point2D aPoint2D_2 = point2Ds.ElementAt(j);

                double aArea_Temp = aPoint2D_1.X * aPoint2D_2.Y - aPoint2D_2.X * aPoint2D_1.Y;
                aArea += aArea_Temp;
                aX += (aPoint2D_1.X + aPoint2D_2.X) * aArea_Temp;
                aY += (aPoint2D_1.Y + aPoint2D_2.Y) * aArea_Temp;
            }

            if (aArea == 0)
                return null;

            aArea *= 3;
            return new Point2D(aX / aArea, aY / aArea);
        }

        public static Point2D GetInternalPoint2D(IEnumerable<Point2D> point2Ds, double tolerance = Core.Tolerance.Angle)
        {
            if (point2Ds == null || point2Ds.Count() < 3)
                return null;

            Point2D result = GetCentroid(point2Ds);
            if (Inside(point2Ds, result))
                return result;

            List<Point2D> point2Ds_List = new List<Point2D>(point2Ds);
            point2Ds_List.Add(point2Ds_List[0]);
            point2Ds_List.Add(point2Ds_List[1]);

            int count = point2Ds_List.Count;

            List<Segment2D> segments = Create.Segment2Ds(point2Ds, true);
            for (int i = 0; i < count - 2; i++)
            {
                for (int j = i + 1; j < count - 1; j++)
                {
                    for (int k = j + 1; k < count; k++)
                    {
                        Point2D point2D_1 = point2Ds_List[i];
                        Point2D point2D_2 = point2Ds_List[j];
                        Point2D point2D_3 = point2Ds_List[k];

                        Vector2D vector2D_1 = new Vector2D(point2D_1, point2D_2);
                        Vector2D vector2D_2 = new Vector2D(point2D_2, point2D_3);

                        if (vector2D_1.SmallestAngle(vector2D_2) < tolerance)
                            continue;

                        result = Mid(point2D_1, point2D_3);
                        if (Inside(point2Ds, result) && !Query.On(segments, result))
                            return result;
                    }
                }
            }

            return null;
        }

        public static Point2D Mid(Point2D point2D_1, Point2D point2D_2)
        {
            return new Point2D((point2D_1.X + point2D_2.X)/2, (point2D_1.Y + point2D_2.Y) / 2);
        }

        public static double GetMaxDistance(IEnumerable<Point2D> point2Ds, out Point2D point2D_1, out Point2D point2D_2)
        {
            int aCount = point2Ds.Count();

            double aDistance = double.MinValue;
            point2D_1 = null;
            point2D_2 = null;

            for (int i = 0; i < aCount; i++)
            {
                Point2D point2D_Temp_1 = point2Ds.ElementAt(i);
                for (int j = 0; j < aCount - 1; j++)
                {
                    Point2D point2D_Temp_2 = point2Ds.ElementAt(j);
                    double aDistance_Temp = point2D_Temp_1.Distance(point2D_Temp_2);
                    if (aDistance_Temp > aDistance)
                    {
                        aDistance = aDistance_Temp;
                        point2D_1 = point2D_Temp_1;
                        point2D_2 = point2D_Temp_2;
                    }
                }
            }

            return aDistance;
        }

        public static bool Add(List<Point2D> point2Ds, Point2D point2D, double offset = 0)
        {
            if (point2Ds == null || point2D == null)
                return false;

            if (offset == 0)
            {
                foreach (Point2D point3D_Temp in point2Ds)
                    if (point3D_Temp.Equals(point2D))
                        return false;
            }
            else
            {
                foreach (Point2D point3D_Temp in point2Ds)
                    if (point3D_Temp.Distance(point2D) <= offset)
                        return false;
            }

            point2Ds.Add(point2D);
            return true;
        }

        public static double GetArea(IEnumerable<Point2D> point2Ds)
        {
            if (point2Ds == null)
                return double.NaN;

            if (point2Ds.Count() < 3)
                return 0;

            List<Point2D> point2DList = new List<Point2D>(point2Ds);
            if (!point2DList[point2DList.Count - 1].Equals(point2DList[0]))
                point2DList.Add(point2DList[0]);

            return System.Math.Abs(point2Ds.Take(point2DList.Count - 1).Select((p, i) => (point2DList[i + 1].X - p.X) * (point2DList[i + 1].Y + p.Y)).Sum() / 2);
        }

        public static bool Contains(IEnumerable<Point2D> point2Ds, Point2D point2D, double tolerance = 0)
        {
            if (point2Ds == null || point2Ds.Count() == 0)
                return false;

            foreach (Point2D point2D_Temp in point2Ds)
                if (point2D_Temp.Equals(point2D, tolerance))
                    return true;

            return false;
        }

        public static bool Inside(IEnumerable<Point2D> point2Ds, Point2D point2D)
        {
            if (point2Ds == null)
                return false;

            int aCount = point2Ds.Count();

            if (aCount < 3)
                return false;

            bool result = false;

            int j = aCount - 1;
            for (int i = 0; i < aCount; i++)
            {
                if (point2Ds.ElementAt(i).Y < point2D.Y && point2Ds.ElementAt(j).Y >= point2D.Y || point2Ds.ElementAt(j).Y < point2D.Y && point2Ds.ElementAt(i).Y >= point2D.Y)
                    if (point2Ds.ElementAt(i).X + (point2D.Y - point2Ds.ElementAt(i).Y) / (point2Ds.ElementAt(j).Y - point2Ds.ElementAt(i).Y) * (point2Ds.ElementAt(j).X - point2Ds.ElementAt(i).X) < point2D.X)
                        result = !result;
                j = i;
            }
            return result;
        }

        public static void Scale(List<Point2D> point2Ds, Point2D point2D, double factor)
        {
            if (point2Ds == null)
                return;

            List<Point2D> result = new List<Point2D>();
            if (point2Ds.Count() == 0)
                return;

            for (int i = 0; i < point2Ds.Count; i++)
                point2Ds[i].Scale(point2D, factor);
        }

        public static void Move(List<Point2D> point2Ds, Vector2D vector2D)
        {
            if (point2Ds == null || vector2D == null)
                return;

            if (point2Ds.Count() == 0)
                return;

            for (int i = 0; i < point2Ds.Count; i++)
                point2Ds[i].Move(vector2D);
        }

        public static void Mirror(List<Point2D> point2Ds, Segment2D segment2D)
        {
            if (point2Ds == null || segment2D == null)
                return;

            if (point2Ds.Count() == 0)
                return;

            for (int i = 0; i < point2Ds.Count; i++)
                point2Ds[i].Mirror(segment2D);
        }

        public static void Mirror(List<Point2D> point2Ds, Point2D point2D)
        {
            if (point2Ds == null || point2D == null)
                return;

            if (point2Ds.Count() == 0)
                return;

            for (int i = 0; i < point2Ds.Count; i++)
                point2Ds[i].Mirror(point2D);
        }

        public static IEnumerable<Point2D> Split(Point2D point2D_1, Point2D point2D_2, int count)
        {
            if (point2D_1 == null || point2D_2 == null)
                return null;

            if (count <= 0)
                return null;

            if (count == 1)
                return new Point2D[] { point2D_1, point2D_2 };

            Vector2D vector2D = new Vector2D(point2D_1, point2D_2);
            double aLength_Split = vector2D.Length / count;
            vector2D = vector2D.Unit * aLength_Split;

            Point2D[] aResult = new Point2D[count + 1];

            aResult[0] = new Point2D(point2D_1);
            for (int i = 0; i < count; i++)
                aResult[i + 1] = (Point2D)aResult[i].GetMoved(vector2D);

            aResult[count] = new Point2D(point2D_2);

            return aResult;
        }
        
        public static Rectangle2D GetRectangle2D(IEnumerable<Point2D> point2Ds, Vector2D direction)
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
            double aHeight = GetMaxDistance(point2DList_Height, out point2D_1_Height, out point2D_2_Height);
            if (point2D_1_Height == null || point2D_2_Height == null)
                return null;

            Point2D point2D_1_Width = null;
            Point2D point2D_2_Width = null;
            double aWidth = GetMaxDistance(point2DList_Width, out point2D_1_Width, out point2D_2_Width);
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
        
        public static Rectangle2D GetRectangle2D(IEnumerable<Point2D> point2Ds)
        {
            if (point2Ds == null || point2Ds.Count() < 2)
                return null;

            List<Point2D> point2Ds_ConvexHull = Query.ConvexHull(point2Ds);

            double area = double.MaxValue;
            Rectangle2D rectangle = null;

            Vector2D vector2D_Base = new Vector2D(0, 1);

            HashSet<double> angleHashSet = new HashSet<double>();
            for (int i = 0; i < point2Ds_ConvexHull.Count - 1; i++)
            {
                Vector2D direction = new Vector2D(point2Ds_ConvexHull[i], point2Ds_ConvexHull[i + 1]);
                double angle = direction.Angle(vector2D_Base);
                if (!angleHashSet.Contains(angle))
                {
                    angleHashSet.Add(angle);
                    Rectangle2D rectangle_Temp = GetRectangle2D(point2Ds_ConvexHull, direction);
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

        public static List<Point2D> Clone(IEnumerable<Point2D> point2Ds)
        {
            List<Point2D> result = new List<Point2D>();
            foreach (Point2D point2D in point2Ds)
                result.Add(new Point2D(point2D));

            return result;
        }

        public static Point2D Closest(IEnumerable<Point2D> point2Ds, Point2D point2D)
        {
            if (point2Ds == null || point2D == null)
                return null;

            Point2D result = null;
            double distance_Min = double.MaxValue;
            foreach (Point2D point2D_Temp in point2Ds)
            {
                if (point2D_Temp == null)
                    continue;

                double distance = point2D.Distance(point2D_Temp);
                if (distance == 0)
                    return point2D_Temp;

                if(distance < distance_Min)
                {
                    result = point2D_Temp;
                    distance_Min = distance;
                }
            }

            return result;
        }


        public static bool operator ==(Point2D point2D_1, Point2D point2D_2)
        {
            if (object.ReferenceEquals(point2D_1, null) && object.ReferenceEquals(point2D_2, null))
                return true;

            if (object.ReferenceEquals(point2D_1, null) || object.ReferenceEquals(point2D_2, null))
                return false;

            return point2D_1.coordinates[0] == point2D_2.coordinates[0] && point2D_1.coordinates[1] == point2D_2.coordinates[1];
        }

        public static bool operator !=(Point2D point2D_1, Point2D point2D_2)
        {
            return point2D_1?.coordinates[0] != point2D_2?.coordinates[0] || point2D_1?.coordinates[1] != point2D_2?.coordinates[1];
        }

        public static Vector2D operator -(Point2D point2D_1, Point2D point2D_2)
        {
            return new Vector2D(point2D_1.coordinates[0] - point2D_2.coordinates[0], point2D_1.coordinates[1] - point2D_2.coordinates[1]);
        }
    }
}
