using SAM.Analytical.Grasshopper.Properties;
using SAM.Core.Grasshopper;
using System;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalZoneType : GH_SAMEnumComponent<ZoneType>
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("a69a8102-c893-4c0b-bc55-fed129876bd7");

        /// <summary>
        /// The latest version of this component
        /// </summary>
        public override string LatestComponentVersion => "1.0.1";

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.SAM_Small;

        /// <summary>
        /// Zone Type Enum Component
        /// </summary>
        public SAMAnalyticalZoneType()
          : base("SAMAnalytical.ZoneType", "SAMAnalytical.ZoneType",
              "Select Zone Type",
              "SAM", "Analytical")
        {
        }
    }
}