using SAM.Analytical.Grasshopper.Properties;
using SAM.Core.Grasshopper;
using System;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalTM52BuildingCategory : GH_SAMEnumComponent<TM52BuildingCategory>
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("94ca0b6d-4160-4c12-bf85-ed190f4b99aa");

        /// <summary>
        /// The latest version of this component
        /// </summary>
        public override string LatestComponentVersion => "1.0.0";

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
                protected override System.Drawing.Bitmap Icon => Core.Convert.ToBitmap(Resources.SAM_Small);

        /// <summary>
        /// Panel Type
        /// </summary>
        public SAMAnalyticalTM52BuildingCategory()
          : base("SAMAnalytical.TM52BuildingCategory", "SAMAnalytical.TM52BuildingCategory",
              "TM52 Building Category",
              "SAM", "Analytical03")
        {
        }
    }
}