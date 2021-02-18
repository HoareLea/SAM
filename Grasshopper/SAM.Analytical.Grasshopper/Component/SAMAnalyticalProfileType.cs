using SAM.Analytical.Grasshopper.Properties;
using SAM.Core.Grasshopper;
using System;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalProfileType : GH_SAMEnumComponent<ProfileType>
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("13f7d6c7-b06a-4b20-a1a6-e8820e066ca4");

        /// <summary>
        /// The latest version of this component
        /// </summary>
        public override string LatestComponentVersion => "1.0.1";

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.SAM_Small;

        /// <summary>
        /// Clear Option Enum Component
        /// </summary>
        public SAMAnalyticalProfileType()
          : base("SAMAnalytical.ProfileType", "SAMAnalytical.ProfileType",
              "Select ProfileType",
              "SAM", "Analytical")
        {
        }
    }
}