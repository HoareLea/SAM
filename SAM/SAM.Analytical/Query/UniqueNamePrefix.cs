namespace SAM.Analytical
{
    public static partial class Query
    {

        public static string UniqueNamePrefix(this PanelType panelType)
        {
            string prefix = null;
            switch (panelType)
            {
                case Analytical.PanelType.Ceiling:
                    prefix = "Compound Ceiling";
                    break;
                case Analytical.PanelType.CurtainWall:
                    prefix = "Curtain Wall";
                    break;
                case Analytical.PanelType.Floor:
                case Analytical.PanelType.FloorExposed:
                case Analytical.PanelType.FloorInternal:
                case Analytical.PanelType.FloorRaised:
                case Analytical.PanelType.SlabOnGrade:
                case Analytical.PanelType.UndergroundSlab:
                case Analytical.PanelType.UndergroundCeiling:
                    prefix = "Floor";
                    break;
                case Analytical.PanelType.Roof:
                case Analytical.PanelType.Shade:
                case Analytical.PanelType.SolarPanel:
                    prefix = "Basic Roof";
                    break;
                case Analytical.PanelType.UndergroundWall:
                case Analytical.PanelType.Wall:
                case Analytical.PanelType.WallExternal:
                case Analytical.PanelType.WallInternal:
                    prefix = "Basic Wall";
                    break;
                case Analytical.PanelType.Air:
                    prefix = "Air";
                    break;
                default:
                    prefix = "Undefined";
                    break;
            }

            return prefix;
        }

        public static string UniqueNamePrefix(this ApertureType apertureType)
        {
            string prefix = null;
            switch (apertureType)
            {
                case Analytical.ApertureType.Window:
                    prefix = "Windows";
                    break;
                case Analytical.ApertureType.Door:
                    prefix = "Doors";
                    break;
                case Analytical.ApertureType.Undefined:
                    prefix = "Undefined";
                    break;
            }

            return prefix;
        }
    }
}