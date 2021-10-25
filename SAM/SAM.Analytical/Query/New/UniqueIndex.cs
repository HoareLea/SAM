using System.Collections.Generic;

namespace SAM.Analytical
{
    public static partial class Query
    {
        public static int UniqueIndex(this ArchitecturalModel architecturalModel, IPartition partition)
        {
            if (partition == null)
            {
                return -1;
            }

            List<IPartition> partitions = architecturalModel?.GetPartitions();
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

        public static int UniqueIndex(this ArchitecturalModel architecturalModel, IOpening opening)
        {
            if (opening == null)
            {
                return -1;
            }

            List<IOpening> openings = architecturalModel?.GetOpenings();
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

        public static int UniqueIndex(this ArchitecturalModel architecturalModel, Space space)
        {
            if (architecturalModel == null || space == null)
            {
                return -1;
            }

            List<Space> spaces = architecturalModel.GetSpaces();
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