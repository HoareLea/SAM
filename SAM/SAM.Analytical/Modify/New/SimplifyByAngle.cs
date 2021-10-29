using SAM.Geometry.Spatial;
using System.Collections.Generic;

namespace SAM.Analytical
{
    public static partial class Modify
    {
        public static void SimplifyByAngle(this ArchitecturalModel architecturalModel, double tolerance_Angle = Core.Tolerance.Angle, double tolerance_Distance = Core.Tolerance.Distance)
        {
            List<IPartition> partitions = architecturalModel?.GetPartitions();
            if(partitions == null)
            {
                return;
            }


            foreach(IPartition partition in partitions)
            {
                Face3D face3D = partition?.Face3D;
                if(face3D == null)
                {
                    continue;
                }

                face3D = face3D.SimplifyByAngle(tolerance_Angle, tolerance_Distance);
                IPartition partition_New = Create.Partition(partition, partition.Guid, face3D, tolerance_Distance);
                if(partition_New is IHostPartition)
                {
                    IHostPartition hostPartition = (IHostPartition)partition_New;

                    List<IOpening> openings = hostPartition.GetOpenings();
                    if (openings != null)
                    {
                        foreach (IOpening opening in openings)
                        {
                            Face3D face3D_Opening = opening?.Face3D;
                            if (face3D_Opening == null)
                            {
                                continue;
                            }

                            face3D_Opening = face3D_Opening.SimplifyByAngle(tolerance_Angle, tolerance_Distance);
                            IOpening opening_New = Create.Opening(opening.Guid, opening, face3D_Opening);
                            if (opening_New != null)
                            {
                                hostPartition.AddOpening(opening_New, tolerance_Distance);
                            }
                        }
                    }
                }


                architecturalModel.Add(partition_New);
            }
        }
    }
}