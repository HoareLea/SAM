using SAM.Core;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical
{
    public static partial class Query
    {
        //public static ApertureConstruction DefaultApertureConstruction(this ApertureType apertureType, bool external = true)
        //{
        //    Setting setting = ActiveSetting.Setting;

        //    string parameterName = null;
        //    switch (apertureType)
        //    {
        //        case Analytical.ApertureType.Window:
        //            if (external)
        //                parameterName = ActiveSetting.Name.ApertureConstruction_ExternalWindows;
        //            else
        //                parameterName = ActiveSetting.Name.ApertureConstruction_InternalWindows;
        //            break;

        //        case Analytical.ApertureType.Door:
        //            if (external)
        //                parameterName = ActiveSetting.Name.ApertureConstruction_ExternalDoors;
        //            else
        //                parameterName = ActiveSetting.Name.ApertureConstruction_InternalDoors;
        //            break;
        //    }

        //    ApertureConstruction apertureConstruction;
        //    if (!setting.TryGetValue(parameterName, out apertureConstruction))
        //        return null;

        //    return apertureConstruction;
        //}

        public static ApertureConstruction DefaultApertureConstruction(this Panel panel, ApertureType apertureType)
        {
            if (panel == null)
                return null;

            return DefaultApertureConstruction(panel.PanelType, apertureType);
        }

        public static ApertureConstruction DefaultApertureConstruction(this PanelType panelType, ApertureType apertureType)
        {
            ApertureConstructionLibrary apertureConstructionLibrary = ActiveSetting.Setting.GetValue<ApertureConstructionLibrary>(AnalyticalSettingParameter.DefaultApertureConstructionLibrary);
            if (apertureConstructionLibrary == null)
                return null;

            List<ApertureConstruction> apertureConstructions = apertureConstructionLibrary.GetApertureConstructions(apertureType, panelType);
            if (apertureConstructions == null || apertureConstructions.Count == 0)
                apertureConstructions = apertureConstructionLibrary.GetApertureConstructions(apertureType, panelType.PanelGroup());

            if (apertureConstructions == null || apertureConstructions.Count == 0)
                return null;

            return apertureConstructions.FirstOrDefault();
        }
    }
}