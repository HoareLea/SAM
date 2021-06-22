namespace SAM.Architectural
{
    public static partial class Query
    {
        public static System.Drawing.Color Color(this HostBuildingElement hostBuildingElement)
        {
            System.Drawing.Color result = System.Drawing.Color.Empty;
            if(hostBuildingElement == null)
            {
                return result;
            }

            if(hostBuildingElement is Wall)
            {
                return System.Drawing.ColorTranslator.FromHtml("#FFB400");
            }

            if (hostBuildingElement is Floor)
            {
                return System.Drawing.ColorTranslator.FromHtml("#804000");
            }

            if (hostBuildingElement is Roof)
            {
                return System.Drawing.ColorTranslator.FromHtml("#800000");
            }

            return result;
        }

        public static System.Drawing.Color Color(this HostBuildingElement hostBuildingElement, bool internalEdges)
        {
            System.Drawing.Color result = System.Drawing.Color.Empty;
            if (hostBuildingElement == null)
            {
                return result;
            }

            if (internalEdges)
            {
                return System.Drawing.Color.Gray;
            }

            return Color(hostBuildingElement);
        }

        public static System.Drawing.Color Color(this Opening opening)
        {
            System.Drawing.Color result = System.Drawing.Color.Empty;
            if(opening == null)
            {
                return result;
            }

            if(opening is Window)
            {
                return System.Drawing.Color.Blue;
            }

            if(opening is Door)
            {
                return System.Drawing.Color.Brown;
            }

            return result;
        }

        public static System.Drawing.Color Color(this Opening opening, bool internalEdges)
        {
            System.Drawing.Color result = System.Drawing.Color.Empty;
            if (opening == null)
            {
                return result;
            }

            if (internalEdges)
            {
                return System.Drawing.Color.Violet;
            }

            return Color(opening);
        }
    }
}