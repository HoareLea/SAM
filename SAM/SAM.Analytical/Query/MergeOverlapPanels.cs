using NetTopologySuite.Geometries;
using SAM.Geometry;
using SAM.Geometry.Planar;
using SAM.Geometry.Spatial;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical
{
    public static partial class Query
    {
        public static List<Panel> MergeOverlapPanels(this IEnumerable<Panel> panels, double offset, ref List<Panel> redundantPanels, bool setDefaultConstruction, double tolerance = Core.Tolerance.Distance)
        {
            if (panels == null)
                return null;

            Dictionary<PanelGroup, List<Panel>> dictionary = new Dictionary<PanelGroup, List<Panel>>();
            foreach (PanelGroup panelGroup in Enum.GetValues(typeof(PanelGroup)))
                dictionary[panelGroup] = new List<Panel>();

            foreach (Panel panel in panels)
            {
                if (panel == null)
                    continue;

                dictionary[PanelGroup(panel.PanelType)].Add(panel);
            }

            List<Panel> result = new List<Panel>();

            List<Panel> panels_Temp = dictionary[Analytical.PanelGroup.Floor];
            panels_Temp.AddRange(dictionary[Analytical.PanelGroup.Roof]);

            panels_Temp = MergeOverlapPanels_FloorAndRoof(panels_Temp, offset, ref redundantPanels, setDefaultConstruction, tolerance);
            if (panels_Temp != null && panels_Temp.Count > 0)
                result.AddRange(panels_Temp);

            panels_Temp = dictionary[Analytical.PanelGroup.Wall];
            panels_Temp = MergeOverlapPanels_Walls(panels_Temp, offset, ref redundantPanels, setDefaultConstruction, tolerance);
            if (panels_Temp != null && panels_Temp.Count > 0)
                result.AddRange(panels_Temp);

            return result;
        }

        private static List<Panel> MergeOverlapPanels_FloorAndRoof(this IEnumerable<Panel> panels, double offset, ref List<Panel> redundantPanels, bool setDefaultConstruction, double tolerance = Core.Tolerance.Distance)
        {
            List<Tuple<double, Panel>> tuples = new List<Tuple<double, Panel>>();
            foreach (Panel panel in panels)
            {
                double elevation = panel.MaxElevation();
                tuples.Add(new Tuple<double, Panel>(elevation, panel));
            }

            List<Panel> result = new List<Panel>();
            HashSet<Guid> guids = new HashSet<Guid>();

            tuples.Sort((x, y) => x.Item1.CompareTo(y.Item1));
            while (tuples.Count > 0)
            {
                Tuple<double, Panel> tuple = tuples[0];
                tuples.RemoveAt(0);

                double elevation = tuple.Item1;
                double elevation_Offset = elevation + offset;
                List<Tuple<double, Panel>> tuples_Offset = new List<Tuple<double, Panel>>();
                foreach (Tuple<double, Panel> tuple_Temp in tuples)
                {
                    if (tuple_Temp.Item1 > elevation_Offset)
                        break;

                    tuples_Offset.Add(tuple_Temp);
                }

                foreach (Tuple<double, Panel> tuple_Temp in tuples_Offset)
                    tuples.Remove(tuple_Temp);

                tuples_Offset.Insert(0, tuple);

                Plane plane = tuple.Item2.GetFace3D().GetPlane();

                List<Tuple<Polygon, Panel>> tuples_Polygon = new List<Tuple<Polygon, Panel>>();
                foreach (Tuple<double, Panel> tuple_Temp in tuples_Offset)
                {
                    Panel panel = tuple_Temp.Item2;

                    Face3D face3D = plane.Project(panel.GetFace3D());

                    Face2D face2D = plane.Convert(face3D);

                    tuples_Polygon.Add(new Tuple<Polygon, Panel>(face2D.ToNTS(tolerance), panel));
                }

                List<Polygon> polygons_Temp = tuples_Polygon.ConvertAll(x => x.Item1);
                Geometry.Planar.Modify.RemoveAlmostSimilar_NTS(polygons_Temp, tolerance);

                List<Polygon> polygons = Geometry.Convert.ToNTS_Polygons(polygons_Temp, tolerance);
                if (polygons != null || polygons.Count > 0)
                {
                    Dictionary<Construction, List<Tuple<Panel, Polygon>>> dictionary = new Dictionary<Construction, List<Tuple<Panel, Polygon>>>();

                    foreach (Polygon polygon in polygons)
                    {
                        Point point = polygon.InteriorPoint;

                        List<Tuple<Polygon, Panel>> tuples_Polygon_Contains = tuples_Polygon.FindAll(x => x.Item1.Contains(point) || x.Item1.Contains(polygon.InteriorPoint));
                        int count = tuples_Polygon_Contains.Count;
                        if (count == 0)
                            continue;

                        Panel panel_Old = null;
                        if (setDefaultConstruction)
                        {
                            Construction construction = null;
                            PanelType panelType = Analytical.PanelType.Undefined;
                            if (TryGetConstruction(tuples_Polygon_Contains.ConvertAll(x => x.Item2), out panel_Old, out construction, out panelType))
                            {
                                if (panel_Old != null && construction != null)
                                {
                                    tuples_Polygon_Contains.RemoveAll(x => x.Item2 == panel_Old);
                                    redundantPanels.AddRange(tuples_Polygon_Contains.ConvertAll(x => x.Item2));
                                    panel_Old = new Panel(panel_Old, construction);
                                    panel_Old = new Panel(panel_Old, panelType);
                                }
                            }
                        }

                        if (panel_Old == null)
                        {
                            List<Tuple<Polygon, Panel>> tuples_Polygon_Floor = tuples_Polygon_Contains.FindAll(x => Query.PanelGroup(x.Item2.PanelType) == Analytical.PanelGroup.Floor);
                            if (tuples_Polygon_Floor != null && tuples_Polygon_Floor.Count != 0)
                            {
                                panel_Old = tuples_Polygon_Floor.Find(x => x.PanelType() == Analytical.PanelType.FloorInternal)?.Item2;
                                if (panel_Old == null)
                                    panel_Old = tuples_Polygon_Floor.First().Item2;
                            }
                            else
                            {
                                List<Tuple<Polygon, Panel>> tuples_Polygon_Roof = tuples_Polygon_Contains.FindAll(x => Query.PanelGroup(x.Item2.PanelType) == Analytical.PanelGroup.Roof);
                                if (tuples_Polygon_Roof != null && tuples_Polygon_Roof.Count > 1)
                                    panel_Old = tuples_Polygon_Contains.Find(x => x.PanelType() == Analytical.PanelType.FloorInternal)?.Item2;
                            }

                            if (panel_Old == null)
                                panel_Old = tuples_Polygon_Contains.First().Item2;

                            tuples_Polygon_Contains.RemoveAt(0);
                            redundantPanels.AddRange(tuples_Polygon_Contains.ConvertAll(x => x.Item2));
                        }

                        if (panel_Old == null)
                            continue;

                        Polygon polygon_Temp = Geometry.Planar.Query.SimplifyByNTS_Snapper(polygon);
                        polygon_Temp = Geometry.Planar.Query.SimplifyByNTS_TopologyPreservingSimplifier(polygon_Temp);

                        Face3D face3D = new Face3D(plane, polygon_Temp.ToSAM());
                        Guid guid = panel_Old.Guid;
                        if (guids.Contains(guid))
                            guid = Guid.NewGuid();

                        Panel panel_New = new Panel(guid, panel_Old, face3D);

                        result.Add(panel_New);
                        guids.Add(guid);
                    }
                }
            }

            return result;
        }

        private static List<Panel> MergeOverlapPanels_Walls(this IEnumerable<Panel> panels, double offset, ref List<Panel> redundantPanels, bool setDefaultConstruction, double tolerance = Core.Tolerance.Distance)
        {
            List<Tuple<Face3D, Panel>> tuples = panels?.ToList().ConvertAll(x => new Tuple<Face3D, Panel>(x.GetFace3D(), x));
            if (tuples == null || tuples.Count == 0)
                return null;

            tuples.Sort((x, y) => y.Item1.GetArea().CompareTo(x.Item1.GetArea()));

            List<Panel> result = new List<Panel>();
            HashSet<Guid> guids = new HashSet<Guid>();
            List<Panel> redundantPanels_Temp = new List<Panel>(panels);
            while (tuples.Count > 0)
            {
                Tuple<Face3D, Panel> tuple = tuples[0];
                tuples.RemoveAt(0);

                Plane plane = tuple.Item1.GetPlane();

                List<Tuple<Face3D, Panel>> tuples_Face3D = tuples.FindAll(x => x.Item1.Coplanar(tuple.Item1) && plane.Distance(x.Item1.GetPlane()) <= offset);
                if (tuples_Face3D.Count == 0)
                {
                    result.Add(tuple.Item2);
                    redundantPanels_Temp.Remove(tuple.Item2);
                    continue;
                }

                tuples.RemoveAll(x => tuples_Face3D.Contains(x));

                Polygon polygon = plane.Convert(tuple.Item1).ToNTS(tolerance);
                List<Tuple<Polygon, Panel>> tuples_Polygon = tuples_Face3D.ConvertAll(x => new Tuple<Polygon, Panel>(plane.Convert(plane.Project(x.Item1)).ToNTS(tolerance), x.Item2));
                tuples_Polygon.Add(new Tuple<Polygon, Panel>(polygon, tuple.Item2));

                List<Polygon> polygons = tuples_Polygon.ConvertAll(x => x.Item1).ToNTS_Polygons(tolerance);

                foreach (Polygon polygon_Temp in polygons)
                {
                    List<Tuple<Polygon, Panel>> tuples_Polygon_Contains = tuples_Polygon.FindAll(x => x.Item1.Contains(polygon_Temp.InteriorPoint) || polygon_Temp.Contains(x.Item1.InteriorPoint));
                    if (tuples_Polygon_Contains == null || tuples_Polygon_Contains.Count == 0)
                        continue;

                    tuples_Polygon_Contains.Sort((x, y) => y.Item1.Area.CompareTo(x.Item1.Area));

                    Panel panel_Old = tuples_Polygon_Contains.First().Item2;
                    redundantPanels_Temp.Remove(panel_Old);

                    Guid guid = panel_Old.Guid;
                    if (guids.Contains(guid))
                        guid = Guid.NewGuid();

                    guids.Add(guid);

                    Polygon polygon_Simplify = Geometry.Planar.Query.SimplifyByNTS_Snapper(polygon_Temp);
                    polygon_Simplify = Geometry.Planar.Query.SimplifyByNTS_TopologyPreservingSimplifier(polygon_Simplify);

                    Face3D face3D = plane.Convert(polygon_Simplify.ToSAM(tolerance));
                    if (face3D == null)
                        continue;

                    Panel panel_New = new Panel(guid, panel_Old, face3D);

                    result.Add(panel_New);
                }
            }

            redundantPanels.AddRange(redundantPanels_Temp);
            return result;
        }
    }
}