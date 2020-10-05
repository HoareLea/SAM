namespace SAM.Analytical
{
    public static partial class Query
    {
        public static bool External(this PanelType panelType)
        {
            switch(panelType)
            {
                case Analytical.PanelType.CurtainWall:
                case Analytical.PanelType.FloorExposed:
                case Analytical.PanelType.FloorRaised:
                case Analytical.PanelType.Roof:
                case Analytical.PanelType.Shade:
                case Analytical.PanelType.SolarPanel:
                case Analytical.PanelType.SlabOnGrade:
                case Analytical.PanelType.UndergroundWall:
                case Analytical.PanelType.UndergroundCeiling:
                case Analytical.PanelType.WallExternal:
                case Analytical.PanelType.UndergroundSlab:
                    return true;
                default:
                    return false;
            }
        }
    }
}