namespace SAM.Analytical.Grasshopper
{
    public static partial class Query
    {
        public static Rhino.Display.DisplayMaterial DisplayMaterial(this PanelType panelType)
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

            if (color == System.Drawing.Color.Empty)
                return null;

            return new Rhino.Display.DisplayMaterial(color);
        }

        public static Rhino.Display.DisplayMaterial DisplayMaterial(this ApertureType apertureType)
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

            if (color == System.Drawing.Color.Empty)
                return null;

            return new Rhino.Display.DisplayMaterial(color);
        }
    }
}