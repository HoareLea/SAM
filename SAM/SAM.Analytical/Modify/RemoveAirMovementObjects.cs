using SAM.Core;
using System;
using System.Collections.Generic;

namespace SAM.Analytical
{
    public static partial class Modify
    {
        public static List<Guid> RemoveAirMovementObjects<T>(this AdjacencyCluster adjacencyCluster) where T : IAirMovementObject 
        {
            if(adjacencyCluster == null)
            {
                return null;
            }

            List<T> airMovemenetObjects = adjacencyCluster.GetObjects<T>();
            if(airMovemenetObjects == null)
            {
                return null;
            }

            List<SAMObject> sAMObjects = new List<SAMObject>(); 
            foreach(T airMovemenetObject in airMovemenetObjects)
            {
                SAMObject sAMObject = airMovemenetObject as SAMObject;
                if(airMovemenetObject == null)
                {
                    continue;
                }

                sAMObjects.Add(sAMObject);
            }

            return adjacencyCluster.Remove(sAMObjects);
        }
    }
}