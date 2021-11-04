using Rhino;

namespace SAM.Analytical.Grasshopper
{
    public static partial class Modify
    {
        public static void BakeGeometry_ByAnalyticalType(this RhinoDoc rhinoDoc, global::Grasshopper.Kernel.Data.IGH_Structure gH_Structure, bool cutOpenings, double tolerance = Core.Tolerance.Distance)
        {
            if (rhinoDoc == null)
                return;

            foreach (var variable in gH_Structure.AllData(true))
            {
                if (variable is GooArchitecturalModel)
                {
                    ArchitecturalModel architecturalModel = ((GooArchitecturalModel)variable).Value;
                    if (architecturalModel != null)
                    {
                        Rhino.Modify.BakeGeometry_ByAnalyticalType(rhinoDoc, architecturalModel, cutOpenings, tolerance);
                    }
                }
            }
        }
    }
}