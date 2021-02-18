using SAM.Analytical.Grasshopper.Properties;
using SAM.Core.Grasshopper;
using System;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalHeatFlowDirection : GH_SAMEnumComponent<HeatFlowDirection>
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("0494cfc0-e0a2-401f-8c7c-d2629399d946");

        /// <summary>
        /// The latest version of this component
        /// </summary>
        public override string LatestComponentVersion => "1.0.1";

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.SAM_Small;

        /// <summary>
        /// Heat Flow Direction
        /// </summary>
        public SAMAnalyticalHeatFlowDirection()
          : base("SAMAnalytical.HeatFlowDirection", "SAMAnalytical.HeatFlowDirection",
              "Select Heat Flow Direction",
              "SAM", "Analytical")
        {
        }
    }
}