using SAM.Geometry.Spatial;

namespace SAM.Analytical
{
    public static partial class Query
    {
        public static ConstructionLayer LastConstructionLayer(this Panel panel, Vector3D direction)
        {
            if (panel == null || direction == null)
                return null;

            Vector3D normal = panel.Normal;
            if (normal == null)
                return null;

            Construction construction = panel.Construction;
            if (construction == null)
                return null;

            if (direction.SameHalf(normal))
                return ExternalConstructionLayer(construction);

            return InternalConstructionLayer(construction);
        }
    }
}