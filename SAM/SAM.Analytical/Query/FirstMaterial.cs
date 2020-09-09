using SAM.Core;
using SAM.Geometry.Spatial;

namespace SAM.Analytical
{
    public static partial class Query
    {
        public static IMaterial FirstMaterial(this Panel panel, Vector3D direction, MaterialLibrary materialLibrary)
        {
            if (panel == null || direction == null || materialLibrary == null)
                return null;

            ConstructionLayer constructionLayer = FirstConstructionLayer(panel, direction);
            if (constructionLayer == null)
                return null;

            return Material(constructionLayer, materialLibrary);
        }
    }
}