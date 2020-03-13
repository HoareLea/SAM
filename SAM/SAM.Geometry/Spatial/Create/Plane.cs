using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Geometry.Spatial
{
    public static partial class Create
    {
        public static Plane Plane(this IEnumerable<Point3D> point3Ds, bool useMode, double tolerance = Core.Tolerance.MicroDistance)
        {
            if (point3Ds == null)
                return null;

            if (!useMode)
                return Plane(point3Ds, tolerance);

            List<Vector3D> vector3Ds = Query.Normals(point3Ds);
            if (vector3Ds == null || vector3Ds.Count == 0)
                return null;

            int count = vector3Ds.Count;
            if (count == 0)
                return null;

            Dictionary<int, Tuple<Vector3D, Point3D>> dictionary = new Dictionary<int, Tuple<Vector3D, Point3D>>();
            for (int i=0; i < count; i++)
            {
                Vector3D vector3D = vector3Ds[i];
                if (vector3D == null)
                    continue;

                dictionary[vector3D.GetHashCode()] = new Tuple<Vector3D, Point3D>(vector3D, point3Ds.ElementAt(i));
            }

            if (dictionary == null || dictionary.Count() == 0)
                return null;

            int hashSet = Math.Query.Modal(dictionary.Keys);
            
            if (!dictionary.ContainsKey(hashSet))
                return null;

            Tuple<Vector3D, Point3D> tuple = dictionary[hashSet];
            return new Plane(tuple.Item2, tuple.Item1);
        }

        public static Plane Plane(this IEnumerable<Point3D> point3Ds, double tolerance = Core.Tolerance.MicroDistance)
        {
            Vector3D normal = Query.Normal(point3Ds, tolerance);
            if (normal == null)
                return null;

            return new Plane(point3Ds.ElementAt(0), normal.Unit);
        }

    }
}
