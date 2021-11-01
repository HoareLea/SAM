using System.Collections.Generic;

namespace SAM.Analytical
{
    public static partial class Query
    {
        public static PartitionAnalyticalType PartitionAnalyticalType(this ArchitecturalModel architecturalModel, IPartition partition, double tolerance_Angle = Core.Tolerance.Angle, double tolerance_Distance = Core.Tolerance.Distance)
        {
            if (architecturalModel == null || partition == null)
            {
                return Analytical.PartitionAnalyticalType.Undefined;
            }

            if (partition is AirPartition)
            {
                return Analytical.PartitionAnalyticalType.Air;
            }

            List<Space> spaces = architecturalModel.GetSpaces(partition);
            if (spaces == null || spaces.Count == 0)
            {
                return Analytical.PartitionAnalyticalType.Shade;
            }

            if (partition is Roof)
            {
                return Analytical.PartitionAnalyticalType.Roof;
            }

            Architectural.Terrain terrain = architecturalModel.Terrain;

            if (partition is Wall)
            {
                Wall wall = partition as Wall;

                if (architecturalModel.GetMaterialType(wall.Type) == Core.MaterialType.Transparent)
                {
                    return Analytical.PartitionAnalyticalType.CurtainWall;
                }

                if (spaces.Count > 1)
                {
                    return Analytical.PartitionAnalyticalType.InternalWall;
                }

                if (terrain == null)
                {
                    return Analytical.PartitionAnalyticalType.Undefined;
                }

                if (terrain.Below(wall, tolerance_Distance))
                {
                    return Analytical.PartitionAnalyticalType.UndergroundWall;
                }

                return Analytical.PartitionAnalyticalType.ExternalWall;
            }

            if (partition is Floor)
            {
                Floor floor = partition as Floor;

                if (spaces.Count > 1)
                {
                    return Analytical.PartitionAnalyticalType.InternalFloor;
                }

                if (terrain == null)
                {
                    return Analytical.PartitionAnalyticalType.Undefined;
                }

                if (terrain.On(floor, tolerance_Distance))
                {
                    HostPartitionCategory hostPartitionCategory = HostPartitionCategory(floor, tolerance_Angle);
                    if (hostPartitionCategory == Analytical.HostPartitionCategory.Roof)
                    {
                        return Analytical.PartitionAnalyticalType.UndergroundCeiling;
                    }
                    else
                    {
                        return Analytical.PartitionAnalyticalType.OnGradeFloor;
                    }
                }

                if (terrain.Below(floor, tolerance_Distance))
                {
                    return Analytical.PartitionAnalyticalType.UndergroundFloor;
                }
            }

            return Analytical.PartitionAnalyticalType.Undefined;
        }

        public static PartitionAnalyticalType? PartitionAnalyticalType(this HostPartitionType hostPartitionType)
        {
            if(hostPartitionType == null)
            {
                return null;
            }

            if(!hostPartitionType.TryGetValue(HostPartitionTypeParameter.PartitionAnalyticalType, out string value, true))
            {
                return null;
            }

            return Core.Query.Enum<PartitionAnalyticalType>(value);
        }

        public static PartitionAnalyticalType? PartitionAnalyticalType(this OpeningType openingType)
        {
            if (openingType == null)
            {
                return null;
            }

            if (!openingType.TryGetValue(OpeningTypeParameter.PartitionAnalyticalType, out string value, true))
            {
                return null;
            }

            return Core.Query.Enum<PartitionAnalyticalType>(value);
        }
    }
}