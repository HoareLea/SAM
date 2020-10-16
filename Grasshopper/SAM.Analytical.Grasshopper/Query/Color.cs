namespace SAM.Analytical.Grasshopper
{
    public static partial class Query
    {
        public static System.Drawing.Color Color(this PanelType panelType, bool internalEdges)
        {
            if (internalEdges)
            {
                switch (panelType)
                {
                    case PanelType.Wall:
                        return System.Drawing.Color.Gray;

                    default:
                        return System.Drawing.Color.Empty;
                }
            }
            else
            //geometry external edges
            {
                switch (panelType)
                {
                    case PanelType.Roof:
                        return System.Drawing.Color.Red;

                    case PanelType.Wall:
                        return System.Drawing.Color.Red;

                    case PanelType.WallExternal:
                        return System.Drawing.Color.Red;

                    case PanelType.WallInternal:
                        return System.Drawing.Color.Lime;

                    case PanelType.Floor:
                        return System.Drawing.Color.Red;

                    case PanelType.FloorExposed:
                        return System.Drawing.Color.Red;

                    case PanelType.FloorInternal:
                        return System.Drawing.Color.Lime;

                    case PanelType.UndergroundSlab:
                        return System.Drawing.Color.Red;

                    case PanelType.UndergroundWall:
                        return System.Drawing.Color.Red;

                    case PanelType.UndergroundCeiling:
                        return System.Drawing.Color.Red;

                    case PanelType.SlabOnGrade:
                        return System.Drawing.Color.Red;

                    default:
                        return System.Drawing.Color.Empty;
                }
            }
        }

        public static System.Drawing.Color Color(this PanelType panelType)
        {
            System.Drawing.Color color = System.Drawing.Color.Empty;

            switch (panelType)
            {
                case PanelType.Ceiling:
                    color = System.Drawing.ColorTranslator.FromHtml("#FF8080");
                    break;

                case PanelType.CurtainWall:
                    color = System.Drawing.Color.BlueViolet;
                    break;

                case PanelType.Floor:
                    color = System.Drawing.ColorTranslator.FromHtml("#804000");
                    break;

                case PanelType.FloorExposed:
                    color = System.Drawing.ColorTranslator.FromHtml("#40B4FF");
                    break;

                case PanelType.FloorInternal:
                    color = System.Drawing.ColorTranslator.FromHtml("#80FFFF");
                    break;

                case PanelType.FloorRaised:
                    color = System.Drawing.ColorTranslator.FromHtml("#80FFFF");
                    break;

                case PanelType.Roof:
                    color = System.Drawing.ColorTranslator.FromHtml("#800000");
                    break;

                case PanelType.Shade:
                    color = System.Drawing.ColorTranslator.FromHtml("#FFCE9D");
                    break;

                case PanelType.SlabOnGrade:
                    color = System.Drawing.ColorTranslator.FromHtml("#804000");
                    break;

                case PanelType.SolarPanel:
                    color = System.Drawing.ColorTranslator.FromHtml("#FFB400");
                    break;

                case PanelType.Undefined:
                    color = System.Drawing.ColorTranslator.FromHtml("#FFB400");
                    break;

                case PanelType.UndergroundCeiling:
                    color = System.Drawing.ColorTranslator.FromHtml("#408080");
                    break;

                case PanelType.UndergroundSlab:
                    color = System.Drawing.ColorTranslator.FromHtml("#804000");
                    break;

                case PanelType.UndergroundWall:
                    color = System.Drawing.ColorTranslator.FromHtml("#A55200");
                    break;

                case PanelType.Wall:
                    color = System.Drawing.ColorTranslator.FromHtml("#FFB400");
                    break;

                case PanelType.WallExternal:
                    color = System.Drawing.ColorTranslator.FromHtml("#FFB400");
                    break;

                case PanelType.WallInternal:
                    color = System.Drawing.ColorTranslator.FromHtml("#008000");
                    break;
            }

            return color;
        }

        public static System.Drawing.Color Color(this ApertureType apertureType, bool internalEdges)
        {
            if (internalEdges)
            {
                switch (apertureType)
                {
                    case ApertureType.Door:
                        return System.Drawing.Color.Violet;

                    case ApertureType.Window:
                        return System.Drawing.Color.Violet;

                    default:
                        return System.Drawing.Color.Empty;
                }
            }
            else
            {
                switch (apertureType)
                {
                    case ApertureType.Door:
                        return System.Drawing.Color.Brown;

                    case ApertureType.Window:
                        return System.Drawing.Color.Blue;

                    default:
                        return System.Drawing.Color.Empty;
                }
            }
        }

        public static System.Drawing.Color Color(this ApertureType apertureType)
        {
            System.Drawing.Color color = System.Drawing.Color.Empty;

            switch (apertureType)
            {
                case ApertureType.Door:
                    color = System.Drawing.Color.Brown;
                    break;

                case ApertureType.Window:
                    color = System.Drawing.Color.Blue;
                    break;
            }

            return color;
        }
    }
}