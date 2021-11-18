using SAM.Geometry.Spatial;
using System.Collections.Generic;

namespace SAM.Geometry
{
    public static partial class Convert
    {
        public static Shell ToSAM_Shell(this Mesh3D mesh3D)
        {
            List<Triangle3D> triangles =  mesh3D?.GetTriangles();
            if(triangles == null || triangles.Count == 0)
            {
                return null;
            }

            return new Shell(triangles.ConvertAll(x => new Face3D(x)));
        }
    }
}