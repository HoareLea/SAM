namespace SAM.Analytical.Grasshopper
{
    public static partial class Query
    {
        public static System.Drawing.Color Color(this PanelType panelType, bool internalEdges = false)
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

                    case PanelType.SlabOnGrade:
                        return System.Drawing.Color.Red;

                    default:
                        return System.Drawing.Color.Empty;
                }
            }
        }

        public static System.Drawing.Color Color(this ApertureType apertureType, bool internalEdges = false)
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
    }
}