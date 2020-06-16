using SAM.Geometry.Spatial;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical
{
    public static partial class Query
    {
        public static bool Inside(this AdjacencyCluster adjacencyCluster, Space space, Point3D point3D, double silverSpacing = Core.Tolerance.MacroDistance, double tolerance = Core.Tolerance.Distance)
        {
            if (adjacencyCluster == null || space == null || point3D == null)
                return false;

            List<Panel> panels = adjacencyCluster.GetRelatedObjects<Panel>(space);
            if (panels == null || panels.Count == 0)
                return false;

            List<Tuple<Panel, BoundingBox3D, Face3D>> tuples = new List<Tuple<Panel, BoundingBox3D, Face3D>>();
            foreach(Panel panel in panels)
            {
                Face3D face3D = panel.GetFace3D();
                if (face3D == null)
                    continue;

                tuples.Add(new Tuple<Panel, BoundingBox3D, Face3D>(panel, face3D.GetBoundingBox(), face3D));
            }

            BoundingBox3D boundingBox3D = new BoundingBox3D(tuples.ConvertAll(x => x.Item2));
            if (!boundingBox3D.Inside(point3D))
                return false;

            Vector3D vector3D = new Vector3D(boundingBox3D.Min, boundingBox3D.Max);
            if (vector3D.Length < silverSpacing)
                return false;

            Segment3D segment3D = new Segment3D(point3D, vector3D);

            HashSet<Point3D> point3Ds = new HashSet<Point3D>();
            foreach(Tuple<Panel, BoundingBox3D, Face3D> tuple in tuples)
            {
                PlanarIntersectionResult planarIntersectionResult = PlanarIntersectionResult.Create(tuple.Item3, segment3D, tolerance);
                if (planarIntersectionResult == null || !planarIntersectionResult.Intersecting)
                    continue;

                List<Point3D> point3Ds_Temp = planarIntersectionResult.GetGeometry3Ds<Point3D>();
                if (point3Ds_Temp != null && point3Ds_Temp.Count > 0)
                    point3Ds_Temp.ForEach(x => point3Ds.Add(x));
            }

            if (point3Ds == null || point3Ds.Count == 0)
                return false;

            return point3Ds.Count % 2 != 0;
        }
    }
}