using SAM.Geometry.Spatial;
using System.Collections.Generic;

namespace SAM.Analytical
{
    public static partial class Query
    {
        /// <summary>
        /// Update Partitions normals in the given room to point out outside direction
        /// </summary>
        /// <param name="buildingModel">SAM Architectural Model</param>
        /// <param name="space">Space</param>
        /// <param name="includeOpenings">Update Normals of Openings<</param>
        /// <param name="silverSpacing">Sliver Spacing Tolerance</param>
        /// <param name="flippedPartitions">Partitions have been flipped</param>
        /// <param name="tolerance">Distance Tolerance</param>
        public static List<IPartition> OrientedPartitions(this BuildingModel buildingModel, Space space, bool includeOpenings, out List<IPartition> flippedPartitions, double silverSpacing = Core.Tolerance.MacroDistance, double tolerance = Core.Tolerance.Distance)
        {
            flippedPartitions = null;
            
            if (buildingModel == null || space == null)
            {
                return null;
            }

            Dictionary<IPartition, Vector3D> dictionary = buildingModel.NormalDictionary(space, out Shell shell, true, silverSpacing, tolerance);
            if (dictionary == null)
            {
                return null;
            }

            flippedPartitions = new List<IPartition>();

            List<IPartition> result = new List<IPartition>();
            foreach (KeyValuePair<IPartition, Vector3D> keyValuePair in dictionary)
            {
                IPartition partition = keyValuePair.Key;
                if (partition == null)
                {
                    continue;
                }

                Vector3D normal_External = keyValuePair.Value;
                if (normal_External != null)
                {
                    Vector3D normal_Partition = partition.Face3D?.GetPlane()?.Normal;
                    if (normal_Partition != null && !normal_External.SameHalf(normal_Partition))
                    {
                        partition = partition.FlipNormal(includeOpenings, false);
                        flippedPartitions.Add(partition);
                    }
                }

                result.Add(partition);
            }

            return result;
        }

        /// <summary>
        /// Update Partitions normals in the given room to point out outside direction
        /// </summary>
        /// <param name="buildingModel">SAM Architectural Model</param>
        /// <param name="space">Space</param>
        /// <param name="includeOpenings">Update Normals of Openings<</param>
        /// <param name="silverSpacing">Sliver Spacing Tolerance</param>
        /// <param name="tolerance">Distance Tolerance</param>
        public static List<IPartition> OrientedPartitions(this BuildingModel buildingModel, Space space, bool includeOpenings, double silverSpacing = Core.Tolerance.MacroDistance, double tolerance = Core.Tolerance.Distance)
        {
            return OrientedPartitions(buildingModel, space, includeOpenings, out List<IPartition> flippedPartitions, silverSpacing, tolerance);
        }

    }
}