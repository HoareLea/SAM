using SAM.Core;

namespace SAM.Analytical
{
    public static partial class Query
    {
        public static ApertureConstruction DefaultApertureConstruction(this ApertureType apertureType, bool external = true)
        {
            Setting setting = ActiveSetting.Setting;

            string parameterName = null;
            switch (apertureType)
            {
                case Analytical.ApertureType.Window:
                    if (external)
                        parameterName = ActiveSetting.Name.ApertureConstruction_ExternalWindows;
                    else
                        parameterName = ActiveSetting.Name.ApertureConstruction_InternalWindows;
                    break;

                case Analytical.ApertureType.Door:
                    if (external)
                        parameterName = ActiveSetting.Name.ApertureConstruction_ExternalDoors;
                    else
                        parameterName = ActiveSetting.Name.ApertureConstruction_InternalDoors;
                    break;
            }

            ApertureConstruction apertureConstruction;
            if (!setting.TryGetValue(parameterName, out apertureConstruction))
                return null;

            return apertureConstruction;
        }

        public static ApertureConstruction DefaultApertureConstruction(this Panel panel, ApertureType apertureType)
        {
            if (panel == null)
                return null;

            return DefaultApertureConstruction(panel.PanelType, apertureType);
        }

        public static ApertureConstruction DefaultApertureConstruction(this PanelType panelType, ApertureType apertureType)
        {
            ApertureType apertureType_Temp = apertureType;

            bool external = true;
            switch (panelType)
            {
                case Analytical.PanelType.Undefined:
                case Analytical.PanelType.Roof:
                    ApertureConstruction apertureConstruction;
                    if (!ActiveSetting.Setting.TryGetValue(ActiveSetting.Name.ApertureConstruction_Skylight, out apertureConstruction))
                        return null;
                    return apertureConstruction;

                case Analytical.PanelType.CurtainWall:
                case Analytical.PanelType.Wall:
                case Analytical.PanelType.Floor:
                case Analytical.PanelType.FloorExposed:
                case Analytical.PanelType.FloorRaised:
                case Analytical.PanelType.Shade:
                case Analytical.PanelType.SolarPanel:
                case Analytical.PanelType.WallExternal:
                    external = true;
                    break;

                case Analytical.PanelType.WallInternal:
                case Analytical.PanelType.FloorInternal:
                case Analytical.PanelType.Ceiling:
                    external = false;
                    break;

                default:
                    return null;
            }

            return ApertureConstruction(apertureType_Temp, external);
        }
    }
}