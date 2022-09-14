using SAM.Geometry.Planar;
using SAM.Geometry.Spatial;
using System;
using System.Collections.Generic;

namespace SAM.Analytical
{
    public static partial class Modify
    {
        public static List<Space> SplitSpace(this AdjacencyCluster adjacencyCluster, Guid spaceGuid, Func<Panel, double> func, double elevationOffset = 0.1, double minSectionOffset = 1, double silverSpacing = Core.Tolerance.MacroDistance, double tolerance_Angle = Core.Tolerance.Angle, double tolerance_Distance = Core.Tolerance.Distance, double tolerance_Snap = Core.Tolerance.MacroDistance)
        {
            if(adjacencyCluster == null || spaceGuid == Guid.Empty)
            {
                return null;
            }

            Space space = adjacencyCluster.GetObject<Space>(spaceGuid);
            if(space == null)
            {
                return null;
            }

            Shell shell = adjacencyCluster.Shell(space);
            if(shell == null)
            {
                return null;
            }

            BoundingBox3D boundingBox3D = shell.GetBoundingBox();

            Plane plane_Top = Geometry.Spatial.Create.Plane(boundingBox3D.Max.Z);

            Plane plane_Offset = Geometry.Spatial.Create.Plane(boundingBox3D.Min.Z + elevationOffset);

            Plane plane_Bottom = Geometry.Spatial.Create.Plane(boundingBox3D.Min.Z);

            List<Face3D> face3Ds = shell.Section(plane_Offset, true, tolerance_Angle, tolerance_Distance, tolerance_Snap);
            if(face3Ds == null || face3Ds.Count == 0)
            {
                return null;
            }

            List<Panel> panels = adjacencyCluster.GetPanels(space);
            if (panels == null || panels.Count == 0)
            {
                return null;
            }

            List<Face3D> face3Ds_Shell = shell.Face3Ds;

            List<Face3D> face3Ds_New = new List<Face3D>();
            foreach (Face3D face3D in face3Ds)
            {
                Plane plane_Face3D = face3D.GetPlane();
                if(plane_Face3D == null)
                {
                    continue;
                }

                Face2D face2D = plane_Face3D.Convert(face3D);
                if(face2D == null)
                {
                    continue;
                }

                ISegmentable2D segmentable2D =  face2D.ExternalEdge2D as ISegmentable2D;
                if(segmentable2D == null)
                {
                    throw new NotImplementedException();
                }

                Polygon2D polygon2D = new Polygon2D(segmentable2D.GetPoints());

                List<double> offsets = new List<double>();
                foreach(Segment2D segment2D in polygon2D.GetSegments())
                {
                    Point3D point3D = plane_Face3D.Convert(segment2D.Mid());
                    if(point3D == null)
                    {
                        offsets.Add(0);
                        continue;
                    }

                    Panel panel = panels.Find(x => x.GetFace3D(false).On(point3D, tolerance_Snap));
                    if(panel == null)
                    {
                        offsets.Add(0);
                        continue;
                    }

                    double offset_Panel = func.Invoke(panel);
                    if(double.IsNaN(offset_Panel))
                    {
                        offsets.Add(0);
                        continue;
                    }

                    offsets.Add(offset_Panel);
                }

                if(offsets.FindIndex(x => x > 0) == -1)
                {
                    continue;
                }

                //Offset polygon2D
                List<Polygon2D> polygon2Ds_Offset = polygon2D.Offset(offsets, true, true, true, tolerance_Distance);
                if(polygon2Ds_Offset == null || polygon2Ds_Offset.Count == 0)
                {
                    continue;
                }

                //Remove unwanted polygon2Ds
                for(int i = polygon2Ds_Offset.Count - 1; i >= 0; i--)
                {
                    Polygon2D polygon2D_Offset = polygon2Ds_Offset[i];

                    if (polygon2D_Offset == null || !polygon2D_Offset.IsValid())
                    {
                        polygon2Ds_Offset.RemoveAt(i);
                        continue;
                    }

                    List<Polygon2D> polygon2Ds_Temp = polygon2D_Offset.Offset(-minSectionOffset, tolerance_Distance);
                    if (polygon2Ds_Temp == null || polygon2Ds_Temp.Count == 0)
                    {
                        polygon2Ds_Offset.RemoveAt(i);
                        continue;
                    }

                    polygon2Ds_Offset[i] = Geometry.Planar.Query.SimplifyByAngle(polygon2D_Offset, tolerance_Angle);
                }

                Func<Segment2D, Face3D> createFace3D = new Func<Segment2D, Face3D>((Segment2D segment2D) =>
                {
                    if(segment2D == null)
                    {
                        return null;
                    }

                    Segment3D segment3D = plane_Face3D.Convert(segment2D);
                    if(segment3D == null)
                    {
                        return null;
                    }


                    Segment3D segment3D_Top = plane_Top.Project(segment3D);
                    if(segment3D_Top == null || !segment3D_Top.IsValid() || segment3D_Top.GetLength() < tolerance_Snap)
                    {
                        return null;
                    }

                    Segment3D segment3D_Bottom = plane_Bottom.Project(segment3D);
                    if (segment3D_Bottom == null || !segment3D_Bottom.IsValid() || segment3D_Bottom.GetLength() < tolerance_Snap)
                    {
                        return null;
                    }

                    return new Face3D(new Polygon3D(new Point3D[] { segment3D_Bottom[0], segment3D_Bottom[1], segment3D_Top[1], segment3D_Top[0] }, tolerance_Distance));
                });
                
                //Create new face3Ds
                foreach(Polygon2D polygon2D_Offset in polygon2Ds_Offset)
                {
                    List<Face3D> face3Ds_Polygon2D = new List<Face3D>();
                    foreach (Segment2D segment2D in polygon2D_Offset.GetSegments())
                    {
                        if(segment2D == null)
                        {
                            continue;
                        }

                        if(segment2D.GetLength() < tolerance_Distance)
                        {
                            continue;
                        }

                        Point3D point3D = plane_Face3D.Convert(segment2D.Mid());
                        if (point3D == null)
                        {
                            continue;
                        }

                        Panel panel = panels.Find(x => x.GetFace3D(false).On(point3D, tolerance_Snap));
                        if (panel != null)
                        {
                            continue;
                        }

                        Face3D face3D_New = createFace3D.Invoke(segment2D);
                        if(face3D_New == null)
                        {
                            continue;
                        }

                        face3D_New = face3D_New.Snap(face3Ds_Polygon2D, tolerance_Snap, tolerance_Distance);
                        face3D_New = face3D_New.Snap(face3Ds_Shell, tolerance_Snap, tolerance_Distance);

                        face3Ds_New.Add(face3D_New);
                        face3Ds_Polygon2D.Add(face3D_New);
                    }
                }

                Polygon2D polygon2D_Simplify = Geometry.Planar.Query.SimplifyByAngle(polygon2D, tolerance_Angle);

                //Create additional new face3Ds
                foreach (Polygon2D polygon2D_Offset in polygon2Ds_Offset)
                {
                    List<Segment2D> segment2Ds = polygon2D_Offset.GetSegments();
                    if(segment2Ds == null || segment2Ds.Count == 0)
                    {
                        continue;
                    }

                    List<bool> obtuseAngles = polygon2D_Offset.ObtuseAngles();
                    List<Point2D> point2Ds = polygon2D_Offset.GetPoints();

                    List<Tuple<Point2D, List<Face3D>>> tuples = new List<Tuple<Point2D, List<Face3D>>>();
                    for (int i = 0; i < segment2Ds.Count; i++)
                    {
                        Segment2D segment2D_1 = segment2Ds[i];
                        Segment2D segment2D_2 = Core.Query.Next(segment2Ds, i);

                        Point2D point2D = segment2D_1[1];
                        if(polygon2D.On(point2D, tolerance_Snap))
                        {
                            continue;
                        }

                        Point2D point2D_Previous = segment2D_1[0];
                        Point2D point2D_Next = segment2D_2[1];

                        Vector2D vector2D_1 = point2D_Previous.Vector(point2D);
                        Vector2D vector2D_2 = point2D_Next.Vector(point2D);

                        int index = point2Ds.FindIndex(x => point2D.AlmostEquals(x, tolerance_Distance));
                        if(index == -1)
                        {
                            continue;
                        }

                        bool obtuseAngle = obtuseAngles[index];
                        if (!obtuseAngle)
                        {
                            vector2D_1.Negate();
                            vector2D_2.Negate();
                        }

                        vector2D_1 = Geometry.Planar.Query.TraceFirst(point2D, vector2D_1, polygon2D_Simplify);
                        if (vector2D_1 == null)
                        {
                            continue;
                        }

                        vector2D_2 = Geometry.Planar.Query.TraceFirst(point2D, vector2D_2, polygon2D_Simplify);
                        if (vector2D_2 == null)
                        {
                            continue;
                        }

                        Point2D point2D_1 = point2D.GetMoved(vector2D_1);
                        Point2D point2D_2 = point2D.GetMoved(vector2D_2);

                        Segment2D segment2D_Temp = new Segment2D(point2D_1, point2D_2);

                        Point2D point2D_Polygon2D = null;
                        double distance = double.MaxValue;
                        foreach (Point2D point2D_Temp in polygon2D_Simplify)
                        {
                            if(polygon2D_Offset.On(point2D_Temp, tolerance_Snap))
                            {
                                continue;
                            }

                            Vector2D vector2D = new Vector2D(point2D, point2D_Temp);
                            vector2D = Geometry.Planar.Query.TraceFirst(point2D, vector2D, segment2D_Temp);
                            if (vector2D == null)
                            {
                                continue;
                            }

                            double distance_Temp = point2D.Distance(point2D_Temp);
                            if (distance_Temp < distance)
                            {
                                point2D_Polygon2D = point2D_Temp;
                                distance = distance_Temp;
                            }
                        }

                        if (point2D_Polygon2D == null)
                        {
                            continue;
                        }

                        Segment2D segment2D = new Segment2D(point2D, point2D_Polygon2D);
                        if (segment2D == null || !segment2D.IsValid() || segment2D.GetLength() < tolerance_Snap)
                        {
                            continue;
                        }

                        if (polygon2D_Simplify.On(segment2D.Mid(), tolerance_Snap))
                        {
                            continue;
                        }

                        Face3D face3D_New = createFace3D.Invoke(segment2D);
                        if (face3D_New == null)
                        {
                            continue;
                        }

                        List<Face3D> face3Ds_Temp = tuples.Find(x => x.Item1.AlmostEquals(point2D_Polygon2D, tolerance_Snap))?.Item2;
                        if(face3Ds_Temp == null)
                        {
                            face3Ds_Temp = new List<Face3D>();
                            tuples.Add(new Tuple<Point2D, List<Face3D>>(point2D_Polygon2D, face3Ds_Temp) );
                        }

                        face3Ds_Temp.Add(face3D_New);
                    }

                    if(tuples != null && tuples.Count != 0)
                    {
                        foreach(Tuple<Point2D, List<Face3D>> tuple in tuples)
                        {
                            if (tuple.Item2.Count > 1)
                            {
                                tuple.Item2.Sort((x, y) => x.GetArea().CompareTo(y.GetArea()));
                            }

                            Face3D face3D_New = tuple.Item2[0];
                            face3D_New = face3D_New.Snap(face3Ds_New, tolerance_Snap, tolerance_Distance);
                            face3D_New = face3D_New.Snap(face3Ds_Shell, tolerance_Snap, tolerance_Distance);
                            face3Ds_New.Add(face3D_New);
                        }
                    }
                }

            }

            if(face3Ds_New == null || face3Ds_New.Count == 0)
            {
                return null;
            }

            List<Panel> panels_Result = adjacencyCluster.AddPanels(face3Ds_New, null, new Space[] { space }, silverSpacing, tolerance_Angle, tolerance_Distance, tolerance_Snap);
            if(panels_Result == null)
            {
                return null;
            }

            List<Space> result = new List<Space>();
            foreach(Panel panel_Result in panels_Result)
            {
                List<Space> spaces_Panel = adjacencyCluster.GetRelatedObjects<Space>(panel_Result);
                if(spaces_Panel == null)
                {
                    continue;
                }

                foreach(Space space_Panel in spaces_Panel)
                {
                    if(result.Find(x => x.Guid == space_Panel.Guid) != null)
                    {
                        continue;
                    }

                    result.Add(space_Panel);
                }
            }

            return result;
        }

        public static List<Space> SplitSpace_2(this AdjacencyCluster adjacencyCluster, Guid spaceGuid, Func<Panel, double> func, double elevationOffset = 0.1, double minSectionOffset = 1, double adjustmentOffset = 1, double silverSpacing = Core.Tolerance.MacroDistance, double tolerance_Angle = Core.Tolerance.Angle, double tolerance_Distance = Core.Tolerance.Distance, double tolerance_Snap = Core.Tolerance.MacroDistance)
        {
            if (adjacencyCluster == null || spaceGuid == Guid.Empty)
            {
                return null;
            }

            Space space = adjacencyCluster.GetObject<Space>(spaceGuid);
            if (space == null)
            {
                return null;
            }

            Shell shell = adjacencyCluster.Shell(space);
            if (shell == null)
            {
                return null;
            }

            BoundingBox3D boundingBox3D = shell.GetBoundingBox();

            Plane plane_Top = Geometry.Spatial.Create.Plane(boundingBox3D.Max.Z);

            Plane plane_Offset = Geometry.Spatial.Create.Plane(boundingBox3D.Min.Z + elevationOffset);

            Plane plane_Bottom = Geometry.Spatial.Create.Plane(boundingBox3D.Min.Z);

            List<Face3D> face3Ds = shell.Section(plane_Offset, true, tolerance_Angle, tolerance_Distance, tolerance_Snap);
            if (face3Ds == null || face3Ds.Count == 0)
            {
                return null;
            }

            List<Panel> panels = adjacencyCluster.GetPanels(space);
            if (panels == null || panels.Count == 0)
            {
                return null;
            }

            List<Face3D> face3Ds_Shell = shell.Face3Ds;

            Func<Face2D, Polygon2D, double, Polygon2D> adjustPolygon2D = new Func<Face2D, Polygon2D, double, Polygon2D>((Face2D face2D, Polygon2D polygon2D, double offset) => 
            {
                List<ISegmentable2D> segmentable2Ds = face2D.Edge2Ds?.ConvertAll(x => x as ISegmentable2D)?.FindAll(x => x != null);
                if(segmentable2Ds == null || segmentable2Ds.Count == 0)
                {
                    return null;
                }

                List<Point2D> point2Ds = polygon2D.Points;
                if(point2Ds == null || point2Ds.Count == 0)
                {
                    return null;
                }

                for(int i =0; i < point2Ds.Count; i++)
                {
                    Point2D point2D = point2Ds[i];

                    if(segmentable2Ds.On(point2D, tolerance_Distance))
                    {
                        continue;
                    }

                    List<Segment2D> segment2Ds = polygon2D.ClosestSegment2Ds(point2D, tolerance_Distance);
                    if (segment2Ds == null || segment2Ds.Count == 0)
                    {
                        continue;
                    }

                    Point2D point2D_Intersection = null;
                    double distance = double.MaxValue;
                    foreach (Segment2D segment2D in segment2Ds)
                    {
                        List<Point2D> point2Ds_Segment2D = segment2D.GetPoints();
                        point2Ds_Segment2D.SortByDistance(point2D);

                        Segment2D segment2D_Temp = new Segment2D(point2Ds_Segment2D[1], point2Ds_Segment2D[0]);
                        Vector2D vector2D = Geometry.Planar.Query.TraceFirst(segment2D_Temp[1], segment2D_Temp.Direction, segmentable2Ds);
                        if (vector2D == null)
                        {
                            continue;
                        }

                        double length = vector2D.Length;
                        if (length > offset)
                        {
                            continue;
                        }

                        if (length > distance)
                        {
                            continue;
                        }

                        distance = length;
                        point2D_Intersection = segment2D_Temp[1].GetMoved(vector2D);
                    }

                    if (point2D_Intersection == null)
                    {
                        continue;
                    }

                    point2Ds[i] = point2D_Intersection;
                }
                
                return new Polygon2D(point2Ds);
            });

            List<Face3D> face3Ds_New = new List<Face3D>();
            foreach (Face3D face3D in face3Ds)
            {
                Plane plane_Face3D = face3D.GetPlane();
                if (plane_Face3D == null)
                {
                    continue;
                }

                Face2D face2D = plane_Face3D.Convert(face3D);
                if (face2D == null)
                {
                    continue;
                }

                ISegmentable2D segmentable2D = face2D.ExternalEdge2D as ISegmentable2D;
                if (segmentable2D == null)
                {
                    throw new NotImplementedException();
                }

                Polygon2D polygon2D = new Polygon2D(segmentable2D.GetPoints());

                List<Vector2D> vector2Ds = polygon2D.InternalVector2Ds();
                List<Segment2D> segment2Ds = polygon2D.GetSegments();

                List<Tuple<Vector2D, List<Tuple<Segment2D, Polygon2D>>>> tuples = new List<Tuple<Vector2D, List<Tuple<Segment2D,  Polygon2D>>>>();
                for(int i =0; i < segment2Ds.Count; i++)
                {
                    Segment2D segment2D = segment2Ds[i];
                    Vector2D vector2D = vector2Ds[i];

                    Point3D point3D = plane_Face3D.Convert(segment2D.Mid());
                    if (point3D == null)
                    {
                        continue;
                    }

                    Panel panel = panels.Find(x => x.GetFace3D(false).On(point3D, tolerance_Snap));
                    if (panel == null)
                    {
                        continue;
                    }

                    double offset_Panel = func.Invoke(panel);
                    if (double.IsNaN(offset_Panel) || Core.Query.AlmostEqual(offset_Panel, tolerance_Distance, tolerance_Snap))
                    {
                        continue;
                    }

                    Polygon2D polygon2D_Panel = new Polygon2D(new Point2D[] { segment2D[0], segment2D[1], segment2D[1].GetMoved(vector2D * offset_Panel), segment2D[0].GetMoved(vector2D * offset_Panel) });

                    Tuple<Vector2D, List<Tuple<Segment2D, Polygon2D>>> tuple = tuples.Find(x => x.Item1.AlmostEqual(vector2D, tolerance_Angle));
                    if(tuple == null)
                    {
                        tuple = new Tuple<Vector2D, List<Tuple<Segment2D, Polygon2D>>>(vector2D, new List<Tuple<Segment2D, Polygon2D>>());
                        tuples.Add(tuple);
                    }

                    tuple.Item2.Add(new Tuple<Segment2D, Polygon2D>(segment2D, polygon2D_Panel));
                }

                if(tuples == null || tuples.Count == 0)
                {
                    continue;
                }

                Func<Segment2D, Face3D> createFace3D = new Func<Segment2D, Face3D>((Segment2D segment2D) =>
                {
                    if (segment2D == null)
                    {
                        return null;
                    }

                    Segment3D segment3D = plane_Face3D.Convert(segment2D);
                    if (segment3D == null)
                    {
                        return null;
                    }


                    Segment3D segment3D_Top = plane_Top.Project(segment3D);
                    if (segment3D_Top == null || !segment3D_Top.IsValid() || segment3D_Top.GetLength() < tolerance_Snap)
                    {
                        return null;
                    }

                    Segment3D segment3D_Bottom = plane_Bottom.Project(segment3D);
                    if (segment3D_Bottom == null || !segment3D_Bottom.IsValid() || segment3D_Bottom.GetLength() < tolerance_Snap)
                    {
                        return null;
                    }

                    return new Face3D(new Polygon3D(new Point3D[] { segment3D_Bottom[0], segment3D_Bottom[1], segment3D_Top[1], segment3D_Top[0] }, tolerance_Distance));
                });

                foreach (Tuple<Vector2D, List<Tuple<Segment2D, Polygon2D>>> tuple in tuples)
                {
                    List<Polygon2D> polygon2Ds = tuple.Item2.ConvertAll(x => x.Item2);
                    polygon2Ds = polygon2Ds.Union(tolerance_Distance);

                    List<Face3D> face3Ds_Polygon2D = new List<Face3D>();

                    List<Segment2D> segment2Ds_Temp = new List<Segment2D>();
                    foreach (Polygon2D polygon2D_Temp in polygon2Ds)
                    {
                        List<Polygon2D> polygon2Ds_Difference = polygon2D.Difference(polygon2D_Temp);
                        if(polygon2Ds_Difference == null || polygon2Ds_Difference.Count == 0)
                        {
                            segment2Ds_Temp.AddRange(polygon2D_Temp.GetSegments());
                            continue;
                        }

                        List<Polygon2D> polygon2Ds_Union = new List<Polygon2D>() { polygon2D_Temp};
                        foreach(Polygon2D polygon2D_Difference in polygon2Ds_Difference)
                        {
                            List<Polygon2D> polygon2Ds_Offset = polygon2D_Difference.Offset(-minSectionOffset, tolerance_Distance);
                            if(polygon2Ds_Offset != null && polygon2Ds_Offset.Count > 0)
                            {
                                continue;
                            }

                            polygon2Ds_Union.Add(polygon2D_Difference);
                        }

                        Polygon2D polygon2D_Union = polygon2D_Temp;

                        polygon2Ds_Union = polygon2Ds_Union.Union(tolerance_Distance);
                        if (polygon2Ds_Union != null && polygon2Ds_Union.Count != 0)
                        {
                            polygon2D_Union = polygon2Ds_Union.Find(x => x.Inside(polygon2D_Temp.InternalPoint2D(tolerance_Distance)));
                        }

                        polygon2D_Union = adjustPolygon2D.Invoke(face2D, polygon2D_Union, adjustmentOffset);

                        if (polygon2D_Union == null)
                        {
                            continue;
                        }

                        segment2Ds_Temp.AddRange(polygon2D_Union.GetSegments());
                    }

                    List<Segment2D> segment2Ds_Split = polygon2D.GetSegments();
                    segment2Ds_Split.AddRange(segment2Ds_Temp);
                    segment2Ds_Split = Geometry.Planar.Query.Split(segment2Ds_Split, tolerance_Distance);

                    foreach (Segment2D segment2D in segment2Ds_Split)
                    {
                        if (segment2D == null)
                        {
                            continue;
                        }

                        if (segment2D.GetLength() < tolerance_Distance)
                        {
                            continue;
                        }

                        Point2D point2D = segment2D.Mid();
                        if (point2D == null)
                        {
                            continue;
                        }

                        if (polygon2D.On(point2D, tolerance_Distance))
                        {
                            continue;
                        }

                        Point3D point3D = plane_Face3D.Convert(point2D);
                        if (point3D == null)
                        {
                            continue;
                        }

                        Panel panel = panels.Find(x => x.GetFace3D(false).On(point3D, tolerance_Snap));
                        if (panel != null)
                        {
                            continue;
                        }

                        Face3D face3D_New = createFace3D.Invoke(segment2D);
                        if (face3D_New == null)
                        {
                            continue;
                        }

                        face3D_New = face3D_New.Snap(face3Ds_Polygon2D, tolerance_Snap, tolerance_Distance);
                        face3D_New = face3D_New.Snap(face3Ds_Shell, tolerance_Snap, tolerance_Distance);

                        face3Ds_New.Add(face3D_New);
                        face3Ds_Polygon2D.Add(face3D_New);
                    }
                }
            }

            if (face3Ds_New == null || face3Ds_New.Count == 0)
            {
                return null;
            }

            List<Panel> panels_Result = adjacencyCluster.AddPanels(face3Ds_New, null, new Space[] { space }, silverSpacing, tolerance_Angle, tolerance_Distance, tolerance_Snap);
            if (panels_Result == null)
            {
                return null;
            }

            List<Space> result = new List<Space>();
            foreach (Panel panel_Result in panels_Result)
            {
                List<Space> spaces_Panel = adjacencyCluster.GetRelatedObjects<Space>(panel_Result);
                if (spaces_Panel == null)
                {
                    continue;
                }

                foreach (Space space_Panel in spaces_Panel)
                {
                    if (result.Find(x => x.Guid == space_Panel.Guid) != null)
                    {
                        continue;
                    }

                    result.Add(space_Panel);
                }
            }

            return result;
        }
    }
}