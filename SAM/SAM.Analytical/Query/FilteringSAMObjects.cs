using SAM.Core;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical
{
    public static partial class Query
    {
        public static List<IJSAMObject> FilteringSAMObjects(this AdjacencyCluster adjacencyCluster)
        {
            if(adjacencyCluster == null)
            {
                return null;
            }

            List<IJSAMObject> result = new List<IJSAMObject>();

            List<Panel> panels = adjacencyCluster.GetObjects<Panel>();
            if(panels != null)
            {
                result.AddRange(panels);
            }

            List<Aperture> apertures = adjacencyCluster.GetApertures();
            if (panels != null)
            {
                result.AddRange(apertures);
            }

            List<Aperture> apertures_Temp = adjacencyCluster.GetObjects<Aperture>();
            if (apertures_Temp != null)
            {
                foreach(Aperture aperture in apertures_Temp)
                {
                    if(aperture == null)
                    {
                        continue;
                    }

                    if(apertures.Find(x => x.Guid == aperture.Guid) != null)
                    {
                        continue;
                    }

                    result.Add(aperture);
                }
            }

            List<InternalCondition> internalConditions = adjacencyCluster.GetInternalConditions(true, true)?.ToList();
            if (panels != null)
            {
                result.AddRange(internalConditions);
            }

            List<Space> spaces = adjacencyCluster.GetSpaces();
            if (spaces != null)
            {
                result.AddRange(spaces);
            }

            return result;

        }
    }
}