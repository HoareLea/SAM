using System;
using System.Collections.Generic;

namespace SAM.Geometry.Spatial
{
    public static partial class Query
    {
        public static Dictionary<double, List<Face3D>> ElevationDictionary(this IEnumerable<Face3D> face3Ds, out double maxElevation, double tolerance = Core.Tolerance.Distance)
        {
            maxElevation = double.NaN;

            if (face3Ds == null)
                return null;

            List<Tuple<double, List<Face3D>>> tuples_Elevation = new List<Tuple<double, List<Face3D>>>();
            foreach (Face3D face3D in face3Ds)
            {
                BoundingBox3D boundingBox3D = face3D.GetBoundingBox();
                if (boundingBox3D == null)
                    continue;

                double minElevation = boundingBox3D.Min.Z;
                Tuple<double, List<Face3D>> tuple = tuples_Elevation.Find(x => System.Math.Abs(x.Item1 - minElevation) < tolerance);
                if (tuple == null)
                {
                    tuple = new Tuple<double, List<Face3D>>(minElevation, new List<Face3D>());
                    tuples_Elevation.Add(tuple);
                }

                double maxElevation_Temp = boundingBox3D.Max.Z;
                if (double.IsNaN(maxElevation) || maxElevation < maxElevation_Temp)
                {
                    maxElevation = maxElevation_Temp;
                }

                tuple.Item2.Add(face3D);
            }

            tuples_Elevation.Sort((x, y) => x.Item1.CompareTo(y.Item1));

            Dictionary<double, List<Face3D>> result = new Dictionary<double, List<Face3D>>();
            tuples_Elevation.ForEach(x => result[x.Item1] = x.Item2);

            return result;
        }

        public static Dictionary<double, List<T>> ElevationDictionary<T>(this IEnumerable<T> face3DObjects, out double maxElevation, double tolerance = Core.Tolerance.Distance) where T: IFace3DObject
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

            Dictionary<double, List<Face3D>> dictionary = ElevationDictionary(tuples.ConvertAll(x => x.Item2), out maxElevation, tolerance);
            if (dictionary == null)
            {
                return null;
            }

            Dictionary<double, List<T>> result = new Dictionary<double, List<T>>();
            foreach (KeyValuePair<double, List<Face3D>> keyValuePair in dictionary)
            {
                if(keyValuePair.Value == null)
                {
                    continue;
                }

                List<T> ts = new List<T>();
                foreach(Face3D face3D in keyValuePair.Value)
                {
                    int index = tuples.FindIndex(x => x.Item2 == face3D);
                    if(index == -1)
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

        public static Dictionary<double, List<Face3D>> ElevationDictionary(this IEnumerable<Face3D> face3Ds, double tolerance = Core.Tolerance.Distance)
        {
            return ElevationDictionary(face3Ds, out double maxElevation, tolerance);
        }
    }
}