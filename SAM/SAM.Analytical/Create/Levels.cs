using SAM.Geometry.Spatial;
using System.Collections.Generic;

namespace SAM.Analytical
{
    public static partial class Create
    {
        public static List<Architectural.Level> Levels(this List<Panel> panels, double tolerance = Core.Tolerance.MacroDistance)
        {
            if (panels == null)
                return null;

            HashSet<double> elevations = new HashSet<double>();
            foreach(Panel panel in panels)
            {
                if (panel == null)
                    return null;

                double elevation = panel.MinElevation();
                if (double.IsNaN(elevation))
                    continue;

                elevation = Core.Query.Round(elevation, tolerance);

                elevations.Add(elevation);
            }

            List<Architectural.Level> result = new List<Architectural.Level>();
            foreach(double elevation in elevations)
                result.Add(Architectural.Create.Level(elevation));

            return result;
        }
    }
}