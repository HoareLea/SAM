using System.Collections.Generic;

using SAM.Geometry.Spatial;

namespace SAM.Analytical
{
    public static partial class Create
    {
        public static List<Panel> Panels(this List<ISAMGeometry3D> geometry3Ds, PanelType panelType, Construction construction)
        {
            List<Face> faces = Geometry.Spatial.Query.Faces(geometry3Ds);
            if (faces == null)
                return null;

            List<PlanarBoundary3D> planarBoundary3Ds = null;
            if (!PlanarBoundary3D.TryGetPlanarBoundary3Ds(faces, out planarBoundary3Ds))
                return null;

            List<Panel> result = new List<Panel>();
            foreach (PlanarBoundary3D planarBoundary3D in planarBoundary3Ds)
                result.Add(new Panel(construction, panelType, planarBoundary3D));

            return result;
        }
    }
}
