using SAM.Core;
using System;
using System.Collections.Generic;

namespace SAM.Analytical
{
    public static partial class Create
    {
        public static ApertureConstruction ApertureConstruction(Guid guid, string name, ApertureType apertureType, IEnumerable<ConstructionLayer> paneConstructionLayers, IEnumerable<ConstructionLayer> frameConstructionLayers, PanelType panelType)
        {
            return ApertureConstruction(guid, name, apertureType, paneConstructionLayers, frameConstructionLayers, panelType, ActiveSetting.Setting);
        }

        public static ApertureConstruction ApertureConstruction(Guid guid, string name, ApertureType apertureType, IEnumerable<ConstructionLayer> paneConstructionLayers, IEnumerable<ConstructionLayer> frameConstructionLayers, PanelType panelType, Setting setting)
        {
            ApertureConstruction apertureConstruction = new ApertureConstruction(guid, name, apertureType, paneConstructionLayers, frameConstructionLayers);

            apertureConstruction.SetPanelType(panelType, setting);

            return apertureConstruction;
        }
    }
}