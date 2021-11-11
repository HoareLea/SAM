using SAM.Geometry.Spatial;
using System.Collections.Generic;

namespace SAM.Analytical
{
    public static partial class Query
    {
        public static IPartition FlipNormal(this IPartition partition, bool includeOpenings, bool flipX = true, double tolerance = Core.Tolerance.Distance)
        {
            Face3D face3D = partition?.Face3D;
            if(face3D == null)
            {
                return null;
            }

            face3D.FlipNormal(flipX);

            if(partition is AirPartition)
            {
                return new AirPartition(partition.Guid, face3D);
            }

            IHostPartition hostPartition = partition as IHostPartition;
            if(hostPartition == null)
            {
                throw new System.NotImplementedException();
            }

            hostPartition = Create.Partition(hostPartition, partition.Guid, face3D, tolerance);
            if(includeOpenings)
            {
                List<IOpening> openings = hostPartition.GetOpenings();
                if (openings != null)
                {
                    for (int i = 0; i < openings.Count; i++)
                    {
                        if (includeOpenings)
                        {
                            openings[i] = openings[i].FlipNormal(flipX);
                        }

                        hostPartition.AddOpening(openings[i]);
                    }
                }
            }

            return hostPartition;
        }

        public static IOpening FlipNormal(this IOpening opening, bool flipX = true)
        {
            Face3D face3D = opening?.Face3D;
            if (face3D == null)
            {
                return null;
            }

            face3D.FlipNormal(flipX);

            return Create.Opening((opening as dynamic).SAMType as OpeningType, face3D);
        }
    }
}