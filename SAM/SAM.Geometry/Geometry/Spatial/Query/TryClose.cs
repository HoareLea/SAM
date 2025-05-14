using System;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Geometry.Spatial
{
    public static partial class Query
    {
        public static bool TryClose(this Shell shell, out Shell shell_Closed, double tolerance_SilverSpacing = Core.Tolerance.MacroDistance, double tolerance = Core.Tolerance.Distance)
        {
            shell_Closed = null;

            if(shell == null)
            {
                return false;
            }

            if(shell.IsClosed(tolerance))
            {
                shell_Closed = (Shell)shell.Clone();
                return true;
            }

            List<Face3D> face3Ds = shell.Face3Ds;
            if(face3Ds == null || face3Ds.Count < 3)
            {
                return false;
            }

            shell_Closed = AveragedPoints(shell, true, tolerance_SilverSpacing, tolerance);
            if(!shell_Closed.IsClosed(tolerance))
            {
                shell_Closed = AveragedPoints(shell_Closed, false, tolerance_SilverSpacing, tolerance);

                if (!shell_Closed.IsClosed(tolerance))
                {
                    shell_Closed = AveragedPoints(shell_Closed, false, tolerance_SilverSpacing, tolerance);
                }
            }

            return shell_Closed.IsClosed(tolerance);
        }


        public static Shell AveragedPoints(this Shell shell, bool distanceCheck = true, double tolerance_SilverSpacing = Core.Tolerance.MacroDistance, double tolerance = Core.Tolerance.Distance)
        {

            List<Face3D> face3Ds = shell?.Face3Ds;
            if (face3Ds == null || face3Ds.Count < 3)
            {
                return shell.Clone() as Shell;
            }

            List<Tuple<Face3D, List<Point3D>>> tuples = new List<Tuple<Face3D, List<Point3D>>>();

            List<Point3D> point3Ds = new List<Point3D>();
            foreach (Face3D face3D in face3Ds)
            {
                List<Point3D> point3Ds_Face3D = face3D?.GetExternalEdge3D()?.Point3Ds();
                if (point3Ds_Face3D == null)
                {
                    continue;
                }

                tuples.Add(new Tuple<Face3D, List<Point3D>>(face3D, point3Ds_Face3D));
                point3Ds.AddRange(point3Ds_Face3D);
            }

            Point3DCluster point3DCluster = new Point3DCluster(point3Ds.Distinct(), tolerance_SilverSpacing);
            List<List<Point3D>> point3DsList = point3DCluster.Combine();
            if (point3DsList == null || point3DsList.Count == 0)
            {
                return shell.Clone() as Shell;
            }

            if(distanceCheck)
            {
                for (int i = point3DsList.Count - 1; i >= 0; i--)
                {
                    ExtremePoints(point3DsList[i], out Point3D point3D_1, out Point3D point3D_2, out double dictance);
                    if (point3D_1 == null || point3D_2 == null || dictance <= tolerance)
                    {
                        point3DsList.RemoveAt(i);
                        continue;
                    }
                }

                if (point3DsList == null || point3DsList.Count == 0)
                {
                    return shell.Clone() as Shell;
                }
            }


            for (int i = point3DsList.Count - 1; i >= 0; i--)
            {
                point3Ds = point3DsList[i];

                Point3D point3D = point3Ds.Average();

                foreach (Tuple<Face3D, List<Point3D>> tuple in tuples)
                {
                    for (int j = 0; j < tuple.Item2.Count; j++)
                    {
                        if (point3Ds.Contains(tuple.Item2[j]))
                        {
                            tuple.Item2[j] = point3D;
                        }
                    }
                }
            }

            face3Ds = new List<Face3D>();

            foreach (Tuple<Face3D, List<Point3D>> tuple in tuples)
            {
                Plane plane = tuple.Item1.GetPlane();

                Face3D face3D = Face3D.Create(plane, new Planar.Polygon2D(tuple.Item2.ConvertAll(x => plane.Convert(x))), tuple.Item1.InternalEdge2Ds);
                face3Ds.Add(face3D);
            }

            return new Shell(face3Ds);
        }
    }
}