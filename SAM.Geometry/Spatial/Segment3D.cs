using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAM.Geometry.Spatial
{
    public class Segment3D : IGeometry3D
    {
        private Point3D[] points = new Point3D[2];

        public Segment3D(Point3D start, Point3D end)
        {
            points[0] = start;
            points[1] = end;
        }
    }
}
