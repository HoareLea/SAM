using SAM.Geometry.Spatial;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical
{
    public static partial class Query
    {
        public static Dictionary<IPartition, Vector3D> NormalDictionary(this ArchitecturalModel architecturalModel, Space space, out Shell shell, bool external = true, double silverSpacing = Core.Tolerance.MacroDistance, double tolerance = Core.Tolerance.Distance)
        {
            shell = null;

            if (architecturalModel == null || space == null)
                return null;

            List<IPartition> partitions = architecturalModel.GetPartitions(space);
            if (partitions == null)
                return null;

            List<Face3D> face3Ds = partitions.ConvertAll(x => x.Face3D);

            shell = new Shell(face3Ds);

            Dictionary<IPartition, Vector3D> result = new Dictionary<IPartition, Vector3D>();
            for (int i = 0; i < face3Ds.Count(); i++)
                result[partitions[i]] = shell.Normal(face3Ds[i].InternalPoint3D(), external, silverSpacing, tolerance);

            return result;

        }
    }
}