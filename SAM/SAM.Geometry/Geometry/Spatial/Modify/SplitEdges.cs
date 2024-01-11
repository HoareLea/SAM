using System.Collections.Generic;

namespace SAM.Geometry.Spatial
{
    public static partial class Modify
    {
        public static bool SplitEdges(this List<Face3D> face3Ds, double tolerance = Core.Tolerance.Distance)
        {
            if(face3Ds == null || face3Ds.Count == 0)
            {
                return false;
            }

            bool result = false;
            for (int i = 0; i < face3Ds.Count; i++)
            {
                List<Face3D> face3Ds_Temp = new List<Face3D>(face3Ds);
                face3Ds_Temp.Remove(face3Ds[i]);

                if(Query.TrySplitEdges(face3Ds[i], face3Ds_Temp, out Face3D face3D, tolerance) && face3D != null)
                {
                    face3Ds[i] = face3D;
                    result = true;
                }
            }

            return result;
        }
    }
}