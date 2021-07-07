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
    }
}