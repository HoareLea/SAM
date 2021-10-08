namespace SAM.Architectural.Grasshopper
{
    public static partial class Query
    {
        /// <summary>
        /// This is used to disply Panels in Rhino depends on PanelType. Here we used default colors. 
        /// </summary>
        /// <param name="panelType"></param>
        /// <returns></returns>
        public static Rhino.Display.DisplayMaterial DisplayMaterial(this HostPartition hostPartition)
        {
            System.Drawing.Color color = Architectural.Query.Color(hostPartition);

            if (color == System.Drawing.Color.Empty)
                return null;

            return new Rhino.Display.DisplayMaterial(color);
        }

        public static Rhino.Display.DisplayMaterial DisplayMaterial(this IOpening opening)
        {
            System.Drawing.Color color = Architectural.Query.Color(opening);

            if (color == System.Drawing.Color.Empty)
                return null;

            return new Rhino.Display.DisplayMaterial(color);
        }
    }
}