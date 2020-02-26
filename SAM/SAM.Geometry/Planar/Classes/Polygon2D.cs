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

        public void Reverse()
        {
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

        public Polygon2D Offset(double offset, Orientation orientation)
        {
            return Offset(new double[] { offset }, orientation);
        }
        
        public Polygon2D Offset(IEnumerable<double> offsets, Orientation orientation)
        {
            if (points == null || points.Count < 3 || offsets == null)
                return null;

            int count = offsets.Count();

            if (count == 0)
                return new Polygon2D(this);

            double offset = offsets.Last();

            List<Segment2D> segment2Ds = new List<Segment2D>();
            for(int i=0; i < points.Count; i++)
            {
                if (i < count)
                    offset = offsets.ElementAt(i);

                Segment2D segment2D = null;
                if(i < points.Count - 1)
                    segment2D = new Segment2D(points[i], points[i + 1]).Offset(offset, orientation);
                else
                    segment2D = new Segment2D(points[i], points[0]).Offset(offset, orientation);

                if (segment2Ds.Count > 0)
                {
                    Segment2D segment2D_Previous = segment2Ds[segment2Ds.Count - 1];

                    Point2D point2D_1;
                    Point2D point2D_2;
                    Point2D point2D_Intersection = segment2D_Previous.Intersection(segment2D, out point2D_1, out point2D_2);
                    if (point2D_Intersection != null)
                    {
                        segment2Ds[segment2Ds.Count - 1] = new Segment2D(segment2D_Previous[0], point2D_Intersection);
                        segment2D = new Segment2D(point2D_Intersection, segment2D[1]);
                    }
                }

                segment2Ds.Add(segment2D);
            }

            if (segment2Ds == null || segment2Ds.Count < 3)
                return null;

            return new Polygon2D(segment2Ds.ConvertAll(x => x.Start));
        }

        public BoundingBox2D GetBoundingBox(double offset = 0)
        {
           return new BoundingBox2D(points, offset);
        }
    }
}
