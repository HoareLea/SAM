using SAM.Geometry.Spatial;

namespace SAM.Geometry.Object.Spatial
{
    public static partial class Query
    {
        public static Segment3D MaxIntersectionSegment3D(this Plane plane, IFace3DObject face3DObject)
        {
            return Geometry.Spatial.Query.MaxIntersectionSegment3D(plane, face3DObject?.Face3D);
        }
    }
}