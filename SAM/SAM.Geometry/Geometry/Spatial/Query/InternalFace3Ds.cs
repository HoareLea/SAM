using System.Collections.Generic;
using System.Linq;

namespace SAM.Geometry.Spatial
{
    public static partial class Query
    {
        public static List<Face3D> InternalFace3Ds(this Face3D face3D, Shell shell, double silverSpacing = Core.Tolerance.MacroDistance, double tolerance_Angle = Core.Tolerance.Angle, double tolerance_Distance= Core.Tolerance.Distance)
        {
            if(face3D == null || shell == null)
            {
                return null;
            }

            List<Face3D> face3Ds_Temp = face3D.Split(shell, tolerance_Angle, tolerance_Distance);
            if(face3Ds_Temp == null)
            {
                face3Ds_Temp = new List<Face3D>();
            }

            if(face3Ds_Temp.Count == 0)
            {
                face3Ds_Temp.Add(new Face3D(face3D));
            }

            List<Face3D> result = new List<Face3D>();
            foreach(Face3D face3D_Temp in face3Ds_Temp)
            {
                if(face3D_Temp == null || !face3D_Temp.IsValid())
                {
                    continue;
                }

                Point3D point3D = face3D_Temp.InternalPoint3D(tolerance_Distance);
                if(point3D == null || !point3D.IsValid())
                {
                    continue;
                }

                if(!shell.Inside(point3D, silverSpacing, tolerance_Distance))
                {
                    continue;
                }

                result.Add(face3D_Temp);
            }

            return result;
        }

        public static List<Face3D> InternalFace3Ds(this Face3D face3D, IEnumerable<Shell> shells, double silverSpacing = Core.Tolerance.MacroDistance, double tolerance_Angle = Core.Tolerance.Angle, double tolerance_Distance = Core.Tolerance.Distance)
        {
            if(face3D == null || shells == null || shells.Count() == 0)
            {
                return null;
            }

            List<Face3D> face3Ds = new List<Face3D>();
            foreach(Shell shell in shells)
            {
                List<Face3D> face3Ds_Shell = face3D.InternalFace3Ds(shell, silverSpacing, tolerance_Angle, tolerance_Distance);
                if(face3Ds_Shell != null && face3Ds_Shell.Count != 0)
                {
                    face3Ds.AddRange(face3Ds_Shell);
                }
            }

            return face3Ds?.Union();
        }
    }
}