using System.Collections.Generic;

namespace SAM.Geometry.Spatial
{
    public static partial class Query
    {
        public static Dictionary<Face3D, Vector3D> NormalDictionary(this Shell shell, bool external = true, double silverSpacing = Core.Tolerance.MacroDistance, double tolerance = Core.Tolerance.Distance)
        {
            List<Face3D> face3Ds = shell?.Face3Ds;
            if (face3Ds == null)
                return null;
            
            Dictionary<Face3D, Vector3D> result = new Dictionary<Face3D, Vector3D>();
            for (int i = 0; i < face3Ds.Count; i++)
                result[face3Ds[i]] = shell.Normal(face3Ds[i].InternalPoint3D(), external, silverSpacing, tolerance);

            return result;
        }
    }
}