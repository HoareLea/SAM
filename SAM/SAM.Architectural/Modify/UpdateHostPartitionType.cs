using SAM.Core;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Architectural
{
    public static partial class Modify
    {
        public static List<IPartition> UpdateHostPartitionType(this ArchitecturalModel architecturalModel, HostPartitionType hostPartitionType, IEnumerable<Func<IPartition, bool>> functions, IEnumerable<IPartition> partitions = null, MaterialLibrary materialLibrary = null)
        {
            if (architecturalModel == null || hostPartitionType == null || functions == null)
                return null;

            List<Func<IPartition, bool>> functions_Temp = new List<Func<IPartition, bool>>(functions);

            Func<IPartition, bool> function_HostPartitionType = new Func<IPartition, bool>((IPartition partition) =>
            {
                if (partition is AirPartition)
                {
                    return true;
                }

                IHostPartition hostPartition = partition as IHostPartition;
                if (hostPartition == null)
                {
                    return false;
                }

                HostPartitionType hostPartitionType_Temp = hostPartition?.Type();
                if (hostPartitionType_Temp == null)
                {
                    return false;
                }

                return hostPartitionType.GetType().IsAssignableFrom(hostPartitionType_Temp.GetType());
            });

            functions_Temp.Insert(0, function_HostPartitionType);

            List<IPartition> partitions_Filtered = architecturalModel.GetObjects(functions_Temp.ToArray());
            if (partitions_Filtered == null || partitions_Filtered.Count == 0)
            {
                return null;
            }

            HashSet<Guid> guids = null;
            if (partitions != null)
            {
                foreach (IPartition partition in partitions)
                {
                    if (partition != null)
                    {
                        guids.Add(partition.Guid);
                    }
                }
            }

            List<IPartition> result = new List<IPartition>();
            foreach (IPartition partition in partitions_Filtered)
            {
                if (guids != null && !guids.Contains(partition.Guid))
                {
                    continue;
                }

                if (partition is AirPartition)
                {
                    List<IJSAMObject> relatedObjects = architecturalModel.GetRelatedObjects<IJSAMObject>(partition);
                    architecturalModel.RemoveObject(partition);

                    IHostPartition hostPartition = Create.HostPartition(partition.Guid, partition.Face3D, hostPartitionType);
                    architecturalModel.Add(hostPartition);
                    result.Add(hostPartition);

                    if (relatedObjects != null)
                    {
                        foreach (IJSAMObject relatedObject in relatedObjects)
                        {
                            architecturalModel.AddRelation(hostPartition, relatedObject);
                        }
                    }
                }
                else if (partition is IHostPartition)
                {
                    if (!Type((IHostPartition)partition, hostPartitionType))
                    {
                        continue;
                    }

                    architecturalModel.Add(partition);
                    result.Add(partition);
                }
            }

            if(materialLibrary != null && result != null && result.Count != 0)
            {
                architecturalModel.UpdateMaterials(hostPartitionType.MaterialLayers, materialLibrary, out HashSet<string> missingMaterialsNames);
            }

            return result;
        }

        public static List<IPartition> UpdateHostPartitionType(this ArchitecturalModel architecturalModel, HostPartitionType hostPartitionType, Func<IPartition, bool> function, IEnumerable<IPartition> partitions = null, MaterialLibrary materialLibrary = null)
        {
            if(function == null)
            {
                return null;
            }

            return UpdateHostPartitionType(architecturalModel, hostPartitionType, new Func<IPartition, bool>[] { function }, partitions, materialLibrary);
        }

        public static List<IPartition> UpdateHostPartitionType(this ArchitecturalModel architecturalModel,
            IEnumerable<IPartition> partitions = null,
            WallType curtainWallType = null,
            WallType internalWallType = null,
            WallType externalWallType = null,
            WallType undergroundWallType = null,
            FloorType internalFloorType = null,
            FloorType externalFloorType = null,
            FloorType onGradeFloorType = null,
            FloorType undergroundFloorType = null,
            FloorType undergroundCeilingFloorType = null,
            RoofType roofType = null,
            MaterialLibrary materialLibrary = null,
            double tolerance_Angle = Tolerance.Angle,
            double tolerance_Distance = Tolerance.Distance)
        {
            if(architecturalModel == null)
            {
                return null;
            }

            Terrain terrain = architecturalModel.Terrain;

            Dictionary<Guid, IPartition> dictionary = new Dictionary<Guid, IPartition>();

            if (curtainWallType != null)
            {
                Func<IPartition, bool> function = new Func<IPartition, bool>((IPartition partition) =>
                {
                    if(partition is AirPartition)
                    {
                        return false;
                    }
                    else if(partition is IHostPartition)
                    {
                        IHostPartition hostPartition = partition as IHostPartition;
                        if (hostPartition == null)
                        {
                            return false;
                        }

                        return architecturalModel.GetMaterialType(hostPartition) == MaterialType.Transparent;
                    }

                    return false;
                });

                List<IPartition> partitions_Update = architecturalModel.UpdateHostPartitionType(curtainWallType, function, partitions, materialLibrary);
                if(partitions_Update != null)
                {
                    foreach(IPartition partition_Update in partitions_Update)
                    {
                        dictionary[partition_Update.Guid] = partition_Update;
                    }
                }
            }

            if(internalWallType != null)
            {
                Func<IPartition, bool> function = new Func<IPartition, bool>((IPartition partition) =>
                {
                    if(partition == null)
                    {
                        return false;
                    }

                    if (partition is AirPartition)
                    {
                        if (Query.HostPartitionCategory(partition, tolerance_Angle) != HostPartitionCategory.Wall)
                        {
                            return false;
                        }
                    }

                    if (!architecturalModel.Internal(partition))
                    {
                        return false;
                    }

                    if (terrain.On(partition, tolerance_Distance))
                    {
                        return false;
                    }

                    return true;
                });

                List<IPartition> partitions_Update = architecturalModel.UpdateHostPartitionType(internalWallType, function, partitions, materialLibrary);
                if (partitions_Update != null)
                {
                    foreach (IPartition partition_Update in partitions_Update)
                    {
                        dictionary[partition_Update.Guid] = partition_Update;
                    }
                }
            }

            if (externalWallType != null)
            {
                Func<IPartition, bool> function = new Func<IPartition, bool>((IPartition partition) =>
                {
                    if(partition == null)
                    {
                        return false;
                    }

                    if(partition is AirPartition)
                    {
                        if (Query.HostPartitionCategory(partition, tolerance_Angle) != HostPartitionCategory.Wall)
                        {
                            return false;
                        }
                    }

                    if(!architecturalModel.External(partition))
                    {
                        return false;
                    }

                    if (partition is IHostPartition)
                    {
                        IHostPartition hostPartition = partition as IHostPartition;

                        if (architecturalModel.GetMaterialType(hostPartition) == MaterialType.Transparent)
                        {
                            return false;
                        }
                    }

                    if (terrain.Below(partition, tolerance_Distance))
                    {
                        return false;
                    }

                    return true;
                });

                List<IPartition> partitions_Update = architecturalModel.UpdateHostPartitionType(externalWallType, function, partitions, materialLibrary);
                if (partitions_Update != null)
                {
                    foreach (IPartition partition_Update in partitions_Update)
                    {
                        dictionary[partition_Update.Guid] = partition_Update;
                    }
                }
            }

            if (undergroundWallType != null)
            {
                Func<IPartition, bool> function = new Func<IPartition, bool>((IPartition partition) =>
                {
                    if(partition == null)
                    {
                        return false;
                    }

                    if (partition is AirPartition)
                    {
                        if (Query.HostPartitionCategory(partition, tolerance_Angle) != HostPartitionCategory.Wall)
                        {
                            return false;
                        }
                    }

                    if (!architecturalModel.External(partition))
                    {
                        return false;
                    }

                    if(partition is IHostPartition)
                    {
                        IHostPartition hostPartition = partition as IHostPartition;
                        if (architecturalModel.GetMaterialType(hostPartition) == MaterialType.Transparent)
                        {
                            return false;
                        }
                    }

                    if (!terrain.Below(partition, tolerance_Distance))
                    {
                        return false;
                    }

                    return true;
                });

                List<IPartition> partitions_Update = architecturalModel.UpdateHostPartitionType(undergroundWallType, function, partitions, materialLibrary);
                if (partitions_Update != null)
                {
                    foreach (IPartition partition_Update in partitions_Update)
                    {
                        dictionary[partition_Update.Guid] = partition_Update;
                    }
                }
            }

            if (internalFloorType != null)
            {
                Func<IPartition, bool> function = new Func<IPartition, bool>((IPartition partition) =>
                {
                    if(partition == null)
                    {
                        return false;
                    }

                    if (partition is AirPartition)
                    {
                        HostPartitionCategory hostPartitionCategory = Query.HostPartitionCategory(partition, tolerance_Angle);
                        if (hostPartitionCategory != HostPartitionCategory.Floor && hostPartitionCategory != HostPartitionCategory.Roof)
                        {
                            return false;
                        }
                    }

                    if (architecturalModel.External(partition))
                    {
                        return false;
                    }

                    return true;
                });

                List<IPartition> partitions_Update = architecturalModel.UpdateHostPartitionType(internalFloorType, function, partitions, materialLibrary);
                if (partitions_Update != null)
                {
                    foreach (IPartition partition_Update in partitions_Update)
                    {
                        dictionary[partition_Update.Guid] = partition_Update;
                    }
                }
            }

            if (externalFloorType != null)
            {
                Func<IPartition, bool> function = new Func<IPartition, bool>((IPartition partition) =>
                {
                    if(partition == null)
                    {
                        return false;
                    }

                    if (partition is AirPartition)
                    {
                        if (Query.HostPartitionCategory(partition, tolerance_Angle) != HostPartitionCategory.Floor)
                        {
                            return false;
                        }
                    }

                    if (!architecturalModel.External(partition))
                    {
                        return false;
                    }

                    if (terrain.Below(partition, tolerance_Distance))
                    {
                        return false;
                    }

                    if (terrain.On(partition, tolerance_Distance))
                    {
                        return false;
                    }

                    return true;
                });

                List<IPartition> partitions_Update = architecturalModel.UpdateHostPartitionType(externalFloorType, function, partitions, materialLibrary);
                if (partitions_Update != null)
                {
                    foreach (IPartition partition_Update in partitions_Update)
                    {
                        dictionary[partition_Update.Guid] = partition_Update;
                    }
                }
            }

            if (onGradeFloorType != null)
            {
                Func<IPartition, bool> function = new Func<IPartition, bool>((IPartition partition) =>
                {
                    if (partition == null)
                    {
                        return false;
                    }

                    if (partition is AirPartition)
                    {
                        if (Query.HostPartitionCategory(partition, tolerance_Angle) != HostPartitionCategory.Floor)
                        {
                            return false;
                        }
                    }

                    if (!architecturalModel.External(partition))
                    {
                        return false;
                    }

                    if (!terrain.On(partition, tolerance_Distance))
                    {
                        return false;
                    }

                    if (Query.HostPartitionCategory(partition, tolerance_Angle) != HostPartitionCategory.Floor)
                    {
                        return false;
                    }

                    return true;
                });

                List<IPartition> partitions_Update = architecturalModel.UpdateHostPartitionType(onGradeFloorType, function, partitions, materialLibrary);
                if (partitions_Update != null)
                {
                    foreach (IPartition partition_Update in partitions_Update)
                    {
                        dictionary[partition_Update.Guid] = partition_Update;
                    }
                }
            }

            if (undergroundFloorType != null)
            {
                Func<IPartition, bool> function = new Func<IPartition, bool>((IPartition partition) =>
                {
                    if (partition == null)
                    {
                        return false;
                    }

                    if (partition is AirPartition)
                    {
                        if (Query.HostPartitionCategory(partition, tolerance_Angle) != HostPartitionCategory.Floor)
                        {
                            return false;
                        }
                    }

                    if (!architecturalModel.External(partition))
                    {
                        return false;
                    }

                    if (architecturalModel.Terrain.On(partition, tolerance_Distance))
                    {
                        return false;
                    }

                    if (!terrain.Below(partition, tolerance_Distance))
                    {
                        return false;
                    }

                    if (Query.HostPartitionCategory(partition, tolerance_Angle) != HostPartitionCategory.Floor)
                    {
                        return false;
                    }

                    return true;
                });

                List<IPartition> partitions_Update = architecturalModel.UpdateHostPartitionType(undergroundFloorType, function, partitions, materialLibrary);
                if (partitions_Update != null)
                {
                    foreach (IPartition partition_Update in partitions_Update)
                    {
                        dictionary[partition_Update.Guid] = partition_Update;
                    }
                }
            }

            if (undergroundCeilingFloorType != null)
            {
                Func<IPartition, bool> function = new Func<IPartition, bool>((IPartition partition) =>
                {
                    if (partition == null)
                    {
                        return false;
                    }

                    if (Query.HostPartitionCategory(partition, tolerance_Angle) != HostPartitionCategory.Roof)
                    {
                        return false;
                    }

                    if (!architecturalModel.External(partition))
                    {
                        return false;
                    }

                    if (!architecturalModel.Terrain.On(partition, tolerance_Distance))
                    {
                        return false;
                    }

                    return true;
                });

                List<IPartition> partitions_Update = architecturalModel.UpdateHostPartitionType(undergroundCeilingFloorType, function , partitions, materialLibrary);
                if (partitions_Update != null)
                {
                    foreach (IPartition partition_Update in partitions_Update)
                    {
                        dictionary[partition_Update.Guid] = partition_Update;
                    }
                }
            }

            if(roofType != null)
            {
                Func<IPartition, bool> function = new Func<IPartition, bool>((IPartition partition) =>
                {
                    if (partition == null)
                    {
                        return false;
                    }

                    if (Query.HostPartitionCategory(partition, tolerance_Angle) != HostPartitionCategory.Roof)
                    {
                        return false;
                    }

                    if (architecturalModel.Internal(partition))
                    {
                        return false;
                    }

                    if (terrain.On(partition, tolerance_Distance))
                    {
                        return false;
                    }

                    if (terrain.Below(partition, tolerance_Distance))
                    {
                        return false;
                    }

                    return true;
                });

                List<IPartition> partitions_Update = architecturalModel.UpdateHostPartitionType(roofType, function , partitions, materialLibrary);
                if (partitions_Update != null)
                {
                    foreach (IPartition partition_Update in partitions_Update)
                    {
                        dictionary[partition_Update.Guid] = partition_Update;
                    }
                }
            }

            return dictionary?.Values?.ToList();
        }
    }
}