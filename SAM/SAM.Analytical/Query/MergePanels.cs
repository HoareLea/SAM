using NetTopologySuite.Geometries;
using SAM.Geometry;
using SAM.Geometry.Planar;
using System;
using System.Collections.Generic;

namespace SAM.Analytical
{
    public static partial class Query
    {
        public static List<Panel> MergePanels(this IEnumerable<Panel> panels, double offset, double tolerance = Core.Tolerance.Distance)
        {
            if (panels == null)
                return null;
            
            Dictionary<PanelGroup, List<Panel>> dictionary = new Dictionary<PanelGroup, List<Panel>>();
            foreach (PanelGroup panelGroup in Enum.GetValues(typeof(PanelGroup)))
                dictionary[panelGroup] = new List<Panel>();

            foreach(Panel panel in panels)
            {
                if (panel == null)
                    continue;

                dictionary[Query.PanelGroup(panel.PanelType)].Add(panel);
            }

            List<Panel> panels_Temp = dictionary[Analytical.PanelGroup.Floor];
            panels_Temp.AddRange(dictionary[Analytical.PanelGroup.Roof]);

            List<Tuple<double, Panel>> tuples = new List<Tuple<double, Panel>>();
            foreach(Panel panel in panels_Temp)
            {
                double elevation = panel.MaxElevation();

                tuples.Add(new Tuple<double, Panel>(elevation, panel));
            }

            List<Panel> result = new List<Panel>();

            tuples.Sort((x, y) => x.Item1.CompareTo(y.Item1));
            while(tuples.Count > 0)
            {
                Tuple<double, Panel> tuple = tuples[0];
                tuples.RemoveAt(0);

                double elevation = tuple.Item1;
                double elevation_Offset = elevation + offset;
                List<Tuple<double, Panel>> tuples_Offset = new List<Tuple<double, Panel>>();
                foreach(Tuple<double, Panel> tuple_Temp in tuples)
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

                    Geometry.Spatial.IClosedPlanar3D closedPlanar3D = plane.Project(panel.GetFace3D().GetExternalEdge());

                    Geometry.Spatial.Polygon3D polygon3D = closedPlanar3D as Geometry.Spatial.Polygon3D;
                    if (polygon3D == null)
                        continue;

                    Polygon2D polygon2D = plane.Convert(polygon3D);

                    tuples_Polygon.Add(new Tuple<Polygon, Panel>(polygon2D.ToNetTopologySuite_Polygon(tolerance), panel));
                }

                List<Polygon> polygons_Temp = tuples_Polygon.ConvertAll(x => x.Item1);
                Geometry.Planar.Modify.RemoveAlmostSimilar(polygons_Temp, tolerance);

                List<Polygon> polygons = Geometry.Convert.ToNetTopologySuite_Polygons(polygons_Temp);
                if(polygons != null || polygons.Count > 0)
                {
                    HashSet<Guid> guids = new HashSet<Guid>();
                    
                    foreach(Polygon polygon in polygons)
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
                        }
                        else
                        {
                            Tuple<Polygon, Panel> tuple_Temp = tuples_Polygon_Contains.Find(x => PanelGroup(x.Item2.PanelType) == Analytical.PanelGroup.Floor);
                            if(tuple_Temp == null)
                            {
                                panel_Old = tuples_Polygon_Contains[0].Item2;
                            }
                            else
                            {
                                panel_Old = tuple_Temp.Item2;
                            }
                        }

                        if (panel_Old == null)
                            continue;

                        foreach(Polygon2D polygon2D in polygon.ToSAM())
                        {
                            Geometry.Spatial.Face3D face3D = new Geometry.Spatial.Face3D(plane, polygon2D);
                            Guid guid = panel_Old.Guid;
                            if (guids.Contains(guid))
                                guid = Guid.NewGuid();

                            guids.Add(guid);

                            Panel panel_New = new Panel(guid, panel_Old, face3D);
                            result.Add(panel_New);
                        }
                    }
                }
            }

            return result;
        }
    }
}
