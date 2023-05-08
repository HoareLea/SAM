using Rhino;
using System;

namespace SAM.Analytical.Grasshopper
{
    public static partial class Modify
    {
        [Obsolete("Obsolete since 2021.11.24")]
        public static void BakeGeometry_ByAnalyticalType(this RhinoDoc rhinoDoc, global::Grasshopper.Kernel.Data.IGH_Structure gH_Structure, bool cutOpenings, double tolerance = Core.Tolerance.Distance)
        {
            if (rhinoDoc == null)
                return;

            foreach (var variable in gH_Structure.AllData(true))
            {
                if (variable is GooBuildingModel)
                {
                    BuildingModel buildingModel = ((GooBuildingModel)variable).Value;
                    if (buildingModel != null)
                    {
                        Rhino.Modify.BakeGeometry_ByAnalyticalType(rhinoDoc, buildingModel, cutOpenings, tolerance);
                    }
                }
            }
        }
    }
}