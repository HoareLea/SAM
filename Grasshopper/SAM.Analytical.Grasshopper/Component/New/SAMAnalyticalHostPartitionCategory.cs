using Grasshopper.Kernel;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core.Grasshopper;
using System;

namespace SAM.Analytical.Grasshopper
{
    [Obsolete("Obsolete since 2021.11.24")]
    public class SAMAnalyticalHostPartitionCategory : GH_SAMEnumComponent<HostPartitionCategory>
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("42dce4ad-8eba-4808-b24f-00c85540d052");

        /// <summary>
        /// The latest version of this component
        /// </summary>
        public override string LatestComponentVersion => "1.0.0";

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.SAM_Small;

        public override GH_Exposure Exposure => GH_Exposure.hidden;

        /// <summary>
        /// Panel Type
        /// </summary>
        public SAMAnalyticalHostPartitionCategory()
          : base("SAMAnalytical.HostPartitionCategory", "SAMAnalytical.HostPartitionCategory",
              "Select HostPartitionCategory",
              "SAM WIP", "Analytical")
        {
        }
    }
}