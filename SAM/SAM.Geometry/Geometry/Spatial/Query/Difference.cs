using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SAM.Geometry.Spatial
{
    public static partial class Query
    {
        /// <summary>
        /// Difference of two shells
        /// </summary>
        /// <param name="shell_1">Shell to be substracted from</param>
        /// <param name="shell_2">Shell will be substracted</param>
        /// <param name="silverSpacing">Silver Spacing</param>
        /// <param name="tolerance_Angle">Angle tolerance</param>
        /// <param name="tolerance_Distance">Distance Tolerance</param>
        /// <returns>Shells</returns>
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

            Task task_1 = Task.Factory.StartNew(() => shell_1_Temp.SplitFace3Ds(shell_2, silverSpacing, tolerance_Angle, tolerance_Distance));
            Task task_2 = Task.Factory.StartNew(() => shell_2_Temp.SplitFace3Ds(shell_1, silverSpacing, tolerance_Angle, tolerance_Distance));
            Task.WaitAll(task_1, task_2);

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

            List<Face3D> face3Ds_Shell_1 = shell_1.Face3Ds;
            System.Threading.Tasks.Parallel.For(0, face3Ds.Count, (int i) => 
            {
                face3Ds[i] = face3Ds[i].Snap(face3Ds_Shell_1, tolerance_Distance, tolerance_Distance);
            });

            List<Shell> result = new List<Shell>();
            while(face3Ds.Count >= 3)
            {
                Face3D face3D = face3Ds[0];
                List<Face3D> face3Ds_Shell = face3D?.ConnectedFace3Ds(face3Ds, tolerance_Angle, silverSpacing);
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

        /// <summary>
        /// Shells difference (shells substracted from shell)
        /// </summary>
        /// <param name="shell">Shell to be substracted from</param>
        /// <param name="shells">Shells will be substracted</param>
        /// <param name="silverSpacing">Silver Spacing</param>
        /// <param name="tolerance_Angle">Angle tolerance</param>
        /// <param name="tolerance_Distance">Distance Tolerance</param>
        /// <returns>Shells</returns>
        public static List<Shell> Difference(this Shell shell, IEnumerable<Shell> shells, double silverSpacing = Core.Tolerance.MacroDistance, double tolerance_Angle = Core.Tolerance.Angle, double tolerance_Distance = Core.Tolerance.Distance)
        {
            if(shell == null || shells == null)
            {
                return null;
            }

            List<Shell> result = new List<Shell>() { shell };
            foreach(Shell shell_Temp in shells)
            {
                List<Shell> result_New = new List<Shell>();
                foreach(Shell shell_Result in result)
                {
                    List<Shell> shells_Difference = Difference(shell_Result, shell_Temp, silverSpacing, tolerance_Angle, tolerance_Distance);
                    if(shells_Difference != null && shells_Difference.Count != 0)
                    {
                        result_New.AddRange(shells_Difference);
                    }
                }

                result = result_New;
                if(result == null || result.Count == 0)
                {
                    break;
                }
            }

            return result;
        }

        public static List<Face3D> Difference(this Face3D face3D, IEnumerable<Face3D> face3Ds, double tolerance_Angle = Core.Tolerance.Angle, double tolerance_Distance = Core.Tolerance.Distance)
        {
            if(face3D == null || face3Ds == null)
            {
                return null;
            }

            PlanarIntersectionResult planarIntersectionResult = Create.PlanarIntersectionResult(face3D, face3Ds, tolerance_Angle, tolerance_Distance);
            if(planarIntersectionResult == null || !planarIntersectionResult.Intersecting)
            {
                return new List<Face3D>() { new Face3D(face3D) };
            }

            List<Planar.Face2D> face2Ds = planarIntersectionResult.GetGeometry2Ds<Planar.Face2D>();
            if (face2Ds == null || face2Ds.Count == 0)
            {
                return null;
            }

            Plane plane = planarIntersectionResult.Plane;
            if (plane == null)
            {
                return null;
            }

            face2Ds = Planar.Query.Difference(plane.Convert(face3D), face2Ds, tolerance_Distance);
            if (face2Ds == null || face2Ds.Count == 0)
            {
                return null;
            }

            return face2Ds.ConvertAll(x => plane.Convert(x));
        }
    }
}