using SAM.Geometry.Spatial;
using System.Collections.Generic;

namespace SAM.Architectural
{
    public static partial class Modify
    {
        public static bool TryAddOpening(this ArchitecturalModel architecturalModel, IOpening opening, out IHostPartition hostPartition, double tolerance = Core.Tolerance.Distance)
        {
            hostPartition = null;
            
            if (architecturalModel == null || opening == null)
            {
                return false;
            }

            BoundingBox3D boundingBox3D = opening.Face3D?.GetBoundingBox();
            if (boundingBox3D == null)
            {
                return false;
            }

            List<IHostPartition> hostPartitions = architecturalModel.GetPartitions<IHostPartition>();
            if (hostPartitions == null)
            {
                return false;
            }

            foreach (IHostPartition hostPartition_Temp in hostPartitions)
            {
                BoundingBox3D boundingBox3D_HostPartition = hostPartition_Temp?.Face3D?.GetBoundingBox();
                if (boundingBox3D_HostPartition == null)
                {
                    continue;
                }

                if (!boundingBox3D_HostPartition.InRange(boundingBox3D, tolerance))
                {
                    continue;
                }

                if (hostPartition_Temp.IsValid(opening, tolerance))
                {
                    if (hostPartition_Temp.AddOpening(opening, tolerance))
                    {
                        hostPartition = hostPartition_Temp;
                        architecturalModel.Add(hostPartition);
                        return true;
                    }
                }
            }

            return false;
        }

        public static bool TryAddOpening(this IHostPartition hostPartition, IOpening opening, double tolerance = Core.Tolerance.Distance)
        {
            if(hostPartition == null || opening == null)
            {
                return false;
            }

            if(!hostPartition.IsValid(opening, tolerance))
            {
                return false;
            }

            return hostPartition.AddOpening(opening, tolerance);
        }

        public static bool TryAddOpening(this ArchitecturalModel architecturalModel, IOpening opening, double tolerance = Core.Tolerance.Distance)
        {
            return TryAddOpening(architecturalModel, opening, out IHostPartition hostPartition, tolerance);
        }
    }
}