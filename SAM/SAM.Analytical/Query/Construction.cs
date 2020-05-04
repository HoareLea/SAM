using SAM.Core;

namespace SAM.Analytical
{
    public static partial class Query
    {
        /// <summary>
        /// Returns Default SAM Analytical Construction for givet PanelType
        /// </summary>
        /// <param name="panelType">SAM Analytical PanelType</param>
        /// <returns name="construction"> Default SAM Analytical Construction</returns>
        /// <search>Default SAM Analytical Construction, PanelType</search> 
        public static Construction Construction(this PanelType panelType)
        {
            Setting setting = ActiveSetting.Setting;

            string parameterName = null;
            switch (panelType)
            {
                case Analytical.PanelType.Ceiling:
                    parameterName = ActiveSetting.Name.Construction_Ceiling;
                    break;

                case Analytical.PanelType.CurtainWall:
                    parameterName = ActiveSetting.Name.Construction_CurtainWall;
                    break;

                case Analytical.PanelType.Floor:
                    parameterName = ActiveSetting.Name.Construction_Floor;
                    break;

                case Analytical.PanelType.FloorExposed:
                    parameterName = ActiveSetting.Name.Construction_FloorExposed;
                    break;

                case Analytical.PanelType.FloorInternal:
                    parameterName = ActiveSetting.Name.Construction_FloorInternal;
                    break;

                case Analytical.PanelType.FloorRaised:
                    parameterName = ActiveSetting.Name.Construction_FloorRaised;
                    break;

                case Analytical.PanelType.Roof:
                    parameterName = ActiveSetting.Name.Construction_Roof;
                    break;

                case Analytical.PanelType.Shade:
                    parameterName = ActiveSetting.Name.Construction_Shade;
                    break;

                case Analytical.PanelType.SlabOnGrade:
                    parameterName = ActiveSetting.Name.Construction_SlabOnGrade;
                    break;

                case Analytical.PanelType.SolarPanel:
                    parameterName = ActiveSetting.Name.Construction_SolarPanel;
                    break;

                case Analytical.PanelType.Undefined:
                    parameterName = ActiveSetting.Name.Construction_Undefined;
                    break;

                case Analytical.PanelType.UndergroundCeiling:
                    parameterName = ActiveSetting.Name.Construction_UndergroundCeiling;
                    break;

                case Analytical.PanelType.UndergroundSlab:
                    parameterName = ActiveSetting.Name.Construction_UndergroundSlab;
                    break;

                case Analytical.PanelType.UndergroundWall:
                    parameterName = ActiveSetting.Name.Construction_UndergroundWall;
                    break;

                case Analytical.PanelType.Wall:
                    parameterName = ActiveSetting.Name.Construction_Wall;
                    break;

                case Analytical.PanelType.WallExternal:
                    parameterName = ActiveSetting.Name.Construction_WallExternal;
                    break;

                case Analytical.PanelType.WallInternal:
                    parameterName = ActiveSetting.Name.Construction_WallInternal;
                    break;
            }

            Construction construction;
            if (!setting.TryGetValue(parameterName, out construction))
                return null;

            return construction;
        }
    }
}