using SAM.Core;
using System;
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

            List<Aperture> apertures = adjacencyCluster.GetObjects<Aperture>();
            if (apertures != null)
            {
                result.AddRange(apertures);
            }

            apertures = adjacencyCluster.GetApertures();
            if (panels != null)
            {
                result.AddRange(apertures);
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