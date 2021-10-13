using System.Collections.Generic;

namespace SAM.Architectural
{
    public static partial class Query
    {
        public static PartitionAnalyticalType PartitionAnalyticalType(this ArchitecturalModel architecturalModel, IPartition partition, double tolerance_Angle = Core.Tolerance.Angle, double tolerance_Distance = Core.Tolerance.Distance)
        {
            if(architecturalModel == null || partition == null)
            {
                return Architectural.PartitionAnalyticalType.Undefined;
            }

            if(partition is AirPartition)
            {
                return Architectural.PartitionAnalyticalType.Air;
            }

            List<Room> rooms = architecturalModel.GetRooms(partition);
            if (rooms == null || rooms.Count == 0)
            {
                return Architectural.PartitionAnalyticalType.Shade;
            }

            if (partition is Roof)
            {
                return Architectural.PartitionAnalyticalType.Roof;
            }

            Terrain terrain = architecturalModel.Terrain;

            if (partition is Wall)
            {
                Wall wall = partition as Wall;

                if (architecturalModel.GetMaterialType(wall.Type) == Core.MaterialType.Transparent)
                {
                    return Architectural.PartitionAnalyticalType.CurtainWall;
                }

                if(rooms.Count > 1)
                {
                    return Architectural.PartitionAnalyticalType.InternalWall;
                }

                if (terrain == null)
                {
                    return Architectural.PartitionAnalyticalType.Undefined;
                }

                if(terrain.Below(wall, tolerance_Distance))
                {
                    return Architectural.PartitionAnalyticalType.UndergroundWall;
                }

                return Architectural.PartitionAnalyticalType.ExternalWall;
            }

            if(partition is Floor)
            {
                Floor floor = partition as Floor;

                if (rooms.Count > 1)
                {
                    return Architectural.PartitionAnalyticalType.InternalFloor;
                }

                if (terrain == null)
                {
                    return Architectural.PartitionAnalyticalType.Undefined;
                }

                if (terrain.On(floor, tolerance_Distance))
                {
                    HostPartitionCategory hostPartitionCategory = HostPartitionCategory(floor, tolerance_Angle);
                    if(hostPartitionCategory == Architectural.HostPartitionCategory.Roof)
                    {
                        return Architectural.PartitionAnalyticalType.UndergroundCeiling;
                    }
                    else
                    {
                        return Architectural.PartitionAnalyticalType.OnGradeFloor;
                    }
                }

                if (terrain.Below(floor, tolerance_Distance))
                {
                    return Architectural.PartitionAnalyticalType.UndergroundFloor;
                }
            }

            return Architectural.PartitionAnalyticalType.Undefined;
        }
    }
}