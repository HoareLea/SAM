namespace SAM.Analytical.Grasshopper
{
    public static partial class Query
    {
        /// <summary>
        /// This is used to disply Panels in Rhino depends on PanelType. Here we used default colors. 
        /// </summary>
        /// <param name="panelType"></param>
        /// <returns></returns>
        public static Rhino.Display.DisplayMaterial DisplayMaterial(this PanelType panelType)
        {
            System.Drawing.Color color = Color(panelType);

            if (color == System.Drawing.Color.Empty)
                return null;

            return new Rhino.Display.DisplayMaterial(color);
        }

        public static Rhino.Display.DisplayMaterial DisplayMaterial(this ApertureType apertureType)
        {
            System.Drawing.Color color = Color(apertureType);

            if (color == System.Drawing.Color.Empty)
                return null;

            return new Rhino.Display.DisplayMaterial(color);
        }
    }
}