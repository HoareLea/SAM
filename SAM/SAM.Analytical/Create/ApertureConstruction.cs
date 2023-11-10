using System;
using System.Collections.Generic;

namespace SAM.Analytical
{
    public static partial class Create
    {
        public static ApertureConstruction ApertureConstruction(Guid guid, string name, ApertureType apertureType, IEnumerable<ConstructionLayer> paneConstructionLayers, IEnumerable<ConstructionLayer> frameConstructionLayers, PanelType panelType)
        {
            ApertureConstruction apertureConstruction = new ApertureConstruction(guid, name, apertureType, paneConstructionLayers, frameConstructionLayers);
            apertureConstruction.SetValue(ApertureConstructionParameter.DefaultPanelType, panelType.Text());

            return apertureConstruction;
        }

        public static ApertureConstruction ApertureConstruction(ApertureType apertureType, string name, Construction paneConstruction, Construction frameConstruction = null)
        {
            if(apertureType == ApertureType.Undefined || (paneConstruction == null && frameConstruction == null))
            {
                return null;
            }

            ApertureConstruction result = new ApertureConstruction(Guid.NewGuid(), name, apertureType, paneConstruction?.ConstructionLayers, frameConstruction?.ConstructionLayers);

            return result;
        }
    }
}