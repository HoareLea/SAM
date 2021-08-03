using SAM.Geometry.Spatial;
using System.Collections.Generic;

namespace SAM.Analytical
{
    public static partial class Query
    {
        
        public static List<Point3D> Point3Ds(this Panel panel, bool externalEdge = true, bool internalEdges = true)
        {
            Face3D face3D = panel?.GetFace3D();
            if(face3D == null || !face3D.IsValid())
            {
                return null;
            }

            return Geometry.Spatial.Query.Point3Ds(face3D, externalEdge, internalEdges);
        }
    }
}