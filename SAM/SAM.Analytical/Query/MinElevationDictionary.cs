using SAM.Core;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical
{
    public static partial class Query
    {
        public static Dictionary<double, List<Panel>> MinElevationDictionary(this IEnumerable<Panel> panels, double tolerance = Tolerance.MicroDistance)
        {
            if (panels == null)
                return null;

            Dictionary<double, List<Panel>> result = new Dictionary<double, List<Panel>>();
            foreach (Panel panel in panels)
            {               
                double minElevation = Core.Query.Round(panel.MinElevation(), tolerance);

                List<Panel> panels_Elevation = null;
                if (!result.TryGetValue(minElevation, out panels_Elevation))
                {
                    panels_Elevation = new List<Panel>();
                    result[minElevation] = panels_Elevation;
                }

                panels_Elevation.Add(panel);
            }

            return result;
        }

        public static Dictionary<double, List<Panel>> MinElevationDictionary(this IEnumerable<Panel> panels, bool filterElevations, double tolerance = Tolerance.MicroDistance)
        {
            if (panels == null)
                return null;

            List<Panel> panels_Temp = panels.ToList();
            if (panels_Temp.Count == 0)
                return new Dictionary<double, List<Panel>>();

            List<Panel> panels_Levels = null;

            if (filterElevations)
            {
                panels_Levels = new List<Panel>();
                foreach(Panel panel in panels_Temp)
                {
                    PanelGroup panelGroup = PanelGroup(panel.PanelType);
                    if (panelGroup == Analytical.PanelGroup.Undefined)
                        continue;

                    if (panelGroup == Analytical.PanelGroup.Floor)
                    {
                        panels_Levels.Add(panel);
                        continue;
                    }

                    if (panel.PanelType != Analytical.PanelType.Air)
                        continue;

                    Geometry.Spatial.Vector3D normal = panel.Normal;
                    if (normal == null)
                        continue;

                    if(normal.AlmostEqual(Geometry.Spatial.Vector3D.WorldZ) || normal.GetNegated().AlmostEqual(Geometry.Spatial.Vector3D.WorldZ) )
                    {
                        panels_Levels.Add(panel);
                        continue;
                    }
                }

                if (panels_Levels.Count == 0)
                    panels_Levels = panels_Temp.FindAll(x => PanelGroup(x.PanelType) == Analytical.PanelGroup.Wall && x.PanelType != Analytical.PanelType.CurtainWall);

                if (panels_Levels.Count == 0)
                    panels_Levels = panels_Temp.FindAll(x => PanelGroup(x.PanelType) == Analytical.PanelGroup.Wall);
            }

            if (panels_Levels == null || panels_Levels.Count == 0)
                panels_Levels = panels_Temp;

            if (panels_Levels == null || panels_Levels.Count == 0)
                return new Dictionary<double, List<Panel>>();


            //Dictionary of Minimal Elevations and List of Panels
            Dictionary<double, List<Panel>> result = MinElevationDictionary(panels_Levels, tolerance);
            List<double> elevations = result.Keys.ToList();

            foreach (Panel panel in panels)
            {
                if (panels_Levels.Contains(panel))
                    continue;

                double elevation = panel.MinElevation();
                List<double> differences = elevations.ConvertAll(x => System.Math.Abs(x - elevation));

                int index = differences.IndexOf(differences.Min());
                result.Values.ElementAt(index).Add(panel);
            }

            return result;
        }
    }
}