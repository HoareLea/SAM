using SAM.Geometry.Spatial;

namespace SAM.Analytical
{
    public static partial class Create
    {
        public static Aperture Aperture(this ApertureConstruction apertureConstruction, Face3D face3D)
        {
            if(apertureConstruction == null || face3D == null || !face3D.IsValid())
            {
                return null;
            }

            return new Aperture(apertureConstruction, face3D);
        }
    }
}