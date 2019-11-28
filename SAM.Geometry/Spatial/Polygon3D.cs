using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAM.Geometry.Spatial
{
    public class Polygon3D : IGeometry3D
    {
        private List<Point3D> points;

        public Polygon3D(IEnumerable<Point3D> points)
        {
            if (points != null)
                this.points = new List<Point3D>(points);
        }

        public List<Point3D> Points
        {
            get
            {
                return points;
            }
        }
    }
}
