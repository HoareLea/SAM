using SAM.Geometry.Spatial;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical
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

            Face3D face3D = opening.Face3D;
            if (face3D == null)
            {
                return false;
            }

            BoundingBox3D boundingBox3D = face3D.GetBoundingBox();
            if (boundingBox3D == null)
            {
                return false;
            }

            List<IHostPartition> hostPartitions = architecturalModel.GetPartitions<IHostPartition>();
            if (hostPartitions == null)
            {
                return false;
            }

            double area = face3D.GetArea();

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

                List<IOpening> openings_Add = hostPartition_Temp.AddOpening(opening, tolerance);
                if(openings_Add == null || openings_Add.Count == 0)
                {
                    continue;
                }

                double area_Temp = openings_Add.ConvertAll(x => x.Face3D.GetArea()).Sum();

                if (area - area_Temp - tolerance <= 0)
                {
                    hostPartition = hostPartition_Temp;
                    architecturalModel.Add(hostPartition);
                    return true;
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

            List<IOpening> openings = hostPartition.AddOpening(opening, tolerance);

            return openings != null && openings.Count != 0;
        }

        public static bool TryAddOpening(this ArchitecturalModel architecturalModel, IOpening opening, double tolerance = Core.Tolerance.Distance)
        {
            return TryAddOpening(architecturalModel, opening, out IHostPartition hostPartition, tolerance);
        }
    }
}