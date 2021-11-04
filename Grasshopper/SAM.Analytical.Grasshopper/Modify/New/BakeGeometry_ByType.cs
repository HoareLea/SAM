using Rhino;
using Rhino.DocObjects;
using System;
using System.Collections.Generic;

namespace SAM.Analytical.Grasshopper
{
    public static partial class Modify
    {
        public static void BakeGeometry_ByType(this RhinoDoc rhinoDoc, global::Grasshopper.Kernel.Data.IGH_Structure gH_Structure, bool cutOpenings = false, double tolerance = Core.Tolerance.Distance)
        {
            if (rhinoDoc == null)
                return;

            List<IPartition> partitions = new List<IPartition>();
            foreach (var variable in gH_Structure.AllData(true))
            {
                if (variable is GooPartition)
                {
                    partitions.Add(((GooPartition)variable).Value);
                }
                else if (variable is GooArchitecturalModel)
                {
                    ArchitecturalModel architecturalModel = ((GooArchitecturalModel)variable).Value;
                    if (architecturalModel != null)
                    {
                        List<IPartition> hostPartitions_Temp = architecturalModel.GetObjects<IPartition>();
                        if (hostPartitions_Temp != null && hostPartitions_Temp.Count > 0)
                        {
                            partitions.AddRange(hostPartitions_Temp);
                        }
                    }
                }
            }

            Rhino.Modify.BakeGeometry_ByType(rhinoDoc, partitions, cutOpenings, tolerance);
        }
    }
}