using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAM.Geometry.Spatial
{
    public class Sphere : IGeometry3D
    {
        private Point3D origin;
        private double radious;

        public Sphere(Point3D origin, double radious)
        {
            this.origin = origin;
            this.radious = radious;
        }

        public Sphere(Sphere sphere)
        {
            origin = new Point3D(sphere.origin);
            radious = sphere.radious;
        }

        public IGeometry Clone()
        {
            return new Sphere(this);
        }

        public bool Inside(Point3D point3D)
        {
            return origin.Distance(point3D) < radious;
        }

        public bool Inside(Segment3D segment3D)
        {
            return Inside(segment3D.GetPoints());
        }

        public bool Inside(Polygon3D polygon3D)
        {
            return Inside(polygon3D.Points);
        }

        public bool Inside(IEnumerable<Point3D> point3Ds)
        {
            if (point3Ds == null || point3Ds.Count() == 0)
                return false;
            
            foreach (Point3D point3D in point3Ds)
                if (!Inside(point3D))
                    return false;

            return true;
        }
    }
}
