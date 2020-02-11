using System.Collections.Generic;

using SAM.Geometry.Spatial;

namespace SAM.Analytical
{
    public static partial class Create
    {
        public static List<Panel> Panels(this List<ISAMGeometry3D> geometry3Ds, PanelType panelType, Construction construction)
        {
            List<Face3D> faces = Geometry.Spatial.Query.Faces(geometry3Ds);
            if (faces == null)
                return null;

            List<Panel> result = new List<Panel>();
            foreach (Face3D face in faces)
                result.Add(new Panel(construction, panelType, face));

            return result;
        }
    }
}
