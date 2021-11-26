using System.Collections.Generic;

namespace SAM.Analytical
{
    public static partial class Query
    {
        public static double ExternalPartitionsArea(this BuildingModel buildingModel, Space space)
        {
            if (buildingModel == null || space == null)
                return double.NaN;

            List<IHostPartition> hostPartitions = buildingModel.GetObjects((IHostPartition hostPartition) => !buildingModel.Underground(hostPartition) && buildingModel.External(hostPartition));
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