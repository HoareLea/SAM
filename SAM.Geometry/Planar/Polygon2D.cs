using System;
using System.Collections;
using System.Collections.Generic;

namespace SAM.Geometry.Planar
{
    class Polygon2D : IGeometry2D, IEnumerable
    {
        private List<Point2D> points;

        public IEnumerator GetEnumerator()
        {
            return points.GetEnumerator();
        }
    }
}
