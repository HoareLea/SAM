using SAM.Geometry.Spatial;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SAM.Analytical
{
    public static partial class Query
    {
        public static Dictionary<Panel, List<T>> SectionDictionary<T>(this IEnumerable<Panel> panels, Plane plane, double tolerance = Core.Tolerance.Distance) where T: Geometry.ISAMGeometry
        {
            if (plane == null || panels == null)
                return null;

            List<Tuple<Panel, List<T>>> tuples = new List<Tuple<Panel, List<T>>>();
            foreach(Panel panel in panels)
            {
                if(panel == null)
                {
                    continue;
                }

                tuples.Add(new Tuple<Panel, List<T>>(panel, new List<T>()));
            }

            if(tuples.Count == 0)
            {
                return null;
            }

            Parallel.For(0, tuples.Count, (int i) => 
            {
                Panel panel = tuples[i].Item1;
                
                Face3D face3D = panel?.GetFace3D();
                if (face3D == null)
                {
                    return;
                }

                PlanarIntersectionResult planarIntersectionResult = Geometry.Spatial.Create.PlanarIntersectionResult(plane, face3D, tolerance);
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
                else if (typeof(Geometry.Planar.ISAMGeometry2D).IsAssignableFrom(typeof(T)))
                {
                    List<Geometry.Planar.ISAMGeometry2D> sAMGeometry2Ds = planarIntersectionResult.Geometry2Ds;
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

            Dictionary<Panel, List<T>> result = new Dictionary<Panel, List<T>>();
            foreach(Tuple<Panel, List<T>> tuple in tuples)
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