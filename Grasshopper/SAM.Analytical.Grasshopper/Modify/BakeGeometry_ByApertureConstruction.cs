using Rhino;
using System.Collections.Generic;

namespace SAM.Analytical.Grasshopper
{
    public static partial class Modify
    {
        public static void BakeGeometry_ByApertureConstruction(this RhinoDoc rhinoDoc, global::Grasshopper.Kernel.Data.IGH_Structure gH_Structure, bool includeFrame = false)
        {
            if (rhinoDoc == null)
                return;

            List<Aperture> apertures = new List<Aperture>();
            foreach (var variable in gH_Structure.AllData(true))
            {
                if (variable is GooPanel)
                {
                    List<Aperture> apertures_Temp = ((GooPanel)variable).Value.Apertures;
                    if(apertures_Temp != null)
                    {
                        apertures.AddRange(apertures_Temp);
                    }
                }
                else if (variable is GooAperture)
                {
                    apertures.Add(((GooAperture)variable).Value);
                }
                else if (variable is GooAdjacencyCluster)
                {
                    List<Aperture> apertures_Temp = ((GooAdjacencyCluster)variable).Value?.GetApertures();
                    if (apertures_Temp != null && apertures_Temp.Count != 0)
                        apertures.AddRange(apertures_Temp);
                }
                else if (variable is GooAnalyticalModel)
                {
                    List<Aperture> apertures_Temp = ((GooAnalyticalModel)variable).Value?.AdjacencyCluster.GetApertures();
                    if (apertures_Temp != null && apertures_Temp.Count != 0)
                        apertures.AddRange(apertures_Temp);
                }
            }

            Rhino.Modify.BakeGeometry_ByApertureConstruction(rhinoDoc, apertures, includeFrame);
        }
  }
}