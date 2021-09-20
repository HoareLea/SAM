using System;
using System.Collections.Generic;

namespace SAM.Geometry.Spatial
{
    public static partial class Query
    {
        public static List<Shell> Difference(this Shell shell_1, Shell shell_2, double silverSpacing = Core.Tolerance.MacroDistance, double tolerance_Angle = Core.Tolerance.Angle, double tolerance_Distance = Core.Tolerance.Distance)
        {
            if (shell_1 == null || shell_2 == null)
            {
                return null;
            }

            Shell shell_1_Temp = new Shell(shell_1);
            Shell shell_2_Temp = new Shell(shell_2);

            if (!shell_1.GetBoundingBox().InRange(shell_2.GetBoundingBox(), tolerance_Distance) || !shell_1.IsClosed(silverSpacing) || !shell_2.IsClosed(silverSpacing))
            {
                return new List<Shell>() { shell_1_Temp };
            }

            System.Threading.Tasks.Task task_1 = System.Threading.Tasks.Task.Factory.StartNew(() => shell_1_Temp.Split(shell_2, tolerance_Angle, tolerance_Distance));
            System.Threading.Tasks.Task task_2 = System.Threading.Tasks.Task.Factory.StartNew(() => shell_2_Temp.Split(shell_1, tolerance_Angle, tolerance_Distance));
            System.Threading.Tasks.Task.WaitAll(task_1, task_2);

            List<Tuple<BoundingBox3D, Face3D>> boundaries_1 = shell_1_Temp.Boundaries;
            List<Tuple<BoundingBox3D, Face3D>> boundaries_2 = shell_2_Temp.Boundaries;

            bool difference = false;
            for (int i = boundaries_1.Count - 1; i >= 0; i--)
            {
                Face3D face3D = boundaries_1[i].Item2;
                Point3D point3D = face3D.InternalPoint3D(tolerance_Distance);

                List<Tuple<BoundingBox3D, Face3D>> boundaries_On = boundaries_2.FindAll(x => x.Item1.InRange(point3D, tolerance_Distance) && x.Item2.On(point3D, tolerance_Distance));
                if (boundaries_On != null && boundaries_On.Count != 0)
                {
                    boundaries_On.ForEach(x => boundaries_2.Remove(x));
                    boundaries_1.Remove(boundaries_1[i]);
                    difference = true;
                }
                else if (shell_2.Inside(point3D, silverSpacing, tolerance_Distance))
                {
                    boundaries_1.RemoveAt(i);
                    difference = true;
                }
            }

            for (int i = boundaries_2.Count - 1; i >= 0; i--)
            {
                Face3D face3D = boundaries_2[i].Item2;
                Point3D point3D = face3D.InternalPoint3D(tolerance_Distance);

                if (!shell_1.Inside(point3D, silverSpacing, tolerance_Distance))
                {
                    boundaries_2.RemoveAt(i);
                    difference = true;
                }
            }

            if (!difference)
            {
                return new List<Shell>() { shell_1_Temp };
            }

            List<Face3D> face3Ds = new List<Face3D>();
            if (boundaries_1 != null && boundaries_1.Count != 0)
            {
                face3Ds.AddRange(boundaries_1.ConvertAll(x => x.Item2));
            }

            if (boundaries_2 != null && boundaries_2.Count != 0)
            {
                face3Ds.AddRange(boundaries_2.ConvertAll(x => x.Item2));
            }

            if(face3Ds.Count < 3)
            {
                return null;
            }

            face3Ds = face3Ds.ConvertAll(x => x.Snap(shell_1.Face3Ds, tolerance_Distance, tolerance_Distance));

            List<Shell> result = new List<Shell>();
            while(face3Ds.Count >= 3)
            {
                Face3D face3D = face3Ds[0];
                List<Face3D> face3Ds_Shell = face3D?.ConnectedFace3Ds(face3Ds, tolerance_Angle, tolerance_Distance);
                face3Ds.RemoveAt(0);
                
                if(face3Ds_Shell != null && face3Ds_Shell.Count > 2)
                {
                    Shell shell = Create.Shell(face3Ds_Shell, silverSpacing, tolerance_Distance);
                    if(shell != null)
                    {
                        result.Add(shell);
                        face3Ds.RemoveAll(x => face3Ds_Shell.Contains(x));
                    }
                }
            }

            return result;
        }
    }
}