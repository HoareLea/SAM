using SAM.Geometry.Spatial;

namespace SAM.Geometry.Object.Spatial
{
    public static partial class Query
    {
        public static Rectangle3D MaxRectangle3D(this IFace3DObject face3DObject)
        {
            Face3D face3D = face3DObject?.Face3D;
            if (face3D == null)
            {
                return null;
            }

            return Geometry.Spatial.Query.MaxRectangle3D(face3D);
        }
    }
}