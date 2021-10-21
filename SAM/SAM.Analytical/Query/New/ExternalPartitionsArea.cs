using System.Collections.Generic;

namespace SAM.Analytical
{
    public static partial class Query
    {
        public static double ExternalPartitionsArea(this ArchitecturalModel architecturalModel, Space space)
        {
            if (architecturalModel == null || space == null)
                return double.NaN;

            List<IHostPartition> hostPartitions = architecturalModel.GetObjects((IHostPartition hostPartition) => !architecturalModel.Underground(hostPartition) && architecturalModel.External(hostPartition));
            if (hostPartitions == null || hostPartitions.Count == 0)
                return double.NaN;

            double result = 0;
            foreach(IHostPartition hostPartition in hostPartitions)
            {

                Geometry.Spatial.Face3D face3D = hostPartition?.Face3D;
                if (face3D == null)
                    continue;
                
                double area = face3D.GetArea();
                if (double.IsNaN(area) || area == 0)
                    continue;

                result += area;
            }

            return result;
        }
    }
}