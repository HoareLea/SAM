using SAM.Geometry.Spatial;
using System.Collections.Generic;

namespace SAM.Analytical
{
    public static partial class Query
    {
        public static List<Shell> Intersection(this Shell shell_1, Shell shell_2, double silverSpacing = SAM.Core.Tolerance.MacroDistance, double tolerance_Angle = Core.Tolerance.Angle, double tolerance_Distance = SAM.Core.Tolerance.Distance)
        {
            BoundingBox3D boundingBox3D_1 = shell_1?.GetBoundingBox();
            if(boundingBox3D_1 == null)
            {
                return null;
            }

            BoundingBox3D boundingBox3D_2 = shell_2?.GetBoundingBox();
            if (boundingBox3D_2 == null)
            {
                return null;
            }

            if(!boundingBox3D_1.InRange(boundingBox3D_2))
            {
                return null;
            }

            Shell shell_1_Temp = new Shell(shell_1);
            shell_1_Temp.Split(shell_2, tolerance_Angle, tolerance_Distance);

            Shell shell_2_Temp = new Shell(shell_2);
            shell_2_Temp.Split(shell_1, tolerance_Angle, tolerance_Distance);

            List<Face3D> face3Ds = new List<Face3D>();

            List<Face3D> face3Ds_Temp = null;

            face3Ds_Temp = shell_1_Temp.Face3Ds;
            if(face3Ds_Temp == null || face3Ds_Temp.Count == 0)
            {
                return null;
            }

            face3Ds.AddRange(face3Ds_Temp);

            face3Ds_Temp = shell_2_Temp.Face3Ds;
            if (face3Ds_Temp == null || face3Ds_Temp.Count == 0)
            {
                return null;
            }

            face3Ds.AddRange(face3Ds_Temp);

            for(int i = face3Ds.Count - 1; i >= 0; i--)
            {
                Point3D point3D = face3Ds[i]?.GetCentroid();
                if (point3D == null)
                {
                    face3Ds.RemoveAt(i);
                    continue;
                }

                if(!boundingBox3D_1.InRange(point3D, tolerance_Distance) || !boundingBox3D_2.InRange(point3D, tolerance_Distance))
                {
                    face3Ds.RemoveAt(i);
                    continue;
                }

                if(!shell_1.Inside(point3D, silverSpacing, tolerance_Distance) && !shell_1.On(point3D, tolerance_Distance))
                {
                    face3Ds.RemoveAt(i);
                    continue;
                }

                if (!shell_2.Inside(point3D, silverSpacing, tolerance_Distance) && !shell_2.On(point3D, tolerance_Distance))
                {
                    face3Ds.RemoveAt(i);
                    continue;
                }
            }

            List<Shell> result = new List<Shell>();
            while(face3Ds.Count > 0)
            {
                Face3D face3D = face3Ds[0];
                face3Ds.RemoveAt(0);

                List<Face3D> face3Ds_Shell = face3D.ConnectedFace3Ds(face3Ds, tolerance_Angle, tolerance_Distance);
                if (face3Ds_Shell == null || face3Ds_Shell.Count == 0)
                {
                    continue;
                }

                Shell shell = Geometry.Spatial.Create.Shell(face3Ds_Shell, tolerance_Distance);
                if(shell == null)
                {
                    continue;
                }

                face3Ds.RemoveAll(x => face3Ds_Shell.Contains(x));
                result.Add(shell);
            }

            return result;
        }
    }
}