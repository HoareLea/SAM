namespace SAM.Analytical.Grasshopper
{
    public static partial class Query
    {
        /// <summary>
        /// This is used to disply Panels in Rhino depends on PanelType. Here we used default colors. 
        /// </summary>
        /// <param name="partition">IPartition</param>
        /// <returns></returns>
        public static global::Rhino.Display.DisplayMaterial DisplayMaterial(this IPartition partition)
        {
            System.Drawing.Color color = Analytical.Query.Color(partition);

            if (color == System.Drawing.Color.Empty)
                return null;

            return new global::Rhino.Display.DisplayMaterial(color);
        }

        public static global::Rhino.Display.DisplayMaterial DisplayMaterial(this IOpening opening)
        {
            System.Drawing.Color color = Analytical.Query.Color(opening);

            if (color == System.Drawing.Color.Empty)
                return null;

            return new global::Rhino.Display.DisplayMaterial(color);
        }

        public static global::Rhino.Display.DisplayMaterial DisplayMaterial(this IOpening opening, OpeningPart openingPart)
        {
            System.Drawing.Color color = Analytical.Query.Color(opening, openingPart);

            if (color == System.Drawing.Color.Empty)
                return null;

            return new global::Rhino.Display.DisplayMaterial(color);
        }
    }
}