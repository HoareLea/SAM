using SAM.Analytical.Grasshopper.Properties;
using SAM.Core.Grasshopper;
using System;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalPanelGroup : GH_SAMEnumComponent<PanelGroup>
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("ba1f2b22-7dc6-4dec-84c0-4027998b9683");

        /// <summary>
        /// The latest version of this component
        /// </summary>
        public override string LatestComponentVersion => "1.0.0";

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.SAM_Small;

        /// <summary>
        /// Panel Type
        /// </summary>
        public SAMAnalyticalPanelGroup()
          : base("SAMAnalytical.PanelGroup", "SAMAnalytical.PanelGroup",
              "Select PanelGroup",
              "SAM WIP", "Analytical")
        {
        }
    }
}