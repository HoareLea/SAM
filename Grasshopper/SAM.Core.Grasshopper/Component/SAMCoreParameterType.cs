using SAM.Core.Grasshopper.Properties;
using System;

namespace SAM.Core.Grasshopper
{
    public class SAMCoreParameterType : GH_SAMEnumComponent<ParameterType>
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("96271426-fb5c-4bf5-9877-9ca5adda133a");

        /// <summary>
        /// The latest version of this component
        /// </summary>
        public override string LatestComponentVersion => "1.0.0";

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.SAM_Small3;

        /// <summary>
        /// Clear Option Enum Component
        /// </summary>
        public SAMCoreParameterType()
          : base("SAMCore.ParameterType", "SAMCore.ParameterType",
              "Select ParameterType",
              "SAM", "Core")
        {
        }
    }
}