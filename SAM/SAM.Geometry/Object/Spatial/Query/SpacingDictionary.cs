using SAM.Geometry.Spatial;
using System.Collections.Generic;
using System;

namespace SAM.Geometry.Object.Spatial
{
    public static partial class Query
    {
        public static Dictionary<Point3D, List<T>> SpacingDictionary<T>(this IEnumerable<T> face3DObjects, double maxTolerance = Core.Tolerance.MacroDistance, double minTolerance = Core.Tolerance.MicroDistance) where T : IFace3DObject
        {
            if (face3DObjects == null)
            {
                return null;
            }

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

            Dictionary<Point3D, List<Face3D>> dictionary = Geometry.Spatial.Query.SpacingDictionary(tuples.ConvertAll(x => x.Item2), maxTolerance, minTolerance);
            if (dictionary == null)
            {
                return null;
            }

            Dictionary<Point3D, List<T>> result = new Dictionary<Point3D, List<T>>();
            foreach (KeyValuePair<Point3D, List<Face3D>> keyValuePair in dictionary)
            {
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
    }
}