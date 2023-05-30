using System.Collections.Generic;

namespace SAM.Geometry.Spatial
{
    public static partial class Query
    {
        /// <summary>
        /// Check if given point is above plane
        /// </summary>
        /// <param name="plane"></param>
        /// <param name="point3D"></param>
        /// <param name="tolerance"></param>
        /// <returns></returns>
        public static bool Above(this Plane plane, Point3D point3D, double tolerance = 0)
        {
            if (point3D == null)
                return false;

            Vector3D normal = plane?.Normal;
            if (normal == null)
                return false;

            Point3D origin = plane.Origin;
            if (origin == null)
                return false;

            return (normal.X * (point3D.X - origin.X)) + (normal.Y * (point3D.Y - origin.Y)) + (normal.Z * (point3D.Z - origin.Z)) > 0 + tolerance;
        }

        public static bool Above(this Plane plane, IFace3DObject face3DObject, double tolerance = 0)
        {
            if (face3DObject == null || plane == null)
            {
                return false;
            }

            return Above(plane, face3DObject.Face3D, tolerance);
        }

        public static bool Above(this Plane plane, Face3D face3D, double tolerance)
        {
            if (face3D == null)
            {
                return false;
            }

            ISegmentable3D segmentable3D = face3D.GetExternalEdge3D() as ISegmentable3D;
            if (segmentable3D == null)
            {
                throw new System.NotImplementedException();
            }


            List<Point3D> point3Ds = segmentable3D.GetPoints();
            if (point3Ds == null || point3Ds.Count == 0)
            {
                return false;
            }

            return point3Ds.TrueForAll(x => plane.Above(x, tolerance));
        }

        public static bool Above(this Plane plane, Shell shell, double tolerance)
        {
            if (plane == null || shell == null)
            {
                return false;
            }

            BoundingBox3D boundingBox3D = shell.GetBoundingBox();

            if (boundingBox3D == null)
            {
                return false;
            }

            if(Above(plane, boundingBox3D, tolerance))
            {
                return true;
            }

            return shell.Boundaries.TrueForAll(x => plane.Above(x.Item1, tolerance) && plane.Above(x.Item2, tolerance));
        }

        public static bool Above(this Plane plane, BoundingBox3D boundingBox3D, double tolerance)
        {
            if(plane == null || boundingBox3D == null)
            {
                return false;
            }

            return boundingBox3D.GetPoints().TrueForAll(x => plane.Above(x, tolerance));
        }
    }
}