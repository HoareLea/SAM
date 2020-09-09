using SAM.Core;

namespace SAM.Analytical
{
    public static partial class Query
    {
        public static IMaterial Material(this ConstructionLayer constructionLayer, MaterialLibrary materialLibrary)
        {
            if (constructionLayer == null || materialLibrary == null)
                return null;

            return materialLibrary.GetMaterial(constructionLayer.Name);
        }
    }
}