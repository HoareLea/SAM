namespace SAM.Analytical
{
    public static partial class Query
    {
        public static System.Drawing.Color Color(this PanelType panelType, bool internalEdges)
        {
            if (internalEdges)
            {
                switch (panelType)
                {
                    case Analytical.PanelType.Wall:
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
                    case Analytical.PanelType.Roof:
                        return System.Drawing.Color.Red;

                    case Analytical.PanelType.Wall:
                        return System.Drawing.Color.Red;

                    case Analytical.PanelType.WallExternal:
                        return System.Drawing.Color.Red;

                    case Analytical.PanelType.WallInternal:
                        return System.Drawing.Color.Lime;

                    case Analytical.PanelType.Floor:
                        return System.Drawing.Color.Red;

                    case Analytical.PanelType.FloorExposed:
                        return System.Drawing.Color.Red;

                    case Analytical.PanelType.FloorInternal:
                        return System.Drawing.Color.Lime;

                    case Analytical.PanelType.UndergroundSlab:
                        return System.Drawing.Color.Red;

                    case Analytical.PanelType.UndergroundWall:
                        return System.Drawing.Color.Red;

                    case Analytical.PanelType.UndergroundCeiling:
                        return System.Drawing.Color.Red;

                    case Analytical.PanelType.SlabOnGrade:
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
                case Analytical.PanelType.Ceiling:
                    color = System.Drawing.ColorTranslator.FromHtml("#FF8080");
                    break;

                case Analytical.PanelType.CurtainWall:
                    color = System.Drawing.Color.BlueViolet;
                    break;

                case Analytical.PanelType.Floor:
                    color = System.Drawing.ColorTranslator.FromHtml("#804000");
                    break;

                case Analytical.PanelType.FloorExposed:
                    color = System.Drawing.ColorTranslator.FromHtml("#40B4FF");
                    break;

                case Analytical.PanelType.FloorInternal:
                    color = System.Drawing.ColorTranslator.FromHtml("#80FFFF");
                    break;

                case Analytical.PanelType.FloorRaised:
                    color = System.Drawing.ColorTranslator.FromHtml("#80FFFF");
                    break;

                case Analytical.PanelType.Roof:
                    color = System.Drawing.ColorTranslator.FromHtml("#800000");
                    break;

                case Analytical.PanelType.Shade:
                    color = System.Drawing.ColorTranslator.FromHtml("#FFCE9D");
                    break;

                case Analytical.PanelType.SlabOnGrade:
                    color = System.Drawing.ColorTranslator.FromHtml("#804000");
                    break;

                case Analytical.PanelType.SolarPanel:
                    color = System.Drawing.ColorTranslator.FromHtml("#FFB400");
                    break;

                case Analytical.PanelType.Undefined:
                    color = System.Drawing.ColorTranslator.FromHtml("#FFB400");
                    break;

                case Analytical.PanelType.UndergroundCeiling:
                    color = System.Drawing.ColorTranslator.FromHtml("#408080");
                    break;

                case Analytical.PanelType.UndergroundSlab:
                    color = System.Drawing.ColorTranslator.FromHtml("#804000");
                    break;

                case Analytical.PanelType.UndergroundWall:
                    color = System.Drawing.ColorTranslator.FromHtml("#A55200");
                    break;

                case Analytical.PanelType.Wall:
                    color = System.Drawing.ColorTranslator.FromHtml("#FFB400");
                    break;

                case Analytical.PanelType.WallExternal:
                    color = System.Drawing.ColorTranslator.FromHtml("#FFB400");
                    break;

                case Analytical.PanelType.WallInternal:
                    color = System.Drawing.ColorTranslator.FromHtml("#008000");
                    break;

                case Analytical.PanelType.Air:
                    color = System.Drawing.ColorTranslator.FromHtml("#FFFF00");
                    break;
            }

            return color;
        }

        public static System.Drawing.Color Color(this Panel panel)
        {
            if (panel == null)
                return System.Drawing.Color.Empty;

            return Color(panel.PanelType);
        }

        public static System.Drawing.Color Color(this ApertureType apertureType, bool internalEdges)
        {
            if (internalEdges)
            {
                switch (apertureType)
                {
                    case Analytical.ApertureType.Door:
                        return System.Drawing.Color.Violet;

                    case Analytical.ApertureType.Window:
                        return System.Drawing.Color.Violet;

                    default:
                        return System.Drawing.Color.Empty;
                }
            }
            else
            {
                switch (apertureType)
                {
                    case Analytical.ApertureType.Door:
                        return System.Drawing.Color.Brown;

                    case Analytical.ApertureType.Window:
                        return System.Drawing.Color.Blue;

                    default:
                        return System.Drawing.Color.Empty;
                }
            }
        }

        public static System.Drawing.Color Color(this ApertureType apertureType, AperturePart aperturePart)
        {
            System.Drawing.Color color = System.Drawing.Color.Empty;

            switch (apertureType)
            {
                case Analytical.ApertureType.Door:
                    switch(aperturePart)
                    {
                        case AperturePart.Frame:
                            color = System.Drawing.Color.Brown;
                            break;

                        case AperturePart.Pane:
                            color = System.Drawing.Color.Blue;
                            break;
                    }
                    break;

                case Analytical.ApertureType.Window:
                    switch (aperturePart)
                    {
                        case AperturePart.Frame:
                            color = System.Drawing.Color.Brown;
                            break;

                        case AperturePart.Pane:
                            color = System.Drawing.Color.Blue;
                            break;
                    }
                    break;
            }

            return color;
        }

        public static System.Drawing.Color Color(this ApertureType apertureType)
        {
            switch(apertureType)
            {
                case Analytical.ApertureType.Window:
                    return Color(apertureType, AperturePart.Pane);

                case Analytical.ApertureType.Door:
                    return Color(apertureType, AperturePart.Frame);
            }

            return System.Drawing.Color.Empty;
        }
    }
}