namespace SAM.Analytical.Grasshopper
{
    public static partial class Query
    {
        public static Rhino.Display.DisplayMaterial DisplayMaterial(this PanelType panelType)
        {
            System.Drawing.Color color = System.Drawing.Color.Empty;

            switch (panelType)
            {
                case PanelType.Wall:
                    color = System.Drawing.Color.Red;
                    break;
                case PanelType.Floor:
                    color = System.Drawing.Color.Red;
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
                    color = System.Drawing.Color.Red;
                    break;
                case ApertureType.Window:
                    color = System.Drawing.Color.Red;
                    break;
            }

            if (color == System.Drawing.Color.Empty)
                return null;

            return new Rhino.Display.DisplayMaterial(color);
        }
    }
}