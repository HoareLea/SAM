using System;
using System.Collections.Generic;

namespace SAM.Geometry.Spatial
{
    public static partial class Query
    {
        public static Dictionary<N, List<T>> SectionDictionary<N, T>(this IEnumerable<N> face3DObjects, Plane plane, double tolerance = Core.Tolerance.Distance) where T: ISAMGeometry where N: IFace3DObject
        {
            if (plane == null || face3DObjects == null)
                return null;

            List<Tuple<N, List<T>>> tuples = new List<Tuple<N, List<T>>>();
            foreach(N face3DObject in face3DObjects)
            {
                if(face3DObject == null)
                {
                    continue;
                }

                tuples.Add(new Tuple<N, List<T>>(face3DObject, new List<T>()));
            }

            if(tuples.Count == 0)
            {
                return null;
            }

            System.Threading.Tasks.Parallel.For(0, tuples.Count, (int i) => 
            {
                N n = tuples[i].Item1;
                
                Face3D face3D = n?.Face3D;
                if (face3D == null)
                {
                    return;
                }

                PlanarIntersectionResult planarIntersectionResult = Create.PlanarIntersectionResult(plane, face3D, tolerance);
                if (planarIntersectionResult == null || !planarIntersectionResult.Intersecting)
                {
                    return;
                }

                if (typeof(ISAMGeometry3D).IsAssignableFrom(typeof(T)))
                {
                    List<ISAMGeometry3D> sAMGeometry3Ds = planarIntersectionResult.Geometry3Ds;
                    if (sAMGeometry3Ds == null || sAMGeometry3Ds.Count == 0)
                    {
                        return;
                    }

                    List<T> sAMGeometries = new List<T>();

                    foreach (ISAMGeometry3D sAMGeometry3D in sAMGeometry3Ds)
                    {
                        if (sAMGeometry3D is T)
                        {
                            sAMGeometries.Add((T)sAMGeometry3D);
                        }
                    }

                    tuples[i].Item2.AddRange(sAMGeometries);
                }
                else if (typeof(Planar.ISAMGeometry2D).IsAssignableFrom(typeof(T)))
                {
                    List<Planar.ISAMGeometry2D> sAMGeometry2Ds = planarIntersectionResult.Geometry2Ds;
                    if (sAMGeometry2Ds == null || sAMGeometry2Ds.Count == 0)
                    {
                        return;
                    }

                    List<T> sAMGeometries = new List<T>();

                    foreach (Geometry.Planar.ISAMGeometry2D sAMGeometry2D in sAMGeometry2Ds)
                    {
                        if (sAMGeometry2D is T)
                        {
                            sAMGeometries.Add((T)sAMGeometry2D);
                        }
                    }

                    tuples[i].Item2.AddRange(sAMGeometries);
                }
            });

            Dictionary<N, List<T>> result = new Dictionary<N, List<T>>();
            foreach(Tuple<N, List<T>> tuple in tuples)
            {
                if(tuple.Item2 == null || tuple.Item2.Count == 0)
                {
                    continue;
                }

                result[tuple.Item1] = tuple.Item2;
            }

            return result;
        }
    }
}