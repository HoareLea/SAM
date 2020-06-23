using SAM.Geometry.Spatial;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical
{
    public static partial class Query
    {
        public static List<Panel> SplitByInternalEdges(this Panel panel, double tolerance = Core.Tolerance.Distance)
        {
            if (panel == null)
                return null;

            Face3D face3D = panel.GetFace3D();
            if (face3D == null)
                return null;

            List<Face3D> face3Ds = face3D.SplitByInternalEdges(tolerance);
            if (face3Ds == null)
                return null;

            List<Panel> result = new List<Panel>();
            for(int i=0; i < face3Ds.Count; i++)
            {
                Face3D face3D_Temp = face3Ds[i];

                System.Guid guid = System.Guid.NewGuid();
                if (i == 0)
                    guid = panel.Guid;


                Panel panel_Temp = new Panel(guid, panel, face3D_Temp, null, true);
                result.Add(panel_Temp);
            }

            return result;
        }
    }
}