using SAM.Core.Grasshopper.Properties;
using System;

namespace SAM.Core.Grasshopper
{
    public class SAMCoreNumberComparisonType : GH_SAMEnumComponent<NumberComparisonType>
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("4232f771-1d7f-4bf9-a07c-3ac868a82812");

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
        public SAMCoreNumberComparisonType()
          : base("SAMCore.NumberComparisonType", "SAMCore.NumberComparisonType",
              "Select Number Comparison Type",
              "SAM", "Core")
        {
        }
    }
}