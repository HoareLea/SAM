using SAM.Geometry.Spatial;
using System;
using System.Collections.Generic;

namespace SAM.Geometry.Spatial
{
    public static partial class Query
    {
        public static Dictionary<double, List<T>> ElevationDictionary<T>(this IEnumerable<T> face3DObjects, out double maxElevation, double tolerance = Core.Tolerance.Distance) where T: IFace3DObject
        {
            maxElevation = double.NaN;
            
            if (face3DObjects == null)
                return null;

            List<Tuple<double, List<T>>> tuples_Elevation = new List<Tuple<double, List<T>>>();
            foreach (T face3DObject in face3DObjects)
            {
                BoundingBox3D boundingBox3D = face3DObject?.Face3D?.GetBoundingBox();
                if (boundingBox3D == null)
                    continue;

                double minElevation = boundingBox3D.Min.Z;
                Tuple<double, List<T>> tuple = tuples_Elevation.Find(x => System.Math.Abs(x.Item1 - minElevation) < tolerance);
                if (tuple == null)
                {
                    tuple = new Tuple<double, List<T>>(minElevation, new List<T>());
                    tuples_Elevation.Add(tuple);
                }

                double maxElevation_Temp = boundingBox3D.Max.Z;
                if(double.IsNaN(maxElevation) || maxElevation < maxElevation_Temp)
                {
                    maxElevation = maxElevation_Temp;
                }

                tuple.Item2.Add(face3DObject);
            }

            tuples_Elevation.Sort((x, y) => x.Item1.CompareTo(y.Item1));

            Dictionary<double, List<T>> result = new Dictionary<double, List<T>>();
            tuples_Elevation.ForEach(x => result[x.Item1] = x.Item2);

            return result;
        }

        public static Dictionary<double, List<T>> ElevationDictionary<T>(this IEnumerable<T> face3DObjects, double tolerance = Core.Tolerance.Distance) where T : IFace3DObject
        {
            return ElevationDictionary(face3DObjects, out double maxElevation, tolerance);
        }
    }
}