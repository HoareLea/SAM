using System.Linq;
using System.Collections.Generic;

namespace SAM.Geometry.Spatial
{
    public static partial class Create
    {
        // Constructs a plane from a collection of points so that the summed squared distance to all points is minimzized
        public static Plane Plane(this IEnumerable<Point3D> point3Ds, bool fit, double tolerance = Core.Tolerance.MicroDistance)
        {
            if (point3Ds == null || point3Ds.Count() < 3)
                return null;

            if (!fit)
                return Plane(point3Ds, tolerance);

            Point3D point3D_Centroid = Query.Centroid(point3Ds);

            double xx = 0;
            double yy = 0;
            double xy = 0;
            double yz = 0;
            double xz = 0;
            double zz = 0;

            foreach(Point3D point3D in point3Ds)
            {
                if (point3D == null)
                    continue;
                
                Vector3D vector3D = point3D - point3D_Centroid;
                xx += vector3D.X * vector3D.X;
                xy += vector3D.X * vector3D.Y;
                xz += vector3D.X * vector3D.Z;
                yy += vector3D.Y * vector3D.Y;
                yz += vector3D.Y * vector3D.Z;
                zz += vector3D.Z * vector3D.Z;
            }

            double[] det = new double[] { yy * zz - yz * yz, xx * zz - xz * xz, xx * yy - xy * xy };

            double det_Max = det.Max();

            if (det_Max <= 0)
                return null;

            Vector3D vector3D_Normal;
            if (det[0].Equals(det_Max))
                vector3D_Normal = new Vector3D(det_Max, xz * yz - xy * zz, xy * yz - xz * yy);
            else if (det[1].Equals(det_Max))
                vector3D_Normal = new Vector3D(xz * yz - xy * zz, det_Max, xy * xz - yz * xx);
            else
                vector3D_Normal = new Vector3D(xy * yz - xz * yy, xy * xz - yz * xx, det_Max);

            //if (!Query.Normal(point3Ds).SameHalf(vector3D_Normal))
            //    vector3D_Normal.Negate();

            return new Plane(point3D_Centroid, vector3D_Normal);
        }

        public static Plane Plane(this IEnumerable<Point3D> point3Ds, double tolerance = Core.Tolerance.MicroDistance)
        {
            Vector3D normal = Query.Normal(point3Ds, tolerance);
            if (normal == null)
                return null;

            return new Plane(point3Ds.ElementAt(0), normal.Unit);
        }
    }
}
