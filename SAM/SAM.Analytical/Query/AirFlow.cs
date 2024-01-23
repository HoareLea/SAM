using SAM.Core;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical
{
    public static partial class Query
    {
        public static Profile AirFlow(this AdjacencyCluster adjacencyCluster, AirHandlingUnitAirMovement airHandlingUnitAirMovement)
        {
            AirHandlingUnit airHandlingUnit = adjacencyCluster?.GetRelatedObjects<AirHandlingUnit>(airHandlingUnitAirMovement)?.FirstOrDefault();
            if (airHandlingUnit == null)
            {
                return null;
            }

            List<SpaceAirMovement> spaceAirMovements = adjacencyCluster.GetRelatedObjects<SpaceAirMovement>(airHandlingUnit);
            if (spaceAirMovements == null || spaceAirMovements.Count == 0)
            {
                return null;
            }

            ObjectReference objectReference = new ObjectReference(airHandlingUnit);

            Profile result = null;
            foreach (SpaceAirMovement spaceAirMovement in spaceAirMovements)
            {
                string from = spaceAirMovement?.From;
                if (string.IsNullOrWhiteSpace(from))
                {
                    continue;
                }

                ObjectReference objectReference_From = Core.Convert.ComplexReference<ObjectReference>(from);
                if (objectReference != objectReference_From)
                {
                    continue;
                }

                Profile profile = spaceAirMovement.AirFlow;
                if (result == null)
                {
                    result = profile;
                    continue;
                }

                result = result.Combine(profile);
            }


            return result;

        }
    }
}