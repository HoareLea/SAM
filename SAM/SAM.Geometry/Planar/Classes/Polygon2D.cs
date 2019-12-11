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
            this.points = new List<Point2D>(points);
        }

        public List<Segment2D> GetSegments()
        {
            return Point2D.GetSegmentList(points, true);
        }

        public List<Point2D> Points
        {
            get
            {
                return new List<Point2D>(points);
            }
        }
    }
}
