using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAM.Geometry.Spatial
{
    public class Face : IClosedPlanar3D
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

            List<Spatial.Point3D> point3Ds = triangle3D.GetPoints();
            boundary = new Planar.Triangle2D(plane.Convert(point3Ds[0]), plane.Convert(point3Ds[1]), plane.Convert(point3Ds[2]));
        }

        public Face(Polygon3D polygon3D)
        {
            plane = polygon3D.GetPlane();

            boundary = new Planar.Polygon2D(polygon3D.GetPoints().ConvertAll(x => plane.Convert(x)));
        }

        public Face(Face face)
        {
            this.plane = new Plane(face.plane);
            this.boundary = (Planar.IClosed2D)face.boundary.Clone();
        }

        public Plane GetPlane()
        {
            return new Plane(plane);
        }

        public IGeometry Clone()
        {
            return new Face(this);
        }

        public IClosed3D ToClosed3D()
        {

            return plane.Convert(boundary);
        }

        public BoundingBox3D GetBoundingBox(double offset = 0)
        {
            throw new NotImplementedException();
        }
    }
}
