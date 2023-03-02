using SAM.Core;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical
{
    public static partial class Query
    {
        public static List<T> Filter<T>(this AdjacencyCluster adjacencyCluster, IFilter filter) where T: IJSAMObject
        {
            if(adjacencyCluster == null || filter == null)
            {
                return null;
            }

            List<IJSAMObject> jSAMObjects = new List<IJSAMObject>();

            List<Panel> panels = adjacencyCluster.GetObjects<Panel>();
            if(panels != null)
            {
                jSAMObjects.AddRange(panels);
            }

            List<Aperture> apertures = adjacencyCluster.GetObjects<Aperture>();
            if (apertures != null)
            {
                jSAMObjects.AddRange(apertures);
            }

            apertures = adjacencyCluster.GetApertures();
            if (panels != null)
            {
                jSAMObjects.AddRange(apertures);
            }

            List<InternalCondition> internalConditions = adjacencyCluster.GetInternalConditions(true, true)?.ToList();
            if (panels != null)
            {
                jSAMObjects.AddRange(internalConditions);
            }

            List<Space> spaces = adjacencyCluster.GetSpaces();
            if (spaces != null)
            {
                jSAMObjects.AddRange(spaces);
            }

            Modify.AssignAdjacencyCluster(filter, adjacencyCluster);

            return Core.Query.Filter<T>(jSAMObjects, filter);
        }
    }
}