using SAM.Analytical.Grasshopper.Properties;
using SAM.Core.Grasshopper;
using System;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalProfileGroup : GH_SAMEnumComponent<ProfileGroup>
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("80c68760-50f1-481e-a7f2-a8a02983407a");

        /// <summary>
        /// The latest version of this component
        /// </summary>
        public override string LatestComponentVersion => "1.0.1";

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.SAM_Small;

        /// <summary>
        /// Profile Group Component
        /// </summary>
        public SAMAnalyticalProfileGroup()
          : base("SAMAnalytical.ProfileGroup", "SAMAnalytical.ProfileGroup",
              "Select ProfileGroup",
              "SAM", "Analytical")
        {
        }
    }
}