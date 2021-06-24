using System;
using System.Collections.Generic;
using System.Linq;
using SAM.Geometry.Planar;

namespace SAM.Geometry.Spatial
{
    public static partial class Query
    {
        public static List<Shell> Union(this Shell shell_1, Shell shell_2, double silverSpacing = Core.Tolerance.MacroDistance, double tolerance_Angle = Core.Tolerance.Angle, double tolerance_Distance = Core.Tolerance.Distance)
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

            List<Face3D> face3Ds_New = boundaries_1.ConvertAll(x => x.Item2);
            face3Ds_New.AddRange(boundaries_2.ConvertAll(x => x.Item2));

            //Snap Face3Ds to origin
            List<Face3D> face3Ds = shell_1.Face3Ds;
            face3Ds.AddRange(shell_2.Face3Ds);

            face3Ds_New = face3Ds_New.ConvertAll(x => x.Snap(face3Ds, tolerance_Distance, tolerance_Distance));

            return new List<Shell>() { new Shell(face3Ds_New) };

        }

        public static List<Shell> Union(this IEnumerable<Shell> shells, double silverSpacing = Core.Tolerance.MacroDistance, double tolerance_Angle = Core.Tolerance.Angle, double tolerance_Distance = Core.Tolerance.Distance)
        {
            if (shells == null)
            {
                return null;
            }

            List<Shell> shells_Temp = new List<Shell>(shells);
            if (shells_Temp.Count == 0)
            {
                return new List<Shell>();
            }

            List<Shell> result = new List<Shell>();

            if (shells_Temp.Count == 1)
            {
                result.Add(new Shell(shells_Temp[0]));
                return result;
            }

            while (shells_Temp.Count > 0)
            {
                Shell shell = shells_Temp[0];
                shells_Temp.RemoveAt(0);

                List<Shell> shells_Union = null;
                foreach (Shell shell_Result in result)
                {
                    shells_Union = shell_Result.Union(shell);
                    if (shells_Union != null && shells_Union.Count == 1)
                    {
                        result.Remove(shell_Result);
                        break;
                    }
                }

                if (shells_Union != null && shells_Union.Count == 1)
                {
                    result.Add(shells_Union[0]);
                }
                else
                {
                    result.Add(new Shell(shell));
                }
            }

            if(result.Count != shells.Count())
            {
                result = Union(result, silverSpacing, tolerance_Angle, tolerance_Distance);
            }

            return result;
        }
    
        public static List<Face3D> Union(this IEnumerable<Face3D> face3Ds, double tolerance = Core.Tolerance.Distance)
        {
            if(face3Ds == null)
            {
                return null;
            }

            List<Face3D> result = new List<Face3D>();
            if(face3Ds.Count() == 0)
            {
                return result;
            }

            List<Face3D> face3Ds_Temp = new List<Face3D>(face3Ds);
            face3Ds_Temp.RemoveAll(x => x == null || !x.IsValid());

            while(face3Ds_Temp.Count > 0)
            {
                Face3D face3D_Temp = face3Ds_Temp[0];

                Plane plane = face3D_Temp.GetPlane();
                if(plane == null)
                {
                    face3Ds_Temp.RemoveAt(0);
                    continue;
                }

                List<Face3D> face3Ds_Coplanar = face3Ds_Temp.FindAll(x => plane.Coplanar(x.GetPlane(), tolerance));
                face3Ds_Coplanar.ForEach(x => face3Ds_Temp.Remove(x));

                switch(face3Ds_Coplanar.Count)
                {
                    case 0:
                        return null;

                    case 1:
                        result.Add(face3Ds_Coplanar[0]);
                        break;

                    default:

                        List<Face2D> face2Ds = face3Ds_Coplanar.ConvertAll(x => plane.Convert(x)).Union(tolerance);
                        if(face2Ds == null || face2Ds.Count == 0)
                        {
                            result.AddRange(face3Ds_Coplanar);
                        }
                        else
                        {
                            foreach(Face2D face2D in face2Ds)
                            {
                                if(face2D == null || !face2D.IsValid())
                                {
                                    continue;
                                }

                                result.Add(plane.Convert(face2D));
                            }
                        }

                        break;
                }

            }

            return result;
        }
    }
}