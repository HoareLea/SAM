using SAM.Core;
using SAM.Core.Grasshopper;
using SAM.Core.Grasshopper.Properties;
using System;

namespace SAM.Analytical.Grasshopper
{
    public class SAMCoreCombineType : GH_SAMEnumComponent<CombineType>
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("02f668ca-a859-4168-a020-b434dff7a229");

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
        public SAMCoreCombineType()
          : base("SAMCore.CombineType", "SAMCore.CombineType",
              "Select CombineType",
              "SAM", "Core")
        {
        }
    }
}