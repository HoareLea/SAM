using SAM.Analytical.Grasshopper.Properties;
using SAM.Core.Grasshopper;
using System;

namespace SAM.Analytical.Grasshopper
{
    public class SAMArchitecturalHostPartitionCategory : GH_SAMEnumComponent<HostPartitionCategory>
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

        /// <summary>
        /// Panel Type
        /// </summary>
        public SAMArchitecturalHostPartitionCategory()
          : base("SAMArchitectural.HostPartitionCategory", "SAMArchitectural.HostPartitionCategory",
              "Select HostPartitionCategory",
              "SAM", "Architectural")
        {
        }
    }
}