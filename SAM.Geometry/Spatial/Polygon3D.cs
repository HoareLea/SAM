using System;
using System.Collections;
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

        public Segment3D[] GetSegments()
        {
            int count = points.Count;

            Segment3D[] result = new Segment3D[count];
            for (int i = 0; i < count - 1; i++)
                result[i] = new Segment3D(points[i], points[i + 1]);

            result[count - 1] = new Segment3D(new Point3D(points[count - 1]), new Point3D(points[0]));

            return result;
        }
    }
}
