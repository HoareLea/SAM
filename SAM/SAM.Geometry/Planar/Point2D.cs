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
    public class Point2D : IGeometry2D
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

        public void Move(Vector2D vector2D)
        {
            coordinates[0] += vector2D[0];
            coordinates[1] += vector2D[1];
        }

        public double Distance(Point2D point)
        {
            return Vector2D(point).Length;
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

        public string ToString(int decimals)
        {
            return string.Format("Point2D(X={0},Y={1})", Math.Round(coordinates[0], decimals), Math.Round(coordinates[1], decimals));
        }

        public bool AlmostEqual(Point2D point2D, double tolerance = Tolerance.MicroDistance)
        {
            return ((Math.Abs(coordinates[0] - point2D.coordinates[0]) < tolerance) && (Math.Abs(coordinates[1] - point2D.coordinates[1]) < tolerance));
        }
        
        public bool AlmostOnSegment(Segment2D segment2D, double tolerance = Tolerance.MicroDistance)
        {
            Segment2D segment2D_temp = new Segment2D(new Point2D(0, 0), new Point2D(segment2D[1].X - segment2D[0].X, segment2D[1].Y - segment2D[0].Y));
            Point2D aPoint2D = new Point2D(coordinates[0] - segment2D[0].X, coordinates[1] - segment2D[0].Y);
            return Math.Abs(segment2D_temp[1].X * aPoint2D.Y - aPoint2D.X * segment2D_temp[1].Y) < tolerance;
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

            Vector2D vector = Vector2D(point2D);
            vector.Length = vector.Length * factor;

            coordinates[0] = point2D.coordinates[0] + vector[0];
            coordinates[1] = point2D.coordinates[1] + vector[1];
        }

        public override bool Equals(object obj)
        {
            if (!(obj is Point2D))
                return false;

            Point2D aPoint2D = (Point2D)obj;
            return aPoint2D.coordinates[0].Equals(coordinates[0]) && aPoint2D.coordinates[1].Equals(coordinates[1]);
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

        public Vector2D Vector2D(Point2D point2D)
        {
            return new Vector2D(coordinates[0] - point2D[0], coordinates[1] - point2D[1]);
        }

        public Vector2D AsVector2D()
        {
            return new Vector2D(coordinates[0], coordinates[1]);
        }

        public Point2D GetMoved(Vector2D vector2D)
        {
            return new Point2D(vector2D[0] + coordinates[0], vector2D[1] + coordinates[1]);
        }


        public static Point2D Move(Point2D point, Vector2D vector)
        {
            Point2D point_Temp = new Point2D(point);
            point_Temp.Move(vector);
            return point_Temp;
        }

        public static Point2D Max(Point2D point2D_1, Point2D point2D_2)
        {
            return new Point2D(Math.Max(point2D_1.X, point2D_2.X), Math.Max(point2D_1.Y, point2D_2.Y));
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
            return new Point2D(Math.Min(point2D_1.X, point2D_2.X), Math.Min(point2D_1.Y, point2D_2.Y));
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
        }

        public static Orientation Orientation(Point2D point2D_1, Point2D point2D_2, Point2D point2D_3)
        {
            double aDeterminant = Determinant(point2D_1, point2D_2, point2D_3);

            if (aDeterminant == 0)
                return Geometry.Orientation.Collinear;

            if (aDeterminant > 0)
                return Geometry.Orientation.Clockwise;
            else
                return Geometry.Orientation.CounterClockwise;
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
            if (point2Ds == null || point2Ds.Count() < 3 || orientation == Geometry.Orientation.Collinear)
                return null;

            List<Point2D> aResult = new List<Point2D>(point2Ds);

            if (orientation == Geometry.Orientation.Undefined)
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

        public static List<Segment2D> GetSegmentList(IEnumerable<Point2D> point2Ds, bool close = false)
        {
            if (point2Ds == null)
                return null;

            List<Segment2D> result = new List<Segment2D>();
            if (point2Ds.Count() < 2)
                return result;

            int aCount = point2Ds.Count();

            for (int i = 0; i < aCount - 1; i++)
                result.Add(new Segment2D(point2Ds.ElementAt(i), point2Ds.ElementAt(i + 1)));

            if (close)
                result.Add(new Segment2D(point2Ds.Last(), point2Ds.First()));

            return result;
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
    }
}
