using SAM.Core;
using SAM.Core.Grasshopper;
using SAM.Core.Grasshopper.Properties;
using System;

namespace SAM.Analytical.Grasshopper
{
    public class SAMCoreDirection : GH_SAMEnumComponent<Direction>
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("4ac990e2-e42c-4b4c-b336-b45763f0a9cd");

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
        public SAMCoreDirection()
          : base("SAMCore.Direction", "SAMCore.Direction",
              "Select Direction",
              "SAM", "Core")
        {
        }
    }
}