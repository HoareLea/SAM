using System.Collections.Generic;

namespace SAM.Geometry.Spatial
{
    public static partial class Query
    {
        public static Face3D Normalize(this Face3D face3D)
        {
            if(face3D == null)
            {
                return null;
            }
        }

        public static Polygon3D Normalize(this Polygon3D polygon3D)
        {
            if (polygon3D == null)
            {
                return null;
            }

            Plane plane = polygon3D?.GetPlane();
            if (plane == null)
            {
                return null;
            }

            Planar.ISegmentable2D segmentable2D = plane.Convert(polygon3D);
            if (segmentable2D == null)
            {
                throw new System.NotImplementedException();
            }

            Vector3D normal = Normal(plane, segmentable2D.GetPoints());
            if (normal == null)
            {
                return null;
            }

            if( plane.Normal.SameHalf(normal))
            {
                return new Polygon3D(polygon3D);
            }
            else
            {
                List<Planar.Point2D> point2Ds = segmentable2D.GetPoints();
                point2Ds.Reverse();

                return new Polygon3D(plane, point2Ds);
            }
        }
    }
}