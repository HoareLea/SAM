using Newtonsoft.Json.Linq;
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
    public class Segment2D : SAMGeometry, ICurve2D, ISegmentable2D
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

        public List<Point2D> GetPoints()
        {
            return new List<Point2D>() {origin, End };
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

        /// <summary>
        /// Project given Point2D onto Segment2D
        /// </summary>
        /// <returns>
        /// Point2D.
        /// </returns>
        /// <param name="point2D">Point2D to be projected.</param>
        public Point2D Project(Point2D point2D)
        {
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

        public double Distance(Point2D point2D)
        {
            if (point2D == null)
                return double.NaN;

            return point2D.Distance(Closest(point2D));
        }

        public double Distance(Segment2D segment2D)
        {
            Point2D point2D_Closest_1 = null;
            Point2D point2D_Closest_2 = null;

            Point2D point2D_Intersection = Intersection(segment2D, out point2D_Closest_1, out point2D_Closest_2);

            if (point2D_Intersection == null)
            {
                point2D_Closest_1 = Project(segment2D.origin);
                point2D_Closest_2 = segment2D.Project(point2D_Closest_1);
            }
            else if (point2D_Closest_1 == null || point2D_Closest_2 == null)
            {
                return 0;
            }

            return point2D_Closest_1.Distance(point2D_Closest_2);
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

        public Segment2D Offset(double offset, Orientation orientation)
        {
            return Move(Direction.GetPerpendicular(orientation) * offset);
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
            return new List<Segment2D>() { new Segment2D(origin, vector) };
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
                return Point2D.Max(origin, End);
            }
        }

        public Point2D Min
        {
            get
            {
                return Point2D.Min(origin, End);
            }
        }

        public BoundingBox2D GetBoundingBox()
        {
            return new BoundingBox2D(origin, End);
        }

        public double GetLength()
        {
            return vector.Length;
        }

        public List<ICurve2D> GetCurves()
        {
            return new List<ICurve2D>() { new Segment2D(this) };
        }

        public BoundingBox2D GetBoundingBox(double offset = 0)
        {
            return new BoundingBox2D(origin, GetEnd(), offset);
        }

        public double Distance(ISegmentable2D segmentable2D)
        {
            return Query.Distance(this, segmentable2D);
        }


    }
}
