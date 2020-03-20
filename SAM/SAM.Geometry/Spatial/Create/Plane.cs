using System;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Geometry.Spatial
{
    public static partial class Create
    {
        public static Plane Plane(this IEnumerable<Point3D> point3Ds, bool fit, double tolerance = Core.Tolerance.MicroDistance)
        {
            if (point3Ds == null || point3Ds.Count() < 3)
                return null;

            if (!fit)
                return Plane(point3Ds, tolerance);

            Point3D point3D_Centroid = Query.Centroid(point3Ds);

            double xx = 0;
            double yy = 0;
            double xy = 0;
            double yz = 0;
            double xz = 0;
            double zz = 0;

            foreach(Point3D point3D in point3Ds)
            {
                if (point3D == null)
                    continue;
                
                Vector3D vector3D = point3D - point3D_Centroid;
                xx += vector3D.X * vector3D.X;
                xy += vector3D.X * vector3D.Y;
                xz += vector3D.X * vector3D.Z;
                yy += vector3D.Y * vector3D.Y;
                yz += vector3D.Y * vector3D.Z;
                zz += vector3D.Z * vector3D.Z;
            }

            double[] det = new double[] { yy * zz - yz * yz, xx * zz - xz * xz, xx * yy - xy * xy };

            double det_Max = det.Max();

            if (det_Max <= 0)
                return null;

            Vector3D vector3D_Normal;
            if (det[0].Equals(det_Max))
                vector3D_Normal = new Vector3D(det_Max, xz * yz - xy * zz, xy * yz - xz * yy);
            else if (det[1].Equals(det_Max))
                vector3D_Normal = new Vector3D(xz * yz - xy * zz, det_Max, xy * xz - yz * xx);
            else
                vector3D_Normal = new Vector3D(xy * yz - xz * yy, xy * xz - yz * xx, det_Max);

            return new Plane(point3D_Centroid, vector3D_Normal);
        }

        public static Plane Plane(this IEnumerable<Point3D> point3Ds, double tolerance = Core.Tolerance.MicroDistance)
        {
            Vector3D normal = Query.Normal(point3Ds, tolerance);
            if (normal == null)
                return null;

            return new Plane(point3Ds.ElementAt(0), normal.Unit);
        }

        //public static Plane Plane(this IEnumerable<Point3D> point3Ds, bool useMode, bool closed, double tolerance = Core.Tolerance.MicroDistance)
        //{
        //    if (point3Ds == null)
        //        return null;

        //    if (!useMode)
        //        return Plane(point3Ds, tolerance);

        //    List<Point3D> point3Ds_Temp = new List<Point3D>(point3Ds);
        //    Modify.SimplifyByAngle(point3Ds_Temp, closed);

        //    List<Vector3D> vector3Ds = Query.Normals(point3Ds_Temp);
        //    if (vector3Ds == null || vector3Ds.Count == 0)
        //        return null;

        //    int count = vector3Ds.Count;
        //    if (count == 0)
        //        return null;

        //    Dictionary<int, Tuple<Vector3D, Point3D>> dictionary = new Dictionary<int, Tuple<Vector3D, Point3D>>();
        //    List<int> hashSets = new List<int>();
        //    for (int i = 0; i < count; i++)
        //    {
        //        Vector3D vector3D = vector3Ds[i];
        //        if (!Query.IsValid(vector3D))
        //            continue;

        //        int hashSet = vector3D.GetHashCode();

        //        hashSets.Add(hashSet);

        //        if (!dictionary.ContainsKey(hashSet))
        //            dictionary[hashSet] = new Tuple<Vector3D, Point3D>(vector3D, point3Ds_Temp[i]);
        //    }

        //    if (dictionary == null || dictionary.Count() == 0)
        //        return null;

        //    Tuple<Vector3D, Point3D> tuple = null;
        //    if (dictionary.Count != 1)
        //    {
        //        int hashSet_Modal = Math.Query.Modal(hashSets);
        //        tuple = dictionary[hashSet_Modal];
        //    }
        //    else
        //    {
        //        tuple = dictionary.First().Value;
        //    }

        //    return new Plane(tuple.Item2, tuple.Item1);
        //}
    }
}
