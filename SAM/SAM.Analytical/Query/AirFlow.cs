using SAM.Core;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical
{
    public static partial class Query
    {
        public static double AirFlow(this AdjacencyCluster adjacencyCluster, AirHandlingUnitAirMovement airHandlingUnitAirMovement, out Profile profile)
        {
            profile = null;

            AirHandlingUnit airHandlingUnit = adjacencyCluster?.GetRelatedObjects<AirHandlingUnit>(airHandlingUnitAirMovement)?.FirstOrDefault();
            if (airHandlingUnit == null)
            {
                return double.NaN;
            }

            List<SpaceAirMovement> spaceAirMovements = adjacencyCluster.GetRelatedObjects<SpaceAirMovement>(airHandlingUnit);
            if (spaceAirMovements == null || spaceAirMovements.Count == 0)
            {
                return double.NaN;
            }

            ObjectReference objectReference = new ObjectReference(airHandlingUnit);

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

                Profile profile_SpaceAirMovement = spaceAirMovement.Profile;
                profile_SpaceAirMovement.Multiply(spaceAirMovement.AirFlow);

                if(profile == null)
                {
                    profile = profile_SpaceAirMovement;
                }
                else
                {
                    profile.Sum(profile_SpaceAirMovement);
                }
            }

            if(profile == null)
            {
                return double.NaN;
            }

            double result = profile.MaxValue;
            profile.Divide(result);

            return result;

        }
    }
}