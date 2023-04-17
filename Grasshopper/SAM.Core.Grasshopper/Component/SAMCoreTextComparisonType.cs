using SAM.Core.Grasshopper.Properties;
using System;

namespace SAM.Core.Grasshopper
{
    public class SAMCoreTextComparisonType : GH_SAMEnumComponent<TextComparisonType>
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("79fc1ff3-7d7b-4cfd-8e77-a7c64a46ad43");

        /// <summary>
        /// The latest version of this component
        /// </summary>
        public override string LatestComponentVersion => "1.0.1";

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.SAM_Small3;

        /// <summary>
        /// Text Comparison Type Enum Component
        /// </summary>
        public SAMCoreTextComparisonType()
          : base("SAMCore.TextComparisonType", "SAMCore.TextComparisonType",
              "Select Text Comparison Type",
              "SAM", "Core")
        {
        }
    }
}