using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAM.Geometry.Spatial
{
    public class Face : IClosedPlanar3D, IGeometry3D
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
            this.plane = new Plane(plane);
            this.boundary = (Planar.IClosed2D)boundary.Clone();
        }

        public Face(IClosedPlanar3D closedPlanar3D)
        {
            plane = closedPlanar3D.GetPlane();
            boundary = plane.Convert(closedPlanar3D);

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

        public IClosedPlanar3D ToClosedPlanar3D()
        {

            return plane.Convert(boundary);
        }

        public Surface ToSurface()
        {
            return new Surface(plane.Convert(boundary));
        }

        public BoundingBox3D GetBoundingBox(double offset = 0)
        {
            return plane.Convert(boundary).GetBoundingBox(offset);
        }

        public Planar.IClosed2D Boundary
        {
            get
            {
                return boundary.Clone() as Planar.IClosed2D;
            }
        }

        public IClosed3D GetBoundary()
        {
            return plane.Convert(boundary).GetBoundary();
        }

        public IGeometry3D GetMoved(Vector3D vector3D)
        {
            return new Face((Plane)plane.GetMoved(vector3D), (Planar.IClosed2D)boundary.Clone());
        }

        public double GetArea()
        {
            return boundary.GetArea();
        }

        public bool Inside(Face face, double tolerance = Tolerance.MicroDistance)
        {
            if (!face.plane.Coplanar(plane, tolerance))
                return false;

            return boundary.Inside(face.boundary);
        }
    }
}
