using SAM.Geometry.Planar;
using SAM.Geometry.Spatial;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical
{
    public static partial class Query
    {
        public static List<List<Panel>> OverlapPanels(this IEnumerable<Panel> panels, double tolerance = Core.Tolerance.Distance)
        {
            if (panels == null)
                return null;

            List<Tuple<Plane, Face3D, Point3D, Panel>> tuples = panels.ToList().ConvertAll(x => new Tuple<Plane, Face3D, Point3D, Panel>(x.PlanarBoundary3D.Plane, x.GetFace3D(), x.GetFace3D().GetInternaPoint3D(), x));

            List<List<Panel>> result = new List<List<Panel>>();
            while (tuples.Count > 0)
            {
                Tuple<Plane, Face3D, Point3D, Panel> tuple = tuples.First();
                tuples.RemoveAt(0);

                List<Tuple<Plane, Face3D, Point3D, Panel>> tuples_Temp = tuples.FindAll(x => tuple.Item1.Coplanar(x.Item1, tolerance) && tuple.Item1.Distance(x.Item1) < tolerance);
                if (tuples_Temp == null || tuples_Temp.Count == 0)
                    continue;

                List<Tuple<Plane, Face3D, Point3D, Panel>> tuples_Temp_Temp = new List<Tuple<Plane, Face3D, Point3D, Panel>>();
                foreach (Tuple<Plane, Face3D, Point3D, Panel> tuple_Temp in tuples_Temp)
                {
                    PlanarIntersectionResult planarIntersectionResult = PlanarIntersectionResult.Create(tuple.Item1, tuple_Temp.Item1);
                    if (planarIntersectionResult == null || !planarIntersectionResult.Intersecting)
                        continue;

                    List<ISAMGeometry2D> geometry2Ds = planarIntersectionResult.Geometry2Ds;
                    if (geometry2Ds.Find(x => x is IClosed2D) == null)
                        continue;

                    tuples_Temp_Temp.Add(tuple_Temp);
                }
                tuples_Temp = tuples_Temp_Temp;

                tuples.RemoveAll(x => tuples_Temp.Contains(x));

                List<Panel> panels_Temp = tuples_Temp.ConvertAll(x => x.Item4);
                panels_Temp.Add(tuple.Item4);

                result.Add(panels_Temp);
            }

            return result;
        }
    }
}