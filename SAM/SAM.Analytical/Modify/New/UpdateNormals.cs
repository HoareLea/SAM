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
        public static void UpdateNormals(this ArchitecturalModel architecturalModel, bool includeOpenings, double silverSpacing = Core.Tolerance.MacroDistance, double tolerance = Core.Tolerance.Distance)
        {
            if (architecturalModel == null)
                return;

            List<Space> spaces = architecturalModel.GetSpaces();
            if (spaces == null || spaces.Count == 0)
                return;

            HashSet<System.Guid> guids = new HashSet<System.Guid>();
            foreach (Space space in spaces)
            {
                Shell shell = null;
                Dictionary<IPartition, Vector3D> dictionary = architecturalModel.NormalDictionary(space, out shell, true, silverSpacing, tolerance);
                if (dictionary == null)
                    continue;

                foreach (KeyValuePair<IPartition, Vector3D> keyValuePair in dictionary)
                {
                    IPartition partition = keyValuePair.Key;
                    if (partition == null)
                        continue;

                    if (guids.Contains(partition.Guid))
                        continue;

                    guids.Add(partition.Guid);

                    Vector3D normal_External = keyValuePair.Value;
                    if (normal_External == null)
                        continue;

                    Vector3D normal_Partition = partition.Face3D?.GetPlane()?.Normal;
                    if (normal_Partition == null)
                        continue;

                    if (normal_External.SameHalf(normal_Partition))
                        continue;

                    partition = partition.FlipNormal(includeOpenings, false);
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
        public static void UpdateNormals(this ArchitecturalModel architecturalModel, Space space, bool includeOpenings, double silverSpacing = Core.Tolerance.MacroDistance, double tolerance = Core.Tolerance.Distance)
        {
            if (architecturalModel == null || space == null)
                return;

            Shell shell = null;

            Dictionary<IPartition, Vector3D> dictionary = architecturalModel.NormalDictionary(space, out shell, true, silverSpacing, tolerance);
            if (dictionary == null)
                return;

            foreach (KeyValuePair<IPartition, Vector3D> keyValuePair in dictionary)
            {
                IPartition partition = keyValuePair.Key;
                if (partition != null)
                {
                    Vector3D normal_External = keyValuePair.Value;
                    if (normal_External != null)
                    {
                        Vector3D normal_Partition = partition.Face3D?.GetPlane()?.Normal;
                        if (normal_Partition != null && !normal_External.SameHalf(normal_Partition))
                        {
                            partition = partition.FlipNormal(includeOpenings, false);
                            architecturalModel.Add(partition);
                        }
                    }
                }
            }
        }

    }
}