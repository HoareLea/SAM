using System;
using System.Collections.Generic;
using SAM.Geometry.Spatial;
using SAM.Geometry.Planar;
using System.Linq;

namespace SAM.Analytical
{
    public static partial class Query
    {
        public static List<Panel> AdjustRoofs(this IEnumerable<Panel> roofs, IEnumerable<Shell> shells, double offset = 0.1, double tolerance_Angle = Core.Tolerance.Angle, double tolerance_Distance = Core.Tolerance.Distance, double tolerance_Snap = Core.Tolerance.MacroDistance)
        {
            if (roofs == null || shells == null)
                return null;

            Plane plane = Plane.WorldXY;

            List<Tuple<Face2D, Panel>> tuples_Roof = new List<Tuple<Face2D, Panel>>();
            foreach(Panel roof in roofs)
            {
                Face3D face3D = roof.GetFace3D();
                if (face3D == null)
                    continue;

                Plane plane_Roof = face3D.GetPlane();
                if (plane_Roof == null)
                    continue;

                if (Geometry.Spatial.Query.Perpendicular(plane, plane_Roof, tolerance_Distance))
                    continue;

                Face3D face3D_Projected = plane.Project(face3D);
                if (face3D_Projected == null || face3D_Projected.GetArea() <= tolerance_Distance)
                    continue;

                Face2D face2D = plane.Convert(face3D_Projected);
                if (face2D == null)
                    continue;

                tuples_Roof.Add(new Tuple<Face2D, Panel>(face2D, roof));
            }

            if (tuples_Roof == null || tuples_Roof.Count == 0)
                return null;

            List<Tuple<Face2D, Shell>> tuples_Shell = new List<Tuple<Face2D, Shell>>();
            foreach(Shell shell in shells)
            {
                BoundingBox3D boundingBox3D = shell?.GetBoundingBox();
                if (boundingBox3D == null)
                    continue;

                Plane plane_Temp = plane.GetMoved(new Vector3D(0, 0, boundingBox3D.Min.Z + offset)) as Plane;
                
                List<Face3D> face3Ds = shell?.Section(plane_Temp, tolerance_Angle, tolerance_Distance, tolerance_Snap);
                if (face3Ds == null)
                    continue;
                
                foreach(Face3D face3D in face3Ds)
                {
                    Face2D face2D = plane.Convert(face3D);
                    if (face2D == null)
                        continue;

                    tuples_Shell.Add(new Tuple<Face2D, Shell>(face2D, shell));
                }
            }

            if (tuples_Shell == null || tuples_Shell.Count == 0)
                return null;

            List<Face2D> face3Ds_Union_Roof = Geometry.Planar.Query.Union( tuples_Roof.ConvertAll(x => x.Item1), tolerance_Distance);
            if (face3Ds_Union_Roof == null || face3Ds_Union_Roof.Count == 0)
                return null;

            List<Face2D> face3Ds_Union_Shell = Geometry.Planar.Query.Union(tuples_Shell.ConvertAll(x => x.Item1), tolerance_Distance);
            if (face3Ds_Union_Shell == null || face3Ds_Union_Shell.Count == 0)
                return null;

            List<Face2D> face2Ds_Difference_Shell = new List<Face2D>();
            foreach (Face2D face2D_Union_Shell in face3Ds_Union_Shell)
            {
                List<Face2D> face2Ds_Difference_Shell_Temp = Geometry.Planar.Query.Difference(face2D_Union_Shell, face3Ds_Union_Roof);
                if (face2Ds_Difference_Shell_Temp == null || face2Ds_Difference_Shell_Temp.Count == 0)
                    continue;

                face2Ds_Difference_Shell.AddRange(face2Ds_Difference_Shell_Temp);
            }

            if(face2Ds_Difference_Shell != null && face2Ds_Difference_Shell.Count > 0)
            {
                for(int i=0; i < tuples_Roof.Count; i++)
                {
                    Face2D face2D = Geometry.Planar.Query.Join(tuples_Roof[i].Item1, face2Ds_Difference_Shell, tolerance_Distance);
                    if (face2D == null)
                        continue;

                    tuples_Roof[i] = new Tuple<Face2D, Panel>(face2D, tuples_Roof[i].Item2);
                }
            }

            List<Tuple<Face3D, Panel>> tuples_Face3D = new List<Tuple<Face3D, Panel>>();
            foreach(Tuple<Face2D, Panel> tuple_Roof in tuples_Roof)
            {
                Plane plane_Roof = tuple_Roof.Item2.Plane;

                Face2D face2D = tuple_Roof.Item1;

                List<Face2D> face2Ds_Offset = face2D.Offset(1, true, false, tolerance_Distance);
                foreach(Face2D face2D_Offset in face2Ds_Offset)
                {
                    List<Face2D> face2Ds_Intersection = new List<Face2D>();
                    foreach (Face2D face3D_Union_Shell in face3Ds_Union_Shell)
                    {
                        List<Face2D> face2Ds_Intersection_Temp = face2D_Offset.Intersection(face3D_Union_Shell, tolerance_Distance);
                        if (face2Ds_Intersection_Temp != null)
                            face2Ds_Intersection.AddRange(face2Ds_Intersection_Temp);
                    }

                    if (face2Ds_Intersection == null || face2Ds_Intersection.Count == 0)
                        continue;

                    foreach (Face2D face2D_Intersection in face2Ds_Intersection)
                    {
                        Face3D face3D = plane.Convert(face2D_Intersection);

                        face3D = plane_Roof.Project(face3D, plane.Normal);
                        if (face3D == null)
                            continue;

                        tuples_Face3D.Add(new Tuple<Face3D, Panel>(face3D, tuple_Roof.Item2));
                    }
                }
            }

            if (tuples_Face3D == null || tuples_Face3D.Count == 0)
                return null;

            List<ISegmentable3D> segmentable3Ds = tuples_Face3D.ConvertAll(x => x.Item1).Intersections<ISegmentable3D>(tolerance_Angle, tolerance_Distance);
            if (segmentable3Ds != null && segmentable3Ds.Count > 0)
            {
                List<Segment3D> segment3Ds = new List<Segment3D>();
                segmentable3Ds.ForEach(x => segment3Ds.AddRange(x.GetSegments()));
                if (segment3Ds != null && segment3Ds.Count > 0)
                {
                    Dictionary<Panel, List<Face2D>> dictionary = new Dictionary<Panel, List<Face2D>>();

                    List<Segment2D> segment2Ds = segment3Ds.ConvertAll(x => plane.Convert(plane.Project(x)));
                    List<Face3D> face3Ds = new List<Face3D>();
                    foreach (Tuple<Face3D, Panel> tuple in tuples_Face3D)
                    {
                        Face3D face3D_Projected = plane.Project(tuple.Item1);
                        if (face3D_Projected == null || face3D_Projected.GetArea() <= tolerance_Distance)
                            continue;

                        Face2D face2D = plane.Convert(face3D_Projected);
                        if (face2D == null)
                            continue;

                        List<IClosed2D> closed2Ds = face2D.Edge2Ds;
                        foreach (IClosed2D closed2D in closed2Ds)
                        {
                            ISegmentable2D segmentable2D = closed2D as ISegmentable2D;
                            if (segmentable2D == null)
                                continue;

                            segment2Ds.AddRange(segmentable2D.GetSegments());
                        }

                        face3Ds.Add(tuple.Item1);
                    }

                    segment2Ds = Geometry.Planar.Query.Split(segment2Ds, tolerance_Distance);

                    segment2Ds = Geometry.Planar.Query.Snap(segment2Ds, true, tolerance_Snap);

                    if (segment2Ds != null && segment2Ds.Count != 0)
                    {
                        List<Polygon2D> polygon2Ds = Geometry.Planar.Create.Polygon2Ds(segment2Ds, tolerance_Distance);
                        if (polygon2Ds != null && polygon2Ds.Count > 0)
                        {
                            foreach (Polygon2D polygon2D in polygon2Ds)
                            {
                                Point2D point2D = polygon2D.GetInternalPoint2D();

                                Dictionary<Face3D, Point3D> dictionary_Intersection = Geometry.Spatial.Query.IntersectionDictionary(face3Ds, plane.Convert(point2D), plane.Normal, tolerance_Distance);
                                if (dictionary_Intersection == null || dictionary_Intersection.Count == 0)
                                    continue;

                                Face3D face3D = dictionary_Intersection.Keys.FirstOrDefault();
                                if (face3D == null)
                                    continue;

                                Tuple<Face3D, Panel> tuple = tuples_Face3D.Find(x => x.Item1 == face3D);
                                if (tuple == null)
                                    continue;

                                if (!dictionary.TryGetValue(tuple.Item2, out List<Face2D> face2Ds))
                                {
                                    face2Ds = new List<Face2D>();
                                    dictionary[tuple.Item2] = face2Ds;
                                }

                                face2Ds.Add(new Face2D(polygon2D));
                            }
                        }
                    }

                    if (dictionary != null && dictionary.Count > 0)
                    {
                        tuples_Face3D = new List<Tuple<Face3D, Panel>>();
                        foreach (KeyValuePair<Panel, List<Face2D>> keyValuePair in dictionary)
                        {
                            Plane plane_Roof = keyValuePair.Key.Plane;

                            //Join Face2Ds
                            List<Face2D> face2Ds = Geometry.Planar.Query.Union(keyValuePair.Value, tolerance_Distance);
                            foreach (Face2D face2D in face2Ds)
                            {
                                Face3D face3D = plane.Convert(face2D);

                                face3D = plane_Roof.Project(face3D, plane.Normal);
                                if (face3D == null)
                                    continue;

                                tuples_Face3D.Add(new Tuple<Face3D, Panel>(face3D, keyValuePair.Key));
                            }
                        }
                    }
                }
            }

            List<Panel> result = new List<Panel>();
            foreach(Tuple<Face3D, Panel> tuple in tuples_Face3D)
            {
                Guid guid = tuple.Item2.Guid;
                if (result.Find(x => x.Guid == guid) != null)
                    guid = Guid.NewGuid();

                Panel panel = new Panel(guid, tuple.Item2, tuple.Item1, maxDistance: tolerance_Snap);
                if (!panel.Normal.SameHalf(tuple.Item2.Normal))
                    panel.FlipNormal(false, false);

                result.Add(panel);
            }

            return result;
        }
    }
}