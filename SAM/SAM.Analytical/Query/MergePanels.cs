using NetTopologySuite.Geometries;
using SAM.Geometry;
using SAM.Geometry.Planar;
using System;
using System.Collections.Generic;

namespace SAM.Analytical
{
    public static partial class Query
    {
        public static List<Panel> MergePanels(this IEnumerable<Panel> panels, double offset, double tolerance = Core.Tolerance.MicroDistance)
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

                List<Tuple<LinearRing, Panel>> tuples_LinearRing = new List<Tuple<LinearRing, Panel>>();
                foreach (Tuple<double, Panel> tuple_Temp in tuples_Offset)
                {
                    Panel panel = tuple_Temp.Item2;

                    Geometry.Spatial.IClosedPlanar3D closedPlanar3D = plane.Project(panel.GetFace3D().GetExternalEdge());

                    Geometry.Spatial.Polygon3D polygon3D = closedPlanar3D as Geometry.Spatial.Polygon3D;
                    if (polygon3D == null)
                        continue;

                    Polygon2D polygon2D = plane.Convert(polygon3D);

                    tuples_LinearRing.Add(new Tuple<LinearRing, Panel>(polygon2D.ToNetTopologySuite(tolerance), panel));
                }


                
                List<Polygon> polygons = Geometry.Convert.ToNetTopologySuite_Polygons(tuples_LinearRing.ConvertAll(x => x.Item1));
                if(polygons != null || polygons.Count > 0)
                {
                    foreach(Polygon polygon in polygons)
                    {
                        Point point = polygon.InteriorPoint;

                        Point2D point2D = Geometry.Convert.ToSAM(polygon.InteriorPoint);

                        List<Tuple<LinearRing, Panel>> tuples_LinearRing_Within = tuples_LinearRing.FindAll(x => x.Item1.ToSAM().Inside(point2D));
                        int count = tuples_LinearRing_Within.Count;
                        if (count == 0)
                            continue;

                        Panel panel_Old = null;

                        if (count == 1)
                        {
                            panel_Old = tuples_LinearRing_Within[0].Item2;
                        }
                        else
                        {
                            Tuple<LinearRing, Panel> tuple_Temp = tuples_LinearRing_Within.Find(x => PanelGroup(x.Item2.PanelType) == Analytical.PanelGroup.Floor);
                            if(tuple_Temp == null)
                            {
                                panel_Old = tuples_LinearRing_Within[0].Item2;
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
                            if (result.Find(x => x.Guid == guid) != null)
                                guid = Guid.NewGuid();

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
