using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical
{
    public static partial class Query
    {
        public static double MaxThickness(this ApertureConstruction apertureConstruction)
        {
            if(apertureConstruction == null)
            {
                return double.NaN;
            }

            List<ConstructionLayer> constructionLayers_Pane = apertureConstruction.PaneConstructionLayers;
            List<ConstructionLayer> constructionLayers_Frame = apertureConstruction.PaneConstructionLayers;
            if (constructionLayers_Pane == null && constructionLayers_Frame == null)
            {
                return double.NaN;
            }

            double result = 0;
            if(constructionLayers_Pane != null)
            {
                result = constructionLayers_Pane.ConvertAll(x => x.Thickness).Sum();
            }

            if (constructionLayers_Frame != null)
            {
                double thickness = constructionLayers_Frame.ConvertAll(x => x.Thickness).Sum();
                result = System.Math.Max(result, thickness);
            }

            return result;
        }
    }
}