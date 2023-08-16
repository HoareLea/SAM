using SAM.Analytical.Grasshopper.Properties;
using SAM.Core.Grasshopper;
using System;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalNCMSystemType : GH_SAMEnumComponent<NCMSystemType>
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("91de3898-8dbd-4bd3-89cd-5da459f4cbbc");

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
        public SAMAnalyticalNCMSystemType()
          : base("SAMAnalytical.NCMSystemType", "SAMAnalytical.NCMSystemType",
              "Select NCMSystemType",
              "SAM", "Analytical")
        {
        }
    }
}