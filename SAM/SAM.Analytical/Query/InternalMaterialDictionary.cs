using SAM.Core;
using System.Collections.Generic;

namespace SAM.Analytical
{
    public static partial class Query
    {
        public static Dictionary<Panel, IMaterial> InternalMaterialDictionary(this Space space, AdjacencyCluster adjacencyCluster, MaterialLibrary materialLibrary, double silverSpacing = Tolerance.MacroDistance, double tolerance = Tolerance.Distance)
        {
            if (materialLibrary == null)
                return null;

            Dictionary<Panel, ConstructionLayer> dictionary = InternalConstructionLayerDictionary(space, adjacencyCluster, silverSpacing, tolerance);
            if (dictionary == null || dictionary.Count == 0)
                return null;

            Dictionary<Panel, IMaterial> result = new Dictionary<Panel, IMaterial>();
            foreach (KeyValuePair<Panel, ConstructionLayer> keyValuePair in dictionary)
                result[keyValuePair.Key] = keyValuePair.Value?.Material(materialLibrary);

            return result;
        }
    }
}