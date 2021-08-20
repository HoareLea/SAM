using System.Collections.Generic;

namespace SAM.Geometry.Spatial
{
    public static partial class Query
    {
        public static Shell RemoveInvalidFace3Ds(this Shell shell, double tolerance = Core.Tolerance.Distance)
        {
            if(shell == null)
            {
                return null;
            }

            Shell result = new Shell(shell);

            List<Segment3D> segment3Ds = shell.NakedSegment3Ds(int.MaxValue, tolerance);
            if(segment3Ds == null || segment3Ds.Count == 0)
            {
                return result;
            }

            while (segment3Ds != null && segment3Ds.Count > 0)
            {
                List<Face3D> face3Ds = result.Face3Ds;
                if(face3Ds == null || face3Ds.Count < 3)
                {
                    return null;
                }

                foreach(Segment3D segment3D in segment3Ds)
                {
                    Point3D point3D = segment3D.Mid();
                    face3Ds.RemoveAll(x => x.OnEdge(point3D, tolerance));
                    if(face3Ds.Count < 3)
                    {
                        return null;
                    }
                }

                result = new Shell(face3Ds);
                segment3Ds = result.NakedSegment3Ds(int.MaxValue, tolerance);
            }

            return result;
        }
    }
}