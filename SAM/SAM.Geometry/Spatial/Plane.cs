using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAM.Geometry.Spatial
{
    public class Plane : IGeometry3D
    {
        private Vector3D normal;
        private Point3D origin;
        private Vector3D baseX;

        public Plane()
        {
            normal = new Vector3D(0, 0, 1);
            origin = new Point3D(0, 0, 0);
            baseX = GetBaseX();
        }

        public Plane(Vector3D normal, Point3D origin)
        {
            this.normal = normal.Unit;
            this.origin = origin;
            baseX = GetBaseX();
        }

        private Vector3D GetBaseX()
        {
            if (normal == null)
                return null;
            
            if (normal.X == 0 && normal.Y == 0)
                return new Vector3D(1, 0, 0);

            return new Vector3D(normal.Y, -normal.X, 0).Unit;
        }

        public Vector3D Normal
        {
            get
            {
                return new Vector3D(normal);
            }
        }

        public Point3D Origin
        {
            get
            {
                return new Point3D(origin);
            }
        }

        public Vector3D BaseX
        {
            get
            {
                return new Vector3D(baseX);
            }
        }

        public Vector3D BaseY
        {
            get
            {
                return normal.CrossProduct(baseX);
            }
        }


        /// <summary>
        /// Scalar constant relating origin point to normal vector.
        /// </summary>
        public double K
        {
            get
            {
                return normal.DotProduct(origin.ToVector3D());
            }
        }

        public Point3D Convert(Planar.Point2D point2D)
        {
            Vector3D baseY = BaseY;

            Vector3D u = new Vector3D(baseX.X * point2D.X, baseX.Y * point2D.X, baseX.Z * point2D.X);
            Vector3D v = new Vector3D(baseY.X * point2D.Y, baseY.Y * point2D.Y, baseY.Z * point2D.Y);

            return new Point3D(Origin.X + u.X + v.X, Origin.Y + u.Y + v.Y, Origin.Z + u.Z + v.Z);
        }

        public Planar.Point2D Convert(Point3D point3D)
        {
            Vector3D vector3D = new Vector3D(point3D.X - origin.X, point3D.Y - origin.Y, point3D.Z - origin.Z);
            return new Planar.Point2D(baseX.DotProduct(vector3D), BaseY.DotProduct(vector3D));
        }

        public Point3D Closest(Point3D point3D)
        {
            double factor = point3D.ToVector3D().DotProduct(normal) - K;
            return new Point3D(point3D.X - (normal.X * factor), point3D.Y - (normal.Y * factor), point3D.Z - (normal.Z * factor));
        }

        public double Distance(Point3D point3D)
        {
            return Closest(point3D).
        }
    }
}

