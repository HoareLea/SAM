using System;
using System.Collections.Generic;
using System.Linq;

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


            if (!shell_1.GetBoundingBox().InRange(shell_2.GetBoundingBox(), tolerance_Distance))
            {
                return new List<Shell>() { shell_1_Temp, shell_2_Temp };
            }

            shell_1_Temp.Split(shell_2, tolerance_Angle, tolerance_Distance);
            shell_2_Temp.Split(shell_1, tolerance_Angle, tolerance_Distance);

            List<Tuple<BoundingBox3D, Face3D>> boundaries_1 = shell_1_Temp.Boundaries;
            List<Tuple<BoundingBox3D, Face3D>> boundaries_2 = shell_2_Temp.Boundaries;

            bool union = false;
            for (int i = boundaries_1.Count - 1; i >= 0; i--)
            {
                Face3D face3D = boundaries_1[i].Item2;
                Point3D point3D = face3D.InternalPoint3D(tolerance_Distance);
                Vector3D normal = null;

                List<Tuple<BoundingBox3D, Face3D>> boundaries_On = boundaries_2.FindAll(x => x.Item1.InRange(point3D, tolerance_Distance) && x.Item2.On(point3D, tolerance_Distance));
                if (boundaries_On != null && boundaries_On.Count != 0)
                {
                    boundaries_On.ForEach(x => boundaries_2.Remove(x));
                    if(normal == null)
                    {
                        normal = shell_1_Temp.Normal(i, true, silverSpacing, tolerance_Distance);
                    }

                    for (int j = boundaries_On.Count - 1; j >= 0; j--)
                    {
                        Vector3D normal_Temp = shell_2_Temp.Normal(boundaries_On[j].Item2.InternalPoint3D(tolerance_Distance), true, silverSpacing, tolerance_Distance);
                        if(!normal.SameHalf(normal_Temp))
                        {
                            boundaries_1.Remove(boundaries_1[i]);
                            break;
                        }
                    }

                    union = true;
                }
                else if (shell_2.Inside(point3D, silverSpacing, tolerance_Distance))
                {
                    boundaries_1.RemoveAt(i);
                    union = true;
                }
            }

            for (int i = boundaries_2.Count - 1; i >= 0; i--)
            {
                Face3D face3D = boundaries_2[i].Item2;
                Point3D point3D = face3D.InternalPoint3D(tolerance_Distance);

                if (shell_1.Inside(point3D, silverSpacing, tolerance_Distance))
                {
                    boundaries_2.RemoveAt(i);
                    union = true;
                }
            }

            if (!union)
            {
                return new List<Shell>() { shell_1_Temp, shell_2_Temp };
            }

            List<Face3D> face3Ds = boundaries_1.ConvertAll(x => x.Item2);
            face3Ds.AddRange(boundaries_2.ConvertAll(x => x.Item2));

            return new List<Shell>() { new Shell(face3Ds) };

        }
    }
}