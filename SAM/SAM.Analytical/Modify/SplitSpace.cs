using SAM.Geometry.Planar;
using SAM.Geometry.Spatial;
using System;
using System.Collections.Generic;

namespace SAM.Analytical
{
    public static partial class Modify
    {
        public static List<Space> SplitSpace(this AdjacencyCluster adjacencyCluster, Guid spaceGuid, Func<Panel, double> func, double offset = 0.1, double silverSpacing = Core.Tolerance.MacroDistance, double tolerance_Angle = Core.Tolerance.Angle, double tolerance_Distance = Core.Tolerance.Distance, double tolerance_Snap = Core.Tolerance.MacroDistance)
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

            Plane plane_Offset = Geometry.Spatial.Create.Plane(boundingBox3D.Min.Z + offset);

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

                List<Polygon2D> polygon2Ds_Offset = polygon2D.Offset(offsets, true, true, true, tolerance_Distance);
                if(polygon2Ds_Offset == null || polygon2Ds_Offset.Count == 0)
                {
                    continue;
                }


                foreach(Polygon2D polygon2D_Offset in polygon2Ds_Offset)
                {
                    if(polygon2D_Offset == null)
                    {
                        continue;
                    }

                    foreach (Segment2D segment2D in polygon2D_Offset.GetSegments())
                    {
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

                        Segment3D segment3D = plane_Face3D.Convert(segment2D);

                        Segment3D segment3D_Top = plane_Top.Project(segment3D);
                        Segment3D segment3D_Bottom = plane_Bottom.Project(segment3D);

                        Face3D face3D_New = new Face3D(new Polygon3D(new Point3D[] { segment3D_Bottom[0], segment3D_Bottom[1], segment3D_Top[1], segment3D_Top[0]}, tolerance_Distance));
                        face3Ds_New.Add(face3D_New);
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
    }
}