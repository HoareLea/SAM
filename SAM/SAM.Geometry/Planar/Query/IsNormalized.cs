namespace SAM.Geometry.Spatial
{
    public static partial class Query
    {
        public static bool IsNormalized(this Face3D face3D)
        {
            return IsNormalized(face3D?.GetExternalEdge3D());
        }

        public static bool IsNormalized(this IClosedPlanar3D closedPlanar3D)
        {
            Plane plane = closedPlanar3D?.GetPlane();
            if (plane == null)
            {
                return false;
            }

            Planar.ISegmentable2D segmentable2D = closedPlanar3D as Planar.ISegmentable2D;
            if (segmentable2D == null)
            {
                throw new System.NotImplementedException();
            }

            Vector3D normal = Normal(plane, segmentable2D.GetPoints());
            if (normal == null)
            {
                return false;
            }

            return plane.Normal.SameHalf(normal);
        }

    }
}