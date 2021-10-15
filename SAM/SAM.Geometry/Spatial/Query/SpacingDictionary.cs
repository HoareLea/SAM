using System;
using System.Collections.Generic;

namespace SAM.Geometry.Spatial
{
    public static partial class Query
    {
        public static Dictionary<Point3D, List<T>> SpacingDictionary<T>(this IEnumerable<T> face3DObjects, double maxTolerance = Core.Tolerance.MacroDistance, double minTolerance = Core.Tolerance.MicroDistance) where T : IFace3DObject
        {
            if(face3DObjects == null)
            {
                return null;
            }

            List<Tuple<T, Face3D>> tuples = new List<Tuple<T, Face3D>>();
            foreach(T face3DObject in face3DObjects)
            {
                Face3D face3D = face3DObject?.Face3D;
                if(face3D == null)
                {
                    continue;
                }

                tuples.Add(new Tuple<T, Face3D>(face3DObject, face3D));
            }

            Dictionary<Point3D, List<Face3D>> dictionary = SpacingDictionary(tuples.ConvertAll(x => x.Item2), maxTolerance, minTolerance);
            if(dictionary == null)
            {
                return null;
            }

            Dictionary<Point3D, List<T>> result = new Dictionary<Point3D, List<T>>();
            foreach(KeyValuePair<Point3D, List<Face3D>> keyValuePair in dictionary)
            {
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
        
        public static Dictionary<Point3D, List<Face3D>> SpacingDictionary(this IEnumerable<Face3D> face3Ds, double maxTolerance = Core.Tolerance.MacroDistance, double minTolerance = Core.Tolerance.MicroDistance)
        {
            List<Tuple<BoundingBox3D, Face3D, List<Point3D>>> tuples = new List<Tuple<BoundingBox3D, Face3D, List<Point3D>>>();
            foreach (Face3D face3D in face3Ds)
            {
                if(face3D == null || !face3D.IsValid())
                {
                    continue;
                }

                tuples.Add(new Tuple<BoundingBox3D, Face3D, List<Point3D>>(face3D.GetBoundingBox(maxTolerance), face3D, Point3Ds(face3D, true, true)));
            }

            Dictionary<Point3D, List<Face3D>> result = new Dictionary<Point3D, List<Face3D>>();
            foreach (Tuple<BoundingBox3D, Face3D, List<Point3D>> tuple in tuples)
            {
                List<Tuple<BoundingBox3D, Face3D, List<Point3D>>> tuples_Temp = tuples.FindAll(x => x.Item1.Intersect(tuple.Item1) || x.Item1.Inside(tuple.Item1) || tuple.Item1.Inside(x.Item1));
                if (tuples_Temp == null || tuples_Temp.Count < 2)
                    continue;

                tuples_Temp.Remove(tuple);

                Face3D face3D = tuple.Item2;
                BoundingBox3D boundingBox3D = tuple.Item1;
                foreach (Tuple<BoundingBox3D, Face3D, List<Point3D>> tuple_Temp in tuples_Temp)
                {
                    foreach (Point3D point3D_Temp in tuple_Temp.Item3)
                    {
                        if (!boundingBox3D.Inside(point3D_Temp, true, minTolerance))
                            continue;

                        Point3D point3D = point3D_Temp;
                        foreach(Point3D point3D_Result in result.Keys)
                        {
                            if (point3D_Result.Distance(point3D_Temp) <= minTolerance)
                            {
                                point3D = point3D_Result;
                                break;
                            }
                        }

                        double distance = face3D.DistanceToEdges(point3D);
                        if (distance < maxTolerance && distance > minTolerance)
                        {
                            if (!result.TryGetValue(point3D, out List<Face3D> face3Ds_Temp))
                            {
                                face3Ds_Temp = new List<Face3D>();
                                result[point3D] = face3Ds_Temp;
                            }

                            if (!face3Ds_Temp.Contains(tuple_Temp.Item2))
                                face3Ds_Temp.Add(tuple_Temp.Item2);

                            if (!face3Ds_Temp.Contains(tuple.Item2))
                                face3Ds_Temp.Add(tuple.Item2);
                        }
                    }
                }
            }

            return result;
        }

        public static Dictionary<Point3D, List<Face3D>> SpacingDictionary(this Shell shell, double maxTolerance = Core.Tolerance.MacroDistance, double minTolerance = Core.Tolerance.MicroDistance)
        {
            List<Face3D> face3Ds = shell?.Face3Ds;
            if(face3Ds == null)
            {
                return null;
            }

            return SpacingDictionary(face3Ds, maxTolerance, minTolerance);
        }
    }
}