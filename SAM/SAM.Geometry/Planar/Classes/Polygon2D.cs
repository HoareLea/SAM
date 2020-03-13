using System;
using System.Collections.Generic;
using System.Linq;

using Newtonsoft.Json.Linq;


namespace SAM.Geometry.Planar
{
    public class Polygon2D : SAMGeometry, IClosed2D, ISegmentable2D
    {
        private List<Point2D> points;

        public Polygon2D(IEnumerable<Point2D> points)
        {
            this.points = Point2D.Clone(points);
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
            return Point2D.GetSegments(points, true);
        }

        public List<ICurve2D> GetCurves()
        {
            return GetSegments().ConvertAll(x => (ICurve2D)x);
        }

        public Orientation GetOrientation()
        {
            return Point2D.Orientation(points, true);
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

        public double Distance(ISegmentable2D segmentable2D)
        {
            return Query.Distance(this, segmentable2D);
        }

        public bool Inside(Point2D point2D)
        {
            return Point2D.Inside(points, point2D);
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
                return Query.Closest(this, point2D);

            return Query.Closest(points, point2D);
        }

        public void Reverse(bool keepFirstPoint = true)
        {
            if (points == null || points.Count < 2)
                return;

            if(keepFirstPoint)
            {
                Point2D point2D = points[0];
                points.RemoveAt(0);
                points.Add(point2D);
            }

            points.Reverse();
        }

        public double GetArea()
        {
            return Point2D.GetArea(points);
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

        public List<Polygon2D> Offset(double offset, Orientation orientation)
        {
            return Offset(new double[] { offset }, orientation);
        }

        public List<Polygon2D> Offset(IEnumerable<double> offsets, Orientation orientation)
        {
            if (points == null || points.Count < 3 || offsets == null)
                return null;

            int count = offsets.Count();

            if (count == 0)
                return new List<Polygon2D>() { new Polygon2D(this)};

            List<Segment2D> segment2Ds = GetSegments();
            if (segment2Ds == null || segment2Ds.Count() < 3)
                return null;

            segment2Ds.Insert(0, segment2Ds.Last());
            segment2Ds.Add(segment2Ds[1]);

            double offset = offsets.Last();

            Dictionary<Segment2D, List<Polygon2D>> segment2Ds_Offset = new Dictionary<Segment2D, List<Polygon2D>>();
            for (int i = 1; i < segment2Ds.Count - 1; i++)
            {
                if (i < count)
                    offset = offsets.ElementAt(i);

                Segment2D segment2D_Previous = segment2Ds[i - 1];
                Segment2D segment2D = segment2Ds[i];
                Segment2D segment2D_Next = segment2Ds[i + 1];

                Segment2D segment2D_Offset = segment2D.Offset(offset, orientation);

                Vector2D Vector2D_Previous = Query.MidVector(segment2D_Previous, segment2D);
                Vector2D Vector2D_Next = Query.MidVector(segment2D, segment2D_Next);

                Segment2D segment2D_Vector_Previous = new Segment2D(segment2D_Previous.End, Vector2D_Previous);
                Segment2D segment2D_Vector_Next = new Segment2D(segment2D_Next.Start, Vector2D_Next);

                Point2D point2D_Intersection_Previous = segment2D_Offset.Intersection(segment2D_Vector_Previous, false);
                if (point2D_Intersection_Previous == null)
                    continue;

                Point2D point2D_Intersection_Next = segment2D_Offset.Intersection(segment2D_Vector_Next, false);
                if (point2D_Intersection_Next == null)
                    continue;

                Segment2D segment2D_Offset_New = new Segment2D(point2D_Intersection_Previous, point2D_Intersection_Next);

                Point2D point2D_Intersection = segment2D_Vector_Previous.Intersection(segment2D_Vector_Next);

                List<Polygon2D> polygon2Ds = new List<Polygon2D>();
                if (point2D_Intersection == null)
                {
                    polygon2Ds.Add(new Polygon2D(new Point2D[] { segment2D.Start, segment2D.End, segment2D_Offset_New.Start, segment2D_Offset_New.End }));
                }
                else
                {
                    polygon2Ds.Add(new Polygon2D(new Point2D[] { segment2D.Start, segment2D.End, point2D_Intersection }));
                    polygon2Ds.Add(new Polygon2D(new Point2D[] { segment2D_Offset_New.Start, segment2D_Offset_New.End, point2D_Intersection }));
                }

                //if (!segment2D_Offset_New.Direction.AlmostEqual(segment2D.Direction))
                //    continue;
                //segments2Ds.Add(new Segment2D[] { segment2D_Offset_New, new Segment2D(segment2D.Start, point2D_Intersection_Previous), new Segment2D(segment2D.Start, point2D_Intersection_Previous) });

                segment2Ds_Offset[segment2D_Offset_New] = polygon2Ds;
            }

            List<Segment2D> segment2Ds_Split = Modify.Split(segment2Ds_Offset.Keys, Tolerance.MicroDistance);

            List<Polygon2D> polygon2Ds_All = new List<Polygon2D>();
            foreach(KeyValuePair<Segment2D, List<Polygon2D>> keyValuePair in segment2Ds_Offset)
                polygon2Ds_All.AddRange(keyValuePair.Value);
             
            List<int> indexes = new List<int>();
            for(int i=0; i < segment2Ds_Split.Count; i++)
            {
                Segment2D segment2D = segment2Ds_Split[i];

                Point2D point2D_1 = segment2D[0];
                Point2D point2D_2 = segment2D[1];

                foreach (Polygon2D polygon2D in polygon2Ds_All)
                {
                    List<Point2D> point2Ds_Intersections = polygon2D.Intersections(segment2D);
                    if(point2Ds_Intersections != null)
                    {
                        point2Ds_Intersections.RemoveAll(x => x.Distance(point2D_1) < Tolerance.MicroDistance);
                        point2Ds_Intersections.RemoveAll(x => x.Distance(point2D_2) < Tolerance.MicroDistance);

                        if (point2Ds_Intersections.Count > 0)
                        {
                            indexes.Add(i);
                            break;
                        }

                    }
                    
                    if(polygon2D.Inside(point2D_1) && !polygon2D.On(point2D_1))
                    {
                        indexes.Add(i);
                        break;
                    }

                    if (polygon2D.Inside(point2D_1) && !polygon2D.On(point2D_1))
                    {
                        indexes.Add(i);
                        break;
                    }
                }
            }

            if(indexes.Count > 0)
            {
                indexes.Reverse();
                indexes.ForEach(x => segment2Ds_Split.RemoveAt(x));
            }

            PointGraph2D pointGraph2D = new PointGraph2D(segment2Ds_Split);
            return pointGraph2D.GetPolygon2Ds();
        }

        public List<Polygon2D> Offset(IEnumerable<double> offsets, bool inside)
        {
            if (offsets == null || offsets.Count() == 0)
                return null;
            
            Orientation orientation = GetOrientation();
            if(!inside)
            {
                switch (orientation)
                {
                    case Orientation.Clockwise:
                        orientation = Orientation.CounterClockwise;
                        break;
                    case Orientation.CounterClockwise:
                        orientation = Orientation.Clockwise;
                        break;
                }
            }

            if (orientation == Orientation.Collinear || orientation == Orientation.Undefined)
                return null;

            return Offset(offsets, orientation);
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
            return Point2D.GetCentroid(points);
        }

        public List<Point2D> Intersections(ISegmentable2D segmentable2D)
        {
            return Query.Intersections(this, segmentable2D);
        }

        public bool On(Point2D point2D, double tolerance = 1E-09)
        {
            return Query.On(GetSegments(), point2D, tolerance);
        }

        public int IndexOf(Point2D point2D)
        {
            return points.IndexOf(point2D);
        }

        public bool Reorder(int startIndex)
        {
            List<Point2D> points_New = Core.Modify.Reorder(points, startIndex);
            if (points_New == null)
                return false;

            points = points_New;
            return true;
        }

        public double Distance(Point2D point2D)
        {
            return Query.Distance(this, point2D);
        }

        public double GetParameter(Point2D point2D)
        {
            return Query.Parameter(this, point2D);
        }

        public Point2D GetPoint(double parameter)
        {
            return Query.Point2D(this, parameter);
        }

        public double GetLength()
        {
            return GetSegments().ConvertAll(x => x.GetLength()).Sum();
        }
    }
}
