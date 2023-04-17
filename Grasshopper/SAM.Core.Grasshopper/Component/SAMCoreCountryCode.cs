using SAM.Core.Grasshopper.Properties;
using System;

namespace SAM.Core.Grasshopper
{
    public class SAMCoreCoutryCode : GH_SAMEnumComponent<CountryCode>
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("24cc64c7-ac78-424a-914c-49166de992f3");

        /// <summary>
        /// The latest version of this component
        /// </summary>
        public override string LatestComponentVersion => "1.0.0";

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.SAM_Small3;

        /// <summary>
        /// Text Comparison Type Enum Component
        /// </summary>
        public SAMCoreCoutryCode()
          : base("SAMCore.CoutryCode", "SAMCore.CoutryCode",
              "Select CoutryCode",
              "SAM", "Core")
        {
        }
    }
}