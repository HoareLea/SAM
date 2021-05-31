using System.Collections.Generic;

namespace SAM.Analytical
{
    public static partial class Query
    {
        public static Dictionary<Architectural.Level, List<Panel>> LevelsDictionary(this List<Panel> panels, bool includeOtherPanels = false, double tolerance = Core.Tolerance.MacroDistance)
        {
            if (panels == null)
            {
                return null;
            }

            List<Architectural.Level> levels = Create.Levels(panels, includeOtherPanels, tolerance);
            if(levels == null)
            {
                return null;
            }

            Dictionary<Architectural.Level, List<Panel>> result = new Dictionary<Architectural.Level, List<Panel>>();
            foreach(Panel panel in panels)
            {
                if(panel == null)
                {
                    continue;
                }

                double elevation = panel.MinElevation();
                if (double.IsNaN(elevation))
                {
                    continue;
                }

                elevation = Core.Query.Round(elevation, tolerance);

                double distance = double.MaxValue;
                Architectural.Level level = null;
                foreach (Architectural.Level level_Temp in levels)
                {
                    double distance_Temp = System.Math.Abs(level_Temp.Elevation - elevation);

                    if (distance_Temp < distance)
                    {
                        distance = distance_Temp;
                        level = level_Temp;
                    }
                }

                if(level == null)
                {
                    continue;
                }

                if(!result.TryGetValue(level, out List<Panel> panels_Level))
                {
                    panels_Level = new List<Panel>();
                    result[level] = panels_Level;
                }

                panels_Level.Add(panel);
            }

            return result;
        }
    }
}