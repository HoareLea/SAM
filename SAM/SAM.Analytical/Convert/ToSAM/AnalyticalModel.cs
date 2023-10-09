using System.Collections.Generic;

namespace SAM.Analytical
{
    public static partial class Convert
    {  
        public static AnalyticalModel ToSAM_AnalyticalModel(this IEnumerable<IAnalyticalObject> analyticalObjects)
        {
            if(analyticalObjects == null)
            {
                return null;
            }

            AnalyticalModel result = new AnalyticalModel(System.Guid.NewGuid(), null);

            foreach(IAnalyticalObject analyticalObject in analyticalObjects)
            {
                if(analyticalObject is AnalyticalModel)
                {
                    AnalyticalModel analyticalModel = (AnalyticalModel)analyticalObject;

                    result = new AnalyticalModel(analyticalModel);
                }

                if(analyticalObject is Panel)
                {
                    result.AddPanel(new Panel((Panel)analyticalObject));
                }

                if(analyticalObject is AdjacencyCluster)
                {
                    result = new AnalyticalModel(result, new AdjacencyCluster((AdjacencyCluster)analyticalObject));
                }
            }

            return result;
        }
    }
}