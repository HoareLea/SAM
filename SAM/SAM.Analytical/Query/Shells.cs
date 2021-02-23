using System;
using System.Collections.Generic;
using System.Linq;

using SAM.Geometry.Spatial;

namespace SAM.Analytical
{
    public static partial class Query
    {
        public static List<Shell> Shells(this IEnumerable<Panel> panels, double offset = 0.1, double snapTolerance = Core.Tolerance.MacroDistance, double tolerance = Core.Tolerance.Distance)
        {
            if (panels == null)
                return null;

            List<Shell> result = new List<Shell>();
            if (panels.Count() < 4)
                return result;

            List<Tuple<double, List<Panel>>> tuples_Elevation = new List<Tuple<double, List<Panel>>>();
            foreach (Panel panel in panels)
            {
                BoundingBox3D boundingBox3D = panel?.GetBoundingBox();
                if (boundingBox3D == null)
                    continue;

                double elevation_Min = boundingBox3D.Min.Z;
                Tuple<double, List<Panel>> tuple = tuples_Elevation.Find(x => System.Math.Abs(x.Item1 - elevation_Min) < tolerance);
                if(tuple == null)
                {
                    tuple = new Tuple<double, List<Panel>>(elevation_Min, new List<Panel>());
                    tuples_Elevation.Add(tuple);
                }

                tuple.Item2.Add(panel);
            }

            List<Face3D> face3Ds = new List<Face3D>();
            foreach (Tuple<double, List<Panel>> tuple in tuples_Elevation)
            {
                Plane plane = Plane.WorldXY.GetMoved(new Vector3D(0, 0, tuple.Item1 + offset)) as Plane;

                List<Geometry.Planar.Segment2D> segment2Ds = new List<Geometry.Planar.Segment2D>();
                foreach (Panel panel in tuple.Item2)
                {
                    Face3D face3D = panel.GetFace3D();
                    if (face3D == null)
                        continue;

                    face3D = new Face3D(face3D.GetExternalEdge3D());

                    PlanarIntersectionResult planarIntersectionResult = plane.Intersection(face3D);
                    if (planarIntersectionResult == null)
                        continue;

                    List<Geometry.Planar.ISegmentable2D> segmentable2Ds_Temp = planarIntersectionResult.GetGeometry2Ds<Geometry.Planar.ISegmentable2D>();
                    if (segmentable2Ds_Temp == null || segmentable2Ds_Temp.Count == 0)
                        continue;

                    foreach (Geometry.Planar.ISegmentable2D segmentable2D in segmentable2Ds_Temp)
                        segment2Ds.AddRange(segmentable2D.GetSegments());
                }

                if (segment2Ds == null || segment2Ds.Count == 0)
                    continue;

                segment2Ds = segment2Ds.ConvertAll(x => Geometry.Planar.Query.Extend(x, snapTolerance, true, true));
                segment2Ds = Geometry.Planar.Query.Snap(segment2Ds, true, snapTolerance);

                List<Geometry.Planar.Polygon2D> polygon2Ds = Geometry.Planar.Create.Polygon2Ds(segment2Ds, snapTolerance);
                if (polygon2Ds == null || polygon2Ds.Count == 0)
                    continue;

                List<Geometry.Planar.Face2D> face2Ds = Geometry.Planar.Create.Face2Ds(polygon2Ds, true);
                if (face2Ds == null || face2Ds.Count == 0)
                    continue;

                plane = Plane.WorldXY.GetMoved(new Vector3D(0, 0, tuple.Item1)) as Plane;

                face3Ds.AddRange(face2Ds.ConvertAll(x => new Face3D(plane, x)));
            }

            List<Face3D> face3Ds_Top = Geometry.Spatial.Query.TopFace3Ds(face3Ds);
            if (face3Ds_Top == null || face3Ds_Top.Count == 0)
                return result;

            int count_1 = 0;
            foreach(Face3D face3D in face3Ds_Top)
            {
                count_1++;

                Plane plane = face3D?.GetPlane();
                if (plane == null)
                    continue;

                Tuple<double, List<Panel>> tuple = tuples_Elevation.Find(x => x.Item1 == plane.Origin.Z);
                if (tuple == null)
                    continue;

                int count_2 = 0;
                List<Panel> panels_Face3D = new List<Panel>();
                foreach(Panel panel in tuple.Item2)
                {
                    count_2++;

                    PlanarIntersectionResult planarIntersectionResult = PlanarIntersectionResult.Create(face3D, panel.GetFace3D(), tolerance_Distance: tolerance);
                    if (planarIntersectionResult == null || !planarIntersectionResult.Intersecting)
                        continue;

                    panels_Face3D.Add(panel);
                }

                if (panels_Face3D == null || panels_Face3D.Count == 0)
                    continue;

                double elevation_Max = panels_Face3D.ConvertAll(x => x.GetBoundingBox().Max.Z).Max();

                if (System.Math.Abs(elevation_Max - plane.Origin.Z) < tolerance)
                    continue;

                face3Ds.Add(face3D.GetMoved(new Vector3D(0, 0, elevation_Max - plane.Origin.Z)) as Face3D);
            }

            return Geometry.Spatial.Create.Shells(face3Ds, snapTolerance, tolerance);
        }
    }
}