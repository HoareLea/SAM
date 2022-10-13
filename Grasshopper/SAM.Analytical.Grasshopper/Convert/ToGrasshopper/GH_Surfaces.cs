using Grasshopper.Kernel.Types;
using SAM.Geometry.Spatial;
using System.Collections.Generic;

namespace SAM.Analytical.Grasshopper
{
    public static partial class Convert
    {
        public static List<GH_Surface> ToGrasshopper(this Aperture aperture, bool includeFrame = false)
        {
            if(aperture == null)
            {
                return null;
            }

            List<GH_Surface> surfaces = new List<GH_Surface>();

            if(!includeFrame)
            {
                surfaces.Add(new GH_Surface(Geometry.Rhino.Convert.ToRhino_Brep(new Face3D(aperture.GetExternalEdge3D()))));
            }
            else
            {
                List<Face3D> face3Ds;

                face3Ds = aperture.GetFace3Ds(AperturePart.Frame);
                face3Ds?.ForEach(x => new GH_Surface(Geometry.Rhino.Convert.ToRhino_Brep(x)));

                face3Ds = aperture.GetFace3Ds(AperturePart.Pane);
                face3Ds?.ForEach(x => new GH_Surface(Geometry.Rhino.Convert.ToRhino_Brep(x)));
            }

            return surfaces;
        }
    }
}