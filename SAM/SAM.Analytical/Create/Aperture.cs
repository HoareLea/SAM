using SAM.Geometry.Spatial;
using System;

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

        public static Aperture Aperture(this Aperture aperture, Face3D face3D, Guid? guid = null)
        {
            if (aperture == null || face3D == null || !face3D.IsValid())
            {
                return null;
            }

            Guid guid_Temp = aperture.Guid;
            if(guid != null && guid.HasValue && guid.Value != Guid.Empty)
            {
                guid_Temp = guid.Value;
            }

            return new Aperture(guid_Temp, aperture, face3D);
        }
    }
}