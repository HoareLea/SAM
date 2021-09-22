using SAM.Core;
using System;
using System.Collections.Generic;

namespace SAM.Geometry.Spatial
{
    public static partial class Query
    {
        /// <summary>
        /// Intersection of tw lines represented by vector3D (direction) and Point3D (origin). Source: https://github.com/arakis/Net3dBool/blob/39354914eba2f9d34aedd2a16a6528d50e19beec/src/Net3dBool/Line.cs#L46
        /// </summary>
        /// <param name="point3D_1">Origin point3D of first line</param>
        /// <param name="vector3D_1">Direction Vector3D of first line</param>
        /// <param name="point3D_2">Origin point3D of second line</param>
        /// <param name="vector3D_2">Direction Vector3D of second line</param>
        /// <param name="tolerance">Tolerance</param>
        /// <returns></returns>
        public static Point3D Intersection(this Point3D point3D_1, Vector3D vector3D_1, Point3D point3D_2, Vector3D vector3D_2, double tolerance = Tolerance.Distance)
        {
            //x = x1 + a1*t = x2 + b1*s
            //y = y1 + a2*t = y2 + b2*s
            //z = z1 + a3*t = z2 + b3*s

            if (point3D_1 == null || vector3D_1 == null || point3D_2 == null || vector3D_2 == null)
                return null;

            double t;
            if (System.Math.Abs(vector3D_1.Y * vector3D_2.X - vector3D_1.X * vector3D_2.Y) > tolerance)
            {
                t = (-point3D_1.Y * vector3D_2.X + point3D_2.Y * vector3D_2.X + vector3D_2.Y * point3D_1.X - vector3D_2.Y * point3D_2.X) / (vector3D_1.Y * vector3D_2.X - vector3D_1.X * vector3D_2.Y);
            }
            else if (System.Math.Abs(-vector3D_1.X * vector3D_2.Z + vector3D_1.Z * vector3D_2.X) > tolerance)
            {
                t = -(-vector3D_2.Z * point3D_1.X + vector3D_2.Z * point3D_2.X + vector3D_2.X * point3D_1.Z - vector3D_2.X * point3D_2.Z) / (-vector3D_1.X * vector3D_2.Z + vector3D_1.Z * vector3D_2.X);
            }
            else if (System.Math.Abs(-vector3D_1.Z * vector3D_2.Y + vector3D_1.Y * vector3D_2.Z) > tolerance)
            {
                t = (point3D_1.Z * vector3D_2.Y - point3D_2.Z * vector3D_2.Y - vector3D_2.Z * point3D_1.Y + vector3D_2.Z * point3D_2.Y) / (-vector3D_1.Z * vector3D_2.Y + vector3D_1.Y * vector3D_2.Z);
            }
            else
            {
                return null;
            }

            double x = point3D_1.X + vector3D_1.X * t;
            double y = point3D_1.Y + vector3D_1.Y * t;
            double z = point3D_1.Z + vector3D_1.Z * t;

            return new Point3D(x, y, z);
        }

        public static List<Shell> Intersection(this Shell shell_1, Shell shell_2, double silverSpacing = Tolerance.MacroDistance, double tolerance_Angle = Tolerance.Angle, double tolerance_Distance = Tolerance.Distance)
        {
            if (shell_1 == null || shell_2 == null)
            {
                return null;
            }

            Shell shell_1_Temp = new Shell(shell_1);
            Shell shell_2_Temp = new Shell(shell_2);

            BoundingBox3D boundingBox3D_1 = shell_1.GetBoundingBox();
            BoundingBox3D boundingBox3D_2 = shell_2.GetBoundingBox();

            if (!boundingBox3D_1.InRange(boundingBox3D_2, tolerance_Distance) || !shell_1.IsClosed(silverSpacing) || !shell_2.IsClosed(silverSpacing))
            {
                return new List<Shell>();
            }

            System.Threading.Tasks.Task task_1 = System.Threading.Tasks.Task.Factory.StartNew(() => shell_1_Temp.SplitFace3Ds(shell_2, silverSpacing, tolerance_Angle, tolerance_Distance));
            System.Threading.Tasks.Task task_2 = System.Threading.Tasks.Task.Factory.StartNew(() => shell_2_Temp.SplitFace3Ds(shell_1, silverSpacing, tolerance_Angle, tolerance_Distance));
            System.Threading.Tasks.Task.WaitAll(task_1, task_2);

            List<Tuple<BoundingBox3D, Face3D>> boundaries_1 = shell_1_Temp.Boundaries;
            List<Tuple<BoundingBox3D, Face3D>> boundaries_2 = shell_2_Temp.Boundaries;

            bool inside = false;
            for (int i = boundaries_1.Count - 1; i >= 0; i--)
            {
                if(!boundingBox3D_2.InRange(boundaries_1[i].Item1, tolerance_Distance))
                {
                    boundaries_1.RemoveAt(i);
                    continue;
                }

                Face3D face3D = boundaries_1[i].Item2;
                Point3D point3D = face3D.InternalPoint3D(tolerance_Distance);

                if (!boundingBox3D_2.InRange(point3D, tolerance_Distance))
                {
                    boundaries_1.RemoveAt(i);
                    continue;
                }

                List<Tuple<BoundingBox3D, Face3D>> boundaries_On = boundaries_2.FindAll(x => x.Item1.InRange(point3D, tolerance_Distance) && x.Item2.On(point3D, tolerance_Distance));
                if(boundaries_On == null || boundaries_On.Count == 0)
                {
                    if(shell_2.Inside(point3D, silverSpacing, tolerance_Distance))
                    {
                        inside = true;
                    }
                    else
                    {
                        boundaries_1.RemoveAt(i);
                    }

                    continue;
                }

                boundaries_On.ForEach(x => boundaries_2.Remove(x));
            }

            for (int i = boundaries_2.Count - 1; i >= 0; i--)
            {
                if (!boundingBox3D_1.InRange(boundaries_2[i].Item1, tolerance_Distance))
                {
                    boundaries_2.RemoveAt(i);
                    continue;
                }

                Face3D face3D = boundaries_2[i].Item2;
                Point3D point3D = face3D.InternalPoint3D(tolerance_Distance);

                if (!boundingBox3D_1.InRange(point3D, tolerance_Distance))
                {
                    boundaries_2.RemoveAt(i);
                    continue;
                }

                List<Tuple<BoundingBox3D, Face3D>> boundaries_On = boundaries_1.FindAll(x => x.Item1.InRange(point3D, tolerance_Distance) && x.Item2.On(point3D, tolerance_Distance));
                if (boundaries_On == null || boundaries_On.Count == 0)
                {
                    if (shell_1.Inside(point3D, silverSpacing, tolerance_Distance))
                    {
                        inside = true;
                    }
                    else
                    {
                        boundaries_2.RemoveAt(i);
                    }
                    continue;
                }

                boundaries_On.ForEach(x => boundaries_1.Remove(x));
            }

            if(!inside)
            {
                boundaries_1 = shell_1_Temp.Boundaries;
                boundaries_2 = shell_2_Temp.Boundaries;

                foreach(Tuple<BoundingBox3D, Face3D> boundary in boundaries_1)
                {
                    Point3D point3D = boundary.Item2.InternalPoint3D(tolerance_Distance);
                    List<Tuple<BoundingBox3D, Face3D>> boundaries_2_Temp = boundaries_2.FindAll(x => x.Item1.InRange(point3D, tolerance_Distance) && x.Item2.On(point3D, tolerance_Distance));
                    if(boundaries_2_Temp == null || boundaries_2_Temp.Count == 0)
                    {
                        return new List<Shell>();
                    }
                }

                foreach (Tuple<BoundingBox3D, Face3D> boundary in boundaries_2)
                {
                    Point3D point3D = boundary.Item2.InternalPoint3D(tolerance_Distance);
                    List<Tuple<BoundingBox3D, Face3D>> boundaries_1_Temp = boundaries_1.FindAll(x => x.Item1.InRange(point3D, tolerance_Distance) && x.Item2.On(point3D, tolerance_Distance));
                    if (boundaries_1_Temp == null || boundaries_1_Temp.Count == 0)
                    {
                        return new List<Shell>();
                    }
                }

                return new List<Shell>() { new Shell(shell_1) };
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

            if (face3Ds.Count < 3)
            {
                return null;
            }

            face3Ds = face3Ds.ConvertAll(x => x.Snap(shell_1.Face3Ds, tolerance_Distance, tolerance_Distance));
            face3Ds = face3Ds.ConvertAll(x => x.Snap(shell_2.Face3Ds, tolerance_Distance, tolerance_Distance));

            List<Shell> result = new List<Shell>();
            while (face3Ds.Count >= 3)
            {
                Face3D face3D = face3Ds[0];
                List<Face3D> face3Ds_Shell = face3D?.ConnectedFace3Ds(face3Ds, tolerance_Angle, tolerance_Distance);
                face3Ds.RemoveAt(0);

                if (face3Ds_Shell != null && face3Ds_Shell.Count > 2)
                {
                    Shell shell = Create.Shell(face3Ds_Shell, silverSpacing, tolerance_Distance);
                    if (shell != null)
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