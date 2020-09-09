using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical
{
    public static partial class Query
    {
        public static double Volume(this Space space, AdjacencyCluster adjacencyCluster)
        {
            if (space == null)
                return double.NaN;

            double volume = double.NaN;
            if (Core.Query.TryGetValue(space, ParameterName_Volume(), out volume))
                return volume;

            //TODO: Find better way to calculate volume from panels

            List<Panel> panels_All = adjacencyCluster.GetPanels(space);
            if (panels_All == null || panels_All.Count == 0)
                return double.NaN;

            List<Panel> panels = panels_All.FindAll(x => PanelGroup(PanelType(x.Normal)) == Analytical.PanelGroup.Floor);
            if (panels.Count == 0)
                return double.NaN;

            double area = panels.ConvertAll(x => x.GetArea()).Sum();
            if (area <= 0)
                return double.NaN;

            double elevation_Min = MinElevation(panels);
            double elevation_Max = MaxElevation(panels);

            return area * (elevation_Max - elevation_Min);
        }
    }
}