using System;
using System.Collections;
using System.Collections.Generic;

namespace SAM.Geometry.Planar
{
    public class Polygon2D : IClosed2D
    {
        private List<Point2D> points;

        public Polygon2D(IEnumerable<Point2D> points)
        {
            this.points = Point2D.Clone(points);
        }

        public Polygon2D(Polygon2D polygon2D)
        {
            this.points = new List<Point2D>(points);
        }

        public List<Segment2D> GetSegments()
        {
            return Point2D.GetSegments(points, true);
        }

        public IGeometry Clone()
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
    }
}
