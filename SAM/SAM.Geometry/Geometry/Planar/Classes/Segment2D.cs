using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Geometry.Planar
{
    /// <summary>
    /// Segment2D
    /// </summary>
    public class Segment2D : SAMGeometry, ICurve2D, ISegmentable2D, IReversible, IMovable2D<Segment2D>
    {
        private Point2D origin;
        private Vector2D vector;

        public Segment2D(double x_1, double y_1, double x_2, double y_2)
        {
            origin = new Point2D(x_1, y_1);
            vector = new Vector2D(x_2 - x_1, y_2 - y_1);
        }

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
        /// <param name="segment2D">Segment2D</param>
        public Segment2D(Segment2D segment2D)
        {
            origin = new Point2D(segment2D.origin);
            vector = new Vector2D(segment2D.vector);
        }

        public Segment2D(JObject jObject)
            : base(jObject)
        {
        }

        public Vector2D Direction
        {
            get
            {
                return vector.Unit;
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

        public Vector2D Vector
        {
            get
            {
                return new Vector2D(vector);
            }
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

        public static bool operator !=(Segment2D segment2D_1, Segment2D segment2D_2)
        {
            if (ReferenceEquals(segment2D_1, null) && ReferenceEquals(segment2D_2, null))
                return false;

            if (ReferenceEquals(segment2D_1, null) || ReferenceEquals(segment2D_2, null))
                return true;

            return (!segment2D_1.origin.Equals(segment2D_2.origin)) || (!segment2D_1.vector.Equals(segment2D_2.vector));
        }

        public static bool operator ==(Segment2D segment2D_1, Segment2D segment2D_2)
        {
            if (ReferenceEquals(segment2D_1, null) && ReferenceEquals(segment2D_2, null))
                return true;

            if (ReferenceEquals(segment2D_1, null) || ReferenceEquals(segment2D_2, null))
                return false;

            return segment2D_1.origin.Equals(segment2D_2.origin) && segment2D_1.vector.Equals(segment2D_2.vector);
        }

        public void Adjust(Point2D point2D)
        {
            Point2D point2D_Projected = Project(point2D);

            if (point2D_Projected.Distance(origin) < point2D_Projected.Distance(End))
                Start = point2D_Projected;
            else
                End = point2D_Projected;
        }

        public override ISAMGeometry Clone()
        {
            return new Segment2D(origin, vector);
        }

        public Point2D Closest(Point2D point2D, bool bounded = true)
        {
            Point2D start = Start;
            Point2D end = End;

            double a = point2D.X - start.X;
            double b = point2D.Y - start.Y;
            double c = end.X - start.X;
            double d = end.Y - start.Y;

            double dot = a * c + b * d;
            double len_sq = c * c + d * d;
            double parameter = -1;
            if (len_sq != 0)
                parameter = dot / len_sq;

            if (parameter < 0 && bounded)
                return start;
            else if (parameter > 1 && bounded)
                return end;
            else
                return new Point2D(start.X + parameter * c, start.Y + parameter * d);
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

        public bool Collinear(Segment2D segment2D, double tolerance = Core.Tolerance.Angle)
        {
            return vector.Collinear(segment2D.vector, tolerance);
        }

        public double Distance(Point2D point2D)
        {
            if (point2D == null)
                return double.NaN;

            return point2D.Distance(Closest(point2D));
        }

        public double Distance(ISegmentable2D segmentable2D)
        {
            return Query.Distance(this, segmentable2D);
        }

        public double Distance(Segment2D segment2D, double tolerance = Core.Tolerance.Distance)
        {
            if (segment2D == null)
                return double.NaN;

            Point2D point2D_Closest_1 = null;
            Point2D point2D_Closest_2 = null;

            Point2D point2D_Intersection = Intersection(segment2D, out point2D_Closest_1, out point2D_Closest_2, tolerance);

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

            return Math.Query.Min(Distance(segment2D[0]), Distance(segment2D[1]), segment2D.Distance(origin), segment2D.Distance(End));

            //return point2D_Closest_1.Distance(point2D_Closest_2);
        }

        public override bool Equals(object obj)
        {
            if (!(obj is Segment2D))
                return false;

            Segment2D segment2D = (Segment2D)obj;
            return segment2D.origin.Equals(origin) && segment2D.vector.Equals(vector);
        }

        public override bool FromJObject(JObject jObject)
        {
            origin = new Point2D(jObject.Value<JObject>("Origin"));
            vector = new Vector2D(jObject.Value<JObject>("Vector"));

            return true;
        }

        public BoundingBox2D GetBoundingBox()
        {
            return new BoundingBox2D(origin, End);
        }

        public BoundingBox2D GetBoundingBox(double offset = 0)
        {
            return new BoundingBox2D(origin, GetEnd(), offset);
        }

        public List<ICurve2D> GetCurves()
        {
            return new List<ICurve2D>() { new Segment2D(this) };
        }

        public Point2D GetEnd()
        {
            return origin.GetMoved(vector);
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

        public override int GetHashCode()
        {
            return Tuple.Create(origin, vector).GetHashCode();
        }

        /// <summary>
        /// Segment2D length
        /// </summary>
        public double GetLength()
        {
            return vector.Length;
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

        /// <summary>
        /// Returns parameter value (not normalized value so can be less than 0 and greater than 1). Parameter value which is less than 0 determine direction opposite to Segment 2D direction
        /// </summary>
        /// <param name="point2D">Point to be checked</param>
        /// <param name="inverted">Inverted segment. If value set to false then parameter value counted from End of the Segment2D</param>
        /// <param name="tolerance">Tolerance</param>
        /// <returns>Value which determine position of the point on Segment2D</returns>
        public double GetParameter(Point2D point2D, bool inverted = false, double tolerance = Core.Tolerance.Distance)
        {
            if (point2D == null)
            {
                return double.NaN;
            }

            if (inverted)
            {
                Segment2D segment2D = new Segment2D(this);
                segment2D.Reverse();
                return segment2D.GetParameter(point2D, false, tolerance);
            }

            Point2D point2D_Closest = Closest(point2D, false);
            if (point2D_Closest == null)
            {
                return double.NaN;
            }

            double length = vector.Length;
            if (double.IsNaN(length) || length < tolerance)
            {
                return double.NaN;
            }

            Vector2D vector2D = new Vector2D(origin, point2D_Closest);
            if (vector2D.Length < tolerance)
            {
                return 0;
            }

            double result = vector2D.Length / length;
            if (!vector2D.SameHalf(vector))
            {
                result = -result;
            }

            return result;
        }

        /// <summary>
        /// Gets point for given parameter value
        /// </summary>
        /// <param name="parameter">parameter value (not normalized value so can be less than 0 and greater than 1) value which is less than 0 determine direction opposite to Segment 2D direction</param>
        /// <param name="inverted">Inverted segment. If value set to false then parameter value counted from End of the Segment2D</param>
        /// <returns>Point2D on Segment2D</returns>
        public Point2D GetPoint(double parameter, bool inverted = false)
        {
            if (double.IsNaN(parameter))
            {
                return null;
            }

            if (inverted)
            {
                Segment2D segment2D = new Segment2D(this);
                segment2D.Reverse();
                return segment2D.GetPoint(parameter, false);
            }

            return origin.GetMoved(vector * parameter);
        }

        public List<Point2D> GetPoints()
        {
            return new List<Point2D>() { origin, End };
        }

        public Segment2D GetReversed()
        {
            return new Segment2D(End, vector.GetNegated());
        }

        public List<Segment2D> GetSegments()
        {
            return new List<Segment2D>() { new Segment2D(this) };
        }

        public Point2D GetStart()
        {
            return new Point2D(origin);
        }

        /// <summary>
        /// Find intersection Point2D by two segments2Ds. Method will aslo return closest point2Ds on Segment2Ds to extended intersection Point2D
        /// </summary>
        /// <param name="segment2D">Segment2D for intersection.</param>
        /// <param name="point2D_Closest1">Point2D closest to given Segment2D and being on this Segment2D</param>
        /// <param name="point2D_Closest2">Point2D closest to this Segment2D and being on given Segment2D</param>
        /// <param name="tolerance">Tolerance</param>
        /// <returns>Intersection Point2D</returns>
        public Point2D Intersection(Segment2D segment2D, out Point2D point2D_Closest1, out Point2D point2D_Closest2, double tolerance = Core.Tolerance.Distance)
        {
            return Query.Intersection(Start, End, segment2D?.Start, segment2D?.End, out point2D_Closest1, out point2D_Closest2, tolerance);
        }

        /// <summary>
        /// Intersection of two segments
        /// </summary>
        /// <param name="segment2D">segment2D</param>
        /// <param name="bounded">Second parameter determines if segments are bounded. If sets to false intersection point may not lay on given segments</param>
        /// <param name="tolerance">Tolerance</param>
        /// <returns>Intersection Point2D</returns>
        public Point2D Intersection(Segment2D segment2D, bool bounded = true, double tolerance = Core.Tolerance.Distance)
        {
            if(segment2D == null)
            {
                return null;
            }

            return Query.Intersection(Start, End, segment2D.Start, segment2D.End, bounded, tolerance);
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

        public bool IsNaN()
        {
            return (origin != null && origin.IsNaN()) || (vector != null && vector.IsNaN());
        }

        public Point2D Mid()
        {
            return Query.Move(origin, vector.GetScaled(0.5));
        }

        public void MoveTo(Point2D point2D)
        {
            origin = point2D;
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

        public bool On(Point2D point2D, double tolerance = Core.Tolerance.Distance)
        {
            return Distance(point2D) < tolerance;
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

            double x = (m * point2D.Y + point2D.X - m * b) / (m * m + 1);
            double y = (m * m * point2D.Y + m * point2D.X + b) / (m * m + 1);

            return new Point2D(x, y);
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

        public void Reverse()
        {
            origin = End;
            vector.Negate();
        }

        public void Round(double tolerance = Core.Tolerance.Distance)
        {
            origin.Round(tolerance);
            vector.Round(tolerance);
        }

        public void SetLength(double value)
        {
            vector.Length = value;
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

        /// <summary>
        /// Trim given Segments by parameter value (not normalized value so can be less than 0 and greater than 1) value which is less than 0 determine direction opposite to Segment 2D direction
        /// </summary>
        /// <param name="parameter">Parameter value</param>
        /// <param name="inverted">Inverted segment. If value set to false then parameter value counted from End of the Segment2D</param>
        /// <returns>Trimmed Segment2D</returns>
        public ISegmentable2D Trim(double parameter, bool inverted = false)
        {
            if (inverted)
            {
                Segment2D segment2D = new Segment2D(this);
                segment2D.Reverse();
                return segment2D.Trim(parameter, false);
            }

            Point2D point2D = GetPoint(parameter, false);
            if (point2D == null)
                return null;

            return new Segment2D(Start, point2D);
        }

        public ISAMGeometry2D GetTransformed(Transform2D transform2D)
        {
            return Query.Transform(this, transform2D);
        }

        public bool Move(Vector2D vector2D)
        {
            if(vector2D == null || origin == null)
            {
                return false;
            }

            origin.Move(vector2D);
            return true;
        }
    }
}