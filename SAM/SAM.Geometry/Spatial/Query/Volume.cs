using System.Collections.Generic;

namespace SAM.Geometry.Spatial
{
    public static partial class Query
    {
        public static double Volume(this Shell shell, double silverSpacing = Core.Tolerance.MacroDistance, double tolerance = Core.Tolerance.Distance)
        {
            if(shell == null)
            {
                return double.NaN;
            }

            if(!shell.IsClosed(silverSpacing))
            {
                return double.NaN;
            }

            Shell shell_Temp = new Shell(shell);
            shell_Temp.OrientNormals(false, silverSpacing, tolerance);

            List<Face3D> face3Ds = shell_Temp.Face3Ds;
            if(face3Ds == null || face3Ds.Count == 0)
            {
                return double.NaN;
            }

            double result = 0;
            foreach(Face3D face3D in face3Ds)
            {
                if(face3D == null || face3D.GetArea() < silverSpacing)
                {
                    continue;
                }

                List<Triangle3D>  triangle3Ds = face3D.Triangulate(tolerance);
                if(triangle3Ds == null || triangle3Ds.Count == 0)
                {
                    continue;
                }

                Vector3D normal = face3D.GetPlane().Normal;

                foreach(Triangle3D triangle3D in triangle3Ds)
                {
                    if(triangle3D == null)
                    {
                        continue;
                    }

                    if (!triangle3D.GetNormal().SameHalf(normal))
                    {
                        triangle3D.Reverse();
                    }

                    result += triangle3D.SignedVolume();
                }
            }

            return result;

        }

        public static double Volume(this Extrusion extrusion)
        {
            if(extrusion == null)
            {
                return double.NaN;
            }

            Face3D face3D = extrusion.Face3D;
            if(face3D == null)
            {
                return double.NaN;
            }

            Vector3D vector3D = extrusion.Vector;
            if(vector3D == null)
            {
                return double.NaN;
            }

            return face3D.GetArea() * vector3D.Length;
        }
    }
}