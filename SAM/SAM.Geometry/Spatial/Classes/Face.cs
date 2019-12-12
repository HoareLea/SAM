using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAM.Geometry.Spatial
{
    public class Face : IGeometry3D
    {
        private Plane plane;
        private Planar.IClosed2D boundary;

        public Face(Planar.Polygon2D polygon2D)
        {
            plane = new Plane(Point3D.Zero, Vector3D.BaseZ);
            boundary = polygon2D;
        }

        public Face(Plane plane, Planar.IClosed2D boundary)
        {
            this.plane = plane;
            this.boundary = boundary;
        }

        public Face(Triangle3D triangle3D)
        {
            plane = triangle3D.GetPlane();
        }

    }
}
