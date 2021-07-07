using SAM.Core;

namespace SAM.Analytical
{
    public static partial class Query
    {
        public static IMaterial ExternalMaterial(this Construction construction, MaterialLibrary materialLibrary)
        {
            if (construction == null || materialLibrary == null)
                return null;

            ConstructionLayer constructionLayer = ExternalConstructionLayer(construction);
            if (constructionLayer == null)
                return null;

            return Material(constructionLayer, materialLibrary);
        }

        public static IMaterial ExternalMaterial(this Panel panel, MaterialLibrary materialLibrary)
        {
            return ExternalMaterial(panel?.Construction, materialLibrary);

        }
    }
}