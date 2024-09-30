using SAM.Analytical.Grasshopper.Properties;
using SAM.Core.Grasshopper;
using System;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalPanelType : GH_SAMEnumComponent<PanelType>
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("25a6b405-19ab-4ff1-9666-7760997ccfdd");

        /// <summary>
        /// The latest version of this component
        /// </summary>
        public override string LatestComponentVersion => "1.0.1";

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.SAM_Small;

        /// <summary>
        /// Panel Type Component
        /// </summary>
        public SAMAnalyticalPanelType()
          : base("SAMAnalytical.PanelType", "SAMAnalytical.PanelType",
              "Select Panel Type",
              "SAM", "Analytical03")
        {
        }
    }
}