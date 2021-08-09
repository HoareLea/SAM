using System.Collections.Generic;

namespace SAM.Geometry.Spatial
{
    public static partial class Query
    {
        public static List<Face3D> FixEdges(this Face3D face3D, double tolerance = Core.Tolerance.Distance)
        {
            Plane plane = face3D?.GetPlane();
            if (plane == null)
            {
                return null;
            }

            List<Planar.Face2D> face2Ds = Planar.Query.FixEdges(plane.Convert(face3D), tolerance);
            return face2Ds?.ConvertAll(x => plane.Convert(x));
        }

        public static Shell FixEdges(this Shell shell, double tolerance = Core.Tolerance.Distance)
        {
            List<Face3D> face3Ds = shell?.Face3Ds;
            if(face3Ds == null || face3Ds.Count == 0)
            {
                return null;
            }

            List<Face3D> face3Ds_Result = new List<Face3D>();
            foreach(Face3D face3D in face3Ds)
            {
                if(face3D == null)
                {
                    continue;
                }

                List<Face3D> face3D_FixEdges = face3D.FixEdges(tolerance);
                if(face3D_FixEdges == null || face3D_FixEdges.Count == 0)
                {
                    face3Ds_Result.Add(face3D);
                    continue;
                }

                face3Ds_Result.AddRange(face3D_FixEdges);
            }

            return new Shell(face3Ds_Result);

        }
    }
}