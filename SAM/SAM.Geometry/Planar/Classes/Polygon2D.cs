using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Geometry.Planar
{
    public class Polygon2D : SAMGeometry, IClosed2D, ISegmentable2D, IEnumerable<Point2D>, IReversible
    {
        private List<Point2D> points;

        public Polygon2D(IEnumerable<Point2D> points)
        {
            this.points = Query.Clone(points);
            if (this.points.Last().Equals(this.points.First()))
                this.points.RemoveAt(this.points.Count - 1);
        }

        public Polygon2D(Polygon2D polygon2D)
        {
            points = polygon2D.GetPoints();
        }

        public Polygon2D(JObject jObject)
            : base(jObject)
        {
        }

        public int Count
        {
            get
            {
                return points.Count;
            }
        }

        public List<Point2D> Points
        {
            get
            {
                return Query.Clone(points);
            }
        }

        public static implicit operator Polygon2D(BoundingBox2D boundingBox2D)
        {
            if (boundingBox2D == null)
                return null;

            return new Polygon2D(boundingBox2D.GetPoints());
        }

        public static implicit operator Polygon2D(Polyline2D polyline2D)
        {
            if (polyline2D == null)
                return null;

            return new Polygon2D(polyline2D.GetPoints());
        }

        public static implicit operator Polygon2D(Triangle2D triangle2D)
        {
            if (triangle2D == null)
                return null;

            return new Polygon2D(triangle2D.GetPoints());
        }

        public static implicit operator Polygon2D(Rectangle2D rectangle2D)
        {
            if (rectangle2D == null)
                return null;

            return new Polygon2D(rectangle2D.GetPoints());
        }

        public static implicit operator Polygon2D(Spatial.Polygon3D polygon3D)
        {
            if (polygon3D == null)
                return null;

            return Spatial.Query.Convert(polygon3D.GetPlane(), polygon3D);
        }

        public override ISAMGeometry Clone()
        {
            return new Polygon2D(this);
        }

        public Point2D Closest(Point2D point2D, bool includeEdges)
        {
            if (includeEdges)
                return Query.Closest((ISegmentable2D)this, point2D);

            return Query.Closest(points, point2D);
        }

        public double Distance(ISegmentable2D segmentable2D)
        {
            return Query.Distance(this, segmentable2D);
        }

        public double Distance(Point2D point2D)
        {
            return Query.Distance(this, point2D);
        }

        public override bool FromJObject(JObject jObject)
        {
            if (jObject == null)
                return false;

            points = Geometry.Create.ISAMGeometries<Point2D>(jObject.Value<JArray>("Points"));
            return true;
        }

        public double GetArea()
        {
            return Query.Area(points);
        }

        public BoundingBox2D GetBoundingBox(double offset = 0)
        {
            return new BoundingBox2D(points, offset);
        }

        public Point2D GetCentroid()
        {
            return Query.Centroid(points);
        }

        public List<ICurve2D> GetCurves()
        {
            return GetSegments().ConvertAll(x => (ICurve2D)x);
        }

        public IEnumerator<Point2D> GetEnumerator()
        {
            return GetPoints().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public override int GetHashCode()
        {
            if (points == null)
                return base.GetHashCode();

            int hash = 13;
            if (points != null)
                foreach (Point2D point2D in points)
                    hash = (hash * 7) + point2D.GetHashCode();

            return hash;
        }

        public Point2D GetInternalPoint2D(double tolerance = Core.Tolerance.Distance)
        {
            return Query.InternalPoint2D(points, tolerance);
        }

        public double GetLength()
        {
            return GetSegments().ConvertAll(x => x.GetLength()).Sum();
        }

        public Orientation GetOrientation()
        {
            return Query.Orientation(points, true);
        }

        public double GetParameter(Point2D point2D, bool inverted = false, double tolerance = Core.Tolerance.Distance)
        {
            return Query.Parameter(this, point2D, inverted, tolerance);
        }

        public Point2D GetPoint(double parameter, bool inverted = false)
        {
            return Query.Point2D(this, parameter, inverted);
        }

        public List<Point2D> GetPoints()
        {
            return points.ConvertAll(x => new Point2D(x));
        }

        public Polyline2D GetPolyline(int index_Start, int count)
        {
            return new Polyline2D(points.GetRange(index_Start, count), false);
        }

        public Polyline2D GetPolyline()
        {
            return new Polyline2D(points, true);
        }

        public List<Segment2D> GetSegments()
        {
            return Create.Segment2Ds(points, true);
        }
        public int IndexOf(Point2D point2D)
        {
            return points.IndexOf(point2D);
        }

        /// <summary>
        /// Inserts new point on one of the edges (closest to given point2D)
        /// </summary>
        /// <param name="point2D"> Point2D will be use as a reference to insert Point2D on Polygon2D edge</param>
        /// <param name="tolerance">tolerance</param>
        /// <returns>Point2D on Polygon2D edge</returns>
        public Point2D InsertClosest(Point2D point2D, double tolerance = Core.Tolerance.Distance)
        {
            return Modify.InsertClosest(points, point2D, true, tolerance);
        }

        public bool Inside(Point2D point2D, double tolerance = Core.Tolerance.Distance)
        {
            if (point2D == null)
                return false;

            bool result = Query.Inside(points, point2D);
            if (!result)
                return result;

            return !On(point2D, tolerance);
        }

        /// <summary>
        /// Checks if closed2D is inside this Polygon2D
        /// </summary>
        /// <param name="closed2D">Closed2D</param>
        /// <param name="tolerance">Tolerance</param>
        /// <returns>True if closed2D is inside this Polygon2D</returns>
        public bool Inside(IClosed2D closed2D, double tolerance = Core.Tolerance.Distance)
        {
            if (closed2D == null || points == null || points.Count == 0)
            {
                return false;
            }

            IClosed2D closed2D_Temp = closed2D;
            if (closed2D_Temp is Face2D)
            {
                closed2D_Temp = ((Face2D)closed2D_Temp).ExternalEdge2D;
            }

            if (!(closed2D_Temp is ISegmentable2D))
            {
                throw new NotImplementedException();
            }

            bool result = ((ISegmentable2D)closed2D_Temp).GetPoints().TrueForAll(x => Inside(x, tolerance) || On(x, tolerance));
            if (!result)
            {
                return result;
            }

            result = points.Find(x => closed2D_Temp.Inside(x, tolerance) && !closed2D_Temp.On(x, tolerance)) == null;

            return result;
        }

        public List<Point2D> Intersections(ISegmentable2D segmentable2D)
        {
            return Query.Intersections(this, segmentable2D);
        }

        public bool Move(Vector2D vector2D)
        {
            if (points == null || vector2D == null)
                return false;

            for (int i = 0; i < points.Count; i++)
                points[i] = points[i].GetMoved(vector2D);

            return true;
        }

        public bool On(Point2D point2D, double tolerance = Core.Tolerance.Distance)
        {
            return Query.On(GetSegments(), point2D, tolerance);
        }

        public bool Reorder(int startIndex)
        {
            return Core.Modify.Reorder(points, startIndex);
        }

        public void Reverse(bool keepFirstPoint)
        {
            Modify.Reverse(points, keepFirstPoint);
        }

        public void Reverse()
        {
            Reverse(true);
        }

        public bool SetOrientation(Orientation orientation)
        {
            if (points == null || points.Count < 3 || orientation == Orientation.Undefined || orientation == Orientation.Collinear)
                return false;

            if (GetOrientation() != orientation)
                Modify.Reverse(points, true);

            return true;
        }

        public override JObject ToJObject()
        {
            JObject jObject = base.ToJObject();
            if (jObject == null)
                return null;

            jObject.Add("Points", Core.Create.JArray(points));
            return jObject;
        }

        public ISegmentable2D Trim(double parameter, bool inverted = false)
        {
            return Query.Trim(this, parameter, inverted);
        }
    }
}