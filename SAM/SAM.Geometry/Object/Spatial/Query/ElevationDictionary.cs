using SAM.Geometry.Spatial;
using System.Collections.Generic;
using System;

namespace SAM.Geometry.Object.Spatial
{
    public static partial class Query
    {
        public static Dictionary<double, List<T>> ElevationDictionary<T>(this IEnumerable<T> face3DObjects, out double maxElevation, double tolerance = Core.Tolerance.Distance) where T : IFace3DObject
        {
            List<Tuple<T, Face3D>> tuples = new List<Tuple<T, Face3D>>();
            foreach (T face3DObject in face3DObjects)
            {
                Face3D face3D = face3DObject?.Face3D;
                if (face3D == null)
                {
                    continue;
                }

                tuples.Add(new Tuple<T, Face3D>(face3DObject, face3D));
            }

            Dictionary<double, List<Face3D>> dictionary = Geometry.Spatial.Query.ElevationDictionary(tuples.ConvertAll(x => x.Item2), out maxElevation, tolerance);
            if (dictionary == null)
            {
                return null;
            }

            Dictionary<double, List<T>> result = new Dictionary<double, List<T>>();
            foreach (KeyValuePair<double, List<Face3D>> keyValuePair in dictionary)
            {
                if (keyValuePair.Value == null)
                {
                    continue;
                }

                List<T> ts = new List<T>();
                foreach (Face3D face3D in keyValuePair.Value)
                {
                    int index = tuples.FindIndex(x => x.Item2 == face3D);
                    if (index == -1)
                    {
                        continue;
                    }

                    ts.Add(tuples[index].Item1);
                }

                result[keyValuePair.Key] = ts;

            }

            return result;

        }

        public static Dictionary<double, List<T>> ElevationDictionary<T>(this IEnumerable<T> face3DObjects, double tolerance = Core.Tolerance.Distance) where T : IFace3DObject
        {
            return ElevationDictionary(face3DObjects, out double maxElevation, tolerance);
        }
    }
}