using SAM.Analytical.Grasshopper.Properties;
using SAM.Core.Grasshopper;
using System;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalLightingOccupancyControls : GH_SAMEnumComponent<LightingOccupancyControls>
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("6ba25b70-4973-49b5-9945-8ea4b408d39c");

        /// <summary>
        /// The latest version of this component
        /// </summary>
        public override string LatestComponentVersion => "1.0.0";

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
                protected override System.Drawing.Bitmap Icon => Core.Convert.ToBitmap(Resources.SAM_Small);

        /// <summary>
        /// Panel Type
        /// </summary>
        public SAMAnalyticalLightingOccupancyControls()
          : base("SAMAnalytical.LightingOccupancyControls", "SAMAnalytical.LightingOccupancyControls",
              "Select LightingOccupancyControls",
              "SAM", "Analytical02")
        {
        }
    }
}