using Newtonsoft.Json.Linq;
using SAM.Geometry.Interfaces;
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
            this.points = Point2D.Clone(points);
            if (this.points.Last().Equals(this.points.First()))
                this.points.RemoveAt(this.points.Count - 1);
        }

        public Polygon2D(Polygon2D polygon2D)
        {
            this.points = polygon2D.GetPoints();
        }

        public Polygon2D(JObject jObject)
            : base(jObject)
        {
        }

        public List<Segment2D> GetSegments()
        {
            return Create.Segment2Ds(points, true);
        }

        public List<ICurve2D> GetCurves()
        {
            return GetSegments().ConvertAll(x => (ICurve2D)x);
        }

        public Orientation GetOrientation()
        {
            return Query.Orientation(points, true);
        }

        public int Count
        {
            get
            {
                return points.Count;
            }
        }

        public bool SetOrientation(Orientation orientation)
        {
            if (points == null || points.Count < 3 || orientation == Orientation.Undefined || orientation == Orientation.Collinear)
                return false;

            if (GetOrientation() != orientation)
                Modify.Reverse(points, true);

            return true;
        }

        public override ISAMGeometry Clone()
        {
            return new Polygon2D(this);
        }

        public List<Point2D> Points
        {
            get
            {
                return Point2D.Clone(points);
            }
        }

        public List<Point2D> GetPoints()
        {
            return points.ConvertAll(x => new Point2D(x));
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

        public Polyline2D GetPolyline(int index_Start, int count)
        {
            return new Polyline2D(points.GetRange(index_Start, count), false);
        }

        public Polyline2D GetPolyline()
        {
            return new Polyline2D(points, true);
        }

        public double Distance(ISegmentable2D segmentable2D)
        {
            return Query.Distance(this, segmentable2D);
        }

        public bool Inside(Point2D point2D)
        {
            //TODO: Safe to change for Inside
            return Query.Inside(points, point2D);
        }

        public bool Inside(IClosed2D closed2D)
        {
            if (closed2D is ISegmentable2D)
                return ((ISegmentable2D)closed2D).GetPoints().TrueForAll(x => Inside(x));

            throw new NotImplementedException();
        }

        public Point2D Closest(Point2D point2D, bool includeEdges)
        {
            if (includeEdges)
                return Query.Closest((ISegmentable2D)this, point2D);

            return Query.Closest(points, point2D);
        }

        public void Reverse(bool keepFirstPoint)
        {
            Modify.Reverse(points, keepFirstPoint);
        }

        public void Reverse()
        {
            Reverse(true);
        }

        public double GetArea()
        {
            return Query.Area(points);
        }

        public override bool FromJObject(JObject jObject)
        {
            if (jObject == null)
                return false;

            points = Geometry.Create.ISAMGeometries<Point2D>(jObject.Value<JArray>("Points"));
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

        public BoundingBox2D GetBoundingBox(double offset = 0)
        {
            return new BoundingBox2D(points, offset);
        }

        public Point2D GetInternalPoint2D()
        {
            return Point2D.GetInternalPoint2D(points);
        }

        public Point2D GetCentroid()
        {
            return Query.Centroid(points);
        }

        public bool Move(Vector2D vector2D)
        {
            if (points == null || vector2D == null)
                return false;

            for (int i = 0; i < points.Count; i++)
                points[i] = points[i].GetMoved(vector2D);

            return true;
        }

        public List<Point2D> Intersections(ISegmentable2D segmentable2D)
        {
            return Query.Intersections(this, segmentable2D);
        }

        public bool On(Point2D point2D, double tolerance = Core.Tolerance.Distance)
        {
            return Query.On(GetSegments(), point2D, tolerance);
        }

        public int IndexOf(Point2D point2D)
        {
            return points.IndexOf(point2D);
        }

        public bool Reorder(int startIndex)
        {
            return Core.Modify.Reorder(points, startIndex); ;
        }

        public double Distance(Point2D point2D)
        {
            return Query.Distance(this, point2D);
        }

        public double GetParameter(Point2D point2D, bool inverted = false, double tolerance = Core.Tolerance.Distance)
        {
            return Query.Parameter(this, point2D, inverted, tolerance);
        }

        public Point2D GetPoint(double parameter, bool inverted = false)
        {
            return Query.Point2D(this, parameter, inverted);
        }

        public ISegmentable2D Trim(double parameter, bool inverted = false)
        {
            return Query.Trim(this, parameter, inverted);
        }

        public double GetLength()
        {
            return GetSegments().ConvertAll(x => x.GetLength()).Sum();
        }

        public bool SimplifyByAngle(double maxAngle = Core.Tolerance.Angle)
        {
            return Query.SimplifyBySAM_Angle(points, true, maxAngle);
        }

        public IEnumerator<Point2D> GetEnumerator()
        {
            return GetPoints().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        public static explicit operator Polygon2D(Polyline2D polyline2D)
        {
            if (polyline2D == null)
                return null;

            return new Polygon2D(polyline2D.GetPoints());
        }

        public static explicit operator Polygon2D(Triangle2D triangle2D)
        {
            if (triangle2D == null)
                return null;

            return new Polygon2D(triangle2D.GetPoints());
        }

        public static explicit operator Polygon2D(Spatial.Polygon3D polygon3D)
        {
            if (polygon3D == null)
                return null;

            return polygon3D.GetPlane()?.Convert(polygon3D);
        }
    }
}