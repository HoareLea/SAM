using SAM.Geometry.Spatial;
using System;
using System.Collections.Generic;

namespace SAM.Geometry.Object.Spatial
{
    public static partial class Query
    {
        public static Dictionary<N, List<T>> SectionDictionary<N, T>(this IEnumerable<N> face3DObjects, Plane plane, double tolerance_Angle = Core.Tolerance.Angle, double tolerance_Distance = Core.Tolerance.Distance) where T: ISAMGeometry where N: IFace3DObject
        {
            if (plane == null || face3DObjects == null)
            {
                return null;
            }

            List<Tuple<N, Face3D>> tuples = new List<Tuple<N, Face3D>>();
            foreach (N face3DObject in face3DObjects)
            {
                Face3D face3D = face3DObject?.Face3D;
                if(face3D == null)
                {
                    continue;
                }

                tuples.Add(new Tuple<N, Face3D>(face3DObject, face3D));
            }

            Dictionary<Face3D, List<T>> dictionary = Geometry.Spatial.Query.SectionDictionary<T>(tuples.ConvertAll(x => x.Item2), plane, tolerance_Angle, tolerance_Distance);
            if(dictionary ==null)
            {
                return null;
            }

            Dictionary<N, List<T>> result = new Dictionary<N, List<T>>();
            foreach(KeyValuePair<Face3D, List<T>> keyValuePair in dictionary)
            {
                if(keyValuePair.Key == null)
                {
                    continue;
                }

                int index = tuples.FindIndex(x => x.Item2 == keyValuePair.Key);
                if(index == -1)
                {
                    continue;
                }

                result[tuples[index].Item1] = keyValuePair.Value;
            }

            return result;
        }
    }
}