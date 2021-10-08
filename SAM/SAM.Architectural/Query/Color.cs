namespace SAM.Architectural
{
    public static partial class Query
    {
        public static System.Drawing.Color Color(this IPartition partition)
        {
            System.Drawing.Color result = System.Drawing.Color.Empty;
            if(partition == null)
            {
                return result;
            }

            if(partition is Wall)
            {
                return System.Drawing.ColorTranslator.FromHtml("#FFB400");
            }

            if (partition is Floor)
            {
                return System.Drawing.ColorTranslator.FromHtml("#804000");
            }

            if (partition is Roof)
            {
                return System.Drawing.ColorTranslator.FromHtml("#800000");
            }

            if (partition is AirPartition)
            {
                return System.Drawing.ColorTranslator.FromHtml("#FFFF00");
            }

            return result;
        }

        public static System.Drawing.Color Color(this IPartition partition, bool internalEdges)
        {
            System.Drawing.Color result = System.Drawing.Color.Empty;
            if (partition == null)
            {
                return result;
            }

            if (internalEdges)
            {
                return System.Drawing.Color.Gray;
            }

            return Color(partition);
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