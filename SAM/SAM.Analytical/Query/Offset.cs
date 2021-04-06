using SAM.Core;
using SAM.Geometry.Planar;
using SAM.Geometry.Spatial;
using System.Collections.Generic;

namespace SAM.Analytical
{
    public static partial class Query
    {
        public static List<Panel> Offset(this Panel panel, double offset, bool includeExternalEdge = true, bool includeInternalEdges = true, double tolerance = Tolerance.Distance)
        {
            List<Face3D> face3Ds = panel?.GetFace3D()?.Offset(offset, includeExternalEdge, includeInternalEdges, tolerance);
            if (face3Ds == null)
                return null;

            face3Ds.Sort((x, y) => y.GetArea().CompareTo(x.GetArea()));

            List<Panel> result = new List<Panel>();
            foreach(Face3D face3D in face3Ds)
            {
                System.Guid guid = panel.Guid;
                while (result.Find(x => x.Guid == guid) != null)
                    guid = System.Guid.NewGuid();

                result.Add(new Panel(guid, panel, face3D));
            }

            return result;
        }
    }
}