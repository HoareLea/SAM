using System.Collections.Generic;

namespace SAM.Analytical
{
    public static partial class Query
    {
        public static int UniqueIndex(this BuildingModel buildingModel, IPartition partition)
        {
            if (partition == null)
            {
                return -1;
            }

            List<IPartition> partitions = buildingModel?.GetPartitions();
            if (partitions == null || partitions.Count == 0)
            {
                return -1;
            }

            int index = partitions.FindIndex(x => x.Guid == partition.Guid);
            if (index == -1)
            {
                return -1;
            }

            index++;

            return index;
        }

        public static int UniqueIndex(this BuildingModel buildingModel, IOpening opening)
        {
            if (opening == null)
            {
                return -1;
            }

            List<IOpening> openings = buildingModel?.GetOpenings();
            if (openings == null || openings.Count == 0)
            {
                return -1;
            }

            int index = openings.FindIndex(x => x.Guid == opening.Guid);
            if (index == -1)
            {
                return -1;
            }

            index++;

            return index;

        }

        public static int UniqueIndex(this BuildingModel buildingModel, Space space)
        {
            if (buildingModel == null || space == null)
            {
                return -1;
            }

            List<Space> spaces = buildingModel.GetSpaces();
            if (spaces == null || spaces.Count == 0)
            {
                return -1;
            }

            int index = spaces.FindIndex(x => x.Guid == space.Guid);
            if (index == -1)
            {
                return -1;
            }

            index++;

            return index;
        }
    }
}