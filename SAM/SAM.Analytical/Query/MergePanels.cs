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
            foreach (PanelGroup panelGroup in System.Enum.GetValues(typeof(PanelType)))
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

                List<Tuple<Polygon2D, Panel>> tuples_Polygon3D = new List<Tuple<Polygon2D, Panel>>();
                foreach (Tuple<double, Panel> tuple_Temp in tuples_Offset)
                {
                    Panel panel = tuple_Temp.Item2;

                    Geometry.Spatial.IClosedPlanar3D closedPlanar3D = plane.Project(panel.GetFace3D().GetExternalEdge());

                    Geometry.Spatial.Polygon3D polygon3D = closedPlanar3D as Geometry.Spatial.Polygon3D;
                    if (polygon3D == null)
                        continue;

                    Polygon2D polygon2D = plane.Convert(polygon3D);

                    tuples_Polygon3D.Add(new Tuple<Polygon2D, Panel>(polygon2D, panel));
                }

                foreach(Tuple<Polygon2D, Panel> tuple_Temp in tuples_Polygon3D)
                {
                    
                    //List<Polygon2D> polygon2Ds = Geometry.Planar.Query.Intersection(tolerance);
                }


            }

            throw new NotImplementedException();
        }
    }
}
