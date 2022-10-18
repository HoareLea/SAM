using SAM.Core;
using System.Collections.Generic;

namespace SAM.Analytical
{
    public static partial class Query
    {
        public static bool Transparent(this Panel panel, MaterialLibrary materialLibrary = null)
        {
            Construction construction_Panel = panel.Construction;
            MaterialType materialType = MaterialType(construction_Panel?.ConstructionLayers, materialLibrary);
            if (materialType == Core.MaterialType.Undefined && panel.PanelType == Analytical.PanelType.CurtainWall)
                materialType = Core.MaterialType.Transparent;

            return materialType == Core.MaterialType.Transparent;
        }

        public static bool Transparent(this Construction construction, MaterialLibrary materialLibrary)
        {
            MaterialType materialType = MaterialType(construction.ConstructionLayers, materialLibrary);
            return materialType == Core.MaterialType.Transparent;
        }

        public static bool Transparent(this ApertureConstruction apertureConstruction, MaterialLibrary materialLibrary)
        {
            if (apertureConstruction == null)
            {
                return false;
            }

            List<ConstructionLayer> constructionLayers = apertureConstruction.PaneConstructionLayers;
            if(constructionLayers == null || constructionLayers.Count == 0)
            {
                constructionLayers = apertureConstruction.FrameConstructionLayers;
            }
            
            MaterialType materialType = MaterialType(constructionLayers, materialLibrary);
            return materialType == Core.MaterialType.Transparent;
        }

        public static bool Transparent(this Aperture aperture, MaterialLibrary materialLibrary)
        {
            return Transparent(aperture?.ApertureConstruction, materialLibrary);
        }

        public static bool Transparent(this ApertureConstruction apertureConstruction, MaterialLibrary materialLibrary, AperturePart aperturePart)
        {
            if (apertureConstruction == null)
            {
                return false;
            }

            List<ConstructionLayer> constructionLayers = apertureConstruction.GetConstructionLayers(aperturePart);
            if (constructionLayers == null || constructionLayers.Count == 0)
            {
                constructionLayers = apertureConstruction.FrameConstructionLayers;
            }

            MaterialType materialType = MaterialType(constructionLayers, materialLibrary);
            return materialType == Core.MaterialType.Transparent;
        }

        public static bool Transparent(this Aperture aperture, MaterialLibrary materialLibrary, AperturePart aperturePart)
        {
            return Transparent(aperture?.ApertureConstruction, materialLibrary, aperturePart);
        }
    }
}