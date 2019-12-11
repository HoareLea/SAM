using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAM.Geometry.Planar
{
    /// <summary>
    /// Segment2D
    /// </summary>
    public class Segment2D : ICurve2D
    {
        private Point2D origin;
        private Vector2D vector;

        /// <summary>
        /// Creates Segment2D by start and end Point2D 
        /// </summary>
        /// <param name="start">Segment2D start point.</param>
        /// <param name="end">Segment2D end point.</param>
        public Segment2D(Point2D start, Point2D end)
        {
            origin = new Point2D(start);
            vector = new Vector2D(start, end);
        }

        /// <summary>
        /// Creates Segment2D by origin and Vector2D 
        /// </summary>
        /// <param name="origin">Segment2D origin point.</param>
        /// <param name="vector2D">Segment2D direction.</param>
        public Segment2D(Point2D origin, Vector2D vector2D)
        {
            origin = new Point2D(origin);
            vector = new Vector2D(vector2D);
        }

        /// <summary>
        /// Duplicate Segment2D by Segment2D
        /// </summary>
        /// <param name="origin">Segment2D origin point.</param>
        /// <param name="vector2D">Segment2D direction.</param>
        public Segment2D(Segment2D segment2D)
        {
            origin = new Point2D(segment2D.origin);
            vector = new Vector2D(segment2D.vector);
        }

        /// <summary>
        /// Point2D of serment by given index
        /// </summary>
        public Point2D this[int Index]
        {
            get
            {
                if (Index == 0)
                    return new Point2D(origin);

                if (Index == 1)
                {
                    Point2D point2D = new Point2D(origin);
                    point2D.Move(vector);
                    return point2D;
                }

                throw new Exception();
            }
            set
            {
                if (Index == 0)
                {
                    Point2D point = this[1];
                    origin = value;
                    vector = new Vector2D(origin, point);
                }

                if (Index == 1)
                {
                    vector = new Vector2D(origin, new Point2D(value));
                }

                throw new Exception();
            }
        }

        /// <summary>
        /// Segment2D Start Point2D
        /// </summary>
        public Point2D Start
        {
            get
            {
                return new Point2D(origin);
            }
            set
            {
                Point2D point2D = End;
                origin = value;
                vector = new Vector2D(origin, point2D);
            }
        }

        /// <summary>
        /// Segment2D End Point2D
        /// </summary>
        public Point2D End
        {
            get
            {
                Point2D point2D = new Point2D(origin);
                point2D.Move(vector);
                return point2D;
            }
            set
            {
                vector = new Vector2D(origin, value);
            }
        }

        /// <summary>
        /// Segment2D length
        /// </summary>
        public double Length
        {
            get
            {
                return vector.Length;
            }
            set
            {
                vector.Length = value;
            }
        }

        /// <summary>
        /// Project given Point2D onto Segment2D
        /// </summary>
        /// <returns>
        /// Point2D.
        /// </returns>
        /// <param name="point2D">Point2D to be projected.</param>
        public Point2D Project(Point2D point2D)
        {
            if (Start.X == End.X)
                return new Point2D(Start.X, point2D.Y);

            double m = (End.Y - Start.Y) / (End.X - Start.X);
            double b = Start.Y - (m * Start.X);

            double X = (m * point2D.Y + point2D.X - m * b) / (m * m + 1);
            double Y = (m * m * point2D.Y + m * point2D.X + b) / (m * m + 1);

            return new Point2D(X, Y);
        }

        /// <summary>
        /// Move segment by Vector *was GetMoved
        /// </summary>
        /// <returns>
        /// Segment2D
        /// </returns>
        /// <param name="vector2D">Vector tranformation.</param>
        public Segment2D Move(Vector2D vector2D)
        {
            return new Segment2D((Point2D)origin.GetMoved(vector2D), vector);
        }

        /// <summary>
        /// Find intersection Point2D by two segments2Ds.  Method will aslo return closest point2Ds on Segmnet2Ds to extended intersection Point2D 
        /// </summary>
        /// <returns>
        /// Point2D
        /// </returns>
        /// <param name="segment2D">Segment2D for intersection.</param>
        public Point2D Intersection(Segment2D segment2D, out Point2D point2D_Closest1, out Point2D point2D_Closest2)
        {

            // Get the segments' parameters.
            double aDx12 = End.X - Start.X;
            double aDy12 = End.Y - Start.Y;
            double aDx34 = segment2D.End.X - segment2D.Start.X;
            double aDy34 = segment2D.End.Y - segment2D.Start.Y;

            // Solve for t1 and t2
            double aDenominator = (aDy12 * aDx34 - aDx12 * aDy34);

            double aT1 = ((Start.X - segment2D.Start.X) * aDy34 + (segment2D.Start.Y - Start.Y) * aDx34) / aDenominator;
            if (double.IsInfinity(aT1))
            {
                // The lines are parallel (or close enough to it).
                point2D_Closest1 = null;
                point2D_Closest2 = null;
                return null;
            }

            double aT2 = ((segment2D.Start.X - Start.X) * aDy12 + (Start.Y - segment2D.Start.Y) * aDx12) / -aDenominator;

            // Find the point of intersection.
            Point2D aPoint_Intersection = new Point2D(Start.X + aDx12 * aT1, Start.Y + aDy12 * aT1);

            // The segments intersect if t1 and t2 are between 0 and 1.
            if (((aT1 >= 0) && (aT1 <= 1) && (aT2 >= 0) && (aT2 <= 1)))
            {
                point2D_Closest1 = null;
                point2D_Closest2 = null;
                return aPoint_Intersection;
            }

            // Find the closest points on the segments.
            if (aT1 < 0)
                aT1 = 0;
            else if (aT1 > 1)
                aT1 = 1;

            if (aT2 < 0)
                aT2 = 0;
            else if (aT2 > 1)
                aT2 = 1;

            point2D_Closest1 = new Point2D(Start.X + aDx12 * aT1, Start.Y + aDy12 * aT1);
            point2D_Closest2 = new Point2D(segment2D.Start.X + aDx34 * aT2, segment2D.Start.Y + aDy34 * aT2);
            return aPoint_Intersection;
        }

        /// <summary>
        /// Offset Segment2D by Vector2D and number count (default = 1) 
        /// </summary>
        /// <returns>
        /// List Segment2D
        /// </returns>
        /// <param name="vector2D">Ofset Vector.</param>
        /// <param name="count">Ofset count.</param>
        public List<Segment2D> Offset(Vector2D vector2D, int count = 1)
        {
            if (count < 1)
                return null;
            
            List<Segment2D> aResult = new List<Segment2D>();
            for (int i = 0; i < count; i++)
                aResult.Add(Move(vector2D * i));
            return aResult;
        }

        /// <summary>
        /// Split Segment2Ds  
        /// </summary>
        /// <returns>
        /// List Segment2D
        /// </returns>
        /// <param name="segment2Ds">Sermnets2Ds</param>
        /// <param name="tolerance"> tolerance (default = 0) .</param>
        public static List<Segment2D> Split(IEnumerable<Segment2D> segment2Ds, double tolerance = 0)
        {
            if (segment2Ds == null)
                return null;

            int aCount = segment2Ds.Count();

            Dictionary<int, List<Point2D>> aPointDictionary = new Dictionary<int, List<Point2D>>();

            for (int i = 0; i < aCount - 1; i++)
            {
                Segment2D segment2D_1 = segment2Ds.ElementAt(i);
                for (int j = 1; j < aCount; j++)
                {
                    Segment2D segment2D_2 = segment2Ds.ElementAt(j);

                    Point2D point2D_Closest1;
                    Point2D point2D_Closest2;

                    Point2D point2D_Intersection = segment2D_1.Intersection(segment2D_2, out point2D_Closest1, out point2D_Closest2);
                    if (point2D_Intersection == null || point2D_Intersection.IsNaN())
                        continue;

                    if (point2D_Closest1 != null && point2D_Closest2 != null)
                        if (point2D_Closest1.Distance(point2D_Closest2) > tolerance)
                            continue;

                    List<Point2D> aPointList;

                    if (point2D_Intersection.Distance(segment2D_1.Start) > tolerance && point2D_Intersection.Distance(segment2D_1.End) > tolerance)
                    {
                        if (!aPointDictionary.TryGetValue(i, out aPointList))
                        {
                            aPointList = new List<Point2D>();
                            aPointDictionary[i] = aPointList;
                        }

                        Point2D.Add(aPointList, point2D_Intersection, tolerance);
                    }

                    if (point2D_Intersection.Distance(segment2D_2.Start) > tolerance && point2D_Intersection.Distance(segment2D_2.End) > tolerance)
                    {
                        if (!aPointDictionary.TryGetValue(j, out aPointList))
                        {
                            aPointList = new List<Point2D>();
                            aPointDictionary[j] = aPointList;
                        }

                        Point2D.Add(aPointList, point2D_Intersection, tolerance);
                    }
                }
            }

            List<Segment2D> aResult = new List<Segment2D>();

            for (int i = 0; i < aCount; i++)
            {
                Segment2D segment2D_Temp = segment2Ds.ElementAt(i);

                List<Point2D> aPointList;
                if (!aPointDictionary.TryGetValue(i, out aPointList))
                {
                    aResult.Add(segment2D_Temp);
                    continue;
                }

                Point2D.Add(aPointList, segment2D_Temp[0], tolerance);
                Point2D.Add(aPointList, segment2D_Temp[1], tolerance);

                aPointList = Point2D.SortByDistance(segment2D_Temp[0], aPointList);

                for (int j = 0; j < aPointList.Count - 1; j++)
                    aResult.Add(new Segment2D(aPointList[j], aPointList[j + 1]));
            }

            return aResult;
        }
    }
}
