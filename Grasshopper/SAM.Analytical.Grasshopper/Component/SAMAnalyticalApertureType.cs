using SAM.Analytical.Grasshopper.Properties;
using SAM.Core.Grasshopper;
using System;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalApertureType : GH_SAMEnumComponent<ApertureType>
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("6398c87e-ec05-44ab-a35b-09f09f08ff38");

        /// <summary>
        /// The latest version of this component
        /// </summary>
        public override string LatestComponentVersion => "1.0.1";

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.SAM_Small;

        /// <summary>
        /// Panel Type
        /// </summary>
        public SAMAnalyticalApertureType()
          : base("SAMAnalytical.ApertureType", "SAMAnalytical.ApertureType",
              "Select Aperture Type",
              "SAM", "Analytical")
        {
        }
    }
}