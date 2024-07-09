namespace SAM.Analytical.Grasshopper
{
    public static partial class Query
    {
        /// <summary>
        /// This is used to disply Panels in Rhino depends on PanelType. Here we used default colors. 
        /// </summary>
        /// <param name="panelType"></param>
        /// <returns></returns>
        public static global::Rhino.Display.DisplayMaterial DisplayMaterial(this PanelType panelType)
        {
            System.Drawing.Color color = Analytical.Query.Color(panelType);

            if (color == System.Drawing.Color.Empty)
                return null;

            return new global::Rhino.Display.DisplayMaterial(color);
        }

        public static global::Rhino.Display.DisplayMaterial DisplayMaterial(this BoundaryType boundaryType)
        {
            System.Drawing.Color color = Analytical.Query.Color(boundaryType);

            if (color == System.Drawing.Color.Empty)
                return null;

            return new global::Rhino.Display.DisplayMaterial(color);
        }

        public static global::Rhino.Display.DisplayMaterial DisplayMaterial(this ExternalPanel externalPanel)
        {
            System.Drawing.Color color = Analytical.Query.Color(externalPanel);

            if (color == System.Drawing.Color.Empty)
            {
                return null;
            }

            return new global::Rhino.Display.DisplayMaterial(color);
        }

        public static global::Rhino.Display.DisplayMaterial DisplayMaterial(this ApertureType apertureType)
        {
            System.Drawing.Color color = Analytical.Query.Color(apertureType);

            if (color == System.Drawing.Color.Empty)
                return null;

            return new global::Rhino.Display.DisplayMaterial(color);
        }

        public static global::Rhino.Display.DisplayMaterial DisplayMaterial(this ApertureType apertureType, AperturePart aperturePart, bool openable = false)
        {
            System.Drawing.Color color = Analytical.Query.Color(apertureType, aperturePart, openable);

            if (color == System.Drawing.Color.Empty)
                return null;

            return new global::Rhino.Display.DisplayMaterial(color);
        }
    }
}