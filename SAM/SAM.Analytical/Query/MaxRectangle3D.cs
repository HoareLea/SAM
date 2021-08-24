using SAM.Geometry.Spatial;

namespace SAM.Analytical
{
    public static partial class Query
    {
        public static Rectangle3D MaxRectangle3D(this Panel panel)
        {
            Face3D face3D = panel?.GetFace3D();
            if(face3D == null)
            {
                return null;
            }

            return Geometry.Spatial.Query.MaxRectangle3D(face3D);
        }

        public static Rectangle3D MaxRectangle3D(this Aperture aperture)
        {
            Face3D face3D = aperture?.GetFace3D();
            if (face3D == null)
            {
                return null;
            }

            return Geometry.Spatial.Query.MaxRectangle3D(face3D);
        }
    }
}