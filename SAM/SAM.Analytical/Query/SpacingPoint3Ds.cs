using SAM.Geometry.Spatial;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical
{
    public static partial class Query
    {
        public static List<Point3D> SpacingPoint3Ds(this IEnumerable<Panel> panels, double maxTolerance = Core.Tolerance.MacroDistance, double minTolerance = Core.Tolerance.MicroDistance)
        {
            List<Tuple<Panel, BoundingBox3D, Face3D, List<Point3D>>> tuples = new List<Tuple<Panel, BoundingBox3D, Face3D, List<Point3D>>>();
            foreach (Panel panel in panels)
            {
                if (panel == null)
                    continue;

                Face3D face3D = panel.GetFace3D();
                if (face3D == null)
                    continue;

                tuples.Add(new Tuple<Panel, BoundingBox3D, Face3D, List<Point3D>>(panel, face3D.GetBoundingBox(maxTolerance), face3D, Geometry.Spatial.Query.Point3Ds(face3D, true, false)));
            }

            HashSet<Point3D> point3Ds = new HashSet<Point3D>();
            foreach (Tuple<Panel, BoundingBox3D, Face3D, List<Point3D>> tuple in tuples)
            {
                List<Tuple<Panel, BoundingBox3D, Face3D, List<Point3D>>> tuples_Temp = tuples.FindAll(x => x.Item2.Intersect(tuple.Item2) || x.Item2.Inside(tuple.Item2) || tuple.Item2.Inside(x.Item2));
                if (tuples_Temp == null || tuples_Temp.Count < 2)
                    continue;

                tuples_Temp.Remove(tuple);

                Face3D face3D = tuple.Item3;
                BoundingBox3D boundingBox3D = tuple.Item2;
                foreach (Tuple<Panel, BoundingBox3D, Face3D, List<Point3D>> tuple_Temp in tuples_Temp)
                {
                    foreach (Point3D point3D in tuple_Temp.Item4)
                    {
                        if (point3Ds.Contains(point3D))
                            continue;

                        if (!boundingBox3D.Inside(point3D))
                            continue;

                        double distance = face3D.DistanceToEdge2Ds(point3D);
                        if (distance < maxTolerance && distance > minTolerance)
                            point3Ds.Add(point3D);
                    }
                }
            }

            return point3Ds.ToList();
        }
    }
}