using SAM.Geometry.Spatial;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical
{
    public static partial class Query
    {
        public static ConstructionLayer FirstConstructionLayer(this Panel panel, Vector3D direction)
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
                return InternalConstructionLayer(construction);

            return ExternalConstructionLayer(construction);
        }
    }
}