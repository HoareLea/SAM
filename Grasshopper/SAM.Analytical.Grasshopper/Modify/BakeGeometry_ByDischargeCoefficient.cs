using Rhino;
using System.Collections.Generic;

namespace SAM.Analytical.Grasshopper
{
    public static partial class Modify
    {
        public static void BakeGeometry_ByDischargeCoefficient(this RhinoDoc rhinoDoc, global::Grasshopper.Kernel.Data.IGH_Structure gH_Structure)
        {
            if (rhinoDoc == null)
                return;

            List<Aperture> apertures = new List<Aperture>();

            List<Panel> panels = new List<Panel>();
            foreach (var variable in gH_Structure.AllData(true))
            {
                if (variable is GooAperture)
                {
                    apertures.Add(((GooAperture)variable).Value);
                }
                else if(variable is GooPanel)
                {
                    panels.Add(((GooPanel)variable).Value);
                }
                else if (variable is GooAdjacencyCluster)
                {
                    List<Panel> panels_Temp = ((GooAdjacencyCluster)variable).Value?.GetPanels();
                    if (panels_Temp != null && panels_Temp.Count != 0)
                        panels.AddRange(panels_Temp);
                }
                else if (variable is GooAnalyticalModel)
                {
                    List<Panel> panels_Temp = ((GooAnalyticalModel)variable).Value?.AdjacencyCluster.GetPanels();
                    if (panels_Temp != null && panels_Temp.Count != 0)
                        panels.AddRange(panels_Temp);
                }
            }


            foreach(Panel panel in panels)
            {
                List<Aperture> apertures_Panel = panel?.Apertures;
                if(apertures_Panel == null)
                {
                    continue;
                }

                apertures.AddRange(apertures_Panel);
            }


            Rhino.Modify.BakeGeometry_ByDischargeCoefficient(rhinoDoc, apertures);
        }
  }
}