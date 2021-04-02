using SAM.Geometry.Spatial;
using SAM.Geometry.Planar;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical
{
    public static partial class Modify
    {
        public static void Align(this List<Panel> panels, double elevation, double referenceElevation, double maxDistance = 0.2, double tolerance_Angle = Core.Tolerance.Angle, double tolerance_Distance = Core.Tolerance.Distance)
        {
            if (panels == null || double.IsNaN(elevation) || double.IsNaN(referenceElevation))
                return;

            List<Panel> panels_Temp = new List<Panel>();
            Dictionary<Segment2D, Panel> dictionary_Reference = new Dictionary<Segment2D, Panel>();
            foreach (Panel panel in panels)
            {
                if (panel == null)
                    continue;

                double max = panel.MaxElevation();
                double min = panel.MinElevation();

                Plane plane = new Plane(new Point3D(0, 0, (max + min) / 2), Vector3D.WorldZ);

                PlanarIntersectionResult planarIntersectionResult = null;

                if (System.Math.Abs(min - elevation) < Core.Tolerance.Distance || (min - Core.Tolerance.Distance < elevation && max - Core.Tolerance.Distance > elevation))
                {
                    planarIntersectionResult = PlanarIntersectionResult.Create(plane, panel.GetFace3D());
                    if (planarIntersectionResult != null)
                    {
                        List<Segment2D> segment2Ds = planarIntersectionResult.GetGeometry2Ds<Segment2D>();
                        if (segment2Ds != null && segment2Ds.Count != 0)
                        {
                            Geometry.Planar.Query.ExtremePoints(segment2Ds.Point2Ds(), out Point2D point2D_1, out Point2D point2D_2);
                            if (point2D_1.Distance(point2D_2) > tolerance_Distance)
                                panels_Temp.Add(panel);
                        }
                    }
                }

                if (System.Math.Abs(min - referenceElevation) < Core.Tolerance.Distance || (min - Core.Tolerance.Distance < referenceElevation && max - Core.Tolerance.Distance > referenceElevation))
                {
                    if (planarIntersectionResult == null)
                        planarIntersectionResult = PlanarIntersectionResult.Create(plane, panel.GetFace3D());

                    if (planarIntersectionResult != null)
                    {
                        List<Segment2D> segment2Ds = planarIntersectionResult.GetGeometry2Ds<Segment2D>();
                        if (segment2Ds != null && segment2Ds.Count != 0)
                        {
                            foreach (Segment2D segment2D in segment2Ds)
                            {
                                dictionary_Reference[segment2D] = panel;
                            }
                        }
                    }
                }
            }

            if (panels_Temp.Count == 0 || dictionary_Reference.Count == 0)
                return;

            Align(panels_Temp, dictionary_Reference, maxDistance, tolerance_Angle, tolerance_Distance);

            foreach (Panel panel_Temp in panels_Temp)
            {
                int index = panels.FindIndex(x => x.Guid == panel_Temp.Guid);
                if (index == -1)
                    continue;

                panels[index] = panel_Temp;
            }
        }

        public static void Align(this List<Panel> panels, Dictionary<Segment2D, Panel> dictionary_Reference, double maxDistance = 0.2, double tolerance_Angle = Core.Tolerance.Angle, double tolerance_Distance = Core.Tolerance.Distance)
        {
            if (panels == null || dictionary_Reference == null)
                return;

            Plane plane = Plane.WorldXY;

            bool updated = false;
            for (int i = 0; i < panels.Count; i++)
            {
                Panel panel = panels[i];
                if (panel == null)
                    continue;

                double max = panel.MaxElevation();
                double min = panel.MinElevation();

                Plane plane_Temp = Plane.WorldXY.GetMoved(Vector3D.WorldZ * ((max + min) / 2)) as Plane;

                PlanarIntersectionResult planarIntersectionResult = PlanarIntersectionResult.Create(plane_Temp, panel.GetFace3D());
                if (planarIntersectionResult == null || !planarIntersectionResult.Intersecting)
                    continue;

                List<Segment2D> segment2Ds_Intersection = planarIntersectionResult.GetGeometry2Ds<Segment2D>();
                if (segment2Ds_Intersection == null || segment2Ds_Intersection.Count == 0)
                    continue;

                Geometry.Planar.Query.ExtremePoints(segment2Ds_Intersection.Point2Ds(), out Point2D point2D_1, out Point2D point2D_2);
                if (point2D_1.Distance(point2D_2) <= tolerance_Distance)
                    continue;

                Segment2D segment2D = new Segment2D(point2D_1, point2D_2);

                List<Segment2D> segment2Ds_Temp = dictionary_Reference.Keys.ToList().FindAll(x => x.Collinear(segment2D));
                if (segment2Ds_Temp == null || segment2Ds_Temp.Count == 0)
                    continue;

                List<Segment2D> segment2Ds_Result = new List<Segment2D>();
                foreach (Segment2D segment2D_Temp in segment2Ds_Temp)
                {
                    double distance = segment2D_Temp.Distance(segment2D, Core.Tolerance.MacroDistance);
                    if(distance < tolerance_Distance)
                    {
                        segment2Ds_Result = null;
                        break;
                    }

                    if (distance > maxDistance + Core.Tolerance.MacroDistance)
                        continue;

                    segment2Ds_Result.Add(segment2D_Temp);
                }

                if (segment2Ds_Result == null || segment2Ds_Result.Count == 0)
                    continue;

                segment2Ds_Temp = segment2Ds_Result;

                segment2Ds_Temp.Sort((x, y) => segment2D.Distance(x).CompareTo(segment2D.Distance(y)));

                Segment2D segment2D_Reference = null;

                foreach (Segment2D segment2D_Temp in segment2Ds_Temp)
                {
                    Panel panel_Reference = dictionary_Reference[segment2D_Temp];
                    if (panel.Construction == null)
                    {
                        if (panel_Reference.Construction == null)
                        {
                            segment2D_Reference = segment2D_Temp;
                            break;
                        }
                        continue;
                    }

                    if (panel.Construction.Name.Equals(panel_Reference.Construction.Name))
                    {
                        segment2D_Reference = segment2D_Temp;
                        break;
                    }
                }

                if (segment2D_Reference == null)
                {
                    HashSet<PanelType> panelTypes = new HashSet<PanelType>();
                    panelTypes.Add(panel.PanelType);
                    switch (panelTypes.First())
                    {
                        case Analytical.PanelType.CurtainWall:
                            panelTypes.Add(Analytical.PanelType.WallExternal);
                            break;

                        case Analytical.PanelType.UndergroundWall:
                            panelTypes.Add(Analytical.PanelType.WallExternal);
                            break;

                        case Analytical.PanelType.Undefined:
                            panelTypes.Add(Analytical.PanelType.WallInternal);
                            break;
                    }

                    foreach (Segment2D segment2D_Temp in segment2Ds_Temp)
                    {
                        PanelType panelType_Temp = dictionary_Reference[segment2D_Temp].PanelType;
                        if (panelTypes.Contains(panelType_Temp))
                        {
                            segment2D_Reference = segment2D_Temp;
                            break;
                        }
                    }
                }

                if (segment2D_Reference == null)
                    segment2D_Reference = segment2Ds_Temp.First();

                if (segment2D_Reference == null)
                    continue;

                point2D_1 = segment2D.Mid();
                point2D_2 = segment2D_Reference.Project(point2D_1);
                if (point2D_1.Distance(point2D_2) <= tolerance_Distance)
                    continue;

                Vector3D vector3D = Plane.WorldXY.Convert(new Vector2D(point2D_1, point2D_2));

                List<Panel> panels_Connected = panel.ConnectedPanels(panels, tolerance_Distance);

                panel = new Panel(panel); 
                panel.Move(vector3D);
                panels[i] = panel;

                if (panels_Connected != null)
                {
                    Plane plane_Panel = panel.Plane;

                    foreach (Panel panel_Connected in panels_Connected)
                    {
                        Panel panel_New = Align(panel_Connected, plane_Panel, tolerance_Angle, tolerance_Distance);
                        if (panel_New == null)
                            continue;

                        int index = panels.IndexOf(panel_Connected);
                        if (index != -1)
                            panels[index] = panel_New;
                    }
                }

                updated = true;
                break;
            }

            if (updated)
                Align(panels, dictionary_Reference, maxDistance, tolerance_Angle, tolerance_Distance);

        }

        public static Panel Align(this Panel panel, Plane plane, double tolerance_Angle = Core.Tolerance.Angle, double tolerance_Distance = Core.Tolerance.Distance)
        {
            if (panel == null || plane == null)
                return null;

            Face3D face3D = panel.GetFace3D();
            if (face3D == null)
                return null;

            ISegmentable3D segmentable3D = face3D.GetExternalEdge3D() as ISegmentable3D;
            if (segmentable3D == null)
                return null;

            Plane plane_Face3D = face3D.GetPlane();

            PlanarIntersectionResult planarIntersectionResult = PlanarIntersectionResult.Create(plane, plane_Face3D, tolerance_Angle);
            if (planarIntersectionResult == null || !planarIntersectionResult.Intersecting)
                return null;

            Line3D line3D_Intersection = planarIntersectionResult.GetGeometry3D<Line3D>();
            if (line3D_Intersection == null)
                return null; ;

            Dictionary<Point2D, Segment2D> dictionary = new Dictionary<Point2D, Segment2D>();
            foreach (Point3D point3D in segmentable3D.GetPoints())
            {
                Point3D point3D_Temp = line3D_Intersection.Project(point3D);
                if (point3D_Temp == null)
                    continue;

                dictionary[plane_Face3D.Convert(point3D_Temp)] = plane_Face3D.Convert(new Segment3D(point3D_Temp, point3D));
            }

            if (dictionary == null || dictionary.Count < 2)
                return null;

            Geometry.Planar.Query.ExtremePoints(dictionary.Keys, out Point2D point2D_1, out Point2D point2D_2);

            if (point2D_1.Distance(point2D_2) < tolerance_Distance)
                return null;

            ISegmentable2D segmentable2D = face3D.ExternalEdge2D as ISegmentable2D;
            List<Segment2D> segment2Ds = segmentable2D.GetSegments();
            segment2Ds.Add(new Segment2D(point2D_1, point2D_2));
            segment2Ds.Add(dictionary[point2D_1]);
            segment2Ds.Add(dictionary[point2D_2]);

            segment2Ds = Geometry.Planar.Query.Split(segment2Ds, tolerance_Distance);

            List<Polygon2D> polygon2Ds = Geometry.Planar.Create.Polygon2Ds(segment2Ds, tolerance_Distance);
            polygon2Ds = Geometry.Planar.Query.Union(polygon2Ds, tolerance_Distance);

            if (polygon2Ds == null || polygon2Ds.Count == 0)
                return null;

            if (polygon2Ds.Count > 1)
                polygon2Ds.Sort((x, y) => y.GetArea().CompareTo(x.GetArea()));

            Polygon2D polygon2D = polygon2Ds[0];

            polygon2D.SetOrientation(segmentable2D.GetPoints().Orientation());

            Face3D face3D_New = Face3D.Create(plane_Face3D, polygon2D, face3D.InternalEdge2Ds);
            if (face3D_New == null)
                return null;

            return new Panel(panel.Guid, panel, face3D_New, null, true);
        }
    }
}