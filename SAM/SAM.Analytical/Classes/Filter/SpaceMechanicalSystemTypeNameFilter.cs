using Newtonsoft.Json.Linq;
using SAM.Core;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical
{
    public abstract class SpaceMechanicalSystemTypeNameFilter<T> : TextFilter, IAdjacencyClusterFilter where T : MechanicalSystem
    {       
        public AdjacencyCluster AdjacencyCluster { get; set; }

        public SpaceMechanicalSystemTypeNameFilter(TextComparisonType textComparisonType, string value)
            : base(textComparisonType, value)
        {

        }

        public SpaceMechanicalSystemTypeNameFilter(SpaceMechanicalSystemTypeNameFilter<T> spaceMechanicalSystemTypeNameFilter)
            : base(spaceMechanicalSystemTypeNameFilter)
        {

        }

        public SpaceMechanicalSystemTypeNameFilter(JObject jObject)
            : base(jObject)
        {

        }

        public override bool TryGetText(IJSAMObject jSAMObject, out string text)
        {
            text = null;

            Space space = jSAMObject as Space;
            if (space == null)
            {
                return false;
            }

            AdjacencyCluster adjacencyCluster = AdjacencyCluster;
            if (adjacencyCluster == null)
            {
                return false;
            }

            List<T> mechanicalSystems = adjacencyCluster.GetRelatedObjects<T>(space);
            if(mechanicalSystems == null || mechanicalSystems.Count == 0)
            {
                return false;
            }

            text = mechanicalSystems?.First()?.Type.Name;
            return true;
        }
    }
}