using System;
using SAM.Geometry.Spatial;
using System.Collections.Generic;

namespace SAM.Analytical
{
    public static partial class Modify
    {
        public static List<Guid> JoinExternal(this List<Panel> panels, double elevation, double maxDistance, double snapTolerance = Core.Tolerance.MacroDistance, double tolerance_Angle = Core.Tolerance.Angle, double tolerance_Distance = Core.Tolerance.Distance)
        {
            if (panels == null)
                return null;

            Plane plane = Plane.WorldXY.GetMoved(new Vector3D(0, 0, elevation)) as Plane;

            List<Geometry.Planar.ISegmentable2D> segmentable2Ds = new List<Geometry.Planar.ISegmentable2D>();
            List<Tuple<Panel, BoundingBox3D, List<Geometry.Planar.ISegmentable2D>>> tuples = new List<Tuple<Panel, BoundingBox3D, List<Geometry.Planar.ISegmentable2D>>>();
            foreach(Panel panel in panels)
            {
                Face3D face3D = panel.GetFace3D(false, tolerance_Distance);
                if(face3D == null)
                {
                    continue;
                }

                BoundingBox3D boundingBox3D = face3D.GetBoundingBox(tolerance_Distance);
                if(boundingBox3D.Max.Z < elevation || boundingBox3D.Min.Z > elevation)
                {
                    continue;
                }

                PlanarIntersectionResult planarIntersectionResult = Geometry.Spatial.Create.PlanarIntersectionResult(plane, face3D, tolerance_Angle, tolerance_Distance);
                if(planarIntersectionResult == null || !planarIntersectionResult.Intersecting)
                {
                    continue;
                }

                List<Geometry.Planar.ISegmentable2D> segmentable2Ds_Temp = planarIntersectionResult.GetGeometry2Ds<Geometry.Planar.ISegmentable2D>();
                if(segmentable2Ds_Temp == null || segmentable2Ds_Temp.Count == 0)
                {
                    continue;
                }

                segmentable2Ds.AddRange(segmentable2Ds_Temp);
                tuples.Add(new Tuple<Panel, BoundingBox3D, List<Geometry.Planar.ISegmentable2D>>(panel, boundingBox3D, segmentable2Ds_Temp));
            }

            List<Guid> result = new List<Guid>();

            List<Geometry.Planar.Polygon2D> polygon2Ds = Geometry.Planar.Query.ExternalPolygon2Ds(segmentable2Ds, maxDistance, snapTolerance, tolerance_Distance);
            if (polygon2Ds == null || polygon2Ds.Count == 0)
                return result;

            panels.Clear();
            panels.AddRange(Create.Panels(polygon2Ds.ConvertAll(x => plane.Convert(x)), 3.0));

            result.AddRange(panels.ConvertAll(x => x.Guid));

            return result;
        }
    }
}