using SAM.Analytical.Grasshopper.Properties;
using SAM.Core.Grasshopper;
using System;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalDefaultGasType : GH_SAMEnumComponent<DefaultGasType>
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("99222429-db00-40d4-a021-783eea84ce0b");

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
        public SAMAnalyticalDefaultGasType()
          : base("SAMAnalytical.DefaultGasType", "SAMAnalytical.DefaultGasType",
              "Select Default Gas Type",
              "SAM", "Analytical")
        {
        }
    }
}