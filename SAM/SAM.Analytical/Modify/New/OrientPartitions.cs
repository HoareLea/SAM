using SAM.Geometry.Spatial;
using System.Collections.Generic;

namespace SAM.Analytical
{
    public static partial class Modify
    {
        /// <summary>
        /// Update Partitions normals to point out outside direction
        /// </summary>
        /// <param name="architecturalModel">SAM Architectural Model</param>
        /// <param name="includeOpenings">Update Normals of Openings</param>
        /// <param name="silverSpacing">Sliver Spacing Tolerance</param>
        /// <param name="tolerance">Distance Tolerance</param>
        public static void OrientPartitions(this ArchitecturalModel architecturalModel, bool includeOpenings, double silverSpacing = Core.Tolerance.MacroDistance, double tolerance = Core.Tolerance.Distance)
        {
            if (architecturalModel == null)
                return;

            List<Space> spaces = architecturalModel.GetSpaces();
            if (spaces == null || spaces.Count == 0)
                return;

            HashSet<System.Guid> guids = new HashSet<System.Guid>();
            foreach (Space space in spaces)
            {
                List<IPartition> partitions = architecturalModel.OrientedPartitions(space, includeOpenings, silverSpacing, tolerance);
                if(partitions == null || partitions.Count == 0)
                {
                    continue;
                }

                foreach(IPartition partition in partitions)
                {
                    if(partition == null || guids.Contains(partition.Guid))
                    {
                        continue;
                    }

                    guids.Add(partition.Guid);

                    architecturalModel.Add(partition);
                }
            }
        }

        /// <summary>
        /// Update Partitions normals in the given room to point out outside direction
        /// </summary>
        /// <param name="architecturalModel">SAM Architectural Model</param>
        /// <param name="space">Space</param>
        /// <param name="includeOpenings">Update Normals of Openings<</param>
        /// <param name="silverSpacing">Sliver Spacing Tolerance</param>
        /// <param name="tolerance">Distance Tolerance</param>
        public static void OrientPartitions(this ArchitecturalModel architecturalModel, Space space, bool includeOpenings, double silverSpacing = Core.Tolerance.MacroDistance, double tolerance = Core.Tolerance.Distance)
        {
            if (architecturalModel == null || space == null)
            {
                return;
            }

            architecturalModel.OrientedPartitions(space, includeOpenings, out List<IPartition> flippedPartitions, silverSpacing, tolerance);

            if(flippedPartitions == null || flippedPartitions.Count == 0)
            {
                return;
            }

            foreach(IPartition partition in flippedPartitions)
            {
                architecturalModel.Add(partition);
            }
        }

    }
}