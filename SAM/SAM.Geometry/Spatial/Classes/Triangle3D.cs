using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAM.Geometry.Spatial
{
    public class Triangle3D : IClosedPlanar3D, ISegmentable3D, IBoundable3D
    {
        private Point3D[] points = new Point3D[3];

        public Triangle3D(Point3D point3D_1, Point3D point3D_2, Point3D point3D_3)
        {
            points[0] = point3D_1;
            points[1] = point3D_2;
            points[2] = point3D_3;
        }

        public Triangle3D(Triangle3D triangle3D)
        {
            points = Point3D.Clone(triangle3D.points).ToArray();
        }

        public Vector3D GetNormal()
        {
            if (points.Length < 3)
                return null;

            return Point3D.GetNormal(points[0], points[1], points[2]);
        }

        public Plane GetPlane()
        {
            return new Plane(points[0], GetNormal());
        }

        public IGeometry Clone()
        {
            throw new NotImplementedException();
        }

        public List<Segment3D> GetSegments()
        {
            return new List<Segment3D>() { new Segment3D(points[0], points[1]), new Segment3D(points[1], points[2]), new Segment3D(points[2], points[0]) };
        }

        public BoundingBox3D GetBoundingBox(double offset = 0)
        {
            return new BoundingBox3D(points, offset);
        }
    }
}
