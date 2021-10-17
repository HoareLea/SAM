using SAM.Geometry.Spatial;
using System.Collections.Generic;

namespace SAM.Analytical
{
    public static partial class Query
    {
        public static IPartition FlipNormal(this IPartition partition, bool includeOpenings, bool flipX = true)
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

            List<IOpening> openings = hostPartition.Openings;

            hostPartition = Create.HostPartition(partition.Guid, face3D, hostPartition.Type());
            if(openings != null)
            {
                for(int i=0; i < openings.Count; i++)
                {
                    if(includeOpenings)
                    {
                        openings[i] = openings[i].FlipNormal(flipX);
                    }

                    hostPartition.AddOpening(openings[i]);
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