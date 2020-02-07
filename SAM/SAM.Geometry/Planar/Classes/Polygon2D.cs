using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;

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
    }
}
