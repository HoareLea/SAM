using SAM.Core;
using System.Collections.Generic;

namespace SAM.Analytical
{
    public static partial class Modify
    {
        public static List<IHostPartition> ReplaceTransparentPartitions(this ArchitecturalModel architecturalModel, double offset = 0, double tolerance_Angle = Tolerance.Angle, double tolerance_Distance = Tolerance.Distance)
        {
            if (architecturalModel == null)
            {
                return null;
            }

            List<HostPartitionType> hostPartitionTypes = architecturalModel.GetHostPartitionTypes();
            if(hostPartitionTypes == null || hostPartitionTypes.Count == 0)
            {
                return null;
            }

            List<IHostPartition> result = new List<IHostPartition>();
            foreach(HostPartitionType hostPartitionType in hostPartitionTypes)
            {
                if(hostPartitionType == null)
                {
                    continue;
                }

                if(!architecturalModel.Transparent(hostPartitionType))
                {
                    continue;
                }

                List<IHostPartition> hostPartitions = architecturalModel.GetHostPartitions(hostPartitionType);
                if(hostPartitions == null || hostPartitions.Count == 0)
                {
                    continue;
                }

                WindowType windowType = new WindowType(hostPartitionType.Name, hostPartitionType.MaterialLayers);

                foreach(IHostPartition hostPartition in hostPartitions)
                {
                    PartitionAnalyticalType partitionAnalyticalType = architecturalModel.PartitionAnalyticalType(hostPartition, tolerance_Angle, tolerance_Distance);
                    if(partitionAnalyticalType == PartitionAnalyticalType.Undefined)
                    {
                        continue;
                    }

                    HostPartitionType hostPartitionType_Default = Query.DefaultHostPartitionType(partitionAnalyticalType);
                    if(hostPartitionType_Default == null)
                    {
                        continue;
                    }

                    Geometry.Spatial.IClosedPlanar3D externalEdge3D = hostPartition.Face3D?.GetExternalEdge3D();
                    if(externalEdge3D == null)
                    {
                        continue;
                    }

                    Geometry.Spatial.Face3D face3D = new Geometry.Spatial.Face3D(externalEdge3D);

                    hostPartition.Type(hostPartitionType_Default);

                    List<IOpening> openings = hostPartition.GetOpenings();
                    if(openings != null && openings.Count != 0)
                    {
                        openings.ForEach(x => hostPartition.RemoveOpening(x.Guid));
                    }

                    List<Geometry.Spatial.Face3D> face3Ds = null;
                    if (offset == 0)
                    {
                        face3Ds = new List<Geometry.Spatial.Face3D>() { face3D };
                    }
                    else
                    {
                        face3Ds = Geometry.Spatial.Query.Offset(face3D, offset, true, false, tolerance_Distance);
                    }

                    foreach(Geometry.Spatial.Face3D face3D_New in face3Ds)
                    {
                        Window window = new Window(windowType, face3D_New);
                        hostPartition.AddOpening(window, tolerance_Distance);
                    }

                    architecturalModel.Add(hostPartition);
                    result.Add(architecturalModel.GetObject<IHostPartition>(hostPartition.Guid));
                }
            }

            return result;
        }
    }
}