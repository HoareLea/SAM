using SAM.Analytical.Grasshopper.Properties;
using SAM.Core.Grasshopper;
using System;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalInternalConditionParameter : GH_SAMEnumComponent<InternalConditionParameter>
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("6c9b04c9-177d-47b2-bb95-13b3fd858fd2");

        /// <summary>
        /// The latest version of this component
        /// </summary>
        public override string LatestComponentVersion => "1.0.1";

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.SAM_Small;

        /// <summary>
        /// Internal Condition Parameter Component
        /// </summary>
        public SAMAnalyticalInternalConditionParameter()
          : base("SAMAnalytical.InternalConditionParameter", "SAMAnalytical.InternalConditionParameter",
              "Select InternalConditionParameter",
              "SAM", "Analytical02")
        {
        }
    }
}