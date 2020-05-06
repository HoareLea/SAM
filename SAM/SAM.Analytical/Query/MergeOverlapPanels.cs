using NetTopologySuite.Geometries;
using SAM.Geometry;
using SAM.Geometry.Planar;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical
{
    public static partial class Query
    {
        public static List<Panel> MergeOverlapPanels(this IEnumerable<Panel> panels, double offset, double tolerance = Core.Tolerance.Distance)
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

            panels_Temp = MergeOverlapPanels_FloorAndRoof(panels_Temp, offset, tolerance);
            if (panels_Temp != null && panels_Temp.Count > 0)
                result.AddRange(panels_Temp);

            return result;
        }
    
        private static List<Panel> MergeOverlapPanels_FloorAndRoof(this IEnumerable<Panel> panels, double offset, double tolerance = Core.Tolerance.Distance)
        {
            List<Tuple<double, Panel>> tuples = new List<Tuple<double, Panel>>();
            foreach (Panel panel in panels)
            {
                double elevation = panel.MaxElevation();
                tuples.Add(new Tuple<double, Panel>(elevation, panel));
            }

            Construction construction_FloorInternal_Default = Construction(Analytical.PanelType.FloorInternal);
            Construction construction_FloorExposed_Default = Construction(Analytical.PanelType.FloorExposed);
            Construction construction_SlabOnGrade_Default = Construction(Analytical.PanelType.SlabOnGrade);

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

                Geometry.Spatial.Plane plane = tuple.Item2.GetFace3D().GetPlane();

                List<Tuple<Polygon, Panel>> tuples_Polygon = new List<Tuple<Polygon, Panel>>();
                foreach (Tuple<double, Panel> tuple_Temp in tuples_Offset)
                {
                    Panel panel = tuple_Temp.Item2;

                    Geometry.Spatial.Face3D face3D = plane.Project(panel.GetFace3D());

                    Face2D face2D = plane.Convert(face3D);

                    tuples_Polygon.Add(new Tuple<Polygon, Panel>(face2D.ToNTS(tolerance), panel));
                }

                List<Polygon> polygons_Temp = tuples_Polygon.ConvertAll(x => x.Item1);
                Modify.RemoveAlmostSimilar_NTS(polygons_Temp, tolerance);

                List<Polygon> polygons = Geometry.Convert.ToNTS_Polygons(polygons_Temp, tolerance);
                if (polygons != null || polygons.Count > 0)
                {
                    Dictionary<Construction, List<Tuple<Panel, Polygon>>> dictionary = new Dictionary<Construction, List<Tuple<Panel, Polygon>>>();

                    foreach (Polygon polygon in polygons)
                    {
                        Point point = polygon.InteriorPoint;

                        List<Tuple<Polygon, Panel>> tuples_Polygon_Contains = tuples_Polygon.FindAll(x => x.Item1.Contains(point));
                        int count = tuples_Polygon_Contains.Count;
                        if (count == 0)
                            continue;

                        Panel panel_Old = null;

                        if (count == 1)
                        {
                            panel_Old = tuples_Polygon_Contains[0].Item2;
                            if (PanelGroup(panel_Old.PanelType) == Analytical.PanelGroup.Floor)
                            {
                                if (panel_Old.MinElevation() < Core.Tolerance.MacroDistance)
                                {
                                    //SlabOnGrad
                                    panel_Old = new Panel(panel_Old, construction_SlabOnGrade_Default);
                                }
                                else
                                {
                                    //Exposed
                                    panel_Old = new Panel(panel_Old, construction_FloorExposed_Default);
                                }
                            }
                        }
                        else
                        {
                            List<Tuple<Polygon, Panel>> tuples_Temp = tuples_Polygon_Contains.FindAll(x => PanelGroup(x.Item2.PanelType) == Analytical.PanelGroup.Floor);
                            if (tuples_Temp == null || tuples_Temp.Count == 0)
                            {
                                panel_Old = tuples_Polygon_Contains[0].Item2;
                            }
                            else
                            {
                                //FloorInternal
                                panel_Old = tuples_Temp.Find(x => x.Item2.PanelType == Analytical.PanelType.FloorInternal)?.Item2;
                                if (panel_Old == null)
                                    panel_Old = new Panel(tuples_Temp.First().Item2, construction_FloorInternal_Default);
                            }
                        }

                        if (panel_Old == null)
                            continue;

                        Geometry.Spatial.Face3D face3D = new Geometry.Spatial.Face3D(plane, polygon.ToSAM());
                        Guid guid = panel_Old.Guid;
                        if (guids.Contains(guid))
                            guid = Guid.NewGuid();

                        Panel panel_New = new Panel(guid, panel_Old, face3D);

                        result.Add(panel_New);
                    }
                }
            }

            return result;
        }
    }
}