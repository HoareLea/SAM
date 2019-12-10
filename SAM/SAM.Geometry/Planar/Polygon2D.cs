using System;
using System.Collections;
using System.Collections.Generic;

namespace SAM.Geometry.Planar
{
    class Polygon2D : IGeometry2D
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
    }
}
