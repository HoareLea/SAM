using Newtonsoft.Json.Linq;
using SAM.Geometry.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Geometry.Planar
{
    /// <summary>
    /// Segment2D
    /// </summary>
    public class Segment2D : SAMGeometry, ICurve2D, ISegmentable2D, IReversible
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
            this.origin = new Point2D(origin);
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

        public Segment2D(JObject jObject)
            : base(jObject)
        {
        }

        /// <summary>
        /// Point2D of serment by given index
        /// </summary>
        public Point2D this[int index]
        {
            get
            {
                if (index == 0)
                    return new Point2D(origin);

                if (index == 1)
                {
                    Point2D point2D = new Point2D(origin);
                    point2D.Move(vector);
                    return point2D;
                }

                throw new Exception();
            }
            set
            {
                if (index == 0)
                {
                    Point2D point = this[1];
                    origin = value;
                    vector = new Vector2D(origin, point);
                }

                if (index == 1)
                {
                    vector = new Vector2D(origin, new Point2D(value));
                }

                throw new Exception();
            }
        }

        public List<Point2D> GetPoints()
        {
            return new List<Point2D>() { origin, End };
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

        public Vector2D Direction
        {
            get
            {
                return vector.Unit;
            }
        }

        public Vector2D Vector
        {
            get
            {
                return new Vector2D(vector);
            }
        }

        public void MoveTo(Point2D point2D)
        {
            origin = point2D;
        }

        public void Reverse()
        {
            origin = End;
            vector.Negate();
        }

        public Segment2D GetReversed()
        {
            return new Segment2D(End, vector.GetNegated());
        }

        /// <summary>
        /// Project given Point2D onto Segment2D
        /// </summary>
        /// <returns>Point2D.</returns>
        /// <param name="point2D">Point2D to be projected.</param>
        public Point2D Project(Point2D point2D)
        {
            if (point2D == null)
                return null;

            Point2D start = Start;
            Point2D end = End;

            if (start.X == end.X)
                return new Point2D(start.X, point2D.Y);

            double m = (end.Y - start.Y) / (end.X - start.X);
            double b = start.Y - (m * start.X);

            double X = (m * point2D.Y + point2D.X - m * b) / (m * m + 1);
            double Y = (m * m * point2D.Y + m * point2D.X + b) / (m * m + 1);

            return new Point2D(X, Y);
        }

        public Segment2D Project(ISegmentable2D segmentable2D)
        {
            List<Point2D> point2Ds = segmentable2D?.GetPoints();
            if (point2Ds == null || point2Ds.Count <= 1)
                return null;

            point2Ds = point2Ds.ConvertAll(x => Project(x));

            Point2D point2D_1;
            Point2D point2D_2;

            Query.ExtremePoints(point2Ds, out point2D_1, out point2D_2);
            if (point2D_1 == null || point2D_2 == null)
                return null;

            return new Segment2D(point2D_1, point2D_2);
        }

        /// <summary>
        /// Move segment by Vector *was GetMoved
        /// </summary>
        /// <returns>Segment2D</returns>
        /// <param name="vector2D">Vector tranformation.</param>
        public Segment2D GetMoved(Vector2D vector2D)
        {
            return new Segment2D((Point2D)origin.GetMoved(vector2D), vector);
        }

        public double Distance(Point2D point2D)
        {
            if (point2D == null)
                return double.NaN;

            return point2D.Distance(Closest(point2D));
        }

        public double Distance(Segment2D segment2D, double tolerance = Core.Tolerance.Distance)
        {
            if (segment2D == null)
                return double.NaN;

            Point2D point2D_Closest_1 = null;
            Point2D point2D_Closest_2 = null;

            Point2D point2D_Intersection = Intersection(segment2D, out point2D_Closest_1, out point2D_Closest_2);

            if (point2D_Intersection == null)
            {
                //Paraller segments
                Segment2D segment2D_Temp = segment2D;
                point2D_Closest_1 = Project(segment2D.origin);
                if (!On(point2D_Closest_1, tolerance))
                {
                    point2D_Closest_1 = Project(segment2D.End);
                    if (!On(point2D_Closest_1, tolerance))
                    {
                        segment2D_Temp = this;
                        point2D_Closest_1 = segment2D.Project(origin);
                        if (!segment2D.On(point2D_Closest_1, tolerance))
                        {
                            point2D_Closest_1 = segment2D.Project(End);
                            if (!segment2D.On(point2D_Closest_1, tolerance))
                                return (new double[] { segment2D.origin.Distance(origin), segment2D.End.Distance(End), segment2D.origin.Distance(End), segment2D.End.Distance(origin) }).Min();
                        }
                    }
                }

                point2D_Closest_2 = segment2D_Temp.Project(point2D_Closest_1);
            }
            else if (point2D_Closest_1 == null || point2D_Closest_2 == null)
            {
                return 0;
            }

            return point2D_Closest_1.Distance(point2D_Closest_2);
        }

        /// <summary>
        /// Find intersection Point2D by two segments2Ds. Method will aslo return closest point2Ds
        /// on Segmnet2Ds to extended intersection Point2D
        /// </summary>
        /// <returns>Point2D</returns>
        /// <param name="segment2D">Segment2D for intersection.</param>
        public Point2D Intersection(Segment2D segment2D, out Point2D point2D_Closest1, out Point2D point2D_Closest2, double tolerance = Core.Tolerance.Distance)
        {
            point2D_Closest1 = null;
            point2D_Closest2 = null;

            if (segment2D == null)
                return null;

            // Get the segments' parameters.
            double dx12 = End.X - Start.X;
            double dy12 = End.Y - Start.Y;
            double dx34 = segment2D.End.X - segment2D.Start.X;
            double dy34 = segment2D.End.Y - segment2D.Start.Y;

            // Solve for t1 and t2
            double denominator = (dy12 * dx34 - dx12 * dy34);
            if (double.IsNaN(denominator) || System.Math.Abs(denominator) < tolerance)
                return null;

            double t1 = ((Start.X - segment2D.Start.X) * dy34 + (segment2D.Start.Y - Start.Y) * dx34) / denominator;

            // The lines are parallel (or close enough to it).
            if (double.IsInfinity(t1) || double.IsNaN(t1))
                return null;

            double t2 = ((segment2D.Start.X - Start.X) * dy12 + (Start.Y - segment2D.Start.Y) * dx12) / -denominator;

            // Find the point of intersection.
            Point2D point2D_Intersection = new Point2D(Start.X + dx12 * t1, Start.Y + dy12 * t1);

            double t1_Temp = Core.Query.Round(t1, tolerance);
            double aT2_Temp = Core.Query.Round(t2, tolerance);

            // The segments intersect if t1 and t2 are between 0 and 1.
            if (((t1_Temp >= 0) && (t1_Temp <= 1) && (aT2_Temp >= 0) && (aT2_Temp <= 1)))
                return point2D_Intersection;

            // Find the closest points on the segments.
            if (t1 < 0)
                t1 = 0;
            else if (t1 > 1)
                t1 = 1;

            if (t2 < 0)
                t2 = 0;
            else if (t2 > 1)
                t2 = 1;

            point2D_Closest1 = new Point2D(Start.X + dx12 * t1, Start.Y + dy12 * t1);
            point2D_Closest2 = new Point2D(segment2D.Start.X + dx34 * t2, segment2D.Start.Y + dy34 * t2);
            return point2D_Intersection;
        }

        public Point2D Intersection(Segment2D segment2D, bool bounded = true, double tolerance = Core.Tolerance.Distance)
        {
            Point2D point2D_Closest1 = null;
            Point2D point2D_Closest2 = null;

            Point2D point2D_Intersection = Intersection(segment2D, out point2D_Closest1, out point2D_Closest2, tolerance);
            if (bounded && (point2D_Closest1 != null || point2D_Closest2 != null))
                return null;

            return point2D_Intersection;
        }

        public List<Point2D> Intersections(IEnumerable<Segment2D> segment2Ds, double tolerance = Core.Tolerance.Distance)
        {
            if (segment2Ds == null)
                return null;

            List<Point2D> point2Ds = new List<Point2D>();
            foreach (Segment2D segment2D in segment2Ds)
            {
                Point2D point2D = Intersection(segment2D, true, tolerance);
                if (point2D != null)
                    point2Ds.Add(point2D);
            }

            return point2Ds;
        }

        public List<Point2D> Intersections(ISegmentable2D segmentable2D)
        {
            return Intersections(segmentable2D?.GetSegments());
        }

        public bool On(Point2D point2D, double tolerance = Core.Tolerance.Distance)
        {
            return Distance(point2D) < tolerance;
        }

        /// <summary>
        /// Offset Segment2D by Vector2D and number count (default = 1)
        /// </summary>
        /// <returns>List Segment2D</returns>
        /// <param name="vector2D">Ofset Vector.</param>
        /// <param name="count">Ofset count.</param>
        public List<Segment2D> Offset(Vector2D vector2D, int count = 1)
        {
            if (count < 1)
                return null;

            List<Segment2D> aResult = new List<Segment2D>();
            for (int i = 0; i < count; i++)
                aResult.Add(GetMoved(vector2D * i));
            return aResult;
        }

        public Segment2D Offset(double offset, Orientation orientation)
        {
            return GetMoved(Direction.GetPerpendicular(orientation) * offset);
        }

        public override ISAMGeometry Clone()
        {
            return new Segment2D(origin, vector);
        }

        public Point2D GetStart()
        {
            return new Point2D(origin);
        }

        public Point2D GetEnd()
        {
            return origin.GetMoved(vector);
        }

        public List<Segment2D> GetSegments()
        {
            return new List<Segment2D>() { new Segment2D(this) };
        }

        public override bool FromJObject(JObject jObject)
        {
            origin = new Point2D(jObject.Value<JObject>("Origin"));
            vector = new Vector2D(jObject.Value<JObject>("Vector"));

            return true;
        }

        public override JObject ToJObject()
        {
            JObject jObject = base.ToJObject();
            if (jObject == null)
                return null;

            jObject.Add("Origin", origin.ToJObject());
            jObject.Add("Vector", vector.ToJObject());

            return jObject;
        }

        public Point2D Closest(Point2D point2D)
        {
            Point2D start = Start;
            Point2D end = End;

            double A = point2D.X - start.X;
            double B = point2D.Y - start.Y;
            double C = end.X - start.X;
            double D = end.Y - start.Y;

            double aDot = A * C + B * D;
            double aLen_sq = C * C + D * D;
            double aParameter = -1;
            if (aLen_sq != 0)
                aParameter = aDot / aLen_sq;

            if (aParameter < 0)
                return start;
            else if (aParameter > 1)
                return end;
            else
                return new Point2D(start.X + aParameter * C, start.Y + aParameter * D);
        }

        public Point2D ClosestEnd(Point2D point2D)
        {
            Point2D end = End;
            Point2D start = Start;

            if (end.Distance(point2D) < start.Distance(point2D))
                return end;
            else
                return start;
        }

        public int GetEndIndex(Point2D point2D)
        {
            Point2D end = End;
            Point2D start = Start;

            if (end.Distance(point2D) < start.Distance(point2D))
                return 1;
            else
                return 0;
        }

        public Point2D Mid()
        {
            return Point2D.Move(origin, vector.GetScaled(0.5));
        }

        public void Adjust(Point2D point2D)
        {
            Point2D point2D_Projected = Project(point2D);

            if (point2D_Projected.Distance(origin) < point2D_Projected.Distance(End))
                Start = point2D_Projected;
            else
                End = point2D_Projected;
        }

        public Point2D Max
        {
            get
            {
                return Query.Max(origin, End);
            }
        }

        public Point2D Min
        {
            get
            {
                return Query.Min(origin, End);
            }
        }

        public BoundingBox2D GetBoundingBox()
        {
            return new BoundingBox2D(origin, End);
        }

        /// <summary>
        /// Segment2D length
        /// </summary>
        public double GetLength()
        {
            return vector.Length;
        }

        public void SetLength(double value)
        {
            vector.Length = value;
        }

        public List<ICurve2D> GetCurves()
        {
            return new List<ICurve2D>() { new Segment2D(this) };
        }

        public bool Collinear(Segment2D segment2D, double tolerance = Core.Tolerance.Angle)
        {
            return vector.Collinear(segment2D.vector, tolerance);
        }

        public BoundingBox2D GetBoundingBox(double offset = 0)
        {
            return new BoundingBox2D(origin, GetEnd(), offset);
        }

        public double Distance(ISegmentable2D segmentable2D)
        {
            return Query.Distance(this, segmentable2D);
        }

        public Point2D GetPoint(double parameter, bool inverted = false)
        {
            if (parameter < 0 || parameter > 1)
                return null;

            if (inverted)
            {
                Segment2D segment2D = new Segment2D(this);
                segment2D.Reverse();
                return segment2D.GetPoint(parameter, false);
            }

            if (parameter == 0)
                return Start;

            if (parameter == 1)
                return End;

            return origin.GetMoved(vector * parameter);
        }

        public ISegmentable2D Trim(double parameter, bool inverted = false)
        {
            if (parameter <= 0 || parameter > 1)
                return null;

            if (inverted)
            {
                Segment2D segment2D = new Segment2D(this);
                segment2D.Reverse();
                return segment2D.Trim(parameter, false);
            }

            if (parameter == 1)
                return new Segment2D(this);

            Point2D point2D = GetPoint(parameter, false);
            if (point2D == null)
                return null;

            return new Segment2D(Start, point2D);
        }

        public double GetParameter(Point2D point2D, bool inverted = false, double tolerance = Core.Tolerance.Distance)
        {
            if (point2D == null)
                return double.NaN;

            if (inverted)
            {
                Segment2D segment2D = new Segment2D(this);
                segment2D.Reverse();
                return segment2D.GetParameter(point2D, false, tolerance);
            }

            Point2D point2D_Closest = Closest(point2D);
            if (point2D_Closest == null)
                return double.NaN;

            double length = vector.Length;

            double distance = new Vector2D(origin, point2D_Closest).Length;

            if (distance < tolerance)
                return 0;

            if (distance + tolerance > length)
                return 1;

            return length / distance;
        }

        public void Round(double tolerance = Core.Tolerance.Distance)
        {
            origin.Round(tolerance);
            vector.Round(tolerance);
        }
    }
}