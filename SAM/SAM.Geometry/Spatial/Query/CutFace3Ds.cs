using System.Collections.Generic;

namespace SAM.Geometry.Spatial
{
    public static partial class Query
    {
        public static Shell CutFace3Ds(this Shell shell, Plane plane, double tolerance_Distance = Core.Tolerance.Distance, double tolerance_Snap = Core.Tolerance.MacroDistance)
        {
            if(shell == null || plane == null)
            {
                return null;
            }

            List<Face3D> face3Ds = shell.Face3Ds;
            if (face3Ds == null || face3Ds.Count == 0)
            {
                return null;
            }

            if (!plane.Intersect(shell.GetBoundingBox(), tolerance_Distance))
            {
                return new Shell(shell);
            }

            for (int i = face3Ds.Count - 1; i >= 0; i--)
            {
                List<Face3D> face3Ds_Cut = face3Ds[i].Cut(plane, tolerance_Distance);
                if(face3Ds_Cut == null || face3Ds_Cut.Count < 2)
                {
                    continue;
                }

                face3Ds.RemoveAt(i);
                foreach(Face3D face3D_Cut in face3Ds_Cut)
                {
                    face3Ds.Add(face3D_Cut.Snap(shell.Face3Ds, tolerance_Snap, tolerance_Distance));
                }
            }

            return new Shell(face3Ds);
        }
    }
}