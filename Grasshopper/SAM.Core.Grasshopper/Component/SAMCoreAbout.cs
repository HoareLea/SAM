using SAM.Core.Grasshopper.Properties;
using System;

namespace SAM.Core.Grasshopper
{
    public class SAMCoreAbout : GH_SAMEnumComponent<AboutInfoType>
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("266c727d-0d2c-4592-a483-0761c03fcdb9");

        /// <summary>
        /// The latest version of this component
        /// </summary>
        public override string LatestComponentVersion => "1.0.1";

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.SAM_Small3;

        /// <summary>
        /// About SAM Enum Component
        /// </summary>
        public SAMCoreAbout()
          : base("SAM.About", "SAM.About",
              "Right click to find out more about our toolkit",
              "SAM", "About")
        {
        }
    }
}