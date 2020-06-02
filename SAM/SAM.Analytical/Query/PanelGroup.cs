namespace SAM.Analytical
{
    public static partial class Query
    {
        public static PanelGroup PanelGroup(this PanelType panelType)
        {
            switch (panelType)
            {
                case Analytical.PanelType.Wall:
                case Analytical.PanelType.CurtainWall:
                case Analytical.PanelType.UndergroundWall:
                case Analytical.PanelType.WallExternal:
                case Analytical.PanelType.WallInternal:
                    return Analytical.PanelGroup.Wall;

                case Analytical.PanelType.Floor:
                case Analytical.PanelType.FloorExposed:
                case Analytical.PanelType.FloorInternal:
                case Analytical.PanelType.FloorRaised:
                case Analytical.PanelType.SlabOnGrade:
                case Analytical.PanelType.UndergroundSlab:
                case Analytical.PanelType.UndergroundCeiling:
                    return Analytical.PanelGroup.Floor;

                case Analytical.PanelType.Roof:
                    return Analytical.PanelGroup.Roof;

                case Analytical.PanelType.Undefined:
                    return Analytical.PanelGroup.Undefined;

                default:
                    return Analytical.PanelGroup.Other;
            }
        }
    }
}