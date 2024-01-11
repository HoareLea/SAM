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
            if (external && face3Ds.Count > 30)
            {
                foreach(Face3D face3D in face3Ds)
                {
                    result[face3D] = null;
                }

                System.Threading.Tasks.Parallel.For(0, face3Ds.Count, (int i) => 
                {
                    result[face3Ds[i]] = shell.Normal(i, external, silverSpacing, tolerance);
                });
            }
            else
            {
                for (int i = 0; i < face3Ds.Count; i++)
                {
                    result[face3Ds[i]] = shell.Normal(i, external, silverSpacing, tolerance);
                }
            }

            return result;
        }

        public static Dictionary<Face3D, Vector3D> NormalDictionary(this Extrusion extrusion, bool external = true, double silverSpacing = Core.Tolerance.MacroDistance, double tolerance = Core.Tolerance.Distance)
        {
            Shell shell = Create.Shell(extrusion);
            if(shell == null)
            {
                return null;
            }

            return NormalDictionary(shell, external, silverSpacing, tolerance);
        }
    }
}