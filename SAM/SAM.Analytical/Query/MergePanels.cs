using System;
using System.Collections.Generic;

namespace SAM.Analytical
{
    public static partial class Query
    {
        public static List<Panel> MergePanels(this IEnumerable<Panel> panels, double offset)
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


            }
            

        }
    }
}
