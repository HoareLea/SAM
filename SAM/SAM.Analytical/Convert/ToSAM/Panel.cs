using System.Collections.Generic;

namespace SAM.Analytical
{
    public static partial class Convert
    {
        public static Panel ToSAM(this IPartition partition)
        {
            if (partition == null)
            {
                return null;
            }

            List<Aperture> apertures = null;

            Construction construction = null;
            if (partition is IHostPartition)
            {
                IHostPartition hostPartition = ((IHostPartition)partition);

                HostPartitionType hostPartitionType = hostPartition.Type();
                if (hostPartitionType != null)
                {
                    construction = hostPartitionType.ToSAM();
                }

                apertures = hostPartition.GetOpenings()?.ConvertAll(x => x.ToSAM());
            }

            PanelType panelType = PanelType.Undefined;
            if (partition is Wall)
            {
                panelType = PanelType.Wall;
            }
            else if (partition is AirPartition)
            {
                panelType = PanelType.Air;
            }
            else if (partition is Floor)
            {
                panelType = PanelType.Floor;
            }
            else if (partition is Roof)
            {
                panelType = PanelType.Roof;
            }

            Panel result = Create.Panel(construction, panelType, partition.Face3D);
            apertures?.ForEach(x => result.AddAperture(x));
            Core.Modify.CopyParameterSets(partition as Core.SAMObject, result);
            return result;
        }
    }
}