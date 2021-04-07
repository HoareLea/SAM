using SAM.Geometry.Spatial;
using System.Collections.Generic;

namespace SAM.Analytical
{
    public static partial class Create
    {
        public static List<Aperture> Apertures(this List<ISAMGeometry3D> geometry3Ds, ApertureType apertureType = ApertureType.Window, ApertureConstruction apertureConstruction = null, double minArea = Core.Tolerance.MacroDistance, double tolerance = Core.Tolerance.Distance)
        {
            List<Face3D> face3Ds = Geometry.Spatial.Query.Face3Ds(geometry3Ds, tolerance);
            if (face3Ds == null)
                return null;

            List<Aperture> result = new List<Aperture>();
            foreach (Face3D face3D in face3Ds)
            {
                if (minArea != 0 && face3D.GetArea() < minArea)
                    continue;

                Aperture aperture = new Aperture(apertureConstruction, face3D);
                if (aperture.ApertureConstruction == null)
                {
                    PanelType panelType = face3D.GetPlane().Normal.PanelType();

                    ApertureConstruction apertureConstruction_Temp = Query.DefaultApertureConstruction(panelType, aperture.ApertureType);
                    if (apertureConstruction_Temp != null)
                        aperture = new Aperture(aperture, apertureConstruction_Temp);
                }

                result.Add(aperture);
            }

            return result;
        }
    }
}