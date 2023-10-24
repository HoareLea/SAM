using System.Collections.Generic;

namespace SAM.Analytical
{
    public static partial class Create
    {
        public static ConstructionManager ConstructionManager(this AnalyticalModel analyticalModel)
        {
            AdjacencyCluster adjacencyCluster = analyticalModel?.AdjacencyCluster;
            if(adjacencyCluster == null)
            {
                return null;
            }

            List<Construction> constructions = adjacencyCluster.GetConstructions();

            List<ApertureConstruction> apertureConstructions = adjacencyCluster.GetApertureConstructions();

            Core.MaterialLibrary materialLibrary = analyticalModel.MaterialLibrary;

            return new ConstructionManager(apertureConstructions, constructions, materialLibrary);
        }
    }
}