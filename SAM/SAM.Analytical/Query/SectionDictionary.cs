using SAM.Geometry.Spatial;
using System.Collections.Generic;

namespace SAM.Analytical
{
    public static partial class Query
    {
        public static Dictionary<Panel, List<T>> SectionDictionary<T>(this IEnumerable<Panel> panels, Plane plane, double tolerance = Core.Tolerance.Distance) where T: Geometry.ISAMGeometry
        {
            if (plane == null || panels == null)
                return null;

            Dictionary<Panel, List<T>> result = new Dictionary<Panel, List<T>>();
            foreach(Panel panel in panels)
            {
                Face3D face3D = panel?.GetFace3D();
                if(face3D == null)
                {
                    continue;
                }

                PlanarIntersectionResult planarIntersectionResult = Geometry.Spatial.Create.PlanarIntersectionResult(plane, face3D, tolerance);
                if(planarIntersectionResult == null || !planarIntersectionResult.Intersecting)
                {
                    continue;
                }

                if(typeof(ISAMGeometry3D).IsAssignableFrom(typeof(T)))
                {
                    List<ISAMGeometry3D> sAMGeometry3Ds = planarIntersectionResult.Geometry3Ds;
                    if(sAMGeometry3Ds == null || sAMGeometry3Ds.Count == 0)
                    {
                        continue;
                    }

                    List<T> sAMGeometries = new List<T>();

                    foreach (ISAMGeometry3D sAMGeometry3D in sAMGeometry3Ds)
                    {
                        if(sAMGeometry3D is T)
                        {
                            sAMGeometries.Add((T)sAMGeometry3D);
                        }
                    }

                    result[panel] = sAMGeometries;
                }
                else if(typeof(Geometry.Planar.ISAMGeometry2D).IsAssignableFrom(typeof(T)))
                {
                    List<Geometry.Planar.ISAMGeometry2D> sAMGeometry2Ds = planarIntersectionResult.Geometry2Ds;
                    if (sAMGeometry2Ds == null || sAMGeometry2Ds.Count == 0)
                    {
                        continue;
                    }

                    List<T> sAMGeometries = new List<T>();

                    foreach (Geometry.Planar.ISAMGeometry2D sAMGeometry2D in sAMGeometry2Ds)
                    {
                        if (sAMGeometry2D is T)
                        {
                            sAMGeometries.Add((T)sAMGeometry2D);
                        }
                    }

                    result[panel] = sAMGeometries;
                }
            }

            return result;
        }
    }
}