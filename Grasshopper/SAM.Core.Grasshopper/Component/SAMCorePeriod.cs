using SAM.Core;
using SAM.Core.Grasshopper;
using SAM.Core.Grasshopper.Properties;
using System;

namespace SAM.Analytical.Grasshopper
{
    public class SAMCorePeriod : GH_SAMEnumComponent<Period>
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("e22b6c3b-818e-4dcb-8c27-b22c30de083d");

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
        public SAMCorePeriod()
          : base("SAMCore.Period", "SAMCore.Period",
              "Select Period",
              "SAM", "Core")
        {
        }
    }
}