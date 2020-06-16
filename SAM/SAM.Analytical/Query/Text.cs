using System;

namespace SAM.Analytical
{
    public static partial class Query
    {
        public static string Text(this PanelType panelType)
        {
            switch(panelType)
            {
                case Analytical.PanelType.Ceiling:
                    return "Ceiling";
                case Analytical.PanelType.CurtainWall:
                    return "Curtain Wall";
                case Analytical.PanelType.Floor:
                    return "Floor";
                case Analytical.PanelType.FloorExposed:
                    return "Exposed Floor";
                case Analytical.PanelType.FloorInternal:
                    return "Internal Floor";
                case Analytical.PanelType.FloorRaised:
                    return "Raised Floor";
                case Analytical.PanelType.Roof:
                    return "Roof";
                case Analytical.PanelType.Shade:
                    return "Shade";
                case Analytical.PanelType.SlabOnGrade:
                    return "Slab on Grade";
                case Analytical.PanelType.SolarPanel:
                    return "Solar/PV panel";
                case Analytical.PanelType.Air:
                    return "No Type";
                case Analytical.PanelType.UndergroundCeiling:
                    return "Underground Ceiling";
                case Analytical.PanelType.UndergroundSlab:
                    return "Underground Slab";
                case Analytical.PanelType.UndergroundWall:
                    return "Underground Wall";
                case Analytical.PanelType.Wall:
                    return "Wall";
                case Analytical.PanelType.WallExternal:
                    return "External Wall";
                case Analytical.PanelType.WallInternal:
                    return "Internal Wall";
            }

            return null;
        }
    }
}