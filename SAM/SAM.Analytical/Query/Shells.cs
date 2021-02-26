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
            if (panels.Count() < 2)
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
            Dictionary<double, List<Tuple<Panel, List<Geometry.Planar.Segment2D>>>> dictionary = new Dictionary<double, List<Tuple<Panel, List<Geometry.Planar.Segment2D>>>>();
            foreach (Tuple<double, List<Panel>> tuple in tuples_Elevation)
            {

                List<Tuple<Panel, List<Geometry.Planar.Segment2D>>> tuples = new List<Tuple<Panel, List<Geometry.Planar.Segment2D>>>();

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

                    Tuple<Panel, List<Geometry.Planar.Segment2D>> tuple_Panel = new Tuple<Panel, List<Geometry.Planar.Segment2D>>(panel, new List<Geometry.Planar.Segment2D>());
                    foreach (Geometry.Planar.ISegmentable2D segmentable2D in segmentable2Ds_Temp)
                    {
                        segment2Ds.AddRange(segmentable2D.GetSegments());
                        tuple_Panel.Item2.AddRange(segmentable2D.GetSegments());
                    }

                    tuples.Add(tuple_Panel);
                }

                if (segment2Ds == null || segment2Ds.Count == 0)
                    continue;

                //segment2Ds = segment2Ds.ConvertAll(x => Geometry.Planar.Query.Extend(x, snapTolerance, true, true));
                segment2Ds = Geometry.Planar.Query.Snap(segment2Ds, true, snapTolerance);

                List<Geometry.Planar.Polygon2D> polygon2Ds = Geometry.Planar.Create.Polygon2Ds(segment2Ds, snapTolerance);
                if (polygon2Ds == null || polygon2Ds.Count == 0)
                    continue;

                List<Geometry.Planar.Face2D> face2Ds = Geometry.Planar.Create.Face2Ds(polygon2Ds, true);
                if (face2Ds == null || face2Ds.Count == 0)
                    continue;

                List<Geometry.Planar.IClosed2D> closed2Ds = Geometry.Planar.Query.Holes(face2Ds);
                if (closed2Ds != null && closed2Ds.Count > 0)
                    closed2Ds.ForEach(x => face2Ds.Add(new Geometry.Planar.Face2D(x)));

                if (tuples != null && tuples.Count > 0)
                    dictionary[tuple.Item1] = tuples;

                plane = Plane.WorldXY.GetMoved(new Vector3D(0, 0, tuple.Item1)) as Plane;

                face3Ds.AddRange(face2Ds.ConvertAll(x => new Face3D(plane, x)));
            }

            List<Face3D> face3Ds_Top = Geometry.Spatial.Query.TopFace3Ds(face3Ds);
            if (face3Ds_Top == null || face3Ds_Top.Count == 0)
                return result;

            foreach(Face3D face3D in face3Ds_Top)
            {
                Plane plane = face3D?.GetPlane();
                if (plane == null)
                    continue;

                if (!dictionary.TryGetValue(plane.Origin.Z, out List<Tuple<Panel, List<Geometry.Planar.Segment2D>>> tuples))
                    continue;

                IEnumerable<Geometry.Planar.ISegmentable2D> segmentable2Ds = face3D.Edge2Ds?.FindAll(x => x is Geometry.Planar.ISegmentable2D).Cast<Geometry.Planar.ISegmentable2D>();
                if (segmentable2Ds == null || segmentable2Ds.Count() == 0)
                    continue;

                Geometry.Planar.BoundingBox2D boundingBox2D = Geometry.Planar.Create.BoundingBox2D(segmentable2Ds);

                List<Panel> panels_Face3D = new List<Panel>();
                foreach (Tuple<Panel, List<Geometry.Planar.Segment2D>> tuple in tuples)
                {
                    bool include = false;
                    foreach(Geometry.Planar.Segment2D segment2D in tuple.Item2)
                    {
                        Geometry.Planar.Point2D point2D = segment2D.Mid();

                        if (!boundingBox2D.InRange(point2D, tolerance))
                            continue;

                        foreach(Geometry.Planar.ISegmentable2D segmentable2D in segmentable2Ds)
                        {
                            if(segmentable2D.On(point2D, tolerance))
                            {
                                include = true;
                                break;
                            }
                        }

                        if (include)
                            break;
                    }

                    if(include)
                        panels_Face3D.Add(tuple.Item1);
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